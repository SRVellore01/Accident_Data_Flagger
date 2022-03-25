using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using Microsoft.Office.Interop.Excel;
using _Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.IO;

namespace AccidentDataFlagger.Code
{
    class GetNameOfOragamiFile
    {
        public string FirstFileNameGet()
        {
            string Destination = ConfigurationManager.AppSettings["FileLocation"];
            string DestinationBackup = ConfigurationManager.AppSettings["FileLocationBackup"];
            var file = Directory.GetFiles(@"" + Destination, "*.*")
           .FirstOrDefault(f => f != @"" + Destination);
            String path2 = file.Substring(file.LastIndexOf('\\') + 1);
            return path2;
        }
        public string FirstFileNameBackupGet()
        {

            string Destination = ConfigurationManager.AppSettings["FileLocation"];
            string DestinationBackup = ConfigurationManager.AppSettings["FileLocationBackup"];

            var file = Directory.GetFiles(@"" + DestinationBackup, "*.*")
           .FirstOrDefault(f => f != @"" + DestinationBackup);
            String path2 = file.Substring(file.LastIndexOf('\\') + 1);
            return path2;
        }

        public int FileNumberInDirectoryGet()
        {

            string Destination = ConfigurationManager.AppSettings["FileLocation"];
            string DestinationBackup = ConfigurationManager.AppSettings["FileLocationBackup"];

            var fileCount = (from file in Directory.EnumerateFiles(@"" + Destination, "*.csv", SearchOption.AllDirectories) select file).Count();
            return fileCount;


        }


        public int FileNumberInBackupDirectoryGet()
        {

            string Destination = ConfigurationManager.AppSettings["FileLocation"];
            string DestinationBackup = ConfigurationManager.AppSettings["FileLocationBackup"];

            var fileCount = (from file in Directory.EnumerateFiles(@"" + DestinationBackup, "*.csv", SearchOption.AllDirectories) select file).Count();
            return fileCount;


        }

    }
}
