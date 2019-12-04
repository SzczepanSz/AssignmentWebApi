using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using WebApiOnly.Models;
using WebApiOnly.Utils;
using static WebApiOnly.Utils.UtilsJsonManager;

namespace WebApiOnly.Controllers
{
    public class IpStackController : ApiController
    {
        DatabaseManager dbManager = null;
        // GET: api/IpStack/Get  
        public Response GetIP(string ip)
        {
            Response response = null;
            Dictionary<string, JToken> geolocationData = null;
            try
            {
                dbManager = new DatabaseManager();
                int id = 0;
                bool exist = dbManager.CheckIfExistInDB(ip, out id);

                if (exist)
                {
                    response = dbManager.GetIpData(ip);
                }
                else
                {
                    geolocationData = GetJsonData(ip);
                    response = dbManager.ConvertToIpApi(geolocationData: geolocationData);
                }
                if (response==null)
                {
                    response = new Response() { status = ResponseEnum.TransactionFailed.ToString() };
                    return response;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return response;
        }

        //GET: api/IpStack/Get
        public Response GetURL(string url)
        {
            Response response = null;
            Dictionary<string, JToken> geolocationData = null;
            try
            {
                Uri myUri = new Uri(url);
                var ip = Dns.GetHostAddresses(myUri.Host)[0];
                dbManager = new DatabaseManager();
                int id;
                bool exist = dbManager.CheckIfExistInDB(ip.ToString(), out id);

                if (exist && ip != null)
                {
                    response = dbManager.GetIpData(ip.ToString());
                }
                else if(ip != null)
                {
                    geolocationData = GetJsonData(ip.ToString());
                    response = dbManager.ConvertToIpApi(geolocationData: geolocationData);
                }
                else
                {
                    response.status = ResponseEnum.TransactionFailed.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return response;      
        }

        // POST: api/IpStack/Post
        public Response Post(JObject jsonObject)
        {
            Response response = null;
            try
            {            
                dbManager = new DatabaseManager();
                Dictionary<string, JToken> geolocationData = GetJsonDictionary(jsonObject);                                                                                                  
                ResponseEnum res =  dbManager.InsertNewIpData(geolocationData);

                if (res==ResponseEnum.TransactionFailed)
                {
                    response = new Response() { status = ResponseEnum.TransactionFailed.ToString() };
                    return response;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return new Response() { status = ResponseEnum.TransacionSucces.ToString() }; ;
        }


        // DELETE: api/IpStack/Delete
        public Response Delete(JObject jsonObject)
        {
            Response response = null;
            try
            {
                dbManager = new DatabaseManager();
                Dictionary<string, JToken> ip = GetJsonDictionary(jsonObject);
                dbManager = new DatabaseManager();
                ResponseEnum  resp = dbManager.DeleteIpData(ip);           
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return response;
        }
    }
}