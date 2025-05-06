using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using Microsoft.Win32;

using Newtonsoft.Json;

using snes_automatizer.Command;
using snes_automatizer.Extension;

namespace snes_automatizer
{
    public partial class MainWindow : Window
    {
        private readonly ViewModel _viewModel;
        private readonly Compiler _compiler;

        // Null valued until it is verified by the compiler (pre-run)
        private Compilation? _compilation;
        private ValidatedSettings? _validatedSettings;

        public MainWindow()
        {
            InitializeComponent();


            _viewModel = new ViewModel();
            _compiler = new Compiler();
            _compilation = null;
            _validatedSettings = null;

            this.DataContext = _viewModel;

            _viewModel.PropertyChanged += OnViewModelChanged;
            _viewModel.Settings.PropertyChanged += OnViewModelChanged;

            _compiler.ValidationMessageEvent += Log;
            _compiler.RunMessageEvent += Log;

            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                var configFile = args[1];
                if (!string.IsNullOrEmpty(configFile))
                {
                    OpenConfiguration(configFile);
                }
            }

            // Sort descriptors for the project code files list box
            this.ProjectFilesLB.Items.SortDescriptions.Clear();
            this.ProjectFilesLB.Items.SortDescriptions.Add(new SortDescription("Order", ListSortDirection.Ascending));
        }

        private void OnViewModelChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // This won't recurse (becuase the ObservableCollection itself doesn't change for adding the log message)
            Log("Settings Modified (" + e.PropertyName + ")");

            // We have to filter this event to prevent extra calls during sorting
            if (e.PropertyName == "ProjectFolder" ||
                e.PropertyName == "PVSNESLIBFolder")
            {
                ReloadFiles();
                Log("Code files (*.c, *.asm) reloaded from disk");
            }
        }

        private void Log(string message)
        {
            string finalMessage = DateTime.Now.ToString("yyyy-MM-dd  hh:mm:ss") + ":  " + message;

            _viewModel.OutputMessages.Add(finalMessage);

            this.OutputLB.ScrollIntoView(this.OutputLB.Items[this.OutputLB.Items.Count - 1]);

            /*
            _viewModel.OutputMessages.Insert(0, finalMessage);

            if (_viewModel.OutputMessages.Count > 1000)
                _viewModel.OutputMessages.RemoveAt(_viewModel.OutputMessages.Count - 1);
            */
        }

        private void Log(string format, params object[] parameters)
        {
            Log(string.Format(format, parameters));
        }

        private void ReloadFiles()
        {
            try
            {
                // Code Files:  Must merge what is on disk with those currently in memory
                //

                var files = new List<string>();
                var index = 0;

                foreach (var file in Directory.GetFileSystemEntries(_viewModel.Settings.ProjectFolder, "*.c", SearchOption.AllDirectories))
                {
                    files.Add(file);

                    if (!_viewModel.Settings.CodeFiles.Any(x => x.Path == file))
                    {
                        _viewModel.Settings.CodeFiles.Add(new FileItem(file, index, CodeFileType.C));
                    }

                    index++;
                }
                foreach (var file in Directory.GetFileSystemEntries(_viewModel.Settings.ProjectFolder, "*.asm", SearchOption.AllDirectories))
                {
                    files.Add(file);

                    if (!_viewModel.Settings.CodeFiles.Any(x => x.Path == file))
                    {
                        _viewModel.Settings.CodeFiles.Add(new FileItem(file, index, CodeFileType.Assembler));
                    }

                    index++;
                }

                // Prune
                //
                _viewModel.Settings.CodeFiles.RemoveWhere((x, index) => !files.Contains(x.Path));
                _viewModel.Settings.CodeFiles.Sort(x => x.Order);

                // Assign New Order Number
                //
                for (int fileIndex = 0; fileIndex <  _viewModel.Settings.CodeFiles.Count; fileIndex++)
                {
                    _viewModel.Settings.CodeFiles[fileIndex].Order = fileIndex;
                }
            }
            catch (Exception ex)
            {
                Log("There was an error reading files");
                Log(ex.ToString());
            }
        }

        private string OpenFile(string filter)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = filter;

            if (dialog.ShowDialog(this) == true)
            {
                return dialog.FileName;
            }

            return "";
        }
        private string SaveFile(string filter)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = filter;

            if (dialog.ShowDialog(this) == true)
            {
                return dialog.FileName;
            }

            return "";
        }

        private string SelectDirectory()
        {
            var dialog = new OpenFolderDialog();

            if (dialog.ShowDialog(this) == true)
            {
                return dialog.FolderName;
            }

            return "";
        }

        private void ProjectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var result = SelectDirectory();
            if (!string.IsNullOrEmpty(result))
            {
                _viewModel.Settings.ProjectFolder = result;
            }
        }

        private void PVSNESLIBFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var result = SelectDirectory();
            if (!string.IsNullOrEmpty(result))
            {
                _viewModel.Settings.PVSNESLIBFolder = result;
            }
        }

        private void OpenConfiguration(string fileName)
        {
            try
            {
                var json = System.IO.File.ReadAllText(fileName);
                var settings = JsonConvert.DeserializeObject<Settings>(json);

                if (settings != null)
                {
                    _viewModel.Settings.PropertyChanged -= OnViewModelChanged;
                    _viewModel.Settings = settings;
                    _viewModel.Settings.PropertyChanged += OnViewModelChanged;

                    Log("Configuration file loaded:  {0}", fileName);

                    ReloadFiles();
                }
                else
                {
                    Log("Error opening settings file");
                }
            }
            catch (Exception ex)
            {
                Log("There was an error opening configuration {0}", fileName);
                Log(ex.ToString());
            }
        }
        private void SaveConfiguration(string fileName, string json)
        {

            try
            {
                System.IO.File.WriteAllText(fileName, json);
                Log("Configuration file saved:  {0}", fileName);
            }
            catch (Exception ex)
            {
                Log("There was an saving configuration {0}", fileName);
                Log(ex.ToString());
            }
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Settings = new Settings();
        }
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var fileName = OpenFile("Json Files | *.json");

            if (!string.IsNullOrEmpty(fileName))
            {
                OpenConfiguration(fileName);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var json = JsonConvert.SerializeObject(_viewModel.Settings, Formatting.Indented);
            var fileName = SaveFile("Json Files | *.json");

            if (!string.IsNullOrEmpty(fileName))
            {
                SaveConfiguration(fileName, json);
            }
        }

        private void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            Log("Validating Compilation Settings...");
            _validatedSettings = _compiler.Validate(_viewModel.Settings);
            _compilation = null;

            Log("Validation Complete:  {0}", _validatedSettings.ValidationPassed ? "Success" : "Failure");
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (_validatedSettings == null ||
                !_validatedSettings.ValidationPassed)
            {
                Log("Setting not validated. Please validate before running.");
            }
            else
            {
                // Get files from list box in the order they were arranged
                var files = new List<string>();
                foreach (var item in _viewModel.Settings.CodeFiles.OrderBy(x => x.Order))
                {
                    if (item.Included)
                    {
                        files.Add(item.Path);
                    }
                }

                Log("Creating compilation commands...");
                _compilation = _compiler.PrepareRun(_validatedSettings, files);

                if (MessageBox.Show("Execute Compilation?", "SNES Compiler", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
                {
                    Log("Running Compiler...");
                    if (_compiler.ExecuteRun(_compilation, _validatedSettings.Settings.ProjectFolder))
                    {
                        Log("Compilation Succeeded!");
                    }
                    else
                    {
                        Log("Compilation Error. See log for details.");
                    }
                }
            }
        }

        private void OrderDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ProjectFilesLB.SelectedIndex >= 0 &&
                this.ProjectFilesLB.SelectedIndex < this.ProjectFilesLB.Items.Count - 1)
            {
                var index = this.ProjectFilesLB.SelectedIndex;

                var swap = _viewModel.Settings.CodeFiles[index];
                var swapOther = _viewModel.Settings.CodeFiles[index + 1];

                var order = swap.Order;
                swap.Order = swapOther.Order;
                swapOther.Order = order;

                _viewModel.Settings.CodeFiles.Sort(x => x.Order);
            }
        }

        private void OrderUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ProjectFilesLB.SelectedIndex >= 1)
            {
                var index = this.ProjectFilesLB.SelectedIndex;

                var swap = _viewModel.Settings.CodeFiles[index];
                var swapOther = _viewModel.Settings.CodeFiles[index - 1];

                var order = swap.Order;
                swap.Order = swapOther.Order;
                swapOther.Order = order;

                _viewModel.Settings.CodeFiles.Sort(x => x.Order);
            }
        }

        private void OutputMessage_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;

            if (item != null)
            {
                var message = item.Tag as string;

                // Set Clipboard
                Clipboard.SetText(message);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OutputMessages.Clear();
        }
    }
}