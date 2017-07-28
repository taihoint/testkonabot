using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

using System.Web.Http;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Bot_Application1.DB;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Bot_Application1.Models;
using Newtonsoft.Json.Linq;

namespace Bot_Application1
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        /// 
        public static int sorryMessageCnt = 0;
        public static string newUserID = "";
        public static string beforeUserID = "";

        public virtual async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            newUserID = activity.Conversation.Id;

            HttpResponseMessage response;
            if (beforeUserID != newUserID)
            {
                beforeUserID = newUserID;
                sorryMessageCnt = 0;
            }
            
            // welcome message 출력   
            if (activity.Type == ActivityTypes.ConversationUpdate && activity.MembersAdded.Any(m => m.Id == activity.Recipient.Id))
            {

                //WeatherInfo weatherInfo = await GetWeatherInfo();

                //Debug.WriteLine("weatherInfo :  " + weatherInfo.list[0].weather[0].description);
                //Debug.WriteLine("weatherInfo : " + string.Format("{0}°С", Math.Round(weatherInfo.list[0].temp.min, 1)));
                //Debug.WriteLine("weatherInfo : " + string.Format("{0}°С", Math.Round(weatherInfo.list[0].temp.max, 1)));

                DateTime startTime = DateTime.Now;

                // Db
                DbConnect db = new DbConnect();
                List<DialogList> dlg = db.SelectInitDialog();
                Debug.WriteLine("!!!!!!!!!!! : " + dlg[0].dlgId);

                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                for (int n = 0; n < dlg.Count; n++)
                {
                    List<CardList> card = db.SelectDialogCard(dlg[n].dlgId);

                    Activity reply2 = activity.CreateReply();
                    reply2.Recipient = activity.From;
                    reply2.Type = "message";
                    reply2.Attachments = new List<Attachment>();
                    reply2.AttachmentLayout = AttachmentLayoutTypes.Carousel;


                    for (int i = 0; i < card.Count; i++)
                    {
                        VideoCard[] plVideoCard = new VideoCard[card.Count];
                        HeroCard[] plHeroCard = new HeroCard[card.Count];
                        ReceiptCard[] plReceiptCard = new ReceiptCard[card.Count];

                        Attachment[] plAttachment = new Attachment[card.Count];

                        List<ButtonList> btn = db.SelectBtn(card[i].dlgId, card[i].cardId);
                        List<ImagesList> img = db.SelectImage(card[i].dlgId, card[i].cardId);
                        List<MediaList> media = db.SelectMedia(card[i].dlgId, card[i].cardId);

                        List<CardAction> cardButtons = new List<CardAction>();
                        CardAction[] plButton = new CardAction[btn.Count];

                        List<CardImage> cardImages = new List<CardImage>();
                        CardImage[] plImage = new CardImage[img.Count];

                        ThumbnailUrl plThumnail = new ThumbnailUrl();

                        CardAction tap = new CardAction();

                        List<MediaUrl> mediaURL = new List<MediaUrl>();
                        MediaUrl[] plMediaUrl = new MediaUrl[media.Count];

                        for (int l = 0; l < img.Count; l++)
                        {
                            if (card[i].cardType == "herocard")
                            {
                                if (img[l].imgUrl != null)
                                {
                                    plImage[l] = new CardImage()
                                    {
                                        Url = img[l].imgUrl
                                    };
                                }
                            }
                            else if (card[i].cardType == "videocard")
                            {
                                if (img[l].imgUrl != null)
                                {
                                    plThumnail.Url = img[l].imgUrl;
                                }
                            }
                        }

                        cardImages = new List<CardImage>(plImage);

                        for (int l = 0; l < media.Count; l++)
                        {
                            if (media[l].mediaUrl != null)
                            {
                                plMediaUrl[l] = new MediaUrl()
                                {
                                    Url = media[l].mediaUrl
                                };
                            }
                        }
                        mediaURL = new List<MediaUrl>(plMediaUrl);

                        for (int m = 0; m < btn.Count; m++)
                        {
                            if (btn[m].btnTitle != null)
                            {
                                plButton[m] = new CardAction()
                                {
                                    Value = btn[m].btnContext,
                                    Type = btn[m].btnType,
                                    Title = btn[m].btnTitle
                                };
                            }
                        }
                        cardButtons = new List<CardAction>(plButton);

                        if (card[i].cardType == "herocard")
                        {
                            plHeroCard[i] = new HeroCard()
                            {
                                Title = card[i].cardTitle,
                                Text = card[i].cardText,
                                Subtitle = card[i].cardSubTitle,
                                Images = cardImages,
                                Buttons = cardButtons
                            };

                            plAttachment[i] = plHeroCard[i].ToAttachment();
                            reply2.Attachments.Add(plAttachment[i]);

                        }
                        else if (card[i].cardType == "videocard")
                        {

                            plVideoCard[i] = new VideoCard()
                            {
                                Title = card[i].cardTitle,
                                Text = card[i].cardText,
                                Subtitle = card[i].cardSubTitle,
                                Media = mediaURL,
                                Image = plThumnail,
                                Buttons = cardButtons,
                                Autostart = false
                            };

                            plAttachment[i] = plVideoCard[i].ToAttachment();
                            reply2.Attachments.Add(plAttachment[i]);

                        }
                    }
                    var reply1 = await connector.Conversations.SendToConversationAsync(reply2);
                }
                DateTime endTime = DateTime.Now;
                Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - startTime).Milliseconds));

            }
            else if (activity.Type == ActivityTypes.Message)
            {
                
                // Db
                DbConnect db = new DbConnect();
                int inserResult;
                string orgMent = "";
                DateTime startTime = DateTime.Now;
                long unixTime = ((DateTimeOffset)startTime).ToUnixTimeSeconds();
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                orgMent = activity.Text;
                string bannedAnswer = "";
                string bannedAnswerOrigin = "";
                string bannedAnswerFirst = "";
                string bannedAnswerSecond = "";
                string branchStr = "";

                bannedAnswer = db.SelectBannedWordAnswerMsg(orgMent);
                Debug.WriteLine("[ "+orgMent + " ] BANNED ANSWER : " + Regex.Split(bannedAnswer,"@@")[0] );
                bannedAnswerOrigin = bannedAnswer;
                bannedAnswer = Regex.Split(bannedAnswer, "@@")[0];
                if(bannedAnswerOrigin != "")
                {
                    bannedAnswerFirst = Regex.Split(bannedAnswerOrigin, "@@")[1];
                    bannedAnswerSecond = Regex.Split(bannedAnswerOrigin, "@@")[2];
                    branchStr = orgMent.Replace(bannedAnswerFirst, "");
                }

                Translator translateInfo = await getTranslate(orgMent);
                Translator translateInfoBranch = await getTranslate(branchStr);
                if (bannedAnswer != "")
                {
                    if (bannedAnswerSecond.Equals("3"))
                    {
                        Activity reply_brach = activity.CreateReply();
                        reply_brach.Recipient = activity.From;
                        reply_brach.Type = "message";
                        reply_brach.Attachments = new List<Attachment>();
                        reply_brach.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                        JObject Luis_brach = await GetIntentFromTestDriveLUIS(translateInfoBranch.data.translations[0].translatedText.Replace("&#39;", "'"));

                        string luis_brach_intent = (string)Luis_brach["intents"][0]["intent"];
                        Debug.WriteLine("luis_brach_intent = " + luis_brach_intent);
                        Debug.WriteLine("!!!!!!!!!!!!!!!! = " + translateInfoBranch.data.translations[0].translatedText.Replace("&#39;", "'"));

                        List<CarBranchInfo> CarBranchInfo = db.SelectCarBranchDialog(bannedAnswerFirst, luis_brach_intent);
                        /*
                        for (int td = 0; td < CarBranchInfo.Count; td++)
                        {
                            reply_brach.Attachments.Add(
                            GetHeroCard_location(
                            CarBranchInfo[td].dlgBrNM,
                            CarBranchInfo[td].dlgDspBrTN,
                            CarBranchInfo[td].dlgBrAddr,
                            new CardAction())
                            );
                        }
                        */

                        for (int td = 0; td < CarBranchInfo.Count; td++)
                        {
                            var urlImg = "https://openapi.naver.com/v1/map/staticmap.bin?clientId=OPCP0Yh0b2IC9r59XaTR&url=http://www.hyundai.com&crs=EPSG:4326&center=" + CarBranchInfo[td].dlgBrXcoo + "," + CarBranchInfo[td].dlgBrYcoo + "&level=12&w=400&h=300&baselayer=default&markers=" + CarBranchInfo[td].dlgBrXcoo + "," + CarBranchInfo[td].dlgBrYcoo;
                            String fileName = "c:/inetpub/wwwroot/map/" + CarBranchInfo[td].dlgBrXcoo + "," + CarBranchInfo[td].dlgBrYcoo + ".png";

                            System.Net.WebClient client = new System.Net.WebClient();
                            client.DownloadFile(urlImg, fileName);

                            reply_brach.Attachments.Add(
                            UserGetHeroCard_location(
                            CarBranchInfo[td].dlgBrNM,
                            "TEL." + CarBranchInfo[td].dlgDspBrTN,
                            CarBranchInfo[td].dlgBrAddr,
                            new CardImage(url: "http://www.smartsend.co.kr/map/" + CarBranchInfo[td].dlgBrXcoo + "," + CarBranchInfo[td].dlgBrYcoo + ".png"),
                            CarBranchInfo[td].dlgBrXcoo,
                            CarBranchInfo[td].dlgBrYcoo)
                            );
                        }

                        var reply1 = await connector.Conversations.SendToConversationAsync(reply_brach);
                    } else
                    {
                        Activity reply_err = activity.CreateReply();
                        reply_err.Recipient = activity.From;
                        reply_err.Type = "message";
                        reply_err.Text = bannedAnswer;
                        var reply1 = await connector.Conversations.SendToConversationAsync(reply_err);
                    }

                    response = Request.CreateResponse(HttpStatusCode.OK);
                    return response;


                }
                else if (orgMent.Contains("가솔린 2WD 트림 가격") || orgMent.Contains("가솔린 4WD 트림 가격") || orgMent.Contains("디젤 2WD 트림 가격") || orgMent.Contains("TUIX 가솔린 트림 가격") || orgMent.Contains("TUIX 디젤 트림 가격"))
                {
                    Debug.WriteLine("트림 선택 하자");

                    orgMent = orgMent.Replace(" 트림 가격", "");

                    Debug.WriteLine("orgMent : " + orgMent);

                    Activity reply_ment = activity.CreateReply();
                    reply_ment.Recipient = activity.From;
                    reply_ment.Type = "message";
                    reply_ment.Text = "코나 " + orgMent + "를 보여드릴께요.";
                    var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);


                    List<CarTrimList> CarTrimList = db.SelectCarTrimList(orgMent);
                    //Debug.WriteLine("CarTrimList.Count : " + CarTrimList.Count);
                    Activity reply_trim = activity.CreateReply();
                    reply_trim.Recipient = activity.From;
                    reply_trim.Type = "message";
                    reply_trim.Attachments = new List<Attachment>();
                    reply_trim.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    for (int td = 0; td < CarTrimList.Count; td++)
                    {
                        string color = (string)(CarTrimList[td].tuix + CarTrimList[td].cartrim);
                        //Debug.WriteLine(color + " : color : " + color.Replace(" ", ""));
                        string trimNM = CarTrimList[td].carTrimNm;
                        trimNM = trimNM.Replace("가솔린 ", "");
                        trimNM = trimNM.Replace("디젤 ", "");
                        trimNM = trimNM.Replace("2WD ", "");
                        trimNM = trimNM.Replace("4WD ", "");

                        reply_trim.Attachments.Add(
                        GetHeroCard_show(
                        trimNM,
                        string.Format("{0}", CarTrimList[td].salePrice.ToString("n0")) + "원",
                        "",
                        //new CardImage(url: "D:\\bob\\01.project\\05.현대자동차 ChatBot\\01.자료\\20170710\\가격 이미지\\가격 이미지\\trim\\" + color.Replace(" ", "") + ".jpg"),
                        new CardImage(url: "https://bottest.hyundai.com/assets/images/price/trim/" + color.Replace(" ", "") + ".jpg"),
                        new CardAction(ActionTypes.ImBack, "트림", value: CarTrimList[td].carTrimNm + " 트림 보여줄래"))
                        );
                    }
                    var reply1 = await connector.Conversations.SendToConversationAsync(reply_trim);

                    DateTime endTime = DateTime.Now;
                    Debug.WriteLine("USER NUMBER : " + activity.Conversation.Id);
                    Debug.WriteLine("CUSTOMMER COMMENT KOREAN : " + activity.Text);
                    Debug.WriteLine("CUSTOMMER COMMENT ENGLISH : " + "");

                    Debug.WriteLine("CHATBOT_COMMENT_CODE : " + "dlg.noluis.price.trim.Carousel");
                    Debug.WriteLine("CHANNEL_ID : " + activity.ChannelId);
                    Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - startTime).Milliseconds));

                    inserResult = db.insertHistory(activity.Conversation.Id, activity.Text, translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), "dlg.noluis.price.trim.Carousel", activity.ChannelId, ((endTime - startTime).Milliseconds));
                    if (inserResult > 0)
                    {
                        Debug.WriteLine("HISTORY RESULT SUCCESS");
                    }
                    else
                    {
                        Debug.WriteLine("HISTORY RESULT FAIL");
                    }

                    response = Request.CreateResponse(HttpStatusCode.OK);
                    return response;
                }
                else if (orgMent.Contains(" 트림 보여줄래"))
                {
                    string color = "";

                    Debug.WriteLine("트림 보여주자");

                    orgMent = orgMent.Replace(" 트림 보여줄래", "");

                    Debug.WriteLine("orgMent : " + orgMent);

                    string trimNM = orgMent;
                    trimNM = trimNM.Replace("가솔린 ", "");
                    trimNM = trimNM.Replace("디젤 ", "");
                    trimNM = trimNM.Replace("2WD ", "");
                    trimNM = trimNM.Replace("4WD ", "");


                    Activity reply_ment = activity.CreateReply();
                    reply_ment.Recipient = activity.From;
                    reply_ment.Type = "message";
                    reply_ment.Text = "코나 " + orgMent + "를 선택하셨네요. \n\n외장색상, 내장색상, 옵션를 보실수 있습니다.\n\n 견적을 내시기 위해서는 견적 사이트로 이동하시면 됩니다.";
                    var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                    Activity reply_trim_only = activity.CreateReply();
                    reply_trim_only.Recipient = activity.From;
                    reply_trim_only.Type = "message";
                    reply_trim_only.Attachments = new List<Attachment>();
                    reply_trim_only.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    color = db.SelectOnlyTrimColor(orgMent);

                    reply_trim_only.Attachments.Add(
                    GetHeroCard_trim(
                        trimNM,
                        //orgMent,
                        "",
                        "",
                        //new CardImage(url: "D:\\bob\\01.project\\05.현대자동차 ChatBot\\01.자료\\20170710\\가격 이미지\\가격 이미지\\trim\\" + color.Replace(" ", "") + ".jpg"),
                        new CardImage(url: "https://bottest.hyundai.com/assets/images/price/trim/"+ color.Replace(" ", "") + ".jpg"),
                        new CardAction(ActionTypes.ImBack, "외장색상", value: orgMent + " 외장색상 보여줄래"),
                        new CardAction(ActionTypes.ImBack, "내장색상", value: orgMent + " 내장색상 보여줄래"),
                        new CardAction(ActionTypes.ImBack, "옵션보기", value: orgMent + " 옵션 보여줄래"),
                        new CardAction(ActionTypes.OpenUrl, "견적사이트 이동", value: "https://logon.hyundai.com/kr/quotation/main.do?car code=RV104"))
                    );

                    var reply1 = await connector.Conversations.SendToConversationAsync(reply_trim_only);

                    DateTime endTime = DateTime.Now;
                    Debug.WriteLine("USER NUMBER : " + activity.Conversation.Id);
                    Debug.WriteLine("CUSTOMMER COMMENT KOREAN : " + activity.Text);
                    Debug.WriteLine("CUSTOMMER COMMENT ENGLISH : " + "");

                    Debug.WriteLine("CHATBOT_COMMENT_CODE : " + "dlg.noluis.price.onlyTrim");
                    Debug.WriteLine("CHANNEL_ID : " + activity.ChannelId);
                    Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - startTime).Milliseconds));

                    inserResult = db.insertHistory(activity.Conversation.Id, activity.Text, translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), "dlg.noluis.price.onlyTrim", activity.ChannelId, ((endTime - startTime).Milliseconds));
                    if (inserResult > 0)
                    {
                        Debug.WriteLine("HISTORY RESULT SUCCESS");
                    }
                    else
                    {
                        Debug.WriteLine("HISTORY RESULT FAIL");
                    }

                    response = Request.CreateResponse(HttpStatusCode.OK);
                    return response;
                }
                else if (orgMent.Contains("외장색상 보여줄래"))
                {
                    Debug.WriteLine("외장컬러 보여주자");

                    orgMent = orgMent.Replace(" 외장색상 보여줄래", "");

                    Debug.WriteLine("orgMent : " + orgMent);

                    List<CarExColorList> CarExColorList = db.SelectCarExColorList(orgMent);
                    //Debug.WriteLine("CarExColorList.Count : " + CarExColorList.Count);

                    Activity reply_ment = activity.CreateReply();
                    reply_ment.Recipient = activity.From;
                    reply_ment.Type = "message";
                    reply_ment.Text = "코나 " + CarExColorList[0].model + "의 외장색상을 보여드릴께요.";
                    var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);


                    Activity reply_exColor = activity.CreateReply();
                    reply_exColor.Recipient = activity.From;
                    reply_exColor.Type = "message";
                    reply_exColor.Attachments = new List<Attachment>();
                    reply_exColor.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    for (int td = 0; td < CarExColorList.Count; td++)
                    {

                        string trimNM = CarExColorList[td].trimColorNm;
                        trimNM = trimNM.Replace("가솔린 ", "");
                        trimNM = trimNM.Replace("디젤 ", "");
                        trimNM = trimNM.Replace("2WD ", "");
                        trimNM = trimNM.Replace("4WD ", "");

                        reply_exColor.Attachments.Add(
                        GetHeroCard_tap1(
                            trimNM,
                            //CarExColorList[td].trimColorNm,
                            "추가 금액 : " + string.Format("{0}", CarExColorList[td].exColorPrice.ToString("n0")) + "원",
                            "",
                            //new CardImage(url: "D:\\bob\\01.project\\05.현대자동차 ChatBot\\01.자료\\20170710\\가격 이미지\\가격 이미지\\exterior\\" + CarExColorList[td].trimColorCd + ".jpg"),
                            new CardImage(url: "https://bottest.hyundai.com/assets/images/price/exterior/"+ CarExColorList[td].trimColorCd + ".jpg"),
                            new CardAction(ActionTypes.OpenUrl, "견적사이트 이동", value: "https://bottest.hyundai.com/assets/images/price/exterior/" + CarExColorList[td].trimColorCd + ".jpg"))
                        );
                    }

                    var reply1 = await connector.Conversations.SendToConversationAsync(reply_exColor);

                    DateTime endTime = DateTime.Now;
                    Debug.WriteLine("USER NUMBER : " + activity.Conversation.Id);
                    Debug.WriteLine("CUSTOMMER COMMENT KOREAN : " + activity.Text);
                    Debug.WriteLine("CUSTOMMER COMMENT ENGLISH : " + "");

                    Debug.WriteLine("CHATBOT_COMMENT_CODE : " + "dlg.noluis.price.exteriorColor");
                    Debug.WriteLine("CHANNEL_ID : " + activity.ChannelId);
                    Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - startTime).Milliseconds));

                    inserResult = db.insertHistory(activity.Conversation.Id, activity.Text, translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), "dlg.noluis.price.exteriorColor", activity.ChannelId, ((endTime - startTime).Milliseconds));
                    if (inserResult > 0)
                    {
                        Debug.WriteLine("HISTORY RESULT SUCCESS");
                    }
                    else
                    {
                        Debug.WriteLine("HISTORY RESULT FAIL");
                    }

                    response = Request.CreateResponse(HttpStatusCode.OK);
                    return response;
                }
                else if (orgMent.Contains("내장색상 보여줄래"))
                {
                    Debug.WriteLine("내장색상 보여주자");

                    orgMent = orgMent.Replace(" 내장색상 보여줄래", "");

                    Debug.WriteLine("orgMent : " + orgMent);

                    List<CarInColorList> CarInColorList = db.SelectCarInColorList(orgMent);

                    Activity reply_ment = activity.CreateReply();
                    reply_ment.Recipient = activity.From;
                    reply_ment.Type = "message";
                    reply_ment.Text = "코나 " + CarInColorList[0].model + "의 내장색상을 보여드릴께요.";
                    var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                    Activity reply_inColor = activity.CreateReply();
                    reply_inColor.Recipient = activity.From;
                    reply_inColor.Type = "message";
                    reply_inColor.Attachments = new List<Attachment>();
                    reply_inColor.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    for (int td = 0; td < CarInColorList.Count; td++)
                    {
                        string trimNM = CarInColorList[td].internalColorNm;
                        trimNM = trimNM.Replace("가솔린 ", "");
                        trimNM = trimNM.Replace("디젤 ", "");
                        trimNM = trimNM.Replace("2WD ", "");
                        trimNM = trimNM.Replace("4WD ", "");

                        reply_inColor.Attachments.Add(
                        GetHeroCard_tap1(
                            trimNM,
                            //CarInColorList[td].internalColorNm,
                            "추가 금액 : " + string.Format("{0}", CarInColorList[td].inColorPrice.ToString("n0")) + "원",
                            "",
                            //new CardImage(url: "D:\\bob\\01.project\\05.현대자동차 ChatBot\\01.자료\\20170710\\가격 이미지\\가격 이미지\\interior\\" + CarInColorList[td].internalColorCd + ".jpg"),
                            new CardImage(url: "https://bottest.hyundai.com/assets/images/price/interior/" + CarInColorList[td].internalColorCd + ".jpg"),
                            new CardAction(ActionTypes.OpenUrl, "견적사이트 이동", value: "https://bottest.hyundai.com/assets/images/price/interior/" + CarInColorList[td].internalColorCd + ".jpg"))
                        );
                    }

                    var reply1 = await connector.Conversations.SendToConversationAsync(reply_inColor);

                    DateTime endTime = DateTime.Now;
                    Debug.WriteLine("USER NUMBER : " + activity.Conversation.Id);
                    Debug.WriteLine("CUSTOMMER COMMENT KOREAN : " + activity.Text);
                    Debug.WriteLine("CUSTOMMER COMMENT ENGLISH : " + "");

                    Debug.WriteLine("CHATBOT_COMMENT_CODE : " + "dlg.noluis.price.interiorColor");
                    Debug.WriteLine("CHANNEL_ID : " + activity.ChannelId);
                    Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - startTime).Milliseconds));

                    inserResult = db.insertHistory(activity.Conversation.Id, activity.Text, translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), "dlg.noluis.price.interiorColor", activity.ChannelId, ((endTime - startTime).Milliseconds));
                    if (inserResult > 0)
                    {
                        Debug.WriteLine("HISTORY RESULT SUCCESS");
                    }
                    else
                    {
                        Debug.WriteLine("HISTORY RESULT FAIL");
                    }

                    response = Request.CreateResponse(HttpStatusCode.OK);
                    return response;
                }
                else if (orgMent.Contains("옵션 보여줄래"))
                {
                    Debug.WriteLine("옵션 보여주자");

                    orgMent = orgMent.Replace(" 옵션 보여줄래", "");

                    Debug.WriteLine("orgMent : " + orgMent);

                    List<CarOptionList> CarOptionList = db.SelectCarOptionList(orgMent);

                    Activity reply_ment = activity.CreateReply();
                    reply_ment.Recipient = activity.From;
                    reply_ment.Type = "message";
                    reply_ment.Text = "코나 " + CarOptionList[0].model + "의 추가옵션을 보여드릴께요.";
                    var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                    Activity reply_option = activity.CreateReply();
                    reply_option.Recipient = activity.From;
                    reply_option.Type = "message";
                    reply_option.Attachments = new List<Attachment>();
                    reply_option.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    for (int td = 0; td < CarOptionList.Count; td++)
                    {

                        string trimNM = CarOptionList[td].optNm;
                        trimNM = trimNM.Replace("가솔린 ", "");
                        trimNM = trimNM.Replace("디젤 ", "");
                        trimNM = trimNM.Replace("2WD ", "");
                        trimNM = trimNM.Replace("4WD ", "");

                        translateInfo = await getTranslate(CarOptionList[td].optNm);
                        reply_option.Attachments.Add(
                        GetHeroCard_tap1(
                            trimNM,
                            //CarOptionList[td].optNm,
                            "추가 금액 : " + string.Format("{0}", CarOptionList[td].optPrice.ToString("n0")) + "원",
                            "",
                            //new CardImage(url: "D:\\bob\\01.project\\05.현대자동차 ChatBot\\01.자료\\20170710\\가격 이미지\\가격 이미지\\option\\" + translateInfo.data.translations[0].translatedText + ".jpg"),
                            new CardImage(url: "https://bottest.hyundai.com/assets/images/price/option/" + translateInfo.data.translations[0].translatedText + ".jpg"),
                            new CardAction(ActionTypes.OpenUrl, "견적사이트 이동", value: "https://bottest.hyundai.com/assets/images/price/option/" + translateInfo.data.translations[0].translatedText + ".jpg"))
                        );
                    }

                    var reply1 = await connector.Conversations.SendToConversationAsync(reply_option);

                    DateTime endTime = DateTime.Now;
                    Debug.WriteLine("USER NUMBER : " + activity.Conversation.Id);
                    Debug.WriteLine("CUSTOMMER COMMENT KOREAN : " + activity.Text);
                    Debug.WriteLine("CUSTOMMER COMMENT ENGLISH : " + "");

                    Debug.WriteLine("CHATBOT_COMMENT_CODE : " + "dlg.noluis.price.option");
                    Debug.WriteLine("CHANNEL_ID : " + activity.ChannelId);
                    Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - startTime).Milliseconds));

                    inserResult = db.insertHistory(activity.Conversation.Id, activity.Text, translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), "dlg.noluis.price.option", activity.ChannelId, ((endTime - startTime).Milliseconds));
                    if (inserResult > 0)
                    {
                        Debug.WriteLine("HISTORY RESULT SUCCESS");
                    }
                    else
                    {
                        Debug.WriteLine("HISTORY RESULT FAIL");
                    }

                    response = Request.CreateResponse(HttpStatusCode.OK);
                    return response;
                }
                else
                {
                    for (int n = 0; n < Regex.Split(orgMent, " ").Length; n++)
                    {
                        string chgMsg = db.SelectChgMsg(Regex.Split(orgMent, " ")[n]);
                        if (!string.IsNullOrEmpty(chgMsg))
                        {
                            orgMent = orgMent.Replace(Regex.Split(orgMent, " ")[n], chgMsg);
                        }
                    }

                    orgMent = orgMent.Replace("&#39;", "/'");
                    Debug.WriteLine("orgMent : " + orgMent);
                    translateInfo = await getTranslate(orgMent);
                    Debug.WriteLine("!!!!!!!!!!!!!! : " + translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // For Query Analysis
                    int luisID = 0;
                    // Try to find dialogue from log history first before checking LUIS
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    JObject Luis = db.SelectQueryAnalysis(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));
                    string entitiesStr = (string)Luis["entities"];
                    string areaStr = (string)Luis["car_area"];
                    string priceWhereStr = (string)Luis["car_priceWhere"];
                    string colorAreaStr = (string)Luis["car_colorArea"];
                    string carOptionStr = (string)Luis["car_option"];
                    string luis_intent = "";
                    //string priceWhereStr = "";

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // For Query Analysis
                    // No results from DB
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //if (string.IsNullOrEmpty(entitiesStr) && string.IsNullOrEmpty((string)Luis["intents"][0]["intent"]) || ((string)Luis["intents"][0]["intent"]).Equals("car quote") || ((string)Luis["intents"][0]["intent"]).Equals("car option") || ((string)Luis["intents"][0]["intent"]).Equals("show color"))
                    if (string.IsNullOrEmpty(entitiesStr) && string.IsNullOrEmpty((string)Luis["intents"][0]["intent"]))
                    {
                        Luis = await GetIntentFromOpenStartLUIS(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));
                        luisID = 1; //Query Analysis
                                    //Debug.WriteLine("Luis.entities.Length : " + Luis.entities.Length);

                        //luis 분기
                        try
                        {
                            Debug.WriteLine("score : " + (float)Luis["intents"][0]["score"]);
                            Debug.WriteLine("score : " + Luis["entities"].Count());

                            if (Luis["entities"].Count() == 0 || (float)Luis["intents"][0]["score"] < 0.5)
                            {
                                Debug.WriteLine("TestDrive LUIS");
                                Luis = await GetIntentFromTestDriveLUIS(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));
                                //if (Luis.intents[0].intent == "None" || Luis.intents[0].score < 0.2)
                                if (Luis["entities"].Count() == 0 || (float)Luis["intents"][0]["score"] < 0.5)
                                {
                                    Debug.WriteLine("Close_1 LUIS");
                                    Luis = await GetIntentFromClose1LUIS(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));

                                    if (Luis["entities"].Count() == 0 || (float)Luis["intents"][0]["score"] < 0.5)
                                    {
                                        Debug.WriteLine("Close_2 LUIS");
                                        Luis = await GetIntentFromClose2LUIS(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));

                                        if (Luis["entities"].Count() == 0 || (float)Luis["intents"][0]["score"] < 0.5)
                                        {
                                            Debug.WriteLine("Personal LUIS");
                                            Luis = await GetIntentFromPersonalLUIS(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));

                                            if (Luis["entities"].Count() == 0)
                                            {
                                                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                                // Query Analysis
                                                // No LUIS result at all, so NLP failed completely
                                                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                                //sorryMessageCnt++;
                                                Debug.WriteLine("sorryMessageCnt1 : " + sorryMessageCnt);
                                                int dbResult = db.insertUserQuery(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), "", "", 0, 'N',"","","","");
                                                Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());
                                                Activity reply_err = activity.CreateReply();
                                                reply_err.Recipient = activity.From;
                                                reply_err.Type = "message";
                                                //reply_err.Text = "죄송해요1. 무슨 말인지 잘 모르겠어요.";
                                                reply_err.Text = SorryMessageList.GetSorryMessage(++sorryMessageCnt);
                                                var reply1 = await connector.Conversations.SendToConversationAsync(reply_err);

                                                response = Request.CreateResponse(HttpStatusCode.OK);
                                                return response;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            // Query Analysis
                            // No LUIS result at all, so NLP failed completely
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            //sorryMessageCnt++;
                            Debug.WriteLine("sorryMessageCnt2 : " + sorryMessageCnt);
                            int dbResult = db.insertUserQuery(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), "", "", 0, 'N',"","", "","");
                            Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());
                            Activity reply_err = activity.CreateReply();
                            reply_err.Recipient = activity.From;
                            reply_err.Type = "message";
                            //reply_err.Text = "죄송해요. 무슨 말인지 잘 모르겠어요..";
                            reply_err.Text = SorryMessageList.GetSorryMessage(++sorryMessageCnt);
                            var reply1 = await connector.Conversations.SendToConversationAsync(reply_err);

                        }

                        
                        Debug.WriteLine(" QUERY : " + Luis["query"]);
                        Debug.WriteLine(" INTENT : " + Luis["intents"][0]["intent"]);

                        luis_intent = (string)Luis["intents"][0]["intent"];

                        for (int i = 0; i < Luis["entities"].Count(); i++)
                        {

                            Debug.WriteLine("Luis[entities] " + Luis["entities"][i]["resolution"]["values"][0]);

                            if (!((string)Luis["entities"][i]["type"]).Contains("::"))
                            {
                                Debug.WriteLine(" LIST ENTITIES " + Luis["entities"][i]["resolution"]["values"][0]);

                                //if ((Luis["intents"][0]["intent"].ToString().Equals("show color") && Luis["entities"][i]["entity"].ToString().Equals("color")))
                                //{
                                //    entitiesStr += Luis["entities"][i]["entity"].ToString() + ",";
                                //}
                                //else 
                                if (Luis["intents"][0]["intent"].ToString().Equals("car quote") || Luis["intents"][0]["intent"].ToString().Equals("car option") || Luis["intents"][0]["intent"].ToString().Equals("show color"))
                                //else if (Luis["intents"][0]["intent"].ToString().Equals("car quote"))
                                {
                                    if (!(Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("price")) || !Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("option"))
                                    {
                                        
                                        priceWhereStr += Luis["entities"][i]["resolution"]["values"][0].ToString() + "=" + Luis["entities"][i]["entity"].ToString() + ",";
                                        
                                    }
                                    //else if (Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("cartrim") || Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("fuel") || Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("drivewheel") || Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("tuix") || Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("colorpackage"))
                                    //{

                                    //    if (Luis["entities"][i]["entity"].ToString() == "premium s")
                                    //    {
                                    //        priceWhereStr += " AND C." + Luis["entities"][i]["resolution"]["values"][0].ToString() + " = 'premium special'";
                                    //    }
                                    //    else
                                    //    {
                                    //        priceWhereStr += " AND C." + Luis["entities"][i]["resolution"]["values"][0].ToString() + " = '" + Luis["entities"][i]["entity"].ToString();
                                    //    }

                                    //}
                                    else if (Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("option"))
                                    {

                                        if (Luis["entities"][i]["entity"].ToString().Equals("option") || Luis["entities"][i]["entity"].ToString().Equals("options"))
                                        {
                                            carOptionStr = "option";
                                        }
                                        else
                                        {
                                            if (Luis["entities"][i]["entity"].ToString().Length > carOptionStr.Length)
                                            {
                                                carOptionStr = Luis["entities"][i]["entity"].ToString();
                                            }
                                        }
                                    }
                                    Debug.WriteLine("priceWhereStr : " + priceWhereStr);
                                    Debug.WriteLine("carOptionStr : " + carOptionStr);

                                    //if ((!Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("car color")))
                                    //{
                                    //    if (Luis["entities"][i]["entity"].ToString() == "premium s")
                                    //    {
                                    //        entitiesStr += "premium special" + ",";
                                    //    }
                                    //    else
                                    //    {
                                    //        entitiesStr += Luis["entities"][i]["entity"].ToString() + ",";
                                    //    }
                                    //}
                                    entitiesStr += Luis["entities"][i]["entity"].ToString() + ",";
                                }





                                /*
                                 else if(Luis["intents"][0]["intent"].ToString().Equals("car quote") || Luis["intents"][0]["intent"].ToString().Equals("car option") || Luis["intents"][0]["intent"].ToString().Equals("show color"))
                                {
                                    if ((!Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("price")) && (!Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("interior color")) && (!Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("exterior color")) && (!Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("car color")) && (!Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("option")))
                                    {
                                        if(Luis["entities"][i]["entity"].ToString() == "premium s")
                                        {
                                            priceWhereStr += Luis["entities"][i]["resolution"]["values"][0].ToString() + " = 'premium special' AND ";
                                        }
                                        else
                                        {
                                            priceWhereStr += Luis["entities"][i]["resolution"]["values"][0].ToString() + " = '" + Luis["entities"][i]["entity"].ToString() + "' AND ";
                                        }
                                    
                                    }
                                    else if (Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("cartrim") || Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("fuel") || Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("drivewheel") || Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("tuix") || Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("colorpackage") )
                                    {

                                        if (Luis["entities"][i]["entity"].ToString() == "premium s")
                                        {
                                            priceWhereStr += " AND C." + Luis["entities"][i]["resolution"]["values"][0].ToString() + " = 'premium special'";
                                        }
                                        else
                                        {
                                            priceWhereStr += " AND C."+Luis["entities"][i]["resolution"]["values"][0].ToString() + " = '" + Luis["entities"][i]["entity"].ToString() ;
                                        }

                                    }else if (Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("option"))
                                    {

                                        if(Luis["entities"][i]["entity"].ToString().Equals("option") || Luis["entities"][i]["entity"].ToString().Equals("options") )
                                        {
                                            carOptionStr = "option";
                                        }
                                        else 
                                        {
                                            if(Luis["entities"][i]["entity"].ToString().Length > carOptionStr.Length)
                                            {
                                                carOptionStr = Luis["entities"][i]["entity"].ToString();
                                            }
                                        }

                                    }
                                    Debug.WriteLine("priceWhereStr : " + priceWhereStr);
                                    Debug.WriteLine("carOptionStr : " + carOptionStr);

                                    if ((!Luis["entities"][i]["resolution"]["values"][0].ToString().Equals("car color")) )
                                    {
                                        if (Luis["entities"][i]["entity"].ToString() == "premium s")
                                        {
                                            entitiesStr += "premium special" + ",";
                                        }
                                        else
                                        {
                                            entitiesStr += Luis["entities"][i]["entity"].ToString() + ",";
                                        }
                                    } 
                                }
                                 
                                 */


                                else
                                {
                                    //시승작업 분류
                                    if (Luis["intents"][0]["intent"].ToString().Equals("test drive center address") || Luis["intents"][0]["intent"].ToString().Equals("test drive center info") || Luis["intents"][0]["intent"].ToString().Equals("test drive can"))
                                    {
                                        entitiesStr += ((string)Luis["entities"][i]["entity"]) + ",";
                                        //areaStr += Luis["entities"][i]["entity"].ToString() + ",";
                                        if (Luis["intents"][0]["intent"].ToString().Equals(Luis["entities"][i]["type"].ToString()))
                                        {
                                            Debug.WriteLine(" LIST ENTITIES " + Luis["entities"][i]["entity"].ToString());
                                            areaStr += Luis["entities"][i]["entity"].ToString() + ",";
                                        }
                                    }
                                    else
                                    {
                                        entitiesStr += ((string)Luis["entities"][i]["resolution"]["values"][0]) + ",";
                                        //areaStr += Luis["entities"][i]["entity"].ToString() + ",";
                                    }

                                    //지점작업 분류

                                    if (Luis["intents"][0]["intent"].ToString().Equals("car color"))
                                    {
                                        if (Luis["entities"][i]["type"].ToString().Equals("test drive center info") || Luis["entities"][i]["type"].ToString().Equals("car color"))
                                        {
                                            colorAreaStr += Luis["entities"][i]["entity"].ToString() + ",";
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Debug.WriteLine(" HIERACHICAL ENTITIES " + Luis["entities"][i]["type"]);
                                entitiesStr += Regex.Split(((string)Luis["entities"][i]["type"]), "::")[1] + ",";
                            }
                        }

                            entitiesStr = entitiesStr.Substring(0, entitiesStr.Length - 1);
                        if (areaStr.Length > 0)
                        {
                            areaStr = areaStr.Substring(0, areaStr.Length - 1);
                        }
                        else
                        {
                            areaStr = "";
                        }
                        //색상
                        if (colorAreaStr.Length > 0)
                        {
                            colorAreaStr = colorAreaStr.Substring(0, colorAreaStr.Length - 1);
                        }
                        else
                        {
                            colorAreaStr = "";
                        }


                        Debug.WriteLine("LUIS : '" + (string)Luis["intents"][0]["intent"] + "','" + entitiesStr + "'");
                        Debug.WriteLine("areaStr : '" + areaStr + "'");
                    }
                    try
                    {
                        List<DialogList> dlg = new List<DialogList>();

                        List<LuisList> LuisDialogID = db.SelectLuis((string)Luis["intents"][0]["intent"], entitiesStr);

                        Debug.WriteLine("LuisDialogID count : " + LuisDialogID.Count);

                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Query Analysis
                        // Result from LUIS or History available, but (if) no dialogue or (else) has dialogue available
                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        /* 아래로 이동
                        if (LuisDialogID.Count == 0)
                        {
                            int dbResult = db.insertUserQuery(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), (string)Luis["intents"][0]["intent"], entitiesStr, luisID, 'D', colorAreaStr, areaStr);
                            Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());
                        }
                        else
                        {
                            int dbResult = db.insertUserQuery(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), (string)Luis["intents"][0]["intent"], entitiesStr, luisID, 'H', colorAreaStr, areaStr);
                            Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());
                        }
                        */

                        Activity replyToConversation = activity.CreateReply();

                        for (int k = 0; k < LuisDialogID.Count; k++)
                        {
                            replyToConversation.Recipient = activity.From;
                            replyToConversation.Type = "message";
                            replyToConversation.Attachments = new List<Attachment>();
                            replyToConversation.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                            dlg = db.SelectDialog(LuisDialogID[k].dlgId);

                            if (dlg.Count > 0)
                            {
                                Debug.WriteLine("dlg[0].dlgMent : [" + dlg[0].dlgMent + "]");
                                Debug.WriteLine("dlg[0].dlgMent : [" + string.IsNullOrEmpty(dlg[0].dlgMent) + "]");
                                if (string.IsNullOrEmpty(dlg[0].dlgMent) != true )
                                {
                                    Activity reply = activity.CreateReply(dlg[0].dlgMent.ToString());
                                    await connector.Conversations.ReplyToActivityAsync(reply);
                                }
                            }

                            //시승센터 작업                        
                            Debug.WriteLine("Luis.intents[0].intent = " + (string)Luis["intents"][0]["intent"].ToString());
                            Debug.WriteLine("entitiesStr = " + entitiesStr);

                            //시승센터 주소
                            if (Luis["intents"][0]["intent"].ToString().Equals("test drive center address"))
                            {

                                if (string.IsNullOrEmpty(areaStr))
                                {
                                    areaStr = colorAreaStr;
                                }
                                List<TestDriverCenterList> TestDriverDialog = db.SelectTestDriverDialog(areaStr);

                                Activity reply_ment = activity.CreateReply();
                                reply_ment.Recipient = activity.From;
                                reply_ment.Type = "message";
                                reply_ment.Text = "주소는 ‘" + TestDriverDialog[0].dlgCtrAddr + "'입니다.\n\n  맵으로 위치를 보시려면 아래 이미지를 선택해주세요.";
                                var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                                for (int td = 0; td < TestDriverDialog.Count; td++)
                                {
                                    APIExamMapGeocode.getCodeNaver(TestDriverDialog[td].dlgCtrAddr);
                                    /*
                                    replyToConversation.Attachments.Add(
                                    GetHeroCard_tap(
                                    TestDriverDialog[td].dlgCtrNM + " 시승센터",
                                    "(연중무휴)10-16시까지 예약 가능",
                                    "TEL." + TestDriverDialog[td].dlgCtrPhone + " " + TestDriverDialog[td].dlgCtrAddr,
                                    new CardImage(url: "http://www.smartsend.co.kr/map/" + TestDriverDialog[td].dlgMapXTn + "," + TestDriverDialog[td].dlgMapYTn + ".png"))
                                    );
                                    */
                                    replyToConversation.Attachments.Add(
                                    UserGetHeroCard_location(
                                    TestDriverDialog[td].dlgCtrNM + " 시승센터",
                                    "(연중무휴)10-16시까지 예약 가능",
                                    "TEL." + TestDriverDialog[td].dlgCtrPhone + " " + TestDriverDialog[td].dlgCtrAddr,
                                    new CardImage(url: "http://www.smartsend.co.kr/map/" + APIExamMapGeocode.ll.lat.ToString() + "," + APIExamMapGeocode.ll.lon.ToString() + ".png"),
                                    TestDriverDialog[td].dlgMapXTn,
                                    TestDriverDialog[td].dlgMapYTn));
                                }
                                luis_intent = (string)Luis["intents"][0]["intent"];
                            }
                            //시승센터 정보
                            else if (Luis["intents"][0]["intent"].ToString().Equals("test drive center info") && entitiesStr.ToString() != "nearest.test drive center" && entitiesStr != "I am now a new nonhyeon station")
                            {
                                Activity reply_ment = activity.CreateReply();
                                reply_ment.Recipient = activity.From;
                                reply_ment.Type = "message";
                                reply_ment.Text = EngTransferKor.GetEngTransferKor(areaStr) + " 시승센터 정보를 보여드릴게요";
                                var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                                if (string.IsNullOrEmpty(areaStr))
                                {
                                    areaStr = colorAreaStr;
                                }

                                List<TestDriverCenterList> TestDriverDialog = db.SelectTestDriverDialog(areaStr);
                                for (int td = 0; td < TestDriverDialog.Count; td++)
                                {
                                    replyToConversation.Attachments.Add(
                                    GetHeroCard(
                                    TestDriverDialog[td].dlgCtrNM + " 시승센터",
                                    "(연중무휴)10-16시까지 예약 가능",
                                    "TEL." + TestDriverDialog[td].dlgCtrPhone + " " + TestDriverDialog[td].dlgCtrAddr,
                                    new CardImage(),
                                    new CardAction(ActionTypes.ImBack, "전화하기", value: TestDriverDialog[td].dlgCtrPhone),
                                    new CardAction(ActionTypes.ImBack, "주소보기", value: TestDriverDialog[td].dlgCtrNM + " 시승센터 주소를 알려줘"),
                                    new CardAction(ActionTypes.ImBack, "시승 가능 차량 보기", value: TestDriverDialog[td].dlgCtrNM + " 시승센터에서 시승 가능한 차량을 보여줘"))
                                    );
                                }
                                luis_intent = (string)Luis["intents"][0]["intent"];
                            }
                            //시승가능 차량
                            else if (Luis["intents"][0]["intent"].ToString().Equals("test drive can"))
                            {
                                if (string.IsNullOrEmpty(areaStr))
                                {
                                    areaStr = colorAreaStr;
                                }

                                List<TestDriverCenterList> TestDriverDialog = db.SelectTestDriverDialog(areaStr);
                                for (int td = 0; td < TestDriverDialog.Count; td++)
                                {
                                    replyToConversation.Attachments.Add(
                                    GetHeroCard_info(
                                    TestDriverDialog[td].dlgCtrDtlInfo,
                                    TestDriverDialog[td].dlgXrclCtyNM,
                                    "",
                                    new CardImage(url: "https://bottest.hyundai.com/assets/images/tussanTest/tucson_02.jpg"))
                                    );
                                }

                                //루이스 인텐트 반복, 다이얼로그 2개 출력
                                luis_intent = (string)Luis["intents"][0]["intent"];
                                Luis["intents"][0]["intent"] = "";
                            }
                            //색상별 차량
                            else if (Luis["intents"][0]["intent"].ToString().Equals("showroom"))
                            {
                                Activity reply_showroom = activity.CreateReply();
                                reply_showroom.Recipient = activity.From;
                                reply_showroom.Type = "message";
                                reply_showroom.Text = "네, 가까운 매장에서 차를 직접 보시는 것도 좋을 것 같아요. \n\n  원하시는 차량 외장 컬러를 아래에서 선택해주세요.";
                                var reply_showroom1 = await connector.Conversations.SendToConversationAsync(reply_showroom);

                                //임시 색상
                                //string[] imsiColor = new string[10] { "초크 화이트", "레이크 실버", "벨벳 듄", "다크 나이트", "팬텀 블랙", "블루 라군", "세라믹 블루", "텐저린 코멧", "펄스 레드", "애시드 엘로우" };
                                string[] imsiColor = new string[10] { "chalk white", "lake silver", "velvet dune", "dark night", "phantom black", "blue lagoon", "ceramic blue", "tangerine comet", "pulse red", "acid yellow" };

                                List<CarColorList> CarColorListDialog = db.SelectCarColorListDialog();
                                for (int td = 0; td < CarColorListDialog.Count; td++)
                                {
                                    replyToConversation.Attachments.Add(
                                    GetHeroCard_show(
                                    "",
                                    //CarColorListDialog[td].dlgXrclCtyNM,
                                    ColorTransfer.GetColorTransfer(imsiColor[td]),
                                    CarColorListDialog[td].dlCnt + "개 매장에 전시",
                                    new CardImage(url: "https://bottest.hyundai.com/assets/images/tussanTest/tucson_02.jpg"),
                                    //new CardAction(ActionTypes.ImBack, "전시 차량 보기", value: CarColorListDialog[td].dlgXrclCtyNM + " 컬러가 있는 매장을 알려줘"))
                                    new CardAction(ActionTypes.ImBack, "전시 매장 보기", value: ColorTransfer.GetColorTransfer(imsiColor[td]) + " 컬러가 있는 매장을 알려줘"))
                                    );
                                }
                                luis_intent = (string)Luis["intents"][0]["intent"];
                            }
                            //색상별 지역 출력
                            else if (Luis["intents"][0]["intent"].ToString().Equals("car color") && entitiesStr.Contains("test drive center info"))
                            {
                                //string test1 = "colorAreaStr";
                                string[] colorAreaStrResult;

                                if (colorAreaStr.Contains(","))
                                {
                                    colorAreaStrResult = colorAreaStr.Split(',');
                                }else
                                {
                                    colorAreaStrResult = areaStr.Split(',');                                   
                                }                                

                                Debug.WriteLine("colorAreaStr = " + colorAreaStrResult[0]);
                                Debug.WriteLine("colorAreaStr = " + colorAreaStrResult[1]);

                                List<CarBranchList> CarBranchListDialog = db.SelectCarBranchListDialog(EngTransferKor.GetEngTransferKor(colorAreaStrResult[0]), ColorTransfer.GetColorTransfer(colorAreaStrResult[1]));
                                for (int td = 0; td < CarBranchListDialog.Count; td++)
                                {
                                    replyToConversation.Attachments.Add(
                                    GetHeroCard_show(
                                    CarBranchListDialog[td].dlgBrNM,
                                    CarBranchListDialog[td].dlgDspBrTN,
                                    CarBranchListDialog[td].dlgBrAddr,
                                    new CardImage(),
                                    //new CardAction(ActionTypes.ImBack, "매장 보기", value: CarBranchListDialog[td].dlgBrNM + "지점 " + ColorTransfer.GetColorTransfer(areaStr.ToString().Replace(" color", "")) + " 매장 보여줘"))
                                    new CardAction(ActionTypes.ImBack, "매장 보기", value: CarBranchListDialog[td].dlgBrNM + "지점 보여줘"))
                                    );
                                }
                                luis_intent = (string)Luis["intents"][0]["intent"];
                            }
                            //색상별 지점 출력
                            else if (Luis["intents"][0]["intent"].ToString().Equals("car color"))
                            {                    
                                if(string.IsNullOrEmpty(areaStr))
                                {
                                    areaStr = colorAreaStr;
                                }

                                List<CarColorAreaList> CarColorListAreaDialog = db.SelectCarColorAreaListDialog(ColorTransfer.GetColorTransfer(areaStr.ToString().Replace(" color", "")));
                                for (int td = 0; td < CarColorListAreaDialog.Count; td++)
                                {
                                    replyToConversation.Attachments.Add(
                                    GetHeroCard_show(
                                    CarColorListAreaDialog[td].dlgAddr,
                                    "",
                                    "",
                                    new CardImage(),
                                    new CardAction(ActionTypes.ImBack, "매장 보기", value: CarColorListAreaDialog[td].dlgAddr + "에 " + ColorTransfer.GetColorTransfer(areaStr.ToString().Replace(" color", "")) + " 컬러가 전시된 매장이에요"))
                                    );
                                }
                                luis_intent = (string)Luis["intents"][0]["intent"];
                            }
                            //현재 위치 사용 승인
                            else if (Luis["intents"][0]["intent"].ToString().Equals("test drive location"))
                            {
                                if(entitiesStr.Contains("do not use") || entitiesStr.Contains("search directly"))
                                {
                                    List<AreaTestCenterCountList> AreaTestCenterCountList = db.SelectAreaTestCenterCountListDialog();
                                    for (int td = 0; td < AreaTestCenterCountList.Count; td++)
                                    {
                                        replyToConversation.Attachments.Add(
                                        GetHeroCard_show(
                                        AreaTestCenterCountList[td].dlgAreaNM + " 시승센터",
                                        AreaTestCenterCountList[td].dlgAreaList + " 등 총 " + AreaTestCenterCountList[td].dlgAreaCnt + " 곳",
                                        "",
                                        new CardImage(),
                                        new CardAction(ActionTypes.ImBack, "정보보기", value: AreaTestCenterCountList[td].dlgAreaNM + " 시승센터 알려줘"))
                                        );
                                    }
                                } else { 
                                    Geolocation.getRegion();
                                    String area = EngTransferKor.GetEngTransferKor(Geolocation.ll.regionName.ToLower().ToString());
                                    List<AreaTestCenterList> AreaTestCenterList = db.SelectAreaTestCenterListDialog(area);
                                    for (int td = 0; td < AreaTestCenterList.Count; td++)
                                    {
                                        replyToConversation.Attachments.Add(
                                        GetHeroCard(
                                        AreaTestCenterList[td].dlgCtrNM + " 시승센터",
                                        "(연중무휴)10-16시까지 예약 가능",
                                        "TEL." + AreaTestCenterList[td].dlgCtrPhone + " " + AreaTestCenterList[td].dlgCtrAddr,
                                        new CardImage(""),
                                        new CardAction(ActionTypes.ImBack, "전화하기", value: AreaTestCenterList[td].dlgCtrPhone),
                                        new CardAction(ActionTypes.ImBack, "주소보기", value: AreaTestCenterList[td].dlgCtrNM + " 시승센터 주소를 알려줘"),
                                        new CardAction(ActionTypes.ImBack, "시승 가능 차량 보기", value: AreaTestCenterList[td].dlgCtrNM + " 시승센터에서 시승 가능한 차량을 보여줘"))
                                        );
                                    }
                                }
                                luis_intent = (string)Luis["intents"][0]["intent"];
                            }


                            else if ( (Luis["intents"][0]["intent"].ToString().Equals("show color") && Luis["entities"].ToString().Equals("color")) || (Luis["intents"][0]["intent"].ToString().Equals("show color") && Luis["entities"][0]["entity"].ToString().Equals("color")))
                            {

                                Debug.WriteLine("외장 색상 다 보여주자.!!!!");

                                List<CarExColorList> CarExColorList = db.SelectCarExColorAllList();
                                for (int td = 0; td < CarExColorList.Count; td++)
                                {

                                    string trimNM = CarExColorList[td].trimColorNm;
                                    trimNM = trimNM.Replace("가솔린 ", "");
                                    trimNM = trimNM.Replace("디젤 ", "");
                                    trimNM = trimNM.Replace("2WD ", "");
                                    trimNM = trimNM.Replace("4WD ", "");

                                    replyToConversation.Attachments.Add(
                                    GetHeroCard_info(
                                    trimNM,
                                    //CarExColorList[td].trimColorNm,
                                    "추가 금액 : "+string.Format("{0}", CarExColorList[td].exColorPrice.ToString("n0")) + "원",
                                    "",
                                    new CardImage(url: "https://bottest.hyundai.com/assets/images/price/exterior/" + CarExColorList[td].trimColorCd + ".jpg"))
                                    );
                                }
                                luis_intent = (string)Luis["intents"][0]["intent"];
                            }



                            else if ( (Luis["intents"][0]["intent"].ToString().Equals("car quote")))
                            {
                                Debug.WriteLine(luisID + " : priceWhereStr before : " + priceWhereStr);
                                
                                if (priceWhereStr.Length > 0)
                                {
                                    Debug.WriteLine("AA : " + priceWhereStr.Substring(0, priceWhereStr.Length - 1));
                                    if (priceWhereStr.Substring(priceWhereStr.Length - 4).Contains("AND"))
                                    {
                                        priceWhereStr = priceWhereStr.Substring(0, priceWhereStr.Length - 4);
                                    }
                                }
                                
                                Debug.WriteLine("priceWhereStr after : " + priceWhereStr.Substring(0, priceWhereStr.Length - 1));

                                if (carOptionStr.Length > 0)
                                {
                                    Debug.WriteLine("carOptionStr : " + carOptionStr);
                                    Debug.WriteLine("priceWhereStr : " + priceWhereStr);
                                }

                                if (priceWhereStr != "")
                                {
                                    
                                    if (!priceWhereStr.Contains("option"))
                                    {
                                        Debug.WriteLine("엔진 및 트림 구분");

                                        List<CarTrimList> CarTrimList = db.SelectCarTrimList1(priceWhereStr.Substring(0, priceWhereStr.Length - 1));

                                        for (int td = 0; td < CarTrimList.Count; td++)
                                        {

                                            string trimNM = CarTrimList[td].carTrimNm;
                                            trimNM = trimNM.Replace("가솔린 ", "");
                                            trimNM = trimNM.Replace("디젤 ", "");
                                            trimNM = trimNM.Replace("2WD ", "");
                                            trimNM = trimNM.Replace("4WD ", "");


                                            if(CarTrimList[td].saleCD.Contains("XX"))
                                            {
                                                string color = (string)(CarTrimList[td].tuix + CarTrimList[td].cartrim);
                                                replyToConversation.Attachments.Add(
                                                GetHeroCard_show(
                                                trimNM,
                                                //CarTrimList[td].carTrimNm,
                                                "추가 금액 : " + string.Format("{0}", CarTrimList[td].salePrice.ToString("n0")) + "원",
                                                "",
                                                new CardImage(url: "https://bottest.hyundai.com/assets/images/price/trim/" + color.Replace(" ", "") + ".jpg"),
                                                new CardAction(ActionTypes.ImBack, "가격", value: CarTrimList[td].carTrimNm + " 트림 가격"))
                                                );
                                            }
                                            else
                                            {
                                                string color = (string)(CarTrimList[td].tuix + CarTrimList[td].cartrim);
                                                replyToConversation.Attachments.Add(
                                                GetHeroCard_show(
                                                trimNM,
                                                //CarTrimList[td].carTrimNm,
                                                "추가 금액 : " + string.Format("{0}", CarTrimList[td].salePrice.ToString("n0")) + "원",
                                                "",
                                                new CardImage(url: "https://bottest.hyundai.com/assets/images/price/trim/" + color.Replace(" ", "") + ".jpg"),
                                                new CardAction(ActionTypes.ImBack, "트림", value: CarTrimList[td].carTrimNm + " 트림 보여줄래"))
                                                );
                                            }

                                            //string color = (string)(CarTrimList[td].tuix + CarTrimList[td].cartrim);
                                            //replyToConversation.Attachments.Add(
                                            //GetHeroCard_show(
                                            //trimNM,
                                            ////CarTrimList[td].carTrimNm,
                                            //"추가 금액 : " + string.Format("{0}", CarTrimList[td].salePrice.ToString("n0")) + "원",
                                            //"",
                                            //new CardImage(url: "https://bottest.hyundai.com/assets/images/price/trim/" + color.Replace(" ", "") + ".jpg"),
                                            //new CardAction(ActionTypes.ImBack, "트림", value: CarTrimList[td].carTrimNm + " 트림 보여줄래"))
                                            //);
                                        }
                                    }
                                    else
                                    {
                                        //SelectOptionList
                                        List<CarOptionList> carOptionList = db.SelectOptionList(priceWhereStr.Substring(0, priceWhereStr.Length - 1));
                                        for (int td = 0; td < carOptionList.Count; td++)
                                        {

                                            string trimNM = carOptionList[td].model;
                                            trimNM = trimNM.Replace("가솔린 ", "");
                                            trimNM = trimNM.Replace("디젤 ", "");
                                            trimNM = trimNM.Replace("2WD ", "");
                                            trimNM = trimNM.Replace("4WD ", "");

                                            translateInfo = await getTranslate(carOptionList[td].optNm);
                                            replyToConversation.Attachments.Add(
                                            GetHeroCard_info(
                                            trimNM,
                                            //CarTrimList[td].carTrimNm,
                                            "추가 금액 : " + string.Format("{0}", carOptionList[td].optPrice.ToString("n0")) + "원",
                                            "",
                                            new CardImage(url: "https://bottest.hyundai.com/assets/images/price/option/" + translateInfo.data.translations[0].translatedText + ".jpg"))
                                            //new CardAction(ActionTypes.ImBack, "트림", value: CarTrimList[td].carTrimNm + " 트림 보여줄래"))
                                            );
                                        }
                                    }

                                    //}
                                    //else
                                    //{
                                    //    Debug.WriteLine("엔진 ");
                                    //    for (int i = 0; i < CarPriceList.Count(); i++)
                                    //    {
                                    //        if (CarPriceList[i].saleCd.Contains("X"))
                                    //        {
                                    //            carCdType += "'" + CarPriceList[i].carCdType + "',";
                                    //        }
                                    //    }

                                    //    carCdType = carCdType.Substring(0, carCdType.Length - 1);

                                    //    List<CarModelList> CarModelList = db.SelectCarModelInList(carCdType);
                                    //    for (int td = 0; td < CarModelList.Count; td++)
                                    //    {

                                    //        string trimNM = CarModelList[td].carModelNm;
                                    //        trimNM = trimNM.Replace("가솔린 ", "");
                                    //        trimNM = trimNM.Replace("디젤 ", "");
                                    //        trimNM = trimNM.Replace("2WD ", "");
                                    //        trimNM = trimNM.Replace("4WD ", "");

                                    //        replyToConversation.Attachments.Add(
                                    //        GetHeroCard_show(
                                    //        //trimNM,
                                    //        CarModelList[td].carModelNm,
                                    //        "",
                                    //        "",
                                    //        new CardImage(url: PriceImageList.GetPriceImage(CarModelList[td].carModelNm)),
                                    //        new CardAction(ActionTypes.ImBack, CarModelList[td].carModelNm + " 가격", value: CarModelList[td].carModelNm + " 트림 가격"))
                                    //        );
                                    //    }
                                    //    }
                                    //}
                                }




                                //if (priceWhereStr != "")
                                //{
                                //    Debug.WriteLine("엔진 및 트림 구분");

                                //    List<CarPriceList> CarPriceList = db.SelectCarPriceList(priceWhereStr);
                                //    string carCdType = "";

                                //    if (CarPriceList.Count() > 0)
                                //    {
                                //        carCdType = "";
                                //        if (!CarPriceList[0].saleCd.Contains("X"))
                                //        {
                                //            Debug.WriteLine("트림");
                                //            //string where = "";
                                //            //string fnumCd = "";
                                //            //string saleCd = "";

                                //            //for (int i = 0; i < CarPriceList.Count(); i++)
                                //            //{
                                //            //    if (CarPriceList[i].saleCd.Contains("X"))
                                //            //    {
                                //            //        Debug.WriteLine("XXXXXXXXXXXXXXXXXXXXXX");
                                //            //    }
                                //            //    else
                                //            //    {
                                //            //        if (orgMent.Contains("칼라패키지"))
                                //            //        {
                                //            //            Debug.WriteLine("칼라패키지");
                                //            //            saleCd = "SALE_CD = '" + CarPriceList[i].saleCd + "'";
                                //            //            fnumCd = "FNUM_CD = '" + CarPriceList[i].fnumCd + "'";
                                //            //        }
                                //            //        else
                                //            //        {
                                //            //            carCdType += "'" + CarPriceList[i].saleCd + "',";
                                //            //        }

                                //            //    }
                                //            //}

                                //            //if (carCdType == "")
                                //            //{
                                //            //    where = saleCd + " AND " + fnumCd;
                                //            //}
                                //            //else
                                //            //{
                                //            //        carCdType = carCdType.Substring(0, carCdType.Length - 1);
                                //            //        where = " SALE_CD IN ( " + carCdType + "  ) ";

                                //            //}

                                //            List<CarTrimList> CarTrimList = db.SelectCarTrimList1(priceWhereStr);

                                //            for (int td = 0; td < CarTrimList.Count; td++)
                                //            {

                                //                string trimNM = CarTrimList[td].carTrimNm;
                                //                trimNM = trimNM.Replace("가솔린 ", "");
                                //                trimNM = trimNM.Replace("디젤 ", "");
                                //                trimNM = trimNM.Replace("2WD ", "");
                                //                trimNM = trimNM.Replace("4WD ", "");

                                //                string color = (string)(CarTrimList[td].tuix + CarTrimList[td].cartrim);
                                //                replyToConversation.Attachments.Add(
                                //                GetHeroCard_show(
                                //                trimNM,
                                //                //CarTrimList[td].carTrimNm,
                                //                "추가 금액 : " + string.Format("{0}", CarTrimList[td].salePrice.ToString("n0")) + "원",
                                //                "",
                                //                new CardImage(url: "https://bottest.hyundai.com/assets/images/price/trim/" + color.Replace(" ", "") + ".jpg"),
                                //                new CardAction(ActionTypes.ImBack, "트림", value: CarTrimList[td].carTrimNm + " 트림 보여줄래"))
                                //                );
                                //            }



                                //        }
                                //        else
                                //        {
                                //            Debug.WriteLine("엔진 ");
                                //            for (int i = 0; i < CarPriceList.Count(); i++)
                                //            {
                                //                if (CarPriceList[i].saleCd.Contains("X"))
                                //                {
                                //                    carCdType += "'" + CarPriceList[i].carCdType + "',";
                                //                }
                                //            }

                                //            carCdType = carCdType.Substring(0, carCdType.Length - 1);

                                //            List<CarModelList> CarModelList = db.SelectCarModelInList(carCdType);
                                //            for (int td = 0; td < CarModelList.Count; td++)
                                //            {

                                //                string trimNM = CarModelList[td].carModelNm;
                                //                trimNM = trimNM.Replace("가솔린 ", "");
                                //                trimNM = trimNM.Replace("디젤 ", "");
                                //                trimNM = trimNM.Replace("2WD ", "");
                                //                trimNM = trimNM.Replace("4WD ", "");

                                //                replyToConversation.Attachments.Add(
                                //                GetHeroCard_show(
                                //                //trimNM,
                                //                CarModelList[td].carModelNm,
                                //                "",
                                //                "",
                                //                new CardImage(url: PriceImageList.GetPriceImage(CarModelList[td].carModelNm)),
                                //                new CardAction(ActionTypes.ImBack, CarModelList[td].carModelNm + " 가격", value: CarModelList[td].carModelNm + " 트림 가격"))
                                //                );
                                //            }
                                //        }
                                //    }
                                //}
                                else
                                {
                                    Debug.WriteLine("엔진 전체");
                                    List<CarModelList> CarModelList = db.SelectCarModelList();
                                    for (int td = 0; td < CarModelList.Count; td++)
                                    {
                                        replyToConversation.Attachments.Add(
                                        GetHeroCard_show(
                                        CarModelList[td].carModelNm,
                                        "",
                                        "",
                                        new CardImage(url: PriceImageList.GetPriceImage(CarModelList[td].carModelNm)),
                                        new CardAction(ActionTypes.ImBack, CarModelList[td].carModelNm + " 가격", value: CarModelList[td].carModelNm + " 트림 가격"))
                                        );
                                    }

                                }
                                luis_intent = (string)Luis["intents"][0]["intent"];
                            }



                            //else if ((Luis["intents"][0]["intent"].ToString().Equals("car option")))
                            //{
                            //    string color = "";

                            //    Debug.WriteLine(luisID + " : priceWhereStr before : " + priceWhereStr);

                            //    if (priceWhereStr.Length > 0)
                            //    {
                            //        Debug.WriteLine("AA : " + priceWhereStr.Substring(priceWhereStr.Length - 4).Contains("AND"));
                            //        if (priceWhereStr.Substring(priceWhereStr.Length - 4).Contains("AND"))
                            //        {
                            //            priceWhereStr = priceWhereStr.Substring(0, priceWhereStr.Length - 4);
                            //        }
                                    
                            //    }

                            //    Debug.WriteLine("priceWhereStr after : " + priceWhereStr);

                            //    List<CarPriceList> CarPriceList = db.SelectCarPriceList(priceWhereStr);
                                
                            //    if (CarPriceList.Count() > 0)
                            //    {
                            //        for (int i = 0; i < CarPriceList.Count(); i++)
                            //        {

                            //            if (CarPriceList[i].saleCd.Contains("X"))
                            //            {
                            //                Debug.WriteLine("XXXXXXXXXXXXXXXXXXXXXX");
                            //            }
                            //            else
                            //            {

                            //                string trimNM = CarPriceList[i].modelNm;
                            //                trimNM = trimNM.Replace("가솔린 ", "");
                            //                trimNM = trimNM.Replace("디젤 ", "");
                            //                trimNM = trimNM.Replace("2WD ", "");
                            //                trimNM = trimNM.Replace("4WD ", "");

                            //                color = (string)(CarPriceList[i].tuix + CarPriceList[i].cartrim);
                            //                Debug.WriteLine("color : " + color);
                            //                replyToConversation.Attachments.Add(
                            //                GetHeroCard_show(
                            //                    trimNM.Replace(" 1.6", ""),
                            //                    //CarPriceList[i].modelNm.Replace(" 1.6",""),
                            //                    "",
                            //                    "",
                            //                    new CardImage(url: "https://bottest.hyundai.com/assets/images/price/trim/"+ color.Replace(" ", "") + ".jpg"),
                            //                    new CardAction(ActionTypes.ImBack, "옵션보기", value: CarPriceList[i].modelNm.Replace(" 1.6", "") + " 옵션 보여줄래"))
                            //                );

                            //            }
                            //        }
                            //    }
                            //    luis_intent = (string)Luis["intents"][0]["intent"];
                            //}

                            else if ((Luis["intents"][0]["intent"].ToString().Equals("show color")))
                            {
                                string color = "";

                                Debug.WriteLine(luisID + " : priceWhereStr before : " + priceWhereStr);
                                
                                if (priceWhereStr.Length > 0)
                                {

                                    Debug.WriteLine("AA : " + priceWhereStr.Substring(priceWhereStr.Length - 4).Contains("AND"));
                                    if (priceWhereStr.Substring(priceWhereStr.Length - 4).Contains("AND"))
                                    {
                                        priceWhereStr = priceWhereStr.Substring(0, priceWhereStr.Length - 4);
                                    }
                                }

                                Debug.WriteLine("priceWhereStr after : " + priceWhereStr);

                                List<CarPriceList> CarPriceList = db.SelectCarPriceList(priceWhereStr);
                                
                                if (CarPriceList.Count() > 0)
                                {
                                    for (int i = 0; i < CarPriceList.Count(); i++)
                                    {

                                        if (CarPriceList[i].saleCd.Contains("X"))
                                        {
                                            Debug.WriteLine("XXXXXXXXXXXXXXXXXXXXXX");
                                        }
                                        else
                                        {
                                            if(entitiesStr.Contains("exterior"))
                                            {

                                                string trimNM = CarPriceList[i].modelNm;
                                                trimNM = trimNM.Replace("가솔린 ", "");
                                                trimNM = trimNM.Replace("디젤 ", "");
                                                trimNM = trimNM.Replace("2WD ", "");
                                                trimNM = trimNM.Replace("4WD ", "");

                                                color = (string)(CarPriceList[i].tuix + CarPriceList[i].cartrim);
                                                Debug.WriteLine("color : " + color);
                                                replyToConversation.Attachments.Add(
                                                GetHeroCard_show(
                                                    trimNM.Replace(" 1.6", ""),
                                                    //CarPriceList[i].modelNm.Replace(" 1.6", ""),
                                                    "",
                                                    "",
                                                    new CardImage(url: "https://bottest.hyundai.com/assets/images/price/trim/" + color.Replace(" ", "") + ".jpg"),
                                                    new CardAction(ActionTypes.ImBack, "외장색상", value: CarPriceList[i].modelNm.Replace(" 1.6", "") + " 외장색상 보여줄래"))
                                                );
                                            }
                                            else if(entitiesStr.Contains("interior"))
                                            {
                                                color = (string)(CarPriceList[i].tuix + CarPriceList[i].cartrim);
                                                Debug.WriteLine("color : " + color);
                                                replyToConversation.Attachments.Add(
                                                GetHeroCard_show(
                                                    CarPriceList[i].modelNm.Replace(" 1.6", ""),
                                                    "",
                                                    "",
                                                    new CardImage(url: "https://bottest.hyundai.com/assets/images/price/trim/" + color.Replace(" ", "") + ".jpg"),
                                                    new CardAction(ActionTypes.ImBack, "내장색상", value: CarPriceList[i].modelNm.Replace(" 1.6", "") + " 내장색상 보여줄래"))
                                                );
                                            }
                                            

                                        }
                                    }
                                }
                                luis_intent = (string)Luis["intents"][0]["intent"];
                            }
                            else
                            {
                                Debug.WriteLine("(LuisDialogID[k].dlgId ====" + (LuisDialogID[k].dlgId));

                                List<CardList> card = db.SelectDialogCard(LuisDialogID[k].dlgId);

                                if (string.IsNullOrEmpty(luis_intent))
                                {
                                    luis_intent = (string)Luis["intents"][0]["intent"];
                                }
                                

                                if (card.Count > 0)
                                {
                                    // HeroCard 

                                    for (int i = 0; i < card.Count; i++)
                                    {
                                        List<ButtonList> btn = db.SelectBtn(card[i].dlgId, card[i].cardId);
                                        List<ImagesList> img = db.SelectImage(card[i].dlgId, card[i].cardId);
                                        List<MediaList> media1 = db.SelectMedia(card[i].dlgId, card[i].cardId);

                                        List<CardImage> cardImages = new List<CardImage>();
                                        CardImage[] plImage = new CardImage[img.Count];

                                        ThumbnailUrl plThumnail = new ThumbnailUrl();

                                        List<CardAction> cardButtons = new List<CardAction>();
                                        CardAction[] plButton = new CardAction[btn.Count];

                                        CardAction tap = new CardAction();

                                        List<MediaUrl> mediaURL1 = new List<MediaUrl>();
                                        MediaUrl[] plMediaUrl1 = new MediaUrl[media1.Count];

                                        ReceiptCard[] plReceiptCard = new ReceiptCard[card.Count];
                                        HeroCard[] plHeroCard = new HeroCard[card.Count];
                                        VideoCard[] plVideoCard = new VideoCard[card.Count];
                                        Attachment[] plAttachment = new Attachment[card.Count];

                                        //if(img.Count == 0) { plTap[i] = null; }
                                        for (int l = 0; l < img.Count; l++)
                                        {
                                            if (card[i].cardType == "herocard")
                                            {
                                                if (img[l].imgUrl != null)
                                                {
                                                    plImage[l] = new CardImage()
                                                    {
                                                        Url = img[l].imgUrl
                                                    };
                                                }
                                            }
                                            else if (card[i].cardType == "videocard")
                                            {
                                                if (img[l].imgUrl != null)
                                                {
                                                    plThumnail.Url = img[l].imgUrl;
                                                }
                                            }
                                        }

                                        cardImages = new List<CardImage>(plImage);

                                        Console.WriteLine("111");

                                        //for (int l = 0; l < img.Count; l++)
                                        //{
                                        //    if (card[i].cardType == "herocard")
                                        //    {
                                        //        if (img[l].imgUrl != null)
                                        //        {
                                        //            tap = new CardAction()
                                        //            {
                                        //                Type = "openUrl",
                                        //                Title = "Open",
                                        //                Value = img[l].imgUrl
                                        //            };
                                        //        }
                                        //    }
                                        //}
                                        //cardTaps = new List<CardAction>(plTap);
                                        //Debug.WriteLine("222");
                                        for (int l = 0; l < media1.Count; l++)
                                        {
                                            if (media1[l].mediaUrl != null)
                                            {
                                                plMediaUrl1[l] = new MediaUrl()
                                                {
                                                    Url = media1[l].mediaUrl
                                                };
                                            }
                                        }
                                        mediaURL1 = new List<MediaUrl>(plMediaUrl1);

                                        for (int m = 0; m < btn.Count; m++)
                                        {
                                            if (btn[m].btnTitle != null)
                                            {
                                                plButton[m] = new CardAction()
                                                {
                                                    Value = btn[m].btnContext,
                                                    Type = btn[m].btnType,
                                                    Title = btn[m].btnTitle
                                                };
                                            }
                                        }
                                        cardButtons = new List<CardAction>(plButton);

                                        if (card[i].cardType == "herocard")
                                        {
                                            plHeroCard[i] = new HeroCard()
                                            {
                                                Title = card[i].cardTitle,
                                                Text = card[i].cardText,
                                                Subtitle = card[i].cardSubTitle,
                                                Images = cardImages,
                                                //Tap = tap,
                                                Buttons = cardButtons
                                            };

                                            plAttachment[i] = plHeroCard[i].ToAttachment();
                                            replyToConversation.Attachments.Add(plAttachment[i]);
                                        }
                                        else if (card[i].cardType == "videocard")
                                        {
                                            plVideoCard[i] = new VideoCard()
                                            {
                                                Title = card[i].cardTitle,
                                                Text = card[i].cardText,
                                                Subtitle = card[i].cardSubTitle,
                                                Image = plThumnail,
                                                Media = mediaURL1,
                                                Buttons = cardButtons,
                                                Autostart = false
                                            };

                                            plAttachment[i] = plVideoCard[i].ToAttachment();
                                            replyToConversation.Attachments.Add(plAttachment[i]);
                                        }
                                    }

                                    //Thread.Sleep(3000);
                                }
                            }

                            var reply1 = await connector.Conversations.SendToConversationAsync(replyToConversation);
                        }
                        //}

                        if (LuisDialogID.Count == 0)
                        {
                            int dbResult = db.insertUserQuery(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), luis_intent, entitiesStr, luisID, 'D', colorAreaStr, areaStr, priceWhereStr, carOptionStr);
                            Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());
                        }
                        else
                        {
                            int dbResult = db.insertUserQuery(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), luis_intent, entitiesStr, luisID, 'H', colorAreaStr, areaStr, priceWhereStr, carOptionStr);
                            Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());
                        }

                        DateTime endTime = DateTime.Now;
                        Debug.WriteLine("USER NUMBER : " + activity.Conversation.Id);
                        Debug.WriteLine("CUSTOMMER COMMENT KOREAN : " + activity.Text);
                        Debug.WriteLine("CUSTOMMER COMMENT ENGLISH : " + translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));

                        Debug.WriteLine("CHATBOT_COMMENT_CODE : " + dlg[0].dlgNm);
                        Debug.WriteLine("CHANNEL_ID : " + activity.ChannelId);
                        Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - startTime).Milliseconds));

                        inserResult = db.insertHistory(activity.Conversation.Id, activity.Text, translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), dlg[0].dlgNm, activity.ChannelId, ((endTime - startTime).Milliseconds));
                        if (inserResult > 0)
                        {
                            Debug.WriteLine("HISTORY RESULT SUCCESS");
                        }
                        else
                        {
                            Debug.WriteLine("HISTORY RESULT FAIL");
                        }
                        //});
                    }
                    catch
                    {
                        
                        Debug.WriteLine("sorryMessageCnt3 : " + sorryMessageCnt);
                        Activity reply_err = activity.CreateReply();
                        reply_err.Recipient = activity.From;
                        reply_err.Type = "message";
                        //reply_err.Text = "죄송해요3. 무슨 말인지 잘 모르겠어요. '" + (string)Luis["intents"][0]["intent"] + "','" + entitiesStr + "'";
                        reply_err.Text = SorryMessageList.GetSorryMessage(++sorryMessageCnt) + "[ '"+ (string)Luis["intents"][0]["intent"] + "','" + entitiesStr + "' ]";

                        var reply1 = await connector.Conversations.SendToConversationAsync(reply_err);

                        DateTime endTime = DateTime.Now;

                        Debug.WriteLine("USER NUMBER : " + activity.Conversation.Id);
                        Debug.WriteLine("CUSTOMMER COMMENT KOREAN : " + activity.Text);
                        Debug.WriteLine("CUSTOMMER COMMENT ENGLISH : " + translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));
                        //Debug.WriteLine("CHATBOT_COMMENT_CODE : " + dlg[0].dlgNm);
                        Debug.WriteLine("CHANNEL_ID : " + activity.ChannelId);
                        Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - startTime).Milliseconds));

                        inserResult = db.insertHistory(activity.Conversation.Id, activity.Text, translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), "ERROR", activity.ChannelId, ((endTime - startTime).Milliseconds));
                        if (inserResult > 0)
                        {
                            Debug.WriteLine("HISTORY RESULT SUCCESS");
                        }
                        else
                        {
                            Debug.WriteLine("HISTORY RESULT FAIL");
                        }


                        response = Request.CreateResponse(HttpStatusCode.OK);
                        return response;
                    }
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }

            response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        private static async Task<JObject> GetIntentFromOpenStartLUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            JObject jsonObj = new JObject();
            using (HttpClient client = new HttpClient())
            {
                string RequestURI = "https://api.projectoxford.ai/luis/v1/application?id=bb66fe0d-2993-4df5-98e8-bf12c8791204&subscription-key=7efb093087dd48918b903885b944740c&q=" + Query;
                HttpResponseMessage msg = await client.GetAsync(RequestURI);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    jsonObj = JObject.Parse(JsonDataResponse);
                }
                msg.Dispose();
            }
            return jsonObj;
        }

        private static async Task<JObject> GetIntentFromClose1LUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            JObject jsonObj = new JObject();
            using (HttpClient client = new HttpClient())
            {
                //string RequestURI = "https://api.projectoxford.ai/luis/v1/application?id=9242e200-9c04-41d8-8229-8a464fc703ec&subscription-key=7489b95cf3fb4797939ea70ce94a4b11&q=" + Query;
                //string RequestURI = "https://api.projectoxford.ai/luis/v1/application?id=3b07d950-ba77-46f4-a930-1684a120d7e8&subscription-key=7489b95cf3fb4797939ea70ce94a4b11&q=" + Query; ;
                string RequestURI = "https://api.projectoxford.ai/luis/v1/application?id=b1437ec6-3301-4c24-8bcb-1af58ee2c47c&subscription-key=7efb093087dd48918b903885b944740c&q=" + Query;
                //https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/53e1f554-98aa-4afe-a56f-c61f66bc2e62?subscription-key=c20ded166bd94a859ad30a695c966922&timezoneOffset=0&verbose=true&q=

                HttpResponseMessage msg = await client.GetAsync(RequestURI);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    //Debug.WriteLine("JsonDataResponse : " + JsonDataResponse);
                    jsonObj = JObject.Parse(JsonDataResponse);
                }
                msg.Dispose();
            }
            return jsonObj;
        }

        private static async Task<JObject> GetIntentFromClose2LUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            JObject jsonObj = new JObject();
            using (HttpClient client = new HttpClient())
            {
                string RequestURI = "https://api.projectoxford.ai/luis/v1/application?id=3d64c832-4f91-43f8-af8e-bccbdc395584&subscription-key=7efb093087dd48918b903885b944740c&q=" + Query;
                HttpResponseMessage msg = await client.GetAsync(RequestURI);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    jsonObj = JObject.Parse(JsonDataResponse);
                }
                msg.Dispose();
            }
            return jsonObj;
        }

        private static async Task<JObject> GetIntentFromTestDriveLUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            JObject jsonObj = new JObject();
            using (HttpClient client = new HttpClient())
            {
                //string RequestURI = "https://api.projectoxford.ai/luis/v1/application?id=b8510f44-ea01-43c7-8c8f-b3d4f448af0f&subscription-key=7efb093087dd48918b903885b944740c&q=" + Query;
                string RequestURI = "https://api.projectoxford.ai/luis/v1/application?id=e263ae53-2a07-4949-9bec-8b8cfb54dbd0&subscription-key=7efb093087dd48918b903885b944740c&q=" + Query;
                //
                HttpResponseMessage msg = await client.GetAsync(RequestURI);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    jsonObj = JObject.Parse(JsonDataResponse);
                }
                msg.Dispose();
            }
            return jsonObj;
        }
        
        private static async Task<JObject> GetIntentFromOpen1LUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            JObject jsonObj = new JObject();
            using (HttpClient client = new HttpClient())
            {
                string RequestURI = "https://api.projectoxford.ai/luis/v1/application?id=3e3dd340-893a-47f2-8844-ebde72b07394&subscription-key=7efb093087dd48918b903885b944740c&q=" + Query;
                HttpResponseMessage msg = await client.GetAsync(RequestURI);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    jsonObj = JObject.Parse(JsonDataResponse);
                }
                msg.Dispose();
            }
            return jsonObj;
        }


        
        private static async Task<JObject> GetIntentFromPersonalLUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            JObject jsonObj = new JObject();
            using (HttpClient client = new HttpClient())
            {
                string RequestURI = "https://api.projectoxford.ai/luis/v1/application?id=aa8cbee5-8ce6-4939-bf6d-9ec4fc3a8336&subscription-key=7489b95cf3fb4797939ea70ce94a4b11&q=" + Query;
                HttpResponseMessage msg = await client.GetAsync(RequestURI);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    jsonObj = JObject.Parse(JsonDataResponse);
                }
                msg.Dispose();
            }
            return jsonObj;
        }

        private static async Task<WeatherInfo> GetWeatherInfo()

        {
            WeatherInfo weather = new WeatherInfo();
            Debug.WriteLine("1");


            using (HttpClient client = new HttpClient())
            {
                string appId = "0221b2d1d8edb99a011cd7a3f152b756";

                string url = string.Format("http://api.openweathermap.org/data/2.5/forecast/daily?q={0}&units=metric&cnt=1&APPID={1}", "Seoul", appId);

                Debug.WriteLine("2");

                HttpResponseMessage msg = await client.GetAsync(url);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    weather = JsonConvert.DeserializeObject<WeatherInfo>(JsonDataResponse);
                }
                return weather;

                //Debug.WriteLine("Weather Info :::::::::: " + weatherInfo.city.name + "," + weatherInfo.city.country);
                //Debug.WriteLine("Weather Info :::::::::: " + weatherInfo.list[0].weather[0].description);
                //Debug.WriteLine("Weather Info :::::::::: " + string.Format("{0}°С", Math.Round(weatherInfo.list[0].temp.min, 1)));
                //Debug.WriteLine("Weather Info :::::::::: " + string.Format("{0}°С", Math.Round(weatherInfo.list[0].temp.max, 1)));
                //Debug.WriteLine("Weather Info :::::::::: " + string.Format("{0}°С", Math.Round(weatherInfo.list[0].temp.day, 1)));
                //Debug.WriteLine("Weather Info :::::::::: " + weatherInfo.list[0].humidity.ToString());
                //lblCity_Country.Text = weatherInfo.city.name + "," + weatherInfo.city.country;

                //imgCountryFlag.ImageUrl = string.Format("http://openweathermap.org/images/flags/{0}.png", weatherInfo.city.country.ToLower());

                //lblDescription.Text = weatherInfo.list[0].weather[0].description;

                //imgWeatherIcon.ImageUrl = string.Format("http://openweathermap.org/img/w/{0}.png", weatherInfo.list[0].weather[0].icon);

                //lblTempMin.Text = string.Format("{0}°С", Math.Round(weatherInfo.list[0].temp.min, 1));

                //lblTempMax.Text = string.Format("{0}°С", Math.Round(weatherInfo.list[0].temp.max, 1));

                //lblTempDay.Text = string.Format("{0}°С", Math.Round(weatherInfo.list[0].temp.day, 1));

                //lblTempNight.Text = string.Format("{0}°С", Math.Round(weatherInfo.list[0].temp.night, 1));

                //lblHumidity.Text = weatherInfo.list[0].humidity.ToString();

                //tblWeather.Visible = true;
            }
        }


        private static async Task<Translator> getTranslate(string input)
        {
            Translator trans = new Translator();

            using (HttpClient client = new HttpClient())
            {
                string appId = "AIzaSyDr4CH9BVfENdM9uoSK0fANFVWD0gGXlJM";

                string url = string.Format("https://translation.googleapis.com/language/translate/v2/?key={0}&q={1}&source=ko&target=en&model=nmt", appId, input);

                Debug.WriteLine("2");

                HttpResponseMessage msg = await client.GetAsync(url);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    trans = JsonConvert.DeserializeObject<Translator>(JsonDataResponse);
                }
                return trans;
            }

        }


        public bool isContainHangul(string s)
        {
            char[] charArr = s.ToCharArray();
            foreach (char c in charArr)
            {
                if (char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherLetter)
                {
                    return true;
                }
            }
            return false;
        }

        //herocard 추가
        //private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
        private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction1, CardAction cardAction2, CardAction cardAction3)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                /*Images = new List<CardImage>() { cardImage },*/
                Buttons = new List<CardAction>() { cardAction1, cardAction2, cardAction3 },
            };
            return heroCard.ToAttachment();
        }
        
        private static Attachment GetHeroCard_info(string title, string subtitle, string text, CardImage cardImage)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                //Buttons = new List<CardAction>() { cardAction1, cardAction2 },
            };
            return heroCard.ToAttachment();
        }

        private static Attachment GetHeroCard_tap(string title, string subtitle, string text/*, CardImage cardImage*/, CardAction tap)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                //Images = new List<CardImage>() { cardImage },
                Tap = tap,
                //Buttons = new List<CardAction>() { cardAction1, cardAction2 },
            };
            return heroCard.ToAttachment();
        }

        private static Attachment GetHeroCard_tap1(string title, string subtitle, string text, CardImage cardImage, CardAction tap)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Tap = tap,
                //Buttons = new List<CardAction>() { cardAction1, cardAction2 },
            };
            return heroCard.ToAttachment();
        }

        private static Attachment GetHeroCard_show(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = new List<CardAction>() { cardAction },
            };
            return heroCard.ToAttachment();
        }

        private static Attachment GetHeroCard_trim(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction1, CardAction cardAction2, CardAction cardAction3, CardAction cardAction4)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = new List<CardAction>() { cardAction1, cardAction2, cardAction3, cardAction4 },
            };
            return heroCard.ToAttachment();
        }

        private static Attachment GetHeroCard_option(string title, string subtitle, string text)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
            };
            return heroCard.ToAttachment();
        }

		private static Attachment GetHeroCard_location(string title, string subtitle, string text, CardAction cardAction)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Buttons = new List<CardAction>() { cardAction },
            };
            return heroCard.ToAttachment();
        }
        private static Attachment UserGetHeroCard_location(string title, string subtitle, string text, CardImage cardImage, string latitude, string longitude)
        {
            var userheroCard = new UserHeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Latitude = latitude,
                Longitude = longitude,
            };
            return userheroCard.ToAttachment();
        }

    }
}