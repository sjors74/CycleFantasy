using Domain.Dto;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebCycle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly IResultsRepository resultsRepository;
        public ResultsController(IResultsRepository resultsRepository)
        {
            this.resultsRepository = resultsRepository;
        }

        [HttpGet("{id}/stage")]
        public async Task<IEnumerable<Result>> GetById(int id)
        {
            return await resultsRepository.GetResultsByEventId(id);
        }
        //Get results by event id => all points

        //Get results by poolDeelnemerId => individual score

        //Get results by eventId  => all points per pooldeelnemer
    }
}
