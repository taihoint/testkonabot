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

#pragma warning disable 1998

    [Serializable]
    public class RootDialog : IDialog<object>
    {

        public static int sorryMessageCnt = 0;
        public static string newUserID = "";
        public static string beforeUserID = "";
        private string luis_intent;
        private string entitiesStr;
        public static string messgaeText = "";

        public RootDialog(string luis_intent, string entitiesStr)
        {
            this.luis_intent = luis_intent;
            this.entitiesStr = entitiesStr;
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

            // Db
            DbConnect db = new DbConnect();

            Debug.WriteLine("activity : " + context.Activity.Conversation.Id);


            newUserID = context.Activity.Conversation.Id;
            if (beforeUserID != newUserID)
            {
                beforeUserID = newUserID;
                sorryMessageCnt = 0;
            }

            

            var message = await result;
            
            if (message.Text.StartsWith("코나") == true)
            {
                messgaeText = message.Text;
            }
            else
            {
                messgaeText = "코나 " + message.Text;
            }

            if (messgaeText.StartsWith("코나") == true)
            //if (message.Text != "")
            {
                //context.Call(new SearchDialog(), this.SearchDialogResumeAfter);
                //string messgaeText = message.Text.Substring(3, message.Text.Length - 3);             
                var reply = context.MakeMessage();
                Debug.WriteLine("SERARCH MESSAGE : " + messgaeText);
                if ((messgaeText != null) && messgaeText.Trim().Length > 0)
                {
                    //Naver Search API
                    string url = "https://openapi.naver.com/v1/search/news.json?query=" + messgaeText + "&display=10&start=1&sort=sim"; // JSON result 
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
                            if (serarchList.items[0].title.Contains("코나"))
                            {
                                //Only One item
                                List<CardImage> cardImages = new List<CardImage>();
                                CardImage img = new CardImage();
                                img.Url = "https://bottest.hyundai.com/assets/images/preview.jpg";
                                cardImages.Add(img);
                                LinkHeroCard card = new LinkHeroCard()
                                {
                                    Title = serarchList.items[0].title,
                                    Subtitle = null,
                                    Text = serarchList.items[0].description,
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
                            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            reply.Attachments = new List<Attachment>();
                            for (int i = 0; i < serarchList.display; i++)
                            {
                                if (serarchList.items[i].title.Contains("코나"))
                                {
                                    List<CardImage> cardImages = new List<CardImage>();
                                    CardImage img = new CardImage();
                                    img.Url = "https://bottest.hyundai.com/assets/images/preview.jpg";
                                    cardImages.Add(img);
                                    LinkHeroCard card = new LinkHeroCard()
                                    {
                                        Title = serarchList.items[i].title,
                                        Subtitle = null,
                                        Text = serarchList.items[i].description,
                                        Images = cardImages,
                                        Buttons = null,
                                        Link = Regex.Replace(serarchList.items[i].link, "amp;", "")
                                    };
                                    var attachment = card.ToAttachment();
                                    reply.Attachments.Add(attachment);
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
                            Translator translateInfo = await getTranslate(messgaeText);

                            int dbResult = db.insertUserQuery(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), luis_intent, entitiesStr, 1, 'S', "", "", "", "SEARCH");
                            Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());
                        }
                        
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Error 발생=" + status);
                        await this.SendWelcomeMessageAsync(context);
                    }

                    context.Done(message.Text);

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

            var reply_err = context.MakeMessage();

            //Activity reply_err = context.Activity.CreateReply();
            reply_err.Recipient = context.Activity.From;
            reply_err.Type = "message";
            //reply_err.Text = SorryMessageList.GetSorryMessage(++sorryMessageCnt) + "[ '" +gubunVal+ "','" + entitiesStr + "' ]";
            reply_err.Text = SorryMessageList.GetSorryMessage(++sorryMessageCnt) + "[ '" + luis_intent + "','" + entitiesStr + "' ]";
            await context.PostAsync(reply_err);
            
            Translator translateInfo = await getTranslate(messgaeText);

            int dbResult = db.insertUserQuery(translateInfo.data.translations[0].translatedText.Replace("&#39;", "'"), "", "", 0, 'D', "", "", "", "");
            Debug.WriteLine("INSERT QUERY RESULT : " + dbResult.ToString());

            //response = Request.CreateResponse(HttpStatusCode.OK);
            //return response;


            //context.Call(new UsageDialog(), this.UsageDialogResumeAfter);            
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