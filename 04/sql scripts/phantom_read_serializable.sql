-- Session A
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
BEGIN TRAN;

--should return 4 rows (1 more from previous execution - phantom_read_repeatable_read.sql)
SELECT COUNT(*) FROM Orders;
-- Session B tries to insert — will be blocked
-- Prevents phantom reads

WAITFOR DELAY '00:00:30';

-- Returns the exact same 4 rows. No phantoms allowed.
SELECT COUNT(*) FROM Orders;

COMMIT TRAN;

-- Session B 

BEGIN TRAN;

-- This insert statement will block until Session A completes.
INSERT INTO Orders (Customer) VALUES ('Eve');

COMMIT TRAN;
