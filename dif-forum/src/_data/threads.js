const getPostgresql = require('../utils/getPostgresql');

module.exports = async function () {
  const sql = getPostgresql();

  const forumsResult = await sql`
        SELECT "ForumThreadJson"
        FROM "forum"."PublishedThreads"
        ORDER BY "UpdatedDate" desc;
    `;

  const threads = forumsResult.map((forum) => forum.ForumThreadJson);

  sql.end();

  return threads;
};
