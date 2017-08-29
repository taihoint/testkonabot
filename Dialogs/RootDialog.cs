namespace Bot_Application1.Dialogs

{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using System.Diagnostics;
    using System.Net;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using Bot_Application1.Models;
    using System.Text.RegularExpressions;
    using Bot_Application1.DB;
    using System.Configuration;
    using System.Web.Configuration;

#pragma warning disable 1998

    [Serializable]
    public class RootDialog : IDialog<object>
    {

        //public static int sorryMessageCnt = 0;
        public static string newUserID = "";
        public static string beforeUserID = "";
        private string luis_intent;
        private string entitiesStr;
        private DateTime startTime;
        public static string beforeMessgaeText = "";
        public static string messgaeText = "";
        private string orgKRMent;
        private string orgENGMent;

        public static Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration("/testkonabot");
        const string redirectEventPageURL = "redirectPageURL";
        //ConnectionStringSettings eventURL = rootWebConfig.ConnectionStrings.ConnectionStrings[redirectEventPageURL];
        string eventURL = rootWebConfig.ConnectionStrings.ConnectionStrings[redirectEventPageURL].ToString();

        public RootDialog(string luis_intent, string entitiesStr, DateTime startTime, string orgKRMent, string orgENGMent)
        {
            this.luis_intent = luis_intent;
            this.entitiesStr = entitiesStr;
            this.startTime = startTime;
            this.orgKRMent = orgKRMent;
            this.orgENGMent = orgENGMent;

        }

        public async Task StartAsync(IDialogContext context)
        {
            /* Wait until the first message is received from the conversation and call MessageReceviedAsync 
             *  to process that message. */

            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            /* When MessageReceivedAsync is called, it's passed an IAwaitable<IMessageActivity>. To get the message,
             *  await the result. */

            Debug.WriteLine("luis_intent : " + luis_intent);
            Debug.WriteLine("entitiesStr : " + entitiesStr);

            // Db
            DbConnect db = new DbConnect();

            Debug.WriteLine("activity : " + context.Activity.Conversation.Id);
            
            newUserID = context.Activity.Conversation.Id;
            if (beforeUserID != newUserID)
            {
                beforeUserID = newUserID;
                MessagesController.sorryMessageCnt = 0;
            }
            
            var message = await result;
            beforeMessgaeText = message.Text;
            if (message.Text.Contains("코나") == true)
            {
                messgaeText = message.Text;
                if(messgaeText.Contains("현대자동차") != true|| messgaeText.Contains("현대 자동차") != true)
                {
                    //messgaeText = messgaeText;
                    messgaeText = "현대자동차 " + messgaeText;
                }
                //else
                //{
                //    messgaeText = "현대자동차 "+message.Text;
                //}
            }
            else
            {
                messgaeText = "코나 " + message.Text;
                if (messgaeText.Contains("현대자동차") != true || messgaeText.Contains("현대 자동차") != true)
                {
                    //messgaeText = messgaeText;
                    messgaeText = "현대자동차 " + messgaeText;
                }
                //else
                //{
                //    messgaeText = "현대자동차 " + messgaeText;
                //}
            }

            if (messgaeText.Contains("코나") == true && (messgaeText.Contains("현대자동차") == true || messgaeText.Contains("현대 자동차") == true))
            //if (message.Text != "")
            {
                //context.Call(new SearchDialog(), this.SearchDialogResumeAfter);
                //string messgaeText = message.Text.Substring(3, message.Text.Length - 3);             
                var reply = context.MakeMessage();
                Debug.WriteLine("SERARCH MESSAGE : " + messgaeText);
                if ((messgaeText != null) && messgaeText.Trim().Length > 0)
                {
                    //Naver Search API

                    //string url = string.Format("https://translation.googleapis.com/language/translate/v2/?key={0}&q={1}&source=ko&target=en&model=nmt", appId, input);
                    //string url = string.Format("https://openapi.naver.com/v1/search/{1}.json?query={2}&display=10&start=1&sort=sim", , messgaeText);

                    string url = "https://openapi.naver.com/v1/search/news.json?query=" + messgaeText + "&display=10&start=1&sort=sim"; // JSON result 
                    //string blogUrl = "https://openapi.naver.com/v1/search/blog.json?query=" + messgaeText + "&display=10&start=1&sort=sim"; // JSON result 
                    //string cafeUrl = "https://openapi.naver.com/v1/search/cafearticle.json?query=" + messgaeText + "&display=10&start=1&sort=sim"; // JSON result 
                    //string url = "https://openapi.naver.com/v1/search/blog.xml?query=" + query; //XML result
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Headers.Add("X-Naver-Client-Id", "Y536Z1ZMNv93Oej6TrkF");
                    request.Headers.Add("X-Naver-Client-Secret", "cPHOFK6JYY");
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    string status = response.StatusCode.ToString();
                    if (status == "OK")
                    {
                        Stream stream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                        string text = reader.ReadToEnd();

                        RootObject serarchList = JsonConvert.DeserializeObject<RootObject>(text);

                        Debug.WriteLine("serarchList : " + serarchList);
                        //description
                        
                        if (serarchList.display == 1)
                        {
                            Debug.WriteLine("SERARCH : " + Regex.Replace(serarchList.items[0].title, @"[^<:-:>-<b>-</b>]", "", RegexOptions.Singleline));

                            if (serarchList.items[0].title.Contains("코나"))
                            {
                                //Only One item
                                List<CardImage> cardImages = new List<CardImage>();
                                CardImage img = new CardImage();
                                img.Url = "";
                                cardImages.Add(img);

                                string searchTitle = "";
                                string searchText = "";

                                searchTitle = serarchList.items[0].title;
                                searchText = serarchList.items[0].description;

                                

                                if(context.Activity.ChannelId == "facebook")
                                {
                                    searchTitle = Regex.Replace(searchTitle, @"[<][a-z|A-Z|/](.|)*?[>]", "", RegexOptions.Singleline).Replace("\n", "").Replace("<:", "").Replace(":>", "");
                                    searchText = Regex.Replace(searchText, @"[<][a-z|A-Z|/](.|)*?[>]", "", RegexOptions.Singleline).Replace("\n", "").Replace("<:", "").Replace(":>", "");
                                }
                                

                                LinkHeroCard card = new LinkHeroCard()
                                {
                                    Title = searchTitle,
                                    Subtitle = null,
                                    Text = searchText,
                                    Images = cardImages,
                                    Buttons = null,
                                    Link = Regex.Replace( serarchList.items[0].link,"amp;", "")
                                };
                                var attachment = card.ToAttachment();

                                reply.Attachments = new List<Attachment>();
                                reply.Attachments.Add(attachment);
                            }
                        }
                        else
                        {
                            //string test = "[낭중지추]금호타이어, <b>현대</b>차 <b>코나</b>行 \n 초대장 :> 못받은 <: 사연";
                            //Debug.WriteLine("SERARCH : " + Regex.Replace(test, @"[<][a-z|A-Z|/](.|)*?[>]", "", RegexOptions.Singleline));
                            //Debug.WriteLine("SERARCH : " + Regex.Replace(test, @"[<][a-z|A-Z|/](.|)*?[>]", "", RegexOptions.Singleline));
                            //Debug.WriteLine("SERARCH : " + Regex.Replace(test, @"[<][a-z|A-Z|/](.|)*?[>]", "", RegexOptions.Singleline).Replace("\n","").Replace("<:","").Replace(":>", ""));

                            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            reply.Attachments = new List<Attachment>();
                            for (int i = 0; i < serarchList.display; i++)
                            {
                                string searchTitle = "";
                                string searchText = "";

                                searchTitle = serarchList.items[i].title;
                                searchText = serarchList.items[i].description;

                                if (context.Activity.ChannelId == "facebook")
                                {
                                    searchTitle = Regex.Replace(searchTitle, @"[<][a-z|A-Z|/](.|)*?[>]", "", RegexOptions.Singleline).Replace("\n", "").Replace("<:", "").Replace(":>", "");
                                    searchText = Regex.Replace(searchText, @"[<][a-z|A-Z|/](.|)*?[>]", "", RegexOptions.Singleline).Replace("\n", "").Replace("<:", "").Replace(":>", "");
                                }

                                if (serarchList.items[i].title.Contains("코나"))
                                {
                                    List<CardImage> cardImages = new List<CardImage>();
                                    CardImage img = new CardImage();
                                    img.Url = "";
                                    cardImages.Add(img);

                                    List<CardAction> cardButtons = new List<CardAction>();
                                    CardAction[] plButton = new CardAction[1];
                                    plButton[0] = new CardAction()
                                    {
                                        Value = Regex.Replace(serarchList.items[i].link, "amp;", ""),
                                        Type = "openUrl",
                                        Title = "기사 바로가기"
                                    };
                                    cardButtons = new List<CardAction>(plButton);

                                    if (context.Activity.ChannelId == "facebook")
                                    {
                                        LinkHeroCard card = new LinkHeroCard()
                                        {
                                            Title = searchTitle,
                                            Subtitle = null,
                                            Text = searchText,
                                            Images = cardImages,
                                            Buttons = cardButtons,
                                            Link = null
                                        };
                                        var attachment = card.ToAttachment();
                                        reply.Attachments.Add(attachment);
                                    }
                                    else
                                    {
                                        LinkHeroCard card = new LinkHeroCard()
                                        {
                                            Title = searchTitle,
                                            Subtitle = null,
                                            Text = searchText,
                                            Images = cardImages,
                                            Buttons = null,
                                            Link = Regex.Replace(serarchList.items[i].link, "amp;", "")
                                        };
                                        var attachment = card.ToAttachment();
                                        reply.Attachments.Add(attachment);
                                    }
                                }
                            }
                        }
                        await context.PostAsync(reply);
                        


                        if(reply.Attachments.Count == 0)
                        {

                            await this.SendWelcomeMessageAsync(context);

                            //var reply_err = context.MakeMessage();

                            ////Activity reply_err = context.Activity.CreateReply();
                            //reply_err.Recipient = context.Activity.From;
                            //reply_err.Type = "message";
                            ////reply_err.Text = SorryMessageList.GetSorryMessage(++sorryMessageCnt) + "[ '" +gubunVal+ "','" + entitiesStr + "' ]";
                            //reply_err.Text = SorryMessageList.GetSorryMessage(++sorryMessageCnt);
                            //await context.PostAsync(reply_err);
                        }
                        else
                        {

                            

                            orgKRMent = Regex.Replace(message.Text, @"[^a-zA-Z0-9ㄱ-힣]", "", RegexOptions.Singleline);


                            for (int n = 0; n < Regex.Split(message.Text, " ").Length; n++)
                            {
                                string chgMsg = db.SelectChgMsg(Regex.Split(message.Text, " ")[n]);
                                if (!string.IsNullOrEmpty(chgMsg))
                                {
                                    message.Text = message.Text.Replace(Regex.Split(message.Text, " ")[n], chgMsg);
                                }
                            }


                            Translator translateInfo = await getTranslate(message.Text);

                            orgENGMent = Regex.Replace(translateInfo.data.translations[0].translatedText, @"[^a-zA-Z0-9ㄱ-힣-\s-&#39;]", "", RegexOptions.Singleline);

                            orgENGMent = orgENGMent.Replace("&#39;", "'");

                            int dbResult = db.insertUserQuery(orgKRMent, orgENGMent, "", "", "", 1, 'S', "", "", "", "SEARCH");
                            Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());

                            DateTime endTime = DateTime.Now;

                            Debug.WriteLine("USER NUMBER : " + context.Activity.Conversation.Id);
                            Debug.WriteLine("CUSTOMMER COMMENT KOREAN : " + messgaeText.Replace("코나 ", ""));
                            Debug.WriteLine("CUSTOMMER COMMENT ENGLISH : " + translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));
                            Debug.WriteLine("CHANNEL_ID : " + context.Activity.ChannelId);
                            Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - startTime).Milliseconds));

                            int inserResult = db.insertHistory(context.Activity.Conversation.Id, messgaeText.Replace("코나 ", ""), translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), "SEARCH", context.Activity.ChannelId, ((endTime - startTime).Milliseconds));
                            if (inserResult > 0)
                            {
                                Debug.WriteLine("HISTORY RESULT SUCCESS");
                            }
                            else
                            {
                                Debug.WriteLine("HISTORY RESULT FAIL");
                            }
                            HistoryLog("[ SEARCH ] ==>> userID :: [ " + context.Activity.Conversation.Id + " ]       message :: [ " + messgaeText.Replace("코나 ", "") + " ]       date :: [ " + DateTime.Now + " ]");
                        }
                        
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Error 발생=" + status);
                        await this.SendWelcomeMessageAsync(context);
                    }

                    context.Done(messgaeText);

                }
            }
            else
            {
                await this.SendWelcomeMessageAsync(context);  
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


        private async Task SendWelcomeMessageAsync(IDialogContext context)
        {
            //await context.PostAsync("코나 추천을 시작합니다.");
            // Db
            DbConnect db = new DbConnect();

            Debug.WriteLine("root before sorry count : " + MessagesController.sorryMessageCnt);
            HistoryLog("root before sorry count : " + MessagesController.sorryMessageCnt);


            int sorryMessageCheck = db.SelectUserQueryErrorMessageCheck(context.Activity.Conversation.Id);

            ++MessagesController.sorryMessageCnt;
            //var reply_err = context.MakeMessage();

            //Activity reply_err = context.Activity.CreateReply();
            //reply_err.Recipient = context.Activity.From;
            //reply_err.Type = "message";
            ////reply_err.Text = SorryMessageList.GetSorryMessage(++sorryMessageCnt) + "[ '" +gubunVal+ "','" + entitiesStr + "' ]";
            //reply_err.Text = SorryMessageList.GetSorryMessage(++sorryMessageCnt);
            //await context.PostAsync(reply_err);

            var reply_err = context.MakeMessage();
            reply_err.Recipient = context.Activity.From;
            reply_err.Type = "message";

            Debug.WriteLine("root after sorry count : " + MessagesController.sorryMessageCnt);
            HistoryLog("root after sorry count : " + MessagesController.sorryMessageCnt);

            if (sorryMessageCheck == 1)
            //if (MessagesController.sorryMessageCnt > 1)
            {
                reply_err.Attachments = new List<Attachment>();
                reply_err.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                

                reply_err.Attachments.Add(
                MessagesController.GetHeroCard_sorry(
                SorryMessageList.GetSorryMessage(sorryMessageCheck),
                //new CardAction(ActionTypes.OpenUrl, "코나 챗봇 페이스북 바로가기", value: "https://www.facebook.com/hyundaimotorgroup/")
                //new CardAction(ActionTypes.OpenUrl, "코나 챗봇 페이스북 바로가기", value: rootWebConfig.ConnectionStrings.ConnectionStrings[redirectEventPageURL]))
                new CardAction(ActionTypes.OpenUrl, "코나 챗봇 페이스북 바로가기", value: eventURL))
                
                );
            }
            else
            {
                reply_err.Text = SorryMessageList.GetSorryMessage(sorryMessageCheck);
            }

            //reply_err.Text = SorryMessageList.GetSorryMessage(++sorryMessageCnt);
            await context.PostAsync(reply_err);

            //response = Request.CreateResponse(HttpStatusCode.OK);
            //return response;




            //Translator translateInfo = await getTranslate(messgaeText.Replace("코나 ", ""));



            orgKRMent = Regex.Replace(beforeMessgaeText, @"[^a-zA-Z0-9ㄱ-힣]", "", RegexOptions.Singleline);


            for (int n = 0; n < Regex.Split(beforeMessgaeText, " ").Length; n++)
            {
                string chgMsg = db.SelectChgMsg(Regex.Split(beforeMessgaeText, " ")[n]);
                if (!string.IsNullOrEmpty(chgMsg))
                {
                    beforeMessgaeText = beforeMessgaeText.Replace(Regex.Split(beforeMessgaeText, " ")[n], chgMsg);
                }
            }


            Translator translateInfo = await getTranslate(beforeMessgaeText);

            //orgENGMent = Regex.Replace(translateInfo.data.translations[0].translatedText, @"[^a-zA-Z0-9가-힣-\s]", "", RegexOptions.Singleline);
            orgENGMent = Regex.Replace(translateInfo.data.translations[0].translatedText, @"[^a-zA-Z0-9ㄱ-힣-\s-&#39;]", "", RegexOptions.Singleline);

            orgENGMent = orgENGMent.Replace("&#39;", "'");

            int dbResult = db.insertUserQuery(orgKRMent, orgENGMent, "", "", "", 0, 'D', "", "", "", "SEARCH");
            Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());


            DateTime endTime = DateTime.Now;

            Debug.WriteLine("USER NUMBER : " + context.Activity.Conversation.Id);
            Debug.WriteLine("CUSTOMMER COMMENT KOREAN : " + beforeMessgaeText);
            Debug.WriteLine("CUSTOMMER COMMENT ENGLISH : " + translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"));
            Debug.WriteLine("CHANNEL_ID : " + context.Activity.ChannelId);
            Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - startTime).Milliseconds));

            int inserResult = db.insertHistory(context.Activity.Conversation.Id, beforeMessgaeText, translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), "SEARCH", context.Activity.ChannelId, ((endTime - startTime).Milliseconds));
            if (inserResult > 0)
            {
                Debug.WriteLine("HISTORY RESULT SUCCESS");
            }
            else
            {
                Debug.WriteLine("HISTORY RESULT FAIL");
            }

            //response = Request.CreateResponse(HttpStatusCode.OK);
            //return response;


            //context.Call(new UsageDialog(), this.UsageDialogResumeAfter);            
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
                    string strLog = String.Format("{0}: {1}", dtNow.ToString(dtNow.Hour + "시mm분ss초"), strMsg);
                    swStream.WriteLine(strLog);
                    swStream.Close(); ;
                }
            }
            catch (System.Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        //private async Task UsageDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        //{
        //    try
        //    {
        //        this.usage = await result;

        //        context.Call(new ImDialog(), this.ImDialogResumeAfter);
        //    }
        //    catch (TooManyAttemptsException)
        //    {
        //        /*await context.PostAsync("I'm sorry, I'm having issues understanding you. Let's try again.");

        //        await this.SendWelcomeMessageAsync(context);*/
        //    }
        //}

        //private async Task ImDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        //{
        //    try
        //    {
        //        this.importance = await result;

        //        context.Call(new GenderAgeDialog(usage, importance), this.GenderAgeDialogResumeAfter);
        //    }
        //    catch (TooManyAttemptsException)
        //    {
        //        /*await context.PostAsync("I'm sorry, I'm having issues understanding you. Let's try again.");*/
        //    }

        //}

        //private async Task GenderAgeDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        //{
        //    try
        //    {
        //        this.genderAge = await result;

        //    }
        //    catch (TooManyAttemptsException)
        //    {
        //        /*await context.PostAsync("I'm sorry, I'm having issues understanding you. Let's try again.");

        //        await this.SendWelcomeMessageAsync(context);*/
        //    }
        //}
        //private async Task SearchDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        //{
        //    try
        //    {               

        //    }
        //    catch (TooManyAttemptsException)
        //    {

        //    }       
        //}

    }
}