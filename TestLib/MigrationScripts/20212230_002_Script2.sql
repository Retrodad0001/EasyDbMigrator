USE EasyDbMigrator

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='bb' AND xtype='U')
BEGIN
   CREATE TABLE bb 
    (
        Id int IDENTITY(1,1) PRIMARY KEY,
        Stuff2 nvarchar(100) NOT NULL,
        County int 
     )
END