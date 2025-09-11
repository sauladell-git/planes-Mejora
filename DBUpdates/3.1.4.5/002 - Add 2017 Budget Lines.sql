INSERT INTO Budgets (
	SchoolYearId
	,ProvinceId
	,FieldId
	,LineId
	)
SELECT SchoolYears.Id
	,Provinces.Id
	,Lines.FieldId
	,Lines.Id
FROM SchoolYears
	,Provinces
	,Lines
WHERE SchoolYears.Cycle = 2018
	AND NOT EXISTS (
		SELECT *
		FROM Budgets AS b
		WHERE b.FieldId = Lines.FieldId
			AND b.LineId = Lines.Id
			AND b.ProvinceId = Provinces.Id
			AND b.SchoolYearId = SchoolYears.Id
		)
