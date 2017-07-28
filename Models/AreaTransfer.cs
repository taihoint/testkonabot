using System;
using System.Net;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Web;
using Bot_Application1.Models;

public class AreaTransfer
{
    public static string GetAreaTransfer(string entitiesStr)
    {

        String colorName = "";

        switch (entitiesStr)
        {
            case "chalk white":
                colorName = "초크 화이트";
                break;
            case "lake silver":
                colorName = "레이크 실버";
                break;
            case "velvet dune":
                colorName = "벨벳 듄";
                break;                
            case "dark night":
                colorName = "다크 나이트";
                break;
            case "phantom black":
                colorName = "팬텀 블랙";
                break;
            case "blue lagoon":
                colorName = "블루 라군";
                break;
            case "ceramic blue":
                colorName = "세라믹 블루";
                break;
            case "tangerine comet":
                colorName = "텐저린 코멧";
                break;
            case "pulse red":
                colorName = "펄스 레드";
                break;
            case "acid yellow":
                colorName = "애시드 엘로우";
                break;

            default:
                colorName = "";
                break;
        }

        return colorName;
    }
}
