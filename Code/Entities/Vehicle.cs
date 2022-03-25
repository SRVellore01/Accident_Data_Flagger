using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccidentDataFlagger.Code.Entities
{
    public class Vehicle
    {
        private string vehicleId;
        private string vehicleName;
        private string serialNo;
        private string engineType;
        private string result;

        public string VehicleId
        {
            get
            {
                return vehicleId;
            }

            set
            {
                vehicleId = value;
            }
        }

        public string VehicleName
        {
            get
            {
                return vehicleName;
            }

            set
            {
                vehicleName = value;
            }
        }

        public string SerialNo
        {
            get
            {
                return serialNo;
            }

            set
            {
                serialNo = value;
            }
        }

        public string EngineType
        {
            get
            {
                return engineType;
            }

            set
            {
                engineType = value;
            }
        }

        public string Result
        {
            get
            {
                return result;
            }

            set
            {
                result = value;
            }
        }

    }
}
