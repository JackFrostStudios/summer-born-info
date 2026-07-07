import { spawn } from 'node:child_process';
import { readdirSync } from 'node:fs';
import { basename, join, resolve } from 'node:path';

const uiRoot = resolve(import.meta.dirname, '..');
const distRoot = join(uiRoot, 'dist', 'summer-born-info');
const browserRoot = join(distRoot, 'browser');
const serverEntry = join(distRoot, 'server', 'server.mjs');
const port = 4310;

function findLocalizedOutputDirectory() {
  return readdirSync(browserRoot, { withFileTypes: true })
    .filter((entry) => entry.isDirectory())
    .map((entry) => join(browserRoot, entry.name))
    .find((directory) => {
      try {
        return readdirSync(directory).includes('index.html');
      } catch {
        return false;
      }
    });
}

function requireFile(directory, predicate, description) {
  const match = readdirSync(directory, { withFileTypes: true })
    .filter((entry) => entry.isFile())
    .map((entry) => entry.name)
    .find(predicate);

  if (match === undefined) {
    throw new Error(`Expected ${description} in ${directory}.`);
  }

  return match;
}

async function waitForServer(serverProcess) {
  await new Promise((resolveReady, rejectReady) => {
    const timeout = setTimeout(() => {
      rejectReady(new Error('Timed out waiting for the localized SSR server to start.'));
    }, 15000);

    const handleExit = (code) => {
      clearTimeout(timeout);
      rejectReady(new Error(`Localized SSR server exited before startup with code ${String(code)}.`));
    };

    const handleStdout = (chunk) => {
      const output = chunk.toString();

      if (output.includes('Node Express server listening on')) {
        clearTimeout(timeout);
        serverProcess.stdout.off('data', handleStdout);
        serverProcess.off('exit', handleExit);
        resolveReady();
      }
    };

    serverProcess.once('exit', handleExit);
    serverProcess.stdout.on('data', handleStdout);
  });
}

async function assertOkResponse(url, expectedContentTypePrefix) {
  const response = await fetch(url);

  if (!response.ok) {
    throw new Error(`Expected ${url} to return 200 OK, received ${response.status}.`);
  }

  const contentType = response.headers.get('content-type') ?? '';

  if (!contentType.startsWith(expectedContentTypePrefix)) {
    throw new Error(
      `Expected ${url} to return ${expectedContentTypePrefix}, received ${contentType || 'no content-type'}.`,
    );
  }

  const body = new Uint8Array(await response.arrayBuffer());

  if (body.byteLength === 0) {
    throw new Error(`Expected ${url} to return a non-empty response body.`);
  }

  return new TextDecoder().decode(body);
}

const localizedOutputDirectory = findLocalizedOutputDirectory();

if (localizedOutputDirectory === undefined) {
  throw new Error(`Expected a localized browser output directory under ${browserRoot}.`);
}

const locale = basename(localizedOutputDirectory);
const heroImage = requireFile(
  join(localizedOutputDirectory, 'images'),
  (name) => name.endsWith('.avif'),
  'an AVIF hero image',
);
const fontAsset = requireFile(
  join(localizedOutputDirectory, 'fonts'),
  (name) => name === 'HankenGrotesk-VariableFont_wght.woff2',
  'the normal WOFF2 font asset',
);
const iconAsset = requireFile(
  join(localizedOutputDirectory, 'icons'),
  (name) => name.endsWith('.svg'),
  'an icon asset',
);

const serverProcess = spawn(process.execPath, [serverEntry], {
  cwd: uiRoot,
  env: {
    ...process.env,
    NG_ALLOWED_HOSTS: 'localhost',
    PORT: String(port),
  },
  stdio: ['ignore', 'pipe', 'pipe'],
});

let stderr = '';
serverProcess.stderr.on('data', (chunk) => {
  stderr += chunk.toString();
});

try {
  await waitForServer(serverProcess);

  const origin = `http://localhost:${String(port)}`;
  const localizedPage = await assertOkResponse(`${origin}/${locale}/`, 'text/html');

  if (!localizedPage.includes(`<base href="/${locale}/">`)) {
    throw new Error(`Expected localized SSR HTML to include <base href="/${locale}/">.`);
  }

  await assertOkResponse(`${origin}/images/${heroImage}`, 'image/');
  await assertOkResponse(`${origin}/fonts/${fontAsset}`, 'font/');
  await assertOkResponse(`${origin}/icons/${iconAsset}`, 'image/svg+xml');
} catch (error) {
  const details = stderr.trim();

  if (details.length > 0 && error instanceof Error) {
    error.message = `${error.message}\nSSR server stderr:\n${details}`;
  }

  throw error;
} finally {
  serverProcess.kill();
}
