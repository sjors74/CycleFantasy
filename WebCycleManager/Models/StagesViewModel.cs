using Domain.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebCycleManager.Models
{
    public class StagesViewModel
    {
        public List<Stage> Stages { get; set; }
        public int CurrentSearchEventId { get; set; }

        public List<SelectListItem> Events { get; set; }
    }
}
