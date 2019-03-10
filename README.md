# vbox-vm-backup

Command line utility to make backups of Oracle VirtualBox virtual machines

It sends shutdown signal to VM, waits for it's shutdown, copies all VM files to specified folder and then starts VM back. 
To decrease downtime and disk load, utility processes VMs one by one. 

This utility is designed to be started manually or by Windows Task Scheduler (you should make a task manually), makes a log file of it's work (**vbox-vm-backup.log** in program folder) and uses XML config file (**settings.xml** in program folder).

Example settings.xml included in release package and rather intuitive:

```XML
<?xml version="1.0"?>
<ArrayOfVMInfo xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <VMInfo>
    <VMName>XMPPServer</VMName>
    <SourcePath>C:\Users\user\Desktop\VMs\</SourcePath>
    <DestPath>D:\</DestPath>
    <NumberOfCopies>7</NumberOfCopies>
    <WaitVMToStart>90000</WaitVMToStart>
    <VBoxInstallPath>C:\Program Files\Oracle\VirtualBox\</VBoxInstallPath>
  </VMInfo>
  <VMInfo>
    <VMName>WebServer</VMName>
    <SourcePath>C:\Users\user\Desktop\VMs\</SourcePath>
    <DestPath>D:\</DestPath>
    <NumberOfCopies>7</NumberOfCopies>
    <WaitVMToStart>90000</WaitVMToStart>
    <VBoxInstallPath>C:\Program Files\Oracle\VirtualBox\</VBoxInstallPath>
  </VMInfo>
</ArrayOfVMInfo>
```

There are two example VMs - XMPPServer and WebServer (these are VM names in VirtualBox Control Panel).

So program copies files from `C:\Users\user\Desktop\VMs\XMPPServer` to `D:\XMPPServer_Date-Time`

Waits for 90 seconds (**WaitVMToStart**), allowing first VM to start.

Then goes for next VM - `C:\Users\user\Desktop\VMs\WebServer` to `D:\WebServer_Date-Time`
