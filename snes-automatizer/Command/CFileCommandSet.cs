using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snes_automatizer.Command
{
    public class CFileCommandSet
    {
        public SubProcessCommand Compile { get; set; }
        public SubProcessCommand Optimize { get; set; }
        public SubProcessCommand Constify { get; set; }
        public SubProcessCommand Assemble { get; set; }

        public CFileCommandSet(SubProcessCommand compile, SubProcessCommand optimize, SubProcessCommand constify, SubProcessCommand assemble)
        {
            this.Compile = compile;
            this.Optimize = optimize;
            this.Constify = constify;
            this.Assemble = assemble;
        }
    }
}
