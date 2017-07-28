﻿using System;
using System.Net;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Web;
using Bot_Application1.Models;

public class SorryMessageList
{
    public static string GetSorryMessage(int cnt)
    {

        int sorryCnt = 0;
        Debug.WriteLine("cnt : " + cnt);
        String sorryMsg = "";

        if (cnt > 1)
        {
            sorryCnt = 1;
        }
        else
        {
            sorryCnt = 0;
        }

        switch (sorryCnt)
        {
         
            case  0:
                sorryMsg = "쉽게 다시 한번 말씀해 주시면 안 될까요? 예를들어 코나의 특징이 알고 싶으시면 ''특징'' 이란 단어 입력만으로도 제가 이해할 수 있어요";
                break;

            case 1:
                sorryMsg = "죄송해요, 무슨 말인지 이해하지 못했어요";
                break;

        }

        return sorryMsg;
    }
}
