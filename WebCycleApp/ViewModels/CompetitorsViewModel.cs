using WebCycleApp.Models;

namespace WebCycleApp.ViewModels
{
    public class CompetitorsViewModel
    {
        private List<Competitor> competitorsCollection { get; set; } = new List<Competitor>();

        public CompetitorsViewModel()
        {
        }

        public List<Competitor> CompetitorsCollection
        {
            get { return competitorsCollection; }
            set { competitorsCollection = value; }
        }
    }
}
