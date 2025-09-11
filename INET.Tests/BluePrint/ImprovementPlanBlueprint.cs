using System;
using INET.Data;
using Plant.Core;

namespace INET.Tests.BluePrint
{
    public class ImprovementPlanBlueprint : IBlueprint
    {
        public void SetupPlant(BasePlant p)
        {
            p.DefinePropertiesOf(new SchoolYear() { Cycle = "2014", Description = "Ciclo Lectivo 2014" });
            p.DefinePropertiesOf(new StatusCriterion() { Description = "Plan - Segun Etapa" });
            p.DefinePropertiesOf(new Status() { Description = "En Ingreso", StatusCriterion = p.Build<StatusCriterion>() });
            p.DefinePropertiesOf(new Field() { Description = "Igualdad de oportunidades - JURISDICCIONAL" });
            p.DefinePropertiesOf(new UserProfile() { UserName = "admin"});

            p.DefinePropertiesOf(new ImprovementPlan()
            {
                CUE = "20458700",
                Summary = "Resumen del plan",
                Field = p.Build<Field>(),
                ReceptionDate = DateTime.Now,
                SchoolYear = p.Build<SchoolYear>(),
                Status = p.Build<Status>(),
                UserProfile = p.Build<UserProfile>()
            });
        }
    }
}
