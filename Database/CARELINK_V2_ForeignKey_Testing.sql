
/*
    CARELINK V2 - Foreign Key Testing
    Tests all 12 Foreign Key relationships.
    All test data is rolled back at the end.
    Note: Sequence values are not rolled back, so ID gaps are normal.
*/

USE [CARELINK_V2];
GO

SET NOCOUNT ON;

DECLARE
    @PatientUserId VARCHAR(20),
    @DoctorUserId VARCHAR(20),
    @PatientId VARCHAR(20),
    @DoctorId VARCHAR(20),
    @RequestId VARCHAR(20);

BEGIN TRY
    BEGIN TRANSACTION;

    -- Create valid test users
    INSERT INTO dbo.ApplicationUsers (Email, Role, PasswordHash)
    VALUES ('fk_patient_test@carelink.local', 'Test', 'TEST_HASH');

    SET @PatientUserId = (
        SELECT User_Id FROM dbo.ApplicationUsers
        WHERE Email = 'fk_patient_test@carelink.local'
    );

    INSERT INTO dbo.ApplicationUsers (Email, Role, PasswordHash)
    VALUES ('fk_doctor_test@carelink.local', 'Test', 'TEST_HASH');

    SET @DoctorUserId = (
        SELECT User_Id FROM dbo.ApplicationUsers
        WHERE Email = 'fk_doctor_test@carelink.local'
    );

    -- Create valid patient and doctor profiles
    INSERT INTO dbo.PatientProfile (User_Id, FullName)
    VALUES (@PatientUserId, 'FK Test Patient');

    SET @PatientId = (
        SELECT Patient_Id FROM dbo.PatientProfile
        WHERE User_Id = @PatientUserId
    );

    INSERT INTO dbo.DoctorProfile (User_Id, FullName, Specialty)
    VALUES (@DoctorUserId, 'FK Test Doctor', 'Testing');

    SET @DoctorId = (
        SELECT Doctor_Id FROM dbo.DoctorProfile
        WHERE User_Id = @DoctorUserId
    );

    -- Create a valid access request
    INSERT INTO dbo.AccessRequests
        (Doctor_Id, Patient_Id, Status, CreatedAt)
    VALUES
        (@DoctorId, @PatientId, 'Pending', SYSDATETIME());

    SET @RequestId = (
        SELECT Request_Id FROM dbo.AccessRequests
        WHERE Doctor_Id = @DoctorId
          AND Patient_Id = @PatientId
    );

    PRINT 'Starting Foreign Key Tests...';

    -- 1. PatientProfile.User_Id
    BEGIN TRY
        INSERT INTO dbo.PatientProfile (User_Id)
        VALUES ('INVALID-USER');
        PRINT 'TEST 1 FAILED';
    END TRY
    BEGIN CATCH
        PRINT 'TEST 1 PASSED: PatientProfile.User_Id';
    END CATCH;

    -- 2. DoctorProfile.User_Id
    BEGIN TRY
        INSERT INTO dbo.DoctorProfile (User_Id)
        VALUES ('INVALID-USER');
        PRINT 'TEST 2 FAILED';
    END TRY
    BEGIN CATCH
        PRINT 'TEST 2 PASSED: DoctorProfile.User_Id';
    END CATCH;

    -- 3. AccessRequests.Doctor_Id
    BEGIN TRY
        INSERT INTO dbo.AccessRequests
            (Doctor_Id, Patient_Id, Status, CreatedAt)
        VALUES
            ('INVALID-DOCTOR', @PatientId, 'Pending', SYSDATETIME());
        PRINT 'TEST 3 FAILED';
    END TRY
    BEGIN CATCH
        PRINT 'TEST 3 PASSED: AccessRequests.Doctor_Id';
    END CATCH;

    -- 4. AccessRequests.Patient_Id
    BEGIN TRY
        INSERT INTO dbo.AccessRequests
            (Doctor_Id, Patient_Id, Status, CreatedAt)
        VALUES
            (@DoctorId, 'INVALID-PATIENT', 'Pending', SYSDATETIME());
        PRINT 'TEST 4 FAILED';
    END TRY
    BEGIN CATCH
        PRINT 'TEST 4 PASSED: AccessRequests.Patient_Id';
    END CATCH;

    -- 5. AccessGrants.Request_Id
    BEGIN TRY
        INSERT INTO dbo.AccessGrants
            (Request_Id, CodeHash, ExpiredAt)
        VALUES
            ('INVALID-REQUEST', 'TEST_HASH',
             DATEADD(HOUR, 1, SYSDATETIME()));
        PRINT 'TEST 5 FAILED';
    END TRY
    BEGIN CATCH
        PRINT 'TEST 5 PASSED: AccessGrants.Request_Id';
    END CATCH;

    -- 6. AuditLogs.User_Id
    BEGIN TRY
        INSERT INTO dbo.AuditLogs (User_Id, Action, LoggedAt)
        VALUES ('INVALID-USER', 'FK Test', SYSDATETIME());
        PRINT 'TEST 6 FAILED';
    END TRY
    BEGIN CATCH
        PRINT 'TEST 6 PASSED: AuditLogs.User_Id';
    END CATCH;

    -- 7. MedicalConditions.Patient_Id
    BEGIN TRY
        INSERT INTO dbo.MedicalConditions (Patient_Id)
        VALUES ('INVALID-PATIENT');
        PRINT 'TEST 7 FAILED';
    END TRY
    BEGIN CATCH
        PRINT 'TEST 7 PASSED: MedicalConditions.Patient_Id';
    END CATCH;

    -- 8. Allergies.Patient_Id
    BEGIN TRY
        INSERT INTO dbo.Allergies (Patient_Id)
        VALUES ('INVALID-PATIENT');
        PRINT 'TEST 8 FAILED';
    END TRY
    BEGIN CATCH
        PRINT 'TEST 8 PASSED: Allergies.Patient_Id';
    END CATCH;

    -- 9. Surgeries.Patient_Id
    BEGIN TRY
        INSERT INTO dbo.Surgeries (Patient_Id)
        VALUES ('INVALID-PATIENT');
        PRINT 'TEST 9 FAILED';
    END TRY
    BEGIN CATCH
        PRINT 'TEST 9 PASSED: Surgeries.Patient_Id';
    END CATCH;

    -- 10. Medications.Patient_Id
    BEGIN TRY
        INSERT INTO dbo.Medications (Patient_Id)
        VALUES ('INVALID-PATIENT');
        PRINT 'TEST 10 FAILED';
    END TRY
    BEGIN CATCH
        PRINT 'TEST 10 PASSED: Medications.Patient_Id';
    END CATCH;

    -- 11. MedicalVisits.Patient_Id
    BEGIN TRY
        INSERT INTO dbo.MedicalVisits (Patient_Id, Doctor_Id)
        VALUES ('INVALID-PATIENT', @DoctorId);
        PRINT 'TEST 11 FAILED';
    END TRY
    BEGIN CATCH
        PRINT 'TEST 11 PASSED: MedicalVisits.Patient_Id';
    END CATCH;

    -- 12. MedicalVisits.Doctor_Id
    BEGIN TRY
        INSERT INTO dbo.MedicalVisits (Patient_Id, Doctor_Id)
        VALUES (@PatientId, 'INVALID-DOCTOR');
        PRINT 'TEST 12 FAILED';
    END TRY
    BEGIN CATCH
        PRINT 'TEST 12 PASSED: MedicalVisits.Doctor_Id';
    END CATCH;

    ROLLBACK TRANSACTION;

    PRINT 'Foreign Key Testing Completed.';
    PRINT 'All test records were rolled back.';

END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    PRINT 'ERROR:';
    PRINT ERROR_MESSAGE();
END CATCH;
GO
