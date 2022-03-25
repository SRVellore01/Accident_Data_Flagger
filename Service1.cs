using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Configuration;
using AccidentDataFlagger.Code;
using System.IO;

namespace AccidentDataFlagger
{
    public partial class Service1 : ServiceBase
    {
        Timer tm = new Timer();

        public Service1()
        {
            InitializeComponent();

        }
       

        protected override void OnStart(string[] args)
        {



            int timeInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["TimeInMinutes"].ToString());
            tm.Elapsed += new ElapsedEventHandler(onElapsedTime);
            tm.Enabled = true;
            tm.Interval = timeInMinutes * 60 *1000;

                //10800000;
            


        }

        private void onElapsedTime(object sender, ElapsedEventArgs e)
        {

            new OragamiFilesDownloader().GetFiles();
            new CSVReaderWriter().IterateThroughCSV();
            SQLReader.JobStart();
            SQLReader.ReadFromJobQueue();
            SQLReader.JobEnd();




        }

        protected override void OnStop()
        {
        }

        public void WriteToTraceListener(string logMessage, Exception ex, params object[] args)
        {
            var LogFilePath = ConfigurationManager.AppSettings["LogFilePath"];
            string m_exePath = string.Empty;

            //m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            m_exePath = Path.GetDirectoryName(LogFilePath);
            try
            {
                using (StreamWriter w = File.AppendText(m_exePath + "\\" + "AccidentDataFlaggerLog.txt"))
                // using (StreamWriter w = File.AppendText(m_exePath))//
                {
                    Log(logMessage, w, ex, args);
                }
            }
            catch (Exception ex1)
            {
                string Exception = ex1.Message.Substring(0, Math.Min(ex1.Message.Length, 200));

                WriteToTraceListener("Error Occured while creating the path for log  ", ex1, null);
                Environment.Exit(1);
            }
        }
        public void Log(string message, TextWriter txtWriter, Exception ex, params object[] args)
        {
            try
            {
                string sMessageToLog = string.Empty;
                if (args == null)
                {
                    sMessageToLog = message.Trim();

                }
                else
                {
                    sMessageToLog = string.Format(message.Trim(), args);
                }
                sMessageToLog = string.Format("{0} -- {1}", DateTime.Now.ToString(), sMessageToLog.Trim());
                txtWriter.WriteLine(sMessageToLog);
                if (ex != null)
                {
                    txtWriter.WriteLine(ex.Message);
                    txtWriter.WriteLine(ex.StackTrace);

                    if (ex.InnerException != null)
                    {
                        txtWriter.WriteLine(ex.InnerException.Message);
                        txtWriter.WriteLine(ex.InnerException.StackTrace);
                    }
                }
                txtWriter.Close();
            }
            catch (Exception ex2)
            {
                string Exception = ex2.Message.Substring(0, Math.Min(ex2.Message.Length, 200));

                WriteToTraceListener("Error Occured while creating the csvfile with data  ", ex2, null);
                Environment.Exit(1);
            }
        }
    }
}
