using INET.Data;
using INET.Tests.BluePrint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Plant.Core;

namespace INET.Tests
{
    [TestClass]
    public class ImprovementPlanTest : BaseTest
    {
        [TestMethod]
        public void CanCreateImprovementPlan()
        {
            var plant = new BasePlant().WithBlueprintsFromAssemblyOf<ImprovementPlanBlueprint>();
            var improvementPlan = plant.Create<ImprovementPlan>();

            //improvementPlan.Comments.Add(new Comment() { Date = System.DateTime.Today, Text = "Comentario de prueba", UserProfile = new UserProfile() { UserId = 1, UserName = "san" } });

            _context.ImprovementPlans.Add(improvementPlan);
            _context.SaveChanges();

            Assert.IsTrue(improvementPlan.Id > 0);
            Assert.Equals(improvementPlan.SchoolYear.Cycle, "2014");
            Assert.Equals(improvementPlan.Status.StatusCriterion.Description, "Plan - Segun Etapa");
        }
    }
}
