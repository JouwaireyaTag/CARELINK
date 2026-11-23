
/*
    CARELINK V2 - Seed Data
    Inserts sample data into all 11 database tables.
    All information is fictional and for testing/demo purposes only.
    IDs are generated automatically using SQL Server Sequences.
    This script can be executed more than once without duplicating
    the same seed records.
*/

USE [CARELINK_V2];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE
        @PatientUserId VARCHAR(20),
        @DoctorUserId VARCHAR(20),
        @PatientId VARCHAR(20),
        @DoctorId VARCHAR(20),
        @RequestId VARCHAR(20);

   
    -- 1. APPLICATION USERS
   

    IF NOT EXISTS (
        SELECT 1 FROM dbo.ApplicationUsers
        WHERE Email = 'seed.patient@carelink.local'
    )
    BEGIN
        INSERT INTO dbo.ApplicationUsers
            (Email, Role, PasswordHash)
        VALUES
            ('seed.patient@carelink.local',
             'Patient',
             'DEMO_HASH_NOT_FOR_LOGIN');
    END;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.ApplicationUsers
        WHERE Email = 'seed.doctor@carelink.local'
    )
    BEGIN
        INSERT INTO dbo.ApplicationUsers
            (Email, Role, PasswordHash)
        VALUES
            ('seed.doctor@carelink.local',
             'Doctor',
             'DEMO_HASH_NOT_FOR_LOGIN');
    END;

    SELECT @PatientUserId = User_Id
    FROM dbo.ApplicationUsers
    WHERE Email = 'seed.patient@carelink.local';

    SELECT @DoctorUserId = User_Id
    FROM dbo.ApplicationUsers
    WHERE Email = 'seed.doctor@carelink.local';


    
    -- 2. PATIENT PROFILE
    

    IF NOT EXISTS (
        SELECT 1 FROM dbo.PatientProfile
        WHERE User_Id = @PatientUserId
    )
    BEGIN
        INSERT INTO dbo.PatientProfile
            (User_Id, FullName, DateOfBirth, NationalIdEncrypted)
        VALUES
            (@PatientUserId,
             'Sara Ahmed',
             '2000-05-15',
             'DEMO_ENCRYPTED_VALUE_NOT_REAL');
    END;

    SELECT @PatientId = Patient_Id
    FROM dbo.PatientProfile
    WHERE User_Id = @PatientUserId;


    
    -- 3. DOCTOR PROFILE
    

    IF NOT EXISTS (
        SELECT 1 FROM dbo.DoctorProfile
        WHERE User_Id = @DoctorUserId
    )
    BEGIN
        INSERT INTO dbo.DoctorProfile
            (User_Id, Specialty, FullName, LicenseNumber, Is_Approved)
        VALUES
            (@DoctorUserId,
             'Cardiology',
             'Dr. Omar Hassan',
             'CARELINK-DEMO-LIC-001',
             1);
    END;

    SELECT @DoctorId = Doctor_Id
    FROM dbo.DoctorProfile
    WHERE User_Id = @DoctorUserId;


    
    -- 4. ACCESS REQUESTS
    

    IF NOT EXISTS (
        SELECT 1 FROM dbo.AccessRequests
        WHERE Doctor_Id = @DoctorId
          AND Patient_Id = @PatientId
          AND Status = 'Approved'
    )
    BEGIN
        INSERT INTO dbo.AccessRequests
            (Doctor_Id, Patient_Id, Status, CreatedAt)
        VALUES
            (@DoctorId,
             @PatientId,
             'Approved',
             '2026-11-20T10:00:00');
    END;

    SELECT TOP (1) @RequestId = Request_Id
    FROM dbo.AccessRequests
    WHERE Doctor_Id = @DoctorId
      AND Patient_Id = @PatientId
      AND Status = 'Approved'
    ORDER BY CreatedAt DESC;


    
    -- 5. ACCESS GRANTS
    

    IF NOT EXISTS (
        SELECT 1 FROM dbo.AccessGrants
        WHERE Request_Id = @RequestId
    )
    BEGIN
        INSERT INTO dbo.AccessGrants
            (Request_Id, CodeHash, ExpiredAt, IsUsed, Attempts)
        VALUES
            (@RequestId,
             'DEMO_CODE_HASH_NOT_FOR_REAL_ACCESS',
             '2026-12-01T12:00:00',
             0,
             0);
    END;


    
    -- 6. AUDIT LOGS
   

    IF NOT EXISTS (
        SELECT 1 FROM dbo.AuditLogs
        WHERE User_Id = @PatientUserId
          AND Action = 'Approved doctor access request - DEMO'
    )
    BEGIN
        INSERT INTO dbo.AuditLogs
            (User_Id, Action, LoggedAt)
        VALUES
            (@PatientUserId,
             'Approved doctor access request - DEMO',
             '2026-11-20T10:05:00');
    END;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.AuditLogs
        WHERE User_Id = @DoctorUserId
          AND Action = 'Viewed patient medical history - DEMO'
    )
    BEGIN
        INSERT INTO dbo.AuditLogs
            (User_Id, Action, LoggedAt)
        VALUES
            (@DoctorUserId,
             'Viewed patient medical history - DEMO',
             '2026-11-20T10:10:00');
    END;


    
    -- 7. MEDICAL CONDITIONS
    

    IF NOT EXISTS (
        SELECT 1 FROM dbo.MedicalConditions
        WHERE Patient_Id = @PatientId
          AND ConditionName = 'Hypertension'
    )
    BEGIN
        INSERT INTO dbo.MedicalConditions
            (Patient_Id, ConditionName, DiagnosisDate, Notes_Condition)
        VALUES
            (@PatientId,
             'Hypertension',
             '2023-03-10',
             'Regular blood pressure monitoring');
    END;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.MedicalConditions
        WHERE Patient_Id = @PatientId
          AND ConditionName = 'Asthma'
    )
    BEGIN
        INSERT INTO dbo.MedicalConditions
            (Patient_Id, ConditionName, DiagnosisDate, Notes_Condition)
        VALUES
            (@PatientId,
             'Asthma',
             '2018-06-20',
             'Condition controlled with prescribed treatment');
    END;


    
    -- 8. ALLERGIES
    

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Allergies
        WHERE Patient_Id = @PatientId
          AND AllergyName = 'Penicillin'
    )
    BEGIN
        INSERT INTO dbo.Allergies
            (Patient_Id, AllergyName, Reaction, Severity, Notes_Allergies)
        VALUES
            (@PatientId,
             'Penicillin',
             'Skin rash',
             'Moderate',
             'Fictional sample allergy record');
    END;


    
    -- 9. SURGERIES
    

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Surgeries
        WHERE Patient_Id = @PatientId
          AND SurgeryName = 'Appendectomy'
    )
    BEGIN
        INSERT INTO dbo.Surgeries
            (Patient_Id, SurgeryName, SurgeryDate, Hospital, Notes_Surgeries)
        VALUES
            (@PatientId,
             'Appendectomy',
             '2019-08-15',
             'Demo General Hospital',
             'Fictional sample surgery record');
    END;


   
    -- 10. MEDICATIONS
   

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Medications
        WHERE Patient_Id = @PatientId
          AND MedicationName = 'Demo Medication A'
    )
    BEGIN
        INSERT INTO dbo.Medications
            (Patient_Id, MedicationName, Dosage, Frequency,
             StartDate, EndDate, Notes_Medications)
        VALUES
            (@PatientId,
             'Demo Medication A',
             '10 mg',
             'Once daily',
             '2026-01-01',
             '2026-12-31',
             'Fictional medication record for database testing');
    END;


    
    -- 11. MEDICAL VISITS
    

    IF NOT EXISTS (
        SELECT 1 FROM dbo.MedicalVisits
        WHERE Patient_Id = @PatientId
          AND Doctor_Id = @DoctorId
          AND Diagnosis = 'Demo follow-up visit'
    )
    BEGIN
        INSERT INTO dbo.MedicalVisits
            (Patient_Id, Doctor_Id, VisitDate,
             Diagnosis, Treatment, Notes_Visits)
        VALUES
            (@PatientId,
             @DoctorId,
             '2026-11-20T10:30:00',
             'Demo follow-up visit',
             'Routine follow-up',
             'Fictional sample medical visit');
    END;


    COMMIT TRANSACTION;

    PRINT '========================================';
    PRINT 'CARELINK V2 SEED DATA INSERTED SUCCESSFULLY';
    PRINT 'All 11 tables were checked.';
    PRINT '========================================';

    SELECT 'ApplicationUsers' AS TableName,
           COUNT(*) AS SeedRecords
    FROM dbo.ApplicationUsers
    WHERE Email IN (
        'seed.patient@carelink.local',
        'seed.doctor@carelink.local'
    )

    UNION ALL

    SELECT 'PatientProfile', COUNT(*)
    FROM dbo.PatientProfile
    WHERE User_Id = @PatientUserId

    UNION ALL

    SELECT 'DoctorProfile', COUNT(*)
    FROM dbo.DoctorProfile
    WHERE User_Id = @DoctorUserId

    UNION ALL

    SELECT 'AccessRequests', COUNT(*)
    FROM dbo.AccessRequests
    WHERE Request_Id = @RequestId

    UNION ALL

    SELECT 'AccessGrants', COUNT(*)
    FROM dbo.AccessGrants
    WHERE Request_Id = @RequestId

    UNION ALL

    SELECT 'AuditLogs', COUNT(*)
    FROM dbo.AuditLogs
    WHERE User_Id IN (@PatientUserId, @DoctorUserId)
      AND Action LIKE '%DEMO'

    UNION ALL

    SELECT 'MedicalConditions', COUNT(*)
    FROM dbo.MedicalConditions
    WHERE Patient_Id = @PatientId

    UNION ALL

    SELECT 'Allergies', COUNT(*)
    FROM dbo.Allergies
    WHERE Patient_Id = @PatientId

    UNION ALL

    SELECT 'Surgeries', COUNT(*)
    FROM dbo.Surgeries
    WHERE Patient_Id = @PatientId

    UNION ALL

    SELECT 'Medications', COUNT(*)
    FROM dbo.Medications
    WHERE Patient_Id = @PatientId

    UNION ALL

    SELECT 'MedicalVisits', COUNT(*)
    FROM dbo.MedicalVisits
    WHERE Patient_Id = @PatientId
      AND Doctor_Id = @DoctorId;

END TRY
BEGIN CATCH

    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    PRINT 'SEED DATA INSERTION FAILED';
    PRINT ERROR_MESSAGE();

END CATCH;
GO
