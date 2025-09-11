set identity_insert Templates ON
go

INSERT INTO [dbo].[Templates]
           ([TemplateTypeId]
           ,[Name]
           ,[Active]
           ,[Content]
           ,[marginLeft]
           ,[marginRight]
           ,[marginTop]
           ,[marginBottom]
           ,[SignatureImagePath]
           ,[Header]
		   ,Id)
     VALUES
           (4
           ,'Disposiciones'
           ,1
           ,'Contenido General'
           ,0
           ,0
           ,0
           ,0
           ,NULL
           ,NULL
		   ,99)
GO
set identity_insert Templates OFF
GO

insert into TemplateField (Templates_Id, Fields_Id) select 99, Fields.Id from Fields where Fields.StatusId = 36
GO