using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;

using snes_automatizer.Extension;

namespace snes_automatizer
{
    // TODO: Bring in some documentation for high-level compiler settings
    public enum MemoryMapSettings
    {
        [Display(Name = "LOROM", Description = "TODO")]
        LOROM = 0,

        [Display(Name = "HIROM", Description = "TODO")]
        HIROM = 1
    }

    // TODO: Bring in some documentation for high-level compiler settings
    public enum SpeedSettings
    {
        [Display(Name = "FAST", Description = "TODO")]
        FAST = 0,
        [Display(Name = "SLOW", Description = "TODO")]
        SLOW = 1
    }

    public enum CodeFileType
    {
        Assembler = 0,
        C = 1
    }

    public class FileItem : ViewModelBase
    {
        public static readonly DependencyProperty OrderProperty = DependencyProperty.Register("Order", typeof(int), typeof(FileItem));

        string _path;
        bool _included;
        int _order;
        CodeFileType _type;

        public string Path
        {
            get { return _path; }
            set { this.RaiseAndSetIfChanged(ref _path, value); }
        }
        public bool Included
        {
            get { return _included; }
            set { this.RaiseAndSetIfChanged(ref _included, value); }
        }
        public int Order
        {
            get { return _order; }
            set { this.RaiseAndSetIfChanged(ref _order, value); }
        }
        public CodeFileType Type
        {
            get { return _type; }
            set { this.RaiseAndSetIfChanged(ref _type, value); }
        }
        public FileItem()
        {
            _included = true;
            _path = "";
            _order = -1;
            _type = CodeFileType.Assembler;
        }
        public FileItem(string path, int order, CodeFileType type)
        {
            _path = path;
            _included = true;
            _order = order;
            _type = type;
        }

        public override string ToString()
        {
            return _path;
        }
    }

    public class Settings : ViewModelBase
    {
        string _projectFolder;
        string _pvSnesLibFolder;
        MemoryMapSettings _memoryMap;
        SpeedSettings _speed;
        SimpleObservableCollection<FileItem> _codeFiles;

        /// <summary>
        /// Base folder for your project code. This program will search through all your files to look
        /// for compilable files for the project.
        /// </summary>
        public string ProjectFolder
        {
            get { return _projectFolder; }
            set { this.RaiseAndSetIfChanged(ref _projectFolder, value); }
        }

        /// <summary>
        /// Base folder for the pvsneslib development "IDE". Sub-files and folder locations will be part of validation.
        /// </summary>
        public string PVSNESLIBFolder
        {
            get { return _pvSnesLibFolder; }
            set { this.RaiseAndSetIfChanged(ref _pvSnesLibFolder, value); }
        }

        public MemoryMapSettings MemoryMap
        {
            get { return _memoryMap; }
            set { this.RaiseAndSetIfChanged(ref _memoryMap, value); }
        }
        public SpeedSettings Speed
        {
            get { return _speed; }
            set { this.RaiseAndSetIfChanged(ref _speed, value); }
        }

        public SimpleObservableCollection<FileItem> CodeFiles
        {
            get { return _codeFiles; }
            set { this.RaiseAndSetIfChanged(ref _codeFiles, value); }
        }

        public Settings()
        {
            this.ProjectFolder = "";
            this.PVSNESLIBFolder = "";
            this.CodeFiles = new SimpleObservableCollection<FileItem>();

            this.CodeFiles.ItemPropertyChanged += CodeFiles_ItemPropertyChanged;
        }

        private void CodeFiles_ItemPropertyChanged(SimpleObservableCollection<FileItem> sender, FileItem arg1, System.ComponentModel.PropertyChangedEventArgs arg2)
        {
            this.OnPropertyChanged("CodeFiles");
        }
    }

    public class ViewModel : ViewModelBase
    {
        Settings _settings;
        ObservableCollection<string> _outputMessages;

        public Settings Settings
        {
            get { return _settings; }
            set { this.RaiseAndSetIfChanged(ref _settings, value); }
        }

        public ObservableCollection<string> OutputMessages
        {
            get { return _outputMessages; }
            set { this.RaiseAndSetIfChanged(ref _outputMessages, value); }
        }



        public ViewModel()
        {
            this.Settings = new Settings();
            this.OutputMessages = new ObservableCollection<string>();
        }
    }
}
