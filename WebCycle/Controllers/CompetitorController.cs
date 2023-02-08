using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebCycle.Controllers
{
    [Route("api/competitors")]
    [ApiController]
    public class CompetitorController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public CompetitorController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        [HttpGet]
        public IEnumerable<Competitor> GetAllCompetitors()
        {
            var competitors = unitOfWork.Competitor.GetAll();
            return competitors;
        }
    }
}
