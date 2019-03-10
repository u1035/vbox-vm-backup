using System;

namespace vbox_vm_backup
{
    [Serializable]
    public class VMInfo
    {
        public string VMName { get; set; }                      //Name of VM
        public string SourcePath { get; set; }                  //Copy from
        public string DestPath { get; set; }                    //Copy to
        public string VBoxInstallPath { get; set; }             //Path to VirtualBox executable files
        public int NumberOfCopies { get; set; }                 //Store last n copies
        public int WaitVMToStart { get; set; }                  //Time to wait before next VM backup (milliseconds)

        public VMInfo()  
        { }

        public VMInfo(string Name, string Src, string Dest, int Copies, int Wait, string VBoxPath)
        {
            VMName = Name;
            SourcePath = Src;
            DestPath = Dest;
            NumberOfCopies = Copies;
            WaitVMToStart = Wait;
            VBoxInstallPath = VBoxPath; 
        }
    }
}
