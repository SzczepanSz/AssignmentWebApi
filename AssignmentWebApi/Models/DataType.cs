using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApiOnly.Models
{
    public enum DataTypeEnum
    {
        query,
        status,
        country,
        countryCode,
        region,
        regionName,
        city,
        zip,
        lat,
        lon,
        timezone,
        isp,
        org,
        AS
    }
    [Serializable]
    public class DataType
    {
       
        public long ID_DATA_TYPE { get; set; }
        public string NAME { get; set; }
        public int ID_DATA_TYPE_CLASS { get; set; }
        public DataTypeClass DATA_TYPE_CLASS { get; set; }

        public DataType() { }
        public DataType(long idDaaataType, string name, int idDataTypeClass, List<DataTypeClass> dataTypeClasses)
        {
            this.ID_DATA_TYPE = idDaaataType;
            NAME = name;
            ID_DATA_TYPE_CLASS = idDataTypeClass;
            DATA_TYPE_CLASS = getDataTypeClass(dataTypeClasses, idDataTypeClass);
        }

        private DataTypeClass getDataTypeClass(List<DataTypeClass> dataTypeClasses, long idDataTypeClass)
        {
            return  dataTypeClasses.FirstOrDefault(f => f.ID_DATA_TYPE_CLASS == idDataTypeClass);
        }
    }
}