USE master ;
GO
CREATE DATABASE polyDB
GO
USE [polyDB]
GO
/****** Object:  Table [dbo].[collects]    Script Date: 09/24/2013 11:07:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[collects](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](50) NULL,
	[points_data] [nvarchar](max) NULL
) ON [PRIMARY]
GO
