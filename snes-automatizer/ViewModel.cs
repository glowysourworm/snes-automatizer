using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snes_automatizer
{
    // TODO: Bring in some documentation for high-level compiler settings
    public enum MemoryMapSettings
    {
        [Display(Name = "LOROM", Description = "TODO")]
        LOROM = 0,

        [Display(Name = "HIROM", Description = "TODO")]
        HIROM = 1
    }

    // TODO: Bring in some documentation for high-level compiler settings
    public enum SpeedSettings
    {
        [Display(Name = "FAST", Description = "TODO")]
        FAST = 0,
        [Display(Name = "SLOW", Description = "TODO")]
        SLOW = 1
    }

    public class FileItem : ViewModelBase
    {
        string _path;
        bool _included;

        public string Path
        {
            get { return _path; }
            set { this.RaiseAndSetIfChanged(ref _path, value); }
        }
        public bool Included
        {
            get { return _included; }
            set { this.RaiseAndSetIfChanged(ref _included, value); }
        }
        public FileItem()
        {
            _included = true;
            _path = "";
        }
        public FileItem(string path)
        {
            _path = path;
            _included = true;
        }
    }

    public class Settings : ViewModelBase
    {
        string _projectFolder;
        string _pvSnesLibFolder;
        MemoryMapSettings _memoryMap;
        SpeedSettings _speed;

        /// <summary>
        /// Base folder for your project code. This program will search through all your files to look
        /// for compilable files for the project.
        /// </summary>
        public string ProjectFolder
        {
            get { return _projectFolder; }
            set { this.RaiseAndSetIfChanged(ref _projectFolder, value); }
        }

        /// <summary>
        /// Base folder for the pvsneslib development "IDE". Sub-files and folder locations will be part of validation.
        /// </summary>
        public string PVSNESLIBFolder
        {
            get { return _pvSnesLibFolder; }
            set { this.RaiseAndSetIfChanged(ref _pvSnesLibFolder, value); }
        }

        public MemoryMapSettings MemoryMap
        {
            get { return _memoryMap; }
            set { this.RaiseAndSetIfChanged(ref _memoryMap, value); }
        }
        public SpeedSettings Speed
        {
            get { return _speed; }
            set { this.RaiseAndSetIfChanged(ref _speed, value); }
        }

        public Settings()
        {
            this.ProjectFolder = "";
            this.PVSNESLIBFolder = "";
        }
    }

    public class ViewModel : ViewModelBase
    {
        Settings _settings;
        SimpleObservableCollection<string> _assemblerFiles;
        SimpleObservableCollection<string> _cFiles;
        SimpleObservableCollection<string> _outputMessages;

        public Settings Settings
        {
            get { return _settings; }
            set { this.RaiseAndSetIfChanged(ref _settings, value); }
        }
        public SimpleObservableCollection<string> AssemblerFiles
        {
            get { return _assemblerFiles; }
            set { this.RaiseAndSetIfChanged(ref _assemblerFiles, value); }
        }
        public SimpleObservableCollection<string> CFiles
        {
            get { return _cFiles; }
            set { this.RaiseAndSetIfChanged(ref _cFiles, value); }
        }
        public SimpleObservableCollection<string> OutputMessages
        {
            get { return _outputMessages; }
            set { this.RaiseAndSetIfChanged(ref _outputMessages, value); }
        }



        public ViewModel()
        {
            this.Settings = new Settings();
            this.CFiles = new SimpleObservableCollection<string>();
            this.AssemblerFiles = new SimpleObservableCollection<string>();
            this.OutputMessages = new SimpleObservableCollection<string>();
        }
    }

    /*

from pathlib import Path
import tkinter as tk
from tkinter import messagebox
import sys
import os
import subprocess

DEBUG: str | bool = input("Debug mode: yes(y) or no(n)?\n")

asking = True

while asking == True:
    if DEBUG == "y":
        DEBUG = True
        asking = False
    elif DEBUG == "n":
        DEBUG = False
        asking = False
    else:
        DEBUG = input("Not valid response! just y or n, case sensitive!\n")
        continue

class DragDropListbox(tk.Listbox):
    def __init__(self, master, **kw):
        kw['selectmode'] = tk.SINGLE
        tk.Listbox.__init__(self, master, kw)
        self.bind('<Button-1>', self.setCurrent)
        self.bind('<B1-Motion>', self.shiftSelection)
        self.curIndex = None

    def setCurrent(self, event):
        self.curIndex = self.nearest(event.y)

    def shiftSelection(self, event):
        i = self.nearest(event.y)
        if i < self.curIndex:
            x = self.get(i)
            self.delete(i)
            self.insert(i+1, x)
            self.curIndex = i
        elif i > self.curIndex:
            x = self.get(i)
            self.delete(i)
            self.insert(i-1, x)
            self.curIndex = i

def reorder_list(items: list):
    def check_order():
        order = listbox.get(0, tk.END)
        if messagebox.askyesno("Confirmation", f"Linkfile order is correct?\n\n{order}"):
            root.quit()
            return order
        else:
            messagebox.showinfo("Order was not confirmed", "Please, reorder the list.")

    def add_element():
        new_element = input("Write down the element you want to add to the list:\n")
        listbox.insert(tk.END, new_element)

    def remove_element():
        selected_item_index = listbox.curselection()
        if selected_item_index:
            listbox.delete(selected_item_index)

    root = tk.Tk()
    root.title("Linkfile reoderer")
    root.geometry("300x700")

    label = tk.Label(root, text="Move the linkfile elements to change it order:")
    label.pack(pady=10)

    listbox = DragDropListbox(root, bg="white", fg="black", width=40, height=25)
    listbox.pack(pady=10)

    for item in items:
        listbox.insert(tk.END, item)

    insertb = tk.Button(root, text="Insert new element", command=add_element)
    insertb.pack(pady=10)
    
    removeb = tk.Button(root, text="Remove selected element", command=remove_element)
    removeb.pack(pady=10)
    
    button = tk.Button(root, text="Confirm Order", command=check_order)
    button.pack(pady=10)

    root.mainloop()

    return list(listbox.get(0, tk.END))


def get_executable_path():
    if getattr(sys, 'frozen', False):
        # PyInstaller executable
        print("Executable path mode chosen")
        return os.path.dirname(sys.executable)
    else:
        # Normal script
        print("Python script path mode chosen")
        return os.path.dirname(os.path.abspath(__file__))


base_dir = Path(get_executable_path()).parent
print(base_dir)
src_dir = Path(sys.argv[1])
memory_map: str = sys.argv[2]
speed: str = sys.argv[3]
if src_dir.exists() == False or src_dir.is_dir() == False:
    raise Exception("Path does not exists or is not a path")

if memory_map == "HIROM" and speed == "FAST":
    lib_dir = base_dir / 'lib' / 'HiROM_FastROM'
elif memory_map == "LOROM" and speed == "FAST":
    lib_dir = base_dir / 'lib' / 'LoROM_FastROM'
elif memory_map == "HIROM" and speed == "SLOW":
    lib_dir = base_dir / 'lib' / 'HiROM_SlowROM'
elif memory_map == "LOROM" and speed == "SLOW":
    lib_dir = base_dir / 'lib' / 'LoROM_SlowROM'
else:
    raise Exception("Error mapping memory...")
    
tools_dir = base_dir / 'tools'
devkit_dir = base_dir / 'devkitsnes'
asm_files: list[Path] = list()
c_files: list[Path] = list()

for file in src_dir.rglob("*.c"):
    c_files.append(Path(file))
    
for file in src_dir.rglob("*.asm"):
    asm_files.append(Path(file))

cc = devkit_dir / 'bin' / '816-tcc.exe'
assembler = devkit_dir / 'bin' / 'wla-65816.exe'
linker = devkit_dir / 'bin' / 'wlalink.exe'
opt = tools_dir / '816-opt.exe'
ctf = tools_dir /'constify.exe'

for c_file in c_files:
    ps_file = c_file.with_suffix('.ps')
    asp_file = c_file.with_suffix('.asp')
    asm_file = c_file.with_suffix('.asm')
    obj_file = c_file.with_suffix('.obj')
          
    if memory_map == "HIROM":
        if speed == "FAST":
            subprocess.run([cc,'-I' + str(devkit_dir / "include"), '-Wall', '-c', c_file,'-H','-F','-o', ps_file])
        else:
            subprocess.run([cc,'-I' + str(devkit_dir / "include"), '-Wall', '-c', c_file,'-H', '-o', ps_file])
    else:
        if speed == "FAST":
            subprocess.run([cc,'-I' + str(devkit_dir / "include"), '-Wall', '-c', c_file,'-F', '-o', ps_file])
        else:
            subprocess.run([cc,'-I' + str(devkit_dir / "include"), '-Wall', '-c', c_file, '-o', ps_file])

    with open(asp_file, "w") as f:
        subprocess.run([opt, ps_file], stdout=f)
    
    subprocess.run([ctf, c_file, asp_file, asm_file])

    subprocess.run([assembler, '-d', '-s', '-x', '-o', obj_file, asm_file])
    
for asm_file in asm_files:
    subprocess.run([assembler, '-d', '-s', '-x', '-o', asm_file.with_suffix('.obj'), asm_file])
    

linkfile = [str(obj_file.with_suffix('.obj')) for obj_file in asm_files + c_files]


linkfile.insert(0, "[objects]")

for file in lib_dir.rglob("*"):
    linkfile.append(file)

linkfile = reorder_list(linkfile)

with open('linkfile', 'w') as f:
    f.write('\n'.join(linkfile))
subprocess.run([linker, '-d', '-s', '-c', '-v', '-A', '-L', lib_dir, 'linkfile', src_dir / 'output.sfc'])

print("\nBuild finished succesfully!\n")

if DEBUG == False:
    try:
        for c_file in c_files:
            ps_file = c_file.with_suffix('.ps')
            asp_file = c_file.with_suffix('.asp')
            asm_file = c_file.with_suffix('.asm')
            obj_file = c_file.with_suffix('.obj')

            os.unlink(ps_file)
            os.unlink(asp_file)
            os.unlink(asm_file)
            os.unlink(obj_file)
    
        for asm_file in asm_files:
            obj_file = asm_file.with_suffix('.obj')
            os.unlink(obj_file)
        
        os.unlink(src_dir / "linkfile")
        os.unlink(src_dir / 'output.sym')
        
    except FileNotFoundError:
        pass
    
    print("TEMP FILES REMOVED SUCCESFULLY!")
    input("PRESS ANY KEY TO EXIT...")
else:
    print("Debug files in source directory: .ps -> tcc_dbg, .asp -> opt_dbg, .asm -> constifier debug, .sym -> linker debug")
    input("PRESS ANY KEY TO EXIT...")



    */
}
