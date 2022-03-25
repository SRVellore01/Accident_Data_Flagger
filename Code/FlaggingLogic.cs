using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Collections;
using System.Threading.Tasks;
using Geotab.Checkmate;
using Geotab.Checkmate.ObjectModel;
using AccidentDataFlagger.Code.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore;
using Newtonsoft.Json.Linq;

namespace AccidentDataFlagger.Code
{
    class FlaggingLogic
    {

        private API _api;
        private Boolean _authenticationFlag = true;
        private Boolean _setDeviceFlag = true;

        Service1 logger = new Service1();


        public API Authenticate(string userName, string password, string server, string database)
        {

            logger.WriteToTraceListener(userName + " " + password + " "+ server + " "+ database, null, null);
            _api = new API(userName, password, null, database, server, 300000);
            _api.Authenticate();
            return _api;
        }
        public void EnableRequest(String SerialNo)
        {
            logger.WriteToTraceListener("Get enable request start", null, null);
            string SQLConnection = ConfigurationManager.ConnectionStrings["SQLConnection"].ConnectionString.ToString();

            var returnVehicle = new Vehicle();
            //Authenticate


         
            

            //Get Device
            DeviceSearch deviceSearchBySerial = new DeviceSearch();
            deviceSearchBySerial.SerialNumber = SerialNo;
            logger.WriteToTraceListener(SerialNo, null, null);
            var devicesBySerial = _api.Call<List<Device>>("Get", typeof(Device), new { search = deviceSearchBySerial });



            if (devicesBySerial.Count() == 0)
            {
                using (SqlConnection connection = new SqlConnection(SQLConnection))
                using (SqlCommand cmd = new SqlCommand("AccidentDataFlaggerDeleteBadData", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("SerialNo", SqlDbType.VarChar).Value = SerialNo;
                    connection.Open();
                    cmd.ExecuteNonQuery();

                }
                logger.WriteToTraceListener("serailNo missing", null, null);
                returnVehicle.Result = "Serial Number cannot be found";

                logger.WriteToTraceListener(returnVehicle.VehicleName, null, null);
                returnVehicle.VehicleName = "";


                // return returnVehicle; //When Serial number is missing in the server or input serial number is wrong
            }

            Device deviceBySerial = devicesBySerial[0];
            GoDevice goDeviceBySerial = (GoDevice)deviceBySerial;

            //var adevice = _api.Call<List<Device>>("Get", typeof(Device), new { search = new { SerialNumber = SerialNo } });

            //var myDevice = adevice[0];

            //var groupToBeAdded = _api.Call<List<Group>>("Get", typeof(Group), new
            //{
            //    search = new
            //    {

            //        Name = "Flagged Accident Data" // The group to be added
            //                                       //Name = "%fast%" -- wildcard searches using % are also supported
            //    }
            //});

            //myDevice.Groups.Add(groupToBeAdded[0]);
            //var finalDeviceGroups = (myDevice.Groups.Where(group => group.Id != Id.Create("GroupCompanyId"))).ToList();
            //myDevice.Groups = finalDeviceGroups;
            //logger.WriteToTraceListener(finalDeviceGroups.ToString(), null, null);




            CustomParameter AccidentCustomParam = new CustomParameter();

            AccidentCustomParam.Bytes = BitConverter.GetBytes(128).ToArray(); //correct way; "gA==" is base64, convert to Hex is 80, 80 convert to decimal is 128
            AccidentCustomParam.Description = "Trigger Upload of All Collision Data";
            logger.WriteToTraceListener("Data Uploaded", null, null);
            AccidentCustomParam.IsEnabled = true;
            AccidentCustomParam.Offset = 36;

            // Set Device
            goDeviceBySerial.CustomParameters.Add(AccidentCustomParam);

            try
            {
               // var groupUpdate =  _api.Call<Device>("Set", typeof(Device), new { entity = myDevice});
                var objUpadte = _api.Call<Device>("Set", typeof(Device), new { entity = goDeviceBySerial });
            }
            catch (Exception ex)
            {
                _setDeviceFlag = false;
                logger.WriteToTraceListener("EDXCEOPTIONNN", null, null);
                logger.WriteToTraceListener("Throw Excption: " + ex, null, null);
                
            }

            if (_setDeviceFlag == false)
            {
                returnVehicle.Result = "Enable Failed";
                
            }

            returnVehicle = new Vehicle();
            returnVehicle.VehicleId = goDeviceBySerial.Id.ToString();
            returnVehicle.VehicleName = goDeviceBySerial.Name;
            returnVehicle.SerialNo = goDeviceBySerial.SerialNumber;
            returnVehicle.EngineType = goDeviceBySerial.EngineType.Name;
            returnVehicle.Result = "Enable Successfully";






            using (SqlConnection connection = new SqlConnection(SQLConnection))
            using (SqlCommand cmd = new SqlCommand("AccidentDataFlaggerEnableFlag", connection))
            {
                logger.WriteToTraceListener("enableflag SQL Procedure", null, null);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("SerialNo", SqlDbType.VarChar).Value = SerialNo;
                connection.Open();
                cmd.ExecuteNonQuery();

            }


        }
        public void DisableRequest(String SerialNo)
        {
            logger.WriteToTraceListener("Get Disable Flag Start", null, null);
            var returnVehicle = new Vehicle();
            
            var offsetRemove = 36;
            var byteToRemove = BitConverter.GetBytes(128);
            //Get Device
            DeviceSearch deviceSearchBySerial = new DeviceSearch();
            deviceSearchBySerial.SerialNumber = SerialNo;
            var devicesBySerial = _api.Call<List<Device>>("Get", typeof(Device), new { search = deviceSearchBySerial });
            
            if (devicesBySerial.Count() == 0)
            {
                returnVehicle.Result = "Serial Number cannot be found";
                returnVehicle.VehicleName = "";
              //  return returnVehicle;  //When Serial number is missing in the server or input serial number is wrong
            }

            Device deviceBySerial = devicesBySerial[0];
            GoDevice goDeviceBySerial = (GoDevice)deviceBySerial;

            for (int i = 0; i < goDeviceBySerial.CustomParameters.Count(); i++)
            {
                if (goDeviceBySerial.CustomParameters[i].Offset == offsetRemove && goDeviceBySerial.CustomParameters[i].Bytes[0] == byteToRemove[0])
                {
                    goDeviceBySerial.CustomParameters.Remove(goDeviceBySerial.CustomParameters[i]);
                    try
                    {
                        var objDeactive = _api.Call<Device>("Set", typeof(Device), new { entity = goDeviceBySerial });
                    }
                    catch (Exception ex)
                    {
                        _setDeviceFlag = false;
                        Console.WriteLine("Throw Excption: " + ex);
                        logger.WriteToTraceListener(SerialNo, null, null);
                        logger.WriteToTraceListener("EEEEEEEEEEEEEEEEEEXCEPTION", null, null);
                        logger.WriteToTraceListener(ex.Message, null, null);
                        break;
                    }
                    break;
                }
            }

            if (_setDeviceFlag == false)
            {
                returnVehicle.Result = "Enable Failed";

                logger.WriteToTraceListener("Disable Failed", null, null);
                //return returnVehicle;
            }

            returnVehicle = new Vehicle();
            returnVehicle.VehicleId = goDeviceBySerial.Id.ToString();
            returnVehicle.VehicleName = goDeviceBySerial.Name;
            returnVehicle.SerialNo = goDeviceBySerial.SerialNumber;
            returnVehicle.EngineType = goDeviceBySerial.EngineType.Name;
            returnVehicle.Result = "Disable Successfully";

            string SQLConnection = ConfigurationManager.ConnectionStrings["SQLConnection"].ConnectionString.ToString();

            using (SqlConnection connection = new SqlConnection(SQLConnection))
            using (SqlCommand cmd = new SqlCommand("AccidentDataFlaggerDisableFlag", connection))
            {
                logger.WriteToTraceListener("DisaleFlagSQL", null, null);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("SerialNo ", SqlDbType.VarChar).Value = SerialNo;
                connection.Open();
                cmd.ExecuteNonQuery();

            }

            //   return returnVehicle;
        }

        public void GeotabSDK(string userName, string password, string databaseName, string server)
        {
            //setup proxy creds
            WinHttpHandler handler = new WinHttpHandler();
            string GeotabSDKProxy = ConfigurationManager.AppSettings["GeotabSDKProxy"];
            string GeotabSDKProxyUser = ConfigurationManager.AppSettings["GeotabSDKProxyUser"];
            string GeotabSDKProxyPassword = ConfigurationManager.AppSettings["GeotabSDKProxyPassword"];
            logger.WriteToTraceListener(userName + " " + password + " " + server + " " + databaseName, null, null);

            handler.Proxy = new WebProxy(GeotabSDKProxy, 8080);
            handler.Proxy.Credentials = new NetworkCredential(GeotabSDKProxyUser, GeotabSDKProxyPassword);
            handler.WindowsProxyUsePolicy = WindowsProxyUsePolicy.UseCustomProxy;
            logger.WriteToTraceListener(GeotabSDKProxy + " " + GeotabSDKProxyUser + " " + GeotabSDKProxyPassword , null, null);
            handler.ReceiveDataTimeout = Timeout.InfiniteTimeSpan;
            handler.ReceiveHeadersTimeout = Timeout.InfiniteTimeSpan;
            handler.SendTimeout = Timeout.InfiniteTimeSpan;

            try
            {
                _api = new API(userName, password, null, databaseName, server, 300000, handler);
                _api.Authenticate();
                logger.WriteToTraceListener(_authenticationFlag.ToString(), null, null);

            }
            catch (Exception ex)
            {
                logger.WriteToTraceListener(ex.ToString(), null, null);
                logger.WriteToTraceListener(ex.Message, null, null);
                logger.WriteToTraceListener(ex.StackTrace, null, null);

                _authenticationFlag = false;
                logger.WriteToTraceListener(_authenticationFlag.ToString(), null, null);
                Console.WriteLine("Throw Excption: " + ex);
                
            }

        }
     
       
    }
}
