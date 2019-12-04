using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiOnly.Models
{
    [Serializable]
    public class IpAdressData
    {
        public int ID_IP_ADRESSES_DATA;
        public int ID_IP_ADRESS;
        public long ID_DATA_TYPE;
        public object VALUE;
        public DataType DATA_TYPE;
    }
}