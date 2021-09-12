USE EasyDbMigrator
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='aa' AND xtype='U')
BEGIN
    CREATE TABLE aa 
    (
        Id int IDENTITY(1,1) PRIMARY KEY,
        Names nvarchar(150) NOT NULL
    )
END