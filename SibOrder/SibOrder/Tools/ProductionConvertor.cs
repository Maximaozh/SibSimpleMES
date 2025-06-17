using ProductionData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SibOrder
{
    internal class ProductionConvertor
    {
        public static Production MatrixToProduction(uint[,] matrix)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix), "Matrix cannot be null.");

            int machineCount = matrix.GetLength(0);
            int detailCount = matrix.GetLength(1); 

            Production production = new Production();

            for (int detailId = 0; detailId < detailCount; detailId++)
            {
                production.AddDetail(new Detail(detailId + 1));
            }

            for (int machineId = 0; machineId < machineCount; machineId++)
            {
                Machine machine = new Machine(machineId + 1);
                production.AddMachine(machine);

                Line line = new Line(machine);

                for (int detailId = 0; detailId < detailCount; detailId++)
                {
                    Detail detail = new Detail(detailId + 1);
                    uint processingTime = matrix[machineId, detailId];
                    line.SetValue(detail, processingTime);
                }

                production.AddLine(line);
            }

            return production;
        }

        public static uint[,] ProductionToMatrix(Production production)
        {
            if (production == null)
                throw new ArgumentNullException(nameof(production), "Production cannot be null.");

            int machineCount = production.Machines.Count;
            int detailCount = production.Details.Count;

            uint[,] matrix = new uint[machineCount, detailCount];

            int machineIndex = 0;
            foreach (var machine in production.Machines)
            {
                var line = production.Lines.Find(l => l.Machine.Equals(machine));
                if (line == null)
                    throw new InvalidOperationException($"No production line found for machine {machine.ID}.");

                int detailIndex = 0;
                foreach (var detail in production.Details)
                {
                    if (line.ProcessingTime.TryGetValue(detail, out uint processingTime))
                    {
                        matrix[machineIndex, detailIndex] = (uint)processingTime;
                    }
                    else
                    {
                        matrix[machineIndex, detailIndex] = 0;
                    }
                    detailIndex++;
                }
                machineIndex++;
            }

            return matrix;
        }

        public static ObservableCollection<RowDataViewModel> ConvertToObservableCollection(uint[,] array)
        {
            var collection = new ObservableCollection<RowDataViewModel>();
            int rowCount = array.GetLength(0);
            int colCount = array.GetLength(1);

            for (int i = 0; i < rowCount; i++)
            {
                uint[] row = new uint[colCount];
                for (int j = 0; j < colCount; j++)
                {
                    row[j] = array[i, j];
                }
                collection.Add(new RowDataViewModel(row));
            }

            return collection;
        }

        public static uint[,] ConvertTo2DArray(ObservableCollection<RowDataViewModel> collection)
        {
            if (collection == null || collection.Count == 0)
                return new uint[0, 0];

            int rowCount = collection.Count;
            int colCount = collection[0].Values.Length;

            uint[,] array = new uint[rowCount, colCount];

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    array[i, j] = collection[i][j];
                }
            }

            return array;
        }
    }
}
