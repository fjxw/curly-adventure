#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE DATABASE employees;
    CREATE DATABASE payroll;
    CREATE DATABASE recruitment;
    CREATE DATABASE attendance;
    CREATE DATABASE documents;
    
    GRANT ALL PRIVILEGES ON DATABASE employees TO hrmanagement;
    GRANT ALL PRIVILEGES ON DATABASE payroll TO hrmanagement;
    GRANT ALL PRIVILEGES ON DATABASE recruitment TO hrmanagement;
    GRANT ALL PRIVILEGES ON DATABASE attendance TO hrmanagement;
    GRANT ALL PRIVILEGES ON DATABASE documents TO hrmanagement;
EOSQL
