using System;
using System.Net;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Web;
using Bot_Application1.Models;
using System.Configuration;
using System.Web.Configuration;

public class PriceImageList
{
    public static Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration("/testkonabot");
    const string domainURLstr = "domainURL";
    
    public static string GetPriceImage(string model)
    {
        
        String modelURL = "";
        string domainURL = rootWebConfig.ConnectionStrings.ConnectionStrings[domainURLstr].ToString();

        switch (model)
        {
            //case "가솔린 2WD":
            //    modelURL = "D:\\bob\\01.project\\05.현대자동차 ChatBot\\01.자료\\20170710\\가격 이미지\\가격 이미지\\engine\\gasoline.jpg";
            //    break;
            //case "가솔린 4WD":
            //    modelURL = "D:\\bob\\01.project\\05.현대자동차 ChatBot\\01.자료\\20170710\\가격 이미지\\가격 이미지\\engine\\gasoline.jpg";
            //    break;
            //case "디젤 2WD":
            //    modelURL = "D:\\bob\\01.project\\05.현대자동차 ChatBot\\01.자료\\20170710\\가격 이미지\\가격 이미지\\engine\\diesel.jpg";
            //    break;
            //case "TUIX 가솔린":
            //    modelURL = "D:\\bob\\01.project\\05.현대자동차 ChatBot\\01.자료\\20170710\\가격 이미지\\가격 이미지\\engine\\tuix_gasoline.jpg";
            //    break;
            //case "TUIX 디젤":
            //    modelURL = "D:\\bob\\01.project\\05.현대자동차 ChatBot\\01.자료\\20170710\\가격 이미지\\가격 이미지\\engine\\tuix_diesel.jpg";
            //    break;

            case "가솔린 2WD":
                modelURL = domainURL +"/assets/images/price/engine/gasoline.jpg";
                break;
            case "가솔린 4WD":
                modelURL = domainURL + "/assets/images/price/engine/gasoline.jpg";
                break;
            case "디젤 2WD":
                modelURL = domainURL + "/assets/images/price/engine/diesel.jpg";
                break;
            case "TUIX 가솔린":
                modelURL = domainURL + "/assets/images/price/engine/tuix_gasoline.jpg";
                break;
            case "TUIX 디젤":
                modelURL = domainURL + "/assets/images/price/engine/tuix_diesel.jpg";
                break;

            default:
                modelURL = "";
                break;
        }

        return modelURL;
    }
}
