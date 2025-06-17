using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using ProductionData;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace SibOrder
{
    public partial class MachineEfficiencyWindow : Window
    {
        public SeriesCollection UtilizationSeries { get; set; }
        public SeriesCollection IdleTimeSeries { get; set; }
        public List<string> MachineNames { get; set; }

        internal MachineEfficiencyWindow(Production production, List<GanttTask> tasks)
        {
            InitializeComponent();
            DataContext = this;

            var data = CalculateEfficiencyData(production, tasks);
            InitializeCharts(data);
        }

        private List<MachineData> CalculateEfficiencyData(Production production, List<GanttTask> tasks)
        {
            return production.Machines.Select(machine =>
            {
                var machineTasks = tasks?
                    .Where(t => t.Machine.ID == machine.ID)
                    .OrderBy(t => t.StartTime)
                    .ToList();

                double totalTime = machineTasks?.Max(t => (double)(t.StartTime + t.Duration)) ?? 0;
                double busyTime = machineTasks?.Sum(t => (double)t.Duration) ?? 0;
                double idleTime = CalculateIdleTime(machineTasks, totalTime);

                return new MachineData
                {
                    Name = $"Станок {machine.ID}",
                    Utilization = totalTime > 0 ? (busyTime / totalTime) * 100 : 0,
                    IdleTime = idleTime
                };
            }).ToList();
        }

        private double CalculateIdleTime(List<GanttTask> tasks, double totalTime)
        {
            if (tasks == null || tasks.Count == 0) return 0;

            double idle = 0;
            uint prevEnd = 0;

            foreach (var task in tasks.OrderBy(t => t.StartTime))
            {
                if (task.StartTime > prevEnd) idle += task.StartTime - prevEnd;
                prevEnd = task.StartTime + task.Duration;
            }

            if (totalTime > prevEnd) idle += totalTime - prevEnd;
            return idle;
        }

        private void InitializeCharts(List<MachineData> data)
        {
            MachineNames = data.Select(d => d.Name).ToList();

            UtilizationSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Загрузка",
                    Values = new ChartValues<double>(data.Select(d => d.Utilization)),
                    DataLabels = true,
                    LabelPoint = p => $"{p.Y:N1}%"
                }
            };

            IdleTimeSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Простой",
                    Values = new ChartValues<double>(data.Select(d => d.IdleTime)),
                    DataLabels = true,
                    LabelPoint = p => $"{p.Y:N0} сек"
                }
            };
        }

        private void CartesianChart_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
            if (saveFileDialog.ShowDialog() == true)
            {
                // Находим CartesianChart (замените "cartesianChart" на имя вашего графика, если оно другое)
                var chart = FindName("prostoy") as LiveCharts.Wpf.CartesianChart; // !!! Важно: убедитесь, что CartesianChart имеет x:Name="cartesianChart" в XAML !!!
                if (chart == null)
                {
                    MessageBox.Show("Не удалось найти график.");
                    return;
                }

                RenderTargetBitmap rtb = new RenderTargetBitmap((int)chart.ActualWidth, (int)chart.ActualHeight, 96d, 96d, PixelFormats.Default);
                rtb.Render(chart);

                BitmapEncoder encoder;
                string fileExtension = Path.GetExtension(saveFileDialog.FileName).ToLower();
                switch (fileExtension)
                {
                    case ".jpg":
                        encoder = new JpegBitmapEncoder();
                        break;
                    case ".bmp":
                        encoder = new BmpBitmapEncoder();
                        break;
                    default: // ".png"
                        encoder = new PngBitmapEncoder();
                        break;
                }

                encoder.Frames.Add(BitmapFrame.Create(rtb));

                using (FileStream fs = File.Create(saveFileDialog.FileName))
                {
                    encoder.Save(fs);
                }

                MessageBox.Show("График сохранен!");
            }
         }

        private void zagruzka_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
            if (saveFileDialog.ShowDialog() == true)
            {
                // Находим CartesianChart (замените "cartesianChart" на имя вашего графика, если оно другое)
                var chart = FindName("zagruzka") as LiveCharts.Wpf.CartesianChart; // !!! Важно: убедитесь, что CartesianChart имеет x:Name="cartesianChart" в XAML !!!
                if (chart == null)
                {
                    MessageBox.Show("Не удалось найти график.");
                    return;
                }

                RenderTargetBitmap rtb = new RenderTargetBitmap((int)chart.ActualWidth, (int)chart.ActualHeight, 96d, 96d, PixelFormats.Default);
                rtb.Render(chart);

                BitmapEncoder encoder;
                string fileExtension = Path.GetExtension(saveFileDialog.FileName).ToLower();
                switch (fileExtension)
                {
                    case ".jpg":
                        encoder = new JpegBitmapEncoder();
                        break;
                    case ".bmp":
                        encoder = new BmpBitmapEncoder();
                        break;
                    default: // ".png"
                        encoder = new PngBitmapEncoder();
                        break;
                }

                encoder.Frames.Add(BitmapFrame.Create(rtb));

                using (FileStream fs = File.Create(saveFileDialog.FileName))
                {
                    encoder.Save(fs);
                }

                MessageBox.Show("График сохранен!");
            }
        }
    }

    public class MachineData
    {
        public string Name { get; set; }
        public double Utilization { get; set; }
        public double IdleTime { get; set; }
    }
}