using System.Diagnostics;
using System.Windows;

namespace snes_automatizer.Command
{
    public class CommandProcess
    {
        private readonly SubProcessCommand _command;

        public CommandProcess(SubProcessCommand command)
        {
            _command = command;
        }

        public bool Run(SimpleEventHandler<string> messageCallback)
        {
            try
            {
                var process = new Process();
                process.StartInfo.FileName = _command.ExeFilePath;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.Arguments = string.Join(' ', _command.Arguments);
                process.OutputDataReceived += (sender, args) =>
                {
                    // Must pass this to the dispacther
                    Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, () =>
                    {
                        messageCallback(args.Data ?? "");
                    });
                };
                process.Start();
                process.BeginOutputReadLine();
                process.StandardInput.WriteLine("Process Started:  " + _command.ExeFilePath);
                process.StandardInput.Flush();
                process.StandardInput.Close();

                process.WaitForExit();
                
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                messageCallback("Error Running Process:  " + ex.ToString());

                return false;
            }
        }
    }
}
