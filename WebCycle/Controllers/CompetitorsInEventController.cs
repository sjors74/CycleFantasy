using AutoMapper;
using CycleManager.Services.Interfaces;
using Domain.Dto;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace WebCycle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitorsInEventController : ControllerBase
    {
        private readonly ICompetitorsInEventRepository competitorsInEventRepository;
        private readonly ICompetitorInEventService _competitorInEventService;
        private readonly IMapper _mapper;

        public CompetitorsInEventController(ICompetitorsInEventRepository competitorsInEventRepository, ICompetitorInEventService competitorInEventService, IMapper mapper)
        {
            this.competitorsInEventRepository = competitorsInEventRepository;
            _competitorInEventService = competitorInEventService;
            this._mapper = mapper;
        }

        [HttpGet("{id}", Name = "GetCompetitorsByEventId")]
        public async Task<IActionResult> GetById(int id)
        {
            var c = await _competitorInEventService.GetCompetitorById(id);
            var competitorsResponse = _mapper.Map<CompetitorDto>(c);
            return Ok(competitorsResponse);
        }

        /// <summary>
        /// TODO: go to service and then to repository!
        /// </summary>
        /// <param name="id"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        [HttpGet("{id}/{number}", Name = "GetRandomCompetitorsByEventId")]
        public async Task<IActionResult> GetRandomById(int id, int number)
        {
            var c = await competitorsInEventRepository.GetRandomNumberofCompetitors(id, number);
            var competitorsResponse = _mapper.Map<List<CompetitorDto>>(c);
            return Ok(competitorsResponse);
        }
    }
}
