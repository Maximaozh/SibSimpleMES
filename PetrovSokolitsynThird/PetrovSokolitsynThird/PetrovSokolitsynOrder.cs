using ProductionData;
using ProductionOrdering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProductionData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PetrovSokolitsynThird
{
    public class PetrovSokolitsynOrder : IProductionOrdering
    {
        public override string GUID() => "{7FFB2113-BF58-4E00-B9DC-8B3B1056F5CE}";

        public override string Name() => "0012 Петров-Соколицин Третья Очередь";

        private Dictionary<Detail, int> DifferenceBetwenSum(List<Detail> details, Dictionary<Detail, int> wFirst, Dictionary<Detail, int> wLast)
        {
            Log += $"[{DateTime.Now:HH:mm:ss}] Начало расчёта разницы\n";
            var dif = new Dictionary<Detail, int>();

            foreach (Detail detail in details)
            {
                dif[detail] = wFirst[detail] - wLast[detail];
                Log += $"[Разница] Деталь {detail.ID}: {wFirst[detail]} - {wLast[detail]} = {dif[detail]}\n";
            }

            Log += $"[{DateTime.Now:HH:mm:ss}] Завершение расчёта разницы\n";
            return dif;
        }

        private Dictionary<Detail, int> DetailAllMachinesTimed(Production production)
        {
            Log += $"[{DateTime.Now:HH:mm:ss}] Расчёт общего времени обработки\n";
            var result = new Dictionary<Detail, int>();

            foreach (Detail detail in production.Details)
            {
                int sum = production.Lines.Sum(line =>
                {
                    var time = (int)line.ProcessingTime[detail];
                    Log += $"[Деталь {detail.ID}] Станок {line.Machine.ID}: {time} сек\n";
                    return time;
                });

                result.Add(detail, sum);
                Log += $"[Итог] Деталь {detail.ID} суммарное время: {sum} сек\n";
            }

            Log += $"[{DateTime.Now:HH:mm:ss}] Завершение расчёта общего времени\n";
            return result;
        }

        private Dictionary<Detail, int> wFirstMachine(Dictionary<Detail, int> sum, Production production)
        {
            Log += $"[{DateTime.Now:HH:mm:ss}] Коррекция по первому станку\n";
            var withoutFirst = new Dictionary<Detail, int>();
            var firstMachine = production.Lines.First().Machine;

            foreach (Detail detail in production.Details)
            {
                int firstTime = (int)production.Lines
                    .First(line => line.Machine == firstMachine)
                    .ProcessingTime[detail];

                withoutFirst[detail] = sum[detail] - firstTime;
                Log += $"[Коррекция] Деталь {detail.ID}: {sum[detail]} - {firstTime} = {withoutFirst[detail]}\n";
            }

            Log += $"[{DateTime.Now:HH:mm:ss}] Завершение коррекции по первому станку\n";
            return withoutFirst;
        }

        private Dictionary<Detail, int> wLastMachine(Dictionary<Detail, int> sum, Production production)
        {
            Log += $"[{DateTime.Now:HH:mm:ss}] Коррекция по последнему станку\n";
            var withoutLast = new Dictionary<Detail, int>();
            var lastMachine = production.Lines.Last().Machine;

            foreach (Detail detail in production.Details)
            {
                int lastTime = (int)production.Lines
                    .First(line => line.Machine == lastMachine)
                    .ProcessingTime[detail];

                withoutLast[detail] = sum[detail] - lastTime;
                Log += $"[Коррекция] Деталь {detail.ID}: {sum[detail]} - {lastTime} = {withoutLast[detail]}\n";
            }

            Log += $"[{DateTime.Now:HH:mm:ss}] Завершение коррекции по последнему станку\n";
            return withoutLast;
        }

        public override List<Detail> Order(Production production)
        {
            var sw = Stopwatch.StartNew();
            Log = $"[{DateTime.Now:HH:mm:ss}] Запуск алгоритма 'Третья Очередь'\n";

            try
            {
                Log += "=== Этап 1: Общее время обработки ===\n";
                var sum = DetailAllMachinesTimed(production);

                Log += "\n=== Этап 2: Коррекция по первому станку ===\n";
                var wFirst = wFirstMachine(sum, production);

                Log += "\n=== Этап 3: Коррекция по последнему станку ===\n";
                var wLast = wLastMachine(sum, production);

                Log += "\n=== Этап 4: Расчёт разницы ===\n";
                var dif = DifferenceBetwenSum(production.Details.ToList(), wFirst, wLast);

                Log += "\n=== Этап 5: Финальная сортировка ===\n";
                var result = dif.OrderByDescending(pair => pair.Value)
                              .Select(pair =>
                              {
                                  Log += $"[Сортировка] Деталь {pair.Key.ID} -> {pair.Value}\n";
                                  return pair.Key;
                              })
                              .ToList();

                return result;
            }
            catch (Exception ex)
            {
                Log += $"\n[КРИТИЧЕСКАЯ ОШИБКА] {ex.Message}\n";
                return new List<Detail>();
            }
            finally
            {
                sw.Stop();
                Log = $"[Общее время выполнения] {sw.Elapsed.TotalSeconds:0.000} сек\n" + Log;
            }
        }
    }
}