
/*
    CARELINK V2 - UNIQUE and CHECK Constraints Testing
    Tests all UNIQUE constraints, the filtered unique index,
    and both CHECK constraints.
    All test data is rolled back at the end.
*/

USE [CARELINK_V2];
GO

SET NOCOUNT ON;

DECLARE
    @User1 VARCHAR(20),
    @User2 VARCHAR(20),
    @User3 VARCHAR(20),
    @User4 VARCHAR(20),
    @PatientId VARCHAR(20),
    @Doctor1 VARCHAR(20),
    @Doctor2 VARCHAR(20),
    @Request1 VARCHAR(20),
    @Request2 VARCHAR(20);

BEGIN TRY
    BEGIN TRANSACTION;

    -- Create test users
    INSERT INTO dbo.ApplicationUsers (Email, Role, PasswordHash)
    VALUES ('unique_test_1@carelink.local', 'Test', 'TEST_HASH');

    SET @User1 = (
        SELECT User_Id FROM dbo.ApplicationUsers
        WHERE Email = 'unique_test_1@carelink.local'
    );

    INSERT INTO dbo.ApplicationUsers (Email, Role, PasswordHash)
    VALUES ('unique_test_2@carelink.local', 'Test', 'TEST_HASH');

    SET @User2 = (
        SELECT User_Id FROM dbo.ApplicationUsers
        WHERE Email = 'unique_test_2@carelink.local'
    );

    INSERT INTO dbo.ApplicationUsers (Email, Role, PasswordHash)
    VALUES ('unique_test_3@carelink.local', 'Test', 'TEST_HASH');

    SET @User3 = (
        SELECT User_Id FROM dbo.ApplicationUsers
        WHERE Email = 'unique_test_3@carelink.local'
    );

    INSERT INTO dbo.ApplicationUsers (Email, Role, PasswordHash)
    VALUES ('unique_test_4@carelink.local', 'Test', 'TEST_HASH');

    SET @User4 = (
        SELECT User_Id FROM dbo.ApplicationUsers
        WHERE Email = 'unique_test_4@carelink.local'
    );

    -- Create patient
    INSERT INTO dbo.PatientProfile (User_Id, FullName)
    VALUES (@User1, 'Unique Test Patient');

    SET @PatientId = (
        SELECT Patient_Id FROM dbo.PatientProfile
        WHERE User_Id = @User1
    );

    -- Create two doctors with different license numbers
    INSERT INTO dbo.DoctorProfile
        (User_Id, FullName, LicenseNumber)
    VALUES
        (@User2, 'Unique Test Doctor 1', 'TEST-LICENSE-001');

    SET @Doctor1 = (
        SELECT Doctor_Id FROM dbo.DoctorProfile
        WHERE User_Id = @User2
    );

    INSERT INTO dbo.DoctorProfile
        (User_Id, FullName, LicenseNumber)
    VALUES
        (@User3, 'Unique Test Doctor 2', 'TEST-LICENSE-002');

    SET @Doctor2 = (
        SELECT Doctor_Id FROM dbo.DoctorProfile
        WHERE User_Id = @User3
    );

    -- Create two access requests
    INSERT INTO dbo.AccessRequests
        (Doctor_Id, Patient_Id, Status, CreatedAt)
    VALUES
        (@Doctor1, @PatientId, 'Pending', SYSDATETIME());

    SET @Request1 = (
        SELECT Request_Id FROM dbo.AccessRequests
        WHERE Doctor_Id = @Doctor1
          AND Patient_Id = @PatientId
    );

    INSERT INTO dbo.AccessRequests
        (Doctor_Id, Patient_Id, Status, CreatedAt)
    VALUES
        (@Doctor2, @PatientId, 'Pending', SYSDATETIME());

    SET @Request2 = (
        SELECT Request_Id FROM dbo.AccessRequests
        WHERE Doctor_Id = @Doctor2
          AND Patient_Id = @PatientId
    );

    PRINT 'Starting UNIQUE and CHECK Tests...';

    -- 1. ApplicationUsers.Email
    BEGIN TRY
        INSERT INTO dbo.ApplicationUsers (Email, Role, PasswordHash)
        VALUES ('unique_test_1@carelink.local', 'Test', 'TEST_HASH');
        PRINT 'TEST 1 FAILED: Duplicate Email accepted';
    END TRY
    BEGIN CATCH
        PRINT 'TEST 1 PASSED: Duplicate Email rejected';
    END CATCH;

    -- 2. PatientProfile.User_Id
    BEGIN TRY
        INSERT INTO dbo.PatientProfile (User_Id, FullName)
        VALUES (@User1, 'Duplicate Patient');
        PRINT 'TEST 2 FAILED: Duplicate Patient User_Id accepted';
    END TRY
    BEGIN CATCH
        PRINT 'TEST 2 PASSED: Duplicate Patient User_Id rejected';
    END CATCH;

    -- 3. DoctorProfile.User_Id
    BEGIN TRY
        INSERT INTO dbo.DoctorProfile (User_Id, FullName)
        VALUES (@User2, 'Duplicate Doctor');
        PRINT 'TEST 3 FAILED: Duplicate Doctor User_Id accepted';
    END TRY
    BEGIN CATCH
        PRINT 'TEST 3 PASSED: Duplicate Doctor User_Id rejected';
    END CATCH;

    -- 4. AccessGrants.Request_Id
    BEGIN TRY
        INSERT INTO dbo.AccessGrants (Request_Id, CodeHash, ExpiredAt)
        VALUES (@Request1, 'TEST_HASH_1',
                DATEADD(HOUR, 1, SYSDATETIME()));

        INSERT INTO dbo.AccessGrants (Request_Id, CodeHash, ExpiredAt)
        VALUES (@Request1, 'TEST_HASH_2',
                DATEADD(HOUR, 1, SYSDATETIME()));

        PRINT 'TEST 4 FAILED: Duplicate Request_Id accepted';
    END TRY
    BEGIN CATCH
        PRINT 'TEST 4 PASSED: Duplicate Request_Id rejected';
    END CATCH;

    -- 5. DoctorProfile.LicenseNumber unique filtered index
    BEGIN TRY
        INSERT INTO dbo.DoctorProfile
            (User_Id, FullName, LicenseNumber)
        VALUES
            (@User4, 'Duplicate License Doctor',
             'TEST-LICENSE-001');

        PRINT 'TEST 5 FAILED: Duplicate LicenseNumber accepted';
    END TRY
    BEGIN CATCH
        PRINT 'TEST 5 PASSED: Duplicate LicenseNumber rejected';
    END CATCH;

    -- 6. AccessRequests.Status CHECK constraint
    BEGIN TRY
        INSERT INTO dbo.AccessRequests
            (Doctor_Id, Patient_Id, Status, CreatedAt)
        VALUES
            (@Doctor1, @PatientId, 'Waiting', SYSDATETIME());

        PRINT 'TEST 6 FAILED: Invalid Status accepted';
    END TRY
    BEGIN CATCH
        PRINT 'TEST 6 PASSED: Invalid Status rejected';
    END CATCH;

    -- 7. AccessGrants.Attempts CHECK constraint
    BEGIN TRY
        INSERT INTO dbo.AccessGrants
            (Request_Id, CodeHash, ExpiredAt, Attempts)
        VALUES
            (@Request2, 'TEST_HASH_NEGATIVE',
             DATEADD(HOUR, 1, SYSDATETIME()), -1);

        PRINT 'TEST 7 FAILED: Negative Attempts accepted';
    END TRY
    BEGIN CATCH
        PRINT 'TEST 7 PASSED: Negative Attempts rejected';
    END CATCH;

    ROLLBACK TRANSACTION;

    PRINT 'UNIQUE and CHECK Testing Completed.';
    PRINT 'All test records were rolled back.';

END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    PRINT 'ERROR:';
    PRINT ERROR_MESSAGE();
END CATCH;
GO
