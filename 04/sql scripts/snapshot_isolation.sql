-- Session A
SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
BEGIN TRAN;
SELECT Balance FROM Accounts WHERE AccountID = 1;
-- Session B updates the same row and commits
-- Session A SELECTs again — sees old value
-- Snapshot provides consistent snapshot view

WAITFOR DELAY '00:00:30';

SELECT Balance FROM Accounts WHERE AccountID = 1; 

COMMIT TRAN;

-- Session B

BEGIN TRAN;

UPDATE Accounts SET Balance = 999 WHERE AccountID = 1;

COMMIT TRAN;
