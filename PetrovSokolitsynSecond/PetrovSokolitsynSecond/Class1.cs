using ProductionData;
using ProductionOrdering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetrovSokolitsynSecond
{
    public class PertovSokolitsynOrder : IProductionOrdering
    {
        public override string GUID() => "{66DD387A-92FB-44CE-AD65-3E89AA54F269}";

        public override string Name() => "0011 Петров-Соколицин Вторая Очередь";

        private Dictionary<Detail, int> DetailAllMachinesTimed(Production production)
        {
            Log += $"[{DateTime.Now:HH:mm:ss}] Начало расчёта общего времени обработки\n";
            var result = new Dictionary<Detail, int>();

            foreach (Detail detail in production.Details)
            {
                int sum = 0;
                foreach (var line in production.Lines)
                {
                    var time = (int)line.ProcessingTime[detail];
                    sum += time;
                    Log += $"[Деталь {detail.ID}] Станок {line.Machine.ID}: {time} сек\n";
                }
                result.Add(detail, sum);
                Log += $"[Итог] Деталь {detail.ID} общее время: {sum} сек\n";
            }

            Log += $"[{DateTime.Now:HH:mm:ss}] Завершение расчёта общего времени\n";
            return result;
        }

        private Dictionary<Detail, int> wLastMachine(Dictionary<Detail, int> sum, Production production)
        {
            Log += $"[{DateTime.Now:HH:mm:ss}] Начало корректировки времени\n";
            var withoutLast = new Dictionary<Detail, int>();
            var lastMachine = production.Lines.Last().Machine;

            foreach (Detail detail in production.Details)
            {
                int lastTime = (int)production.Lines
                    .First(line => line.Machine == lastMachine)
                    .ProcessingTime[detail];

                withoutLast[detail] = sum[detail] - lastTime;
                Log += $"[Коррекция] Деталь {detail.ID}: " +
                      $"{sum[detail]} - {lastTime} = {withoutLast[detail]}\n";
            }

            Log += $"[{DateTime.Now:HH:mm:ss}] Завершение корректировки времени\n";
            return withoutLast;
        }

        public override List<Detail> Order(Production production)
        {
            var sw = Stopwatch.StartNew();
            Log = $"[{DateTime.Now:HH:mm:ss}] Запуск алгоритма 'Вторая Очередь'\n";

            try
            {
                Log += "=== Этап 1: Расчёт общего времени ===\n";
                var sumTimes = DetailAllMachinesTimed(production);

                Log += "\n=== Этап 2: Коррекция по последнему станку ===\n";
                var correctedTimes = wLastMachine(sumTimes, production);

                Log += "\n=== Этап 3: Сортировка по возрастанию ===\n";
                var result = correctedTimes
                    .OrderBy(pair => pair.Value)
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