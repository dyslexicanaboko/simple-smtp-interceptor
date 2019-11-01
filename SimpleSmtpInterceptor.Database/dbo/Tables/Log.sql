CREATE TABLE [dbo].[Log]
(
[LogId] [int] NOT NULL IDENTITY(1, 1),
[MachineName] [nvarchar] (200) NULL,
[CreatedOnUtc] [datetime2] (7) CONSTRAINT [DF_dbo.Log_CreatedOnUtc] DEFAULT (getutcdate()) NOT NULL,
[Level] [nvarchar] (5) NOT NULL,
[Message] [nvarchar] (max) NOT NULL,
[Logger] [nvarchar] (300) NULL,
[Properties] [nvarchar] (max) NULL,
[Callsite] [nvarchar] (300) NULL,
[Exception] [nvarchar] (max) NULL
)
GO
ALTER TABLE [dbo].[Log] ADD CONSTRAINT [PK_dbo.Log_LogId] PRIMARY KEY CLUSTERED  ([LogId])
GO
;

