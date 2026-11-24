
USE CARELINK_V2;
GO

-- CARELINK V2 - Test Automatic ID Generation

DECLARE @Users TABLE
(
    User_Id VARCHAR(20),
    Email NVARCHAR(256)
);

DECLARE @Patient TABLE (Patient_Id VARCHAR(20));
DECLARE @Doctor TABLE (Doctor_Id VARCHAR(20));
DECLARE @Request TABLE (Request_Id VARCHAR(20));
DECLARE @Grant TABLE (Access_Id VARCHAR(20));
DECLARE @Audit TABLE (Audit_Id VARCHAR(20));
DECLARE @Condition TABLE (Condition_Id VARCHAR(20));
DECLARE @Allergy TABLE (Allergy_Id VARCHAR(20));
DECLARE @Surgery TABLE (Surgery_Id VARCHAR(20));
DECLARE @Medication TABLE (Medication_Id VARCHAR(20));
DECLARE @Visit TABLE (Visit_Id VARCHAR(20));

BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. ApplicationUsers (two users)
    INSERT INTO dbo.ApplicationUsers
        (Email, Role, PasswordHash)
    OUTPUT inserted.User_Id, inserted.Email
        INTO @Users
    VALUES
        ('test_patient@carelink.com', 'Patient', 'TEST_HASH'),
        ('test_doctor@carelink.com', 'Doctor', 'TEST_HASH');

    -- 2. PatientProfile
    INSERT INTO dbo.PatientProfile
        (User_Id, FullName)
    OUTPUT inserted.Patient_Id INTO @Patient
    SELECT User_Id, 'Test Patient'
    FROM @Users
    WHERE Email = 'test_patient@carelink.com';

    -- 3. DoctorProfile
    INSERT INTO dbo.DoctorProfile
        (User_Id, Specialty, FullName)
    OUTPUT inserted.Doctor_Id INTO @Doctor
    SELECT User_Id, 'Cardiology', 'Test Doctor'
    FROM @Users
    WHERE Email = 'test_doctor@carelink.com';

    -- 4. AccessRequests
    INSERT INTO dbo.AccessRequests
        (Doctor_Id, Patient_Id, Status, CreatedAt)
    OUTPUT inserted.Request_Id INTO @Request
    SELECT d.Doctor_Id, p.Patient_Id, 'Pending', GETDATE()
    FROM @Doctor d CROSS JOIN @Patient p;

    -- 5. AccessGrants
    INSERT INTO dbo.AccessGrants
        (Request_Id, CodeHash, ExpiredAt)
    OUTPUT inserted.Access_Id INTO @Grant
    SELECT Request_Id, 'TEST_HASH', DATEADD(HOUR, 1, GETDATE())
    FROM @Request;

    -- 6. AuditLogs
    INSERT INTO dbo.AuditLogs
        (Action, LoggedAt)
    OUTPUT inserted.Audit_Id INTO @Audit
    VALUES ('Test ID Generation', GETDATE());

    -- 7. MedicalConditions
    INSERT INTO dbo.MedicalConditions
        (Patient_Id, ConditionName)
    OUTPUT inserted.Condition_Id INTO @Condition
    SELECT Patient_Id, 'Test Condition' FROM @Patient;

    -- 8. Allergies
    INSERT INTO dbo.Allergies
        (Patient_Id, AllergyName)
    OUTPUT inserted.Allergy_Id INTO @Allergy
    SELECT Patient_Id, 'Test Allergy' FROM @Patient;

    -- 9. Surgeries
    INSERT INTO dbo.Surgeries
        (Patient_Id, SurgeryName)
    OUTPUT inserted.Surgery_Id INTO @Surgery
    SELECT Patient_Id, 'Test Surgery' FROM @Patient;

    -- 10. Medications
    INSERT INTO dbo.Medications
        (Patient_Id, MedicationName)
    OUTPUT inserted.Medication_Id INTO @Medication
    SELECT Patient_Id, 'Test Medication' FROM @Patient;

    -- 11. MedicalVisits
    INSERT INTO dbo.MedicalVisits
        (Patient_Id, Doctor_Id, VisitDate)
    OUTPUT inserted.Visit_Id INTO @Visit
    SELECT p.Patient_Id, d.Doctor_Id, GETDATE()
    FROM @Patient p CROSS JOIN @Doctor d;

    -- Display all generated IDs
    SELECT 'ApplicationUsers' AS TableName, User_Id AS GeneratedID FROM @Users
    UNION ALL SELECT 'PatientProfile', Patient_Id FROM @Patient
    UNION ALL SELECT 'DoctorProfile', Doctor_Id FROM @Doctor
    UNION ALL SELECT 'AccessRequests', Request_Id FROM @Request
    UNION ALL SELECT 'AccessGrants', Access_Id FROM @Grant
    UNION ALL SELECT 'AuditLogs', Audit_Id FROM @Audit
    UNION ALL SELECT 'MedicalConditions', Condition_Id FROM @Condition
    UNION ALL SELECT 'Allergies', Allergy_Id FROM @Allergy
    UNION ALL SELECT 'Surgeries', Surgery_Id FROM @Surgery
    UNION ALL SELECT 'Medications', Medication_Id FROM @Medication
    UNION ALL SELECT 'MedicalVisits', Visit_Id FROM @Visit;

    ROLLBACK TRANSACTION;

    PRINT 'SUCCESS: All 11 ID generators worked.';
    PRINT 'TEST DATA ROLLED BACK - No test records were saved.';

END TRY
BEGIN CATCH

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT 'TEST FAILED:';
    PRINT ERROR_MESSAGE();

END CATCH;
GO
