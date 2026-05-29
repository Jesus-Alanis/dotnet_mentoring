-- Session A
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
BEGIN TRAN;
SELECT Balance FROM Accounts WHERE AccountID = 1;
-- Open another session and run update, then return and re-run the select here
-- Expected: Second SELECT may return a different value

WAITFOR DELAY '00:00:30';

-- The value changed during the same transaction (Non-repeatable Read)
SELECT Balance FROM Accounts WHERE AccountID = 1; 

COMMIT TRAN;

-- Session B

BEGIN TRAN;
UPDATE Accounts SET Balance = 200 WHERE AccountID = 1;
COMMIT TRAN;