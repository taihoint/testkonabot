using System;
using System.Net;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Web;
using Bot_Application1.Models;

public class EngTransferKor
{
    public static string GetEngTransferKor(string entitiesStr)
    {

        String korName = "";

        switch (entitiesStr)
        {
            case "sungnae":
                korName = "성내";
                break;
            case "jamsil":
                korName = "잠실";
                break;
            case "gongreung":
                korName = "공릉";
                break;                
            case "mok - dong":
                korName = "목동";
                break;
            case "mogdong":
                korName = "목동";
                break;
            case "wonhyoro":
                korName = "원효로";
                break;
            case "daebang":
                korName = "대방";
                break;
            case "gangnam":
                korName = "강남";
                break;
            case "busan central":
                korName = "부산중앙";
                break;
            case "busan dongbu":
                korName = "부산동부";
                break;
            case "incheon west":
                korName = "인천서부";
                break;
            case "incheon western":
                korName = "인천서부";
                break;                
            case "incheon east":
                korName = "인천동부";
                break;
            case "incheon dongbu":
                korName = "인천동부";
                break;
            case "daegu east":
                korName = "대구동부";
                break;
            case "daegu eastern":
                korName = "대구동부";
                break;                
            case "daegu west":
                korName = "대구서부";
                break;
            case "ulsan":
                korName = "울산";
                break;
            case "daejeon":
                korName = "대전";
                break;
            case "gwangju":
                korName = "광주";
                break;
            case "gangwon":
                korName = "강원";
                break;
            case "wonju":
                korName = "원주";
                break;
            case "gyeonggi":
                korName = "경기";
                break;                
            case "ansan":
                korName = "안산";
                break;
            case "anshan":
                korName = "안산";
                break;                
            case "suzie":
                korName = "수지";
                break;
            case "anyang":
                korName = "안양";
                break;
            case "dongtan":
                korName = "동탄";
                break;
            case "ilsan":
                korName = "일산";
                break;
            case "bundang":
                korName = "분당";
                break;
            case "uijeongbu":
                korName = "의정부";
                break;
            case "pohang":
                korName = "포항";
                break;
            case "changwon":
                korName = "창원";
                break;
            case "cheongju":
                korName = "청주";
                break;
            case "cheonan":
                korName = "천안아산";
                break;
            case "jeonju":
                korName = "전주";
                break;
            case "seoul":
                korName = "서울";
                break;
            case "busan":
                korName = "부산";
                break;
            case "incheon":
                korName = "인천";
                break;
            case "daegu":
                korName = "대구";
                break;
            case "gyeongbuk":
                korName = "경북";
                break;
            case "gyeongnam":
                korName = "경남";
                break;
            case "chungbuk":
                korName = "충북";
                break;
            case "chungnam":
                korName = "충남";
                break;
            case "jeonbuk":
                korName = "전북";
                break;


            default:
                korName = "";
                break;
        }

        return korName;
    }
}
