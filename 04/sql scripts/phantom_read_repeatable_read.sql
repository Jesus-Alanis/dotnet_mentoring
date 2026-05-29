-- Session A
SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;
BEGIN TRAN;

--Returns 3
SELECT COUNT(*) FROM Orders;
-- Session B inserts a new row into Orders
-- Session A repeats the SELECT and sees a new row => Phantom Read

WAITFOR DELAY '00:00:30';

-- Returns 4 
SELECT COUNT(*) FROM Orders;

COMMIT TRAN;

-- Session B

BEGIN TRAN;

-- REPEATABLE READ does not prevent new rows from being added to the dataset.
INSERT INTO Orders (Customer) VALUES ('David');

COMMIT TRAN;