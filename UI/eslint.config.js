// @ts-check
const eslint = require('@eslint/js');
const { defineConfig } = require('eslint/config');
const tseslint = require('typescript-eslint');
const angular = require('angular-eslint');

module.exports = defineConfig([
  {
    files: ['**/*.ts'],
    languageOptions: {
      parserOptions: {
        projectService: true,
      },
    },
    extends: [
      eslint.configs.recommended,
      tseslint.configs.strictTypeChecked,
      tseslint.configs.stylisticTypeChecked,
      angular.configs.tsAll,
    ],
    processor: angular.processInlineTemplates,
    rules: {
      '@angular-eslint/directive-selector': [
        'error',
        {
          type: 'attribute',
          prefix: 'sbi',
          style: 'camelCase',
        },
      ],
      '@angular-eslint/component-selector': [
        'error',
        {
          type: 'element',
          prefix: 'sbi',
          style: 'kebab-case',
        },
      ],
      '@angular-eslint/prefer-on-push-component-change-detection': 'off',
      '@angular-eslint/use-component-view-encapsulation': 'off',
      '@angular-eslint/component-class-suffix': 'off',
      '@angular-eslint/directive-class-suffix': 'off',
      '@angular-eslint/no-developer-preview': 'off',
      '@angular-eslint/no-experimental': 'off',
      '@typescript-eslint/no-extraneous-class': [
        'error',
        {
          allowEmpty: true,
        },
      ],
      'no-restricted-syntax': [
        'error',
        {
          selector:
            "PropertyDefinition[key.type='Identifier']:not([key.name=/^\\$/])[value.type='CallExpression'][value.callee.type='Identifier'][value.callee.name=/^(signal|computed|linkedSignal|model)$/]",
          message:
            "Prefix signal-backed and computed class fields with '$' so they can be called from templates under the Angular ESLint allowPrefix convention.",
        },
        {
          selector:
            "PropertyDefinition[key.type='Identifier']:not([key.name=/^\\$/])[value.type='CallExpression'][value.callee.type='MemberExpression'][value.callee.property.name='asReadonly']",
          message: "Prefix readonly signal views with '$' so their signal nature stays obvious and template-safe.",
        },
        {
          selector:
            "VariableDeclarator[id.type='Identifier']:not([id.name=/^\\$/])[init.type='CallExpression'][init.callee.type='Identifier'][init.callee.name=/^(signal|computed|linkedSignal|model)$/]",
          message:
            "Prefix signal-backed and computed variables with '$' so they are easy to spot and safe to call from templates.",
        },
        {
          selector:
            "VariableDeclarator[id.type='Identifier']:not([id.name=/^\\$/])[init.type='CallExpression'][init.callee.type='MemberExpression'][init.callee.property.name='asReadonly']",
          message: "Prefix readonly signal views with '$' so their signal nature stays obvious and template-safe.",
        },
      ],
    },
  },
  {
    files: ['src/app/**/*.html'],
    extends: [angular.configs.templateAll, angular.configs.templateAccessibility],
    rules: {
      '@angular-eslint/template/no-call-expression': [
        'error',
        {
          allowPrefix: '$',
        },
      ],
    },
  },
]);
