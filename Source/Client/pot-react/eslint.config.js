import js from '@eslint/js';
import reactHooks from 'eslint-plugin-react-hooks';
import reactRefresh from 'eslint-plugin-react-refresh';
import simpleImportSort from 'eslint-plugin-simple-import-sort';
import globals from 'globals';
import tseslint from 'typescript-eslint';

export default tseslint.config(
  {
    ignores: ['dist/**', 'src/lib/utils.ts', 'src/components/ui/'],
  },
  {
    extends: [
      js.configs.recommended,
      ...tseslint.configs.recommended,
      ...tseslint.configs.stylistic,
    ],
    files: ['**/*.{ts,tsx}'],
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
    },
    plugins: {
      'react-hooks': reactHooks,
      'react-refresh': reactRefresh,
      'simple-import-sort': simpleImportSort,
    },
    rules: {
      ...reactHooks.configs.recommended.rules,
      // DQ-4 resolved (2026-09-08): all set-state-in-effect sites were fixed via
      // useSyncExternalStore / render-time state adjustments / handler-driven
      // resets, so the recommended 'error' severity now guards against regressions.
      'react-refresh/only-export-components': ['off'], // ['warn', { allowConstantExport: true }]
      '@typescript-eslint/consistent-type-definitions': ['error', 'type'],
      '@typescript-eslint/consistent-type-imports': [
        'error',
        { prefer: 'type-imports' },
      ],
      '@typescript-eslint/no-unused-vars': [
        'error',
        { argsIgnorePattern: '^_$', varsIgnorePattern: '^_$' },
      ],
      // Test doubles (empty mocks/stubs) are intentional in the test suites.
      '@typescript-eslint/no-empty-function': [
        'error',
        { allow: ['arrowFunctions', 'methods'] },
      ],
      'simple-import-sort/imports': 'error',
      'simple-import-sort/exports': 'error',
    },
  },
  {
    // E2E runs under Node (Playwright + global setup), not the browser.
    files: [
      'e2e/**/*.{ts,tsx}',
      'playwright.config.ts',
      'playwright.prod.config.ts',
      'playwright.globalSetup.ts',
    ],
    languageOptions: {
      globals: globals.node,
    },
    rules: {
      // The E2E suites deliberately use inline `import()` type annotations
      // (e.g. `import('@playwright/test').TestInfo`) throughout; converting all
      // of them to named type imports is not worth the churn. Other rules
      // (import sort, array-type, no-unused-vars, …) still apply.
      '@typescript-eslint/consistent-type-imports': 'off',
    },
  },
  {
    // Unit tests use the Vitest `typeof import('...')` re-export idiom, which
    // conflicts with the type-import style rule. Keep that hygiene in src (app
    // code) but allow it in tests.
    files: ['tests/**/*.{ts,tsx}'],
    rules: {
      '@typescript-eslint/consistent-type-imports': 'off',
    },
  },
);
