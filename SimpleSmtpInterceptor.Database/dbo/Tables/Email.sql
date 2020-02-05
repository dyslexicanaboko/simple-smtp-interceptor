CREATE TABLE [dbo].[Email] (
	[EmailId]      bigint         IDENTITY (1, 1) NOT NULL,
	[From]         varchar (1000) NOT NULL,
	[To]           varchar (1000) NOT NULL,
	[Subject]      varchar (78)   NOT NULL,
	[Message]      nvarchar (MAX) NULL,
	[HeaderJson]   nvarchar (MAX) NULL CONSTRAINT [CHK_dbo.Email_HeaderJson] CHECK (ISJSON(HeaderJson) = 1),
	[AttachmentCount] int NOT NULL CONSTRAINT [DF_dbo.Email_AttachmentCount] DEFAULT (0),
	[AttachmentArchive] varbinary(MAX) NULL,
	[CreatedOnUtc] datetime2 (7)  CONSTRAINT [DF_dbo.Email_CreatedOnUtc] DEFAULT (getutcdate()) NOT NULL,
	CONSTRAINT [PK_dbo.Email_EmailId] PRIMARY KEY CLUSTERED ([EmailId] ASC)
);

