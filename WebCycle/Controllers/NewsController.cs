using AutoMapper;
using CycleManager.Domain.Dto;
using CycleManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebCycleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;
        private readonly IMapper _mapper;
        public NewsController(INewsService newsService, IMapper mapper)
        {
            _newsService = newsService;
            _mapper = mapper;
        }
        [HttpGet("latest")]
        public async Task<IEnumerable<NewsItemDto>> GetAllNews()
        {
            var newsItems = await _newsService.GetAllActiveNewsItems();
            var newsItemsDto = _mapper.Map<List<NewsItemDto>>(newsItems);
            return  newsItemsDto;
        }
    }
}
