CREATE TABLE [dbo].[department_contact]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [DepartmentId] INT NOT NULL, 
    [DepartmentName] VARCHAR(MAX) NOT NULL, 
    [ContactId] INT NOT NULL, 
    [ContactName] VARCHAR(MAX) NOT NULL, 
    [LastUpdated] DATETIME2 NOT NULL 
)
