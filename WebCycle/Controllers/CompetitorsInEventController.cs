using AutoMapper;
using Domain.Dto;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;


namespace WebCycle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitorsInEventController : ControllerBase
    {
        private readonly ICompetitorsInEventRepository competitorsInEventRepository;
        private readonly IMapper _mapper;

        public CompetitorsInEventController(ICompetitorsInEventRepository competitorsInEventRepository, IMapper mapper)
        {
            this.competitorsInEventRepository = competitorsInEventRepository;
            this._mapper = mapper;
        }

        [HttpGet("{id}", Name = "GetCompetitorsByEventId")]
        public async Task<IActionResult> GetById(int id)
        { 
            var c = await competitorsInEventRepository.GetCompetitors(id);
            var competitorsResponse = _mapper.Map<List<CompetitorDto>>(c);
            return Ok(competitorsResponse);
        }

        [HttpGet("{id}/{number}", Name = "GetRandomCompetitorsByEventId")]
        public async Task<IActionResult> GetRandomById(int id, int number)
        {
            var c = await competitorsInEventRepository.GetRandomNumberofCompetitors(id, number);
            var competitorsResponse = _mapper.Map<List<CompetitorDto>>(c);
            return Ok(competitorsResponse);
        }
    }
}
