using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

using static System.Net.WebRequestMethods;

namespace snes_automatizer
{
    public delegate void SimpleEventHandler<T>(T sender);

    public class ValidatedSettings : ViewModelBase
    {
        public const string DEV_KIT_SNES = "devkitsnes";
        public const string DEV_KIT_SNES_INCLUDE = "devkitsnes/include";
        public const string TOOLS = "tools";
        public const string ASM_COMPILER = "bin/wla-65816.exe";
        public const string C_COMPILER = "bin/816-tcc.exe";
        public const string LINKER = "bin/wlalink.exe";
        public const string OPTIMIZER = "bin/816-opt.exe";
        public const string CONSTIFIER = "bin/constify.exe";
        public const string HIROM_FAST = "lib/HiROM_FastROM";
        public const string LOROM_FAST = "lib/LoROM_FastROM";
        public const string HIROM_SLOW = "lib/HiROM_SlowROM";
        public const string LOROM_SLOW = "lib/LoROM_SlowROM";

        public bool ValidationPassed { get; set; }
        public Settings Settings { get; set; }

        /// <summary>
        /// Memory Map Folder (calculated based on HIROM / LOROM / FAST / SLOW settings
        /// </summary>
        public string MemoryMapFolder { get; set; }
        /// <summary>
        /// Tools folder from the base pvsneslib path
        /// </summary>
        public string ToolsFolder { get; set; }
        /// <summary>
        /// Devkit folder from the base pvsneslib (devkitsnes)
        /// </summary>
        public string DevKitFolder { get; set; }

        /// <summary>
        /// Devkit includes folder (devkitsnes)
        /// </summary>
        public string DevKitIncludeFolder { get; set; }


        /// <summary>
        /// Compiler path for 816-tcc.exe
        /// </summary>
        public string CCompilerPath { get; set; }
        /// <summary>
        /// Compiler path for wla-65816.exe
        /// </summary>
        public string AsmCompilerPath { get; set; }
        /// <summary>
        /// Linker path for wlalink.exe
        /// </summary>
        public string LinkerPath { get; set; }
        /// <summary>
        /// Path to optimizer 816-opt.exe
        /// </summary>
        public string OptimizerPath { get; set; }

        /// <summary>
        /// Path to constify.exe
        /// </summary>
        public string ConstifierPath { get; set; }

        public ValidatedSettings(Settings settings)
        {
            this.Settings = settings;
            this.ValidationPassed = false;

            // Base
            this.DevKitFolder = Path.Combine(settings.PVSNESLIBFolder, DEV_KIT_SNES);
            this.DevKitIncludeFolder = Path.Combine(settings.PVSNESLIBFolder, DEV_KIT_SNES_INCLUDE);
            this.ToolsFolder = Path.Combine(settings.PVSNESLIBFolder, TOOLS);

            // Base -> Devkit
            this.AsmCompilerPath = Path.Combine(this.DevKitFolder, ASM_COMPILER);
            this.CCompilerPath = Path.Combine(this.DevKitFolder, C_COMPILER);
            this.LinkerPath = Path.Combine(this.DevKitFolder, LINKER);

            // Base -> Tools
            this.OptimizerPath = Path.Combine(this.ToolsFolder, OPTIMIZER);
            this.ConstifierPath = Path.Combine(this.ToolsFolder, CONSTIFIER);

            if (settings.MemoryMap == MemoryMapSettings.HIROM)
            {
                this.MemoryMapFolder = settings.Speed == SpeedSettings.FAST ? HIROM_FAST : HIROM_SLOW;
            }
            else
            {
                this.MemoryMapFolder = settings.Speed == SpeedSettings.FAST ? LOROM_FAST : LOROM_SLOW;
            }
        }
    }

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

    public class Compilation
    {
        public CompilationCommandSet CommandSet { get; set; }
        public Compilation(CompilationCommandSet commandSet)
        {
            this.CommandSet = commandSet;
        }
    }

    public class Compiler
    {
        public event SimpleEventHandler<string> RunMessageEvent;
        public event SimpleEventHandler<string> ValidationMessageEvent;

        public const string LINK_FILE_NAME = "linkfile";
        public const string OUTPUT_FILE_NAME = "output.sfc";

        public Compiler()
        {
        }
        public ValidatedSettings Validate(Settings settings)
        {
            // Set calculated paths
            var result = new ValidatedSettings(settings);
            result.ValidationPassed = true;

            // Check that files / folders exist
            if (string.IsNullOrEmpty(settings.ProjectFolder) || Directory.Exists(settings.ProjectFolder))
            {
                OnValidationMessage("Project Folder must be set to your source code base directory");
                result.ValidationPassed = false;
                return result;
            }
            if (string.IsNullOrEmpty(settings.PVSNESLIBFolder) || Directory.Exists(settings.PVSNESLIBFolder))
            {
                OnValidationMessage("pvsneslib Folder must be set to base directory for pvsneslib");
                result.ValidationPassed = false;
                return result;
            }
            if (string.IsNullOrEmpty(result.DevKitFolder) || Directory.Exists(result.DevKitFolder))
            {
                OnValidationMessage("devkitsnes Folder must be set to base directory for devkitsnes");
                result.ValidationPassed = false;
                return result;
            }
            if (string.IsNullOrEmpty(result.DevKitIncludeFolder) || Directory.Exists(result.DevKitIncludeFolder))
            {
                OnValidationMessage("devkitsnes/include Folder be available for devkitsnes");
                result.ValidationPassed = false;
                return result;
            }
            if (string.IsNullOrEmpty(result.ToolsFolder) || Directory.Exists(result.ToolsFolder))
            {
                OnValidationMessage("tools Folder must be set to tools directory of pvsneslib");
                result.ValidationPassed = false;
                return result;
            }

            // Compilers
            if (string.IsNullOrEmpty(result.CCompilerPath) || Path.Exists(result.CCompilerPath))
            {
                OnValidationMessage("c compiler path is invalid");
                result.ValidationPassed = false;
                return result;
            }
            if (string.IsNullOrEmpty(result.AsmCompilerPath) || Path.Exists(result.AsmCompilerPath))
            {
                OnValidationMessage("asm compiler path is invalid");
                result.ValidationPassed = false;
                return result;
            }
            if (string.IsNullOrEmpty(result.LinkerPath) || Path.Exists(result.LinkerPath))
            {
                OnValidationMessage("linker path is invalid");
                result.ValidationPassed = false;
                return result;
            }
            if (string.IsNullOrEmpty(result.OptimizerPath) || Path.Exists(result.OptimizerPath))
            {
                OnValidationMessage("optimizer path is invalid");
                result.ValidationPassed = false;
                return result;
            }
            if (string.IsNullOrEmpty(result.ConstifierPath) || Path.Exists(result.ConstifierPath))
            {
                OnValidationMessage("constifier path is invalid");
                result.ValidationPassed = false;
                return result;
            }

            // Memory Map
            if (string.IsNullOrEmpty(result.MemoryMapFolder) || Directory.Exists(result.MemoryMapFolder))
            {
                OnValidationMessage("Memory Map folder does not exist or is invalid");
                result.ValidationPassed = false;
                return result;
            }

            return result;

        }
        public Compilation PrepareRun(ValidatedSettings settings, IEnumerable<string> files)
        {
            var ccommands = new List<CFileCommandSet>();
            var asmCommands = new List<SubProcessCommand>();
            var linkFilePath = Path.Combine(settings.Settings.ProjectFolder, LINK_FILE_NAME);
            var linkFile = new StringBuilder();
            var outputFile = Path.Combine(settings.Settings.ProjectFolder, OUTPUT_FILE_NAME);

            // C-Files:  For each c file, compile the following commands:  compile -> optimize -> constify -> assemble
            //
            foreach (var file in files.Where(x => x.EndsWith(".c")))
            {
                var compile = new SubProcessCommand();

                // TODO: What are these files?
                var psFile = file.Replace(".c", ".ps");
                var aspFile = file.Replace(".c", ".asp");
                var asmFile = file.Replace(".c", ".asm");
                var objFile = file.Replace(".c", ".obj");

                // TODO: MAKE CONST + SOME COMMENTS FOR COMMAND OPTIONS!
                if (settings.Settings.MemoryMap == MemoryMapSettings.HIROM)
                {
                    compile.ExeFilePath = settings.CCompilerPath;

                    if (settings.Settings.Speed == SpeedSettings.FAST)
                    {
                        compile.Arguments.AddRange("-I", settings.DevKitIncludeFolder, "-Wall", "-c", file, "-H", "-F", "-o", psFile);
                    }
                    else
                    {
                        compile.Arguments.AddRange("-I", settings.DevKitIncludeFolder, "-Wall", "-c", file, "-H", "-o", psFile);
                    }
                }
                else
                {
                    if (settings.Settings.Speed == SpeedSettings.FAST)
                    {
                        compile.Arguments.AddRange("-I", settings.DevKitIncludeFolder, "-Wall", "-c", file, "-F", "-o", psFile);
                    }
                    else
                    {
                        compile.Arguments.AddRange("-I", settings.DevKitIncludeFolder, "-Wall", "-c", file, "-o", psFile);
                    }
                }

                var optimize = new SubProcessCommand();
                var constify = new SubProcessCommand();
                var assemble = new SubProcessCommand();

                optimize.ExeFilePath = settings.OptimizerPath;
                constify.ExeFilePath = settings.ConstifierPath;
                assemble.ExeFilePath = settings.AsmCompilerPath;

                optimize.Arguments.Add(psFile);
                constify.Arguments.AddRange(file, aspFile, asmFile);
                assemble.Arguments.AddRange("-d", "-s", "-x", "-o", objFile, asmFile);

                ccommands.Add(new CFileCommandSet(compile, optimize, constify, assemble));
            }

            // Asm Files:  For each assembler file, run the assembler compiler
            //
            foreach (var file in files.Where(x => x.EndsWith(".asm")))
            {
                var command = new SubProcessCommand();
                command.ExeFilePath = file;
                command.Arguments.AddRange("-d", "-s", "-x", "-o", file.Replace(".asm", ".obj"), file);

                asmCommands.Add(command);
            }

            // Link File:  Create link file from the .obj files (for .asm + .c files), GIVEN IN THE ORDER THEY WERE PRESENTED,
            //             followed by the MemoryMapFolder (lib directory) .obj files.
            //

            // Header
            linkFile.AppendLine("[objects]");

            // Our .obj files
            foreach (var file in files)
            {
                linkFile.AppendLine(file.Replace(".c", ".obj").Replace(".asm", ".obj"));
            }

            // Lib .obj files
            foreach (var file in Directory.GetFiles(settings.MemoryMapFolder, "*.obj"))
            {
                linkFile.AppendLine(file);
            }

            // Write link file to disk
            System.IO.File.WriteAllText(linkFilePath, linkFile.ToString());

            var linkCommand = new SubProcessCommand();
            linkCommand.ExeFilePath = settings.LinkerPath;
            linkCommand.Arguments.AddRange("-d", "-s", "-c", "-v", "-A", "-L", settings.MemoryMapFolder, linkFilePath, settings.Settings.ProjectFolder);

            return new Compilation(new CompilationCommandSet(ccommands, asmCommands, linkCommand));
        }
        public bool ExecuteRun(Compilation compilation)
        {
            // Procedure:  See automatizer.py: https://github.com/BrunoRNS/SNES-IDE/blob/main/libs/pvsneslib/devkitsnes/automatizer.py
            //
            // 1) Run C-File Commands:   Compile -> Optimize -> Constify -> Assemble
            // 2) Run Asm-File Commands: Compile
            // 3) Run Linker
            // 
            foreach (var ccommand in compilation.CommandSet.CFileCommands)
            {
                var compileProcess = new CommandProcess(ccommand.Compile);
                var optimizeProcess = new CommandProcess(ccommand.Optimize);
                var constifyProcess = new CommandProcess(ccommand.Constify);
                var assembleProcess = new CommandProcess(ccommand.Assemble);

                // Run Commands Synchronously
                if (!compileProcess.Run(message => OnRunMessage(message)))
                {
                    OnRunMessage("Compilation Process Error:  " + ccommand.Compile.GetFullCommandLine());
                    return false;
                }
                if (!optimizeProcess.Run(message => OnRunMessage(message)))
                {
                    OnRunMessage("Optimizer Process Error:  " + ccommand.Optimize.GetFullCommandLine());
                    return false;
                }
                if (!constifyProcess.Run(message => OnRunMessage(message)))
                {
                    OnRunMessage("Constify Process Error:  " + ccommand.Constify.GetFullCommandLine());
                    return false;
                }
                if (!assembleProcess.Run(message => OnRunMessage(message)))
                {
                    OnRunMessage("Assembler Process Error:  " + ccommand.Assemble.GetFullCommandLine());
                    return false;
                }
            }

            foreach (var asmCommand in compilation.CommandSet.AssemblerCommands)
            {
                var process = new CommandProcess(asmCommand);

                if (!process.Run(message => OnRunMessage(message)))
                {
                    OnRunMessage("Assembler Process Error:  " + asmCommand.GetFullCommandLine());
                    return false;
                }
            }

            var linker = new CommandProcess(compilation.CommandSet.LinkCommand);

            if (!linker.Run(message => OnRunMessage(message)))
            {
                OnRunMessage("Linker Process Error:  " + compilation.CommandSet.LinkCommand.GetFullCommandLine());
                return false;
            }

            OnRunMessage("Compilation Complete!");

            return true;
        }

        private void OnValidationMessage(string message)
        {
            if (this.ValidationMessageEvent != null)
                this.ValidationMessageEvent(message);
        }
        private void OnRunMessage(string message) 
        {
            if (this.RunMessageEvent != null)
                this.RunMessageEvent(message);
        }
    }
}
