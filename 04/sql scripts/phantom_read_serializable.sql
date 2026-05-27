-- Session A
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
BEGIN TRAN;
SELECT COUNT(*) FROM Orders;
-- Session B tries to insert — will be blocked
-- Prevents phantom reads

WAITFOR DELAY '00:00:30';

SELECT COUNT(*) FROM Orders;

COMMIT TRAN;

-- Session B 

BEGIN TRAN;

INSERT INTO Orders (Customer) VALUES ('Eve');

COMMIT TRAN;
