using System;
using System.Configuration;
using WinSCP;

namespace AccidentDataFlagger.Code
{
    public class OragamiFilesDownloader
    {
        Service1 logger = new Service1();

        public int GetFiles()
        {
            logger.WriteToTraceListener("program start", null, null);

            try
            {
                string hostName = ConfigurationManager.AppSettings["HostName"];
                string userName = ConfigurationManager.AppSettings["OragamiSFTPUser"];
                string password = ConfigurationManager.AppSettings["Password"];
                int portNumber = Convert.ToInt32(ConfigurationManager.AppSettings["PortNumber"].ToString()); 
                string sshHostKeyFingerPrint = ConfigurationManager.AppSettings["SshHostKeyFingerPrint"];
                string Destination = ConfigurationManager.AppSettings["FileLocation"];
                string DestinationBackup = ConfigurationManager.AppSettings["FileLocationBackup"];


                // Setup session options
                SessionOptions sessionOptions = new SessionOptions
                {
                    

                Protocol = Protocol.Sftp,
                    HostName = hostName,
                    UserName = userName,
                    Password = password,
                    PortNumber = portNumber,
                    SshHostKeyFingerprint = sshHostKeyFingerPrint
                };

                using (Session session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);

                    // Upload files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.FileMask = "|Backup*/; Archive*/;";
                    transferOptions.TransferMode = TransferMode.Binary;

                    string FileLocation = ConfigurationManager.AppSettings["FileLocation"];

                    TransferOperationResult transferResult;
                    transferResult =
                        session.GetFiles(@"/", @""+ Destination, false, transferOptions);
                        session.GetFiles(@"/", @"" + DestinationBackup, false, transferOptions);


                    transferResult.Check();

                    // Print results
                    foreach (TransferEventArgs transfer in transferResult.Transfers)
                    {
                        // Console.WriteLine("Upload of {0} succeeded", transfer.FileName);
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                logger.WriteToTraceListener(e.ToString(), null, null);


                Console.WriteLine("Error: {0}", e);
                return 1;
            }
        }


        public void MoveToArchive(String fileName)
        {
            try
            {
                // Setup session options
                SessionOptions sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Sftp,
                    HostName = "ftp2.ups.com",
                    UserName = "OrigamiSFTP",
                    Password = "v9X17UW03qC",
                    PortNumber = 10022,
                    SshHostKeyFingerprint = "ssh-rsa 2048 HUBqpoH+WHd0kyyrHu9LX0IHUoFblmq041+cowtLumk="
                };

                using (Session session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);
                    session.MoveFile(fileName, "/Archive/");


                }


            }
            catch (Exception e)
            {
                logger.WriteToTraceListener(e.ToString(), null, null);
                Console.WriteLine("Error: {0}", e);

            }





        }


    }
}