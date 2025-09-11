BEGIN
   IF NOT EXISTS (select * from SchoolYears where Cycle = 2017)
   BEGIN
       insert into SchoolYears (SchoolYears.Active, SchoolYears.Cycle, SchoolYears.Description) values (1, 2017, 2017)
   END
END