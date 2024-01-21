module.exports = {
  trailingComma: 'es5',
  printWidth: 120,
  tabWidth: 2,
  useTabs: false,
  semi: true,
  singleQuote: true,
  overrides: [
    {
      files: ['*.liquid'],
      options: {
        singleQuote: false,
        liquidSingleQuote: false,
        indentSchema: true,
      },
    },
  ],
  plugins: ['@shopify/prettier-plugin-liquid', 'prettier-plugin-tailwindcss'],
};
