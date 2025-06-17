using JsonWorker;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SibOrder
{
    public partial class SettingsEditorWindow : Window
    {
        private readonly JsonSettingsManager _settingsManager;
        private JObject _config;
        private ObservableCollection<PluginSection> _sections = new ObservableCollection<PluginSection>();

        public SettingsEditorWindow(JsonSettingsManager settingsManager)
        {
            InitializeComponent();
            _settingsManager = settingsManager;
            LoadConfig();
            PluginsList.ItemsSource = _sections;
        }

        private void LoadConfig()
        {
            try
            {
                if (!File.Exists(_settingsManager.FilePath)) return;

                var json = File.ReadAllText(_settingsManager.FilePath);
                _config = JObject.Parse(json);

                foreach (var section in _config.Properties())
                {
                    var pluginSection = new PluginSection
                    {
                        Name = section.Name,
                        Parameters = new ObservableCollection<PluginParameter>()
                    };

                    var settings = _settingsManager.GetSettings(section.Name);
                    foreach (var setting in settings)
                    {
                        var param = new PluginParameter
                        {
                            Key = setting.Key,
                            Value = setting.Value,
                            Type = GetParameterType(setting.Key)
                        };
                        pluginSection.Parameters.Add(param);
                    }

                    _sections.Add(pluginSection);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки конфигурации: {ex.Message}");
            }
        }

        private ParameterType GetParameterType(string key)
        {
            if (key.EndsWith("_int")) return ParameterType.Integer;
            if (key.EndsWith("_string")) return ParameterType.String;
            if (key.EndsWith("_file")) return ParameterType.Path;
            return ParameterType.String;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var section in _sections)
                {
                    var settings = section.Parameters
                        .ToDictionary(p => p.Key, p => p.Value);

                    _settingsManager.SaveSettings(section.Name, settings);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
            this.Close();
        }

        private void PluginsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PluginsList.SelectedItem is PluginSection selected)
            {
                ParametersPanel.Children.Clear();

                foreach (var param in selected.Parameters)
                {
                    var stack = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
                    stack.Children.Add(new TextBlock
                    {
                        Text = param.Key.Split('_')[0],
                        Width = 150,
                        VerticalAlignment = VerticalAlignment.Center
                    });

                    switch (param.Type)
                    {
                        case ParameterType.Integer:
                            var intBox = new TextBox
                            {
                                Text = param.Value,
                                Width = 200,
                                Tag = param
                            };
                            intBox.PreviewTextInput += Numeric_PreviewTextInput;
                            intBox.TextChanged += String_TextChanged;

                            stack.Children.Add(intBox);
                            break;

                        case ParameterType.Path:
                            var pathBox = new TextBox
                            {
                                Text = param.Value,
                                Width = 180,
                                Tag = param
                            };
                            var browseBtn = new Button
                            {
                                Content = "...",
                                Margin = new Thickness(5, 0, 0, 0),
                                Tag = param
                            };
                            browseBtn.Click += BrowseButton_Click;
                            stack.Children.Add(pathBox);
                            stack.Children.Add(browseBtn);
                            break;

                        default:
                            var stringBox = new TextBox
                            {
                                Text = param.Value,
                                Width = 200,
                                Tag = param
                            };
                            stringBox.TextChanged += String_TextChanged;
                            stack.Children.Add(stringBox);
                            break;
                    }

                    ParametersPanel.Children.Add(stack);
                }
            }
        }

        private void Numeric_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is PluginParameter param)
            {
                string selectedPath = string.Empty;

                var fileDialog = new OpenFileDialog();
                if (fileDialog.ShowDialog() == true)
                {
                    selectedPath = fileDialog.FileName;
                }

                if (!string.IsNullOrEmpty(selectedPath))
                {
                    param.Value = selectedPath;
                    if (btn.Parent is StackPanel panel && panel.Children[1] is TextBox textBox)
                    {
                        textBox.Text = selectedPath;
                    }
                }
            }
        }

        private void String_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb && tb.Tag is PluginParameter param)
            {
                param.Value = tb.Text;
            }
        }


    }

    public class PluginSection
    {
        public string Name { get; set; }
        public ObservableCollection<PluginParameter> Parameters { get; set; }
    }

    public class PluginParameter
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public ParameterType Type { get; set; }
    }

    public enum ParameterType
    {
        String,
        Integer,
        Path
    }
}
