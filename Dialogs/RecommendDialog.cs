
namespace BasicMultiDialogBot.Dialogs
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
    using Bot_Application1.DB;

    [Serializable]
    public class RecommendDialog : IDialog<object>
    {
        public static int sorryMessageCnt = 0;
        public static string beforeMessgaeText = "";
        public static string messgaeText = "";
        //private string orgKRMent;
        //private string orgENGMent;
        private string usage;
        private string importance;
        private string genderAge;
        DateTime startTime = DateTime.Now;

        public RecommendDialog()
        {

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
            var message = await result;
            await this.SendWelcomeMessageAsync(context);
        }

        private async Task SendWelcomeMessageAsync(IDialogContext context)
        {
            //await context.PostAsync("코나 추천을 시작합니다.");

            context.Call(new UsageDialog(), this.UsageDialogResumeAfter);
        }

        private async Task UsageDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            DbConnect db = new DbConnect();
            try
            {
                String temp_message = await result;
                String message_header = temp_message.Substring(0, 3);
                String message_body = temp_message.Substring(3);

                switch (message_header)
                {
                    case "(N)":
                        this.usage = message_body;
                        context.Call(new ImDialog(), this.ImDialogResumeAfter);
                        break;
                    default:
                        context.Done(message_body);
                        DateTime endTime = DateTime.Now;
                        //int inserResult = db.insertHistory(context.Activity.Conversation.Id, usage + " | 기타", "", "", context.Activity.ChannelId, ((endTime - startTime).Milliseconds));
                        break;
                }
            }
            catch (TooManyAttemptsException)
            {
                /*await context.PostAsync("I'm sorry, I'm having issues understanding you. Let's try again.");

                await this.SendWelcomeMessageAsync(context);*/
            }
        }

        private async Task ImDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            DbConnect db = new DbConnect();
            try
            {
                String temp_message = await result;
                String message_header = temp_message.Substring(0, 3);
                String message_body = temp_message.Substring(3);

                switch (message_header)
                {
                    case "(N)":
                        this.importance = message_body;
                        context.Call(new GenderAgeDialog(usage, importance), this.GenderAgeDialogResumeAfter);
                        break;
                    default:
                        context.Done(message_body);
                        DateTime endTime = DateTime.Now;
                        //int inserResult = db.insertHistory(context.Activity.Conversation.Id, usage + "|" + importance + "|기타", "", "", context.Activity.ChannelId, ((endTime - startTime).Milliseconds));
                        break;
                }
            }
            catch (TooManyAttemptsException)
            {
                /*await context.PostAsync("I'm sorry, I'm having issues understanding you. Let's try again.");*/
            }

        }

        private async Task GenderAgeDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            this.genderAge = await result;
            DbConnect db = new DbConnect();

            DateTime endTime = DateTime.Now;
            //int inserResult = db.insertHistory(context.Activity.Conversation.Id, usage.Replace("아니오", "기타") + "|" + importance.Replace("아니오", "기타") + "|" + genderAge.Replace("아니오","기타"), "", "", context.Activity.ChannelId, ((endTime - startTime).Milliseconds));
            //try
            //{
            //    //확인 질문에서 아니오일 경우 Done
            //    String temp_message = await result;
            //    String message_header = temp_message.Substring(0,3);
            //    String message_body = temp_message.Substring(3);

            //    switch (message_header)
            //    {
            //        case "(N)":
            //            this.genderAge = message_body;
            //            context.Call(new RecommendResult(usage, importance, genderAge), null);
            //            break;
            //        default:
            //            context.Done(message_body);
            //            break;
            //    }
            //}
            //catch (TooManyAttemptsException)
            //{
            //    /*await context.PostAsync("I'm sorry, I'm having issues understanding you. Let's try again.");

            //    await this.SendWelcomeMessageAsync(context);*/
            //}
        }

    }
}