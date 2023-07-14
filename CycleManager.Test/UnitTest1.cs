using CycleManager.Services.Interfaces;
using Domain.Models;
using Moq;
using WebCycleManager.Controllers;
using NUnit.Framework.Interfaces;

namespace CycleManager.Test
{
    public class Tests
    {
        private CompetitorsController controller;
        private Mock<ICompetitorService> competitorServiceMock;
        private List<Competitor> items;

        [SetUp]
        public void Setup()
        {
            competitorServiceMock = new Mock<ICompetitorService>();

            // 
            var competitorMock = new Mock<Competitor>();
            competitorMock.Setup(item => item.CompetitorId).Returns(2);

            items = new List<Competitor>()
          {
              competitorMock.Object
          };


            competitorServiceMock.Setup(c => c.GetAllCompetitors()).Returns(items.AsQueryable());
        }

        [Test]
        public void ShouldReturnCompetitor()
        {

            //act
            var result = controller.Details(2);
            
            Assert.Pass();
        }
    }
}