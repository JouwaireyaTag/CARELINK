
USE CARELINK_V2;
GO


  --CARELINK V2 - FINAL DATABASE VERIFICATION


-- 1. PRIMARY KEYS AND UNIQUE CONSTRAINTS

SELECT
    'PRIMARY / UNIQUE' AS CheckType,
    t.name AS TableName,
    kc.name AS ConstraintName,
    kc.type_desc AS ConstraintType
FROM sys.key_constraints kc
INNER JOIN sys.tables t
    ON kc.parent_object_id = t.object_id
WHERE t.schema_id = SCHEMA_ID('dbo')

UNION ALL

-- Unique indexes, including LicenseNumber
SELECT
    'UNIQUE INDEX',
    t.name,
    i.name,
    'UNIQUE INDEX'
FROM sys.indexes i
INNER JOIN sys.tables t
    ON i.object_id = t.object_id
WHERE i.is_unique = 1
  AND i.is_primary_key = 0
  AND i.is_unique_constraint = 0
  AND t.schema_id = SCHEMA_ID('dbo');


-- 2. FOREIGN KEYS

SELECT
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS ChildTable,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id)
        AS ForeignKeyColumn,
    OBJECT_NAME(fk.referenced_object_id) AS ParentTable,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id)
        AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc
    ON fk.object_id = fkc.constraint_object_id
ORDER BY ChildTable;


-- 3. CHECK CONSTRAINTS

SELECT
    t.name AS TableName,
    cc.name AS CheckConstraint,
    cc.definition AS CheckExpression,
    cc.is_disabled AS IsDisabled
FROM sys.check_constraints cc
INNER JOIN sys.tables t
    ON cc.parent_object_id = t.object_id
WHERE t.schema_id = SCHEMA_ID('dbo');


-- 4. DEFAULT CONSTRAINTS

SELECT
    t.name AS TableName,
    c.name AS ColumnName,
    dc.name AS DefaultConstraint,
    dc.definition AS DefaultValue
FROM sys.default_constraints dc
INNER JOIN sys.tables t
    ON dc.parent_object_id = t.object_id
INNER JOIN sys.columns c
    ON c.object_id = dc.parent_object_id
   AND c.column_id = dc.parent_column_id
WHERE t.schema_id = SCHEMA_ID('dbo')
ORDER BY t.name, c.column_id;


-- 5. SEQUENCES

SELECT
    name AS SequenceName,
    start_value AS StartValue,
    increment AS IncrementValue,
    minimum_value AS MinimumValue,
    maximum_value AS MaximumValue,
    is_cycling AS IsCycling
FROM sys.sequences
WHERE schema_id = SCHEMA_ID('dbo')
ORDER BY name;


-- 6. TABLE AND RELATIONSHIP COUNTS

SELECT
    (SELECT COUNT(*)
     FROM sys.tables
     WHERE schema_id = SCHEMA_ID('dbo')) AS TotalTables,

    (SELECT COUNT(*)
     FROM sys.foreign_keys) AS TotalForeignKeys,

    (SELECT COUNT(*)
     FROM sys.sequences
     WHERE schema_id = SCHEMA_ID('dbo')) AS TotalSequences,

    (SELECT COUNT(*)
     FROM sys.check_constraints) AS TotalCheckConstraints,

    (SELECT COUNT(*)
     FROM sys.default_constraints) AS TotalDefaultConstraints;
GO
