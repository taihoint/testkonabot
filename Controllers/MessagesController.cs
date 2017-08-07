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
using Microsoft.Bot.Builder.Dialogs;
using Bot_Application1.Dialogs;
using System.IO;

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
        public static string filePath = "";

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
				HistoryLog("[logic start] ==>> userID :: ["+ activity.Conversation.Id + "]" );

                JObject Luis = new JObject();
                string entitiesStr = "";
                string testDriveWhereStr = "";
                string priceWhereStr = "";

                string entitiesValueStr = "";
                string colorStr = "";
                string carOptionStr = "";
                string luis_intent = "";
                string gubunVal = "";

                int luisID = 0;

                // Db
                DbConnect db = new DbConnect();

                List<CarQouteLuisResult> CarQouteLuisValue = new List<CarQouteLuisResult>();
                List<LuisResult> LuisValue = new List<LuisResult>();

                int inserResult;
                string orgMent = "";
                string orgKRMent = "";
                string orgKRMent1 = "";
                string orgENGMent = "";
                DateTime startTime = DateTime.Now;
                long unixTime = ((DateTimeOffset)startTime).ToUnixTimeSeconds();
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                orgMent = activity.Text;

                string bannedAnswer = "";

                bannedAnswer = db.SelectBannedWordAnswerMsg(orgMent);
                bannedAnswer = Regex.Split(bannedAnswer, "@@")[0];

                Translator translateInfo = await getTranslate(orgMent);
				HistoryLog("[bannedword check end] ==>> userID :: ["+ activity.Conversation.Id + "]" );
                if (bannedAnswer != "")
                {
                    {
                        Activity reply_err = activity.CreateReply();
                        reply_err.Recipient = activity.From;
                        reply_err.Type = "message";
                        reply_err.Text = bannedAnswer;
                        var reply1 = await connector.Conversations.SendToConversationAsync(reply_err);
                    }

                    response = Request.CreateResponse(HttpStatusCode.OK);
                    sorryMessageCnt = 0;
                    return response;


                }


                else if (orgMent.Contains(" 트림 외장색상"))
                //else if (orgMent.Substring(orgMent.Length - 8).Equals(" 트림 외장색상"))
                {
                    Debug.WriteLine("외장컬러 보여주자");

                    orgMent = orgMent.Replace(" 트림 외장색상", "");

                    Debug.WriteLine("orgMent : " + orgMent);

                    List<CarExColorList> CarExColorList = db.SelectCarExColorList(orgMent);
                    //exColor = CarExColorList[0].model.ToString();
                    //Debug.WriteLine("CarExColorList.Count : " + CarExColorList.Count);

                    Activity reply_ment = activity.CreateReply();
                    reply_ment.Recipient = activity.From;
                    reply_ment.Type = "message";
                    reply_ment.Text = "코나 " + orgMent.Substring(0, orgMent.Length - 1) + "의 외장색상을 보여드릴게요";
                    var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);


                    Activity reply_exColor = activity.CreateReply();
                    reply_exColor.Recipient = activity.From;
                    reply_exColor.Type = "message";
                    reply_exColor.Attachments = new List<Attachment>();
                    reply_exColor.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    for (int td = 0; td < CarExColorList.Count; td++)
                    {

                        string trimNM = CarExColorList[td].trimColorNm;

                        trimNM = trimNM.Replace("코나 ", "");
                        trimNM = trimNM.Replace("1.6 ", "");
                        trimNM = trimNM.Replace("터보 ", "");
                        trimNM = trimNM.Replace("오토 ", "");

                        reply_exColor.Attachments.Add(
                        GetHeroCard_info(
                            trimNM,
                            //CarExColorList[td].trimColorNm,
                            "추가 금액 : " + string.Format("{0}", CarExColorList[td].exColorPrice.ToString("n0")) + "원",
                            "",
                            //new CardImage(url: "D:\\bob\\01.project\\05.현대자동차 ChatBot\\01.자료\\20170710\\가격 이미지\\가격 이미지\\exterior\\" + CarExColorList[td].trimColorCd + ".jpg"),
                            new CardImage(url: "https://bottest.hyundai.com/assets/images/price/exterior/" + CarExColorList[td].trimColorCd + ".jpg"), "turn", CarExColorList[td].trimColorCd)//,
                            //new CardAction(ActionTypes.OpenUrl, "견적 바로가기", value: "https://bottest.hyundai.com/assets/images/price/exterior/" + CarExColorList[td].trimColorCd + ".jpg"),"turn", CarExColorList[td].trimColorCd)

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
                    sorryMessageCnt = 0;
                    return response;
                }
                else if (orgMent.Contains(" 트림 내장색상"))
                //else if (orgMent.Substring(orgMent.Length - 8).Equals(" 트림 내장색상"))
                {
                    Debug.WriteLine("내장색상 보여주자");

                    orgMent = orgMent.Replace(" 트림 내장색상", "");

                    Debug.WriteLine("orgMent : " + orgMent);

                    string inColor = "";

                    List<CarInColorList> CarInColorList = db.SelectCarInColorList(orgMent);

                    Activity reply_ment = activity.CreateReply();
                    reply_ment.Recipient = activity.From;
                    reply_ment.Type = "message";
                    reply_ment.Text = "코나 " + orgMent + "의 내장색상을 보여드릴게요";
                    var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                    Activity reply_inColor = activity.CreateReply();
                    reply_inColor.Recipient = activity.From;
                    reply_inColor.Type = "message";
                    reply_inColor.Attachments = new List<Attachment>();
                    reply_inColor.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    for (int td = 0; td < CarInColorList.Count; td++)
                    {
                        string trimNM = CarInColorList[td].internalColorNm;

                        trimNM = trimNM.Replace("코나 ", "");
                        trimNM = trimNM.Replace("1.6 ", "");
                        trimNM = trimNM.Replace("터보 ", "");
                        trimNM = trimNM.Replace("오토 ", "");

                        reply_inColor.Attachments.Add(
                        GetHeroCard_info(
                            trimNM,
                            //CarInColorList[td].internalColorNm,
                            "추가 금액 : " + string.Format("{0}", CarInColorList[td].inColorPrice.ToString("n0")) + "원",
                            "",
                            //new CardImage(url: "D:\\bob\\01.project\\05.현대자동차 ChatBot\\01.자료\\20170710\\가격 이미지\\가격 이미지\\interior\\" + CarInColorList[td].internalColorCd + ".jpg"),
                            new CardImage(url: "https://bottest.hyundai.com/assets/images/price/interior/" + CarInColorList[td].internalColorCd + ".jpg"), "", "")//,
                                                                                                                                                                  //new CardAction(ActionTypes.OpenUrl, "견적 바로가기", value: "https://bottest.hyundai.com/assets/images/price/interior/" + CarInColorList[td].internalColorCd + ".jpg"), "", "")
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
                    sorryMessageCnt = 0;
                    return response;
                }
                else if (orgMent.Contains(" 트림 옵션보기"))
                {
                    Debug.WriteLine("옵션 보여주자");

                    orgMent = orgMent.Replace(" 트림 옵션보기", "");

                    Debug.WriteLine("orgMent : " + orgMent);

                    List<CarOptionList> CarOptionList = db.SelectCarOptionList(orgMent);

                    Activity reply_ment = activity.CreateReply();
                    reply_ment.Recipient = activity.From;
                    reply_ment.Type = "message";
                    reply_ment.Text = "코나 " + CarOptionList[0].model.Replace(" ", "") + "의 추가옵션을 보여드릴게요";
                    var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                    Activity reply_option = activity.CreateReply();
                    reply_option.Recipient = activity.From;
                    reply_option.Type = "message";
                    reply_option.Attachments = new List<Attachment>();
                    reply_option.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    for (int td = 0; td < CarOptionList.Count; td++)
                    {

                        string trimNM = CarOptionList[td].optNm;

                        trimNM = trimNM.Replace("코나 ", "");
                        trimNM = trimNM.Replace("1.6 ", "");
                        trimNM = trimNM.Replace("터보 ", "");
                        trimNM = trimNM.Replace("오토 ", "");

                        //Debug.WriteLine("CarOptionList[td].optNm : " + CarOptionList[td].optNm);

                        translateInfo = await getTranslate(CarOptionList[td].optNm);

                        //Debug.WriteLine("CarOptionList[td].optNm : translate " + translateInfo.data.translations[0].translatedText);

                        reply_option.Attachments.Add(
                        GetHeroCard_info(
                            trimNM,
                            //CarOptionList[td].optNm,
                            "추가 금액 : " + string.Format("{0}", CarOptionList[td].optPrice.ToString("n0")) + "원",
                            "",
                            //new CardImage(url: "D:\\bob\\01.project\\05.현대자동차 ChatBot\\01.자료\\20170710\\가격 이미지\\가격 이미지\\option\\" + translateInfo.data.translations[0].translatedText + ".jpg"),
                            new CardImage(url: "https://bottest.hyundai.com/assets/images/price/option/" + translateInfo.data.translations[0].translatedText.Replace(" ", "_") + ".jpg"), "", "")
                        //new CardAction(ActionTypes.OpenUrl, "견적 바로가기", value: "https://bottest.hyundai.com/assets/images/price/option/" + translateInfo.data.translations[0].translatedText.Replace(" ", "_") + ".jpg"), "", "")
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
                    sorryMessageCnt = 0;
                    return response;
                }
                else if (orgMent.Contains(" 트림"))
                //else if (orgMent.Substring(orgMent.Length - 2).Equals("트림"))
                {
                    string color = "";

                    Debug.WriteLine("트림 보여주자");

                    orgMent = orgMent.Replace(" 트림", "");

                    Debug.WriteLine("orgMent : " + orgMent);

                    string trimNM = orgMent;
                    trimNM = trimNM.Replace("코나 ", "");
                    trimNM = trimNM.Replace("1.6 ", "");
                    trimNM = trimNM.Replace("터보 ", "");
                    trimNM = trimNM.Replace("오토 ", "");


                    Activity reply_ment = activity.CreateReply();
                    reply_ment.Recipient = activity.From;
                    reply_ment.Type = "message";
                    reply_ment.Text = "코나 " + orgMent + "의 외장색상, 내장색상, 옵션을 보여 드릴게요,상세 견적은 견적 바로가기를 눌러주세요";
                    var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                    Activity reply_trim_only = activity.CreateReply();
                    reply_trim_only.Recipient = activity.From;
                    reply_trim_only.Type = "message";
                    reply_trim_only.Attachments = new List<Attachment>();
                    reply_trim_only.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    color = db.SelectOnlyTrimColor(orgMent);
                    Debug.WriteLine("트림 보여줄래 color : " + color);
                    reply_trim_only.Attachments.Add(
                    GetHeroCard_trim(
                        trimNM,
                        //orgMent,
                        "",
                        "",
                        //new CardImage(url: "D:\\bob\\01.project\\05.현대자동차 ChatBot\\01.자료\\20170710\\가격 이미지\\가격 이미지\\trim\\" + color.Replace(" ", "") + ".jpg"),
                        new CardImage(url: "https://bottest.hyundai.com/assets/images/price/trim/" + color.Replace(" ", "") + ".jpg"),
                        new CardAction(ActionTypes.ImBack, "외장색상", value: orgMent + " 트림 외장색상"),
                        new CardAction(ActionTypes.ImBack, "내장색상", value: orgMent + " 트림 내장색상"),
                        new CardAction(ActionTypes.ImBack, "옵션보기", value: orgMent + " 트림 옵션보기"),
                        new CardAction(ActionTypes.OpenUrl, "견적 바로가기", value: "https://logon.hyundai.com/kr/quotation/main.do?carcode=RV104"))
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
                    sorryMessageCnt = 0;
                    return response;
                }
                else
                {
                    //for (int n = 0; n < Regex.Split(orgMent, " ").Length; n++)
                    //{
                    //    string chgMsg = db.SelectChgMsg(Regex.Split(orgMent, " ")[n]);
                    //    if (!string.IsNullOrEmpty(chgMsg))
                    //    {
                    //        orgMent = orgMent.Replace(Regex.Split(orgMent, " ")[n], chgMsg);
                    //    }
                    //}

                    orgKRMent = "";
                    orgENGMent = "";

                    Debug.WriteLine("orgMentorgMentorgMent : " + orgMent);
                    orgMent = orgMent.Replace("&#39;", "/'");
                    Debug.WriteLine("orgMent : " + orgMent);
                    translateInfo = await getTranslate(orgMent);
                    orgKRMent = Regex.Replace(orgMent, @"[^a-zA-Z0-9가-힣]", "", RegexOptions.Singleline);

                    HistoryLog("[change msg end] ==>> userID :: ["+ activity.Conversation.Id + "]" );

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // 한글 , 영어 질문 cash table 체크

                    if(db.SelectKoreanCashCheck(orgKRMent).Length == 0)
                    {

                        for (int n = 0; n < Regex.Split(orgMent, " ").Length; n++)
                        {
                            string chgMsg = db.SelectChgMsg(Regex.Split(orgMent, " ")[n]);
                            if (!string.IsNullOrEmpty(chgMsg))
                            {
                                orgMent = orgMent.Replace(Regex.Split(orgMent, " ")[n], chgMsg);
                            }
                        }


                        orgKRMent1 = Regex.Replace(orgMent, @"[^a-zA-Z0-9가-힣-\s]", "", RegexOptions.Singleline);

                        translateInfo = await getTranslate(orgKRMent1);

                        //translateInfo = await getTranslateJP(orgKRMent1);

                        //translateInfo = await getTranslate(translateInfo.data.translations[0].translatedText);

                        orgENGMent = Regex.Replace(translateInfo.data.translations[0].translatedText, @"[^a-zA-Z0-9가-힣-\s]", "", RegexOptions.Singleline);

                        if(db.SelectEnglishCashCheck(orgENGMent).Length > 0)
                        {
                            //translateInfo = await getTranslate(orgMent);
                            Debug.WriteLine("!!!!!!!!!!!!!! : " + translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));
                            
                            HistoryLog("[cash check true] ==>> userID :: ["+ activity.Conversation.Id + "]" );

                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            // For Query Analysis
                            luisID = 0;

                            // Try to find dialogue from log history first before checking LUIS
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            //Luis = db.SelectQueryAnalysis(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));
                            Luis = db.SelectQueryAnalysis(orgENGMent);
                            entitiesStr = (string)Luis["entities"];

                            testDriveWhereStr = (string)Luis["test_driveWhere"];
                            priceWhereStr = (string)Luis["car_priceWhere"];

                            entitiesValueStr = (string)Luis["test_driveWhere"];

                            colorStr = (string)Luis["car_color"];
                            carOptionStr = (string)Luis["car_option"];
                            luis_intent = (string)Luis["intents"][0]["intent"];
                            gubunVal = (string)Luis["car_option"];
                            //string gubunVal = "";
                            //string entitiesValueStr = "";
                        }
                        else 
                        {
                            //translateInfo = await getTranslate(orgMent);
                            Debug.WriteLine("!!!!!!!!!!!!!! : " + translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));


                            HistoryLog("[cash check false] ==>> userID :: ["+ activity.Conversation.Id + "]" );

                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            // For Query Analysis
                            luisID = 0;

                            // Try to find dialogue from log history first before checking LUIS
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            //Luis = db.SelectQueryAnalysis(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));

                            Luis = db.SelectQueryAnalysis(orgENGMent);
                            entitiesStr = (string)Luis["entities"];

                            testDriveWhereStr = (string)Luis["test_driveWhere"];
                            priceWhereStr = (string)Luis["car_priceWhere"];

                            entitiesValueStr = (string)Luis["test_driveWhere"];

                            colorStr = (string)Luis["car_color"];
                            carOptionStr = (string)Luis["car_option"];
                            luis_intent = (string)Luis["intents"][0]["intent"];
                            gubunVal = (string)Luis["car_option"];
                            //string gubunVal = "";
                            //string entitiesValueStr = "";
                        }

                    }else
                    {
                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // For Query Analysis
                        luisID = 0;

                        // Try to find dialogue from log history first before checking LUIS
                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        //Luis = db.SelectQueryAnalysis(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));

                        //for (int n = 0; n < Regex.Split(orgMent, " ").Length; n++)
                        //{
                        //    string chgMsg = db.SelectChgMsg(Regex.Split(orgMent, " ")[n]);
                        //    if (!string.IsNullOrEmpty(chgMsg))
                        //    {
                        //        orgMent = orgMent.Replace(Regex.Split(orgMent, " ")[n], chgMsg);
                        //    }
                        //}


                        //orgKRMent1 = Regex.Replace(orgMent, @"[^a-zA-Z0-9가-힣-\s]", "", RegexOptions.Singleline);

                        //translateInfo = await getTranslate(orgKRMent1);

                        ////translateInfo = await getTranslateJP(orgKRMent1);

                        ////translateInfo = await getTranslate(translateInfo.data.translations[0].translatedText);

                        //orgENGMent = Regex.Replace(translateInfo.data.translations[0].translatedText, @"[^a-zA-Z0-9가-힣-\s]", "", RegexOptions.Singleline);

                        orgENGMent = db.SelectKoreanCashCheck(orgKRMent);

                        Luis = db.SelectQueryAnalysis(orgENGMent);
                        entitiesStr = (string)Luis["entities"];

                        testDriveWhereStr = (string)Luis["test_driveWhere"];
                        priceWhereStr = (string)Luis["car_priceWhere"];

                        entitiesValueStr = (string)Luis["test_driveWhere"];

                        colorStr = (string)Luis["car_color"];
                        carOptionStr = (string)Luis["car_option"];
                        luis_intent = (string)Luis["intents"][0]["intent"];
                        gubunVal = (string)Luis["car_option"];
                        //string gubunVal = "";
                        //string entitiesValueStr = "";
                    }

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // For Query Analysis
                    // No results from DB
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //if (string.IsNullOrEmpty(entitiesStr) && string.IsNullOrEmpty((string)Luis["intents"][0]["intent"]) || ((string)Luis["intents"][0]["intent"]).Equals("car quote") || ((string)Luis["intents"][0]["intent"]).Equals("car option") || ((string)Luis["intents"][0]["intent"]).Equals("show color"))
                    if (string.IsNullOrEmpty(entitiesStr) && string.IsNullOrEmpty((string)Luis["intents"][0]["intent"]))
                    {

                        for (int n = 0; n < Regex.Split(orgMent, " ").Length; n++)
                        {
                            string chgMsg = db.SelectChgMsg(Regex.Split(orgMent, " ")[n]);
                            if (!string.IsNullOrEmpty(chgMsg))
                            {
                                orgMent = orgMent.Replace(Regex.Split(orgMent, " ")[n], chgMsg);
                            }
                        }

                        translateInfo = await getTranslate(orgMent); 

                        Luis = await GetIntentFromKonaBotLUIS(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));
                        luisID = 1; //Query Analysis
                                    //Debug.WriteLine("Luis.entities.Length : " + Luis.entities.Length);

						HistoryLog("[luis request end] ==>> userID :: ["+ activity.Conversation.Id + "]" );

                        //luis 분기
                        try
                        {
                            Debug.WriteLine("score : " + (float)Luis["intents"][0]["score"]);
                            Debug.WriteLine("score : " + Luis["entities"].Count());

                            LuisValue = db.LuisResult(Luis.ToString());

                            gubunVal    = LuisValue[0].val;
                            luis_intent = LuisValue[0].intentValue;
                            entitiesStr = LuisValue[0].entityValue;
                            entitiesValueStr = LuisValue[0].entitieWhereValue;
                            testDriveWhereStr = LuisValue[0].testDriveWhereValue;
                            priceWhereStr = LuisValue[0].carPriceWhereValue;

                            HistoryLog("[luis result db end] ==>> userID :: ["+ activity.Conversation.Id + "]" );

                            /*
                            if ((string)Luis["intents"][0]["intent"] == "Quote")
                            {
                                CarQouteLuisValue = db.SelectCarQouteLuisResult(Luis.ToString());

								HistoryLog("[car qoute db end] ==>> userID :: ["+ activity.Conversation.Id + "]" );

                                Debug.WriteLine("CarQouteLuis INTENT : " + CarQouteLuisValue[0].intentValue);
                                Debug.WriteLine("CarQouteLuis ENTITY : " + CarQouteLuisValue[0].entityValue);
                                Debug.WriteLine("CarQouteLuis WHERE : " + CarQouteLuisValue[0].whereValue);

                                luis_intent = CarQouteLuisValue[0].intentValue;
                                entitiesStr = CarQouteLuisValue[0].entityValue;
                                entitiesValueStr = CarQouteLuisValue[0].whereValue;

                                entitiesValueStr = entitiesValueStr.Replace("pricing", "price");
                                entitiesValueStr = entitiesValueStr.Replace("premium special", "premium s");

                            }
                            else if ((string)Luis["intents"][0]["intent"] == "Test drive" || (string)Luis["intents"][0]["intent"] == "Branch" || (string)Luis["intents"][0]["intent"] == "Test drive car color")
                            {
                                List<TestDriveLuisResult> SelectTestDriveLuisResult = db.SelectTestDriveLuisResult(Luis.ToString());

								HistoryLog("[test drive db end] ==>> userID :: ["+ activity.Conversation.Id + "]" );

                                Debug.WriteLine("SelectLuisTestDirve = " + SelectTestDriveLuisResult.Count);
                                Debug.WriteLine("SelectTestDriveLuisResult[0].intent.ToString() = " + SelectTestDriveLuisResult[0].intent.ToString());
                                Debug.WriteLine("SelectTestDriveLuisResult[0].entity.ToString() = " + SelectTestDriveLuisResult[0].entity.ToString());
                                Debug.WriteLine("SelectTestDriveLuisResult[0].entity_value.ToString() = " + SelectTestDriveLuisResult[0].entity_value.ToString());

                                luis_intent = SelectTestDriveLuisResult[0].intent.ToString();
                                entitiesStr = SelectTestDriveLuisResult[0].entity.ToString();
                                entitiesValueStr = SelectTestDriveLuisResult[0].entity_value.ToString();
                                
                            }
                            else
                            {
                                CarQouteLuisValue = db.SelectLuisResult(Luis.ToString());

                                Debug.WriteLine("CarQouteLuis INTENT : " + CarQouteLuisValue[0].intentValue);
                                Debug.WriteLine("CarQouteLuis ENTITY : " + CarQouteLuisValue[0].entityValue);
                                //Debug.WriteLine("CarQouteLuis WHERE : " + CarQouteLuisValue[0].whereValue);

                                luis_intent = CarQouteLuisValue[0].intentValue;
                                entitiesStr = CarQouteLuisValue[0].entityValue;
                            }
                            */
                        }
                        catch (Exception ex)
                        {
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            // Query Analysis
                            // No LUIS result at all, so NLP failed completely
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            //sorryMessageCnt++;

                            await Conversation.SendAsync(activity, () => new RootDialog(luis_intent, entitiesStr, startTime, orgKRMent, orgENGMent));

                            //if (activity.Text.StartsWith("코나") == true)
                            //{
                            //    await Conversation.SendAsync(activity, () => new RootDialog());

                            //}
                            //else
                            //{
                            //    Debug.WriteLine("sorryMessageCnt2 : " + sorryMessageCnt);
                            //    int dbResult = db.insertUserQuery(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), "", "", 0, 'N', "", "", "", "");
                            //    Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());
                            //    Activity reply_err = activity.CreateReply();
                            //    reply_err.Recipient = activity.From;
                            //    reply_err.Type = "message";
                            //    //reply_err.Text = "죄송해요. 무슨 말인지 잘 모르겠어요..";
                            //    reply_err.Text = SorryMessageList.GetSorryMessage(++sorryMessageCnt);
                            //    var reply1 = await connector.Conversations.SendToConversationAsync(reply_err);


                            //}
                            response = Request.CreateResponse(HttpStatusCode.OK);
                            return response;
                        }
                        
                    }
                    try
                    {
                        List<DialogList> dlg = new List<DialogList>();

                        List<LuisList> LuisDialogID = db.SelectLuis(luis_intent, entitiesStr);

                        //Debug.WriteLine(LuisDialogID[0].dlgId + " LuisDialogID count : " + LuisDialogID.Count);
                        //컬러별 차량이 전시된 매장을 알려줘. showroom에서 car color로 변경되어 , -> . 으로 변경
                        //entitiesStr = (string)entitiesStr.Replace(".", ",");

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
                        String addressStr = "";
                        Activity replyToConversation = activity.CreateReply();

                        for (int k = 0; k < LuisDialogID.Count; k++)
                        {
                            replyToConversation.Recipient = activity.From;
                            replyToConversation.Type = "message";
                            replyToConversation.Attachments = new List<Attachment>();
                            replyToConversation.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                            dlg = db.SelectDialog(LuisDialogID[k].dlgId);

                            //if (dlg.Count > 0)
                            //{
                            //    Debug.WriteLine("dlg[0].dlgMent : [" + dlg[0].dlgMent + "]");
                            //    Debug.WriteLine("dlg[0].dlgMent : [" + string.IsNullOrEmpty(dlg[0].dlgMent) + "]");
                            //    if (string.IsNullOrEmpty(dlg[0].dlgMent) != true)
                            //    {
                            //        Activity reply = activity.CreateReply(dlg[0].dlgMent.ToString());
                            //        await connector.Conversations.ReplyToActivityAsync(reply);
                            //    }
                            //}

                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            // 시승 로직
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            if ((gubunVal.Equals("Test drive") )
                                && entitiesStr != "test drive" && !entitiesStr.Contains("reservation") && !entitiesStr.Contains("near")
                                && k < 1 )
                            {

                                if (dlg.Count > 0)
                                {
                                    Debug.WriteLine("dlg[0].dlgMent : [" + dlg[0].dlgMent + "]");
                                    Debug.WriteLine("dlg[0].dlgMent : [" + string.IsNullOrEmpty(dlg[0].dlgMent) + "]");
                                    if (string.IsNullOrEmpty(dlg[0].dlgMent) != true)
                                    {
                                        Activity reply = activity.CreateReply(dlg[0].dlgMent.ToString());
                                        await connector.Conversations.ReplyToActivityAsync(reply);
                                    }
                                }


                                if (entitiesStr.Contains("current location"))
                                {
                                    //int position;
                                    Geolocation.getRegion();
                                    if (testDriveWhereStr.Contains("test drive center region"))
                                    {
                                        //position = entitiesValueStr.IndexOf(",");
                                        //entitiesValueStr = entitiesValueStr.Substring(position, (int)entitiesValueStr.Length);
                                        //entitiesValueStr = "test drive center region=" + Geolocation.ll.regionName.ToLower().ToString() + "," + entitiesValueStr;
                                        testDriveWhereStr = "test drive center region=seoul,current location=current location,query=Approve your current location";
                                    }
                                    else
                                    {
                                        testDriveWhereStr = "test drive center region=" + Geolocation.ll.regionName.ToLower().ToString() + "," + testDriveWhereStr;
                                    }
                                }

                                List<TestDriveList> SelectTestDriveList = db.SelectTestDriveList(testDriveWhereStr);

                                if (SelectTestDriveList.Count == 0)
                                {
                                    //Activity reply_err = activity.CreateReply();
                                    //reply_err.Recipient = activity.From;
                                    //reply_err.Type = "message";
                                    ////reply_err.Text = SorryMessageList.GetSorryMessage(++sorryMessageCnt) + "[ '" +gubunVal+ "','" + entitiesStr + "' ]";
                                    //reply_err.Text = SorryMessageList.GetSorryMessage(++sorryMessageCnt) + "[ '" + luis_intent + "','" + entitiesStr + "' ]";
                                    //await connector.Conversations.SendToConversationAsync(reply_err);

                                    await Conversation.SendAsync(activity, () => new RootDialog(luis_intent, entitiesStr, startTime, orgKRMent, orgENGMent));

                                    response = Request.CreateResponse(HttpStatusCode.OK);
                                    return response;
                                }

                                if (SelectTestDriveList[0].dlgGubun.Equals("1"))
                                {
                                    // dlgStr1 = AREANM, dlgStr2 = AREALIST , dlgStr3 = AREACNT
                                    Debug.WriteLine("case 1");
                                    for (int td = 0; td < SelectTestDriveList.Count; td++)
                                    {
                                        replyToConversation.Attachments.Add(
                                        GetHeroCard_location(
                                        SelectTestDriveList[td].dlgStr1 + " 시승센터",
                                        "",
                                        SelectTestDriveList[td].dlgStr2 + " 등 총 " + SelectTestDriveList[td].dlgStr3 + " 곳",
                                        new CardAction(ActionTypes.ImBack, "정보보기", value: SelectTestDriveList[td].dlgStr1 + " 시승센터 "))
                                        );
                                    }
                                }
                                else if (SelectTestDriveList[0].dlgGubun.Equals("2"))
                                {
                                    // dlgStr1 = XRCL_CTY_NM, dlgStr2 = TRIMCOLOR_CD , dlgStr3 = CNT
                                    Debug.WriteLine("case 2");
                                    for (int td = 0; td < SelectTestDriveList.Count; td++)
                                    {
                                        replyToConversation.Attachments.Add(
                                        GetHeroCard_show(
                                        "",
                                        //CarColorListDialog[td].dlgXrclCtyNM,
                                        SelectTestDriveList[td].dlgStr1,
                                        SelectTestDriveList[td].dlgStr3 + "개 매장에 전시",
                                        new CardImage(url: "https://bottest.hyundai.com/assets/images/price/exterior/" + SelectTestDriveList[td].dlgStr2 + ".jpg"),
                                        //new CardAction(ActionTypes.ImBack, "전시 차량 보기", value: CarColorListDialog[td].dlgXrclCtyNM + " 컬러가 있는 매장을 알려줘"))
                                        new CardAction(ActionTypes.ImBack, "전시 매장 보기", value: SelectTestDriveList[td].dlgStr1 + " 컬러가 있는 매장"), "", "")
                                        );
                                    }
                                }
                                else if (SelectTestDriveList[0].dlgGubun.Equals("3"))
                                {
                                    Debug.WriteLine("case 3");

                                    if (testDriveWhereStr.Contains("test drive center address"))
                                    {
                                        Activity reply_ment = activity.CreateReply();
                                        reply_ment.Recipient = activity.From;
                                        reply_ment.Type = "message";
                                        reply_ment.Text = "주소는 ‘" + SelectTestDriveList[0].dlgStr2 + "'입니다.\n\n  맵으로 위치를 보시려면 아래 이미지를 선택해주세요.";
                                        var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                                        for (int td = 0; td < SelectTestDriveList.Count; td++)
                                        {
                                            APIExamMapGeocode.getCodeNaver(SelectTestDriveList[td].dlgStr4, SelectTestDriveList[td].dlgStr5);
                                            replyToConversation.Attachments.Add(
                                            UserGetHeroCard_location(
                                            SelectTestDriveList[td].dlgStr1 + " 시승센터",
                                            "TEL." + SelectTestDriveList[td].dlgStr3,
                                            "(연중무휴)10-16시까지 예약 가능" + " " + SelectTestDriveList[td].dlgStr2,
                                            //new CardImage(url: "http://www.smartsend.co.kr/map/" + APIExamMapGeocode.ll.lat.ToString() + "," + APIExamMapGeocode.ll.lon.ToString() + ".png"),
                                            new CardImage(url: "http://www.smartsend.co.kr/map/" + SelectTestDriveList[td].dlgStr4 + "," + SelectTestDriveList[td].dlgStr5 + ".png"),
                                            SelectTestDriveList[td].dlgStr4,
                                            SelectTestDriveList[td].dlgStr5));
                                        }
                                    }
                                    else if (testDriveWhereStr.Contains("test drive can") || testDriveWhereStr.Contains("test drive center"))
                                    {
                                        for (int td = 0; td < SelectTestDriveList.Count; td++)
                                        {
                                            replyToConversation.Attachments.Add(
                                            GetHeroCard_info(
                                            SelectTestDriveList[td].dlgStr4,
                                            SelectTestDriveList[td].dlgStr7,
                                            "",
                                            new CardImage(url: "https://bottest.hyundai.com/assets/images/price/exterior/" + SelectTestDriveList[td].dlgStr8 + ".jpg"), "", "")
                                            );
                                        }
                                    }
                                    {

                                    }

                                }
                                else if (SelectTestDriveList[0].dlgGubun.Equals("4"))
                                {
                                    // dlgStr1 = CTR_NM, dlgStr2 = CTR_ADDR , dlgStr3 = CTR_PHONE, dlgStr4 = CAR_DTL_INFO, dlgStr5 = MAP_X_TN, dlgStr6 = MAP_Y_TN, dlgStr7 = XRCL_CTY_NM, dlgStr8 = TRIMCOLOR_CD
                                    Debug.WriteLine("case 4");

                                    for (int td = 0; td < SelectTestDriveList.Count; td++)
                                    {
                                        replyToConversation.Attachments.Add(
                                        GetHeroCard(
                                        SelectTestDriveList[td].dlgStr1 + " 시승센터",
                                        "TEL." + SelectTestDriveList[td].dlgStr3,
                                        "(연중무휴)10-16시까지 예약 가능" + " " + SelectTestDriveList[td].dlgStr2,
                                        new CardAction(ActionTypes.ImBack, "전화하기", value: SelectTestDriveList[td].dlgStr3),
                                        new CardAction(ActionTypes.ImBack, "주소보기", value: SelectTestDriveList[td].dlgStr1 + " 시승센터 주소를 알려줘"),
                                        new CardAction(ActionTypes.ImBack, "시승 가능 차량 보기", value: SelectTestDriveList[td].dlgStr1 + " 시승센터에서 시승 가능한 차량"))
                                        );
                                    }
                                }
                                else if (SelectTestDriveList[0].dlgGubun.Equals("5"))
                                {
                                    Debug.WriteLine("case 5");
                                    //dlgStr1 = BR_NM, dlgStr2 = BR_ADDR, dlgStr3 = BR_CCPC
                                    for (int td = 0; td < SelectTestDriveList.Count; td++)
                                    {

                                        if (SelectTestDriveList[td].dlgStr2.Length > 40)
                                        {
                                            addressStr = SelectTestDriveList[td].dlgStr2.Substring(0, 37) + "...";
                                        }
                                        else
                                        {
                                            addressStr = SelectTestDriveList[td].dlgStr2;
                                        }
                                        replyToConversation.Attachments.Add(
                                        GetHeroCard_location(
                                        SelectTestDriveList[td].dlgStr1,
                                        SelectTestDriveList[td].dlgStr3,
                                        addressStr,
                                        new CardAction(ActionTypes.ImBack, "매장 보기", value: SelectTestDriveList[td].dlgStr1 + " 지점 보기"))
                                        );
                                    }
                                }
                                else if (SelectTestDriveList[0].dlgGubun.Equals("6"))
                                {
                                    //dlgStr1 = BR_DTL_ADDR1, dlgStr2 = XRCL_CTY_NM
                                    Debug.WriteLine("case 6");
                                    for (int td = 0; td < SelectTestDriveList.Count; td++)
                                    {
                                        replyToConversation.Attachments.Add(
                                        GetHeroCard_area(
                                        SelectTestDriveList[td].dlgStr1,
                                        "",
                                        SelectTestDriveList[td].dlgStr1 + " 지역, " + SelectTestDriveList[td].dlgStr2 + " 차량이 전시된 매장",
                                        new CardAction(ActionTypes.ImBack, "매장 보기", value: SelectTestDriveList[td].dlgStr1 + "에 " + SelectTestDriveList[td].dlgStr2 + " 컬러가 전시된 매장"))
                                        );
                                    }
                                }
                                else if (SelectTestDriveList[0].dlgGubun.Equals("7"))
                                {
                                    //branch info                                 
                                    Debug.WriteLine("case 7");
                                    /*
                                    for (int td = 0; td < SelectTestDriveList.Count; td++)
                                    {
                                        replyToConversation.Attachments.Add(
                                        GetHeroCard_location(
                                        SelectTestDriveList[td].dlgStr1,
                                        SelectTestDriveList[td].dlgStr3,
                                        SelectTestDriveList[td].dlgStr2,
                                        new CardAction())
                                        );
                                    }
                                    */
                                    //데이터가 없을 때 예외 처리
                                    if (SelectTestDriveList.Count == 0)
                                    {
                                        //Activity reply_err = activity.CreateReply();
                                        //reply_err.Recipient = activity.From;
                                        //reply_err.Type = "message";
                                        //reply_err.Text = SorryMessageList.GetSorryMessage(++sorryMessageCnt) + "[ '" +gubunVal+ "','' ]";
                                        //await connector.Conversations.SendToConversationAsync(reply_err);

                                        await Conversation.SendAsync(activity, () => new RootDialog(luis_intent, entitiesStr, startTime, orgKRMent, orgENGMent));

                                        response = Request.CreateResponse(HttpStatusCode.OK);
                                        return response;
                                    }
                                    else
                                    {
                                        // dlgStr1 = BR_NM, dlgStr2 = BR_ADDR , dlgStr3 = BR_CCPC, dlgStr4 = BR_XCOO, dlgStr5 = BR_YCOO

                                        for (int td = 0; td < SelectTestDriveList.Count; td++)
                                        {
                                            var urlImg = "https://openapi.naver.com/v1/map/staticmap.bin?clientId=OPCP0Yh0b2IC9r59XaTR&url=http://www.hyundai.com&crs=EPSG:4326&center=" + SelectTestDriveList[td].dlgStr4 + "," + SelectTestDriveList[td].dlgStr5 + "&level=12&w=400&h=300&baselayer=default&markers=" + SelectTestDriveList[td].dlgStr4 + "," + SelectTestDriveList[td].dlgStr5;
                                            String fileName = "c:/inetpub/wwwroot/map/" + SelectTestDriveList[td].dlgStr4 + "," + SelectTestDriveList[td].dlgStr5 + ".png";

                                            System.Net.WebClient client = new System.Net.WebClient();
                                            client.DownloadFile(urlImg, fileName);

                                            if (SelectTestDriveList[td].dlgStr2.Length > 40)
                                            {
                                                addressStr = SelectTestDriveList[td].dlgStr2.Substring(0, 37) + "...";
                                            }
                                            else
                                            {
                                                addressStr = SelectTestDriveList[td].dlgStr2;
                                            }

                                            replyToConversation.Attachments.Add(
                                            UserGetHeroCard_location(
                                            SelectTestDriveList[td].dlgStr1,
                                            "TEL." + SelectTestDriveList[td].dlgStr3,
                                            SelectTestDriveList[td].dlgStr2,
                                            new CardImage(url: "http://www.smartsend.co.kr/map/" + SelectTestDriveList[td].dlgStr4 + "," + SelectTestDriveList[td].dlgStr5 + ".png"),
                                            SelectTestDriveList[td].dlgStr4,
                                            SelectTestDriveList[td].dlgStr5)
                                            );
                                        }

                                        //await connector.Conversations.SendToConversationAsync(reply_reset);

                                    }

                                    //Luis["intents"][0]["intent"] = "";
                                    
                                }
                                luis_intent = (string)Luis["intents"][0]["intent"];
                                //gubunVal = "";
                            }
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            // 견적 로직
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            else if (gubunVal.Equals("Quote"))
                            {
                                Debug.WriteLine("INTENT ::[ " + luis_intent + " ]    ENTITY ::[ " + entitiesStr + " ]   priceWhereStr :: [ " + priceWhereStr + " ]");

                                


                                if (entitiesStr != "")
                                {

                                    if ((entitiesStr.Contains("car color") && entitiesStr.Contains("exterior color")) || (entitiesStr.Contains("car color") && entitiesStr.Contains("interior color")) || (entitiesStr.Contains("car color") && !entitiesStr.Contains("option")) || (entitiesStr.Contains("car color") && !entitiesStr.Contains("price")) || entitiesStr.Equals("car color") )
                                    {

                                        Debug.WriteLine("색상 질문");

                                        List<CarTrimList> CarPriceList = db.SelectCarTrimList1(priceWhereStr);

                                        //데이터가 없을 때 예외 처리
                                        if (CarPriceList.Count == 0)
                                        {
                                            //await Conversation.SendAsync(activity, () => new RootDialog(luis_intent, entitiesStr, startTime, orgKRMent, orgENGMent));
                                            //response = Request.CreateResponse(HttpStatusCode.OK);
                                            //return response;

                                            Activity reply_ment = activity.CreateReply();
                                            reply_ment.Recipient = activity.From;
                                            reply_ment.Type = "message";
                                            reply_ment.Text = "Kona의 가격은 1,895만원부터 시작돼요, 상세 견적이 필요하시면 엔진과 구동 방식을 선택해 주세요";
                                            var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                                            string mainModelTitle = "";
                                            List<CarModelList> CarModelList = db.SelectCarModelList();

                                            for (int td = 0; td < CarModelList.Count; td++)
                                            {
                                                mainModelTitle = CarModelList[td].carModelNm;
                                                Debug.WriteLine("mainModelTitle : " + mainModelTitle);
                                                if (mainModelTitle.Contains("TUIX"))
                                                {
                                                    mainModelTitle = mainModelTitle + " 2WD";
                                                }
                                                Debug.WriteLine("mainModelTitle 2 : " + mainModelTitle);

                                                replyToConversation.Attachments.Add(
                                                GetHeroCard_show(
                                                mainModelTitle,
                                                "",
                                                "",
                                                new CardImage(url: PriceImageList.GetPriceImage(CarModelList[td].carModelNm)),
                                                new CardAction(ActionTypes.ImBack, mainModelTitle + " 가격", value: CarModelList[td].carModelNm + " 가격"), "", "")
                                                );
                                            }
                                        }

                                        if (entitiesStr.Equals("car color"))
                                        {
                                            Debug.WriteLine("전체 색상 질문");

                                            Activity reply_ment = activity.CreateReply();
                                            reply_ment.Recipient = activity.From;
                                            reply_ment.Type = "message";
                                            reply_ment.Text = "전체 색상을 보여드릴게요";
                                            var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                                            List<CarExColorList> CarExColorList = db.SelectCarExColorAllList();
                                            //데이터가 없을 때 예외 처리
                                            if (CarExColorList.Count == 0)
                                            {
                                                Activity reply_err = activity.CreateReply();
                                                reply_err.Recipient = activity.From;
                                                reply_err.Type = "message";
                                                reply_err.Text = SorryMessageList.GetSorryMessage(++sorryMessageCnt);
                                                await connector.Conversations.SendToConversationAsync(reply_err);

                                                response = Request.CreateResponse(HttpStatusCode.OK);
                                                return response;
                                            }
                                            else
                                            {
                                                if (dlg.Count > 0)
                                                {
                                                    Debug.WriteLine("dlg[0].dlgMent : [" + dlg[0].dlgMent + "]");
                                                    Debug.WriteLine("dlg[0].dlgMent : [" + string.IsNullOrEmpty(dlg[0].dlgMent) + "]");
                                                    if (string.IsNullOrEmpty(dlg[0].dlgMent) != true)
                                                    {
                                                        Activity reply = activity.CreateReply(dlg[0].dlgMent.ToString());
                                                        await connector.Conversations.ReplyToActivityAsync(reply);
                                                    }
                                                }


                                                for (int td = 0; td < CarExColorList.Count; td++)
                                                {

                                                    string trimNM = CarExColorList[td].trimColorNm;
                                                    trimNM = trimNM.Replace("가솔린 ", "");
                                                    trimNM = trimNM.Replace("디젤 ", "");
                                                    trimNM = trimNM.Replace("2WD ", "");
                                                    trimNM = trimNM.Replace("4WD ", "");
                                                    trimNM = trimNM.Replace("코나 ", "");
                                                    trimNM = trimNM.Replace("1.6 ", "");
                                                    trimNM = trimNM.Replace("오토 ", "");
                                                    trimNM = trimNM.Replace("오토", "");
                                                    trimNM = trimNM.Replace("터보 ", "");

                                                    replyToConversation.Attachments.Add(
                                                    GetHeroCard_info(
                                                    trimNM,
                                                    "추가 금액 : " + string.Format("{0}", CarExColorList[td].exColorPrice.ToString("n0")) + "원",
                                                    "",
                                                    new CardImage(url: "https://bottest.hyundai.com/assets/images/price/exterior/" + CarExColorList[td].trimColorCd + ".jpg"), "turn", CarExColorList[td].trimColorCd)
                                                    );
                                                }
                                            }
                                            
                                        }
                                        else if (entitiesStr.Equals("car color.exterior color"))
                                        {
                                            Debug.WriteLine("외장 전체 색상 질문");

                                            

                                            List<CarExColorList> CarExColorList = db.SelectCarExColorAllList();
                                            Debug.WriteLine("exteriorexteriorexteriorexterior");
                                            //데이터가 없을 때 예외 처리 
                                            if (CarExColorList.Count == 0)
                                            {
                                                //await Conversation.SendAsync(activity, () => new RootDialog(luis_intent, entitiesStr, startTime, orgKRMent, orgENGMent));
                                                //response = Request.CreateResponse(HttpStatusCode.OK);
                                                //return response;

                                                Activity reply_ment = activity.CreateReply();
                                                reply_ment.Recipient = activity.From;
                                                reply_ment.Type = "message";
                                                reply_ment.Text = "Kona의 가격은 1,895만원부터 시작돼요, 상세 견적이 필요하시면 엔진과 구동 방식을 선택해 주세요";
                                                var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                                                string mainModelTitle = "";
                                                List<CarModelList> CarModelList = db.SelectCarModelList();

                                                for (int td = 0; td < CarModelList.Count; td++)
                                                {
                                                    mainModelTitle = CarModelList[td].carModelNm;
                                                    Debug.WriteLine("mainModelTitle : " + mainModelTitle);
                                                    if (mainModelTitle.Contains("TUIX"))
                                                    {
                                                        mainModelTitle = mainModelTitle + " 2WD";
                                                    }
                                                    Debug.WriteLine("mainModelTitle 2 : " + mainModelTitle);

                                                    replyToConversation.Attachments.Add(
                                                    GetHeroCard_show(
                                                    mainModelTitle,
                                                    "",
                                                    "",
                                                    new CardImage(url: PriceImageList.GetPriceImage(CarModelList[td].carModelNm)),
                                                    new CardAction(ActionTypes.ImBack, mainModelTitle + " 가격", value: CarModelList[td].carModelNm + " 가격"), "", "")
                                                    );
                                                }
                                            }
                                            else
                                            {
                                                Activity reply_ment = activity.CreateReply();
                                                reply_ment.Recipient = activity.From;
                                                reply_ment.Type = "message";
                                                reply_ment.Text = "전체 외장색상을 보여드릴게요";
                                                var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);


                                                for (int td = 0; td < CarExColorList.Count; td++)
                                                {
                                                    Debug.WriteLine("exterior color : " + CarExColorList[td].trimColorCd);
                                                    string trimNM = CarExColorList[td].trimColorNm;
                                                    trimNM = trimNM.Replace("가솔린 ", "");
                                                    trimNM = trimNM.Replace("디젤 ", "");
                                                    trimNM = trimNM.Replace("2WD ", "");
                                                    trimNM = trimNM.Replace("4WD ", "");
                                                    trimNM = trimNM.Replace("코나 ", "");
                                                    trimNM = trimNM.Replace("1.6 ", "");
                                                    trimNM = trimNM.Replace("오토 ", "");
                                                    trimNM = trimNM.Replace("오토", "");
                                                    trimNM = trimNM.Replace("터보 ", "");

                                                    replyToConversation.Attachments.Add(
                                                    GetHeroCard_info(
                                                    trimNM,
                                                    "추가 금액 : " + string.Format("{0}", CarExColorList[td].exColorPrice.ToString("n0")) + "원",
                                                    "",
                                                    new CardImage(url: "https://bottest.hyundai.com/assets/images/price/exterior/" + CarExColorList[td].trimColorCd + ".jpg"), "turn", CarExColorList[td].trimColorCd)
                                                    );
                                                }
                                            }
                                            

                                        }
                                        else if (entitiesStr.Equals("car color.interior color"))
                                        {
                                            Debug.WriteLine("내장 전체 색상 질문");

                                            

                                            List<CarInColorList> CarInColorList = db.SelectCarInColorAllList();
                                            Debug.WriteLine("interiorinteriorinteriorinterior");
                                            //데이터가 없을 때 예외 처리
                                            if (CarInColorList.Count == 0)
                                            {
                                                //await Conversation.SendAsync(activity, () => new RootDialog(luis_intent, entitiesStr, startTime, orgKRMent, orgENGMent));
                                                //response = Request.CreateResponse(HttpStatusCode.OK);
                                                //return response;

                                                Activity reply_ment = activity.CreateReply();
                                                reply_ment.Recipient = activity.From;
                                                reply_ment.Type = "message";
                                                reply_ment.Text = "Kona의 가격은 1,895만원부터 시작돼요, 상세 견적이 필요하시면 엔진과 구동 방식을 선택해 주세요";
                                                var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                                                string mainModelTitle = "";
                                                List<CarModelList> CarModelList = db.SelectCarModelList();

                                                for (int td = 0; td < CarModelList.Count; td++)
                                                {
                                                    mainModelTitle = CarModelList[td].carModelNm;
                                                    Debug.WriteLine("mainModelTitle : " + mainModelTitle);
                                                    if (mainModelTitle.Contains("TUIX"))
                                                    {
                                                        mainModelTitle = mainModelTitle + " 2WD";
                                                    }
                                                    Debug.WriteLine("mainModelTitle 2 : " + mainModelTitle);

                                                    replyToConversation.Attachments.Add(
                                                    GetHeroCard_show(
                                                    mainModelTitle,
                                                    "",
                                                    "",
                                                    new CardImage(url: PriceImageList.GetPriceImage(CarModelList[td].carModelNm)),
                                                    new CardAction(ActionTypes.ImBack, mainModelTitle + " 가격", value: CarModelList[td].carModelNm + " 가격"), "", "")
                                                    );
                                                }
                                            }
                                            else
                                            {
                                                Activity reply_ment = activity.CreateReply();
                                                reply_ment.Recipient = activity.From;
                                                reply_ment.Type = "message";
                                                reply_ment.Text = "전체 내장색상을 보여드릴게요";
                                                var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                                                for (int td = 0; td < CarInColorList.Count; td++)
                                                {

                                                    string trimNM = CarInColorList[td].internalColorNm;
                                                    trimNM = trimNM.Replace("가솔린 ", "");
                                                    trimNM = trimNM.Replace("디젤 ", "");
                                                    trimNM = trimNM.Replace("2WD ", "");
                                                    trimNM = trimNM.Replace("4WD ", "");
                                                    trimNM = trimNM.Replace("코나 ", "");
                                                    trimNM = trimNM.Replace("1.6 ", "");
                                                    trimNM = trimNM.Replace("오토 ", "");
                                                    trimNM = trimNM.Replace("오토", "");
                                                    trimNM = trimNM.Replace("터보 ", "");

                                                    replyToConversation.Attachments.Add(
                                                    GetHeroCard_info(
                                                    trimNM,
                                                    "추가 금액 : " + string.Format("{0}", CarInColorList[td].inColorPrice.ToString("n0")) + "원",
                                                    "",
                                                    new CardImage(url: "https://bottest.hyundai.com/assets/images/price/interior/" + CarInColorList[td].internalColorCd + ".jpg"), "", "")
                                                    );
                                                }
                                            }

                                            
                                        }
                                        else
                                        {
                                            Debug.WriteLine("트림, 엔진, 드라이브 휠, 칼라 패키지, 튜익스 색상 질문");
                                            string colorMent = activity.Text;
                                            //colorMent = activity.Text.Replace("옵션", "");
                                            colorMent = colorMent.Replace("가격", "");
                                            colorMent = colorMent.Replace("exterior", "외장");
                                            colorMent = colorMent.Replace("interior", "내장");
                                            colorMent = colorMent.Replace("color", "색상");
                                            //priceMent = activity.Text.Replace("price", "");

                                            int index = colorMent.IndexOf("색상") + 2;

                                            //Debug.WriteLine("1 : " + colorMent.Substring(0, index));
                                            //Debug.WriteLine("2 : " + colorMent.Substring(0, index + 1));
                                            //Debug.WriteLine("3 : " + colorMent.Substring(0, index + 1));

                                            colorMent = colorMent.Substring(0, index);

                                            Activity reply_ment = activity.CreateReply();
                                            reply_ment.Recipient = activity.From;
                                            reply_ment.Type = "message";
                                            //reply_ment.Text = colorMent + "을 보여드릴게요";
                                            if(priceWhereStr.Contains("exterior"))
                                            {
                                                reply_ment.Text = "선택하신 트림의 외장 색상을 보여드릴게요";
                                            }
                                            else if (priceWhereStr.Contains("interior"))
                                            {
                                                reply_ment.Text = "선택하신 트림의 내장색상을 보여드릴게요";
                                            }
                                                
                                            var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                                            string color = "";

                                            for (int i = 0; i < CarPriceList.Count(); i++)
                                            {

                                                string trimNM = CarPriceList[i].carTrimNm;
                                                trimNM = trimNM.Replace("코나 ", "");
                                                trimNM = trimNM.Replace("1.6 ", "");
                                                trimNM = trimNM.Replace("오토 ", "");
                                                trimNM = trimNM.Replace("오토", "");
                                                trimNM = trimNM.Replace("터보 ", "");

                                                if (!CarPriceList[i].saleCD.Contains("X"))
                                                {

                                                    if (!CarPriceList[i].carTrimNm.Contains("투톤"))
                                                    {

                                                        if (entitiesStr.Contains("exterior") )
                                                        {

                                                            color = (string)(CarPriceList[i].tuix + CarPriceList[i].cartrim);
                                                            replyToConversation.Attachments.Add(
                                                            GetHeroCard_show(
                                                                trimNM.Replace(" 1.6", ""),
                                                                "",
                                                                "",
                                                                new CardImage(url: "https://bottest.hyundai.com/assets/images/price/trim/" + color.Replace(" ", "") + ".jpg"),
                                                                new CardAction(ActionTypes.ImBack, "외장색상", value: trimNM.Replace(" 1.6", "") + " 트림 외장색상"), "", "")
                                                            );
                                                        }
                                                        else if (entitiesStr.Contains("interior"))
                                                        {

                                                            color = (string)(CarPriceList[i].tuix + CarPriceList[i].cartrim);
                                                            replyToConversation.Attachments.Add(
                                                            GetHeroCard_show(
                                                                trimNM,
                                                                "",
                                                                "",
                                                                new CardImage(url: "https://bottest.hyundai.com/assets/images/price/trim/" + color.Replace(" ", "") + ".jpg"),
                                                                new CardAction(ActionTypes.ImBack, "내장색상", value: trimNM.Replace(" 1.6", "") + " 트림 내장색상"), "", "")
                                                            );
                                                        }
                                                        else if (entitiesStr.Contains("car color"))
                                                        {

                                                            color = (string)(CarPriceList[i].tuix + CarPriceList[i].cartrim);
                                                            replyToConversation.Attachments.Add(
                                                            GetHeroCard_show(
                                                                trimNM.Replace(" 1.6", ""),
                                                                "",
                                                                "",
                                                                new CardImage(url: "https://bottest.hyundai.com/assets/images/price/trim/" + color.Replace(" ", "") + ".jpg"),
                                                                new CardAction(ActionTypes.ImBack, "외장색상", value: trimNM.Replace(" 1.6", "") + " 트림 외장색상"), "", "")
                                                            );
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                        //luis_intent = (string)Luis["intents"][0]["intent"];
                                        luis_intent = gubunVal;
                                    }
                                    //else if (!entitiesStr.Contains("car color") && entitiesStr.Contains("option"))
                                    else if (entitiesStr.Contains("option"))
                                    {
                                        Debug.WriteLine("옵션 질문");
                                        if (entitiesStr.Equals("option"))
                                        {

                                            Debug.WriteLine("전체 옵션");

                                            

                                            List<CarOptionList> carOptionList = db.SelectOptionList(priceWhereStr);

                                            //데이터가 없을 때 예외 처리
                                            if (carOptionList.Count == 0)
                                            {
                                                //await Conversation.SendAsync(activity, () => new RootDialog(luis_intent, entitiesStr, startTime, orgKRMent, orgENGMent));
                                                //response = Request.CreateResponse(HttpStatusCode.OK);
                                                //return response;

                                                Activity reply_ment = activity.CreateReply();
                                                reply_ment.Recipient = activity.From;
                                                reply_ment.Type = "message";
                                                reply_ment.Text = "Kona의 가격은 1,895만원부터 시작돼요, 상세 견적이 필요하시면 엔진과 구동 방식을 선택해 주세요";
                                                var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                                                string mainModelTitle = "";
                                                List<CarModelList> CarModelList = db.SelectCarModelList();

                                                for (int td = 0; td < CarModelList.Count; td++)
                                                {
                                                    mainModelTitle = CarModelList[td].carModelNm;
                                                    Debug.WriteLine("mainModelTitle : " + mainModelTitle);
                                                    if (mainModelTitle.Contains("TUIX"))
                                                    {
                                                        mainModelTitle = mainModelTitle + " 2WD";
                                                    }
                                                    Debug.WriteLine("mainModelTitle 2 : " + mainModelTitle);

                                                    replyToConversation.Attachments.Add(
                                                    GetHeroCard_show(
                                                    mainModelTitle,
                                                    "",
                                                    "",
                                                    new CardImage(url: PriceImageList.GetPriceImage(CarModelList[td].carModelNm)),
                                                    new CardAction(ActionTypes.ImBack, mainModelTitle + " 가격", value: CarModelList[td].carModelNm + " 가격"), "", "")
                                                    );
                                                }
                                            }
                                            else
                                            {
                                                Activity reply_ment = activity.CreateReply();
                                                reply_ment.Recipient = activity.From;
                                                reply_ment.Type = "message";
                                                reply_ment.Text = "전체 옵션을 보여드릴게요";
                                                var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                                                for (int td = 0; td < carOptionList.Count; td++)
                                                {

                                                    translateInfo = await getTranslate(carOptionList[td].optNm);

                                                    replyToConversation.Attachments.Add(
                                                    GetHeroCard_info(
                                                    carOptionList[td].optNm,
                                                    "추가 금액 : " + string.Format("{0}", carOptionList[td].optPrice.ToString("n0")) + "원",
                                                    "",
                                                    new CardImage(url: "https://bottest.hyundai.com/assets/images/price/option/" + (translateInfo.data.translations[0].translatedText).Replace(" ", "_") + ".jpg"), "", "")
                                                    );
                                                }
                                            }
                                            
                                        }
                                        else
                                        {
                                            List<CarOptionList> carOptionList = db.SelectOptionList(priceWhereStr);
                                            string optionMent = activity.Text;

                                            //optionMent = optionMent.Replace("가격", "");
                                            //priceMent = activity.Text.Replace("price", "");

                                            //int index = optionMent.IndexOf("옵션") + 2;

                                            //Debug.WriteLine("1 : " + optionMent.Substring(0, index));
                                            //Debug.WriteLine("2 : " + optionMent.Substring(0, index + 1));
                                            

                                            //optionMent = optionMent.Substring(0, index);

                                            //optionMent = optionMent.Replace("옵션", "");

                                            

                                            //데이터가 없을 때 예외 처리
                                            if (carOptionList.Count == 0)
                                            {
                                                //await Conversation.SendAsync(activity, () => new RootDialog(luis_intent, entitiesStr, startTime, orgKRMent, orgENGMent));
                                                //response = Request.CreateResponse(HttpStatusCode.OK);
                                                //return response;

                                                Activity reply_ment = activity.CreateReply();
                                                reply_ment.Recipient = activity.From;
                                                reply_ment.Type = "message";
                                                reply_ment.Text = "Kona의 가격은 1,895만원부터 시작돼요, 상세 견적이 필요하시면 엔진과 구동 방식을 선택해 주세요";
                                                var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                                                string mainModelTitle = "";
                                                List<CarModelList> CarModelList = db.SelectCarModelList();

                                                for (int td = 0; td < CarModelList.Count; td++)
                                                {
                                                    mainModelTitle = CarModelList[td].carModelNm;
                                                    Debug.WriteLine("mainModelTitle : " + mainModelTitle);
                                                    if (mainModelTitle.Contains("TUIX"))
                                                    {
                                                        mainModelTitle = mainModelTitle + " 2WD";
                                                    }
                                                    Debug.WriteLine("mainModelTitle 2 : " + mainModelTitle);

                                                    replyToConversation.Attachments.Add(
                                                    GetHeroCard_show(
                                                    mainModelTitle,
                                                    "",
                                                    "",
                                                    new CardImage(url: PriceImageList.GetPriceImage(CarModelList[td].carModelNm)),
                                                    new CardAction(ActionTypes.ImBack, mainModelTitle + " 가격", value: CarModelList[td].carModelNm + " 가격"), "", "")
                                                    );
                                                }
                                            }
                                            else
                                            {
                                                Activity reply_ment = activity.CreateReply();
                                                reply_ment.Recipient = activity.From;
                                                reply_ment.Type = "message";
                                                //reply_ment.Text = optionMent + "의 추가 옵션을 보여드릴게요";
                                                reply_ment.Text = "선택하신 트림의 추가 옵션을 보여드릴게요";
                                                var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                                                for (int td = 0; td < carOptionList.Count; td++)
                                                {

                                                    Translator translateInfo1 = await getTranslate(carOptionList[td].optNm);

                                                    //Debug.WriteLine("translateInfo. : " + (translateInfo1.data.translations[0].translatedText).Replace(" ", "_"));
                                                    replyToConversation.Attachments.Add(
                                                    GetHeroCard_info(
                                                    carOptionList[td].optNm,
                                                    "추가 금액 : " + string.Format("{0}", carOptionList[td].optPrice.ToString("n0")) + "원",
                                                    "",
                                                    new CardImage(url: "https://bottest.hyundai.com/assets/images/price/option/" + (translateInfo1.data.translations[0].translatedText).Replace(" ", "_") + ".jpg"), "", "")
                                                    );
                                                }
                                            }
                                            
                                        }
                                        //luis_intent = (string)Luis["intents"][0]["intent"];
                                        luis_intent = gubunVal;
                                    }
                                    // 모델 질문(견적 보여줘)
                                    else if (entitiesStr.Equals("price"))
                                    {
                                        Debug.WriteLine("견적 보여줘 질문");
                                        string mainModelTitle = "";
                                        List<CarModelList> CarModelList = db.SelectCarModelList();

                                        //데이터가 없을 때 예외 처리
                                        if (CarModelList.Count == 0)
                                        {

                                            await Conversation.SendAsync(activity, () => new RootDialog(luis_intent, entitiesStr, startTime, orgKRMent, orgENGMent));

                                            //Activity reply_err = activity.CreateReply();
                                            //reply_err.Recipient = activity.From;
                                            //reply_err.Type = "message";
                                            //reply_err.Text = SorryMessageList.GetSorryMessage(++sorryMessageCnt) + "[ '" + luis_intent + "','" + entitiesStr + "' ]";
                                            //await connector.Conversations.SendToConversationAsync(reply_err);

                                            response = Request.CreateResponse(HttpStatusCode.OK);
                                            return response;
                                        }
                                        else
                                        {
                                            if (dlg.Count > 0)
                                            {
                                                Debug.WriteLine("dlg[0].dlgMent : [" + dlg[0].dlgMent + "]");
                                                Debug.WriteLine("dlg[0].dlgMent : [" + string.IsNullOrEmpty(dlg[0].dlgMent) + "]");
                                                if (string.IsNullOrEmpty(dlg[0].dlgMent) != true)
                                                {
                                                    Activity reply = activity.CreateReply(dlg[0].dlgMent.ToString());
                                                    await connector.Conversations.ReplyToActivityAsync(reply);
                                                }
                                            }

                                            for (int td = 0; td < CarModelList.Count; td++)
                                            {
                                                mainModelTitle = CarModelList[td].carModelNm;
                                                Debug.WriteLine("mainModelTitle : " + mainModelTitle);
                                                if (mainModelTitle.Contains("TUIX"))
                                                {
                                                    mainModelTitle = mainModelTitle + " 2WD";
                                                }
                                                Debug.WriteLine("mainModelTitle 2 : " + mainModelTitle);

                                                replyToConversation.Attachments.Add(
                                                GetHeroCard_show(
                                                mainModelTitle,
                                                "",
                                                "",
                                                new CardImage(url: PriceImageList.GetPriceImage(CarModelList[td].carModelNm)),
                                                new CardAction(ActionTypes.ImBack, mainModelTitle + " 가격", value: CarModelList[td].carModelNm + " 가격"), "", "")
                                                );
                                            }
                                        }

                                        
                                        //luis_intent = (string)Luis["intents"][0]["intent"];
                                        luis_intent = gubunVal;
                                    }
                                    // 트림,엔진, 드라이브 휠, 칼라패키지, 튜익스 가격 질문
                                    else if (!entitiesStr.Contains("car color") && !entitiesStr.Contains("option"))
                                    {
                                        Debug.WriteLine("트림, 엔진, 드라이브 휠, 칼라 패키지, 튜익스 가격 질문");

                                        string color = "";
                                        string priceMent = "";
                                        //Debug.WriteLine("가격 질문 멘트 : " + orgMent.Replace("가격", ""));
                                        int index = 0;

                                        //priceMent = activity.Text.Replace("price", "");
                                        
                                        //index = priceMent.IndexOf("가격") + 2;
                                        //index = priceMent.IndexOf("price") + 5;

                                        //priceMent = priceMent.Substring(0, index);

                                        //priceMent = activity.Text.Replace("가격", "");
                                        //priceMent = activity.Text.Replace("price", "");

                                        


                                        List<CarTrimList> CarTrimList = db.SelectCarTrimList1(priceWhereStr);

                                        //데이터가 없을 때 예외 처리
                                        if (CarTrimList.Count == 0)
                                        {
                                            //await Conversation.SendAsync(activity, () => new RootDialog(luis_intent, entitiesStr, startTime, orgKRMent, orgENGMent));
                                            //response = Request.CreateResponse(HttpStatusCode.OK);
                                            //return response;

                                            Activity reply_ment = activity.CreateReply();
                                            reply_ment.Recipient = activity.From;
                                            reply_ment.Type = "message";
                                            reply_ment.Text = "Kona의 가격은 1,895만원부터 시작돼요, 상세 견적이 필요하시면 엔진과 구동 방식을 선택해 주세요";
                                            var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                                            string mainModelTitle = "";
                                            List<CarModelList> CarModelList = db.SelectCarModelList();

                                            for (int td = 0; td < CarModelList.Count; td++)
                                            {
                                                mainModelTitle = CarModelList[td].carModelNm;
                                                Debug.WriteLine("mainModelTitle : " + mainModelTitle);
                                                if (mainModelTitle.Contains("TUIX"))
                                                {
                                                    mainModelTitle = mainModelTitle + " 2WD";
                                                }
                                                Debug.WriteLine("mainModelTitle 2 : " + mainModelTitle);

                                                replyToConversation.Attachments.Add(
                                                GetHeroCard_show(
                                                mainModelTitle,
                                                "",
                                                "",
                                                new CardImage(url: PriceImageList.GetPriceImage(CarModelList[td].carModelNm)),
                                                new CardAction(ActionTypes.ImBack, mainModelTitle + " 가격", value: CarModelList[td].carModelNm + " 가격"), "", "")
                                                );
                                            }

                                        }
                                        else
                                        {
                                            if (CarTrimList.Count > 0)
                                            {
                                                Activity reply_ment = activity.CreateReply();
                                                reply_ment.Recipient = activity.From;
                                                reply_ment.Type = "message";
                                                //reply_ment.Text = priceMent + " 가격을 보여드릴게요";
                                                reply_ment.Text = "선택하신 트림의 가격을 보여드릴게요";
                                                var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);


                                                for (int td = 0; td < CarTrimList.Count; td++)
                                                {

                                                    string trimNM = CarTrimList[td].carTrimNm;
                                                    trimNM = trimNM.Replace("코나 ", "");
                                                    trimNM = trimNM.Replace("1.6 ", "");
                                                    trimNM = trimNM.Replace("오토 ", "");
                                                    trimNM = trimNM.Replace("오토", "");
                                                    trimNM = trimNM.Replace("터보 ", "");

                                                    if (!CarTrimList[td].saleCD.Contains("XX"))
                                                    {
                                                        color = (string)(CarTrimList[td].tuix + CarTrimList[td].cartrim);
                                                        //Debug.WriteLine("AA : " + CarTrimList[td].tuix + CarTrimList[td].cartrim);
                                                        //Debug.WriteLine("color : " + color);
                                                        replyToConversation.Attachments.Add(
                                                        GetHeroCard_show(
                                                        trimNM,
                                                        string.Format("{0}", CarTrimList[td].salePrice.ToString("n0")) + "원",
                                                        "",
                                                        new CardImage(url: "https://bottest.hyundai.com/assets/images/price/trim/" + color.Replace(" ", "") + ".jpg"),
                                                        new CardAction(ActionTypes.ImBack, "트림 자세히 보기", value: trimNM + " 트림"), "", "")
                                                        );
                                                    }

                                                }
                                            }
                                        }
                                        
                                        //luis_intent = (string)Luis["intents"][0]["intent"];
                                        luis_intent = gubunVal;
                                    }
                                }
                                //luis_intent = (string)Luis["intents"][0]["intent"];
                            }

                            //other
                            else
                            {

                                if (dlg.Count > 0)
                                {
                                    Debug.WriteLine("dlg[0].dlgMent : [" + dlg[0].dlgMent + "]");
                                    Debug.WriteLine("dlg[0].dlgMent : [" + string.IsNullOrEmpty(dlg[0].dlgMent) + "]");
                                    if (string.IsNullOrEmpty(dlg[0].dlgMent) != true)
                                    {
                                        Activity reply = activity.CreateReply(dlg[0].dlgMent.ToString());
                                        await connector.Conversations.ReplyToActivityAsync(reply);
                                    }
                                }

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
                                        //HeroCard[] plHeroCard = new HeroCard[card.Count];
                                        UserHeroCard[] plHeroCard = new UserHeroCard[card.Count];
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

                                            string text = card[i].cardTitle;

                                            //card.Count


                                            plHeroCard[i] = new UserHeroCard()
                                            {
                                                Title = card[i].cardTitle,
                                                Text = card[i].cardText,
                                                Subtitle = card[i].cardSubTitle,
                                                Images = cardImages,
                                                //Tap = tap,
                                                Buttons = cardButtons,
                                                Card_division = card[i].cardDivision,
                                                Card_value = card[i].cardValue,
                                                Card_cnt = card.Count
                                            };

                                            //if (i > 0)
                                            //{
                                            //    plHeroCard[i] = new UserHeroCard()
                                            //    {
                                            //        Title = card[i].cardTitle,
                                            //        Text = card[i].cardText,
                                            //        Subtitle = card[i].cardSubTitle,
                                            //        Images = cardImages,
                                            //        //Tap = tap,
                                            //        Buttons = cardButtons,
                                            //        Card_division = card[i].cardDivision,
                                            //        Card_value = card[i].cardValue,
                                            //        Card_cnt = false
                                            //    };
                                            //}
                                            //else if(i == 0)
                                            //{
                                            //    plHeroCard[i] = new UserHeroCard()
                                            //    {
                                            //        Title = card[i].cardTitle,
                                            //        Text = card[i].cardText,
                                            //        Subtitle = card[i].cardSubTitle,
                                            //        Images = cardImages,
                                            //        //Tap = tap,
                                            //        Buttons = cardButtons,
                                            //        Card_division = card[i].cardDivision,
                                            //        Card_value = card[i].cardValue,
                                            //        Card_cnt = true
                                            //    };
                                            //}
                                            
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

                            if ((string)Luis["intents"][0]["intent"] == "communication")
                            {
                                string hambuger = "≡";

                                Activity reply_start = activity.CreateReply();
                                reply_start.Recipient = activity.From;
                                reply_start.Type = "message";
                                reply_start.Text = "<span style=\"color:rgb(0,0,255);font-size:15pt; font-weight:bold\">" + hambuger + "</span> 를 누르면 전체 메뉴가 나와요";
                                await connector.Conversations.SendToConversationAsync(reply_start);

                                response = Request.CreateResponse(HttpStatusCode.OK);
                                return response;
                            }

                        }
                        //}

                        if (LuisDialogID.Count == 0)
                        {
                            //int dbResult = db.insertUserQuery(orgKRMent, orgENGMent, luis_intent, entitiesStr, luisID, 'D', testDriveWhereStr, "", priceWhereStr, gubunVal);
                            ////int dbResult = db.insertUserQuery(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), luis_intent, entitiesStr, luisID, 'D', testDriveWhereStr, "", priceWhereStr, gubunVal);
                            //Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());


                            if(luis_intent.Equals("Quote"))
                            {

                                Activity reply_ment = activity.CreateReply();
                                reply_ment.Recipient = activity.From;
                                reply_ment.Type = "message";
                                reply_ment.Text = "Kona의 가격은 1,895만원부터 시작돼요, 상세 견적이 필요하시면 엔진과 구동 방식을 선택해 주세요";
                                var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);


                                replyToConversation.Recipient = activity.From;
                                replyToConversation.Type = "message";
                                replyToConversation.Attachments = new List<Attachment>();
                                replyToConversation.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                                string mainModelTitle = "";
                                List<CarModelList> CarModelList = db.SelectCarModelList();

                                for (int td = 0; td < CarModelList.Count; td++)
                                {
                                    mainModelTitle = CarModelList[td].carModelNm;
                                    Debug.WriteLine("mainModelTitle : " + mainModelTitle);
                                    if (mainModelTitle.Contains("TUIX"))
                                    {
                                        mainModelTitle = mainModelTitle + " 2WD";
                                    }
                                    Debug.WriteLine("mainModelTitle 2 : " + mainModelTitle);

                                    replyToConversation.Attachments.Add(
                                    GetHeroCard_show(
                                    mainModelTitle,
                                    "",
                                    "",
                                    new CardImage(url: PriceImageList.GetPriceImage(CarModelList[td].carModelNm)),
                                    new CardAction(ActionTypes.ImBack, mainModelTitle + " 가격", value: CarModelList[td].carModelNm + " 가격"), "", "")
                                    );
                                }
                                var reply1 = await connector.Conversations.SendToConversationAsync(replyToConversation);

                                int dbResult = db.insertUserQuery(orgKRMent, orgENGMent, luis_intent, entitiesStr, luisID, 'H', testDriveWhereStr, "", priceWhereStr, gubunVal);
                                //int dbResult = db.insertUserQuery(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), luis_intent, entitiesStr, luisID, 'H', testDriveWhereStr, "", priceWhereStr, gubunVal);
                                Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());

                                response = Request.CreateResponse(HttpStatusCode.OK);
                                return response;
                                

                            }
                            else
                            {
                                await Conversation.SendAsync(activity, () => new RootDialog(luis_intent, entitiesStr, startTime, orgKRMent, orgENGMent));

                                response = Request.CreateResponse(HttpStatusCode.OK);
                                return response;
                            }
                            
                        }
                        else
                        {
                            int dbResult = db.insertUserQuery(orgKRMent, orgENGMent, luis_intent, entitiesStr, luisID, 'H', testDriveWhereStr, "", priceWhereStr, gubunVal);
                            //int dbResult = db.insertUserQuery(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), luis_intent, entitiesStr, luisID, 'H', testDriveWhereStr, "", priceWhereStr, gubunVal);
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
                        sorryMessageCnt = 0;
                        //});
                        HistoryLog("[ DIALOG ] ==>> userID :: [ " + activity.Conversation.Id + " ]       message :: [ " + orgMent + " ]       date :: [ " + DateTime.Now + " ]");
                    }
                    catch
                    {
                        //Regex.Replace(orgMent, @"[^a-zA-Z0-9가-힣-\s]", "", RegexOptions.Singleline);
                        int dbResult = db.insertUserQuery(orgKRMent, orgENGMent, luis_intent, entitiesStr, luisID, 'D', testDriveWhereStr, "", priceWhereStr, gubunVal);
                        //int dbResult = db.insertUserQuery(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), luis_intent, entitiesStr, luisID, 'D', testDriveWhereStr, "",priceWhereStr, gubunVal);
                        Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());

                        await Conversation.SendAsync(activity, () => new RootDialog(luis_intent, entitiesStr, startTime, orgKRMent, orgENGMent));

                        DateTime endTime = DateTime.Now;

                        Debug.WriteLine("USER NUMBER : " + activity.Conversation.Id);
                        Debug.WriteLine("CUSTOMMER COMMENT KOREAN : " + activity.Text);
                        Debug.WriteLine("CUSTOMMER COMMENT ENGLISH : " + translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));
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

                        HistoryLog("[dialog response end] ==>> userID :: ["+ activity.Conversation.Id + "]" );
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


        private static async Task<JObject> GetIntentFromKonaBotLUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            JObject jsonObj = new JObject();
            using (HttpClient client = new HttpClient())
            {
                //string RequestURI = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/fd9f899c-5a48-499e-9037-9ea589953684?subscription-key=7efb093087dd48918b903885b944740c&timezoneOffset=0&verbose=true&q=" + Query;
                string RequestURI = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/04259452-27fe-4f72-9441-c4100b835c52?subscription-key=7efb093087dd48918b903885b944740c&timezoneOffset=0&verbose=true&q=" + Query;
                
                //string RequestURI = "https://api.projectoxford.ai/luis/v1/application?id=fd9f899c-5a48-499e-9037-9ea589953684&subscription-key=7efb093087dd48918b903885b944740c&q=" + Query;
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
                string RequestURI = "https://api.projectoxford.ai/luis/v1/application?id=b1437ec6-3301-4c24-8bcb-1af58ee2c47c&subscription-key=7efb093087dd48918b903885b944740c&q=" + Query;
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
                string RequestURI = "https://api.projectoxford.ai/luis/v1/application?id=251a1260-0150-46ac-99b6-56e401a2e520&subscription-key=7efb093087dd48918b903885b944740c&q=" + Query;
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
                string RequestURI = "https://api.projectoxford.ai/luis/v1/application?id=9860fbdd-48c1-4a28-affe-cb8faf8041e5&subscription-key=7efb093087dd48918b903885b944740c&q=" + Query;
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
                string RequestURI = "https://api.projectoxford.ai/luis/v1/application?id=ad87b24e-200f-4bde-9b25-c8885635f221&subscription-key=7efb093087dd48918b903885b944740c&q=" + Query;
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

                HttpResponseMessage msg = await client.GetAsync(url);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    trans = JsonConvert.DeserializeObject<Translator>(JsonDataResponse);
                }
                return trans;
            }

        }


        private static async Task<Translator> getTranslateJP(string input)
        {
            Translator trans = new Translator();

            using (HttpClient client = new HttpClient())
            {
                string appId = "AIzaSyDr4CH9BVfENdM9uoSK0fANFVWD0gGXlJM";

                string url = string.Format("https://translation.googleapis.com/language/translate/v2/?key={0}&q={1}&source=ko&target=jp&model=nmt", appId, input);

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
        private static Attachment GetHeroCard(string title, string subtitle, string text, CardAction cardAction1, CardAction cardAction2, CardAction cardAction3)
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

        private static Attachment GetHeroCard_info(string title, string subtitle, string text, CardImage cardImage, string cardDivision, string cardValue)
        {
            var heroCard = new UserHeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Card_division = cardDivision,
                Card_value = cardValue,
                //Buttons = new List<CardAction>() { cardAction1, cardAction2 },
            };
            return heroCard.ToAttachment();
        }

        private static Attachment GetHeroCard_show(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction, string cardDivision, string cardValue)
        {
            var heroCard = new UserHeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = new List<CardAction>() { cardAction },
                Card_division = cardDivision,
                Card_value = cardValue,

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

        private static Attachment GetHeroCard_reset(CardAction cardAction1, CardAction cardAction2)
        {
            var heroCard = new HeroCard
            {
                Buttons = new List<CardAction>() { cardAction1, cardAction2 },
            };
            return heroCard.ToAttachment();
        }

        private static Attachment GetHeroCard_area(string title, string subtitle, string text, CardAction cardAction)
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

        public void HistoryLog(String strMsg)
        {
            try
            {

                string m_strLogPrefix = AppDomain.CurrentDomain.BaseDirectory + @"LOG\";
                string m_strLogExt = @".LOG";
                DateTime dtNow = DateTime.Now;
                string strDate = dtNow.ToString("yyyy-MM-dd");
                string strPath = String.Format("{0}{1}{2}", m_strLogPrefix, strDate, m_strLogExt);
                string strDir = Path.GetDirectoryName(strPath);
                DirectoryInfo diDir = new DirectoryInfo(strDir);

                if (!diDir.Exists)
                {
                    diDir.Create();
                    diDir = new DirectoryInfo(strDir);
                }

                if (diDir.Exists)
                {
                    System.IO.StreamWriter swStream = File.AppendText(strPath);
                    string strLog = String.Format("{0}: {1}", dtNow.ToString("MM/dd/yyyy hh:mm:ss.fff"), strMsg);
                    swStream.WriteLine(strLog);
                    swStream.Close(); ;
                }
            }
            catch (System.Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

    }
}
