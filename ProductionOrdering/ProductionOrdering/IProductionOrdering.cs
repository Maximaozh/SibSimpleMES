using ProductionData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionOrdering
{
    public abstract class IProductionOrdering {

        private string log = "";

        public string Log { get; set; }
        public Dictionary<string, string> Settings { get; set; }

        public abstract string GUID();
        public abstract string Name();
        public abstract List<Detail> Order(Production production);
    }
}
