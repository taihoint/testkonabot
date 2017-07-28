using System;
using System.Net;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Web;
using Bot_Application1.Models;

public class Geolocation
{ 
    public static LatLon ll = new LatLon();


    public static void  getRegion()
    {

        String UserIP = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
        if (string.IsNullOrEmpty(UserIP))
        {
            UserIP = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        }
        //테스용
        //UserIP = "121.136.66.204";  
        UserIP = "110.70.57.172";
        Debug.WriteLine("UserIP = " + UserIP);
        /*
        string ipResponse = IPRequestHelper("http://ip-api.com/xml/"+ UserIP);

        //return ipResponse;
        XmlDocument ipInfoXML = new XmlDocument();
        ipInfoXML.LoadXml(ipResponse);
        XmlNodeList responseXML = ipInfoXML.GetElementsByTagName("query");

        try {
            Debug.WriteLine("SUCCEEEEEEEEEEEEEEEEEEESS");
            ll.regionName = responseXML.Item(0).ChildNodes[4].InnerText.ToString(); // region Code
            Debug.WriteLine("ll.regionName = " + ll.regionName.ToString());
            //ll.lat = responseXML.Item(0).ChildNodes[7].InnerText.ToString(); // lat Code
            //ll.lon = responseXML.Item(0).ChildNodes[8].InnerText.ToString();  // lon Code 
        }
        catch {
            Debug.WriteLine("ERRRRRRRRRRRRRRRRRRRRRRRROR");
            //127.0.0.1 일 경우 지역 값 서울로 셋팅
            ll.regionName = "Seoul";
        }     
        */
        string ipResponse = IPRequestHelper("http://freegeoip.net/xml/" + UserIP);

        //return ipResponse;
        XmlDocument ipInfoXML = new XmlDocument();
        ipInfoXML.LoadXml(ipResponse);
        XmlNodeList responseXML = ipInfoXML.GetElementsByTagName("Response");

        try
        {            
            ll.regionName = responseXML.Item(0)["RegionName"].InnerText.ToString(); // region Code

            Debug.WriteLine("ll.regionName = " + ll.regionName.ToString());
            Debug.WriteLine("SUCCEEEEEEEEEEEEEEEEEEESS");
            //ll.lat = responseXML.Item(0).ChildNodes[7].InnerText.ToString(); // lat Code
            //ll.lon = responseXML.Item(0).ChildNodes[8].InnerText.ToString();  // lon Code 
        }
        catch
        {
            Debug.WriteLine("ERRRRRRRRRRRRRRRRRRRRRRRROR");
            //127.0.0.1 일 경우 지역 값 서울로 셋팅
            ll.regionName = "Seoul";
        }

    }

    public static string IPRequestHelper(string url)
    {

        HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
        HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();

        StreamReader responseStream = new StreamReader(objResponse.GetResponseStream());
        string responseRead = responseStream.ReadToEnd();

        responseStream.Close();
        responseStream.Dispose();

        return responseRead;
    }
}
