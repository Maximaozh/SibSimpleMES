using ProductionData;
using ProductionOrdering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace JohnsonGeneralizationFourth
{
    public class JohnsonOrder : IProductionOrdering
    {
        public override string GUID() => "{F0482B72-7E5C-41BD-A36D-AF1CE9862AEF}";

        public override string Name() => "0003 Джонсон Четвёртое Обобщение";

        public override List<Detail> Order(Production production)
        {
            var sw = Stopwatch.StartNew();
            Log = "";

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

                if (production.Machines.Count < 0)
                {
                    Log += "[Предупреждение] Некорректное количество станков\n";
                    return new List<Detail>();
                }

                var maximums = new Dictionary<Detail, uint>();
                Log += "[Этап] Расчёт суммарного времени обработки\n";

                foreach (var detail in production.Details)
                {
                    var sumTime = (uint)production.Lines
                        .Sum(line => line.ProcessingTime.TryGetValue(detail, out var time) ? time : 0);

                    maximums[detail] = sumTime;
                    Log += $"[Деталь] ID: {detail.ID} Суммарное время: {sumTime} сек\n";
                }

                Log += "[Этап] Сортировка по убыванию суммарного времени\n";
                var result = maximums
                    .OrderByDescending(x => x.Value)
                    .Select(x =>
                    {
                        Log += $"[Сортировка] Деталь {x.Key.ID} -> {x.Value} сек\n";
                        return x.Key;
                    })
                    .ToList();

                Log += $"[Результат] Упорядочено деталей: {result.Count}\n";

                return result;
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