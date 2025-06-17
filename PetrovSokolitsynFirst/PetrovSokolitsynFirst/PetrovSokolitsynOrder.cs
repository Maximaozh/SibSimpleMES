using ProductionData;
using ProductionOrdering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProductionData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PetrovSokolitsynFirst
{
    public class PertovSocolitsinOrder : IProductionOrdering
    {
        public override string GUID() => "{4D6726BF-1F25-497D-BA14-92D5AC55667B}";

        public override string Name() => "0010 Петров-Соколицин Первая очередь";

        private Dictionary<Detail, int> DetailAllMachinesTimed(Production production)
        {
            Log += $"[{DateTime.Now:HH:mm:ss}] Начало расчёта общего времени\n";
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
                Log += $"[Итог] Деталь {detail.ID} общее время: {sum} сек\n";
            }

            Log += $"[{DateTime.Now:HH:mm:ss}] Завершение расчёта общего времени\n";
            return result;
        }

        private Dictionary<Detail, int> wFirstMachine(Dictionary<Detail, int> sum, Production production)
        {
            Log += $"[{DateTime.Now:HH:mm:ss}] Начало корректировки времени\n";
            var withoutFirst = new Dictionary<Detail, int>();
            var firstMachine = production.Lines.First().Machine;

            foreach (Detail detail in production.Details)
            {
                int firstTime = (int)production.Lines
                    .First(line => line.Machine == firstMachine)
                    .ProcessingTime[detail];

                withoutFirst[detail] = sum[detail] - firstTime;
                Log += $"[Коррекция] Деталь {detail.ID}: " +
                      $"{sum[detail]} - {firstTime} = {withoutFirst[detail]}\n";
            }

            Log += $"[{DateTime.Now:HH:mm:ss}] Завершение корректировки времени\n";
            return withoutFirst;
        }

        public override List<Detail> Order(Production production)
        {
            var sw = Stopwatch.StartNew();
            Log = $"[{DateTime.Now:HH:mm:ss}] Запуск алгоритма\n";

            try
            {
                Log += "=== Этап 1: Расчёт суммарного времени ===\n";
                var sum = DetailAllMachinesTimed(production);

                Log += "\n=== Этап 2: Коррекция времени ===\n";
                var wFirst = wFirstMachine(sum, production);

                Log += "\n=== Этап 3: Финальная сортировка ===\n";
                var result = wFirst.OrderByDescending(pair => pair.Value)
                                 .Select(pair => pair.Key)
                                 .ToList();

                Log += $"\n[Итог] Упорядочено {result.Count} деталей: " +
                      $"{string.Join(", ", result.Select(d => d.ID))}\n";

                return result;
            }
            catch (Exception ex)
            {
                Log += $"\n[ОШИБКА] {ex.Message}\n";
                return new List<Detail>();
            }
            finally
            {
                sw.Stop();
                Log = $"[Общее время] {sw.Elapsed.TotalSeconds:0.000} сек\n" + Log;
            }
        }
    }
}