using System.IO;
using System.Text;
using System.Windows;

using Microsoft.Win32;

using Newtonsoft.Json;

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
        }

        private void OnViewModelChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // This won't recurse (becuase the ObservableCollection itself doesn't change for adding the log message)
            Log("Settings Modified (" + e.PropertyName + ")");
            ReloadFiles();
            Log("Code files (*.c, *.asm) reloaded from disk");
        }

        private void Log(string message)
        {
            string finalMessage = DateTime.Now.ToString("yyyy-MM-dd  hh:mm:ss") + "\t" + message;

            _viewModel.OutputMessages.Insert(0, finalMessage);

            if (_viewModel.OutputMessages.Count > 1000)
                _viewModel.OutputMessages.RemoveAt(_viewModel.OutputMessages.Count - 1);
        }

        private void Log(string format, params object[] parameters)
        {
            Log(string.Format(format, parameters));
        }

        private void ReloadFiles()
        {
            try
            {
                _viewModel.CFiles.Clear();
                _viewModel.AssemblerFiles.Clear();

                _viewModel.CFiles.AddRange(Directory.GetFileSystemEntries(_viewModel.Settings.ProjectFolder, "*.c", SearchOption.AllDirectories));
                _viewModel.AssemblerFiles.AddRange(Directory.GetFileSystemEntries(_viewModel.Settings.ProjectFolder, "*.asm", SearchOption.AllDirectories));

                this.ProjectFilesLB.Items.Clear();

                // Add these manually; and let the user re-order them
                foreach (var file in _viewModel.CFiles)
                {
                    this.ProjectFilesLB.Items.Add(new FileItem(file));
                }
                foreach (var file in _viewModel.AssemblerFiles)
                {
                    this.ProjectFilesLB.Items.Add(new FileItem(file));
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
                    _viewModel.Settings = settings;
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
            var json = JsonConvert.SerializeObject(_viewModel.Settings);
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
                Log("Validation Failed. Please see log for errors.");
            }
            else
            {
                // Get files from list box in the order they were arranged
                var files = new List<string>();
                foreach (FileItem item in this.ProjectFilesLB.Items)
                {
                    if (item.Included)
                    {
                        files.Add(item.Path);
                    }
                }

                Log("Creating compilation commands...");
                _compilation = _compiler.PrepareRun(_validatedSettings, files);

                var builder = new StringBuilder();
                foreach (var command in _compilation.CommandSet.CFileCommands)
                {
                    builder.AppendLine(command.Compile.GetFullCommandLine());
                    builder.AppendLine(command.Optimize.GetFullCommandLine());
                    builder.AppendLine(command.Constify.GetFullCommandLine());
                    builder.AppendLine(command.Assemble.GetFullCommandLine());
                }
                foreach (var command in _compilation.CommandSet.AssemblerCommands)
                {
                    builder.AppendLine(command.GetFullCommandLine());
                }

                builder.AppendLine(_compilation.CommandSet.LinkCommand.GetFullCommandLine());

                if (MessageBox.Show(builder.ToString()) == MessageBoxResult.OK)
                {
                    Log("Running Compiler...");
                    if (_compiler.ExecuteRun(_compilation))
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
    }
}