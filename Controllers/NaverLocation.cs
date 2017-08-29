
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
            var coordinates = v1 + "," + v2;

            var urlImg = "https://openapi.naver.com/v1/map/staticmap.bin?clientId=OPCP0Yh0b2IC9r59XaTR&url=http://www.hyundai.com&crs=EPSG:4326&center="+ coordinates + "&level=12&w=400&h=300&baselayer=default&markers=" + coordinates;
            String fileName = "c:/inetpub/wwwroot/map/" + coordinates+".png";

            //파일 존재 여부 확인
            if (!System.IO.File.Exists(fileName))
            {
                System.Net.WebClient client = new System.Net.WebClient();
                client.DownloadFile(urlImg, fileName);
            }
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
