/****** Object:  Table [dbo].[UserProfile_InstitutionLevels]    Script Date: 20/10/2016 23:05:56 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].UserProfile_InstitutionLevels(
	[UserId] [int] NOT NULL,
	[InstitutionLevelId] [int] NOT NULL,
 CONSTRAINT [PK_UserProfile_InstitutionLevels] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[InstitutionLevelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[UserProfile_InstitutionLevels]  WITH CHECK ADD  CONSTRAINT [FK_UserProfile_InstitutionLevels_InstitutionLevels] FOREIGN KEY([InstitutionLevelId])
REFERENCES [dbo].[InstitutionLevels] ([Id])
GO

ALTER TABLE [dbo].[UserProfile_InstitutionLevels] CHECK CONSTRAINT [FK_UserProfile_InstitutionLevels_InstitutionLevels]
GO

ALTER TABLE [dbo].[UserProfile_InstitutionLevels]  WITH CHECK ADD  CONSTRAINT [FK_UserProfile_InstitutionLevels_UserProfile] FOREIGN KEY([UserId])
REFERENCES [dbo].[UserProfile] ([UserId])
GO

ALTER TABLE [dbo].[UserProfile_InstitutionLevels] CHECK CONSTRAINT [FK_UserProfile_InstitutionLevels_UserProfile]
GO


