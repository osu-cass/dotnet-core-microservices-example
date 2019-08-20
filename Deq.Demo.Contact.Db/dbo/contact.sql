CREATE TABLE [dbo].[contact]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Name] VARCHAR(MAX) NOT NULL, 
	[ContactNumber] VARCHAR(MAX) NOT NULL, 
    [DateOfBirth] DATETIME NOT NULL, 
    [DepartmentId] VARCHAR(MAX) NOT NULL,
    [LastUpdated] DATETIME2 NOT NULL
)
