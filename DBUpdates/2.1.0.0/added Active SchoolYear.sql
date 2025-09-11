ALTER TABLE SchoolYears ADD Active bit NULL;
GO;
UPDATE SchoolYears set Active = 0;
GO;
update SchoolYears set Active = 1 where Cycle = 2015;
GO;
