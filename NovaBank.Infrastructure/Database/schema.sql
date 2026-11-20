IF OBJECT_ID(N'dbo.Accounts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Accounts
    (
        AccountNumber char(10) NOT NULL CONSTRAINT PK_Accounts PRIMARY KEY,
        CustomerName nvarchar(100) NOT NULL,
        City nvarchar(100) NOT NULL,
        PhoneNumber varchar(15) NOT NULL,
        PasswordHash varchar(200) NOT NULL,
        Balance decimal(18, 2) NOT NULL CONSTRAINT DF_Accounts_Balance DEFAULT (0),
        IsFrozen bit NOT NULL CONSTRAINT DF_Accounts_IsFrozen DEFAULT (0),
        Version rowversion NOT NULL,
        CONSTRAINT CK_Accounts_Balance_NonNegative CHECK (Balance >= 0)
    );

    CREATE UNIQUE INDEX UX_Accounts_CustomerName ON dbo.Accounts(CustomerName);
END;

IF OBJECT_ID(N'dbo.AccountTransactions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AccountTransactions
    (
        TransactionId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_AccountTransactions PRIMARY KEY,
        AccountNumber char(10) NOT NULL,
        OccurredAt datetimeoffset(7) NOT NULL,
        Description nvarchar(500) NOT NULL,
        CONSTRAINT FK_AccountTransactions_Accounts FOREIGN KEY (AccountNumber)
            REFERENCES dbo.Accounts(AccountNumber)
    );

    CREATE INDEX IX_AccountTransactions_AccountNumber_OccurredAt
        ON dbo.AccountTransactions(AccountNumber, OccurredAt);
END;

IF OBJECT_ID(N'dbo.BillPayments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BillPayments
    (
        TransactionId uniqueidentifier NOT NULL CONSTRAINT PK_BillPayments PRIMARY KEY,
        PaymentType int NOT NULL,
        PhoneNumber varchar(15) NOT NULL,
        Amount decimal(18, 2) NOT NULL,
        ProcessedAt datetimeoffset(7) NOT NULL,
        IsSuccessful bit NOT NULL,
        Message nvarchar(500) NOT NULL,
        AccountNumber char(10) NULL,
        CONSTRAINT CK_BillPayments_Amount_Positive CHECK (Amount > 0),
        CONSTRAINT CK_BillPayments_PaymentType CHECK (PaymentType IN (0, 1)),
        CONSTRAINT FK_BillPayments_Accounts FOREIGN KEY (AccountNumber)
            REFERENCES dbo.Accounts(AccountNumber)
    );

    CREATE INDEX IX_BillPayments_AccountNumber_ProcessedAt
        ON dbo.BillPayments(AccountNumber, ProcessedAt DESC);
END;

IF OBJECT_ID(N'dbo.IncomingTransfers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IncomingTransfers
    (
        TransferId uniqueidentifier NOT NULL CONSTRAINT PK_IncomingTransfers PRIMARY KEY,
        RecipientAccountNumber char(10) NOT NULL,
        SenderName nvarchar(100) NOT NULL,
        Amount decimal(18, 2) NOT NULL,
        ReceivedAt datetimeoffset(7) NOT NULL,
        CONSTRAINT CK_IncomingTransfers_Amount_Positive CHECK (Amount > 0),
        CONSTRAINT FK_IncomingTransfers_Accounts FOREIGN KEY (RecipientAccountNumber)
            REFERENCES dbo.Accounts(AccountNumber)
    );
END;
