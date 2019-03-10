using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Diagnostics;

namespace vbox_vm_backup
{
    class Program
    {

        private static VMInfo[] Targets = {};
        private static CLogger logger;


        private static void Main(string[] args)
        {
            logger = new CLogger();

            logger.Log("======================================================================", false);
            logger.Log("-----------------------Backup sequence started------------------------", false, ConsoleColor.Green );
            logger.Log("======================================================================", false);

            LoadConfig();

            foreach (VMInfo vm in Targets)
            {
                CopyVM(vm.SourcePath, vm.DestPath, vm.VMName, vm.VBoxInstallPath);
#if (!DEBUG)
                System.Threading.Thread.Sleep(vm.WaitVMToStart);
#endif
                logger.Log("----------------------------------------------------------------------", false);
            }

#if (DEBUG)
            Console.ReadKey();          
#endif

        }

        /// <summary>
        /// Sends shutdown signal to VM
        /// </summary>
        /// <param name="VMName">VM name in VirtualBox</param>
        /// <param name="VBoxPath">Path to VirtualBox executables</param>
        private static void StopVM(string VMName, string VBoxPath)
        {
            logger.Log("Stopping VM " + VMName + "...", true, ConsoleColor.DarkRed );

#if (!DEBUG)
            Process p = new Process();
            p.StartInfo.FileName= VBoxPath + "VBoxManage.exe";
            p.StartInfo.Arguments = "controlvm " + VMName + " acpipowerbutton";
            p.Start();
#endif
        }

        /// <summary>
        /// Starts virtual machine with specified name
        /// </summary>
        /// <param name="VMName">VM name in VirtualBox</param>
        /// <param name="VBoxPath">Path to VirtualBox executables</param>
        private static void StartVM(string VMName, string VBoxPath)
        {
            logger.Log("Starting VM " + VMName + "...");

#if (!DEBUG)
            Process p = new Process();
            p.StartInfo.FileName = VBoxPath + "VirtualBoxVM.exe";
            p.StartInfo.Arguments = "--startvm " + VMName;
            p.Start();
#endif

            logger.Log("VM " + VMName + " started.", true, ConsoleColor.Green);
        }

        /// <summary>
        /// Sends shutdown signal to VM, waits for it's correct shutdown, copies VM files, starts VM back
        /// </summary>
        /// <param name="source">Copy VM files from this folder</param>
        /// <param name="dest">Copy VM files to this folder</param>
        /// <param name="VMName">VM name in VirtualBox</param>
        /// <param name="VBoxPath">Path to VirtualBox executables</param>
        private static void CopyVM(string source, string dest, string VMName, string VBoxPath)
        {
            StopVM(VMName, VBoxPath);      
            WaitForShutdown(VMName);

#if (!DEBUG)  
            DirectoryCopy(source + VMName, dest + VMName + " " + DateTime.Now.ToString("dd.MM.yyyy-hh.mm.ss"));
#endif

            StartVM(VMName, VBoxPath);            
        }

        /// <summary>
        /// Recursive void to copy directory with subdirs and files
        /// </summary>
        /// <param name="sourceDirName">Copy from</param>
        /// <param name="destDirName">Copy to</param>
        private static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
                logger.Log("Directory " + destDirName + " created");
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
                logger.Log("File " + file.Name + " copied");
            }

           foreach (DirectoryInfo subdir in dirs)
           {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath);
           }

        }

        /// <summary>
        /// Waits for VM process termination
        /// </summary>
        /// <param name="VMName">Name of virtual machine</param>
        private static void WaitForShutdown(string VMName)
        {
#if (!DEBUG) 
            bool running = false;

            do
            {
                running = false;
                Process[] proc = Process.GetProcesses();
                foreach (Process pr in proc)
                {
                    if (pr.MainWindowTitle.Contains(VMName) && pr.ProcessName == "VirtualBoxVM")
                    {
                        running = true;
                    }
                }

                System.Threading.Thread.Sleep(5000);
                

            } while (running);
#endif

            logger.Log("VM " + VMName + " stopped.");
        }

        /// <summary>
        /// Loads list of VMs to backup from settings.xml file
        /// </summary>
        private static void LoadConfig()
        {
            try
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "settings.xml"))
                {
                    using (FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "settings.xml", FileMode.Open))
                    {
                        XmlSerializer formatter = new XmlSerializer(typeof(VMInfo[]));
                        Targets = (VMInfo[])formatter.Deserialize(fs);
                    }
                }
                else 
                {
                    logger.Log("ERROR: settings.xml file not found. Aborting.");
                    Environment.Exit(1);
                }
            }
            catch (System.Exception ex)
            {
                logger.Log("ERROR - please check settins.xml. Details: " + ex.InnerException.Message);
                Environment.Exit(1);
            }
            if (Targets.Count()==0)
            {
                logger.Log("No VMs to backup of something wrong with settings.xml. Exiting.");
                Environment.Exit(1);
            }
        }


    }
}
