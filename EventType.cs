using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gamon.LaunchPlanner
{
    class EventType
    {
        public string localCaption;
        public Event.Periodicity periodicity;

        public EventType(Event.Periodicity Periodicity, string LocalCaption)
        {
            this.periodicity = Periodicity;
            this.localCaption = LocalCaption;
        }

        public override string ToString()
        {
            return localCaption; 
        }
    }
}
