using System.IO;

namespace snes_automatizer.Model
{
    public class ValidatedSettings : ViewModelBase
    {
        public const string DEV_KIT_SNES = "devkitsnes";
        public const string DEV_KIT_SNES_INCLUDE = "devkitsnes\\include";
        public const string TOOLS = "tools";
        public const string ASM_COMPILER = "bin\\wla-65816.exe";
        public const string C_COMPILER = "bin\\816-tcc.exe";
        public const string LINKER = "bin\\wlalink.exe";
        public const string OPTIMIZER = "816-opt.exe";
        public const string CONSTIFIER = "constify.exe";
        public const string GFX4SNES = "gfx4snes.exe";
        public const string HIROM_FAST = "lib\\HiROM_FastROM";
        public const string LOROM_FAST = "lib\\LoROM_FastROM";
        public const string HIROM_SLOW = "lib\\HiROM_SlowROM";
        public const string LOROM_SLOW = "lib\\LoROM_SlowROM";

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

        /// <summary>
        /// Path to gfx4snes.exe
        /// </summary>
        public string GFX4SNESPath { get; set; }

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

            // Base -> More Tools
            this.GFX4SNESPath = Path.Combine(this.ToolsFolder, GFX4SNES);

            if (settings.MemoryMap == MemoryMapSettings.HIROM)
            {
                this.MemoryMapFolder = settings.Speed == SpeedSettings.FAST ? Path.Combine(settings.PVSNESLIBFolder, HIROM_FAST) :
                                                                              Path.Combine(settings.PVSNESLIBFolder, HIROM_SLOW);
            }
            else
            {
                this.MemoryMapFolder = settings.Speed == SpeedSettings.FAST ? Path.Combine(settings.PVSNESLIBFolder, LOROM_FAST) :
                                                                              Path.Combine(settings.PVSNESLIBFolder, LOROM_SLOW);
            }
        }
    }
}
