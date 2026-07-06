import { statSync } from 'node:fs';
import path from 'node:path';
import { execFileSync } from 'node:child_process';

const uiRoot = process.cwd();

const assetBudgets = [
  {
    path: 'public/images/hero-child-playing.avif',
    maximumKiB: 32,
    reason: 'The homepage LCP image should stay aggressively compressed.',
  },
  {
    path: 'public/images/hero-child-playing.png',
    maximumKiB: 320,
    reason: 'The retained legacy hero source should not grow while it remains tracked.',
  },
  {
    path: 'public/fonts/HankenGrotesk-VariableFont_wght.woff2',
    maximumKiB: 64,
    reason: 'The delivered regular variable font should stay close to the optimized baseline.',
  },
  {
    path: 'public/fonts/HankenGrotesk-Italic-VariableFont_wght.woff2',
    maximumKiB: 64,
    reason: 'The delivered italic variable font should stay close to the optimized baseline.',
  },
  {
    path: 'public/fonts/HankenGrotesk-VariableFont_wght.ttf',
    maximumKiB: 160,
    reason: 'The retained source TTF should not grow unexpectedly while it remains tracked.',
  },
  {
    path: 'public/fonts/HankenGrotesk-Italic-VariableFont_wght.ttf',
    maximumKiB: 160,
    reason: 'The retained source italic TTF should not grow unexpectedly while it remains tracked.',
  },
];

function formatKiB(sizeInBytes) {
  return `${(sizeInBytes / 1024).toFixed(1)} KiB`;
}

function readTrackedAssets() {
  const output = execFileSync('git', ['ls-files', '--', 'public/images', 'public/fonts'], {
    cwd: uiRoot,
    encoding: 'utf8',
  });

  return output
    .split(/\r?\n/u)
    .map((line) => line.trim())
    .filter((line) => line.length > 0)
    .sort((left, right) => left.localeCompare(right));
}

const trackedAssets = readTrackedAssets();
const trackedAssetSet = new Set(trackedAssets);
const configuredBudgetMap = new Map(assetBudgets.map((budget) => [budget.path, budget]));

const missingBudgets = trackedAssets.filter((trackedAsset) => !configuredBudgetMap.has(trackedAsset));

if (missingBudgets.length > 0) {
  console.error('Missing asset budgets for tracked UI public assets:');

  for (const missingBudget of missingBudgets) {
    console.error(`- ${missingBudget}`);
  }

  console.error('Add explicit thresholds in UI/scripts/check-asset-sizes.mjs so new assets stay reviewable.');
  process.exit(1);
}

const missingTrackedFiles = assetBudgets
  .map((budget) => budget.path)
  .filter((budgetPath) => !trackedAssetSet.has(budgetPath));

if (missingTrackedFiles.length > 0) {
  console.error('Asset-size budgets reference files that are no longer git-tracked:');

  for (const missingTrackedFile of missingTrackedFiles) {
    console.error(`- ${missingTrackedFile}`);
  }

  console.error('Remove or update the stale entries in UI/scripts/check-asset-sizes.mjs.');
  process.exit(1);
}

let failureCount = 0;

for (const budget of assetBudgets) {
  const absolutePath = path.join(uiRoot, budget.path);
  const fileSizeInBytes = statSync(absolutePath).size;
  const maximumBytes = budget.maximumKiB * 1024;
  const status = fileSizeInBytes <= maximumBytes ? 'PASS' : 'FAIL';

  console.log(`${status} ${budget.path} ${formatKiB(fileSizeInBytes)} / ${budget.maximumKiB} KiB max`);

  if (fileSizeInBytes > maximumBytes) {
    failureCount += 1;
    console.error(`  ${budget.reason}`);
  }
}

if (failureCount > 0) {
  console.error(`Asset size check failed with ${failureCount} oversized file(s).`);
  process.exit(1);
}

console.log(`Asset size check passed for ${assetBudgets.length} tracked file(s).`);
