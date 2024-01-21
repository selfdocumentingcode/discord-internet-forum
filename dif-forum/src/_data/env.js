require('dotenv').config();
const process = require('process');

module.exports = function () {
  return {
    environment: process.env.ENVIRONMENT || 'Development',
  };
};
