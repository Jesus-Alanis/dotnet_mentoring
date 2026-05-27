-- Session A
SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;
BEGIN TRAN;
SELECT Balance FROM Accounts WHERE AccountID = 1;
-- In Session B, try to update the same row — it will be blocked
-- Session A keeps a shared lock until COMMIT/ROLLBACK

WAITFOR DELAY '00:00:30';

SELECT Balance FROM Accounts WHERE AccountID = 1; 

COMMIT TRAN;

-- Session B

BEGIN TRAN;

UPDATE Accounts SET Balance = 300 WHERE AccountID = 1;

COMMIT TRAN;