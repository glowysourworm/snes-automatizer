using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snes_automatizer.Command
{
    public class Compilation
    {
        public CompilationCommandSet CommandSet { get; set; }
        public Compilation(CompilationCommandSet commandSet)
        {
            this.CommandSet = commandSet;
        }
    }
}
