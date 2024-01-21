require('dotenv').config();
const process = require('process');
const postgres = require('postgres');

const PGHOSTADDR = process.env.PGHOSTADDR;
const PGPORT = process.env.PGPORT;
const PGUSER = process.env.PGUSER;
const PGPASSWORD = process.env.PGPASSWORD;
const PGDATABASE = process.env.PGDATABASE;

const maxLifetimeSeconds = 30;

function getPostgresql() {
  const sql = postgres({
    host: PGHOSTADDR,
    port: PGPORT,
    username: PGUSER,
    password: PGPASSWORD,
    database: PGDATABASE,
    max_lifetime: maxLifetimeSeconds,
  });

  return sql;
}

module.exports = getPostgresql;
