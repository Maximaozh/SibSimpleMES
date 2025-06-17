using ProductionData;
using ProductionOrdering;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetrovSokolitsynCompared
{
    public class PetrovSokolitsynOrder : IProductionOrdering
    {
        public override string GUID()
        {
            return "{F5B1E615-51A9-4AEB-B2D3-99B0DFB5C5D8}";
        }

        public override string Name()
        {
            return "0013 Петров-Соколицин Сравнительный";
        }

        public override List<Detail> Order(Production production)
        {
            List<Detail> first = (new PetrovSokolitsynFirst.PertovSocolitsinOrder()).Order(production);
            List<Detail> second = (new PetrovSokolitsynFirst.PertovSocolitsinOrder()).Order(production);
            List<Detail> third = (new PetrovSokolitsynFirst.PertovSocolitsinOrder()).Order(production);

            uint firstTime  = (new ManufactoryTime.Tools()).CalculateTime(production, first);
            uint secondTime = (new ManufactoryTime.Tools()).CalculateTime(production, second);
            uint lastTime   = (new ManufactoryTime.Tools()).CalculateTime(production, third);

            List<(List<Detail>, uint)> times = new List<(List<Detail>, uint)> ();

            times.Add((first, firstTime));
            times.Add((second, secondTime));
            times.Add((third, lastTime));

            List<Detail> result = (from listDetail in times
                                  orderby listDetail.Item2
                                  select listDetail.Item1).First();

            return result;
        }
    }
}
