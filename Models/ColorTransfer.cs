using System;

public class ColorTransfer
{
    public static string GetColorTransfer(string entitiesStr)
    {

        String colorName = "";

        switch (entitiesStr)
        {
            case "chalk white":
                colorName = "쵸크화이트";
                break;
            case "cholk white":
                colorName = "쵸크화이트";
                break;
            case "lake silver":
                colorName = "레이크실버";
                break;
            case "velvet dune":
                colorName = "벨벳듄";
                break;                
            case "dark night":
                colorName = "다크나이트";
                break;
            case "phantom black":
                colorName = "팬텀블랙";
                break;
            case "blue lagoon":
                colorName = "블루라군";
                break;
            case "ceramic blue":
                colorName = "세라믹블루";
                break;
            case "tangerine comet":
                colorName = "탠저린코멧";
                break;
            case "pulse red":
                colorName = "펄스레드";
                break;
            case "acid yellow":
                colorName = "애시드엘로우";
                break;
            case "acid yellows":
                colorName = "애시드엘로우";
                break;

            default:
                colorName = "";
                break;
        }

        return colorName;
    }
}
