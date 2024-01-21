const fs = require('fs/promises');
const path = require('path');

async function readDirRecursively(dirPath, obj = {}) {
  // Read the contents of the directory
  const contents = await fs.readdir(dirPath, { withFileTypes: true });

  // Iterate over each item in the directory
  for (const dirent of contents) {
    const fullPath = path.join(dirPath, dirent.name);

    // If it's a directory, recursively read its contents
    if (dirent.isDirectory()) {
      obj[dirent.name] = {};
      await readDirRecursively(fullPath, obj[dirent.name]);
    } else if (dirent.isFile()) {
      // Check file extension and only process if it's .json
      const fileExt = path.extname(dirent.name);
      if (fileExt === '.json') {
        // If it's a file, read its contents and add to the map
        const fileContent = await fs.readFile(fullPath, 'utf-8');
        const fileJson = JSON.parse(fileContent);
        obj[path.basename(dirent.name, fileExt)] = fileJson;
      }
    }
  }

  return obj;
}

module.exports = readDirRecursively;
