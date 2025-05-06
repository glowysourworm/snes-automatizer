using System.IO;
using System.Text;

using snes_automatizer.Command;
using snes_automatizer.Extension;
using snes_automatizer.Model;
using snes_automatizer.Properties;

using Settings = snes_automatizer.Model.Settings;

namespace snes_automatizer
{
    public class Compiler
    {
        public event SimpleEventHandler<string, MessageSeverity> RunMessageEvent;
        public event SimpleEventHandler<string, MessageSeverity> ValidationMessageEvent;

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
            if (string.IsNullOrEmpty(settings.ProjectFolder) || !Directory.Exists(settings.ProjectFolder))
            {
                OnValidationMessage("Project Folder must be set to your source code base directory", MessageSeverity.Error);
                result.ValidationPassed = false;
                return result;
            }
            if (string.IsNullOrEmpty(settings.PVSNESLIBFolder) || !Directory.Exists(settings.PVSNESLIBFolder))
            {
                OnValidationMessage("pvsneslib Folder must be set to base directory for pvsneslib", MessageSeverity.Error);
                result.ValidationPassed = false;
                return result;
            }
            if (string.IsNullOrEmpty(result.DevKitFolder) || !Directory.Exists(result.DevKitFolder))
            {
                OnValidationMessage("devkitsnes Folder must be set to base directory for devkitsnes", MessageSeverity.Error);
                result.ValidationPassed = false;
                return result;
            }
            if (string.IsNullOrEmpty(result.DevKitIncludeFolder) || !Directory.Exists(result.DevKitIncludeFolder))
            {
                OnValidationMessage("devkitsnes/include Folder be available for devkitsnes", MessageSeverity.Error);
                result.ValidationPassed = false;
                return result;
            }
            if (string.IsNullOrEmpty(result.ToolsFolder) || !Directory.Exists(result.ToolsFolder))
            {
                OnValidationMessage("Tools Folder must be set to tools directory of pvsneslib", MessageSeverity.Error);
                result.ValidationPassed = false;
                return result;
            }

            // Compilers
            if (string.IsNullOrEmpty(result.CCompilerPath) || !Path.Exists(result.CCompilerPath))
            {
                OnValidationMessage("C compiler path is invalid: {0}", MessageSeverity.Error, result.CCompilerPath);
                result.ValidationPassed = false;
                return result;
            }
            if (string.IsNullOrEmpty(result.AsmCompilerPath) || !Path.Exists(result.AsmCompilerPath))
            {
                OnValidationMessage("Assembler compiler path is invalid: {0}", MessageSeverity.Error, result.AsmCompilerPath);
                result.ValidationPassed = false;
                return result;
            }
            if (string.IsNullOrEmpty(result.LinkerPath) || !Path.Exists(result.LinkerPath))
            {
                OnValidationMessage("Linker path is invalid: {0}", MessageSeverity.Error, result.LinkerPath);
                result.ValidationPassed = false;
                return result;
            }
            if (string.IsNullOrEmpty(result.OptimizerPath) || !Path.Exists(result.OptimizerPath))
            {
                OnValidationMessage("Optimizer path is invalid:  {0}", MessageSeverity.Error, result.OptimizerPath);
                result.ValidationPassed = false;
                return result;
            }
            if (string.IsNullOrEmpty(result.ConstifierPath) || !Path.Exists(result.ConstifierPath))
            {
                OnValidationMessage("Constifier path is invalid: {0}", MessageSeverity.Error, result.ConstifierPath);
                result.ValidationPassed = false;
                return result;
            }

            // Optional Resource Compilers
            if (string.IsNullOrEmpty(result.GFX4SNESPath) || !Path.Exists(result.GFX4SNESPath))
            {
                OnValidationMessage("Graphics compiler path is invalid: {0}", MessageSeverity.Error, result.ConstifierPath);
                result.ValidationPassed = false;
                return result;
            }

            // Memory Map
            if (string.IsNullOrEmpty(result.MemoryMapFolder) || !Directory.Exists(result.MemoryMapFolder))
            {
                OnValidationMessage("Memory Map folder does not exist or is invalid");
                result.ValidationPassed = false;
                return result;
            }

            return result;

        }
        public Compilation PrepareRun(ValidatedSettings settings, IEnumerable<FileItem> codeFiles, IEnumerable<FileItem> imageFiles)
        {
            var ccommands = new List<CFileCommandSet>();
            var asmCommands = new List<SubProcessCommand>();
            var resourceCommands = new List<SubProcessCommand>();
            var linkFilePath = Path.Combine(settings.Settings.ProjectFolder, LINK_FILE_NAME);
            var outputFile = Path.Combine(settings.Settings.ProjectFolder, OUTPUT_FILE_NAME);

            // C-Files:  For each c file, compile the following commands:  compile -> optimize -> constify -> assemble
            //
            foreach (var file in codeFiles.Where(x => x.Type == CodeFileType.C))
            {
                var compile = new SubProcessCommand();
                compile.ExeFilePath = settings.CCompilerPath;

                // Create output files for the compiler to use
                var psFile = file.Path.Replace(".c", ".ps");
                var aspFile = file.Path.Replace(".c", ".asp");
                var asmFile = file.Path.Replace(".c", ".asm");
                var objFile = file.Path.Replace(".c", ".obj");

                // TODO: MAKE CONST + SOME COMMENTS FOR COMMAND OPTIONS!
                if (settings.Settings.MemoryMap == MemoryMapSettings.HIROM)
                {
                    if (settings.Settings.Speed == SpeedSettings.FAST)
                    {
                        compile.Arguments.AddRange("-I", settings.DevKitIncludeFolder, "-Wall", "-c", file.Path, "-H", "-F", "-o", psFile);
                    }
                    else
                    {
                        compile.Arguments.AddRange("-I", settings.DevKitIncludeFolder, "-Wall", "-c", file.Path, "-H", "-o", psFile);
                    }
                }
                else
                {
                    if (settings.Settings.Speed == SpeedSettings.FAST)
                    {
                        compile.Arguments.AddRange("-I", settings.DevKitIncludeFolder, "-Wall", "-c", file.Path, "-F", "-o", psFile);
                    }
                    else
                    {
                        compile.Arguments.AddRange("-I", settings.DevKitIncludeFolder, "-Wall", "-c", file.Path, "-o", psFile);
                    }
                }

                var optimize = new SubProcessCommand();
                var constify = new SubProcessCommand();
                var assemble = new SubProcessCommand();

                optimize.ExeFilePath = settings.OptimizerPath;
                constify.ExeFilePath = settings.ConstifierPath;
                assemble.ExeFilePath = settings.AsmCompilerPath;

                optimize.Arguments.AddRange(psFile, ">", aspFile);                       // The Standard Output for this one IS the .asp file
                constify.Arguments.AddRange(file.Path, aspFile, asmFile);                // May have to use just the FILENAME(S), here, and make sure we're compiling out of the same directory
                assemble.Arguments.AddRange("-d", "-s", "-x", "-o", objFile, asmFile);

                ccommands.Add(new CFileCommandSet(compile, optimize, constify, assemble));
            }

            // Asm Files:  For each assembler file, run the assembler compiler
            //
            foreach (var file in codeFiles.Where(x => x.Type == CodeFileType.Assembler))
            {
                var command = new SubProcessCommand();
                command.ExeFilePath = settings.AsmCompilerPath;
                command.Arguments.AddRange("-d", "-s", "-x", "-o", file.Path.Replace(".asm", ".obj"), file.Path);

                asmCommands.Add(command);
            }

            // Link File:  Create link file from the .obj files (for .asm + .c files), GIVEN IN THE ORDER THEY WERE PRESENTED,
            //             followed by the MemoryMapFolder (lib directory) .obj files.
            //

            // (we should be weeding out our files from the project directory tree.. there's lots TBD to handle this whole problem)
            var linkerFiles = codeFiles.Select(x => x.Path.Replace(".c", ".obj").Replace(".asm", ".obj")).Distinct();

            CreateLinkerFile(linkFilePath, settings.MemoryMapFolder, linkerFiles);

            var linkCommand = new SubProcessCommand();
            linkCommand.ExeFilePath = settings.LinkerPath;
            linkCommand.Arguments.AddRange("-d", "-s", "-c", "-v", "-A", "-L", settings.MemoryMapFolder, linkFilePath, Path.Combine(settings.Settings.ProjectFolder, OUTPUT_FILE_NAME));

            // Resource Files
            foreach (var file in imageFiles)
            {
                var command = new SubProcessCommand();
                command.ExeFilePath = settings.GFX4SNESPath;
                command.Arguments.AddRange("-i", file.Path, "-t", file.Type == CodeFileType.ResourceBmp ? "bmp" : "png", "-m");

                resourceCommands.Add(command);
            }

            return new Compilation(new CompilationCommandSet(ccommands, asmCommands, resourceCommands, linkCommand));
        }
        public bool ExecuteRun(Compilation compilation, string workingDir)
        {
            // PROBLEM WITH WORKING DIRECTORY:  The compilers are creating .asm files that reference base directory
            //                                  files. (look for assembler compiler errors)
            //
            //Directory.SetCurrentDirectory(workingDir);

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

                OnRunMessage("Executing Command:  {0}", MessageSeverity.Info, ccommand.Compile.GetFullCommandLine());

                // Run Commands Synchronously
                if (!compileProcess.Run((message, severity) => OnRunMessage(message, severity)))
                {
                    OnRunMessage("Compilation Process Error:  " + ccommand.Compile.GetFullCommandLine(), MessageSeverity.Error);
                    return false;
                }

                OnRunMessage("Executing Command:  {0}", MessageSeverity.Info, ccommand.Optimize.GetFullCommandLine());

                if (!optimizeProcess.Run((message, severity) => OnRunMessage(message, severity)))
                {
                    OnRunMessage("Optimizer Process Error:  " + ccommand.Optimize.GetFullCommandLine(), MessageSeverity.Error);
                    return false;
                }

                OnRunMessage("Executing Command:  {0}", MessageSeverity.Info, ccommand.Constify.GetFullCommandLine());

                if (!constifyProcess.Run((message, severity) => OnRunMessage(message, severity)))
                {
                    OnRunMessage("Constify Process Error:  " + ccommand.Constify.GetFullCommandLine(), MessageSeverity.Error);
                    return false;
                }

                OnRunMessage("Executing Command:  {0}", MessageSeverity.Info, ccommand.Assemble.GetFullCommandLine());

                if (!assembleProcess.Run((message, severity) => OnRunMessage(message, severity)))
                {
                    OnRunMessage("Assembler Process Error:  " + ccommand.Assemble.GetFullCommandLine(), MessageSeverity.Error);
                    return false;
                }
            }

            foreach (var asmCommand in compilation.CommandSet.AssemblerCommands)
            {
                var process = new CommandProcess(asmCommand);

                OnRunMessage("Executing Command:  {0}", MessageSeverity.Info, asmCommand.GetFullCommandLine());

                if (!process.Run((message, severity) => OnRunMessage(message, severity)))
                {
                    OnRunMessage("Assembler Process Error:  " + asmCommand.GetFullCommandLine(), MessageSeverity.Error);
                    return false;
                }
            }

            var linker = new CommandProcess(compilation.CommandSet.LinkCommand);

            OnRunMessage("Executing Command:  {0}", MessageSeverity.Info, compilation.CommandSet.LinkCommand.GetFullCommandLine());

            if (!linker.Run((message, severity) => OnRunMessage(message, severity)))
            {
                OnRunMessage("Linker Process Error:  " + compilation.CommandSet.LinkCommand.GetFullCommandLine(), MessageSeverity.Error);
                return false;
            }

            foreach (var command in compilation.CommandSet.ResourceCommands)
            {
                var process = new CommandProcess(command);

                OnRunMessage("Executing Command:  {0}", MessageSeverity.Info, command.GetFullCommandLine());

                if (!process.Run((message, severity) => OnRunMessage(message, severity)))
                {
                    OnRunMessage("Resource Process Error:  " + command.GetFullCommandLine(), MessageSeverity.Error);
                    return false;
                }
            }

            OnRunMessage("Compilation Complete!");

            return true;
        }

        /// <summary>
        /// Creates (extraneous) linker file from the provided .obj files. This is also done during run preparation from
        /// the compiler (future) products by substituting the file extension.
        /// </summary>
        public bool CreateLinkerFile(string path, string memoryMapFolder, IEnumerable<string> ourObjFiles)
        {
            // Link File:  Create link file from the .obj files, GIVEN IN THE ORDER THEY WERE PRESENTED,
            //             followed by the MemoryMapFolder (lib directory) .obj files.
            //

            try
            {
                var linkFile = new StringBuilder();

                // Header
                linkFile.AppendLine("[objects]");

                // Our .obj files
                foreach (var file in ourObjFiles)
                {
                    linkFile.AppendLine(file);
                }

                // Lib .obj files
                foreach (var file in Directory.GetFiles(memoryMapFolder, "*.obj"))
                {
                    linkFile.AppendLine(file);
                }

                // Write link file to disk
                System.IO.File.WriteAllText(path, linkFile.ToString());
                return true;
            }
            catch (Exception e)
            {
                OnValidationMessage("Error Creating Linker File:  " + e.ToString(), MessageSeverity.Error);
                return false;
            }
        }

        private void OnValidationMessage(string format, MessageSeverity severity = MessageSeverity.Info, params object[] parameters)
        {
            if (this.ValidationMessageEvent != null)
                this.ValidationMessageEvent(string.Format(format, parameters), severity);
        }
        private void OnRunMessage(string format, MessageSeverity severity = MessageSeverity.Info, params object[] parameters)
        {
            if (this.RunMessageEvent != null)
                this.RunMessageEvent(string.Format(format, parameters), severity);
        }
        private void OnValidationMessage(string message, MessageSeverity severity = MessageSeverity.Info)
        {
            if (this.ValidationMessageEvent != null)
                this.ValidationMessageEvent(message, severity);
        }
        private void OnRunMessage(string message, MessageSeverity severity = MessageSeverity.Info)
        {
            if (this.RunMessageEvent != null)
                this.RunMessageEvent(message, severity);
        }
    }
}
