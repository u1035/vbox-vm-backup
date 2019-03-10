using System;
using System.IO;

namespace vbox_vm_backup
{
    class CLogger
    {

        private StreamWriter logfile;

        public CLogger()
        {
            try
            {
                logfile = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "vbox-vm-backup.log", true);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        ~CLogger()
        {
            try
            {
                logfile.Flush();
                logfile.Close();
            }
            catch (System.Exception ex)
            {
                //if we are here - logfile is probably alerady closed
                //Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Shows message in console and logs it to logfile
        /// </summary>
        /// <param name="Message">Message to display</param>
        /// <param name="Timestamp">If true - adds a timestamp to message in log file</param>
        /// <param name="Color">Colors message on console output</param>
        public void Log(string Message, bool Timestamp = true, ConsoleColor Color = ConsoleColor.White)
        {
            Console.ForegroundColor = Color;
            Console.WriteLine(Message);
            Console.ResetColor();

            if (Timestamp)
            {
                logfile.WriteLine(DateTime.Now.ToString() + " " + Message);
            }
            else
            {
                logfile.WriteLine(Message);
            }
            logfile.Flush();
        }
    }
}
