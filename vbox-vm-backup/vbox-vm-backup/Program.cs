using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Collections.Generic;

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
                CopyVM(vm);
                System.Threading.Thread.Sleep(vm.WaitVMToStart);
                logger.Log("----------------------------------------------------------------------", false);
            }

        }

        /// <summary>
        /// Sends shutdown signal to VM
        /// </summary>
        /// <param name="VMName">VM name in VirtualBox</param>
        /// <param name="VBoxPath">Path to VirtualBox executables</param>
        private static void StopVM(string VMName, string VBoxPath)
        {
            logger.Log("Stopping VM " + VMName + "...", true, ConsoleColor.DarkRed );

            Process p = new Process();
            p.StartInfo.FileName= Path.Combine(VBoxPath, "VBoxManage.exe");
            p.StartInfo.Arguments = "controlvm " + "\"" + VMName + "\" acpipowerbutton";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();
        }

        /// <summary>
        /// Starts virtual machine with specified name
        /// </summary>
        /// <param name="VMName">VM name in VirtualBox</param>
        /// <param name="VBoxPath">Path to VirtualBox executables</param>
        private static void StartVM(string VMName, string VBoxPath)
        {
            logger.Log("Starting VM " + VMName + "...");

            Process p = new Process();
            p.StartInfo.FileName = Path.Combine(VBoxPath, "VirtualBoxVM.exe");
            p.StartInfo.Arguments = "--startvm " + "\"" + VMName + "\"";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();

            logger.Log("VM " + VMName + " started.", true, ConsoleColor.Green);
        }

        /// <summary>
        /// Sends shutdown signal to VM, waits for it's correct shutdown, copies VM files, starts VM back
        /// </summary>
        /// <param name="VM">Virtual machine info</param>
        private static void CopyVM(VMInfo VM)
        {
            StopVM(VM.VMName, VM.VBoxInstallPath);      
            WaitForShutdown(VM.VMName);

            if (VM.CompressVDI) CompressVDI(VM);
 
            DirectoryCopy(VM.SourcePath, Path.Combine(VM.DestPath, VM.VMName + " " + DateTime.Now.ToString("dd.MM.yyyy-HH.mm.ss")));

            StartVM(VM.VMName, VM.VBoxInstallPath);

            RemoveOldCopies(VM);
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
                logger.Log("Source directory does not exist or could not be found: " + sourceDirName,true, ConsoleColor.Red);
                return;
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
                file.CopyTo(temppath, true);
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

        private static void RemoveOldCopies(VMInfo VM)
        {
            DirectoryInfo dir = new DirectoryInfo(VM.DestPath);
            DirectoryInfo[] dirs = dir.GetDirectories(VM.VMName + "*", SearchOption.TopDirectoryOnly);          //Get list of VM folders

            SortedDictionary<DateTime, DirectoryInfo> dic = new SortedDictionary<DateTime, DirectoryInfo>();
            System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;

            //Add folders to list, sorted by date
            foreach (DirectoryInfo tmp in dirs)
            {
                DateTime dt = new DateTime();
                bool correct = DateTime.TryParseExact(tmp.Name.Substring(tmp.Name.Length - 19), "dd.MM.yyyy-HH.mm.ss", provider, System.Globalization.DateTimeStyles.None, out dt);
                if (correct)        //If folder name differs of our mask (or just contains VM name in it's name) - ignoring it
                {
                    dic.Add(dt, tmp);
                }
                else
                {
                    logger.Log("Folder " + tmp.FullName + " does not looks like my backup, ignoring it" );
                }

            }

            if (dic.Count() <= VM.NumberOfCopies)
            {
                logger.Log("There are " + dic.Count + " copies of this VM (must store " + VM.NumberOfCopies + "), so nothing to delete.");
                return;
            }

            while (dic.Count() > VM.NumberOfCopies)             //Deleting the oldest folder(s)
            {
                KeyValuePair<DateTime, DirectoryInfo> toDelete = dic.First();

                if (toDelete.Value.Exists)
                {
                    try
                    {
                        logger.Log("Deleting old copy " + toDelete.Value.FullName);
                        toDelete.Value.Delete(true);
                    }
                    catch(Exception ex)
                    {

                        logger.Log("Error deleting old copy - " + toDelete.Value.FullName,true, ConsoleColor.Red);
                        logger.Log("Error info: " + ex.Message );
                    }
                    finally
                    {
                        dic.Remove(toDelete.Key);           //If we can't delete it (maybe it's read-only) - ingore it
                    }
                }
            }
        }
        
        /// <summary>
        /// Compressing all vdi disk images in VM folder
        /// </summary>
        /// <param name="VM">Virtual machine info</param>
        private static void CompressVDI(VMInfo VM)
        {
            logger.Log("Trying to compress VDI image...", true);

            //Getting all *.vdi
            DirectoryInfo dir = new DirectoryInfo(VM.SourcePath);
            FileInfo[] files = dir.GetFiles("*.vdi");

            //Compressing each one
            foreach (FileInfo file in files)
            {
                Process p = new Process();
                p.StartInfo.FileName = Path.Combine(VM.VBoxInstallPath, "VBoxManage.exe");
                p.StartInfo.Arguments = "modifyhd " + "\"" + file.FullName + "\" --compact";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardError = true;
                p.Start();
                logger.Log(p.StandardError.ReadToEnd());
                p.WaitForExit();
            }

        }
    }
}
