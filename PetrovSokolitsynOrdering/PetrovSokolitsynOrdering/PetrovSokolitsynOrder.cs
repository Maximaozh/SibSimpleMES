using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductionData;
using ProductionOrdering;

namespace PetrovSokolitsynOrdering
{
    public class PetrovSokolitsynOrder : IProductionOrdering
    {
        public override string GUID()
        {
            return "{737914ED-F374-409A-B376-D4B5ECF93DED}";
        }
        public override string Name()
        {
            return "Петров-Соколицин вторая очередь";
        }
        
        private Dictionary<Detail,int> DetailAllMachinesTimed(Production production)
        {
            Dictionary<Detail, int> result = new Dictionary<Detail, int>();

            foreach (Detail detail in production.Details)
            {
                int sum = (from line in production.Lines
                           select (int)line.ProcessingTime[detail]).Sum();
                result.Add(detail, sum);
            }


            return result;
        }

        private Dictionary<Detail, int> wFirstMachine(Dictionary<Detail, int> sum, Production production)
        {

            Dictionary<Detail, int> withoutFirst = new Dictionary<Detail, int>();

            foreach (Detail detail in production.Details)
            {
                Machine firstMachine = production.Lines.First().Machine;
                int firstTime = (int)(from line in production.Lines
                                      where line.Machine == firstMachine
                                      select line.ProcessingTime[detail]).First();
                withoutFirst[detail] = sum[detail]-firstTime;
            }

            return withoutFirst;
        }

        private Dictionary<Detail, int> wLastMachine(Dictionary<Detail, int> sum, Production production)
        {

            Dictionary<Detail, int> withoutFirst = new Dictionary<Detail, int>(sum);

            foreach (Detail detail in production.Details)
            {
                Machine firstMachine = production.Lines.Last().Machine;
                int firstTime = (int)(from line in production.Lines
                                      where line.Machine == firstMachine
                                      select line.ProcessingTime[detail]).First();
                withoutFirst[detail] = sum[detail] - firstTime;
            }

            return withoutFirst;
        }

        private Dictionary<Detail, int> DifferenceBetwenSum(List<Detail> details, Dictionary<Detail,int> wFirst, Dictionary<Detail,int> wLast)
        {
            Dictionary<Detail,int> dif = new Dictionary<Detail,int>();

            foreach (Detail detail in details)
            {
                dif[detail] = wFirst[detail]-wLast[detail];
            }

            return dif;
        }

        private uint GetTimeProduction(Production production, List<Detail> order)
        {
            List<Machine> machineOrder = (from line in production.Lines
                                          orderby line.Machine.ID
                                         select line.Machine).ToList<Machine>();

           

            List<Line> lines = new List<Line>();

            Detail prevDetail = order.First();
            Line prevLine = production.Lines[0];
            lines.Add(new Line(prevLine.Machine));

            uint firstTime = production.Lines[0].ProcessingTime[prevDetail];
            lines[0].ProcessingTime[order.First()] = firstTime;

            foreach (Line line in production.Lines.Skip(1))
            {
                Line sumLine = new Line(line.Machine);
                sumLine.SetValue(prevDetail, line.ProcessingTime[prevDetail] + prevLine.ProcessingTime[prevDetail]);
                lines.Add(sumLine);
                prevLine = sumLine;
            }

            foreach (Detail detail in order.Skip(1))
            {
                uint value = lines[0].ProcessingTime[prevDetail] + production.Lines[0].ProcessingTime[detail];
                lines[0].SetValue(detail, value);
                prevDetail = detail;
            }

            prevDetail = order.First();
            prevLine = lines[0];

            //Console.WriteLine("###");
            foreach (Line line in lines.Skip(1))
            {
                prevDetail = order.First();
                foreach (Detail detail in order.Skip(1))
            {
                    uint linedTime = line.ProcessingTime[prevDetail]; ;
                    uint detailedTime = prevLine.ProcessingTime[detail];
                    uint timeFromOrigin = production.Lines.Where(x => x.Machine == line.Machine).First().ProcessingTime[detail];

                    uint value = linedTime > detailedTime ? linedTime + timeFromOrigin : detailedTime+ timeFromOrigin;
                    //Console.WriteLine(linedTime + "\t" + detailedTime +"\t" + timeFromOrigin + "\t= " + value);
                    line.SetValue(detail, value);
                    prevDetail = detail;
                }
                //Console.WriteLine("---");
                prevLine = line;
            }


            return lines.Last().ProcessingTime[order.Last()];
        }


        public override List<Detail> Order(Production production)
        {
            List<Detail> result = new List<Detail>();
            Dictionary<Detail, int> sum = DetailAllMachinesTimed(production);

            Dictionary<Detail, int> 
                wFirst = wFirstMachine(sum,production), 
                wLast = wLastMachine(sum, production),
                dif = DifferenceBetwenSum(production.Details.ToList(),wFirst,wLast);

            List<Detail> firstDescending = (from pair in wFirst
                                            orderby pair.Value descending
                                            select pair.Key).ToList<Detail>();

            List<Detail> secondAscending = (from pair in wLast
                                            orderby pair.Value 
                                            select pair.Key).ToList<Detail>();

            List<Detail> thirdDescending = (from pair in dif
                                            orderby pair.Value descending
                                            select pair.Key).ToList<Detail>();
            Console.WriteLine("{{{{" + GetTimeProduction(production, thirdDescending) + "}}}");

            return thirdDescending;
        }
    }
}
