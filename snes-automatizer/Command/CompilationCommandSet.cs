using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snes_automatizer.Command
{
    public class CompilationCommandSet
    {
        public List<CFileCommandSet> CFileCommands { get; set; }

        public List<SubProcessCommand> AssemblerCommands { get; set; }

        public SubProcessCommand LinkCommand { get; set; }

        public CompilationCommandSet(IEnumerable<CFileCommandSet> cFileCommands, List<SubProcessCommand> assemblerCommands, SubProcessCommand linkCommand)
        {
            this.CFileCommands = new List<CFileCommandSet>(cFileCommands);
            this.AssemblerCommands = assemblerCommands;
            this.LinkCommand = linkCommand;
        }
    }
}
