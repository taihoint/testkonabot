
using System;
using System.Net;
using System.Text;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Bot_Application1.Models;


public class APIExamMapGeocode
{
    public static LatLon ll = new LatLon();

    public static void getCodeNaver(string v1, string v2)
    {
        
        //string query = "양재동 20-9"; // 검색할 주소
        //string url = "https://openapi.naver.com/v1/map/geocode?query=" + query; // 결과가 JSON 포맷
        //string url = "https://openapi.map.naver.com/openapi/v3/maps.js?clientId=OPCP0Yh0b2IC9r59XaTR&submodules=panorama,geocoder,drawing,visualization?query="+query;
        //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        //JSL 꺼
        //request.Headers.Add("X-Naver-Client-Id", "dXUekyWEBhyYa2zD2s33"); // 클라이언트 아이디
        //request.Headers.Add("X-Naver-Client-Secret", "gaIyARjcvi");       // 클라이언트 시크릿
        //hyundai
        //request.Headers.Add("X-Naver-Client-Id", "OPCP0Yh0b2IC9r59XaTR"); // 클라이언트 아이디
        //request.Headers.Add("X-Naver-Client-Secret", "LFWCwCGlCq");       // 클라이언트 시크릿

        //HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        //string status = response.StatusCode.ToString();
        //if (status == "OK")
        //{
            //Stream stream = response.GetResponseStream();
            //StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            //string text = reader.ReadToEnd();

            //JObject jsonObj = JObject.Parse(text);

            //var x = jsonObj["result"]["items"][0]["point"]["x"];
            //var y = jsonObj["result"]["items"][0]["point"]["y"];

            //Debug.WriteLine("json value x = " + x);
            //Debug.WriteLine("json value y = " + y);

            var coordinates = v1 + "," + v2;

            var urlImg = "https://openapi.naver.com/v1/map/staticmap.bin?clientId=OPCP0Yh0b2IC9r59XaTR&url=http://www.hyundai.com&crs=EPSG:4326&center="+ coordinates + "&level=12&w=400&h=300&baselayer=default&markers=" + coordinates;
            String fileName = "c:/inetpub/wwwroot/map/" + coordinates+".png";

            System.Net.WebClient client = new System.Net.WebClient();
            client.DownloadFile(urlImg, fileName);

            //DownloadRemoteImageFile(urlImg, fileName);
            /*
            if (!DownloadRemoteImageFile(url, fileName))
            {
                Debug.WriteLine("이미지 생성 오류");
            }
            */
            //ll.lat = (string)x;
            //ll.lon = (string)y;

        //}
        //else
        //{
        //    Debug.WriteLine("Error 발생=" + status);
        //}

    }

    public static bool DownloadRemoteImageFile(string uri, string fileName)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        bool bImage = response.ContentType.StartsWith("image",
            StringComparison.OrdinalIgnoreCase);
        if ((response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Moved ||
            response.StatusCode == HttpStatusCode.Redirect) &&
            bImage)
        {
            using (Stream inputStream = response.GetResponseStream())
            using (Stream outputStream = File.OpenWrite(fileName))
            {
                byte[] buffer = new byte[4096];
                int bytesRead;
                do
                {
                    bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                    outputStream.Write(buffer, 0, bytesRead);
                } while (bytesRead != 0);
            }

            return true;
        }
        else
        {
            return false;
        }
    }
}
