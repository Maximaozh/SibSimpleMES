using ProductionData;
using ProductionLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomProductionLoader
{
    // Предоставляет случайные данные для генерации
    public class RandomProductionDataProvider : IProductionDataProvider
    {
        public int MachinesCount
        {
            get
            {
                return Convert.ToInt32(Settings["MachinesCount_int"]);
            }
        }
        public int DetailsCount
        {
            get
            {
                return Convert.ToInt32(Settings["DetailsCount_int"]);
            }
        }

        public int MinProductionTime
        {
            get
            {
                return Convert.ToInt32(Settings["MinimumValue_int"]);
            }
        }
        public int MaxProductionTime
        {
            get
            {
                return Convert.ToInt32(Settings["MaximumValue_int"]);
            }
        }

        public int RandomSeed
        {
            get
            {
                return Convert.ToInt32(Settings["RandomSeed_int"]);
            }
        }

        public Random Rand { get; set; }

        public RandomProductionDataProvider()
        {
            Settings = new Dictionary<string, string>();
            Settings.Add("MinimumValue_int", "0");
            Settings.Add("MaximumValue_int", "100");
            Settings.Add("MachinesCount_int", "3");
            Settings.Add("DetailsCount_int", "3");
            Settings.Add("RandomSeed_int", "1");

            Rand = new Random(RandomSeed);
        }

        public override string GUID()
        {
            return "{EC8EE535-B3A7-449A-AC23-19DC23066488}";
        }

        public override string Name()
        {
            return "Случайная генерация 1.0.0";
        }

        public override Production LoadProduction()
        {
            Production prod = new Production();

            
            for (int i = 1; i <= DetailsCount; i++)
                prod.AddDetail(new Detail(i));

            for (int i = 1; i <= MachinesCount; i++)
                prod.AddMachine(new Machine(i));



            foreach (var machine in prod.Machines)
            {
                Line line = new Line(machine);
                prod.AddLine(line);

                foreach (var detail in prod.Details)
                {
                    line.SetValue(detail, (uint)Rand.Next(MinProductionTime,MaxProductionTime));
                }
            }
            return prod;
        }
    }
}
