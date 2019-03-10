using System;
using System.IO;

namespace vbox_vm_backup
{
    class CLogger : IDisposable
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
                Console.WriteLine(ex.InnerException.Message);
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
        
        #region IDisposable implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool m_Disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
                    logfile.Dispose();
                }

                // Unmanaged resources are released here.

                m_Disposed = true;
            }
        }

        ~CLogger()
        {
            Dispose(false);
        }

        #endregion
    }
}
