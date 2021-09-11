IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'EasyDbMigrator')
BEGIN
    CREATE DATABASE EasyDbMigrator
END