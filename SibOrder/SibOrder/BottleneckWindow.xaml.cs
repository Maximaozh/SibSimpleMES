using LiveCharts.Wpf;
using LiveCharts;
using ProductionData;
using System;
using System.Collections.Generic;
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
using Microsoft.Win32;
using System.IO;

namespace SibOrder
{
    public partial class BottleneckWindow : Window
    {
        public SeriesCollection Series { get; set; }
        public List<string> MachineLabels { get; set; }
        public string BottleneckInfo { get; set; }

        public BottleneckWindow(Production production)
        {
            InitializeComponent();
            DataContext = this;
            AnalyzeBottlenecks(production);
        }

        private void AnalyzeBottlenecks(Production production)
        {
            var data = production.Lines.Select(line =>
                line.ProcessingTime.Values.Sum(x => (double)x)).ToList();

            Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Суммарное время",
                    Values = new ChartValues<double>(data),
                    Fill = new SolidColorBrush(Colors.SteelBlue)
                }
            };

            MachineLabels = production.Machines
                .Select(m => $"Станок {m.ID}")
                .ToList();

            var maxTime = data.Max();
            var bottleneckIndex = data.IndexOf(maxTime);
            var bottleneckMachine = production.Machines.ElementAt(bottleneckIndex);

            BottleneckInfo = $"Критический участок:\n" +
                            $"Станок {bottleneckMachine.ID}\n" +
                            $"Общее время обработки: {maxTime} сек\n" +
                            $"Рекомендации:\n- Оптимизировать операции\n- Добавить параллельные линии\n- Проверить оборудование";
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