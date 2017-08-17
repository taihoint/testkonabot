﻿
namespace BasicMultiDialogBot.Dialogs
{
    using Microsoft.Bot.Builder.Dialogs;
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Bot_Application1.Models;
    using Bot_Application1.DB;


    [Serializable]
    public class GenderAgeDialog : IDialog<string>
    {
        private string normal_reply = "(N)";
        private string exit_reply = "(X)";
        private string origin_message;

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Kona를 이용하실 분의 연령대와 성별이 어떻게 되세요 ?");

            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var PromptOptions = new string[] { "예", "아니오" };

            if ((message.Text != null) && (message.Text.Trim().Length > 0))
            {
                origin_message = message.Text;
                if (message.Text.Contains("남자") || message.Text.Contains("여자") || message.Text.Contains("남성") || message.Text.Contains("여성")) {
                    //3번질문 파라메터를 입력된 값으로 세팅
                    context.Done(normal_reply + origin_message);
                } else {
                    PromptDialog.Choice(
                        context,
                        AfterConfirmAsync,
                        PromptOptions,
                        "제가 잘 이해를 못했네요 입력하신 내용이 추천 관련된 내용인가요?",
                        promptStyle: PromptStyle.Auto);
                }
            }
        }

		private async Task AfterConfirmAsync(IDialogContext context, IAwaitable<string> argument)
        {
            string optionSelected = await argument;
            switch (optionSelected)
            {
                case "예":
                    //1번질문 파라메터를 기타로 처리
                    context.Done(normal_reply + origin_message);
                    break;
                default:
                    //추천로직에서 나가려는 유저이기때문에 입력된 쿼리를 일반질문으로 이전
                    context.Done(exit_reply + origin_message);
                    break;
            }
        }

        private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage1, CardImage cardImage2, CardImage cardImage3, CardImage cardImage4, CardImage cardImage5, CardImage cardImage6, CardAction cardAction1, CardAction cardAction2, CardAction cardAction3, CardAction cardAction4)
        {
            var heroCard = new UserHeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage1, cardImage2, cardImage3, cardImage4, cardImage5, cardImage6 },
                Buttons = new List<CardAction>() { cardAction1, cardAction2, cardAction3, cardAction4 },

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
    }
}