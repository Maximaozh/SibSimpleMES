using ProductionData;
using ProductionOrdering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JohnsonGeneralizationFirst;
using JohnsonGeneralizationSecond;
using JohnsonGeneralizationThird;
using JohnsonGeneralizationFourth;
using System.Diagnostics;

namespace JohnsonGeneralizationFifth
{
    public class JohnsonOrdering : IProductionOrdering
    {
        public override string GUID() => "{35EE924F-70E5-478F-9C68-93E446775143}";

        public override string Name() => "0004 Джонсон Пятое Обобщение";

        public override List<Detail> Order(Production production)
        {
            var sw = Stopwatch.StartNew();
            Log = "";

            try
            {
                Log += $"[Старт] {DateTime.Now:HH:mm:ss}\n";
                Log += "[Этап] Инициализация методов\n";

                var first = new JohnsonGeneralizationFirst.JohnsonOrder();
                var second = new JohnsonGeneralizationSecond.JohnsonOrder();
                var third = new JohnsonGeneralizationThird.JohnsonOrder();
                var fourth = new JohnsonGeneralizationFourth.JohnsonOrder();

                Log += $"[Методы] Используются: {first.Name()}, {second.Name()}, " +
                      $"{third.Name()}, {fourth.Name()}\n";

                Log += "[Этап] Получение упорядоченных списков\n";
                var totalGeneralizations = new List<List<Detail>>
                {
                    first.Order(production),
                    second.Order(production),
                    third.Order(production),
                    fourth.Order(production)
                };


                Log += "[Этап] Расчёт суммарных позиций\n";
                var times = new Dictionary<Detail, uint>();

                foreach (var detail in production.Details)
                {
                    var sum = (uint)totalGeneralizations
                        .Select(list => list.IndexOf(detail))
                        .Sum();

                    times[detail] = sum;
                    Log += $"[Деталь] ID: {detail.ID} Сумма позиций: {sum}\n";
                }

                Log += "[Этап] Финальная сортировка\n";
                var result = times
                    .OrderBy(x => x.Value)
                    .Select(x =>
                    {
                        Log += $"[Сортировка] Деталь {x.Key.ID} -> {x.Value}\n";
                        return x.Key;
                    })
                    .ToList();

                Log += $"[Итог] Упорядочено деталей: {result.Count}\n";
                Log += "[Список] " + string.Join(", ", result.Select(d => d.ID)) + "\n";

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
