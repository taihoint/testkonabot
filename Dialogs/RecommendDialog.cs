
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

#pragma warning disable 1998

    [Serializable]
    public class RecommendDialog : IDialog<object>
    {
        public static int sorryMessageCnt = 0;
        public static string newUserID = "";
        public static string beforeUserID = "";
        private string luis_intent;
        private string entitiesStr;
        private DateTime startTime;
        public static string beforeMessgaeText = "";
        public static string messgaeText = "";
        private string orgKRMent;
        private string orgENGMent;
        private string usage;
        private string importance;
        private string genderAge;

        public RecommendDialog(string luis_intent, string entitiesStr, DateTime startTime, string orgKRMent, string orgENGMent)
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
            try
            {
                this.usage = await result;

                context.Call(new ImDialog(), this.ImDialogResumeAfter);
            }
            catch (TooManyAttemptsException)
            {
                /*await context.PostAsync("I'm sorry, I'm having issues understanding you. Let's try again.");

                await this.SendWelcomeMessageAsync(context);*/
            }
        }

        private async Task ImDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                this.importance = await result;

                context.Call(new GenderAgeDialog(usage, importance), this.GenderAgeDialogResumeAfter);
            }
            catch (TooManyAttemptsException)
            {
                /*await context.PostAsync("I'm sorry, I'm having issues understanding you. Let's try again.");*/
            }

        }

        private async Task GenderAgeDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                this.genderAge = await result;

            }
            catch (TooManyAttemptsException)
            {
                /*await context.PostAsync("I'm sorry, I'm having issues understanding you. Let's try again.");

                await this.SendWelcomeMessageAsync(context);*/
            }
        }

        private async Task SearchDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {

            }
            catch (TooManyAttemptsException)
            {

            }
        }

    }
}