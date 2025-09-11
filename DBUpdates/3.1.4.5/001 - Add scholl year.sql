BEGIN
   IF NOT EXISTS (select * from SchoolYears where Cycle = 2018)
   BEGIN
       insert into SchoolYears (SchoolYears.Active, SchoolYears.Cycle, SchoolYears.Description) values (1, 2018, 2018)
   END
END