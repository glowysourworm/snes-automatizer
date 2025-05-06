using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using snes_automatizer.Extension;

namespace snes_automatizer.Command
{
    public class SubProcessCommand
    {
        public string ExeFilePath { get; set; }
        public SimpleList<string> Arguments { get; set; }
        public bool Completed { get; set; }
        public bool ThrewError { get; set; }
        public string GetFullCommandLine()
        {
            return this.ExeFilePath + " " + string.Join(' ', this.Arguments);
        }
        public SubProcessCommand()
        {
            this.ExeFilePath = "";
            this.Arguments = new SimpleList<string>();
        }
    }
}
