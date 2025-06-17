using ProductionData;
using ProductionOrdering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JohnsonGeneralizationFirst
{
    public class JohnsonOrder : IProductionOrdering
    {
        public override string GUID() => "{961305CF-49AE-4402-A01B-5786EFF83978}";

        public override string Name() => "0000 Джонсон Первое обобщение";

        public override List<Detail> Order(Production production)
        {
            var sw = Stopwatch.StartNew();
            Log = "";

            try
            {
                var startTime = DateTime.Now;
                Log += $"[Старт] {startTime:HH:mm:ss}\n";

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

                var mainLine = production.Lines.First();
                Log += $"[Обработка] Линия станка #{mainLine.Machine.ID}\n";

                var orderedDetails = mainLine.ProcessingTime
                    .OrderBy(x => x.Value)
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