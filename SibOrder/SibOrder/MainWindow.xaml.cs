using ProductionLoader;
using ProductionOrdering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProductionData;
using System.Windows.Markup;
using Microsoft.Win32;
using System.IO;
using ManufactoryTime;

namespace SibOrder
{
    public partial class MainWindow : Window
    {
        private JsonWorker.JsonSettingsManager json;
        private PluginWorker Plugins { get; set; }
        private Production Prod { get; set; }
        private List<Detail> OrderedByMethods { get; set; }
        private List<GanttTask> Tasks { get; set; }
        ManufactoryTime.Tools Tools { get; set;}
        private ObservableCollection<RowDataViewModel> PerfomanceTimeData;

        ObservableCollection<PluginComboboxItem<IProductionOrdering>> OrderPlugins { get; set; }
        ObservableCollection<PluginComboboxItem<IProductionDataProvider>> ProviderPlugins { get; set; }
        ObservableCollection<string> BITechnologies { get; set; } 

        public MainWindow()
        {
            InitializeComponent();

            json = new JsonWorker.JsonSettingsManager(AppDomain.CurrentDomain.BaseDirectory + "config.json");
            
            Prod = new Production();
            Plugins = new PluginWorker();
            Tasks = new List<GanttTask>();
            Tools = new ManufactoryTime.Tools();
            OrderedByMethods = new List<Detail>();
            BITechnologies = new ObservableCollection<string>();
            PerfomanceTimeData = new ObservableCollection<RowDataViewModel>();
            OrderPlugins = new ObservableCollection<PluginComboboxItem<IProductionOrdering>>();
            ProviderPlugins = new ObservableCollection<PluginComboboxItem<IProductionDataProvider>>();

            BITechnologies.Add("Загрузка и простой");
            BITechnologies.Add("Бутылочное горлышко");
            BITechnologies.Add("Сравнение очередей");

            OrderCombobox.ItemsSource = OrderPlugins;
            ProviderCombobox.ItemsSource = ProviderPlugins;
            BIComboBox.ItemsSource = BITechnologies;

            Plugins.UpdateAll();


            foreach (var plugin in Plugins.OrderPlugins)
            {
                var item = new PluginComboboxItem<IProductionOrdering>();
                item.Name = plugin.Name();
                item.Launch = plugin;
                OrderPlugins.Add(item);
            }

            foreach (var plugin in Plugins.LoaderPlugins)
            {
                var item = new PluginComboboxItem<IProductionDataProvider>();
                item.Name = plugin.Name();
                item.Launch = plugin;
                ProviderPlugins.Add(item);
            }

            UpdatePlugins();
        }

        public void UpdatePlugins()
        {
            foreach (var plugin in Plugins.LoaderPlugins)
            {
                if (plugin.Settings == null)
                    continue;

                if (!json.SectionExists(plugin.Name()))
                    json.SaveSettings(plugin.Name(), plugin.Settings);
                else
                    plugin.Settings = json.GetSettings(plugin.Name());
            }

            foreach (var plugin in Plugins.OrderPlugins)
            {
                if (plugin.Settings == null)
                    continue;

                if (!json.SectionExists(plugin.Name()))
                    json.SaveSettings(plugin.Name(), plugin.Settings);
                else 
                    plugin.Settings = json.GetSettings(plugin.Name());
            }

        }
        public void UpdateTable(uint[,] values)
        {
            PerfomanceTimeTable.Columns.Clear();

            if (values == null)
                return;

            int columnCount = values.GetLength(1);
            PerfomanceTimeData = ProductionConvertor.ConvertToObservableCollection(values);

            for (int i = 0; i < columnCount; i++)
            {
                var column = new DataGridTextColumn
                {
                    Header = $"{i + 1}",
                    Binding = new Binding($"[{i}]")
                };
                PerfomanceTimeTable.Columns.Add(column);
            }

            PerfomanceTimeTable.ItemsSource = PerfomanceTimeData;
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ProviderCombobox.SelectedItem == null)
                    return;

                Prod = (ProviderCombobox.SelectedItem as PluginComboboxItem<IProductionDataProvider>).Launch.LoadProduction();

                var times = ProductionConvertor.ProductionToMatrix(Prod);
                UpdateTable(times);
            } catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка загрузки");
            }
            
        }

        private async void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrderCombobox.SelectedItem == null || Prod == null)
                return;

            InfoLabel.Clear();
            LaunchButton.IsEnabled = false;
            InfoLabel.Text = "Идёт обработка...";

            try
            {
                
                var pop = ProductionConvertor.ConvertTo2DArray(PerfomanceTimeData);
                Prod = ProductionConvertor.MatrixToProduction(pop);
                var orderMethod = (OrderCombobox.SelectedItem as PluginComboboxItem<IProductionOrdering>).Launch;

                
                var orderedResult = await Task.Run(() =>
                {
                    return orderMethod.Order(Prod);
                });

                
                OrderedByMethods = orderedResult;

                
                InfoLabel.Text = string.Join(" ", OrderedByMethods.Select(d => d.ID)) + "\n---\n";

                
                Tasks = GanttCalculator.Calculate(Prod, OrderedByMethods);

                uint totalIdleTime = CalculateIdleTime(Tasks);

                InfoLabel.Text += $"Время обработки: {Tools.CalculateTime(Prod, OrderedByMethods)}\n---\n";
                InfoLabel.Text += $"Время простоя: {totalIdleTime}\n---\nЛоги обработки:\n";
                InfoLabel.Text += orderMethod.Log;
            }
            catch (Exception ex)
            {
                InfoLabel.Text = $"Ошибка: {ex.Message}";
            }
            finally
            {
                LaunchButton.IsEnabled = true; 
            }
        }

        internal uint CalculateIdleTime(IEnumerable<GanttTask> tasks)
        {
            uint totalIdleTime = 0;
            var tasksByMachine = tasks.GroupBy(t => t.Machine);

            foreach (var group in tasksByMachine)
            {
                var sortedTasks = group.OrderBy(t => t.StartTime).ToList();
                uint previousEndTime = 0;

                foreach (var task in sortedTasks)
                {
                    if (task.StartTime > previousEndTime)
                    {
                        totalIdleTime += task.StartTime - previousEndTime;
                    }
                    previousEndTime = task.StartTime + task.Duration;
                }
            }

            return totalIdleTime;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Toolbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void GanntButton_Click(object sender, RoutedEventArgs e)
        {
            if (Tasks is null || Tasks.Count <= 0)
            { 
                MessageBox.Show("Невозможно отобразить график: количество полученной информации о процессе неизвестно либо равно нулю", "ОШИБКА - ДИАГРАММА ГАНТА");
                return;
            }
            new Diagramm(Tasks, Tools) { Owner = this }.Show();
        }

        private void SaveLogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string logContent = InfoLabel.Text;

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    DefaultExt = ".txt",
                    FileName = $"production_log_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, logContent, Encoding.UTF8);

                    MessageBox.Show("Лог успешно сохранён!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении лога: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BIGraphics_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var text = BIComboBox.Text;
                if (Prod == null || Tasks == null)
                {
                    MessageBox.Show("Сначала выполните расчёт производства!");
                    return;
                }
                switch (text)
                {
                    case "Загрузка и простой":
                        {
                            new MachineEfficiencyWindow(Prod, Tasks) { Owner = this }.Show();
                        }
                        break;
                    case "Бутылочное горлышко":
                        {
                            new BottleneckWindow(Prod) { Owner = this }.Show();
                        }
                        break;
                    case "Сравнение очередей":
                        {
                            new AlgorithmComparisonWindow(Prod, OrderPlugins) { Owner = this }.Show();
                        }
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка - Анализ");
            }

        }
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var editor = new SettingsEditorWindow(json);
            editor.ShowDialog();
            UpdatePlugins();
        }
    }
}