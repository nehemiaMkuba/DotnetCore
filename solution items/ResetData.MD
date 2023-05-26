## Clean the Main DB Tables

DECLARE @Sql NVARCHAR(500) DECLARE @Cursor CURSOR

SET @Cursor = CURSOR FAST_FORWARD FOR
SELECT DISTINCT sql = 'ALTER TABLE [' + tc2.TABLE_SCHEMA + '].[' +  tc2.TABLE_NAME + '] DROP [' + rc1.CONSTRAINT_NAME + '];'
FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc1
LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc2 ON tc2.CONSTRAINT_NAME =rc1.CONSTRAINT_NAME

OPEN @Cursor FETCH NEXT FROM @Cursor INTO @Sql

WHILE (@@FETCH_STATUS = 0)
BEGIN
Exec sp_executesql @Sql
FETCH NEXT FROM @Cursor INTO @Sql
END

CLOSE @Cursor DEALLOCATE @Cursor
GO

EXEC sp_MSforeachtable 'DROP TABLE ?'
GO


## Drop all sequences
DECLARE @name VARCHAR(128)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP(1) sch.name + '.' + seq.name AS [Sequence] FROM sys.sequences AS seq JOIN sys.schemas AS sch ON seq.schema_id = sch.schema_id ORDER BY [Sequence])

WHILE @name IS NOT NULL
BEGIN
    SELECT @SQL = 'DROP SEQUENCE IF EXISTS ' + RTRIM(@name)
    EXEC (@SQL)
    PRINT 'Dropped Sequence: ' + @name
    SELECT @name = (SELECT TOP(1) sch.name + '.' + seq.name AS [Sequence] FROM sys.sequences AS seq JOIN sys.schemas AS sch ON seq.schema_id = sch.schema_id ORDER BY [Sequence])
END
GO


## Clean Hangfire
DROP TABLE [HangFire].[State]
DROP TABLE [HangFire].[Set]
DROP TABLE [HangFire].[Server]
DROP TABLE [HangFire].[List]
DROP TABLE [HangFire].[Schema]
DROP TABLE [HangFire].[JobQueue]
DROP TABLE [HangFire].[JobParameter]
DROP TABLE [HangFire].[Job]
DROP TABLE [HangFire].[Hash]
DROP TABLE [HangFire].[Counter]
DROP TABLE [HangFire].[AggregatedCounter]




## Works only for contained databases
CREATE USER [HangFire] WITH PASSWORD = 'B795aa0f-aa21-457e-8b27-3567b46a23Ef'
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE [name] = 'HangFire') EXEC ('CREATE SCHEMA [HangFire]')
GO

ALTER AUTHORIZATION ON SCHEMA::[HangFire] TO [HangFire]
GO

GRANT CREATE TABLE TO [HangFire]
GO
