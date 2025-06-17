using ProductionData;
using ProductionOrdering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JohnsonGeneralizationThird
{
    public class JohnsonOrder : IProductionOrdering
    {
        public override string GUID() => "{8092EBC2-10CC-4ECE-A07F-FAA970E35358}";

        public override string Name() => "0002 Джонсон Третье обобщение";

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

                if (!production.Lines.Any())
                {
                    Log += "[Ошибка] Нет доступных линий обработки\n";
                    return new List<Detail>();
                }

                var maximums = new List<Tuple<Detail, uint, int>>();
                Log += "[Этап] Поиск максимальных времён обработки\n";

                foreach (var detail in production.Details)
                {
                    Tuple<Detail, uint, int> detailMax = null;
                    Log += $"[Деталь] Обработка ID: {detail.ID}\n";

                    foreach (var line in production.Lines)
                    {
                        if (line.ProcessingTime.TryGetValue(detail, out uint time))
                        {
                            Log += $"[Станок] Линия {line.Machine.ID}: {time} сек\n";

                            if (detailMax == null || time > detailMax.Item2)
                            {
                                detailMax = new Tuple<Detail, uint, int>(detail, time, line.Machine.ID);
                                Log += $"[Обновление] Новый максимум: {time} сек (станок {line.Machine.ID})\n";
                            }
                        }
                    }

                    if (detailMax != null)
                    {
                        maximums.Add(detailMax);
                        Log += $"[Результат] Для детали {detail.ID} установлен максимум: " +
                              $"{detailMax.Item2} сек на станке {detailMax.Item3}\n";
                    }
                }

                Log += "[Этап] Сортировка деталей\n";
                maximums.Sort((x, y) =>
                {
                    int compare = y.Item2.CompareTo(x.Item2);
                    if (compare == 0)
                    {
                        compare = y.Item3.CompareTo(x.Item3);
                    }
                    Log += $"[Сравнение] Деталь {x.Item1.ID} ({x.Item2} сек, станок {x.Item3}) vs " +
                          $"Деталь {y.Item1.ID} ({y.Item2} сек, станок {y.Item3}) => {compare}\n";
                    return compare;
                });

                var result = maximums.Select(data => data.Item1).ToList();
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