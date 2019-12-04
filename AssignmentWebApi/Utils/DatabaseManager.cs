using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using WebApiOnly.Models;
using static WebApiOnly.Utils.UtilsJsonManager;

namespace WebApiOnly.Utils
{
    public class DatabaseManager
    {
        public List<DataType> listDataTypes = null; 

        public DatabaseManager()
        {
            GetDataTypes();
        }
        private void GetDataTypes()
        {      
            try
            {
                List<DataTypeClass> dataTypeClasses = selectDataTypeClasses(GetConnectionString());
                listDataTypes = selectDataTypes(GetConnectionString(), dataTypeClasses);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        internal Response GetIpData(string ip)
        {
            Response response = null;
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                SqlCommand commandIP = new SqlCommand("SELECT*FROM[AssignmentDB].[dbo].[IP_ADRESSES] WHERE [IP_ADRESS] = @IP_ADRESS", connection);
                commandIP.Parameters.AddWithValue("@IP_ADRESS", ip);
                SqlDataReader reader = commandIP.ExecuteReader();

                IpAdress ipAdress = null;
                while (reader.Read())
                {
                    ipAdress = new IpAdress()
                    {
                        ID_IP_ADRESS = reader.GetInt32(0),
                        IP_ADRESS = reader.GetString(1)
                    };
                }
                reader.Close();
                if (ipAdress != null)
                {
                    ipAdress.ipAdressData = new List<IpAdressData>();
                    SqlCommand commandIpData = new SqlCommand("SELECT*FROM[AssignmentDB].[dbo].[IP_ADRESSES_DATA] WHERE [ID_IP_ADRESS] = @ID_IP_ADRESS", connection);
                    commandIpData.Parameters.AddWithValue("@ID_IP_ADRESS", ipAdress.ID_IP_ADRESS);
                    reader = commandIpData.ExecuteReader();
                    while (reader.Read())
                    {
                        IpAdressData ipAdressData = new IpAdressData()
                        {
                            ID_IP_ADRESSES_DATA = reader.GetInt32(0),
                            ID_IP_ADRESS = ipAdress.ID_IP_ADRESS,
                            ID_DATA_TYPE = reader.GetInt64(2),
                            VALUE = reader.GetSqlValue(3),
                            DATA_TYPE = GetDataType(reader.GetInt64(2))
                        };
                        ipAdress.ipAdressData.Add(ipAdressData);
                    }
                }
                response = ConvertToIpApi(ipAdress);
                connection.Close();              
            }

            return response;
        }

        private DataType GetDataType(object iD_DATA_TYPE)
        {
            DataType dataType = null;
            long idDataType = Convert.ToInt64(iD_DATA_TYPE);
            if (iD_DATA_TYPE != null)
            {
                dataType= listDataTypes.FirstOrDefault(f => f.ID_DATA_TYPE == idDataType);
            }

            return dataType;
        }
    

        internal bool CheckIfExistInDB(string ip, out int id)
        {
            id = 0;
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                SqlCommand commandIP = new SqlCommand("SELECT*FROM[AssignmentDB].[dbo].[IP_ADRESSES] WHERE [IP_ADRESS] = @IP_ADRESS", connection);
                commandIP.Parameters.AddWithValue("@IP_ADRESS", ip);
                SqlDataReader reader = commandIP.ExecuteReader();

                while (reader.Read())
                {
                    id = reader.GetInt32(0);
                    return true;
                }
                return false;
            }
        }

        List<DataType> selectDataTypes(string connectionString, List<DataTypeClass> dataTypeClasses)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                List<DataType> dataTypes = new List<DataType>();
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT*FROM[AssignmentDB].[dbo].[DATA_TYPE]", connection);            
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    DataType dataType = new DataType(reader.GetInt64(0), reader.GetString(1), reader.GetInt32(2), dataTypeClasses);               
                    dataTypes.Add(dataType);
                }
                connection.Close();
                return dataTypes;             
            }
        }

        internal Response ConvertToIpApi(IpAdress ipAdress = null, Dictionary<string, JToken> geolocationData = null)
        {
            Response ipApi = null;
            if (ipAdress!=null)
            {             
                var lonOb = ipAdress.ipAdressData.FirstOrDefault(f => f.ID_DATA_TYPE == (int)DataTypeEnum.lon)?.VALUE;
                decimal lon = 0;
                decimal.TryParse(lonOb.ToString(), out lon);

                var latOb = ipAdress.ipAdressData.FirstOrDefault(f => f.ID_DATA_TYPE == (int)DataTypeEnum.lat)?.VALUE;
                decimal lat = 0;
                decimal.TryParse(lonOb.ToString(), out lat);

                ipApi = new Response()
                {
                    status = ResponseEnum.TransacionSucces.ToString(),
                    country = ipAdress.ipAdressData.FirstOrDefault(f => f.ID_DATA_TYPE == (int)DataTypeEnum.country)?.VALUE.ToString(),
                    countryCode = ipAdress.ipAdressData.FirstOrDefault(f => f.ID_DATA_TYPE == (int)DataTypeEnum.countryCode)?.VALUE.ToString(),
                    region = ipAdress.ipAdressData.FirstOrDefault(f => f.ID_DATA_TYPE == (int)DataTypeEnum.region)?.VALUE.ToString(), 
                    regionName = ipAdress.ipAdressData.FirstOrDefault(f => f.ID_DATA_TYPE == (int)DataTypeEnum.regionName).VALUE.ToString(),
                    city = ipAdress.ipAdressData.FirstOrDefault(f => f.ID_DATA_TYPE == (int)DataTypeEnum.city)?.VALUE.ToString(),
                    zip = ipAdress.ipAdressData.FirstOrDefault(f => f.ID_DATA_TYPE == (int)DataTypeEnum.zip)?.VALUE.ToString(),
                    lat = lat,
                    lon = lon,
                    timezone = ipAdress.ipAdressData.FirstOrDefault(f => f.ID_DATA_TYPE == (int)DataTypeEnum.timezone)?.VALUE.ToString(),
                    isp = ipAdress.ipAdressData.FirstOrDefault(f => f.ID_DATA_TYPE == (int)DataTypeEnum.isp)?.VALUE.ToString() ,
                    org = ipAdress.ipAdressData.FirstOrDefault(f => f.ID_DATA_TYPE == (int)DataTypeEnum.org)?.VALUE.ToString(),
                    AS = ipAdress.ipAdressData.FirstOrDefault(f => f.ID_DATA_TYPE == (int)DataTypeEnum.AS)?.VALUE.ToString()
                };
            }
            if (geolocationData != null && geolocationData["status"].ToString() != "fail")
            {
                var lonOb = geolocationData.ContainsKey(DataTypeEnum.lon.ToString()) ? geolocationData[DataTypeEnum.lon.ToString()].ToString() : string.Empty;
                decimal lon = 0;
                decimal.TryParse(lonOb, out lon);

                var latOb = geolocationData.ContainsKey(DataTypeEnum.lat.ToString()) ? geolocationData[DataTypeEnum.lat.ToString()].ToString() : string.Empty;
                decimal lat = 0;
                decimal.TryParse(latOb, out lat);

                ipApi = new Response()
                {
                    status = ResponseEnum.TransacionSucces.ToString(),
                    country = geolocationData.ContainsKey(DataTypeEnum.country.ToString()) ? geolocationData[DataTypeEnum.country.ToString()].ToString() : string.Empty,
                    countryCode = geolocationData.ContainsKey(DataTypeEnum.countryCode.ToString()) ? geolocationData[DataTypeEnum.countryCode.ToString()].ToString() : string.Empty,
                    region = geolocationData.ContainsKey(DataTypeEnum.region.ToString()) ? geolocationData[DataTypeEnum.region.ToString()].ToString() : string.Empty,
                    regionName = geolocationData.ContainsKey(DataTypeEnum.regionName.ToString()) ? geolocationData[DataTypeEnum.regionName.ToString()].ToString() : string.Empty,
                    city = geolocationData.ContainsKey(DataTypeEnum.city.ToString()) ? geolocationData[DataTypeEnum.city.ToString()].ToString() : string.Empty,
                    zip = geolocationData.ContainsKey(DataTypeEnum.zip.ToString()) ? geolocationData[DataTypeEnum.zip.ToString()].ToString() : string.Empty,
                    lat = lat,
                    lon = lon,
                    timezone = geolocationData.ContainsKey(DataTypeEnum.timezone.ToString()) ? geolocationData[DataTypeEnum.timezone.ToString()].ToString() : string.Empty,
                    isp = geolocationData.ContainsKey(DataTypeEnum.isp.ToString()) ? geolocationData[DataTypeEnum.isp.ToString()].ToString() : string.Empty,
                    org = geolocationData.ContainsKey(DataTypeEnum.org.ToString()) ? geolocationData[DataTypeEnum.org.ToString()].ToString() : string.Empty,
                    AS = geolocationData.ContainsKey(DataTypeEnum.AS.ToString()) ? geolocationData[DataTypeEnum.AS.ToString()].ToString() : string.Empty,
                };
            }
            return ipApi;
        }

        internal ResponseEnum InsertNewIpData(Dictionary<string, JToken> jsonDictionary)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                int id = 0;
                string ip = jsonDictionary.ContainsKey(DataTypeEnum.query.ToString()) ? jsonDictionary[DataTypeEnum.query.ToString()].ToString(): string.Empty;
                if (ip.Count()>0 && !CheckIfExistInDB(ip, out id))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("INSERT INTO [AssignmentDB].[dbo].[IP_ADRESSES]([IP_ADRESS]) VALUES(@IP_ADRESS);  SELECT SCOPE_IDENTITY()", connection);
                    command.Parameters.AddWithValue("@IP_ADRESS", ip);
                    var  ipResult = command.ExecuteScalar();

                    int.TryParse(ipResult.ToString(),out id);
                    if (id!=0)
                    {
                        foreach (var item in jsonDictionary)
                        {
                            DataType dataType = listDataTypes.Find(f => f.NAME == item.Key);
                            if (dataType != null && listDataTypes.Select(f => f.NAME).Contains(item.Key.ToString()))
                            {
                                command = new SqlCommand("INSERT INTO [AssignmentDB].[dbo].[IP_ADRESSES_DATA]([ID_IP_ADRESS],[ID_DATA_TYPE],[VALUE]) VALUES(@ID_IP_ADRESS,@ID_DATA_TYPE,@VALUE); SELECT SCOPE_IDENTITY()", connection);
                                command.Parameters.AddWithValue("@ID_IP_ADRESS", ipResult);
                                command.Parameters.AddWithValue("@ID_DATA_TYPE", dataType.ID_DATA_TYPE);
                                command.Parameters.AddWithValue("@VALUE", item.Value.ToString());

                                int ipDataResult = command.ExecuteNonQuery();
                            }                          
                        }
                    }
                    connection.Close();
                }
                else
                {
                    return ResponseEnum.TransactionFailed;
                }             
            }
            return ResponseEnum.TransacionSucces;
        }
   
        List<DataTypeClass> selectDataTypeClasses(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                List<DataTypeClass> dataTypeClasses = new List<DataTypeClass>();
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT*FROM[AssignmentDB].[dbo].[DATA_TYPE_CLASS]", connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    DataTypeClass dataTypeClass = new DataTypeClass()
                    {
                        ID_DATA_TYPE_CLASS = reader.GetInt32(0),
                        NAME = reader.GetString(1),                     
                    };
                    dataTypeClasses.Add(dataTypeClass);
                }
                connection.Close();
                return dataTypeClasses;
            }
        }

        internal ResponseEnum DeleteIpData(Dictionary<string, JToken> ip)
        {
            string Ip = ip.FirstOrDefault().Value.ToString();
            int id = 0;
            if (Ip !=null && CheckIfExistInDB(Ip, out id))
            {
                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand("DELETE [AssignmentDB].[dbo].[IP_ADRESSES_DATA] WHERE [ID_IP_ADRESS] = @ID_IP_ADRESS", connection);
                    command.Parameters.AddWithValue("@ID_IP_ADRESS", id);
                    command.ExecuteNonQuery();

                    command = new SqlCommand("DELETE [AssignmentDB].[dbo].[IP_ADRESSES] WHERE [ID_IP_ADRESS] = @ID_IP_ADRESS", connection);
                    command.Parameters.AddWithValue("@ID_IP_ADRESS", id);
                    command.ExecuteNonQuery();

                    connection.Close();
                }
            }
            return ResponseEnum.TransacionSucces;
        }

        string GetConnectionString()
        {
            return  ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
        }
    }
}