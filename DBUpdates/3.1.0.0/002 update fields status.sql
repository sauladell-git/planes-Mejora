begin transaction;

SET IDENTITY_INSERT StatusCriterions ON;
insert into StatusCriterions (Id, Description) values (7,'Ejes');
SET IDENTITY_INSERT StatusCriterions OFF;

SET IDENTITY_INSERT [Status] ON;
insert into [Status] (Id, Description, StatusCriterionId) values (34,'Archivado',7)
, (35,'No vigente',7)
, (36,'Vigente',7);
SET IDENTITY_INSERT [Status] OFF;

alter table [Fields] ADD StatusId int NULL;

update Fields set StatusId = 36;

alter table fields alter column StatusId int NOT NULL;

Update Fields set StatusId = 35 where Id < 11;

-- Creating foreign key on [StatusId] in table 'Fields'
ALTER TABLE [dbo].[Fields]
ADD CONSTRAINT [FK_FieldStatus]
    FOREIGN KEY ([StatusId])
    REFERENCES [dbo].[Status]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_FieldStatus'
CREATE INDEX [IX_FK_FieldStatus]
ON [dbo].[Fields]
    ([StatusId]);

commit;


