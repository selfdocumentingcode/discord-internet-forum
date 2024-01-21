const getPostgresql = require('../utils/getPostgresql');

module.exports = async function () {
  const sql = getPostgresql();

  const forumsResult = await sql`
        SELECT "ForumChannelJson"
        FROM "forum"."PublishedForums"
        ORDER BY "CreatedDate" ASC;
    `;

  const forums = forumsResult.map((forum) => forum.ForumChannelJson);

  sql.end();

  return forums;
};
