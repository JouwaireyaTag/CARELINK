--1. CREATE DATABASE 

IF DB_ID(N'CARELINK_V2') IS NULL
BEGIN
    CREATE DATABASE [CARELINK_V2];
END;
GO

USE [CARELINK_V2];
GO

-- 2. CREATE SEQUENCES 

CREATE SEQUENCE dbo.Seq_ApplicationUsers
AS INT START WITH 1 INCREMENT BY 1 MINVALUE 1 MAXVALUE 999999 NO CYCLE;
GO

CREATE SEQUENCE dbo.Seq_PatientProfile
AS INT START WITH 1 INCREMENT BY 1 MINVALUE 1 MAXVALUE 999999 NO CYCLE;
GO

CREATE SEQUENCE dbo.Seq_DoctorProfile
AS INT START WITH 1 INCREMENT BY 1 MINVALUE 1 MAXVALUE 999999 NO CYCLE;
GO

CREATE SEQUENCE dbo.Seq_AccessRequests
AS INT START WITH 1 INCREMENT BY 1 MINVALUE 1 MAXVALUE 999999 NO CYCLE;
GO

CREATE SEQUENCE dbo.Seq_AccessGrants
AS INT START WITH 1 INCREMENT BY 1 MINVALUE 1 MAXVALUE 999999 NO CYCLE;
GO

CREATE SEQUENCE dbo.Seq_AuditLogs
AS INT START WITH 1 INCREMENT BY 1 MINVALUE 1 MAXVALUE 999999 NO CYCLE;
GO

CREATE SEQUENCE dbo.Seq_MedicalConditions
AS INT START WITH 1 INCREMENT BY 1 MINVALUE 1 MAXVALUE 999999 NO CYCLE;
GO

CREATE SEQUENCE dbo.Seq_Allergies
AS INT START WITH 1 INCREMENT BY 1 MINVALUE 1 MAXVALUE 999999 NO CYCLE;
GO

CREATE SEQUENCE dbo.Seq_Surgeries
AS INT START WITH 1 INCREMENT BY 1 MINVALUE 1 MAXVALUE 999999 NO CYCLE;
GO

CREATE SEQUENCE dbo.Seq_Medications
AS INT START WITH 1 INCREMENT BY 1 MINVALUE 1 MAXVALUE 999999 NO CYCLE;
GO

CREATE SEQUENCE dbo.Seq_MedicalVisits
AS INT START WITH 1 INCREMENT BY 1 MINVALUE 1 MAXVALUE 999999 NO CYCLE;
GO

--3. APPLICATION USERS 

CREATE TABLE dbo.ApplicationUsers
(
    User_Id VARCHAR(20) NOT NULL
        CONSTRAINT DF_ApplicationUsers_User_Id DEFAULT
        ('USR-' + RIGHT('000000' + CONVERT(VARCHAR(20),
        NEXT VALUE FOR dbo.Seq_ApplicationUsers), 6)),

    Email NVARCHAR(256) NOT NULL,
    Role NVARCHAR(128) NOT NULL,
    PasswordHash NVARCHAR(256) NOT NULL,

    CONSTRAINT PK_ApplicationUsers PRIMARY KEY (User_Id),
    CONSTRAINT UQ_ApplicationUsers_Email UNIQUE (Email)
);
GO

--4. PATIENT PROFILE 

CREATE TABLE dbo.PatientProfile
(
    Patient_Id VARCHAR(20) NOT NULL
        CONSTRAINT DF_PatientProfile_Patient_Id DEFAULT
        ('PT-' + RIGHT('000000' + CONVERT(VARCHAR(20),
        NEXT VALUE FOR dbo.Seq_PatientProfile), 6)),

    User_Id VARCHAR(20) NOT NULL,
    FullName NVARCHAR(150) NULL,
    DateOfBirth DATE NULL,
    NationalIdEncrypted NVARCHAR(256) NULL,

    CONSTRAINT PK_PatientProfile PRIMARY KEY (Patient_Id),
    CONSTRAINT UQ_PatientProfile_User_Id UNIQUE (User_Id),

    CONSTRAINT FK_PatientProfile_ApplicationUsers
        FOREIGN KEY (User_Id)
        REFERENCES dbo.ApplicationUsers(User_Id)
);
GO

--5. DOCTOR PROFILE 

CREATE TABLE dbo.DoctorProfile
(
    Doctor_Id VARCHAR(20) NOT NULL
        CONSTRAINT DF_DoctorProfile_Doctor_Id DEFAULT
        ('DR-' + RIGHT('000000' + CONVERT(VARCHAR(20),
        NEXT VALUE FOR dbo.Seq_DoctorProfile), 6)),

    User_Id VARCHAR(20) NOT NULL,
    Specialty NVARCHAR(256) NULL,
    FullName NVARCHAR(150) NULL,
    LicenseNumber NVARCHAR(256) NULL,

    Is_Approved BIT NOT NULL
        CONSTRAINT DF_DoctorProfile_Is_Approved DEFAULT (0),

    CONSTRAINT PK_DoctorProfile PRIMARY KEY (Doctor_Id),
    CONSTRAINT UQ_DoctorProfile_User_Id UNIQUE (User_Id),

    CONSTRAINT FK_DoctorProfile_ApplicationUsers
        FOREIGN KEY (User_Id)
        REFERENCES dbo.ApplicationUsers(User_Id)
);
GO

CREATE UNIQUE INDEX UX_DoctorProfile_LicenseNumber
ON dbo.DoctorProfile(LicenseNumber)
WHERE LicenseNumber IS NOT NULL;
GO

--6. ACCESS REQUESTS 

CREATE TABLE dbo.AccessRequests
(
    Request_Id VARCHAR(20) NOT NULL
        CONSTRAINT DF_AccessRequests_Request_Id DEFAULT
        ('REQ-' + RIGHT('000000' + CONVERT(VARCHAR(20),
        NEXT VALUE FOR dbo.Seq_AccessRequests), 6)),

    Doctor_Id VARCHAR(20) NOT NULL,
    Patient_Id VARCHAR(20) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,

    CONSTRAINT PK_AccessRequests PRIMARY KEY (Request_Id),

    CONSTRAINT CK_AccessRequests_Status
        CHECK (Status IN ('Pending', 'Approved', 'Rejected', 'Expired')),

    CONSTRAINT FK_AccessRequests_DoctorProfile
        FOREIGN KEY (Doctor_Id)
        REFERENCES dbo.DoctorProfile(Doctor_Id),

    CONSTRAINT FK_AccessRequests_PatientProfile
        FOREIGN KEY (Patient_Id)
        REFERENCES dbo.PatientProfile(Patient_Id)
);
GO

--7. ACCESS GRANTS 

CREATE TABLE dbo.AccessGrants
(
    Access_Id VARCHAR(20) NOT NULL
        CONSTRAINT DF_AccessGrants_Access_Id DEFAULT
        ('ACC-' + RIGHT('000000' + CONVERT(VARCHAR(20),
        NEXT VALUE FOR dbo.Seq_AccessGrants), 6)),

    Request_Id VARCHAR(20) NOT NULL,
    CodeHash NVARCHAR(256) NOT NULL,
    ExpiredAt DATETIME2 NOT NULL,

    IsUsed BIT NOT NULL
        CONSTRAINT DF_AccessGrants_IsUsed DEFAULT (0),

    Attempts INT NOT NULL
        CONSTRAINT DF_AccessGrants_Attempts DEFAULT (0),

    CONSTRAINT PK_AccessGrants PRIMARY KEY (Access_Id),
    CONSTRAINT UQ_AccessGrants_Request_Id UNIQUE (Request_Id),
    CONSTRAINT CK_AccessGrants_Attempts CHECK (Attempts >= 0),

    CONSTRAINT FK_AccessGrants_AccessRequests
        FOREIGN KEY (Request_Id)
        REFERENCES dbo.AccessRequests(Request_Id)
);
GO

--8. AUDIT LOGS 

CREATE TABLE dbo.AuditLogs
(
    Audit_Id VARCHAR(20) NOT NULL
        CONSTRAINT DF_AuditLogs_Audit_Id DEFAULT
        ('LOG-' + RIGHT('000000' + CONVERT(VARCHAR(20),
        NEXT VALUE FOR dbo.Seq_AuditLogs), 6)),

    User_Id VARCHAR(20) NULL,
    Action NVARCHAR(256) NOT NULL,
    LoggedAt DATETIME2 NOT NULL,

    CONSTRAINT PK_AuditLogs PRIMARY KEY (Audit_Id),

    CONSTRAINT FK_AuditLogs_ApplicationUsers
        FOREIGN KEY (User_Id)
        REFERENCES dbo.ApplicationUsers(User_Id)
);
Go

--9. MEDICAL CONDITIONS 

CREATE TABLE dbo.MedicalConditions
(
    Condition_Id VARCHAR(20) NOT NULL
        CONSTRAINT DF_MedicalConditions_Condition_Id DEFAULT
        ('CON-' + RIGHT('000000' + CONVERT(VARCHAR(20),
        NEXT VALUE FOR dbo.Seq_MedicalConditions), 6)),

    Patient_Id VARCHAR(20) NOT NULL,
    ConditionName NVARCHAR(150) NULL,
    DiagnosisDate DATE NULL,
    Notes_Condition NVARCHAR(500) NULL,

    CONSTRAINT PK_MedicalConditions PRIMARY KEY (Condition_Id),

    CONSTRAINT FK_MedicalConditions_PatientProfile
        FOREIGN KEY (Patient_Id)
        REFERENCES dbo.PatientProfile(Patient_Id)
);
GO

--10. ALLERGIES

CREATE TABLE dbo.Allergies
(
    Allergy_Id VARCHAR(20) NOT NULL
        CONSTRAINT DF_Allergies_Allergy_Id DEFAULT
        ('ALG-' + RIGHT('000000' + CONVERT(VARCHAR(20),
        NEXT VALUE FOR dbo.Seq_Allergies), 6)),

    Patient_Id VARCHAR(20) NOT NULL,
    AllergyName NVARCHAR(150) NULL,
    Reaction NVARCHAR(256) NULL,
    Severity NVARCHAR(50) NULL,
    Notes_Allergies NVARCHAR(500) NULL,

    CONSTRAINT PK_Allergies PRIMARY KEY (Allergy_Id),

    CONSTRAINT FK_Allergies_PatientProfile
        FOREIGN KEY (Patient_Id)
        REFERENCES dbo.PatientProfile(Patient_Id)
);
GO

--11. SURGERIES 

CREATE TABLE dbo.Surgeries
(
    Surgery_Id VARCHAR(20) NOT NULL
        CONSTRAINT DF_Surgeries_Surgery_Id DEFAULT
        ('SUR-' + RIGHT('000000' + CONVERT(VARCHAR(20),
        NEXT VALUE FOR dbo.Seq_Surgeries), 6)),

    Patient_Id VARCHAR(20) NOT NULL,
    SurgeryName NVARCHAR(150) NULL,
    SurgeryDate DATE NULL,
    Hospital NVARCHAR(150) NULL,
    Notes_Surgeries NVARCHAR(500) NULL,

    CONSTRAINT PK_Surgeries PRIMARY KEY (Surgery_Id),

    CONSTRAINT FK_Surgeries_PatientProfile
        FOREIGN KEY (Patient_Id)
        REFERENCES dbo.PatientProfile(Patient_Id)
);
GO

--12. MEDICATIONS 

CREATE TABLE dbo.Medications
(
    Medication_Id VARCHAR(20) NOT NULL
        CONSTRAINT DF_Medications_Medication_Id DEFAULT
        ('MED-' + RIGHT('000000' + CONVERT(VARCHAR(20),
        NEXT VALUE FOR dbo.Seq_Medications), 6)),

    Patient_Id VARCHAR(20) NOT NULL,
    MedicationName NVARCHAR(150) NULL,
    Dosage NVARCHAR(100) NULL,
    Frequency NVARCHAR(100) NULL,
    StartDate DATE NULL,
    EndDate DATE NULL,
    Notes_Medications NVARCHAR(500) NULL,

    CONSTRAINT PK_Medications PRIMARY KEY (Medication_Id),

    CONSTRAINT FK_Medications_PatientProfile
        FOREIGN KEY (Patient_Id)
        REFERENCES dbo.PatientProfile(Patient_Id)
);
GO


-- 13. MEDICAL VISITS 

CREATE TABLE dbo.MedicalVisits
(
    Visit_Id VARCHAR(20) NOT NULL
        CONSTRAINT DF_MedicalVisits_Visit_Id DEFAULT
        ('VIS-' + RIGHT('000000' + CONVERT(VARCHAR(20),
        NEXT VALUE FOR dbo.Seq_MedicalVisits), 6)),

    Patient_Id VARCHAR(20) NOT NULL,
    Doctor_Id VARCHAR(20) NOT NULL,
    VisitDate DATETIME2 NULL,
    Diagnosis NVARCHAR(500) NULL,
    Treatment NVARCHAR(500) NULL,
    Notes_Visits NVARCHAR(500) NULL,

    CONSTRAINT PK_MedicalVisits PRIMARY KEY (Visit_Id),

    CONSTRAINT FK_MedicalVisits_PatientProfile
        FOREIGN KEY (Patient_Id)
        REFERENCES dbo.PatientProfile(Patient_Id),

    CONSTRAINT FK_MedicalVisits_DoctorProfile
        FOREIGN KEY (Doctor_Id)
        REFERENCES dbo.DoctorProfile(Doctor_Id)
);
GO

--14. VERIFICATION 

SELECT COUNT(*) AS TableCount
FROM sys.tables
WHERE schema_id = SCHEMA_ID(N'dbo');
GO

SELECT COUNT(*) AS ForeignKeyCount
FROM sys.foreign_keys;
GO
