using ProductionData;
using ProductionOrdering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JohnsonGeneralizationSecond
{
    public class JohnsonOrder : IProductionOrdering
    {
        public override string GUID() => "{37F69B9C-4221-4517-9077-A6A28D365B77}";

        public override string Name() => "0001 Джонсон Второе обобщение";

        public override List<Detail> Order(Production production)
        {
            var sw = Stopwatch.StartNew();
            Log = ""; // Инициализация лога

            try
            {
                Log += $"[Старт] {DateTime.Now:HH:mm:ss}\n";

                if (production == null)
                {
                    Log += "[Ошибка] Отсутствуют данные производства\n";
                    return new List<Detail>();
                }

                Log += $"[Данные] Станки: {production.Machines.Count}, " +
                      $"Детали: {production.Details.Count}, " +
                      $"Линии: {production.Lines.Count}\n";

                if (!production.Lines.Any())
                {
                    Log += "[Ошибка] Нет доступных линий обработки\n";
                    return new List<Detail>();
                }

                var lastLine = production.Lines.Last();
                Log += $"[Обработка] Последняя линия станка #{lastLine.Machine.ID}\n";

                var orderedDetails = lastLine.ProcessingTime
                    .OrderByDescending(x => x.Value)
                    .Select(x =>
                    {
                        Log += $"[Деталь] ID: {x.Key.ID} Время: {x.Value} сек\n";
                        return x.Key;
                    })
                    .ToList();

                Log += $"[Результат] Упорядочено деталей: {orderedDetails.Count}\n";
                return orderedDetails;
            }
            catch (Exception ex)
            {
                Log += $"[Критическая ошибка] {ex.Message}\n";
                return new List<Detail>();
            }
            finally
            {
                sw.Stop();
                var totalTime = $"[Общее время] {sw.Elapsed.TotalSeconds:0.000} сек\n";
                Log = totalTime + Log;
            }
        }
    }
}