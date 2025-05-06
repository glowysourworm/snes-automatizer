using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;

using snes_automatizer.Extension;

namespace snes_automatizer.Model
{
    public class OutputMessage : ViewModelBase
    {
        DateTime _timestamp;
        string _message;
        MessageSeverity _severity;

        public MessageSeverity Severity
        {
            get { return _severity; }
            set { RaiseAndSetIfChanged(ref _severity, value); }
        }
        public string Message
        {
            get { return _message; }
            set { RaiseAndSetIfChanged(ref _message, value); }
        }
        public DateTime Timestamp
        {
            get { return _timestamp; }
            set { RaiseAndSetIfChanged(ref _timestamp, value); }
        }

        public OutputMessage()
        {
            this.Severity = MessageSeverity.Info;
            this.Message = "";
            this.Timestamp = DateTime.Now;
        }
    }

    public class ViewModel : ViewModelBase
    {
        Settings _settings;
        ObservableCollection<OutputMessage> _outputMessages;

        public Settings Settings
        {
            get { return _settings; }
            set { RaiseAndSetIfChanged(ref _settings, value); }
        }

        public ObservableCollection<OutputMessage> OutputMessages
        {
            get { return _outputMessages; }
            set { RaiseAndSetIfChanged(ref _outputMessages, value); }
        }

        public ViewModel()
        {
            this.Settings = new Settings();
            this.OutputMessages = new ObservableCollection<OutputMessage>();
        }
    }
}
