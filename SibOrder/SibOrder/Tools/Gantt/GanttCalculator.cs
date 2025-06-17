using ProductionData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace SibOrder
{
    internal class GanttCalculator
    {
        public static List<GanttTask> Calculate(Production production, List<Detail> order)
        {

            List<ProductionData.Line> lines = production.Lines;

            Dictionary<Machine, uint> machineCompletionTimes = new Dictionary<Machine, uint>();

            foreach (var line in lines)
            {
                machineCompletionTimes[line.Machine] = 0;
            }

            List<GanttTask> tasks = new List<GanttTask>();
            foreach (var detail in order)
            {
                var relevantLines = lines.Where(l => l.ProcessingTime.ContainsKey(detail)).ToList();

                if (relevantLines.Count == 0)
                {
                    throw new InvalidOperationException($"Деталь {detail.ID} не может быть обработана ни на одной линии.");
                }

                uint previousLineCompletionTime = 0;

                foreach (var line in relevantLines)
                {
                    var machine = line.Machine;
                    var processingTime = line.ProcessingTime[detail];

                    uint startTime = Math.Max(
                        machineCompletionTimes[machine], 
                        previousLineCompletionTime 
                    );

                    var task = new GanttTask
                    {
                        Detail = detail,
                        Machine = machine,
                        StartTime = startTime,
                        Duration = processingTime
                    };
                    tasks.Add(task);

                    machineCompletionTimes[machine] = startTime + processingTime;

                    previousLineCompletionTime = startTime + processingTime;
                }
            }

            return tasks;
        }
    }
}