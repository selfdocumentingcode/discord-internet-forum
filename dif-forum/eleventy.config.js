require('dotenv').config();

const { EleventyRenderPlugin, EleventyHtmlBasePlugin } = require('@11ty/eleventy');
const lucideIcons = require('@grimlink/eleventy-plugin-lucide-icons');

const readDirRecursively = require('./src/utils/readDirRecursively');
const fallbackAvatarUrl = require('./src/shortcodes/fallbackAvatarUrl');

const mockDataDirectory = '_mockData/';

module.exports = function (eleventyConfig) {
  eleventyConfig.setServerOptions({
    watch: ['_site/**/*.css'],
  });
  eleventyConfig.setServerPassthroughCopyBehavior('passthrough');

  eleventyConfig.addPlugin(EleventyRenderPlugin);
  eleventyConfig.addPlugin(EleventyHtmlBasePlugin);
  eleventyConfig.addPlugin(lucideIcons);

  eleventyConfig.addFilter('toObject', (str) => JSON.parse(str.replaceAll("'", '"')));

  eleventyConfig.addPassthroughCopy('src/assets/discord-avatars/');
  eleventyConfig.addPassthroughCopy('src/assets/icons/');
  eleventyConfig.addPassthroughCopy({ 'src/assets/favicons/': '/' });

  eleventyConfig.setLiquidOptions({
    jsTruthy: true,
  });

  eleventyConfig.addShortcode('fallbackAvatarUrl', fallbackAvatarUrl);

  eleventyConfig.addWatchTarget('./_mockData/');

  eleventyConfig.addGlobalData('mockData', async () => {
    const data = await readDirRecursively(mockDataDirectory);

    return data;
  });

  return {
    dir: {
      input: 'src',
      output: '_site',
    },
  };
};
