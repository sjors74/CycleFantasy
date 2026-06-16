using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCycleApp.ViewModels
{
    public class EventsViewModel
    {
        private List<Models.Event> eventsCollection { get; set; } = new List<Models.Event>();

        public EventsViewModel()
        {
        }

        public List<Models.Event> EventsCollection
        { 
            get
            {
                return eventsCollection;
            }
            set
            { 
                eventsCollection = value; 
            }   
        }
    }
}
