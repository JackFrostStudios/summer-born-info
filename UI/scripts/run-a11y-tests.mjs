import { spawn } from 'node:child_process';
import { accessSync, constants } from 'node:fs';
import { createRequire } from 'node:module';
import path from 'node:path';
import process from 'node:process';

const browserName = 'ChromiumHeadless';
const require = createRequire(import.meta.url);
const angularCliEntryPoint = require.resolve('@angular/cli/bin/ng.js');

const chromeExecutablePath = resolveChromeExecutablePath();

if (chromeExecutablePath === null) {
  const attemptedLocations = getChromeExecutableCandidates();

  console.error('Unable to find a usable Google Chrome executable for `npm run test:a11y`.');
  console.error('Set CHROME_BIN to a valid Chrome path or install Chrome in one of these locations:');

  for (const candidate of attemptedLocations) {
    console.error(`- ${candidate}`);
  }

  process.exit(1);
}

console.info(`Using Chrome executable for accessibility smoke tests: ${chromeExecutablePath}`);

const childProcess = spawn(
  process.execPath,
  [angularCliEntryPoint, 'run', 'summer-born-info:test-a11y', '--browsers', browserName],
  {
    cwd: path.resolve(process.cwd()),
    env: {
      ...process.env,
      CHROME_BIN: chromeExecutablePath,
    },
    stdio: 'inherit',
  },
);

childProcess.on('error', (error) => {
  console.error(`Failed to launch the Angular accessibility test target: ${error.message}`);
  process.exit(1);
});

childProcess.on('exit', (code, signal) => {
  if (signal !== null) {
    process.kill(process.pid, signal);
    return;
  }

  process.exit(code ?? 1);
});

function resolveChromeExecutablePath() {
  const configuredChromePath = process.env.CHROME_BIN;

  if (isUsableFile(configuredChromePath)) {
    return configuredChromePath;
  }

  for (const candidate of getChromeExecutableCandidates()) {
    if (isUsableFile(candidate)) {
      return candidate;
    }
  }

  return null;
}

function getChromeExecutableCandidates() {
  const candidates = new Set();

  if (process.platform === 'win32') {
    addWindowsChromeCandidates(candidates);
  } else if (process.platform === 'darwin') {
    addMacOsChromeCandidates(candidates);
  } else {
    addLinuxChromeCandidates(candidates);
  }

  return [...candidates];
}

function addWindowsChromeCandidates(candidates) {
  const windowsInstallRoots = [process.env['PROGRAMFILES(X86)'], process.env.PROGRAMFILES, process.env.LOCALAPPDATA];

  for (const installRoot of windowsInstallRoots) {
    if (typeof installRoot === 'string' && path.isAbsolute(installRoot)) {
      candidates.add(path.join(installRoot, 'Google', 'Chrome', 'Application', 'chrome.exe'));
    }
  }
}

function addMacOsChromeCandidates(candidates) {
  candidates.add('/Applications/Google Chrome.app/Contents/MacOS/Google Chrome');

  if (process.env.HOME) {
    candidates.add(
      path.join(process.env.HOME, 'Applications', 'Google Chrome.app', 'Contents', 'MacOS', 'Google Chrome'),
    );
  }
}

function addLinuxChromeCandidates(candidates) {
  const linuxExecutableNames = ['google-chrome', 'google-chrome-stable', 'chrome'];

  for (const executableName of linuxExecutableNames) {
    for (const pathEntry of process.env.PATH?.split(path.delimiter) ?? []) {
      if (pathEntry.length > 0) {
        candidates.add(path.join(pathEntry, executableName));
      }
    }
  }

  candidates.add('/usr/bin/google-chrome');
  candidates.add('/usr/bin/google-chrome-stable');
}

function isUsableFile(filePath) {
  if (typeof filePath !== 'string' || filePath.trim().length === 0) {
    return false;
  }

  try {
    accessSync(filePath, constants.F_OK);
    return true;
  } catch {
    return false;
  }
}
