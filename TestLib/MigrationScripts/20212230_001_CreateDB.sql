USE Master
IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'MyProductionDatabase')
BEGIN
    CREATE DATABASE EasyDbMigrator
END