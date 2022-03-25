using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Data.SqlClient;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using Geotab.Checkmate;
using Geotab.Checkmate.ObjectModel;
using AccidentDataFlagger.Code.Entities;


namespace AccidentDataFlagger.Code
{
    class SQLReader
    {
        private API _api;
        private Boolean _authenticationFlag = true;
        private Boolean _setDeviceFlag = true;
        Service1 logger = new Service1();


        public static void ReadFromJobQueue()
        {
            Service1 logger = new Service1();
          logger.WriteToTraceListener("read from jobqueue", null, null);
            SQLReader.GetSerialNumber();
            EnableFlaggingProcess();
            DisableFlaggingProcess();

        }
        public static void JobStart()
        {
            //Service1 logger = new Service1();
            //logger.WriteToTraceListener("Start Job", null, null);
            JobStartTime();
        }

        public static void JobEnd()
        {
            JobEndTime();
        }

        private static void GetSerialNumber()
        {
            //"Data Source=gaalpcsv0031;Initial Catalog=TelematicsReporting_SmallPackage_Test; Persist Security Info=True ; User ID=TelAppUser; Password=Te!@0321;"
            string SQLConnection = ConfigurationManager.ConnectionStrings["SQLConnection"].ConnectionString.ToString();
            Service1 logger = new Service1();
            logger.WriteToTraceListener("Get Serial Numver Start", null, null);
            //Initilize hashTable
            Hashtable SQLNameHashTable = new Hashtable();
            Hashtable SQLSerialNumberHashTable = new Hashtable();

            int i = 0;


            using (SqlConnection connection = new SqlConnection(SQLConnection))
            using (var command = new SqlCommand("SelectFromAccidentDataJobQueue", connection) { CommandType = CommandType.StoredProcedure })
            {
                //Service1 logger = new Service1();
                logger.WriteToTraceListener("SelectFrom Accidengt Data JobQueue", null, null);
                connection.Open();
                command.ExecuteNonQuery();

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        //Console.WriteLine(reader["Vehicle Name"].ToString());
                        //SqlCommand command2 = new SqlCommand("select [SerialNumber] from [TelematicsReporting_SmallPackage_Test].[dbo].[Device] where name = " + VehicleName , connection);
                        string VehicleName = reader["VehicleName"].ToString();
                        logger.WriteToTraceListener(VehicleName, null, null);
                        SQLNameHashTable.Add(i, VehicleName);
                        i++;


                    }
                }


                connection.Close();
            }

            int Count = i;

            for (int j = 0; j < i; j++)
            {
                using (SqlConnection con = new SqlConnection(SQLConnection))
                {
                    using (SqlCommand cmd = new SqlCommand("GetSerialNumberFromDeviceTableForAccidentDataFlagger", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("SQLNameHashTableValue", SqlDbType.VarChar).Value = SQLNameHashTable[j].ToString();
                        //Service1 logger = new Service1();
                        logger.WriteToTraceListener("Serial Number", null, null);
                        con.Open();
                        cmd.ExecuteNonQuery();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {

                            while (reader.Read())
                            {

                                string SerialNumber = reader["SerialNumber"].ToString();
                                logger.WriteToTraceListener(SerialNumber, null, null);
                                // Console.Write("UPDATE [AccidentDataJobQueue] SET [Vehicle Serial Number] = " + "'" + SerialNumber + "'" + "WHERE [Vehicle Name]= " + "'" + SQLHashTable[j]+ "'");
                                SQLSerialNumberHashTable.Add(j, SerialNumber);




                            }
                        }
                    }


                }


            }
            for (int k = 0; k < Count; k++)
            {
                try
                {
                    using (SqlConnection connection3 = new SqlConnection(SQLConnection))
                    using (SqlCommand cmd = new SqlCommand("InsertSerialNumberIntoAccidentDataJobQueue", connection3))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("SQLNameHashTableValue", SqlDbType.VarChar).Value = SQLNameHashTable[k].ToString();
                        cmd.Parameters.AddWithValue("SQLSerialNumberHashTableValue", SqlDbType.VarChar).Value = SQLSerialNumberHashTable[k].ToString();
                      //  Service1 logger = new Service1();
                        logger.WriteToTraceListener("Insert Seral Number", null, null);

                        logger.WriteToTraceListener(SQLNameHashTable[k].ToString(), null, null);
                        logger.WriteToTraceListener(SQLSerialNumberHashTable[k].ToString(), null, null);



                        connection3.Open();
                        cmd.ExecuteNonQuery();
                        connection3.Close();

                    }


                }
                catch (Exception ex)
                {
                   // Service1 logger = new Service1();
                    logger.WriteToTraceListener(ex.ToString(), null, null);
                    logger.WriteToTraceListener(ex.Message, null, null);
                    logger.WriteToTraceListener(ex.StackTrace, null, null);
                    continue;

                }


            }


            logger.WriteToTraceListener("Get Serial Numver End", null, null);
        }

        private static void EnableFlaggingProcess()
        {
            Service1 logger = new Service1();
            logger.WriteToTraceListener("Get enable Flag Start", null, null);

            string SQLConnection = ConfigurationManager.ConnectionStrings["SQLConnection"].ConnectionString.ToString();

            Hashtable VehiclesThatNeedToBeFlagged = new Hashtable();
            int i = 0;

            using (SqlConnection connection = new SqlConnection(SQLConnection))
            using (SqlCommand cmd = new SqlCommand("GetNullFlaggedVehicles", connection))
            {

              //  logger.WriteToTraceListener("Get null Flag ", null, null);
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {

                        logger.WriteToTraceListener("inside While Read", null, null);

                        string SerialNumber = reader["VehicleSerialNumber"].ToString();
                        logger.WriteToTraceListener(SerialNumber, null, null);
                        VehiclesThatNeedToBeFlagged.Add(i, SerialNumber);

                        i++;




                    }
                }


                connection.Close();
            }

            if (VehiclesThatNeedToBeFlagged[0] != null)
            {
                //authenication to the geotab server
                FlaggingLogic p = new FlaggingLogic();
                string GeotabSDKUser = ConfigurationManager.AppSettings["GeotabSDKUser"];
                string GeotabSDKPassword = ConfigurationManager.AppSettings["GeotabSDKPassword"];
                string GeotabSDKDatabase = ConfigurationManager.AppSettings["GeotabSDKDatabase"];
                string GeotabSDKServer = ConfigurationManager.AppSettings["GeotabSDKServer"];
                p.GeotabSDK(GeotabSDKUser, GeotabSDKPassword, GeotabSDKDatabase, GeotabSDKServer);
              

                for (int j = 0; j < i; j++)
                {
                    if (VehiclesThatNeedToBeFlagged[j].ToString() == "")
                    {
                        continue;
                    }
                    else
                    {

                        p.EnableRequest(VehiclesThatNeedToBeFlagged[j].ToString());
                       
                    }


                }

            }







        }
        private static void DisableFlaggingProcess()
        {
            //DISABLING PROCESS
            Service1 logger = new Service1();
            logger.WriteToTraceListener("disabled Process Starting ", null, null);
            string SQLConnection = ConfigurationManager.ConnectionStrings["SQLConnection"].ConnectionString.ToString();
            Hashtable VehiclesThatNeedToBeUnflagged = new Hashtable();
            int k = 0;



            using (SqlConnection connection = new SqlConnection(SQLConnection))
            using (SqlCommand cmd = new SqlCommand("GetProcessedVehicles", connection) { CommandType = CommandType.StoredProcedure })
            {
                logger.WriteToTraceListener("Command gone through", null, null);
               // cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    logger.WriteToTraceListener("Command gone through2", null, null);
                    while (reader.Read())
                    {


                        string SerialNumber = reader["VehicleSerialNumber"].ToString();
                        logger.WriteToTraceListener(SerialNumber, null, null);
                        VehiclesThatNeedToBeUnflagged.Add(k, SerialNumber);

                        k++;




                    }
                }


                connection.Close();
            }
            //logger.WriteToTraceListener(VehiclesThatNeedToBeUnflagged[0].ToString(), null, null);
            if (VehiclesThatNeedToBeUnflagged[0] != null)
            {

                FlaggingLogic p = new FlaggingLogic();
                string GeotabSDKUser = ConfigurationManager.AppSettings["GeotabSDKUser"];
                string GeotabSDKPassword = ConfigurationManager.AppSettings["GeotabSDKPassword"];
                string GeotabSDKDatabase = ConfigurationManager.AppSettings["GeotabSDKDatabase"];
                string GeotabSDKServer = ConfigurationManager.AppSettings["GeotabSDKServer"];
                p.GeotabSDK(GeotabSDKUser, GeotabSDKPassword, GeotabSDKDatabase, GeotabSDKServer);
                for (int j = 0; j < k; j++)
                {
                    logger.WriteToTraceListener(VehiclesThatNeedToBeUnflagged[j].ToString(), null, null);
                    p.DisableRequest(VehiclesThatNeedToBeUnflagged[j].ToString());


                }

            }



        }

        private static void JobStartTime()
        {
            string SQLConnection = ConfigurationManager.ConnectionStrings["SQLConnection"].ConnectionString.ToString();

            using (var conn = new SqlConnection(SQLConnection))
            using (var command = new SqlCommand("AccidentDataFlaggerJobStartTime", conn) { CommandType = CommandType.StoredProcedure })
            {
                conn.Open();
                command.ExecuteNonQuery();
            }



        }
        private static void JobEndTime()
        {
            string SQLConnection = ConfigurationManager.ConnectionStrings["SQLConnection"].ConnectionString.ToString();

            using (var conn = new SqlConnection(SQLConnection))
            using (var command = new SqlCommand("AccidentDataFlaggerJobEndTime", conn) { CommandType = CommandType.StoredProcedure })
            {
                conn.Open();
                command.ExecuteNonQuery();
            }





        }

      



    }
}
