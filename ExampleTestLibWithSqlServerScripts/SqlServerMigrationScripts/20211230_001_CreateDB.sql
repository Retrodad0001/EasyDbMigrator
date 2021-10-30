USE Master

IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'MyProductionDatabaseSqlServer')
BEGIN
    CREATE DATABASE EasyDbMigrator
END