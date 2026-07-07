import {
  AngularNodeAppEngine,
  createNodeRequestHandler,
  isMainModule,
  writeResponseToNodeResponse,
} from '@angular/ssr/node';
import express from 'express';
import { existsSync, readdirSync } from 'node:fs';
import { join } from 'node:path';

const browserDistFolder = join(import.meta.dirname, '../browser');
const localizedBrowserAssetRoot = existsSync(browserDistFolder)
  ? readdirSync(browserDistFolder, { withFileTypes: true })
      .filter((entry) => entry.isDirectory())
      .map((entry) => join(browserDistFolder, entry.name))
      .find((directory) => existsSync(join(directory, 'index.html')))
  : undefined;

const app = express();
const angularApp = new AngularNodeAppEngine();

/**
 * Example Express Rest API endpoints can be defined here.
 * Uncomment and define endpoints as necessary.
 *
 * Example:
 * ```ts
 * app.get('/api/{*splat}', (req, res) => {
 *   // Handle API request
 * });
 * ```
 */

/**
 * When a localized build emits shared static assets under a locale folder such as /en-GB/,
 * keep the existing root-relative CSS asset URLs working in the production SSR host.
 */
for (const assetFolder of ['fonts', 'icons', 'images']) {
  if (localizedBrowserAssetRoot !== undefined && existsSync(join(localizedBrowserAssetRoot, assetFolder))) {
    app.use(
      `/${assetFolder}`,
      express.static(join(localizedBrowserAssetRoot, assetFolder), {
        maxAge: '1y',
        index: false,
        redirect: false,
      }),
    );
  }
}

/**
 * Serve static files from /browser
 */
app.use(
  express.static(browserDistFolder, {
    maxAge: '1y',
    index: false,
    redirect: false,
  }),
);

/**
 * Handle all other requests by rendering the Angular application.
 */
app.use((req, res, next) => {
  angularApp
    .handle(req)
    .then(async (response) => {
      if (response) {
        await writeResponseToNodeResponse(response, res);
        return;
      }

      next();
    })
    .catch(next);
});

/**
 * Start the server if this module is the main entry point, or it is ran via PM2.
 * The server listens on the port defined by the `PORT` environment variable, or defaults to 4000.
 */
if (isMainModule(import.meta.url) || process.env['pm_id']) {
  const port = Number(process.env['PORT'] ?? 4000);
  app.listen(port, (error) => {
    if (error) {
      throw error;
    }

    console.log(`Node Express server listening on http://localhost:${String(port)}`);
  });
}

/**
 * Request handler used by the Angular CLI (for dev-server and during build) or Firebase Cloud Functions.
 */
export const reqHandler = createNodeRequestHandler(app);
