const fs = require('fs/promises');
const crypto = require('crypto');

const avatarFolderPath = 'src/assets/discord-avatars';
const avatarUrlPath = '/assets/discord-avatars';

async function fallbackAvatarUrl(userName) {
  // get a list of files in the avatar folder
  const avatarFiles = await fs.readdir(avatarFolderPath);

  const seed = stringToNumericHash(userName);

  // pick a random file from the list using the seed
  const randomAvatar = avatarFiles[seed % avatarFiles.length];

  // return a usable URL
  return `${avatarUrlPath}/${randomAvatar}`;
}

function stringToNumericHash(str) {
  // 1. Create a hash using SHA-256
  const hash = crypto.createHash('sha256').update(str).digest();

  // 2. Convert the first 4 bytes of the hash to a standard number
  const numericHash = hash.readUInt32LE();

  return numericHash;
}

module.exports = fallbackAvatarUrl;
