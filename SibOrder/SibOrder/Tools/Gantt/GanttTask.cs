using ProductionData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SibOrder
{
    internal class GanttTask
    {
        public Detail Detail { get; set; }
        public Machine Machine { get; set; }
        public uint StartTime { get; set; }
        public uint Duration { get; set; }
    }
}
