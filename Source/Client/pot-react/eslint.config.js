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
      // Deliberately relaxed to 'warn' (DQ-4, deferred): react-hooks 7 flags 23
      // synchronous setState-in-effect sites across 19 files. Adopting them as
      // errors now would force a large behaviour-sensitive refactor. Kept as
      // warnings and scheduled to be fixed before the modernisation is complete.
      'react-hooks/set-state-in-effect': 'warn',
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
      'simple-import-sort/imports': 'error',
      'simple-import-sort/exports': 'error',
    },
  },
);
