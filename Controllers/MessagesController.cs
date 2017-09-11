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
using BasicMultiDialogBot.Dialogs;
using System.Text;
using System.Web;
using System.Configuration;
using System.Web.Configuration;
using Microsoft.Bot.Builder.ConnectorEx;

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
        public static string searchLuisIntent = "";
        public static string searchLuisEntities = "";
        public static bool recommendChk = false;
        public static string orgMentBefore = "";
        public static string firstRecommend = "";
        public static string secondRecommend = "";
        public static string thirdRecommend = "";

        public static List<MediaUrl> mediaURL_FB = new List<MediaUrl>();
        public static List<string> mediaTitle_FB = new List<string>();

        public static int rotCnt = 0;

        public static int pagePerCardCnt = 9;
        public static int pageRotationCnt = 1;

        public static Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration("/testkonabot");
        const string redirectEventPageURLstr = "redirectPageURL";
        const string domainURLstr = "domainURL";
        const string luisURLstr = "luisURL";
        string eventURL  = rootWebConfig.ConnectionStrings.ConnectionStrings[redirectEventPageURLstr].ToString();
        string domainURL = rootWebConfig.ConnectionStrings.ConnectionStrings[domainURLstr].ToString();
        public static string luisURL = rootWebConfig.ConnectionStrings.ConnectionStrings[luisURLstr].ToString();


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

                HistoryLog("WELCOME MESSAGE START START START ");
                
                //WeatherInfo weatherInfo = await GetWeatherInfo();
                //Debug.WriteLine("weatherInfo :  " + weatherInfo.list[0].weather[0].description);
                //Debug.WriteLine("weatherInfo : " + string.Format("{0}°С", Math.Round(weatherInfo.list[0].temp.min, 1)));
                //Debug.WriteLine("weatherInfo : " + string.Format("{0}°С", Math.Round(weatherInfo.list[0].temp.max, 1)));
                //HistoryLog("weatherInfo :  " + weatherInfo.list[0].weather[0].description);
                //HistoryLog("weatherInfo : " + string.Format("{0}°С", Math.Round(weatherInfo.list[0].temp.min, 1)));
                //HistoryLog("weatherInfo : " + string.Format("{0}°С", Math.Round(weatherInfo.list[0].temp.max, 1)));



                DateTime startTime = DateTime.Now;

                // Db
                DbConnect db = new DbConnect();
                List<DialogList> dlg = db.SelectInitDialog();
                Debug.WriteLine("!!!!!!!!!!! : " + dlg[0].dlgId);
				HistoryLog("!!!!!!!!!!! : " + dlg[0].dlgId);

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

                        List<ButtonList> btn = new List<ButtonList>();
                        if (activity.ChannelId == "facebook")
                        {
                            btn = db.SelectBtn(card[i].dlgId, card[i].cardId, "Y");
                        }
                        else
                        {
                            btn = db.SelectBtn(card[i].dlgId, card[i].cardId, "N");
                        }
                        //List<ButtonList> btn = db.SelectBtn(card[i].dlgId, card[i].cardId);
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
                                //Image = plThumnail,
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
				HistoryLog("프로그램 수행시간 : {0}/ms" + ((endTime - startTime).Milliseconds));

            }
            else if (activity.Type == ActivityTypes.Message)
            {
                //TEST FACEBOOK
                //activity.ChannelId = "facebook";
                //Debug.WriteLine("eventURL : " + eventURL);
                HistoryLog("[logic start] ==>> userID :: [" + activity.Conversation.Id + "]");

                JObject Luis = new JObject();
                string entitiesStr = "";
                string testDriveWhereStr = "";
                string priceWhereStr = "";

                string entitiesValueStr = "";
                string colorStr = "";
                string carOptionStr = "";
                string luis_intent = "";
                string luis_intent_score = "";
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
                string orgENGMent_history = "";
                DateTime startTime = DateTime.Now;
                long unixTime = ((DateTimeOffset)startTime).ToUnixTimeSeconds();
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                orgMent = activity.Text;

                //페이스북 위치 값 저장
                var facebooklocation = activity.Entities?.Where(t => t.Type == "Place").Select(t => t.GetAs<Place>()).FirstOrDefault();
                if (facebooklocation != null)
                {
                    try
                    {
                        var geo = (facebooklocation.Geo as JObject)?.ToObject<GeoCoordinates>();
                        if (geo != null)
                        {
                            HistoryLog("[activity.Text]2 ==>> activity.Text :: location [" + activity.Text + "]");
                            HistoryLog("[logic start] ==>> userID :: location [" + geo.Longitude + " " + geo.Latitude + "]");
                            orgMent = "current location:" + geo.Longitude + ":" + geo.Latitude;
                        }
                    }
                    catch (Exception ex)
                    {
                        HistoryLog("[logic start] ==>> userID :: location error [" + activity.Conversation.Id + "]");
                    }
                }

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

                    if (luis_intent.Equals("Quote"))
                    {
                        gubunVal = "Quote";
                    }
                    else if (luis_intent.Equals("Branch") || luis_intent.Equals("Test drive") || luis_intent.Equals("Test drive car color"))
                    {
                        gubunVal = "Test drive";
                    }
                    else
                    {
                        gubunVal = "OTHER";
                    }
                    int dbResult = db.insertUserQuery(orgMent, orgMent, luis_intent, entitiesStr, luis_intent_score, luisID, 'H', testDriveWhereStr, "", priceWhereStr, gubunVal);
                    //int dbResult = db.insertUserQuery(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), luis_intent, entitiesStr, luisID, 'H', testDriveWhereStr, "", priceWhereStr, gubunVal);
                    HistoryLog("INSERT QUERY RESULT : " + dbResult.ToString());
                    Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());


                    DateTime endTime = DateTime.Now;
                    Debug.WriteLine("USER NUMBER : " + activity.Conversation.Id);
                    Debug.WriteLine("CUSTOMMER COMMENT KOREAN : " + activity.Text);
                    Debug.WriteLine("CUSTOMMER COMMENT ENGLISH : " + translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));

                    Debug.WriteLine("CHATBOT_COMMENT_CODE : " + "dlg.bannedword");
                    Debug.WriteLine("CHANNEL_ID : " + activity.ChannelId);
                    Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - startTime).Milliseconds));

                    HistoryLog("USER NUMBER : " + activity.Conversation.Id);
                    HistoryLog("CUSTOMMER COMMENT KOREAN : " + activity.Text);
                    HistoryLog("CUSTOMMER COMMENT ENGLISH : " + translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));

                    HistoryLog("CHATBOT_COMMENT_CODE : " + "dlg.bannedword");
                    HistoryLog("CHANNEL_ID : " + activity.ChannelId);
                    HistoryLog("프로그램 수행시간 : {0}/ms" + ((endTime - startTime).Milliseconds));

                    inserResult = db.insertHistory(activity.Conversation.Id, activity.Text, translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), "dlg.bannedword", activity.ChannelId, ((endTime - startTime).Milliseconds));
                    if (inserResult > 0)
                    {
                        Debug.WriteLine("HISTORY RESULT SUCCESS");
                        HistoryLog("HISTORY RESULT SUCCESS");
                    }
                    else
                    {
                        Debug.WriteLine("HISTORY RESULT SUCCESS");
                        HistoryLog("HISTORY RESULT FAIL");
                    }
                    sorryMessageCnt = 0;
                    //});
                    HistoryLog("[ DIALOG ] ==>> userID :: [ " + activity.Conversation.Id + " ]       message :: [ " + orgMent + " ]       date :: [ " + DateTime.Now + " ]");
                    response = Request.CreateResponse(HttpStatusCode.OK);
                    sorryMessageCnt = 0;
                    return response;

                }


                else if (orgMent.Contains(" 트림 외장색상"))
                //else if (orgMent.Substring(orgMent.Length - 8).Equals(" 트림 외장색상"))
                {
                    HistoryLog("외장컬러 보여주자");
                    Debug.WriteLine("외장컬러 보여주자");

                    orgMent = orgMent.Replace(" 트림 외장색상", "");

                    HistoryLog("orgMent : " + orgMent);
                    Debug.WriteLine("orgMent : " + orgMent);

                    List<CarExColorList> CarExColorList = db.SelectCarExColorList(orgMent);
                    //exColor = CarExColorList[0].model.ToString();
                    //HistoryLog("CarExColorList.Count : " + CarExColorList.Count);

                    Activity reply_ment = activity.CreateReply();
                    reply_ment.Recipient = activity.From;
                    reply_ment.Type = "message";
                    //reply_ment.Text = "코나 " + orgMent.Substring(0, orgMent.Length - 1) + "의 외장색상을 보여드릴게요";
                    reply_ment.Text = "코나 " + orgMent + "의 외장색상을 보여드릴게요";
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
                        //trimNM = trimNM.Replace("7단 ", "");
                        //trimNM = trimNM.Replace("DCT ", "");
                        //Debug.WriteLine("URL : " + domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd + ".jpg");

                        if (activity.ChannelId != "facebook")
                        {
                            Debug.WriteLine("외장 색상 NO FACEBOOK");
                            HistoryLog("외장 색상 NO FACEBOOK");
                            reply_exColor.Attachments.Add(
                            GetHeroCard_info(
                                trimNM,
                                //CarExColorList[td].trimColorNm,
                                "추가 금액 : " + string.Format("{0}", CarExColorList[td].exColorPrice.ToString("n0")) + "원",
                                "",
                                new CardImage(url: domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd + ".jpg"), "turn", CarExColorList[td].trimColorCd)//,
                                //new CardImage(url: HttpUtility.UrlEncode(domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd + ".jpg").Replace("+", "%20")), "turn", CarExColorList[td].trimColorCd)//,
                            );
                        }
                        else
                        {
                            Debug.WriteLine("외장 색상 FACEBOOK");
                            HistoryLog("외장 색상 FACEBOOK");

                            string exImgUrl = domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd.Replace(" ", "") + ".jpg";

                            //HistoryLog("URL : " + HttpUtility.UrlEncode(exImgUrl).Replace("+", "%20"));
                            
                            reply_exColor.Attachments.Add(
                            GetHeroCard_info(
                                trimNM,
                                //CarExColorList[td].trimColorNm,
                                "추가 금액 : " + string.Format("{0}", CarExColorList[td].exColorPrice.ToString("n0")) + "원",
                                "",
                                new CardImage(url: domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd.Replace(" ", "%20") + ".jpg"), "turn", CarExColorList[td].trimColorCd)//,
                                //new CardImage(url: HttpUtility.UrlEncode(domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd + ".jpg").Replace("+", "%20")), "turn", CarExColorList[td].trimColorCd)//,
                                //new CardImage(url: HttpUtility.UrlEncode(exImgUrl).Replace("+", "%20")), "turn", CarExColorList[td].trimColorCd)//,
                            );
                        }

                    }

                    var reply1 = await connector.Conversations.SendToConversationAsync(reply_exColor);

                    DateTime endTime = DateTime.Now;
                    HistoryLog("USER NUMBER : " + activity.Conversation.Id);
                    HistoryLog("CUSTOMMER COMMENT KOREAN : " + activity.Text);
                    HistoryLog("CUSTOMMER COMMENT ENGLISH : " + "");

                    HistoryLog("CHATBOT_COMMENT_CODE : " + "dlg.noluis.price.exteriorColor");
                    HistoryLog("CHANNEL_ID : " + activity.ChannelId);
                    HistoryLog("프로그램 수행시간 : {0}/ms" + ((endTime - startTime).Milliseconds));

                    Debug.WriteLine("USER NUMBER : " + activity.Conversation.Id);
                    Debug.WriteLine("CUSTOMMER COMMENT KOREAN : " + activity.Text);
                    Debug.WriteLine("CUSTOMMER COMMENT ENGLISH : " + "");

                    Debug.WriteLine("CHATBOT_COMMENT_CODE : " + "dlg.noluis.price.exteriorColor");
                    Debug.WriteLine("CHANNEL_ID : " + activity.ChannelId);
                    Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - startTime).Milliseconds));

                    inserResult = db.insertHistory(activity.Conversation.Id, activity.Text, translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), "dlg.noluis.price.exteriorColor", activity.ChannelId, ((endTime - startTime).Milliseconds));
                    if (inserResult > 0)
                    {
                        HistoryLog("HISTORY RESULT SUCCESS");
                        Debug.WriteLine("HISTORY RESULT SUCCESS");
                    }
                    else
                    {
                        HistoryLog("HISTORY RESULT FAIL");
                        Debug.WriteLine("HISTORY RESULT FAIL");
                    }

                    response = Request.CreateResponse(HttpStatusCode.OK);
                    sorryMessageCnt = 0;
                    return response;
                }
                else if (orgMent.Contains(" 트림 내장색상"))
                //else if (orgMent.Substring(orgMent.Length - 8).Equals(" 트림 내장색상"))
                {
                    HistoryLog("내장색상 보여주자");
                    Debug.WriteLine("내장색상 보여주자");

                    orgMent = orgMent.Replace(" 트림 내장색상", "");

                    HistoryLog("orgMent : " + orgMent);
                    Debug.WriteLine("orgMent : " + orgMent);


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
                        //trimNM = trimNM.Replace("7단 ", "");
                        //trimNM = trimNM.Replace("DCT ", "");

                        if (trimNM.Equals("애시드 옐로우"))
                        {
                            trimNM = "블랙내장/시트 + 애쉬드 옐로우 칼라 포인트";
                        }
                        if (trimNM.Equals("레드"))
                        {
                            trimNM = "블랙내장/시트 + 레드 칼라 포인트";
                        }

                        reply_inColor.Attachments.Add(
                        GetHeroCard_info(
                            trimNM,
                            //CarInColorList[td].internalColorNm,
                            "추가 금액 : " + string.Format("{0}", CarInColorList[td].inColorPrice.ToString("n0")) + "원",
                            "",
                            //new CardImage(url: "D:\\bob\\01.project\\05.현대자동차 ChatBot\\01.자료\\20170710\\가격 이미지\\가격 이미지\\interior\\" + CarInColorList[td].internalColorCd + ".jpg"),
                            new CardImage(url: domainURL+"/assets/images/price/interior/" + CarInColorList[td].internalColorCd + ".jpg"), "img", "")//,
                                                                                                                                                                     //new CardAction(ActionTypes.OpenUrl, "견적 바로가기", value: domainURL+"/assets/images/price/interior/" + CarInColorList[td].internalColorCd + ".jpg"), "", "")
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

                    HistoryLog("USER NUMBER : " + activity.Conversation.Id);
                    HistoryLog("CUSTOMMER COMMENT KOREAN : " + activity.Text);
                    HistoryLog("CUSTOMMER COMMENT ENGLISH : " + "");

                    HistoryLog("CHATBOT_COMMENT_CODE : " + "dlg.noluis.price.interiorColor");
                    HistoryLog("CHANNEL_ID : " + activity.ChannelId);
                    HistoryLog("프로그램 수행시간 : {0}/ms" + ((endTime - startTime).Milliseconds));

                    inserResult = db.insertHistory(activity.Conversation.Id, activity.Text, translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), "dlg.noluis.price.interiorColor", activity.ChannelId, ((endTime - startTime).Milliseconds));
                    if (inserResult > 0)
                    {
                        Debug.WriteLine("HISTORY RESULT SUCCESS");
                        HistoryLog("HISTORY RESULT SUCCESS");
                    }
                    else
                    {
                        Debug.WriteLine("HISTORY RESULT FAIL");
                        HistoryLog("HISTORY RESULT FAIL");
                    }

                    response = Request.CreateResponse(HttpStatusCode.OK);
                    sorryMessageCnt = 0;
                    return response;
                }
                else if (orgMent.Contains(" 트림 옵션보기"))
                {
                    Debug.WriteLine("옵션 보여주자");
                    HistoryLog("옵션 보여주자");

                    orgMent = orgMent.Replace(" 트림 옵션보기", "");

                    Debug.WriteLine("orgMent : " + orgMent);
                    HistoryLog("orgMent : " + orgMent);

                    List<CarOptionList> CarOptionList = db.SelectCarOptionList(orgMent);

                    Activity reply_ment = activity.CreateReply();
                    reply_ment.Recipient = activity.From;
                    reply_ment.Type = "message";
                    reply_ment.Text = "코나 " + orgMent + "의 추가옵션을 보여드릴게요";
                    //reply_ment.Text = "코나 " + CarOptionList[0].model.Replace(" ", "") + "의 추가옵션을 보여드릴게요";
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
                        //trimNM = trimNM.Replace("7단 ", "");
                        //trimNM = trimNM.Replace("DCT ", "");

                        //HistoryLog("CarOptionList[td].optNm : " + CarOptionList[td].optNm);

                        translateInfo = await getTranslate(CarOptionList[td].optNm);

                        //HistoryLog("CarOptionList[td].optNm : translate " + translateInfo.data.translations[0].translatedText);
                        
                        reply_option.Attachments.Add(
                        GetHeroCard_info(
                            trimNM,
                            //CarOptionList[td].optNm,
                            "추가 금액 : " + string.Format("{0}", CarOptionList[td].optPrice.ToString("n0")) + "원",
                            "",
                            //new CardImage(url: "D:\\bob\\01.project\\05.현대자동차 ChatBot\\01.자료\\20170710\\가격 이미지\\가격 이미지\\option\\" + translateInfo.data.translations[0].translatedText + ".jpg"),
                            new CardImage(url: domainURL+"/assets/images/price/option/" + translateInfo.data.translations[0].translatedText.Replace(" ", "_") + ".jpg"), "img", "")
                        //new CardAction(ActionTypes.OpenUrl, "견적 바로가기", value: domainURL+"/assets/images/price/option/" + translateInfo.data.translations[0].translatedText.Replace(" ", "_") + ".jpg"), "", "")
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

                    HistoryLog("USER NUMBER : " + activity.Conversation.Id);
                    HistoryLog("CUSTOMMER COMMENT KOREAN : " + activity.Text);
                    HistoryLog("CUSTOMMER COMMENT ENGLISH : " + "");

                    HistoryLog("CHATBOT_COMMENT_CODE : " + "dlg.noluis.price.option");
                    HistoryLog("CHANNEL_ID : " + activity.ChannelId);
                    HistoryLog("프로그램 수행시간 : {0}/ms" + ((endTime - startTime).Milliseconds));

                    inserResult = db.insertHistory(activity.Conversation.Id, activity.Text, translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), "dlg.noluis.price.option", activity.ChannelId, ((endTime - startTime).Milliseconds));
                    if (inserResult > 0)
                    {
                        Debug.WriteLine("HISTORY RESULT SUCCESS");
                        HistoryLog("HISTORY RESULT SUCCESS");
                    }
                    else
                    {
                        Debug.WriteLine("HISTORY RESULT SUCCESS");
                        HistoryLog("HISTORY RESULT FAIL");
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
                    HistoryLog("트림 보여주자");

                    orgMent = orgMent.Replace(" 트림", "");

                    Debug.WriteLine("orgMent : " + orgMent);
                    HistoryLog("orgMent : " + orgMent);

                    string trimNM = orgMent;
                    trimNM = trimNM.Replace("코나 ", "");
                    trimNM = trimNM.Replace("1.6 ", "");
                    trimNM = trimNM.Replace("터보 ", "");
                    trimNM = trimNM.Replace("오토 ", "");
                    //trimNM = trimNM.Replace("7단 ", "");
                    //trimNM = trimNM.Replace("DCT ", "");


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
                    HistoryLog("트림 보여줄래 color : " + color);
                    reply_trim_only.Attachments.Add(
                    GetHeroCard_trim(
                        trimNM,
                        //orgMent,
                        "",
                        "",
                        //new CardImage(url: "D:\\bob\\01.project\\05.현대자동차 ChatBot\\01.자료\\20170710\\가격 이미지\\가격 이미지\\trim\\" + color.Replace(" ", "") + ".jpg"),
                        new CardImage(url: domainURL+"/assets/images/price/trim/" + color.Replace(" ", "") + ".jpg"),
                        //new CardAction(ActionTypes.ImBack, "외장색상", value: orgMent + " 트림 외장색상"),
                        new CardAction(ActionTypes.ImBack, "외장색상", value: orgMent + " 트림 외장색상"),
                        new CardAction(ActionTypes.ImBack, "내장색상", value: orgMent + " 트림 내장색상"),
                        new CardAction(ActionTypes.ImBack, "옵션보기", value: orgMent + " 트림 옵션보기"),
                        //new CardAction(ActionTypes.OpenUrl, "견적 바로가기", value: "https://logon.hyundai.com/kr/quotation/main.do?carcode=RV104"))
                        new CardAction(ActionTypes.ImBack, "견적 바로가기", value: "견적 바로가기"))
                    );

                    var reply1 = await connector.Conversations.SendToConversationAsync(reply_trim_only);

                    DateTime endTime = DateTime.Now;
                    Debug.WriteLine("USER NUMBER : " + activity.Conversation.Id);
                    Debug.WriteLine("CUSTOMMER COMMENT KOREAN : " + activity.Text);
                    Debug.WriteLine("CUSTOMMER COMMENT ENGLISH : " + "");

                    Debug.WriteLine("CHATBOT_COMMENT_CODE : " + "dlg.noluis.price.onlyTrim");
                    Debug.WriteLine("CHANNEL_ID : " + activity.ChannelId);
                    Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - startTime).Milliseconds));

                    HistoryLog("USER NUMBER : " + activity.Conversation.Id);
                    HistoryLog("CUSTOMMER COMMENT KOREAN : " + activity.Text);
                    HistoryLog("CUSTOMMER COMMENT ENGLISH : " + "");

                    HistoryLog("CHATBOT_COMMENT_CODE : " + "dlg.noluis.price.onlyTrim");
                    HistoryLog("CHANNEL_ID : " + activity.ChannelId);
                    HistoryLog("프로그램 수행시간 : {0}/ms" + ((endTime - startTime).Milliseconds));

                    inserResult = db.insertHistory(activity.Conversation.Id, activity.Text, translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), "dlg.noluis.price.onlyTrim", activity.ChannelId, ((endTime - startTime).Milliseconds));
                    if (inserResult > 0)
                    {
                        Debug.WriteLine("HISTORY RESULT SUCCESS");
                        HistoryLog("HISTORY RESULT SUCCESS");
                    }
                    else
                    {
                        Debug.WriteLine("HISTORY RESULT FAIL");
                        HistoryLog("HISTORY RESULT FAIL");
                    }

                    response = Request.CreateResponse(HttpStatusCode.OK);
                    sorryMessageCnt = 0;
                    return response;
                }
                else
                {
                    orgKRMent = "";
                    orgENGMent = "";

                    HistoryLog("orgMentorgMentorgMent : " + orgMent);
                    Debug.WriteLine("orgMentorgMentorgMent : " + orgMent);
                    orgMent = orgMent.Replace("&#39;", "/'");
                    HistoryLog("orgMent : " + orgMent);
                    Debug.WriteLine("orgMent : " + orgMent);
                    translateInfo = await getTranslate(orgMent);
                    orgKRMent = Regex.Replace(orgMent, @"[^a-zA-Z0-9ㄱ-힣]", "", RegexOptions.Singleline);

                    HistoryLog("[change msg end] ==>> userID :: [" + activity.Conversation.Id + "]");
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // 추천
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    if(activity.ChannelId != "facebook")
                    {
                        //초기 추천 멘트 확인
                        translateInfo = await getTranslate(orgMent);
                        int strIndex = db.SelectedRecommendCheck(1, orgKRMent, translateInfo.data.translations[0].translatedText);

                        StateClient sc = activity.GetStateClient();
                        Debug.WriteLine("activity.ChannelId : " + activity.ChannelId);
                        BotData userData = sc.BotState.GetPrivateConversationData(activity.ChannelId, activity.Conversation.Id, activity.From.Id);
                        Debug.WriteLine("activity.ChannelId  111 : " + activity.ChannelId);
                        int recommendState = userData.GetProperty<int>("recommendState");
                        //if (orgMent.Contains("코나 추천!") || recommendState > 0)

                        Activity replyToConversation = activity.CreateReply();
                        replyToConversation.Recipient = activity.From;
                        replyToConversation.Type = "message";
                        replyToConversation.Attachments = new List<Attachment>();
                        replyToConversation.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                        if (strIndex > 0 || recommendState > 0)
                        {
                            bool convRecommendPass = true;
                            bool recommendEnd = false;
                            StringBuilder strReplyMessage = new StringBuilder();

                            //orgMent = Regex.Replace(orgMent, @"[^a-zA-Z0-9ㄱ-힣]", "", RegexOptions.Singleline);
                            
                            switch (recommendState)
                            {
                                case 0:
                                    //strReplyMessage.Append($"Kona를 주로 어떤 용도로 사용하실 계획이세요?\n\n(예) 출퇴근, 장거리 이동)");
                                    //replyToConversation.Text = "Kona를 주로 어떤 용도로 사용하실 계획이세요?\n\n(예: 출퇴근, 주말레저 이동)";

                                    replyToConversation.Attachments.Add(
                                    GetHeroCard_recommend_1(
                                        "Kona를 주로 어떤 용도를 사용하실 계획이세요?",
                                        new CardAction(ActionTypes.ImBack, "출퇴근", value: "출퇴근"),
                                        new CardAction(ActionTypes.ImBack, "주말레저", value: "주말레저"),
                                        new CardAction(ActionTypes.ImBack, "세컨카", value: "세컨카"),
                                        new CardAction(ActionTypes.ImBack, "기타 용도", value: "기타 용도"))
                                    );



                                    userData.SetProperty<int>("recommendState", 1);
                                    break;
                                case 1:
                                    //조건1 추천 멘트 확인
                                    strIndex = db.SelectedRecommendCheck(2, orgKRMent, translateInfo.data.translations[0].translatedText);
                                    if (strIndex > 0)
                                    {
                                        //strReplyMessage.Append($"가성비, 안전성, 고급사양 중 어떤 점을 중시하시나요?");
                                        //replyToConversation.Text = "가성비, 안전성, 고급사양 중 어떤 점을 중시하시나요?";

                                        replyToConversation.Attachments.Add(
                                        GetHeroCard_recommend_1(
                                            "가성비, 안전성, 고급사양 중 어떤 점을 중시하시나요?",
                                            new CardAction(ActionTypes.ImBack, "가성비", value: "가성비"),
                                            new CardAction(ActionTypes.ImBack, "안전성", value: "안전성"),
                                            new CardAction(ActionTypes.ImBack, "고급사양", value: "고급사양"),
                                            new CardAction(ActionTypes.ImBack, "기타", value: "기타"))
                                        );

                                        userData.SetProperty<int>("recommendState", 2);
                                        userData.SetProperty<String>("usage", orgMent);
                                    }
                                    //else
                                    //{
                                    //    //strReplyMessage.Append($"제가 잘 이해를 못했네요 입력하신 내용이 추천 관련된 내용인가요?");
                                    //    replyToConversation.Attachments.Add(GetHeroCard_button(
                                    //        "", "",
                                    //        "제가 잘 이해를 못했네요 입력하신 내용이 추천 관련된 내용인가요?",
                                    //        new CardAction(ActionTypes.ImBack, "예", value: "예"),
                                    //        new CardAction(ActionTypes.ImBack, "아니오", value: "아니오"))
                                    //        );
                                    //    userData.SetProperty<int>("recommendState", 3);
                                    //    userData.SetProperty<String>("usage", orgMent);
                                    //}
                                    
                                    break;
                                case 2:
                                    //조건2 추천 멘트 확인
                                    strIndex = db.SelectedRecommendCheck(3, orgKRMent, translateInfo.data.translations[0].translatedText);
                                    if (strIndex > 0)
                                    {
                                        //strReplyMessage.Append($"Kona를 이용하실 분의 연령대와 성별이 어떻게 되세요?");
                                        //replyToConversation.Text = "Kona를 이용하실 분의 연령대와 성별이 어떻게 되세요?";

                                        replyToConversation.Attachments.Add(
                                        GetHeroCard_recommend_2(
                                            "Kona를 이용하실 분의 연령대와 성별이 어떻게 되세요?",
                                            new CardAction(ActionTypes.ImBack, "20~30대 여성", value: "20~30대 여성"),
                                            new CardAction(ActionTypes.ImBack, "20~30대 남성", value: "20~30대 남성"),
                                            new CardAction(ActionTypes.ImBack, "40~50대 여성", value: "40~50대 여성"),
                                            new CardAction(ActionTypes.ImBack, "40~50대 남성", value: "40~50대 남성"),
                                            new CardAction(ActionTypes.ImBack, "기타", value: "기타"))
                                        );

                                        userData.SetProperty<int>("recommendState", 4);
                                        userData.SetProperty<String>("importance", orgMent);
                                    }
                                    //else
                                    //{
                                    //    //strReplyMessage.Append($"제가 잘 이해를 못했네요 입력하신 내용이 추천 관련된 내용인가요?");
                                    //    replyToConversation.Attachments.Add(GetHeroCard_button(
                                    //        "", "",
                                    //        "제가 잘 이해를 못했네요 입력하신 내용이 추천 관련된 내용인가요?",
                                    //        new CardAction(ActionTypes.ImBack, "예", value: "예"),
                                    //        new CardAction(ActionTypes.ImBack, "아니오", value: "아니오"))
                                    //        );
                                    //    userData.SetProperty<int>("recommendState", 5);
                                    //    userData.SetProperty<String>("importance", orgMent);
                                    //}
                                    
                                    break;
                                case 3:
                                    if (orgKRMent.Contains("예") || orgKRMent.Contains("네"))
                                    {
                                        //strReplyMessage.Append($"가성비, 안전성, 고급사양 중 어떤 점을 중시하시나요?");
                                        //replyToConversation.Text = "가성비, 안전성, 고급사양 중 어떤 점을 중시하시나요?";
                                        replyToConversation.Attachments.Add(
                                        GetHeroCard_recommend_1(
                                            "가성비, 안전성, 고급사양 중 어떤 점을 중시하시나요?",
                                            new CardAction(ActionTypes.ImBack, "가성비", value: "가성비"),
                                            new CardAction(ActionTypes.ImBack, "안전성", value: "안전성"),
                                            new CardAction(ActionTypes.ImBack, "고급사양", value: "고급사양"),
                                            new CardAction(ActionTypes.ImBack, "기타", value: "기타"))
                                        );
                                        userData.SetProperty<int>("recommendState", 2);
                                    }
                                    else
                                    {
                                        userData.SetProperty<int>("recommendState", 0);
                                        userData.SetProperty<String>("usage", "");
                                        userData.SetProperty<String>("importance", "");
                                        userData.SetProperty<String>("genderAge", "");
                                        convRecommendPass = false;
                                    }
                                    break;
                                case 4:
                                    //조건3 추천 멘트 확인
                                    strIndex = db.SelectedRecommendCheck(4, orgKRMent, translateInfo.data.translations[0].translatedText);
                                    if (strIndex > 0)
                                    {
                                        userData.SetProperty<int>("recommendState", 0);
                                        userData.SetProperty<String>("genderAge", orgMent);
                                        recommendEnd = true;
                                    }
                                    //else
                                    //{
                                    //    replyToConversation.Attachments.Add(GetHeroCard_button(
                                    //        "", "",
                                    //        "제가 잘 이해를 못했네요 입력하신 내용이 추천 관련된 내용인가요?",
                                    //        new CardAction(ActionTypes.ImBack, "예", value: "예"),
                                    //        new CardAction(ActionTypes.ImBack, "아니오", value: "아니오"))
                                    //        );
                                    //    userData.SetProperty<int>("recommendState", 6);
                                    //    userData.SetProperty<String>("genderAge", orgMent);
                                    //}
                                    break;
                                case 5:
                                    if (orgKRMent.Contains("예") || orgKRMent.Contains("네"))
                                    {
                                        //strReplyMessage.Append($"Kona를 이용하실 분의 연령대와 성별이 어떻게 되세요?");
                                        //replyToConversation.Text = "Kona를 이용하실 분의 연령대와 성별이 어떻게 되세요?";

                                        replyToConversation.Attachments.Add(
                                        GetHeroCard_recommend_2(
                                            "Kona를 이용하실 분의 연령대와 성별이 어떻게 되세요?",
                                            new CardAction(ActionTypes.ImBack, "20~30대 여성", value: "20~30대 여성"),
                                            new CardAction(ActionTypes.ImBack, "20~30대 남성", value: "20~30대 남성"),
                                            new CardAction(ActionTypes.ImBack, "40~50대 여성", value: "40~50대 여성"),
                                            new CardAction(ActionTypes.ImBack, "40~50대 남성", value: "40~50대 남성"),
                                            new CardAction(ActionTypes.ImBack, "기타", value: "기타"))
                                        );

                                        userData.SetProperty<int>("recommendState", 4);
                                    }
                                    else
                                    {
                                        userData.SetProperty<int>("recommendState", 0);
                                        userData.SetProperty<String>("usage", "");
                                        userData.SetProperty<String>("importance", "");
                                        userData.SetProperty<String>("genderAge", "");
                                        convRecommendPass = false;
                                    }
                                    break;
                                case 6:
                                    if (orgKRMent.Contains("예") || orgKRMent.Contains("네"))
                                    {
                                        recommendEnd = true;
                                        userData.SetProperty<int>("recommendState", 0);
                                    }
                                    else
                                    {
                                        userData.SetProperty<int>("recommendState", 0);
                                        userData.SetProperty<String>("usage", "");
                                        userData.SetProperty<String>("importance", "");
                                        userData.SetProperty<String>("genderAge", "");
                                        convRecommendPass = false;
                                    }
                                    break;
                                default:
                                    break;
                            }
                            DateTime endTime = DateTime.Now;
                            if (strIndex == 0)
                            {
                                if (orgKRMent.Contains("예") || orgKRMent.Contains("네") || orgKRMent.Contains("아니오"))
                                {
                                    inserResult = db.insertHistory(activity.Conversation.Id, orgKRMent, "", "recommend", activity.ChannelId, ((endTime - startTime).Milliseconds));
                                } else
                                {
                                    inserResult = db.insertHistory(activity.Conversation.Id, "기타", "", "recommend", activity.ChannelId, ((endTime - startTime).Milliseconds));
                                }
                            }else
                            {
                                inserResult = db.insertHistory(activity.Conversation.Id, orgMent, "", "recommend", activity.ChannelId, ((endTime - startTime).Milliseconds));
                            }

                            if (recommendEnd)
                            {
                                List<RecommendList> RecommendList = db.SelectedRecommendList(userData.GetProperty<String>("usage"), userData.GetProperty<String>("importance"), userData.GetProperty<String>("genderAge"));
                                RecommendList recommend = new RecommendList();
                                
                                for (var i = 0; i < RecommendList.Count; i++)
                                {
                                    string main_color_view = "";
                                    string main_color_view_nm = "";

                                    if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_1))
                                    {
                                        main_color_view += domainURL+"/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_1 + "/00001.jpg" + "@";
                                        main_color_view_nm += RecommendList[i].MAIN_COLOR_VIEW_NM1 + "@";
                                    };

                                    if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_2))
                                    {
                                        main_color_view += domainURL+"/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_2 + "/00001.jpg" + "@";
                                        main_color_view_nm += RecommendList[i].MAIN_COLOR_VIEW_NM2 + "@";
                                    };

                                    if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_3))
                                    {
                                        main_color_view += domainURL+"/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_3 + "/00001.jpg" + "@";
                                        main_color_view_nm += RecommendList[i].MAIN_COLOR_VIEW_NM3 + "@";
                                    };

                                    if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_4))
                                    {
                                        main_color_view += domainURL+"/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_4 + "/00001.jpg" + "@";
                                        main_color_view_nm += RecommendList[i].MAIN_COLOR_VIEW_NM4 + "@";
                                    };

                                    if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_5))
                                    {
                                        main_color_view += domainURL+"/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_5 + "/00001.jpg" + "@";
                                        main_color_view_nm += RecommendList[i].MAIN_COLOR_VIEW_NM5 + "@";
                                    };

                                    if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_6))
                                    {
                                        main_color_view += domainURL+"/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_6 + "/00001.jpg" + "@";
                                        main_color_view_nm += RecommendList[i].MAIN_COLOR_VIEW_NM6 + "@";
                                    };

                                    if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_7))
                                    {
                                        main_color_view += domainURL+"/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_7 + "/00001.jpg";
                                        main_color_view_nm += RecommendList[i].MAIN_COLOR_VIEW_NM7 + "@";
                                    };

                                    main_color_view = main_color_view.TrimEnd('@');
                                    main_color_view_nm = main_color_view_nm.TrimEnd('@');
                                     
                                    Debug.Write("main_color_view = " + main_color_view);
                                    Debug.Write("main_color_view_nm = " + main_color_view_nm);

                                    replyToConversation.Attachments.Add(
                                    GetHeroCard_button(
                                    "trim",
                                    RecommendList[i].TRIM_DETAIL + "|" + "가격: " + RecommendList[i].TRIM_DETAIL_PRICE + "|" +
                                    //domainURL+"/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_1 + "/00001.jpg" + "|" +
                                    main_color_view + "|" +
                                    RecommendList[i].OPTION_1_IMG_URL + "|" +
                                    RecommendList[i].OPTION_1 + "|" +
                                    RecommendList[i].OPTION_2_IMG_URL + "|" +
                                    RecommendList[i].OPTION_2 + "|" +
                                    RecommendList[i].OPTION_3_IMG_URL + "|" +
                                    RecommendList[i].OPTION_3 + "|" +
                                    RecommendList[i].OPTION_4_IMG_URL + "|" +
                                    RecommendList[i].OPTION_4 + "|" +
                                    RecommendList[i].OPTION_5_IMG_URL + "|" +
                                    RecommendList[i].OPTION_5 + "|" +
                                    main_color_view_nm
                                    ,
                                    "고객님께서 선택한 결과에 따라 차량을 추천해 드릴게요",
                                    new CardAction(ActionTypes.ImBack, "다시 선택 하기", value: "나에게 맞는 모델을 추천해줘"),
                                    new CardAction(ActionTypes.ImBack, "차량 추천 결과 보기", value: "차량 추천 결과 보기")
                                    )
                                    );

                                }

                                userData.SetProperty<int>("recommendState", 0);
                                sc.BotState.SetPrivateConversationData(activity.ChannelId, activity.Conversation.Id, activity.From.Id, userData);
                                await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                                response = Request.CreateResponse(HttpStatusCode.OK);
                                return response;
                            }
                            //else
                            //{
                            //    replyToConversation = activity.CreateReply(strReplyMessage.ToString());
                            //}
                            sc.BotState.SetPrivateConversationData(activity.ChannelId, activity.Conversation.Id, activity.From.Id, userData);

                            //초기 메뉴로 보내기
                            if (orgKRMent.Contains("아니오") || orgKRMent.Contains("아니") || orgKRMent.Contains("노노") || orgKRMent.ToLower().Contains("no"))
                            {
                                orgMent = "앨리스";
                                orgKRMent = "앨리스";
                            }
                            else
                            {
                                await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                            }

                            if (convRecommendPass)
                            {
                                response = Request.CreateResponse(HttpStatusCode.OK);
                                return response;
                            }
                        }
                    }

                    recommendChk = false;
                    Debug.WriteLine(orgMent + "orgMentorgMentorgMentorgMentorgMent22 : " + orgKRMent);
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // 한글 , 영어 질문 cash table 체크
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //등록된 쿼리 없을경우
                    if (db.SelectKoreanCashCheck(orgKRMent).Length == 0)
                    {

                        for (int n = 0; n < Regex.Split(orgMent, " ").Length; n++)
                        {
                            string chgMsg = db.SelectChgMsg(Regex.Split(orgMent, " ")[n]);
                            if (!string.IsNullOrEmpty(chgMsg))
                            {
                                orgMent = orgMent.Replace(Regex.Split(orgMent, " ")[n], chgMsg);
                            }
                        }



                        orgKRMent1 = Regex.Replace(orgMent, @"[^a-zA-Z0-9ㄱ-힣-\s]", "", RegexOptions.Singleline);

                        translateInfo = await getTranslate(orgKRMent1);
                        
                        orgENGMent = Regex.Replace(translateInfo.data.translations[0].translatedText, @"[^a-zA-Z0-9가-힣-\s-&#39;]", "", RegexOptions.Singleline);

                        orgENGMent = orgENGMent.Replace("&#39;", "'");

                        //한글 쿼리 없고 영어 쿼리가 있을 경우
                        if (db.SelectEnglishCashCheck(orgENGMent).Length > 0)
                        {
                            //translateInfo = await getTranslate(orgMent);

                            
                            Debug.WriteLine("!!!!!!!!!!!!!! : " + translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));
                            for (int n = 0; n < Regex.Split(orgMent, " ").Length; n++)
                            {
                                string chgMsg = db.SelectChgMsg(Regex.Split(orgMent, " ")[n]);
                                if (!string.IsNullOrEmpty(chgMsg))
                                {
                                    orgMent = orgMent.Replace(Regex.Split(orgMent, " ")[n], chgMsg);
                                }
                            }
                            //orgMent = Regex.Replace(orgMent, @"[^a-zA-Z0-9ㄱ-힣-\s]", "", RegexOptions.Singleline);
                            translateInfo = await getTranslate(orgMent);

                            //orgENGMent = translateInfo.data.translations[0].translatedText.Replace("&#39;", "'");

                            orgENGMent_history = Regex.Replace(translateInfo.data.translations[0].translatedText, @"[^a-zA-Z0-9ㄱ-힣-\s-&#39;]", "", RegexOptions.Singleline);
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
                            luis_intent_score = (string)Luis["intent_score"];
                            gubunVal = (string)Luis["car_option"];
                            //string gubunVal = "";
                            //string entitiesValueStr = "";
                        }
                        //한글 쿼리 없고 영어 쿼리도 없을 경우
                        else
                        {
                            //translateInfo = await getTranslate(orgMent);
                            Debug.WriteLine("111111111111111111111111111111");
                            Debug.WriteLine("!!!!!!!!!!!!!! : " + translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));

                            for (int n = 0; n < Regex.Split(orgMent, " ").Length; n++)
                            {
                                string chgMsg = db.SelectChgMsg(Regex.Split(orgMent, " ")[n]);
                                if (!string.IsNullOrEmpty(chgMsg))
                                {
                                    orgMent = orgMent.Replace(Regex.Split(orgMent, " ")[n], chgMsg);
                                }
                            }
                            //orgMent = Regex.Replace(orgMent, @"[^a-zA-Z0-9ㄱ-힣-\s]", "", RegexOptions.Singleline);
                            translateInfo = await getTranslate(orgMent);

                            //orgENGMent = translateInfo.data.translations[0].translatedText.Replace("&#39;", "'");

                            orgENGMent_history = Regex.Replace(translateInfo.data.translations[0].translatedText, @"[^a-zA-Z0-9ㄱ-힣-\s-&#39;]", "", RegexOptions.Singleline);


                            HistoryLog("[cash check false] ==>> userID :: ["+ activity.Conversation.Id + "]" );

                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            // For Query Analysis
                            luisID = 0;
                            // Try to find dialogue from log history first before checking LUIS
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            //if (string.IsNullOrEmpty(orgENGMent))
                            //{
                            //    Luis = db.SelectQueryAnalysisKor(orgKRMent);
                            //}
                            //else
                            //{
                            //    Luis = db.SelectQueryAnalysis(orgENGMent);
                            //}


                            Luis = db.SelectQueryAnalysis(orgENGMent);
                            entitiesStr = (string)Luis["entities"];

                            testDriveWhereStr = (string)Luis["test_driveWhere"];
                            priceWhereStr = (string)Luis["car_priceWhere"];

                            entitiesValueStr = (string)Luis["test_driveWhere"];

                            colorStr = (string)Luis["car_color"];
                            carOptionStr = (string)Luis["car_option"];
                            luis_intent = (string)Luis["intents"][0]["intent"];
                            luis_intent_score = (string)Luis["intent_score"];
                            gubunVal = (string)Luis["car_option"];
                            //string gubunVal = "";
                            //string entitiesValueStr = "";
                        }

                    }
                    //한글 쿼리 있을경우
                    else
                    {
                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // For Query Analysis
                        luisID = 0;

                        // Try to find dialogue from log history first before checking LUIS
                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        

                        orgENGMent = db.SelectKoreanCashCheck1(orgKRMent);

                        if (string.IsNullOrEmpty(orgENGMent))
                        {
                            Luis = db.SelectQueryAnalysisKor(orgKRMent);
                        }
                        else
                        {
                            Luis = db.SelectQueryAnalysis(orgENGMent);
                        }

                        //Luis = db.SelectQueryAnalysis(orgENGMent);
                        entitiesStr = (string)Luis["entities"];

                        testDriveWhereStr = (string)Luis["test_driveWhere"];
                        priceWhereStr = (string)Luis["car_priceWhere"];

                        entitiesValueStr = (string)Luis["test_driveWhere"];

                        colorStr = (string)Luis["car_color"];
                        carOptionStr = (string)Luis["car_option"];
                        luis_intent = (string)Luis["intents"][0]["intent"];
                        luis_intent_score = (string)Luis["intent_score"];
                        gubunVal = (string)Luis["car_option"];


                        for (int n = 0; n < Regex.Split(orgMent, " ").Length; n++)
                        {
                            string chgMsg = db.SelectChgMsg(Regex.Split(orgMent, " ")[n]);
                            if (!string.IsNullOrEmpty(chgMsg))
                            {
                                orgMent = orgMent.Replace(Regex.Split(orgMent, " ")[n], chgMsg);
                            }
                        }
                        //orgMent = Regex.Replace(orgMent, @"[^a-zA-Z0-9ㄱ-힣-\s]", "", RegexOptions.Singleline);
                        translateInfo = await getTranslate(orgMent);

                        //orgENGMent = translateInfo.data.translations[0].translatedText.Replace("&#39;", "'");

                        orgENGMent_history = Regex.Replace(translateInfo.data.translations[0].translatedText, @"[^a-zA-Z0-9ㄱ-힣-\s-&#39;]", "", RegexOptions.Singleline);
                        orgENGMent_history = orgENGMent_history.Replace("&#39;", "'");
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
                        //orgMent = Regex.Replace(orgMent, @"[^a-zA-Z0-9ㄱ-힣-\s]", "", RegexOptions.Singleline);
                        translateInfo = await getTranslate(orgMent);

                        //orgENGMent = translateInfo.data.translations[0].translatedText.Replace("&#39;", "'");

                        orgENGMent_history = Regex.Replace(translateInfo.data.translations[0].translatedText, @"[^a-zA-Z0-9ㄱ-힣-\s-&#39;]", "", RegexOptions.Singleline);

                        orgENGMent_history = orgENGMent_history.Replace("&#39;", "'");

                        //Luis = await GetIntentFromKonaBotLUIS(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));
                        Luis = await GetIntentFromKonaBotLUIS(orgENGMent_history);

                        luisID = 1; //Query Analysis
                                    //Debug.WriteLine("Luis.entities.Length : " + Luis.entities.Length);

						HistoryLog("[luis request end] ==>> userID :: ["+ activity.Conversation.Id + "]" );

                        //luis 분기
                        try
                        {
                            HistoryLog("score : " + (float)Luis["intents"][0]["score"]);
                            HistoryLog("score : " + Luis["entities"].Count());

                            Debug.WriteLine("score : " + (float)Luis["intents"][0]["score"]);
                            Debug.WriteLine("score : " + Luis["entities"].Count());

                            LuisValue = db.LuisResult(Luis.ToString());

                            gubunVal    = LuisValue[0].val;
                            luis_intent = LuisValue[0].intentValue;
                            entitiesStr = LuisValue[0].entityValue;
                            luis_intent_score = LuisValue[0].intentScore;
                            entitiesValueStr = LuisValue[0].entitieWhereValue;
                            testDriveWhereStr = LuisValue[0].testDriveWhereValue;
                            priceWhereStr = LuisValue[0].carPriceWhereValue;

                            HistoryLog("[luis result db end] ==>> userID :: ["+ activity.Conversation.Id + "]" );

                            
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.ToString());
                            await Conversation.SendAsync(activity, () => new RootDialog(luis_intent, entitiesStr, startTime, orgKRMent, orgENGMent));
                            response = Request.CreateResponse(HttpStatusCode.OK);
                            return response;
                        }
                        
                    }
                    try
                    {
                        List<DialogList> dlg = new List<DialogList>();

                        searchLuisIntent = luis_intent;
                        searchLuisEntities = entitiesStr;

                        List<LuisList> LuisDialogID = db.SelectLuis(luis_intent, entitiesStr);
                        


                        String addressStr = "";
                        Activity replyToConversation = activity.CreateReply();
                        Activity replyforfacebook = activity.CreateReply();

                        for (int k = 0; k < LuisDialogID.Count; k++)
                        {
                            replyToConversation.Recipient = activity.From;
                            replyToConversation.Type = "message";
                            replyToConversation.Attachments = new List<Attachment>();
                            replyToConversation.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                            //if(LuisDialogID[k].dlgId == 5162)
                            //{
                            //    dlg = db.SelectInitDialog();
                            //}
                            //else
                            //{ 
                                dlg = db.SelectDialog(LuisDialogID[k].dlgId);
                            //}
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
                            var entitiesStrOld = "";
                            if (entitiesStr.Equals("branch info.near"))
                            {
                                entitiesStrOld = "branch info.near";
                                entitiesStr = "branch info.color-specific";
                            }

                            if ((gubunVal.Equals("Test drive") )
                                && entitiesStr != "test drive" && !entitiesStr.Contains("reservation") && !entitiesStr.Contains("near")
                                && k < 1 )
                            {

                                if (dlg.Count > 0)
                                {
                                    HistoryLog("dlg[0].dlgMent : [" + dlg[0].dlgMent + "]");
                                    HistoryLog("dlg[0].dlgMent : [" + string.IsNullOrEmpty(dlg[0].dlgMent) + "]");

                                    Debug.WriteLine("dlg[0].dlgMent : [" + dlg[0].dlgMent + "]");
                                    Debug.WriteLine("dlg[0].dlgMent : [" + string.IsNullOrEmpty(dlg[0].dlgMent) + "]");

                                    if (string.IsNullOrEmpty(dlg[0].dlgMent) != true)
                                    {
                                        Activity reply = activity.CreateReply(dlg[0].dlgMent.ToString());
                                        await connector.Conversations.ReplyToActivityAsync(reply);
                                    }
                                }

                                //현재위치사용승인
                                if (entitiesStr.Contains("current location"))
                                {
                                    if(activity.ChannelId == "facebook" && facebooklocation == null)
                                    {
                                        //testDriveWhereStr = "test drive center region=seoul,current location=current location,query=Approve your current location";
                                        /////////////////////////////////////////////////////////////////////////////////
                                        //facebook location start
                                        /////////////////////////////////////////////////////////////////////////////////
                                        HistoryLog("[location test] conversation id :: [" + activity.Conversation.Id + "] start");

                                        Activity reply_option = activity.CreateReply();

                                        reply_option.ChannelData = new FacebookMessage
                                        (
                                            text: "나와 함께 당신의 위치를 공유하십시오.",
                                            quickReplies: new List<FacebookQuickReply>
                                            {
                                            // if content_type is location, title and payload are not used
                                            // see https://developers.facebook.com/docs/messenger-platform/send-api-reference/quick-replies#fields
                                            // for more information.
                                            new FacebookQuickReply(
                                                contentType: FacebookQuickReply.ContentTypes.Location,
                                                title: default(string),
                                                payload: default(string)
                                            )
                                            }
                                        );
                                        var reply_facebook = await connector.Conversations.SendToConversationAsync(reply_option);
                                        response = Request.CreateResponse(HttpStatusCode.OK);
                                        return response;
                                        /////////////////////////////////////////////////////////////////////////////////
                                        //facebook location end
                                        /////////////////////////////////////////////////////////////////////////////////
                                    }
                                    else
                                    {
                                        if (!orgMent.Contains(':'))
                                        {
                                            //첫번쨰 메세지 출력 x
                                            response = Request.CreateResponse(HttpStatusCode.OK);
                                            return response;
                                        }
                                        else
                                        {
                                            //위도경도에 따른 값 출력
                                            try
                                            {
                                                string regionStr = "";
                                                string location = orgMent;
                                                //location = location.Replace("current location=current location,query=current location:", "");
                                                //테스트용
                                                //string location = "129.0929788:35.2686635";
                                                string[] location_result = location.Split(':');
                                                regionStr = db.LocationValue(location_result[1], location_result[2]);

                                                testDriveWhereStr = "test drive center region=" + regionStr + ",current location=current location,query=Approve your current location";
                                            }
                                            catch
                                            {
                                                testDriveWhereStr = "test drive center region=seoul,current location=current location,query=Approve your current location";
                                            }
                                        }
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

                                    //await Conversation.SendAsync(activity, () => new RootDialog(luis_intent, entitiesStr, startTime, orgKRMent, orgENGMent));

                                    //response = Request.CreateResponse(HttpStatusCode.OK);
                                    //return response;

                                    priceWhereStr = "";

                                    switch (luis_intent)
                                    {
                                        case "Test drive":
                                            gubunVal = "Test drive";
                                            luis_intent = "Test drive";
                                            entitiesStr = "test drive";
                                            entitiesValueStr = "";
                                            testDriveWhereStr = "search directly=direct search,query=Direct Search";
                                            break;
                                        case "Test drive car color":
                                            gubunVal = "Test drive";
                                            luis_intent = "Test drive car color";
                                            entitiesStr = "branch info.color-specific";
                                            entitiesValueStr = "";
                                            testDriveWhereStr = "branch info=store,color-specific=colored cars,query=Store with colored cars";
                                            break;
                                        case "Branch":
                                            gubunVal = "Test drive";
                                            luis_intent = "Test drive car color";
                                            entitiesStr = "branch info.color-specific";
                                            entitiesValueStr = "";
                                            testDriveWhereStr = "branch info=store,color-specific=colored cars,query=Store with colored cars";
                                            break;
                                        default:
                                            gubunVal = "Test drive";
                                            luis_intent = "Test drive";
                                            entitiesStr = "test drive";
                                            entitiesValueStr = "";
                                            testDriveWhereStr = "search directly=direct search,query=Direct Search";
                                            break;
                                    }

                                    if (entitiesStrOld.Equals("branch info.near"))
                                    {
                                        entitiesStr = entitiesStrOld;
                                    }
                                    
                                    List<TestDriveListInit> SelectTestDriveListInit = db.SelectTestDriveListInit(testDriveWhereStr);

                                    if (SelectTestDriveListInit[0].dlgGubun == "1") {
                                        for (int td = 0; td < SelectTestDriveListInit.Count; td++)
                                        {
                                            replyToConversation.Attachments.Add(
                                            GetHeroCard_location(
                                            SelectTestDriveListInit[td].dlgStr1 + " 시승센터",
                                            "",
                                            SelectTestDriveListInit[td].dlgStr2 + " 등 총 " + SelectTestDriveListInit[td].dlgStr3 + " 곳",
                                            new CardAction(ActionTypes.ImBack, "정보보기", value: SelectTestDriveListInit[td].dlgStr1 + " 시승센터 "))
                                            );
                                        }
                                    } else {
                                        for (int td = 0; td < SelectTestDriveListInit.Count; td++)
                                        {

                                            if(activity.ChannelId == "facebook")
                                            {
                                                replyToConversation.Attachments.Add(
                                                GetHeroCard_show(
                                                "",
                                                //CarColorListDialog[td].dlgXrclCtyNM,
                                                SelectTestDriveListInit[td].dlgStr1,
                                                SelectTestDriveListInit[td].dlgStr3 + "개 매장에 전시",
                                                new CardImage(url: domainURL+"/assets/images/price/exterior/" + SelectTestDriveListInit[td].dlgStr2.Replace(" ", "%20") + ".jpg"),
                                                //new CardAction(ActionTypes.ImBack, "전시 차량 보기", value: CarColorListDialog[td].dlgXrclCtyNM + " 컬러가 있는 매장을 알려줘"))
                                                new CardAction(ActionTypes.ImBack, "전시 매장 보기", value: SelectTestDriveListInit[td].dlgStr1 + " 컬러가 있는 매장"), "", "")
                                                );
                                            }
                                            else
                                            {
                                                replyToConversation.Attachments.Add(
                                                GetHeroCard_show(
                                                "",
                                                //CarColorListDialog[td].dlgXrclCtyNM,
                                                SelectTestDriveListInit[td].dlgStr1,
                                                SelectTestDriveListInit[td].dlgStr3 + "개 매장에 전시",
                                                new CardImage(url: domainURL+"/assets/images/price/exterior/" + SelectTestDriveListInit[td].dlgStr2 + ".jpg"),
                                                //new CardAction(ActionTypes.ImBack, "전시 차량 보기", value: CarColorListDialog[td].dlgXrclCtyNM + " 컬러가 있는 매장을 알려줘"))
                                                new CardAction(ActionTypes.ImBack, "전시 매장 보기", value: SelectTestDriveListInit[td].dlgStr1 + " 컬러가 있는 매장"), "turn", SelectTestDriveListInit[td].dlgStr2)
                                                );
                                            }

                                            
                                        }

                                    }
                                                                    
                                    await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                                    if (luis_intent.Equals("Quote"))
                                    {
                                        gubunVal = "Quote";
                                    }
                                    else if (luis_intent.Equals("Branch") || luis_intent.Equals("Test drive") || luis_intent.Equals("Test drive car color"))
                                    {
                                        gubunVal = "Test drive";
                                    }
                                    else
                                    {
                                        gubunVal = "OTHER";
                                    }

                                    for (int n = 0; n < Regex.Split(orgMent, " ").Length; n++)
                                    {
                                        string chgMsg = db.SelectChgMsg(Regex.Split(orgMent, " ")[n]);
                                        if (!string.IsNullOrEmpty(chgMsg))
                                        {
                                            orgMent = orgMent.Replace(Regex.Split(orgMent, " ")[n], chgMsg);
                                        }
                                    }
                                    //orgMent = Regex.Replace(orgMent, @"[^a-zA-Z0-9ㄱ-힣-\s]", "", RegexOptions.Singleline);
                                    translateInfo = await getTranslate(orgMent);

                                    //orgENGMent = translateInfo.data.translations[0].translatedText.Replace("&#39;", "'");

                                    orgENGMent_history = Regex.Replace(translateInfo.data.translations[0].translatedText, @"[^a-zA-Z0-9ㄱ-힣-\s-&#39;]", "", RegexOptions.Singleline);

                                    //int dbResult = db.insertUserQuery(orgKRMent, orgENGMent, luis_intent, entitiesStr, luis_intent_score, luisID, 'H', testDriveWhereStr, "", priceWhereStr, gubunVal);
                                    int dbResult = db.insertUserQuery(orgKRMent, orgENGMent_history, luis_intent, entitiesStr, luis_intent_score, luisID, 'H', testDriveWhereStr, "", priceWhereStr, gubunVal);
                                    //int dbResult = db.insertUserQuery(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), luis_intent, entitiesStr, luisID, 'H', testDriveWhereStr, "", priceWhereStr, gubunVal);
                                    HistoryLog("INSERT QUERY RESULT : " + dbResult.ToString());
                                    Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());

                                    response = Request.CreateResponse(HttpStatusCode.OK);
                                    return response;
                                }

                                else if (SelectTestDriveList[0].dlgGubun.Equals("1"))
                                {
                                    // dlgStr1 = AREANM, dlgStr2 = AREALIST , dlgStr3 = AREACNT
                                    HistoryLog("case 1");
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
                                    HistoryLog("case 2");
                                    Debug.WriteLine("case 2");
                                    for (int td = 0; td < SelectTestDriveList.Count; td++)
                                    {
                                        if (activity.ChannelId == "facebook")
                                        {
                                            replyToConversation.Attachments.Add(
                                            GetHeroCard_show(
                                            "",
                                            //CarColorListDialog[td].dlgXrclCtyNM,
                                            SelectTestDriveList[td].dlgStr1,
                                            SelectTestDriveList[td].dlgStr3 + "개 매장에 전시",
                                            new CardImage(url: domainURL+"/assets/images/price/exterior/" + SelectTestDriveList[td].dlgStr2.Replace(" ", "%20") + ".jpg"),
                                            //new CardAction(ActionTypes.ImBack, "전시 차량 보기", value: CarColorListDialog[td].dlgXrclCtyNM + " 컬러가 있는 매장을 알려줘"))
                                            new CardAction(ActionTypes.ImBack, "전시 매장 보기", value: SelectTestDriveList[td].dlgStr1 + " 컬러가 있는 매장"), "", "")
                                            );
                                        }
                                        else
                                        {
                                            replyToConversation.Attachments.Add(
                                            GetHeroCard_show(
                                            "",
                                            //CarColorListDialog[td].dlgXrclCtyNM,
                                            SelectTestDriveList[td].dlgStr1,
                                            SelectTestDriveList[td].dlgStr3 + "개 매장에 전시",
                                            new CardImage(url: domainURL+"/assets/images/price/exterior/" + SelectTestDriveList[td].dlgStr2 + ".jpg"),
                                            //new CardAction(ActionTypes.ImBack, "전시 차량 보기", value: CarColorListDialog[td].dlgXrclCtyNM + " 컬러가 있는 매장을 알려줘"))
                                            new CardAction(ActionTypes.ImBack, "전시 매장 보기", value: SelectTestDriveList[td].dlgStr1 + " 컬러가 있는 매장"), "turn", SelectTestDriveList[td].dlgStr2)
                                            );
                                        }
                                    }
                                }
                                else if (SelectTestDriveList[0].dlgGubun.Equals("3"))
                                {
                                    HistoryLog("case 3");
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

                                            if(activity.ChannelId == "facebook")
                                            {
                                                replyToConversation.Attachments.Add(
                                                UserGetHeroCard_location(
                                                SelectTestDriveList[td].dlgStr1 + " 시승센터",
                                                "TEL." + SelectTestDriveList[td].dlgStr3,
                                                "(연중무휴)10-16시까지 예약 가능" + " " + SelectTestDriveList[td].dlgStr2,
                                                //new CardImage(url: "https:///bot.hyundai.com/map/" + APIExamMapGeocode.ll.lat.ToString() + "," + APIExamMapGeocode.ll.lon.ToString() + ".png"),
                                                new CardImage(url: domainURL+"/map/" + SelectTestDriveList[td].dlgStr4 + "%2C" + SelectTestDriveList[td].dlgStr5 + ".png"),
                                                SelectTestDriveList[td].dlgStr4,
                                                SelectTestDriveList[td].dlgStr5));
                                            }
                                            else
                                            {
                                                replyToConversation.Attachments.Add(
                                                UserGetHeroCard_location(
                                                SelectTestDriveList[td].dlgStr1 + " 시승센터",
                                                "TEL." + SelectTestDriveList[td].dlgStr3,
                                                "(연중무휴)10-16시까지 예약 가능" + " " + SelectTestDriveList[td].dlgStr2,
                                                //new CardImage(url: "https:///bot.hyundai.com/map/" + APIExamMapGeocode.ll.lat.ToString() + "," + APIExamMapGeocode.ll.lon.ToString() + ".png"),
                                                new CardImage(url: domainURL+"/map/" + SelectTestDriveList[td].dlgStr4 + "," + SelectTestDriveList[td].dlgStr5 + ".png"),
                                                SelectTestDriveList[td].dlgStr4,
                                                SelectTestDriveList[td].dlgStr5));
                                            }

                                            //replyToConversation.Attachments.Add(
                                            //UserGetHeroCard_location(
                                            //SelectTestDriveList[td].dlgStr1 + " 시승센터",
                                            //"TEL." + SelectTestDriveList[td].dlgStr3,
                                            //"(연중무휴)10-16시까지 예약 가능" + " " + SelectTestDriveList[td].dlgStr2,
                                            ////new CardImage(url: "https:///bot.hyundai.com/map/" + APIExamMapGeocode.ll.lat.ToString() + "," + APIExamMapGeocode.ll.lon.ToString() + ".png"),
                                            //new CardImage(url: domainURL+"/map/" + SelectTestDriveList[td].dlgStr4 + "," + SelectTestDriveList[td].dlgStr5 + ".png"),
                                            //SelectTestDriveList[td].dlgStr4,
                                            //SelectTestDriveList[td].dlgStr5));
                                        }
                                    }
                                    else if (testDriveWhereStr.Contains("test drive can") || testDriveWhereStr.Contains("test drive center"))
                                    {
                                        for (int td = 0; td < SelectTestDriveList.Count; td++)
                                        {

                                            if (activity.ChannelId == "facebook")
                                            {
                                                replyToConversation.Attachments.Add(
                                                GetHeroCard_info(
                                                SelectTestDriveList[td].dlgStr4,
                                                SelectTestDriveList[td].dlgStr7,
                                                "",
                                                new CardImage(url: domainURL+"/assets/images/price/exterior/" + SelectTestDriveList[td].dlgStr8.Replace(" ","%20") + ".jpg"), "", "")
                                                );
                                            }
                                            else
                                            {
                                                replyToConversation.Attachments.Add(
                                                GetHeroCard_info(
                                                SelectTestDriveList[td].dlgStr4,
                                                SelectTestDriveList[td].dlgStr7,
                                                "",
                                                new CardImage(url: domainURL+"/assets/images/price/exterior/" + SelectTestDriveList[td].dlgStr8 + ".jpg"), "", "")
                                                );
                                            }

                                            //replyToConversation.Attachments.Add(
                                            //GetHeroCard_info(
                                            //SelectTestDriveList[td].dlgStr4,
                                            //SelectTestDriveList[td].dlgStr7,
                                            //"",
                                            //new CardImage(url: domainURL+"/assets/images/price/exterior/" + SelectTestDriveList[td].dlgStr8 + ".jpg"), "", "")
                                            //);
                                        }
                                    }
                                    {

                                    }

                                }
                                else if (SelectTestDriveList[0].dlgGubun.Equals("4"))
                                {
                                    // dlgStr1 = CTR_NM, dlgStr2 = CTR_ADDR , dlgStr3 = CTR_PHONE, dlgStr4 = CAR_DTL_INFO, dlgStr5 = MAP_X_TN, dlgStr6 = MAP_Y_TN, dlgStr7 = XRCL_CTY_NM, dlgStr8 = TRIMCOLOR_CD
                                    HistoryLog("case 4");
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
                                    HistoryLog("case 5");
                                    HistoryLog("case 5");
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
                                    HistoryLog("case 6");
                                    HistoryLog("case 6");
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
                                    HistoryLog("case 7");
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
                                            APIExamMapGeocode.getCodeNaver(SelectTestDriveList[td].dlgStr4, SelectTestDriveList[td].dlgStr5);

                                            if (SelectTestDriveList[td].dlgStr2.Length > 40)
                                            {
                                                addressStr = SelectTestDriveList[td].dlgStr2.Substring(0, 37) + "...";
                                            }
                                            else
                                            {
                                                addressStr = SelectTestDriveList[td].dlgStr2;
                                            }


                                            if(activity.ChannelId == "facebook")
                                            {
                                                replyToConversation.Attachments.Add(
                                                UserGetHeroCard_location(
                                                SelectTestDriveList[td].dlgStr1,
                                                "TEL." + SelectTestDriveList[td].dlgStr3,
                                                SelectTestDriveList[td].dlgStr2,
                                                new CardImage(url: domainURL+"/map/" + SelectTestDriveList[td].dlgStr4 + "%2C" + SelectTestDriveList[td].dlgStr5 + ".png"),
                                                SelectTestDriveList[td].dlgStr4,
                                                SelectTestDriveList[td].dlgStr5)
                                                );
                                            }
                                            else
                                            {
                                                replyToConversation.Attachments.Add(
                                                UserGetHeroCard_location(
                                                SelectTestDriveList[td].dlgStr1,
                                                "TEL." + SelectTestDriveList[td].dlgStr3,
                                                SelectTestDriveList[td].dlgStr2,
                                                new CardImage(url: domainURL+"/map/" + SelectTestDriveList[td].dlgStr4 + "," + SelectTestDriveList[td].dlgStr5 + ".png"),
                                                SelectTestDriveList[td].dlgStr4,
                                                SelectTestDriveList[td].dlgStr5)
                                                );
                                            }

                                            //replyToConversation.Attachments.Add(
                                            //UserGetHeroCard_location(
                                            //SelectTestDriveList[td].dlgStr1,
                                            //"TEL." + SelectTestDriveList[td].dlgStr3,
                                            //SelectTestDriveList[td].dlgStr2,
                                            //new CardImage(url: domainURL+"/map/" + SelectTestDriveList[td].dlgStr4 + "," + SelectTestDriveList[td].dlgStr5 + ".png"),
                                            //SelectTestDriveList[td].dlgStr4,
                                            //SelectTestDriveList[td].dlgStr5)
                                            //);
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
                                HistoryLog("INTENT ::[ " + luis_intent + " ]    ENTITY ::[ " + entitiesStr + " ]   priceWhereStr :: [ " + priceWhereStr + " ]");
                                Debug.WriteLine("INTENT ::[ " + luis_intent + " ]    ENTITY ::[ " + entitiesStr + " ]   priceWhereStr :: [ " + priceWhereStr + " ]");

                                if (entitiesStr != "")
                                {

                                    if ((entitiesStr.Contains("car color") && entitiesStr.Contains("exterior color")) || (entitiesStr.Contains("car color") && entitiesStr.Contains("interior color")) || (entitiesStr.Contains("car color") && !entitiesStr.Contains("option")) || (entitiesStr.Contains("car color") && !entitiesStr.Contains("price")) || entitiesStr.Equals("car color") )
                                    {

                                        Debug.WriteLine("색상 질문");
                                        HistoryLog("색상 질문");

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
                                                HistoryLog("mainModelTitle : " + mainModelTitle);
                                                Debug.WriteLine("mainModelTitle : " + mainModelTitle);
                                                if (mainModelTitle.Contains("TUIX"))
                                                {
                                                    mainModelTitle = mainModelTitle + " 2WD";
                                                }
                                                HistoryLog("mainModelTitle 2 : " + mainModelTitle);
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

                                            if(priceWhereStr.Equals("car color=color"))
                                            {
                                                HistoryLog("전체 색상 질문");
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

                                                    //sorryMessageCnt++;
                                                    int sorryMessageCheck = db.SelectUserQueryErrorMessageCheck(activity.Conversation.Id);

                                                    //if (sorryMessageCnt > 1)
                                                    if (sorryMessageCheck == 1)
                                                    {
                                                        reply_err.Attachments = new List<Attachment>();
                                                        reply_err.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                                        HistoryLog(" sorryMessageCnt : " + sorryMessageCnt );
                                                        replyToConversation.Attachments.Add(
                                                        GetHeroCard_sorry(
                                                        SorryMessageList.GetSorryMessage(sorryMessageCheck),
                                                        new CardAction(ActionTypes.OpenUrl, "코나 챗봇 페이스북 바로가기", value: eventURL)
                                                        //new CardAction(ActionTypes.OpenUrl, "코나 챗봇 페이스북 바로가기", value: rootWebConfig.ConnectionStrings.ConnectionStrings[redirectEventPageURL]))
                                                        //new CardAction(ActionTypes.OpenUrl, "코나 챗봇 페이스북 바로가기", value: eventURL)
                                                    )
                                                    );
                                                    }
                                                    else
                                                    {
                                                        reply_err.Text = SorryMessageList.GetSorryMessage(sorryMessageCheck);
                                                    }
                                                    
                                                    //reply_err.Text = SorryMessageList.GetSorryMessage(++sorryMessageCnt);
                                                    await connector.Conversations.SendToConversationAsync(reply_err);

                                                    response = Request.CreateResponse(HttpStatusCode.OK);
                                                    return response;
                                                }
                                                else
                                                {
                                                    if (dlg.Count > 0)
                                                    {
                                                        HistoryLog("dlg[0].dlgMent : [" + dlg[0].dlgMent + "]");
                                                        HistoryLog("dlg[0].dlgMent : [" + string.IsNullOrEmpty(dlg[0].dlgMent) + "]");
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
                                                        //trimNM = trimNM.Replace("7단 ", "");
                                                        //trimNM = trimNM.Replace("DCT ", "");


                                                        if (activity.ChannelId != "facebook")
                                                        {
                                                            replyToConversation.Attachments.Add(
                                                            GetHeroCard_info(
                                                            trimNM,
                                                            "추가 금액 : " + string.Format("{0}", CarExColorList[td].exColorPrice.ToString("n0")) + "원",
                                                            "",
                                                            new CardImage(url: domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd + ".jpg"), "turn", CarExColorList[td].trimColorCd)
                                                            );
                                                        }
                                                        else
                                                        {

                                                            string exImgUrl = domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd.Replace(" ", "") + ".jpg";

                                                            replyToConversation.Attachments.Add(
                                                            GetHeroCard_info(
                                                            trimNM,
                                                            "추가 금액 : " + string.Format("{0}", CarExColorList[td].exColorPrice.ToString("n0")) + "원",
                                                            "",
                                                            new CardImage(url: domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd.Replace(" ", "%20") + ".jpg"), "turn", CarExColorList[td].trimColorCd)
                                                            );
                                                        }

                                                        
                                                    }
                                                }
                                            }else
                                            {
                                                HistoryLog("선택 색상 질문");
                                                Debug.WriteLine("선택 색상 질문");

                                                Activity reply_ment = activity.CreateReply();
                                                reply_ment.Recipient = activity.From;
                                                reply_ment.Type = "message";
                                                reply_ment.Text = "선택 색상을 보여드릴게요";
                                                var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                                                string choiceColor = "";


                                                choiceColor = priceWhereStr.Split('=')[1].ToString();

                                                List<CarExColorList> CarExColorList = db.SelectCarExColorSelectList(choiceColor);
                                                //데이터가 없을 때 예외 처리
                                                if (CarExColorList.Count == 0)
                                                {
                                                    //Activity reply_err = activity.CreateReply();
                                                    //reply_err.Recipient = activity.From;
                                                    //reply_err.Type = "message";
                                                    //reply_err.Text = SorryMessageList.GetSorryMessage(++sorryMessageCnt);
                                                    //await connector.Conversations.SendToConversationAsync(reply_err);

                                                    //response = Request.CreateResponse(HttpStatusCode.OK);
                                                    //return response;

                                                    Activity reply_err = activity.CreateReply();
                                                    reply_err.Recipient = activity.From;
                                                    reply_err.Type = "message";

                                                    //sorryMessageCnt++;
                                                    int sorryMessageCheck = db.SelectUserQueryErrorMessageCheck(activity.Conversation.Id);

                                                    //if (sorryMessageCnt > 1)
                                                    if (sorryMessageCheck == 1)
                                                    {
                                                        reply_err.Attachments = new List<Attachment>();
                                                        reply_err.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                                        HistoryLog(" sorryMessageCnt : " + sorryMessageCnt);
                                                        replyToConversation.Attachments.Add(
                                                        GetHeroCard_sorry(
                                                        SorryMessageList.GetSorryMessage(sorryMessageCheck),
                                                        new CardAction(ActionTypes.OpenUrl, "코나 챗봇 페이스북 바로가기", value: eventURL)
                                                        //new CardAction(ActionTypes.OpenUrl, "코나 챗봇 페이스북 바로가기", value: rootWebConfig.ConnectionStrings.ConnectionStrings[redirectEventPageURL]))
                                                        //new CardAction(ActionTypes.OpenUrl, "코나 챗봇 페이스북 바로가기", value: eventURL)
                                                    )
                                                    );
                                                    }
                                                    else
                                                    {
                                                        reply_err.Text = SorryMessageList.GetSorryMessage(sorryMessageCheck);
                                                    }

                                                    //reply_err.Text = SorryMessageList.GetSorryMessage(++sorryMessageCnt);
                                                    await connector.Conversations.SendToConversationAsync(reply_err);

                                                    response = Request.CreateResponse(HttpStatusCode.OK);
                                                    return response;


                                                }
                                                else
                                                {
                                                    if (dlg.Count > 0)
                                                    {
                                                        HistoryLog("dlg[0].dlgMent : [" + dlg[0].dlgMent + "]");
                                                        HistoryLog("dlg[0].dlgMent : [" + string.IsNullOrEmpty(dlg[0].dlgMent) + "]");
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
                                                        //trimNM = trimNM.Replace("7단 ", "");
                                                        //trimNM = trimNM.Replace("DCT ", "");


                                                        if (activity.ChannelId != "facebook")
                                                        {
                                                            replyToConversation.Attachments.Add(
                                                            GetHeroCard_info(
                                                            trimNM,
                                                            "추가 금액 : " + string.Format("{0}", CarExColorList[td].exColorPrice.ToString("n0")) + "원",
                                                            "",
                                                            new CardImage(url: domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd + ".jpg"), "turn", CarExColorList[td].trimColorCd)
                                                            );
                                                        }
                                                        else
                                                        {

                                                            string exImgUrl = domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd.Replace(" ", "") + ".jpg";

                                                            replyToConversation.Attachments.Add(
                                                            GetHeroCard_info(
                                                            trimNM,
                                                            "추가 금액 : " + string.Format("{0}", CarExColorList[td].exColorPrice.ToString("n0")) + "원",
                                                            "",
                                                            new CardImage(url: domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd.Replace(" ", "%20") + ".jpg"), "turn", CarExColorList[td].trimColorCd)
                                                            );
                                                        }


                                                        //replyToConversation.Attachments.Add(
                                                        //GetHeroCard_info(
                                                        //trimNM,
                                                        //"추가 금액 : " + string.Format("{0}", CarExColorList[td].exColorPrice.ToString("n0")) + "원",
                                                        //"",
                                                        //new CardImage(url: domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd + ".jpg"), "turn", CarExColorList[td].trimColorCd)
                                                        //);
                                                    }
                                                }
                                            }
                                        }
                                        
                                        else if (entitiesStr.Equals("car color.exterior color"))
                                        {
                                            HistoryLog("외장 전체 색상 질문");
                                            Debug.WriteLine("외장 전체 색상 질문");

                                            if(priceWhereStr.Contains("car color=color"))
                                            {
                                                List<CarExColorList> CarExColorList = db.SelectCarExColorAllList();
                                                HistoryLog("exteriorexteriorexteriorexterior");
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
                                                        HistoryLog("mainModelTitle : " + mainModelTitle);
                                                        Debug.WriteLine("mainModelTitle : " + mainModelTitle);
                                                        if (mainModelTitle.Contains("TUIX"))
                                                        {
                                                            mainModelTitle = mainModelTitle + " 2WD";
                                                        }
                                                        Debug.WriteLine("mainModelTitle 2 : " + mainModelTitle);
                                                        HistoryLog("mainModelTitle 2 : " + mainModelTitle);

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
                                                        HistoryLog("exterior color : " + CarExColorList[td].trimColorCd);
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
                                                        //trimNM = trimNM.Replace("7단 ", "");
                                                        //trimNM = trimNM.Replace("DCT ", "");

                                                        //replyToConversation.Attachments.Add(
                                                        //GetHeroCard_info(
                                                        //trimNM,
                                                        //"추가 금액 : " + string.Format("{0}", CarExColorList[td].exColorPrice.ToString("n0")) + "원",
                                                        //"",
                                                        //new CardImage(url: domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd + ".jpg"), "turn", CarExColorList[td].trimColorCd)
                                                        //);

                                                        if (activity.ChannelId != "facebook")
                                                        {
                                                            replyToConversation.Attachments.Add(
                                                            GetHeroCard_info(
                                                            trimNM,
                                                            "추가 금액 : " + string.Format("{0}", CarExColorList[td].exColorPrice.ToString("n0")) + "원",
                                                            "",
                                                            new CardImage(url: domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd + ".jpg"), "turn", CarExColorList[td].trimColorCd)
                                                            );
                                                        }
                                                        else
                                                        {

                                                            string exImgUrl = domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd.Replace(" ", "") + ".jpg";

                                                            replyToConversation.Attachments.Add(
                                                            GetHeroCard_info(
                                                            trimNM,
                                                            "추가 금액 : " + string.Format("{0}", CarExColorList[td].exColorPrice.ToString("n0")) + "원",
                                                            "",
                                                            new CardImage(url: domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd.Replace(" ", "%20") + ".jpg"), "turn", CarExColorList[td].trimColorCd)
                                                            );
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                HistoryLog("선택 색상 질문");
                                                Debug.WriteLine("선택 색상 질문");
                                                //car color=chalk white,exterior color=exterior color
                                                Activity reply_ment = activity.CreateReply();
                                                reply_ment.Recipient = activity.From;
                                                reply_ment.Type = "message";
                                                reply_ment.Text = "선택 색상을 보여드릴게요";
                                                var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                                                string choiceColor = "";


                                                choiceColor = priceWhereStr.Split('=')[1].ToString().Split(',')[0].ToString();

                                                List<CarExColorList> CarExColorList = db.SelectCarExColorSelectList(choiceColor);
                                                //데이터가 없을 때 예외 처리
                                                if (CarExColorList.Count == 0)
                                                {
                                                    Activity reply_err = activity.CreateReply();
                                                    reply_err.Recipient = activity.From;
                                                    reply_err.Type = "message";

                                                    //sorryMessageCnt++;
                                                    int sorryMessageCheck = db.SelectUserQueryErrorMessageCheck(activity.Conversation.Id);

                                                    //if (sorryMessageCnt > 1)
                                                    if (sorryMessageCheck == 1)
                                                    {
                                                        reply_err.Attachments = new List<Attachment>();
                                                        reply_err.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                                        HistoryLog(" sorryMessageCnt : " + sorryMessageCnt);
                                                        replyToConversation.Attachments.Add(
                                                        GetHeroCard_sorry(
                                                        SorryMessageList.GetSorryMessage(sorryMessageCheck),
                                                        new CardAction(ActionTypes.OpenUrl, "코나 챗봇 페이스북 바로가기", value: eventURL)
                                                        //new CardAction(ActionTypes.OpenUrl, "코나 챗봇 페이스북 바로가기", value: rootWebConfig.ConnectionStrings.ConnectionStrings[redirectEventPageURL]))
                                                        //new CardAction(ActionTypes.OpenUrl, "코나 챗봇 페이스북 바로가기", value: eventURL)
                                                    )
                                                    );
                                                    }
                                                    else
                                                    {
                                                        reply_err.Text = SorryMessageList.GetSorryMessage(sorryMessageCheck);
                                                    }

                                                    //reply_err.Text = SorryMessageList.GetSorryMessage(++sorryMessageCnt);
                                                    await connector.Conversations.SendToConversationAsync(reply_err);

                                                    response = Request.CreateResponse(HttpStatusCode.OK);
                                                    return response;
                                                }
                                                else
                                                {
                                                    if (dlg.Count > 0)
                                                    {
                                                        HistoryLog("dlg[0].dlgMent : [" + dlg[0].dlgMent + "]");
                                                        HistoryLog("dlg[0].dlgMent : [" + string.IsNullOrEmpty(dlg[0].dlgMent) + "]");
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
                                                        //trimNM = trimNM.Replace("7단 ", "");
                                                        //trimNM = trimNM.Replace("DCT ", "");

                                                        //replyToConversation.Attachments.Add(
                                                        //GetHeroCard_info(
                                                        //trimNM,
                                                        //"추가 금액 : " + string.Format("{0}", CarExColorList[td].exColorPrice.ToString("n0")) + "원",
                                                        //"",
                                                        //new CardImage(url: domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd + ".jpg"), "turn", CarExColorList[td].trimColorCd)
                                                        //);

                                                        if (activity.ChannelId != "facebook")
                                                        {
                                                            replyToConversation.Attachments.Add(
                                                            GetHeroCard_info(
                                                            trimNM,
                                                            "추가 금액 : " + string.Format("{0}", CarExColorList[td].exColorPrice.ToString("n0")) + "원",
                                                            "",
                                                            new CardImage(url: domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd + ".jpg"), "turn", CarExColorList[td].trimColorCd)
                                                            );
                                                        }
                                                        else
                                                        {

                                                            string exImgUrl = domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd.Replace(" ", "") + ".jpg";

                                                            replyToConversation.Attachments.Add(
                                                            GetHeroCard_info(
                                                            trimNM,
                                                            "추가 금액 : " + string.Format("{0}", CarExColorList[td].exColorPrice.ToString("n0")) + "원",
                                                            "",
                                                            new CardImage(url: domainURL+"/assets/images/price/exterior/" + CarExColorList[td].trimColorCd.Replace(" ", "%20") + ".jpg"), "turn", CarExColorList[td].trimColorCd)
                                                            );
                                                        }

                                                    }
                                                }
                                            }

                                            
                                            

                                        }
                                        else if (entitiesStr.Equals("car color.interior color"))
                                        {
                                            Debug.WriteLine("내장 전체 색상 질문");
                                            HistoryLog("내장 전체 색상 질문");

                                            

                                            List<CarInColorList> CarInColorList = db.SelectCarInColorAllList();
                                            Debug.WriteLine("interiorinteriorinteriorinterior");
                                            HistoryLog("interiorinteriorinteriorinterior");
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
                                                    HistoryLog("mainModelTitle : " + mainModelTitle);
                                                    if (mainModelTitle.Contains("TUIX"))
                                                    {
                                                        mainModelTitle = mainModelTitle + " 2WD";
                                                    }
                                                    Debug.WriteLine("mainModelTitle 2 : " + mainModelTitle);
                                                    HistoryLog("mainModelTitle 2 : " + mainModelTitle);

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
                                                    //trimNM = trimNM.Replace("7단 ", "");
                                                    //trimNM = trimNM.Replace("DCT ", "");

                                                    replyToConversation.Attachments.Add(
                                                    GetHeroCard_info(
                                                    trimNM,
                                                    "추가 금액 : " + string.Format("{0}", CarInColorList[td].inColorPrice.ToString("n0")) + "원",
                                                    "",
                                                    new CardImage(url: domainURL+"/assets/images/price/interior/" + CarInColorList[td].internalColorCd + ".jpg"), "img", "")
                                                    );
                                                }
                                            }

                                            
                                        }
                                        else
                                        {
                                            Debug.WriteLine("트림, 엔진, 드라이브 휠, 칼라 패키지, 튜익스 색상 질문");
                                            HistoryLog("트림, 엔진, 드라이브 휠, 칼라 패키지, 튜익스 색상 질문");
                                            string colorMent = activity.Text;
                                            //colorMent = activity.Text.Replace("옵션", "");
                                            colorMent = colorMent.Replace("가격", "");
                                            colorMent = colorMent.Replace("exterior", "외장");
                                            colorMent = colorMent.Replace("interior", "내장");
                                            colorMent = colorMent.Replace("color", "색상");
                                            //priceMent = activity.Text.Replace("price", "");

                                            int index = colorMent.IndexOf("색상") + 2;

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
                                                //trimNM = trimNM.Replace("7단 ", "");
                                                //trimNM = trimNM.Replace("DCT ", "");

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
                                                                new CardImage(url: domainURL+"/assets/images/price/trim/" + color.Replace(" ", "") + ".jpg"),
                                                                new CardAction(ActionTypes.ImBack, "외장색상", value: trimNM.Replace(" 1.6", "") + " 트림 외장색상"), "", "")
                                                                //new CardAction(ActionTypes.ImBack, "외장색상", value: trimNM.Replace(" 1.6", "") + " 외장색상"), "", "")
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
                                                                new CardImage(url: domainURL+"/assets/images/price/trim/" + color.Replace(" ", "") + ".jpg"),
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
                                                                new CardImage(url: domainURL+"/assets/images/price/trim/" + color.Replace(" ", "") + ".jpg"),
                                                                new CardAction(ActionTypes.ImBack, "외장색상", value: trimNM.Replace(" 1.6", "") + " 트림 외장색상"), "", "")
                                                            );
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                        luis_intent = gubunVal;
                                    }
                                    else if (entitiesStr.Contains("option"))
                                    {
                                        Debug.WriteLine("옵션 질문");
                                        HistoryLog("옵션 질문");
                                        if (entitiesStr.Equals("option"))
                                        {
                                            Debug.WriteLine("전체 옵션");
                                            HistoryLog("전체 옵션");

                                            

                                            List<CarOptionList> carOptionList = db.SelectOptionList(priceWhereStr);

                                            //데이터가 없을 때 예외 처리
                                            if (carOptionList.Count == 0)
                                            {
                                                
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
                                                    HistoryLog("mainModelTitle : " + mainModelTitle);
                                                    if (mainModelTitle.Contains("TUIX"))
                                                    {
                                                        mainModelTitle = mainModelTitle + " 2WD";
                                                    }
                                                    Debug.WriteLine("mainModelTitle 2 : " + mainModelTitle);
                                                    HistoryLog("mainModelTitle 2 : " + mainModelTitle);

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
                                                reply_ment.Text = "선택하신 옵션을 보여드릴게요";
                                                var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                                                for (int td = 0; td < carOptionList.Count; td++)
                                                {

                                                    translateInfo = await getTranslate(carOptionList[td].optNm);

                                                    replyToConversation.Attachments.Add(
                                                    GetHeroCard_info(
                                                    carOptionList[td].optNm,
                                                    "추가 금액 : " + string.Format("{0}", carOptionList[td].optPrice.ToString("n0")) + "원",
                                                    "",
                                                    new CardImage(url: domainURL+"/assets/images/price/option/" + (translateInfo.data.translations[0].translatedText).Replace(" ", "_") + ".jpg"), "img", "")
                                                    );
                                                }
                                            }
                                            
                                        }
                                        else
                                        {
                                            List<CarOptionList> carOptionList = db.SelectOptionList(priceWhereStr);
                                            string optionMent = activity.Text;
                                            
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
                                                    HistoryLog("mainModelTitle : " + mainModelTitle);
                                                    if (mainModelTitle.Contains("TUIX"))
                                                    {
                                                        mainModelTitle = mainModelTitle + " 2WD";
                                                    }
                                                    Debug.WriteLine("mainModelTitle 2 : " + mainModelTitle);
                                                    HistoryLog("mainModelTitle 2 : " + mainModelTitle);

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
                                                    new CardImage(url: domainURL+"/assets/images/price/option/" + (translateInfo1.data.translations[0].translatedText).Replace(" ", "_") + ".jpg"), "img", "")
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
                                        HistoryLog("견적 보여줘 질문");
                                        string mainModelTitle = "";
                                        List<CarModelList> CarModelList = db.SelectCarModelList();

                                        //데이터가 없을 때 예외 처리
                                        if (CarModelList.Count == 0)
                                        {

                                            await Conversation.SendAsync(activity, () => new RootDialog(luis_intent, entitiesStr, startTime, orgKRMent, orgENGMent));
                                            response = Request.CreateResponse(HttpStatusCode.OK);
                                            return response;
                                        }
                                        else
                                        {
                                            if (dlg.Count > 0)
                                            {
                                                Debug.WriteLine("dlg[0].dlgMent : [" + dlg[0].dlgMent + "]");
                                                Debug.WriteLine("dlg[0].dlgMent : [" + string.IsNullOrEmpty(dlg[0].dlgMent) + "]");
                                                HistoryLog("dlg[0].dlgMent : [" + dlg[0].dlgMent + "]");
                                                HistoryLog("dlg[0].dlgMent : [" + string.IsNullOrEmpty(dlg[0].dlgMent) + "]");
                                                if (string.IsNullOrEmpty(dlg[0].dlgMent) != true)
                                                {
                                                    Activity reply = activity.CreateReply(dlg[0].dlgMent.ToString());
                                                    await connector.Conversations.SendToConversationAsync(reply);
                                                }
                                            }

                                            for (int td = 0; td < CarModelList.Count; td++)
                                            {
                                                mainModelTitle = CarModelList[td].carModelNm;
                                                Debug.WriteLine("mainModelTitle : " + mainModelTitle);
                                                HistoryLog("mainModelTitle : " + mainModelTitle);
                                                if (mainModelTitle.Contains("TUIX"))
                                                {
                                                    mainModelTitle = mainModelTitle + " 2WD";
                                                }
                                                Debug.WriteLine("mainModelTitle 2 : " + mainModelTitle);
                                                HistoryLog("mainModelTitle 2 : " + mainModelTitle);

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
                                        luis_intent = gubunVal;
                                    }
                                    // 트림,엔진, 드라이브 휠, 칼라패키지, 튜익스 가격 질문
                                    else if (!entitiesStr.Contains("car color") && !entitiesStr.Contains("option"))
                                    {
                                        Debug.WriteLine("트림, 엔진, 드라이브 휠, 칼라 패키지, 튜익스 가격 질문");
                                        HistoryLog("트림, 엔진, 드라이브 휠, 칼라 패키지, 튜익스 가격 질문");

                                        string color = "";
                                        
                                        List<CarTrimList> CarTrimList = db.SelectCarTrimList1(priceWhereStr);

                                        //데이터가 없을 때 예외 처리
                                        if (CarTrimList.Count == 0)
                                        {
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
                                                HistoryLog("mainModelTitle : " + mainModelTitle);
                                                if (mainModelTitle.Contains("TUIX"))
                                                {
                                                    mainModelTitle = mainModelTitle + " 2WD";
                                                }
                                                Debug.WriteLine("mainModelTitle 2 : " + mainModelTitle);
                                                HistoryLog("mainModelTitle 2 : " + mainModelTitle);

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
                                                    //trimNM = trimNM.Replace("7단 ", "");
                                                    //trimNM = trimNM.Replace("DCT ", "");

                                                    if (!CarTrimList[td].saleCD.Contains("XX"))
                                                    {
                                                        color = (string)(CarTrimList[td].tuix + CarTrimList[td].cartrim);
                                                        //HistoryLog("AA : " + CarTrimList[td].tuix + CarTrimList[td].cartrim);
                                                        //HistoryLog("color : " + color);
                                                        replyToConversation.Attachments.Add(
                                                        GetHeroCard_show(
                                                        trimNM,
                                                        string.Format("{0}", CarTrimList[td].salePrice.ToString("n0")) + "원",
                                                        "",
                                                        new CardImage(url: domainURL+"/assets/images/price/trim/" + color.Replace(" ", "") + ".jpg"),
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

                                //activity.ChannelId = "facebook";

                                if (dlg.Count > 0)
                                {
                                    Debug.WriteLine("dlg[0].dlgMent : [" + dlg[0].dlgMent + "]");
                                    Debug.WriteLine("dlg[0].dlgMent : [" + string.IsNullOrEmpty(dlg[0].dlgMent) + "]");

                                    HistoryLog("dlg[0].dlgMent : [" + dlg[0].dlgMent + "]");
                                    HistoryLog("dlg[0].dlgMent : [" + string.IsNullOrEmpty(dlg[0].dlgMent) + "]");
                                    if (string.IsNullOrEmpty(dlg[0].dlgMent) != true)
                                    {
                                        //Activity reply = activity.CreateReply(dlg[0].dlgMent.ToString());

                                        Activity reply_ment2 = activity.CreateReply();
                                        reply_ment2.Recipient = activity.From;
                                        reply_ment2.Type = "message";
                                        reply_ment2.Text = dlg[0].dlgMent.ToString();

                                        await connector.Conversations.ReplyToActivityAsync(reply_ment2);
                                    }
                                }

                                Debug.WriteLine("(LuisDialogID[k].dlgId ====" + (LuisDialogID[k].dlgId));
                                HistoryLog("(LuisDialogID[k].dlgId ====" + (LuisDialogID[k].dlgId));

                                /////////////////////////////////////////////////////////////////
                                ///// 페이스북 카드 카운트 제한 시작
                                /////////////////////////////////////////////////////////////////
                                //List<CardList> card = new List<CardList>();

                                //if (activity.ChannelId == "facebook")
                                //{


                                //    if (db.SelectDialogCardCnt(LuisDialogID[k].dlgId) > pagePerCardCnt)
                                //    {
                                //        if (pageRotationCnt == 1)
                                //        {
                                //            card = db.SelectDialogCardFB(LuisDialogID[k].dlgId, pagePerCardCnt);
                                //        }
                                //        else if (pageRotationCnt > 1)
                                //        {
                                //            card = db.SelectDialogCardFB(LuisDialogID[k].dlgId, (pagePerCardCnt * pageRotationCnt));
                                //        }

                                //        pageRotationCnt++;
                                //    }
                                //    else
                                //    {
                                //        card = db.SelectDialogCard(LuisDialogID[k].dlgId);
                                //    }
                                //}
                                //else
                                //{
                                //    card = db.SelectDialogCard(LuisDialogID[k].dlgId);
                                //}

                                /////////////////////////////////////////////////////////////////
                                ///// 페이스북 카드 카운트 제한 끝
                                /////////////////////////////////////////////////////////////////



                                List<CardList> card = db.SelectDialogCard(LuisDialogID[k].dlgId);

                                if (string.IsNullOrEmpty(luis_intent))
                                {
                                    luis_intent = (string)Luis["intents"][0]["intent"];
                                }


                                if (card.Count > 0)
                                {
                                    // HeroCard 
                                    mediaURL_FB = new List<MediaUrl>();
                                    mediaTitle_FB = new List<string>();
                                    for (int i = 0; i < card.Count; i++)
                                    {
                                        //Debug.WriteLine(card[i].dlgId +"@@"+ card[i].cardId);
                                        //HistoryLog(card[i].dlgId + "@@" + card[i].cardId);
                                        List<ButtonList> btn = new List<ButtonList>();
                                        if (activity.ChannelId == "facebook")
                                        {
                                            btn = db.SelectBtn(card[i].dlgId, card[i].cardId, "Y");
                                        }
                                        else
                                        {
                                            btn = db.SelectBtn(card[i].dlgId, card[i].cardId, "N");
                                        }

                                        //List<ButtonList> btn = db.SelectBtn(card[i].dlgId, card[i].cardId, "N");

                                        List<ImagesList> img = db.SelectImage(card[i].dlgId, card[i].cardId);
                                        List<MediaList> media1 = db.SelectMedia(card[i].dlgId, card[i].cardId);

                                        List<CardImage> cardImages = new List<CardImage>();
                                        CardImage[] plImage = new CardImage[img.Count];

                                        //ThumbnailUrl plThumnail = new ThumbnailUrl();

                                        List<CardAction> cardButtons = new List<CardAction>();
                                        CardAction[] plButton = new CardAction[btn.Count];

                                        //CardAction tap = new CardAction();

                                        List<MediaUrl> mediaURL1 = new List<MediaUrl>();
                                        MediaUrl[] plMediaUrl1 = new MediaUrl[media1.Count];
                                        
                                        UserHeroCard[] plHeroCard = new UserHeroCard[card.Count];
                                        VideoCard[] plVideoCard = new VideoCard[card.Count];
                                        Attachment[] plAttachment = new Attachment[card.Count];

                                        
                                       // Debug.WriteLine("media1.Count : " + media1.Count);
                                        //HistoryLog("media1.Count : " + media1.Count);
                                        //media
                                        for (int l = 0; l < media1.Count; l++)
                                        {
                                            if (media1[l].mediaUrl != null)
                                            {
                                                //Debug.WriteLine("mediaUrl : " +  media1[l].mediaUrl);

                                                plMediaUrl1[l] = new MediaUrl()
                                                {
                                                    Url = media1[l].mediaUrl
                                                };
                                            }
                                        }
                                        mediaURL1 = new List<MediaUrl>(plMediaUrl1);
                                        
                                        
                                        //mediaURL_FB.AddRange(plMediaUrl1);
                                        
                                        
                                        //button
                                        for (int m = 0; m < btn.Count; m++)
                                        {
                                            //if(activity.ChannelId == "facebook" && mediaURL1.Count > 0)
                                            //{
                                            //    if (btn[m].btnTitle != null)
                                            //    {
                                            //        //Debug.WriteLine(" btn[m].btnContext : " + btn[m].btnContext);
                                            //        //HistoryLog(" btn[m].btnContext : " + btn[m].btnContext);

                                            //        plButton[m] = new CardAction()
                                            //        {
                                            //            Value = card[i].cardValue,
                                            //            Type = "openUrl",
                                            //            Title = btn[m].btnTitle
                                            //        };
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //    if (btn[m].btnTitle != null)
                                            //    {
                                            //        //Debug.WriteLine(" btn[m].btnContext : " + btn[m].btnContext);
                                            //        //HistoryLog(" btn[m].btnContext : " + btn[m].btnContext);

                                            //        plButton[m] = new CardAction()
                                            //        {
                                            //            Value = btn[m].btnContext,
                                            //            Type = btn[m].btnType,
                                            //            Title = btn[m].btnTitle
                                            //        };
                                            //    }
                                            //}
                                            if (btn[m].btnTitle != null)
                                            {
                                                //Debug.WriteLine(" btn[m].btnContext : " + btn[m].btnContext);
                                                //HistoryLog(" btn[m].btnContext : " + btn[m].btnContext);

                                                plButton[m] = new CardAction()
                                                {
                                                    Value = btn[m].btnContext,
                                                    Type = btn[m].btnType,
                                                    Title = btn[m].btnTitle
                                                };
                                            }

                                        }

                                        if (activity.ChannelId == "facebook" && mediaURL1.Count > 0)
                                        {

                                            //Debug.WriteLine(" btn[m].btnContext : " + btn[m].btnContext);
                                            //HistoryLog(" btn[m].btnContext : " + btn[m].btnContext);

                                            plButton = new CardAction[1];

                                                plButton[0] = new CardAction()
                                                {
                                                    Value = card[i].cardValue,
                                                    Type = "openUrl",
                                                    Title = "영상보기"
                                                };
                                            
                                        }


                                        cardButtons = new List<CardAction>(plButton);

                                        //images
                                        for (int l = 0; l < img.Count; l++)
                                        {

                                            HistoryLog("plImageplImageplImage");
                                            if (img[l].imgUrl != null)
                                            {
                                                plImage[l] = new CardImage()
                                                {
                                                    Url = img[l].imgUrl
                                                };
                                            }
                                        }

                                        cardImages = new List<CardImage>(plImage);

                                        //Debug.WriteLine("cardButtons Count : " + cardButtons.Count());
                                        //HistoryLog("cardButtons Count : " + cardButtons.Count());

                                        //Debug.WriteLine("CHANNEL ID : " + activity.ChannelId);
                                        //HistoryLog("CHANNEL ID : " + activity.ChannelId);
                                        
                                        
                                        if (activity.ChannelId == "facebook" && cardButtons.Count < 1 && cardImages.Count < 1 && mediaURL1.Count < 1)
                                        {
                                            Debug.WriteLine("facebook only card Text");
                                            HistoryLog("facebook only card Text ");
                                            Activity reply_facebook = activity.CreateReply();
                                            reply_facebook.Recipient = activity.From;
                                            reply_facebook.Type = "message";
                                            HistoryLog("facebook  card Text : " + card[i].cardText);
                                            reply_facebook.Text = card[i].cardText;
                                            var reply_ment_facebook = await connector.Conversations.SendToConversationAsync(reply_facebook);
                                        }
                                        else
                                        {
                                            
                                            if (activity.ChannelId == "facebook" && mediaURL1.Count > 0)
                                            {
                                            
                                                //plVideoCard[i] = new VideoCard()
                                                //{
                                                //    Title = card[i].cardTitle,
                                                //    //Text = card[i].cardText,
                                                //    //Subtitle = card[i].cardTitle,
                                                //    //Image = plThumnail,
                                                //    Media = mediaURL1
                                                //    //Buttons = cardButtons
                                                //};

                                                plHeroCard[i] = new UserHeroCard()
                                                {
                                                    Title = card[i].cardTitle,
                                                    //Text = card[i].cardText,
                                                    //Subtitle = card[i].cardTitle,
                                                    Images = cardImages,
                                                    //Media = mediaURL1
                                                    Buttons = cardButtons
                                                };

                                                //HistoryLog("facebook video card - 1");
                                                //plAttachment[i] = plVideoCard[i].ToAttachment();
                                                plAttachment[i] = plHeroCard[i].ToAttachment();
                                                //HistoryLog("facebook video card - 2 : " + plAttachment.Count());
                                                replyToConversation.Attachments.Add(plAttachment[i]);
                                                //HistoryLog("facebook video card - 3");
                                                mediaTitle_FB.Add(card[i].cardTitle);
                                            }
                                            //if (card[i].cardType == "herocard")
                                            else
                                            {
                                                HistoryLog("herocard card");
                                                string text = card[i].cardTitle;

                                                if (activity.ChannelId == "facebook")
                                                {
                                                    if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(card[i].cardText))
                                                    {
                                                        text = "선택해 주세요";
                                                    }
                                                }


                                                //    if (activity.ChannelId == "facebook")
                                                //{
                                                //    if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(card[i].cardText))
                                                //    {
                                                //        text = " ";
                                                //        //card.Count
                                                //        plHeroCard[i] = new UserHeroCard()
                                                //        {
                                                //            //Title = card[i].cardTitle,
                                                //            Title = text,
                                                //            Text = card[i].cardText,
                                                //            Subtitle = card[i].cardSubTitle,
                                                //            Images = cardImages,
                                                //            //Tap = tap,
                                                //            Buttons = cardButtons,
                                                //            Card_division = card[i].cardDivision,
                                                //            Card_value = card[i].cardValue,
                                                //            Card_cnt = card.Count
                                                //        };
                                                //        HistoryLog("herocard card - 1");
                                                //        plAttachment[i] = plHeroCard[i].ToAttachment();
                                                //        replyToConversation.Attachments.Add(plAttachment[i]);
                                                //    }
                                                //    else
                                                //    {
                                                //        //card.Count
                                                //        plHeroCard[i] = new UserHeroCard()
                                                //        {
                                                //            Title = card[i].cardTitle,
                                                //            //Title = text,
                                                //            Text = card[i].cardText,
                                                //            Subtitle = card[i].cardSubTitle,
                                                //            Images = cardImages,
                                                //            //Tap = tap,
                                                //            Buttons = cardButtons,
                                                //            Card_division = card[i].cardDivision,
                                                //            Card_value = card[i].cardValue,
                                                //            Card_cnt = card.Count
                                                //        };
                                                //        HistoryLog("herocard card - 2");
                                                //        plAttachment[i] = plHeroCard[i].ToAttachment();
                                                //        replyToConversation.Attachments.Add(plAttachment[i]);
                                                //    }
                                                //}
                                                //else
                                                //{
                                                    ////card.Count
                                                    plHeroCard[i] = new UserHeroCard()
                                                    {
                                                        //Title = card[i].cardTitle,
                                                        Title = text,
                                                        Text = card[i].cardText,
                                                        Subtitle = card[i].cardSubTitle,
                                                        Images = cardImages,
                                                        //Tap = tap,
                                                        Buttons = cardButtons,
                                                        Card_division = card[i].cardDivision,
                                                        Card_value = card[i].cardValue,
                                                        Card_cnt = card.Count
                                                    };
                                                    //HistoryLog("herocard card - 3");
                                                    plAttachment[i] = plHeroCard[i].ToAttachment();
                                                    replyToConversation.Attachments.Add(plAttachment[i]);
                                                //}

                                                
                                            }
                                        }
                                    }
                                }
                                else if (card.Count == 0 && string.IsNullOrEmpty(dlg[0].dlgMent))
                                {
                                    await Conversation.SendAsync(activity, () => new RootDialog(luis_intent, entitiesStr, startTime, orgKRMent, orgENGMent));
                                    response = Request.CreateResponse(HttpStatusCode.OK);
                                    return response;
                                }
                            }
                            //HistoryLog("before reply : "+ replyToConversation.Attachments.Count());

                            if (replyToConversation.Attachments.Count > 0)
                            {

                                //Attachment[] plAttachment = new Attachment[replyToConversation.Attachments.Count];
                                //for (int l = 0; l < replyToConversation.Attachments.Count; l++)
                                //{
                                //    plAttachment[l] = replyToConversation.Attachments[l];
                                //    Debug.WriteLine("plAttachment[l] " + plAttachment[l].ContentType + " || " + plAttachment[l].Content + " || " + plAttachment[l].ContentUrl + " || " + plAttachment[l].Name + " || " + plAttachment[l].ThumbnailUrl);
                                //    //HistoryLog("plAttachment[l] " + plAttachment[l].Name[0] +" || " + plAttachment[l].ContentType + " || " + plAttachment[l].Content + " || " + plAttachment[l].ContentUrl + " || " + plAttachment[l].Name + " || " + plAttachment[l].ThumbnailUrl);
                                //}
                                //plAttachment.
                                //replyMessage.Attachments.Add(new Attachment()
                                //{
                                //    ContentUrl = "https://upload.wikimedia.org/wikipedia/en/a/a6/Bender_Rodriguez.png",
                                //    ContentType = "image/png",
                                //    Name = "Bender_Rodriguez.png"
                                //});

                                //replyToConversation.ChannelData = getFBFunctionMenu("");
                                //plAttachment[0].ContentUrl = "http://www.smartsend.co.kr/assets/videos/tOs7xECRdxY.mp4";
                                //plAttachment[0].Name = "고효주";
                                //plAttachment[0].ContentType = "application / vnd.microsoft.card.video";

                                if (activity.ChannelId == "facebook" && mediaURL_FB.Count > 0)
                                {
                                    replyToConversation.Attachments.Clear();
                                    //HistoryLog("2222222222222222222222222" + mediaURL_FB.Count());
                                    for (int i = 0; i < mediaURL_FB.Count(); i++)
                                    {
                                        //HistoryLog("mediaURL_FB[i].ToString() : " + mediaURL_FB[i].Url.ToString());
                                        
                                        //replyToConversation.ChannelData = getFBFunctionMenu("고효주", "http://www.smartsend.co.kr/assets/videos/tOs7xECRdxY.mp4");
                                        replyToConversation.ChannelData = getFBFunctionMenu(mediaTitle_FB[i].ToString(), mediaURL_FB[i].Url.ToString());
                                        await connector.Conversations.SendToConversationAsync(replyToConversation);

                                        Activity reply_facebook = activity.CreateReply();
                                        reply_facebook.Recipient = activity.From;
                                        reply_facebook.Type = "message";
                                        HistoryLog("facebook  card Text : " + mediaTitle_FB[i].ToString());
                                        reply_facebook.Text = mediaTitle_FB[i].ToString();
                                        var reply_ment_facebook = await connector.Conversations.SendToConversationAsync(reply_facebook);
                                    }
                                }
                                else
                                {
                                    await connector.Conversations.SendToConversationAsync(replyToConversation);
                                }
                            }

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
                                    HistoryLog("mainModelTitle : " + mainModelTitle);
                                    if (mainModelTitle.Contains("TUIX"))
                                    {
                                        mainModelTitle = mainModelTitle + " 2WD";
                                    }
                                    Debug.WriteLine("mainModelTitle 2 : " + mainModelTitle);
                                    HistoryLog("mainModelTitle 2 : " + mainModelTitle);

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
                                if (luis_intent.Equals("Quote"))
                                {
                                    gubunVal = "Quote";
                                }
                                else if (luis_intent.Equals("Branch") || luis_intent.Equals("Test drive") || luis_intent.Equals("Test drive car color"))
                                {
                                    gubunVal = "Test drive";
                                }
                                else
                                {
                                    gubunVal = "OTHER";
                                }
                                //int dbResult = db.insertUserQuery(orgKRMent, orgENGMent, luis_intent, entitiesStr, luis_intent_score, luisID, 'H', testDriveWhereStr, "", priceWhereStr, gubunVal);
                                int dbResult = db.insertUserQuery(orgKRMent, orgENGMent_history, luis_intent, entitiesStr, luis_intent_score, luisID, 'H', testDriveWhereStr, "", priceWhereStr, gubunVal);
                                //int dbResult = db.insertUserQuery(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), luis_intent, entitiesStr, luisID, 'H', testDriveWhereStr, "", priceWhereStr, gubunVal);
                                Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());
                                HistoryLog("INSERT QUERY RESULT : " + dbResult.ToString());

                                response = Request.CreateResponse(HttpStatusCode.OK);
                                return response;
                                

                            }
                            //시승 다이얼로그가 없을때 출력 - 시승
                            else if (luis_intent.Equals("Test drive")|| luis_intent.Equals("Test drive car color") || luis_intent.Equals("Branch"))
                            {
                                List<CardList> card = db.SelectDialogCard(1162);

                                if (string.IsNullOrEmpty(luis_intent))
                                {
                                    luis_intent = (string)Luis["intents"][0]["intent"];
                                }


                                if (card.Count > 0)
                                {
                                    // HeroCard 

                                    for (int i = 0; i < card.Count; i++)
                                    {
                                        List<ButtonList> btn = new List<ButtonList>();
                                        if (activity.ChannelId == "facebook")
                                        {
                                            btn = db.SelectBtn(card[i].dlgId, card[i].cardId, "Y");
                                        }
                                        else
                                        {
                                            btn = db.SelectBtn(card[i].dlgId, card[i].cardId, "N");
                                        }

                                        //List<ButtonList> btn = db.SelectBtn(card[i].dlgId, card[i].cardId);
                                        List<ImagesList> img = db.SelectImage(card[i].dlgId, card[i].cardId);
                                        List<MediaList> media1 = db.SelectMedia(card[i].dlgId, card[i].cardId);

                                        List<CardImage> cardImages = new List<CardImage>();
                                        CardImage[] plImage = new CardImage[img.Count];

                                        ThumbnailUrl plThumnail = new ThumbnailUrl();

                                        List<CardAction> cardButtons = new List<CardAction>();
                                        CardAction[] plButton = new CardAction[btn.Count];

                                        //CardAction tap = new CardAction();

                                        List<MediaUrl> mediaURL1 = new List<MediaUrl>();
                                        MediaUrl[] plMediaUrl1 = new MediaUrl[media1.Count];

                                        //ReceiptCard[] plReceiptCard = new ReceiptCard[card.Count];
                                        //HeroCard[] plHeroCard = new HeroCard[card.Count];
                                        UserHeroCard[] plHeroCard = new UserHeroCard[card.Count];
                                        VideoCard[] plVideoCard = new VideoCard[card.Count];
                                        Attachment[] plAttachment = new Attachment[card.Count];


                                        //Debug.WriteLine("media1.Count : " + media1.Count);
                                        //HistoryLog("media1.Count : " + media1.Count);
                                        //media
                                        for (int l = 0; l < media1.Count; l++)
                                        {
                                            if (media1[l].mediaUrl != null)
                                            {
                                                //Debug.WriteLine("mediaUrl : " + media1[l].mediaUrl);

                                                plMediaUrl1[l] = new MediaUrl()
                                                {
                                                    Url = media1[l].mediaUrl
                                                };
                                            }
                                        }
                                        mediaURL1 = new List<MediaUrl>(plMediaUrl1);

                                        //button
                                        for (int m = 0; m < btn.Count; m++)
                                        {
                                            if (btn[m].btnTitle != null)
                                            {
                                                //Debug.WriteLine(" btn[m].btnContext : " + btn[m].btnContext);
                                                //HistoryLog(" btn[m].btnContext : " + btn[m].btnContext);

                                                plButton[m] = new CardAction()
                                                {
                                                    Value = btn[m].btnContext,
                                                    Type = btn[m].btnType,
                                                    Title = btn[m].btnTitle
                                                };
                                            }

                                        }
                                        cardButtons = new List<CardAction>(plButton);

                                        //images
                                        for (int l = 0; l < img.Count; l++)
                                        {

                                            //HistoryLog("plImageplImageplImage");
                                            if (img[l].imgUrl != null)
                                            {
                                                plImage[l] = new CardImage()
                                                {
                                                    Url = img[l].imgUrl
                                                };
                                            }
                                            //}
                                        }

                                        cardImages = new List<CardImage>(plImage);

                                        //Debug.WriteLine("cardButtons Count : " + cardButtons.Count());
                                        //HistoryLog("cardButtons Count : " + cardButtons.Count());

                                        //Debug.WriteLine("CHANNEL ID : " + activity.ChannelId);
                                        //HistoryLog("CHANNEL ID : " + activity.ChannelId);


                                        if (activity.ChannelId == "facebook" && cardButtons.Count < 1 && cardImages.Count < 1 && mediaURL1.Count < 1)
                                        {
                                            Debug.WriteLine("facebook only card Text");
                                            HistoryLog("facebook only card Text ");
                                            Activity reply_facebook = activity.CreateReply();
                                            reply_facebook.Recipient = activity.From;
                                            reply_facebook.Type = "message";
                                            HistoryLog("facebook  card Text : " + card[i].cardText);
                                            reply_facebook.Text = card[i].cardText;
                                            var reply_ment_facebook = await connector.Conversations.SendToConversationAsync(reply_facebook);
                                        }
                                        else
                                        {
                                            //Debug.WriteLine("no  facebook ");
                                            if (activity.ChannelId == "facebook" && mediaURL1.Count > 0)
                                            //if (card[i].cardType == "videocard")
                                            {
                                                plVideoCard[i] = new VideoCard()
                                                {
                                                    Title = card[i].cardTitle,
                                                    //Text = card[i].cardText,
                                                    //Subtitle = card[i].cardTitle,
                                                    //Image = plThumnail,
                                                    Media = mediaURL1
                                                    //Buttons = cardButtons
                                                };
                                                //HistoryLog("facebook video card - 1");
                                                plAttachment[i] = plVideoCard[i].ToAttachment();
                                                //HistoryLog("facebook video card - 2 : " + plAttachment.Count());
                                                replyToConversation.Attachments.Add(plAttachment[i]);
                                                //HistoryLog("facebook video card - 3");
                                            }
                                            //if (card[i].cardType == "herocard")
                                            else
                                            {
                                                HistoryLog("herocard card");
                                                string text = card[i].cardTitle;
                                                if (activity.ChannelId == "facebook")
                                                {
                                                    if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(card[i].cardText))
                                                    {
                                                        text = "선택해 주세요";
                                                    }
                                                }


                                                //card.Count
                                                plHeroCard[i] = new UserHeroCard()
                                                {
                                                    //Title = card[i].cardTitle,
                                                    Title = text,
                                                    Text = card[i].cardText,
                                                    Subtitle = card[i].cardSubTitle,
                                                    Images = cardImages,
                                                    //Tap = tap,
                                                    Buttons = cardButtons,
                                                    Card_division = card[i].cardDivision,
                                                    Card_value = card[i].cardValue,
                                                    Card_cnt = card.Count
                                                };
                                                //HistoryLog("herocard card - 1");
                                                plAttachment[i] = plHeroCard[i].ToAttachment();
                                                replyToConversation.Attachments.Add(plAttachment[i]);
                                            }
                                        }
                                    }
                                }

                                await connector.Conversations.ReplyToActivityAsync(replyToConversation);
                                if (luis_intent.Equals("Quote"))
                                {
                                    gubunVal = "Quote";
                                }
                                else if (luis_intent.Equals("Branch") || luis_intent.Equals("Test drive") || luis_intent.Equals("Test drive car color"))
                                {
                                    gubunVal = "Test drive";
                                }
                                else
                                {
                                    gubunVal = "OTHER";
                                }

                                

                                //int dbResult = db.insertUserQuery(orgKRMent, orgENGMent, luis_intent, entitiesStr, luis_intent_score, luisID, 'H', testDriveWhereStr, "", priceWhereStr, gubunVal);
                                int dbResult = db.insertUserQuery(orgKRMent, orgENGMent_history, luis_intent, entitiesStr, luis_intent_score, luisID, 'H', testDriveWhereStr, "", priceWhereStr, gubunVal);
                                Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());
                                HistoryLog("INSERT QUERY RESULT : " + dbResult.ToString());

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
                            if (luis_intent.Equals("Quote"))
                            {
                                gubunVal = "Quote";
                            }
                            else if (luis_intent.Equals("Branch") || luis_intent.Equals("Test drive") || luis_intent.Equals("Test drive car color"))
                            {
                                gubunVal = "Test drive";
                            }
                            else
                            {
                                gubunVal = "OTHER";
                            }
                            //int dbResult = db.insertUserQuery(orgKRMent, orgENGMent, luis_intent, entitiesStr, luis_intent_score, luisID, 'H', testDriveWhereStr, "", priceWhereStr, gubunVal);
                            int dbResult = db.insertUserQuery(orgKRMent, orgENGMent_history, luis_intent, entitiesStr, luis_intent_score, luisID, 'H', testDriveWhereStr, "", priceWhereStr, gubunVal);
                            Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());
                            HistoryLog("INSERT QUERY RESULT : " + dbResult.ToString());
                        }

                        DateTime endTime = DateTime.Now;
                        Debug.WriteLine("USER NUMBER : " + activity.Conversation.Id);
                        Debug.WriteLine("CUSTOMMER COMMENT KOREAN : " + activity.Text);
                        Debug.WriteLine("CUSTOMMER COMMENT ENGLISH : " + translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));

                        Debug.WriteLine("CHATBOT_COMMENT_CODE : " + dlg[0].dlgNm);
                        Debug.WriteLine("CHANNEL_ID : " + activity.ChannelId);
                        Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - startTime).Milliseconds));

                        HistoryLog("USER NUMBER : " + activity.Conversation.Id);
                        HistoryLog("CUSTOMMER COMMENT KOREAN : " + activity.Text);
                        HistoryLog("CUSTOMMER COMMENT ENGLISH : " + translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));

                        HistoryLog("CHATBOT_COMMENT_CODE : " + dlg[0].dlgNm);
                        HistoryLog("CHANNEL_ID : " + activity.ChannelId);
                        HistoryLog("프로그램 수행시간 : {0}/ms" + ((endTime - startTime).Milliseconds));

                        inserResult = db.insertHistory(activity.Conversation.Id, orgMent, translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), dlg[0].dlgNm, activity.ChannelId, ((endTime - startTime).Milliseconds));
                        if (inserResult > 0)
                        {
                            Debug.WriteLine("HISTORY RESULT SUCCESS");
                            HistoryLog("HISTORY RESULT SUCCESS");
                        }
                        else
                        {
                            Debug.WriteLine("HISTORY RESULT FAIL");
                            HistoryLog("HISTORY RESULT FAIL");
                        }
                        sorryMessageCnt = 0;
                        //});
                        HistoryLog("[ DIALOG ] ==>> userID :: [ " + activity.Conversation.Id + " ]       message :: [ " + orgMent + " ]       date :: [ " + DateTime.Now + " ]");
                    }
                    catch(Exception e)
                    {

                        Debug.WriteLine("EXCEPTIONEXCEPTIONEXCEPTIONEXCEPTION : " + e.ToString());
                        HistoryLog("EXCEPTIONEXCEPTIONEXCEPTIONEXCEPTION : " + e.ToString());

                        //Regex.Replace(orgMent, @"[^a-zA-Z0-9가-힣-\s]", "", RegexOptions.Singleline);
                        if (luis_intent.Equals("Quote"))
                        {
                            gubunVal = "Quote";
                        }
                        else if (luis_intent.Equals("Branch") || luis_intent.Equals("Test drive") || luis_intent.Equals("Test drive car color"))
                        {
                            gubunVal = "Test drive";
                        }
                        else
                        {
                            gubunVal = "OTHER";
                        }
                        //int dbResult = db.insertUserQuery(orgKRMent, orgENGMent, luis_intent, entitiesStr, luis_intent_score, luisID, 'H', testDriveWhereStr, "", priceWhereStr, gubunVal);
                        int dbResult = db.insertUserQuery(orgKRMent, orgENGMent_history, luis_intent, entitiesStr, luis_intent_score, luisID, 'D', testDriveWhereStr, "", priceWhereStr, gubunVal);
                        Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());                        
						HistoryLog("INSERT QUERY RESULT : " + dbResult.ToString());

                        await Conversation.SendAsync(activity, () => new RootDialog(luis_intent, entitiesStr, startTime, orgKRMent, orgENGMent));

                        DateTime endTime = DateTime.Now;
                        Debug.WriteLine("USER NUMBER : " + activity.Conversation.Id);
                        Debug.WriteLine("CUSTOMMER COMMENT KOREAN : " + activity.Text);
                        Debug.WriteLine("CUSTOMMER COMMENT ENGLISH : " + translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));
                        Debug.WriteLine("CHANNEL_ID : " + activity.ChannelId);
                        Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - startTime).Milliseconds));

                        HistoryLog("USER NUMBER : " + activity.Conversation.Id);
                        HistoryLog("CUSTOMMER COMMENT KOREAN : " + activity.Text);
                        HistoryLog("CUSTOMMER COMMENT ENGLISH : " + translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));
                        HistoryLog("CHANNEL_ID : " + activity.ChannelId);
                        HistoryLog("프로그램 수행시간 : {0}/ms" + ((endTime - startTime).Milliseconds));

                        inserResult = db.insertHistory(activity.Conversation.Id, orgMent, translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), "ERROR", activity.ChannelId, ((endTime - startTime).Milliseconds));
                        if (inserResult > 0)
                        {
                            Debug.WriteLine("HISTORY RESULT SUCCESS");
                            HistoryLog("HISTORY RESULT SUCCESS");
                        }
                        else
                        {
                            Debug.WriteLine("HISTORY RESULT FAIL");
                            HistoryLog("HISTORY RESULT FAIL");
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
                //string RequestURI = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/04259452-27fe-4f72-9441-c4100b835c52?subscription-key=7efb093087dd48918b903885b944740c&timezoneOffset=0&verbose=true&q=" + Query; // taiho azure
                //string RequestURI = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/7f77d1c8-011d-402c-acd9-6a7188d368f7?subscription-key=4da995f76bbc4ffb90ce2caf22265f9d&timezoneOffset=0&verbose=true&q=" + Query; // hyundai luis
                //string RequestURI = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/e6badb41-a62c-4357-af03-4e4c54610afa?subscription-key=4da995f76bbc4ffb90ce2caf22265f9d&timezoneOffset=0&verbose=true&q=" + Query; // hyundai 운영 luis
                string RequestURI = luisURL + "&timezoneOffset=0&verbose=true&q=" + Query; // hyundai 운영 luis


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

        private static Attachment GetHeroCard_button(string title, string subtitle, string text, CardAction cardAction1, CardAction cardAction2)
        {
            var heroCard = new UserHeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Buttons = new List<CardAction>() { cardAction1, cardAction2 },

            };
            return heroCard.ToAttachment();
        }

        internal static Attachment GetHeroCard_sorry(string text, CardAction cardAction1)
        {
            var heroCard = new UserHeroCard
            {
                
                Text = text,
                Buttons = new List<CardAction>() { cardAction1},

            };
            return heroCard.ToAttachment();
        }


        internal static Attachment GetHeroCard_sorry_noEvent(string text)
        {
            var heroCard = new UserHeroCard
            {

                Text = text,

            };
            return heroCard.ToAttachment();
        }

        internal static Attachment GetHeroCard_recommend_1(string text, CardAction cardAction1, CardAction cardAction2, CardAction cardAction3, CardAction cardAction4)
        {
            var heroCard = new UserHeroCard
            {

                Text = text,
                Buttons = new List<CardAction>() { cardAction1, cardAction2, cardAction3, cardAction4 },

            };
            return heroCard.ToAttachment();
        }

        internal static Attachment GetHeroCard_recommend_2(string text, CardAction cardAction1, CardAction cardAction2, CardAction cardAction3, CardAction cardAction4, CardAction cardAction5)
        {
            var heroCard = new UserHeroCard
            {

                Text = text,
                Buttons = new List<CardAction>() { cardAction1, cardAction2, cardAction3, cardAction4, cardAction5 },

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
                HistoryLog(e.Message);
				Debug.WriteLine(e.Message);
            }
        }
        public object getFBFunctionMenu(String paramtitle, String paramurl)
        {
            Models.Messenger fbmsg = new Models.Messenger();
            fbmsg.ChannelData = new MessengerChannelData { notification_type = "NO_PUSH", attachment = new MessengerAttachment { payload = new MessengerPayload() } };
            fbmsg.ChannelData.attachment.type = "video";
            //fbmsg.ChannelData.attachment.type = "web_url";
            fbmsg.ChannelData.attachment.payload.url = paramurl;
            List<MessengerElement> e = new List<MessengerElement>();
            List<MessengerButton> bs = new List<MessengerButton>();
            bs.Add(new MessengerButton { type = "postback", title = "Facebook", payload = "http://www.facebook.com/" });
            bs.Add(new MessengerButton { type = "postback", title = "Google", payload = "http://www.google.com/" });
            bs.Add(new MessengerButton { type = "postback", title = "Amazon", payload = "http://www.amazon.com/" });
            e.Add(new MessengerElement
            {
                //title = paramtitle,
                //url = "",
                //item_url = "",
                url = paramurl,
                //buttons = bs.ToArray()
                //buttons = null
            });
            //fbmsg.ChannelData.attachment.payload.elements = e.ToArray();
            return fbmsg.ChannelData;
        }
    }
}
