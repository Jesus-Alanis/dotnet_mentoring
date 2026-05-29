-- Session A
SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
BEGIN TRAN;

-- This insert statement will block until Session A completes.
SELECT Balance FROM Accounts WHERE AccountID = 1;
-- Session B updates the same row and commits
-- Session A SELECTs again — sees old value
-- Snapshot provides consistent snapshot view

WAITFOR DELAY '00:00:30';

-- Reads the snapshot version. Even though Session B successfully committed a change, Session A ignores it.
SELECT Balance FROM Accounts WHERE AccountID = 1; 

COMMIT TRAN;

-- Session B

BEGIN TRAN;

-- Updates the value to 999. This succeeds immediately without blocking.
UPDATE Accounts SET Balance = 999 WHERE AccountID = 1;

COMMIT TRAN;
