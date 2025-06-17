using HandyControl.Controls;
using Microsoft.Win32;
using ProductionData;
using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Логика взаимодействия для Diagramm.xaml
    /// </summary>
    public partial class Diagramm  : System.Windows.Window
    {
        private Dictionary<Detail, SolidColorBrush> _detailColors = new Dictionary<Detail, SolidColorBrush>();

        private static readonly List<SolidColorBrush> ColorPalette = new List<SolidColorBrush> {
            Brushes.LightBlue,
            Brushes.LightGreen,
            Brushes.LightCoral,
            Brushes.LightGoldenrodYellow,
            Brushes.LightPink,
            Brushes.LightSeaGreen,
            Brushes.LightSkyBlue,
            Brushes.LightSalmon,
            Brushes.LightSteelBlue,
            Brushes.LightGray,
            Brushes.MediumPurple,
            Brushes.MediumAquamarine,
            Brushes.MediumSlateBlue,
            Brushes.MediumVioletRed,
            Brushes.Orange,
            Brushes.Orchid,
            Brushes.PaleGreen,
            Brushes.PaleTurquoise,
            Brushes.PaleVioletRed,
            Brushes.PeachPuff
        };

        private int scaleX;
        private int scaleY;
        private int ScaleX {
            get { 
                return scaleX; 
            }
            set { 
                if (value > 0)
                    scaleX = value;
            }
        }
        private int ScaleY
        {
            get
            {
                return scaleY;
            }
            set
            {
                if (value > 0)
                    scaleY = value;
            }
        }

        List<GanttTask> Tasks { get; set; }
        ManufactoryTime.Tools Tools { get; set; }

        internal Diagramm(List<GanttTask> tasks, ManufactoryTime.Tools tools)
        {
            InitializeComponent();

            Tools = tools;
            Tasks = tasks;
            ScaleX = 10;
            ScaleY = 1;

            GanttScrollviewer.Background = Brushes.White;

            GanntDraw();
        }

        private void GanntDraw()
        {
            GanttCanvas.Children.Clear();

            Dictionary<Detail, SolidColorBrush> detailColors = new Dictionary<Detail, SolidColorBrush>();
            for (int i = 0; i < Tasks.Count; i++)
            {
                var detail = Tasks[i].Detail;
                if (!detailColors.ContainsKey(detail))
                {
                    detailColors[detail] = ColorPalette[detailColors.Count % ColorPalette.Count];
                }
            }

            if (Tasks == null || Tasks.Count == 0)
                return;

            double yOffset = 30;
            double leftMargin = 50;
            double topMargin = 30;
            double yOffsetPerMachine = 30 * ScaleY;

            var uniqueMachines = Tasks.Select(t => t.Machine).Distinct().OrderBy(m => m.ID).ToList();

            double maxEndTime = Tasks.Max(t => t.StartTime + t.Duration);
            double canvasWidth = maxEndTime * ScaleX + leftMargin + 20 * ScaleX;
            double canvasHeight = uniqueMachines.Count * yOffset * ScaleY + 20 * ScaleY;
            
            GanttCanvas.Width = canvasWidth;
            GanttCanvas.Height = canvasHeight;

            DrawTimeScale(maxEndTime, leftMargin, topMargin);
            foreach (var machine in uniqueMachines)
            {
                int machineIndex = uniqueMachines.IndexOf(machine);
                double y = (machineIndex+1) * yOffsetPerMachine;

                TextBlock machineText = new TextBlock
                {
                    Text = $"{machine.ID}",
                    Foreground = Brushes.Black,
                    FontSize = 12,
                    TextAlignment = TextAlignment.Right,
                    Width = leftMargin - 10,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                
                double textTop = y + (20 * ScaleY - machineText.FontSize) / 2;
                Canvas.SetLeft(machineText, 5);
                Canvas.SetTop(machineText, textTop);

                GanttCanvas.Children.Add(machineText);
            }

            foreach (var task in Tasks)
            {
                double x = task.StartTime * ScaleX + leftMargin;
                double width = task.Duration * ScaleX;
                double height = 20 * scaleY;
                double y = task.Machine.ID * yOffset * ScaleY;

                var color = detailColors[task.Detail];

                Rectangle rect = new Rectangle
                {
                    Width = width,
                    Height = height,
                    Fill = color,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);

                GanttCanvas.Children.Add(rect);


                if (width < 20)
                    continue;

                TextBlock text = new TextBlock
                {
                    Text = $"{task.Detail.ID}",
                    Foreground = Brushes.Black,
                    FontSize = 10
                };

                Canvas.SetLeft(text, x + 5);
                Canvas.SetTop(text, y + 2);

                GanttCanvas.Children.Add(text);
            }
        }
        
        private void GanttSave()
        {
            FrameworkElement content = GanttScrollviewer.Content as FrameworkElement;


            Size contentSize = new Size(content.ActualWidth, content.ActualHeight);
            content.Measure(contentSize);
            content.Arrange(new Rect(contentSize));

            RenderTargetBitmap renderTarget = new RenderTargetBitmap(
                (int)content.ActualWidth,
                (int)content.ActualHeight,
                96,
                96,
                PixelFormats.Pbgra32);

            renderTarget.Render(content);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTarget));

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image|*.png";
            saveFileDialog.Title = "Save Content as PNG";
            saveFileDialog.FileName = "Диаграмма.png";

            if (saveFileDialog.ShowDialog() == true)
            {
                using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
        }


        private void DrawTimeScale(double maxTime, double leftMargin, double topMargin)
        {
            // Определение шага для меток
            double timeStep = CalculateOptimalTimeStep(maxTime);

            // Линия шкалы
            System.Windows.Shapes.Line timeLine = new System.Windows.Shapes.Line
            {
                X1 = leftMargin,
                X2 = GanttCanvas.Width,
                Y1 = topMargin - 10,
                Y2 = topMargin - 10,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            GanttCanvas.Children.Add(timeLine);

            // Метки времени
            for (double time = 0; time <= maxTime; time += timeStep)
            {
                double x = time * ScaleX + leftMargin;

                // Вертикальная черта
                System.Windows.Shapes.Line tick = new System.Windows.Shapes.Line
                {
                    X1 = x,
                    X2 = x,
                    Y1 = topMargin - 15,
                    Y2 = topMargin - 5,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                // Текст
                TextBlock label = new TextBlock
                {
                    Text = $"{time}",
                    Foreground = Brushes.Black,
                    FontSize = 10,
                    TextAlignment = TextAlignment.Center
                };

                Canvas.SetLeft(label, x - label.ActualWidth / 2);
                Canvas.SetTop(label, topMargin - 25);

                GanttCanvas.Children.Add(tick);
                GanttCanvas.Children.Add(label);
            }

            if (timeStep > 5)
            {
                for (double time = 0; time <= maxTime; time += timeStep / 2)
                {
                    double x = leftMargin + time * ScaleX;

                    System.Windows.Shapes.Line subGridLine = new System.Windows.Shapes.Line
                    {
                        X1 = x,
                        X2 = x,
                        Y1 = topMargin,
                        Y2 = GanttCanvas.Height - 20,
                        Stroke = Brushes.Gray,
                        StrokeThickness = 0.3,
                        Opacity = 0.2
                    };
                    GanttCanvas.Children.Add(subGridLine);
                }
            }
        }
        private double CalculateOptimalTimeStep(double maxTime)
        {
            double[] possibleSteps = { 1, 2, 5, 10, 15, 30, 60, 100 };
            double pixelsPerStep = 100;
            double desiredStep = pixelsPerStep / ScaleX;

            double selectedStep = possibleSteps.FirstOrDefault(step => step >= desiredStep);

            return selectedStep > 0 ? selectedStep : Math.Max(maxTime / 10, 1);
        }
      
        private void ScalePlusX_Click(object sender, RoutedEventArgs e)
        {
            ScaleX += 1;
            GanntDraw();
        }

        private void ScaleMinusX_Click(object sender, RoutedEventArgs e)
        {
            ScaleX -= 1;
            GanntDraw();
        }

        private void ScalePlusY_Click(object sender, RoutedEventArgs e)
        {
            ScaleY += 1;
            GanntDraw();
        }

        private void ScaleMinusY_Click(object sender, RoutedEventArgs e)
        {
            ScaleY -= 1;
            GanntDraw();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            GanttSave();
        }
        private void GanttCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            GanttSave();
        }
    }
}
