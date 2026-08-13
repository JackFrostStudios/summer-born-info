import { fileURLToPath } from 'node:url';
import { defineConfig } from 'vitest/config';

export default defineConfig({
  resolve: {
    alias: {
      '@a11y-test-helpers': fileURLToPath(new URL('./src/testing/a11y/a11y-test-helpers.ts', import.meta.url)),
    },
  },
});
