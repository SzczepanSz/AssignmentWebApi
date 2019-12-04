using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiOnly.Models
{
    [Serializable]
    public class IpAdress
    {
        public int ID_IP_ADRESS { get; set; }
        public string IP_ADRESS { get; set; }
        public List< IpAdressData> ipAdressData { get; set; }
    }
}