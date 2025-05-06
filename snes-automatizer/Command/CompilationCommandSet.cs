namespace snes_automatizer.Command
{
    public class CompilationCommandSet
    {
        public List<CFileCommandSet> CFileCommands { get; set; }

        public List<SubProcessCommand> AssemblerCommands { get; set; }
        public List<SubProcessCommand> ResourceCommands { get; set; }

        public SubProcessCommand LinkCommand { get; set; }

        public CompilationCommandSet(IEnumerable<CFileCommandSet> cFileCommands,
            List<SubProcessCommand> assemblerCommands, List<SubProcessCommand> resourceCommands, SubProcessCommand linkCommand)
        {
            this.CFileCommands = new List<CFileCommandSet>(cFileCommands);
            this.AssemblerCommands = assemblerCommands;
            this.ResourceCommands = resourceCommands;
            this.LinkCommand = linkCommand;
        }
    }
}
