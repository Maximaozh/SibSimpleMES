using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductionData;
using ProductionLoader;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;

namespace ExcelProductionLoader
{
    public class ExcelProductionDataProvider : IProductionDataProvider
    {
        public string Type
        {
            get
            {
                return Settings["Type_string"];
            }
        }
        public string FilePath
        {
            get
            {
                return Settings["Path_file"];
            }
        }

        public ExcelProductionDataProvider()
        {
            Settings = new Dictionary<string, string>();
            Settings.Add("Type_string", "default");
            Settings.Add("Path_file", "null");
        }

        public override string GUID()
        {
            return "{258C1227-9350-4490-8201-35F2A79D9DDB}";
        }

        public override string Name()
        {
            return "Загрузка xls";
        }
        public override Production LoadProduction()
        {
            Production prod = new Production();

            var data = Settings["Type_string"] == "default" ? ParseTableStandart() : ParseTableRotated();

            if (data == null || data.Count < 0)
                return prod;

            for (int i = 1; i <= data[0].Count; i++)
                prod.AddDetail(new Detail(i));

            for (int i = 1; i <= data.Count; i++)
                prod.AddMachine(new Machine(i));


            int row = 0;
            foreach (var machine in prod.Machines)
            {
                int col = 0;
                Line line = new Line(machine);
                foreach (var detail in prod.Details)
                {
                    line.SetValue(detail, data[row][col]);
                    col++;
                }
                row++;
                prod.AddLine(line);
            }

            return prod;
        }
        private List<List<uint>> ParseTableRotated()
        {
            List<List<uint>> matrix = new List<List<uint>>();

            using (FileStream file = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook;
                if (FilePath.EndsWith(".xls"))
                {
                    workbook = new HSSFWorkbook(file);
                }
                else
                {
                    workbook = new XSSFWorkbook(file);
                }

                ISheet sheet = workbook.GetSheetAt(0);
                IRow headerRow = sheet.GetRow(0);

                for (int col = 0; col < headerRow.LastCellNum; col++)
                {
                    List<uint> currentRow = new List<uint>();
                    for (int row = 0; row <= sheet.LastRowNum; row++)
                    {
                        IRow currentRowData = sheet.GetRow(row);
                        ICell cell = currentRowData?.GetCell(col);

                        if (cell != null && uint.TryParse(cell.ToString(), out uint value))
                        {
                            currentRow.Add(value);
                        }
                        else
                        {
                            currentRow.Add(0);
                        }
                    }
                    matrix.Add(currentRow);
                }
            }
            return matrix;
        }

        private List<List<uint>> ParseTableStandart()
        {
            List<List<uint>> matrix = new List<List<uint>>();

            using (FileStream file = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook;
                if (FilePath.EndsWith(".xls"))
                {
                    workbook = new HSSFWorkbook(file);
                }
                else
                {
                    workbook = new XSSFWorkbook(file);
                }

                ISheet sheet = workbook.GetSheetAt(0);

                for (int row = 0; row <= sheet.LastRowNum; row++)
                {
                    IRow currentRowData = sheet.GetRow(row);
                    List<uint> currentRow = new List<uint>();

                    for (int col = 0; col < currentRowData.LastCellNum; col++)
                    {
                        ICell cell = currentRowData?.GetCell(col);

                        if (cell != null && uint.TryParse(cell.ToString(), out uint value))
                        {
                            currentRow.Add(value);
                        }
                        else
                        {
                            currentRow.Add(0);
                        }
                    }
                    matrix.Add(currentRow);
                }
            }
            return matrix;
        }
    }
}
