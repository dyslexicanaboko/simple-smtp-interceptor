CREATE TABLE [dbo].[Email] (
    [EmailId]      BIGINT         IDENTITY (1, 1) NOT NULL,
    [From]         VARCHAR (1000) NOT NULL,
    [To]           VARCHAR (1000) NOT NULL,
    [Subject]      VARCHAR (78)   NOT NULL,
    [Message]      NVARCHAR (MAX) NOT NULL,
    [CreatedOnUtc] DATETIME2 (7)  DEFAULT [DF_dbo.Email_CreateOnUtc] (getutcdate()) NOT NULL,
    CONSTRAINT [PK_dbo.Email_EmailId] PRIMARY KEY CLUSTERED ([EmailId] ASC)
);

