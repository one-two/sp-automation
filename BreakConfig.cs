using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spauto
{
    public class BreakConfig
    {
        public bool Active { get; set; }
        public int Probability { get; set; }
        public int Minutes { get; set; }
        public BreakConfig(bool active, int prob, int mins)
            {
                Active = active;
                Probability = prob;
                Minutes = mins;
            }
        }
}
