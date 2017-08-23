using System;
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
                sorryMsg = "죄송해요, 준비된 답변이 없어요ㅜㅜ그런데… \n\n좋은 질문이에요!제게 답변할 기회를 주실래요? \n\n지금 현대자동차 페이스북에서 답변 공약 이벤트 중이에요, \n\n방금과 같은 질문을 별명과 함께 써주시고 \n\n페이스북 챗봇 런칭 이벤트에도 같은 질문과 별명을 함께 남기시면 \n\n추첨하여 선물도 드리고 9월 25일까지 답변할 것을 약속 드릴게요";
                break;

        }

        return sorryMsg;
    }
}
