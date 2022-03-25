using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Configuration;
using System.IO;
using System.Globalization;
using Microsoft.VisualBasic.FileIO;

namespace AccidentDataFlagger.Code
{
    class CSVReaderWriter
    {

      
        public void IterateThroughCSV()
        {
            
            string FileLocationFolder = ConfigurationManager.AppSettings["FileLocation"];
            string FileLocationFolderWithSlash = ConfigurationManager.AppSettings["FileLocationSlash"];
            //string FileLocationBackup = Configuration z
            string FileLocationBackupFolderWithSlash = ConfigurationManager.AppSettings["FileLocationBackupSlash"];


            int FileNumber = new GetNameOfOragamiFile().FileNumberInDirectoryGet();
            string[] fileArray = Directory.GetFiles(@"" + FileLocationFolder, "*.csv");
            Hashtable HashTable = new Hashtable();
            for (int i = 0; i < FileNumber; i++)
            {
                HashTable.Add(i, fileArray[i]);
            }

            for (int i = 0; i < FileNumber; i++)
            {
                string fileName = fileArray[i];
                //Checks if files exist in backup, if they DONT, copy them over and continue.  If they DO then skip this file and delete it, since it was already read on the Database  
                string fileName2 = fileName.Substring(55, 46);
          
                FileInfo file = new FileInfo(@""+ FileLocationFolderWithSlash + fileName2);
                new CSVReaderWriter().ReadWriteDoc(fileName);
                new OragamiFilesDownloader().MoveToArchive(fileName2);
                //Deletes empty rowz ---
                // fileName is in this format d:\OragamiFiles\Telematics_Crash_Data_.DT20210001.TM040049.csv 
              //  File.Delete(fileName);







            }


        }

        public void ReadWriteDoc(string fileName)
        {

            var path = @"" + fileName;
            List<String> lines = new List<String>();
            using (StreamReader reader = new StreamReader(System.IO.File.OpenRead(@"" + fileName)))
            {
                String line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(","))
                    {
                        String[] split = line.Split(',');

                        //condition for Edit record like : split[1] == "abc" etc.)
                        String Date = split[0].ToString();
                        String Time = split[1].ToString();
                        String DateTime = "";
                        //Edits and merges Date and Time
                        if (split[0] == "Date of Accident")
                        {
                            split[0] = "DateofAccident";
                            //changing this into a different coloumn all together
                            split[1] = "WeeksTakenToProcess";
                            split[2] = "ClaimNumber";
                            split[3] = "VehicleName";
                            split[4] = "VehicleTypeDescription";
                            split[5] = "SafetyTier";
                        }

                        if (split[0] != "DateofAccident")
                        {
                            //if the date is like 7/1/2021
                            if (Date[3] == '/' && Date[1] == '/')
                            {
                                Date = Date.Substring(4, 4) + "-0" + Date.Substring(0, 1) + "-0" + Date.Substring(2, 1);

                            }
                            //if date is 12/12/2021
                            if (Date[2] == '/' && Date[5] == '/')
                            {
                                Date = Date.Substring(6, 4) + "-" + Date.Substring(0, 2) + "-" + Date.Substring(3, 2);

                            }
                            //if date is like 7/12/2021
                            if (Date[4] == '/' && Date[1] == '/')
                            {
                                Date = Date.Substring(5, 4) + "-0" + Date.Substring(0, 1) + "-" + Date.Substring(2, 2);

                            }
                            //if date is like 12/7/2021
                            if (Date[4] == '/' && Date[2] == '/')
                            {
                                Date = Date.Substring(5, 4) + "-" + Date.Substring(0, 2) + "-0" + Date.Substring(3, 1);

                            }


                            int Length = Time.Length;

                            if (Length == 2)
                            {
                                Time = "00:" + Time;

                            }
                            if (Length == 3)
                            {
                                Time = "0" + Time.Substring(0, 1) + ":" + Time.Substring(1, 2);

                            }
                            if (Length == 4)
                            {

                                Time = Time.Substring(0, 2) + ":" + Time.Substring(2, 2);
                            }
                            DateTime = Date + " " + Time + ":00";
                            split[0] = DateTime;
                            split[1] = "0";
                        }

                        //Edits PKG out
                        if (split[3] != "VehicleName")
                        {

                            string VehicleName = split[3].ToString();
                            if (VehicleName.Length == 10)
                            {
                                split[3] = VehicleName.Substring(4, 6);
                            }
                            if (VehicleName.Length == 6)
                            {

                            }
                            if (VehicleName.Length == 9)
                            {
                                split[3] = VehicleName.Substring(4, 5);
                            }
                            if (VehicleName.Length == 8)
                            {
                                split[3] = VehicleName.Substring(4, 4);
                            }
                            
                        }
                        line = String.Join(",", split);
                        lines.Add(line);

                        if (split[0] != "DateofAccident")
                        {
                            
                            string Tier1 = ConfigurationManager.AppSettings["tier1"];
                            string Tier2 = ConfigurationManager.AppSettings["tier2"];
                            string Tier3 = ConfigurationManager.AppSettings["tier3"];
                            //Add Vehicles to this if you dont want to exclude them.
                            if (split[4] != "Package Car" && split[4] != "Package Car (Ready Team Member)")
                            {
                                lines.Remove(line);
                            }
                            if (split[5] != Tier3 && split[5] != Tier2 && split[5] != Tier1)
                            {
                                lines.Remove(line);
                            }
                        }


                    }

                }
            }

            using (StreamWriter writer = new StreamWriter(@"" + fileName, false))
            {
                foreach (String line in lines)
                    writer.WriteLine(line);
            }

            InsertDataIntoSQLServer(fileName);
        }
        private static void InsertDataIntoSQLServer(string csv_file_path)
        {
            using (StreamReader reader = new StreamReader(System.IO.File.OpenRead(@"" + csv_file_path)))
            {
                String line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(","))
                    {
                        String[] split = line.Split(',');

                        //condition for Edit record like : split[1] == "abc" etc.)\

                        String DateOfAccident = split[0].ToString();
                        Console.Write(DateOfAccident);
                        String WeeksTakenToProcess = split[1].ToString();
                        String ClaimNumber = split[2].ToString();
                        String VehicleName = split[3].ToString();
                        String VehicleTypeDescription = split[4].ToString();
                        String SafetyTier = split[5].ToString();

                        if (split[0].ToString() != "DateofAccident")
                        {
                            string SQLConnection = ConfigurationManager.ConnectionStrings["SQLConnection"].ConnectionString.ToString();
                            using (SqlConnection connection = new SqlConnection(SQLConnection))
                            using (SqlCommand cmd = new SqlCommand("AccidentDataFlaggerInsertData", connection))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("DateOfAccident", SqlDbType.VarChar).Value = DateOfAccident;
                                cmd.Parameters.AddWithValue("VehicleName", SqlDbType.VarChar).Value = VehicleName;
                                cmd.Parameters.AddWithValue("ClaimNumber", SqlDbType.VarChar).Value = ClaimNumber;
                                cmd.Parameters.AddWithValue("VehicleTypeDescription", SqlDbType.VarChar).Value = VehicleTypeDescription;
                                cmd.Parameters.AddWithValue("SafetyTier", SqlDbType.VarChar).Value = SafetyTier;
                                cmd.Parameters.AddWithValue("WeeksTakenToProcess", SqlDbType.VarChar).Value = WeeksTakenToProcess;



                                connection.Open();
                                cmd.ExecuteNonQuery();
                                connection.Close();

                            }
                        }


                    }
                }

            }
        }



    }
}
