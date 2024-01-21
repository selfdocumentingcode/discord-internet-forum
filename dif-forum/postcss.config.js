module.exports = ({ env }) => {
  const isProd = (env ?? '').trim() === 'production';

  return {
    plugins: {
      tailwindcss: {},
      autoprefixer: {},
      cssnano: isProd ? { preset: 'default' } : false,
    },
  };
};
