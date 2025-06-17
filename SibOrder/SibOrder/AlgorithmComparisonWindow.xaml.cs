using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using ProductionData;
using ProductionOrdering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SibOrder
{

    public class BenchmarkResult
    {
        public string AlgorithmName { get; set; }
        public double TotalTime { get; set; }  
        public double TotalIdleTime { get; set; }  
        public double AvgUtilization { get; set; }  
    }

    public partial class AlgorithmComparisonWindow : Window
    {
        public SeriesCollection ChartSeries { get; set; }
        public List<string> AlgorithmNames { get; set; }
        public Func<double, string> Formatter { get; set; }

        public ObservableCollection<BenchmarkResult> Results { get; } = new ObservableCollection<BenchmarkResult>();

        public AlgorithmComparisonWindow(Production production,
                                   IEnumerable<PluginComboboxItem<IProductionOrdering>> algorithms)
        {
            InitializeComponent();
            DataContext = this;
            Formatter = value => value.ToString("N0");
            RunBenchmarks(production, algorithms);
            InitializeChart();
        }
        private void InitializeChart()
        {
            ChartSeries = new SeriesCollection
        {
            new ColumnSeries
            {
                Title = "Общее время",
                Values = new ChartValues<double>(Results.Select(r => r.TotalTime)),
                Fill = Brushes.SteelBlue
            },
            new ColumnSeries
            {
                Title = "Простой",
                Values = new ChartValues<double>(Results.Select(r => r.TotalIdleTime)),
                Fill = Brushes.IndianRed
            },
            new LineSeries
            {
                Title = "Загрузка",
                Values = new ChartValues<double>(Results.Select(r => r.AvgUtilization)),
                Stroke = Brushes.ForestGreen,
                Fill = Brushes.Transparent,
                ScalesYAt = 1 // Используем вторую ось Y
            }
        };

            AlgorithmNames = Results.Select(r => r.AlgorithmName).ToList();
        }
        private void RunBenchmarks(Production production, IEnumerable<PluginComboboxItem<IProductionOrdering>> algorithms)
        {
            Results.Clear();

            foreach (var algorithm in algorithms)
            {
                var ordered = algorithm.Launch.Order(production);
                var tasks = GanttCalculator.Calculate(production, ordered);

                Results.Add(new BenchmarkResult
                {
                    AlgorithmName = algorithm.Name,
                    TotalTime = CalculateTotalProductionTime(tasks),
                    TotalIdleTime = CalculateTotalIdleTime(tasks),
                    AvgUtilization = CalculateAverageUtilization(tasks)
                });
            }
        }

        private double CalculateTotalProductionTime(List<GanttTask> tasks)
        {
            return tasks.Any() ?
                tasks.Max(t => t.StartTime + t.Duration) : 0;
        }

        internal double CalculateTotalIdleTime(List<GanttTask> tasks)
        {
            // Используем существующий метод из MainWindow
            return new MainWindow().CalculateIdleTime(tasks);
        }

        private double CalculateAverageUtilization(List<GanttTask> tasks)
        {
            var machineGroups = tasks
     .GroupBy(t => t.Machine)
     .ToList();

            if (!machineGroups.Any())
                return 0;

            double totalUtilization = 0;
            int machinesWithWork = 0;

            foreach (var group in machineGroups)
            {
                var lastTask = group.OrderByDescending(t => t.StartTime + t.Duration).FirstOrDefault();
                if (lastTask == null) continue;

                double totalTime = lastTask.StartTime + lastTask.Duration;
                if (totalTime <= 0) continue;

                double busyTime = group.Sum(t => t.Duration);
                double utilization = (busyTime / totalTime) * 100;

                totalUtilization += utilization;
                machinesWithWork++;
            }

            return machinesWithWork > 0 ? totalUtilization / machinesWithWork : 0;
        }

        private void Chart_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
            if (saveFileDialog.ShowDialog() == true)
            {
                // Находим CartesianChart (замените "cartesianChart" на имя вашего графика, если оно другое)
                var chart = FindName("Chart") as LiveCharts.Wpf.CartesianChart; // !!! Важно: убедитесь, что CartesianChart имеет x:Name="cartesianChart" в XAML !!!
                if (chart == null)
                {
                    MessageBox.Show("Не удалось найти график.");
                    return;
                }

                RenderTargetBitmap rtb = new RenderTargetBitmap((int)chart.ActualWidth, (int)chart.ActualHeight, 96d, 96d, PixelFormats.Default);
                rtb.Render(chart);

                BitmapEncoder encoder;
                string fileExtension = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower();
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
}
