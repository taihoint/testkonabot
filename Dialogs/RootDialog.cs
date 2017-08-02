namespace Bot_Application1.Dialogs

{
    using System;
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

#pragma warning disable 1998

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private string usage;
        private string importance;
        private string genderAge;

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
            var message = await result;
            string messgaeText = "";
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
                Debug.WriteLine("SERARCH MESSAGE : " + message.Text);
                if ((messgaeText != null) && messgaeText.Trim().Length > 0)
                {
                    //Naver Search API
                    string url = "https://openapi.naver.com/v1/search/blog?query=" + messgaeText + "&display=10&start=1&sort=sim"; // JSON result 
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
                                    Text = serarchList.items[0].description.Substring(0,100)+ "...",
                                    Images = cardImages,
                                    Buttons = null,
                                    Link = serarchList.items[0].link
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
                                        Text = serarchList.items[i].description.Substring(0, 100) + "...",
                                        Images = cardImages,
                                        Buttons = null,
                                        Link = serarchList.items[0].link
                                    };
                                    var attachment = card.ToAttachment();
                                    reply.Attachments.Add(attachment);
                                }
                            }
                        }
                        await context.PostAsync(reply);
                        

                        
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Error 발생=" + status);
                    }

                    context.Done(message.Text);
                }
            }
            else
            {
                await this.SendWelcomeMessageAsync(context);  
            }  
        }

        private async Task SendWelcomeMessageAsync(IDialogContext context)
        {
            await context.PostAsync("코나 추천을 시작합니다.");

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