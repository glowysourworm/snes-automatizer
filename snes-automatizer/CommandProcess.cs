using System.Diagnostics;

namespace snes_automatizer
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
                process.Start();
                process.StandardInput.WriteLine("Process Started:  " + _command.ExeFilePath);
                process.StandardInput.Flush();
                process.StandardInput.Close();

                process.WaitForExit();

                while (process.StandardOutput.Peek() > 0)
                {
                    var message = process.StandardOutput.ReadLine();
                    if (!string.IsNullOrEmpty(message))
                    {
                        messageCallback(message);
                    }
                }

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
