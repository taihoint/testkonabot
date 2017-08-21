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

        private string origin_message;
        private string usage;
        private string importance;
        private string genderAge;

        public GenderAgeDialog(string usage, string importance)
        {
            this.usage = usage;
            this.importance = importance;
        }

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
                this.genderAge = message.Text;
                if (message.Text.Contains("남자") || message.Text.Contains("여자") || message.Text.Contains("남성") || message.Text.Contains("여성")) {
                    //3번질문 파라메터를 입력된 값으로 세팅
                    
                    //this.genderAge = message.Text;

                    var reply = context.MakeMessage();
                    var reply1 = context.MakeMessage();

                    // Db
                    DbConnect db = new DbConnect();
                    //List<RecommendList> RecommendList = db.SelectRecommendList();
                    List<RecommendList> RecommendList = db.SelectedRecommendList(usage, importance, genderAge);
                    RecommendList recommend = new RecommendList();

                    //입력받은 단어들로 3가지 질문에 모두 일치 하는 항목이 있을 경우의 값을 리스트에 담고 Break
                    for (var i = 0; i < RecommendList.Count; i++)
                    {
                        reply.Attachments.Add(
                        GetHeroCard_button(
                        "trim",
                        RecommendList[i].TRIM_DETAIL + "|" + "가격: " + RecommendList[i].TRIM_DETAIL_PRICE + "|" +
                        "https://bottest.hyundai.com/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_1 + "/00001.jpg" + "|" +
                        RecommendList[i].OPTION_1_IMG_URL + "|" +
                        RecommendList[i].OPTION_2_IMG_URL + "|" +
                        RecommendList[i].OPTION_3_IMG_URL + "|" +
                        RecommendList[i].OPTION_4_IMG_URL + "|" +
                        RecommendList[i].OPTION_5_IMG_URL + "|" +
                        RecommendList[i].MAIN_COLOR_VIEW_NM
                        ,
                        "고객님께서 선택한 결과에 따라 차량을 추천해 드릴게요",
                        new CardAction(ActionTypes.ImBack, "다시 선택 하기", value: "다시 선택 하기"),
                        new CardAction(ActionTypes.ImBack, "차량 추천 결과 보기", value: "차량 추천 결과 보기")
                        )
                        );

                        await context.PostAsync(reply);
                        context.Done(message.Text);                  

                    }

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
            //switch (optionSelected)
            //{
            //    case "예":
            //        //1번질문 파라메터를 기타로 처리

            //        //////////////////////////////////////////////////////////////////////////////////
            //        context.Done(normal_reply + origin_message);
            //        break;
            //    default:
            //        //추천로직에서 나가려는 유저이기때문에 입력된 쿼리를 일반질문으로 이전
            //        context.Done(exit_reply + origin_message);
            //        break;
            //}
            if (optionSelected.Equals("예"))
            {                
                var reply = context.MakeMessage();
                var reply1 = context.MakeMessage();

                // Db
                DbConnect db = new DbConnect();
                //List<RecommendList> RecommendList = db.SelectRecommendList();
                List<RecommendList> RecommendList = db.SelectedRecommendList(usage, importance, genderAge);
                RecommendList recommend = new RecommendList();

                //입력받은 단어들로 3가지 질문에 모두 일치 하는 항목이 있을 경우의 값을 리스트에 담고 Break
                for (var i = 0; i < RecommendList.Count; i++)
                {
                    reply.Attachments.Add(
                    GetHeroCard_button(
                    "trim",
                    RecommendList[i].TRIM_DETAIL + "|" + "가격: " + RecommendList[i].TRIM_DETAIL_PRICE + "|" +
                    "https://bottest.hyundai.com/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_1 + "/00001.jpg" + "|" +
                    RecommendList[i].OPTION_1_IMG_URL + "|" +
                    RecommendList[i].OPTION_2_IMG_URL + "|" +
                    RecommendList[i].OPTION_3_IMG_URL + "|" +
                    RecommendList[i].OPTION_4_IMG_URL + "|" +
                    RecommendList[i].OPTION_5_IMG_URL + "|" +
                    RecommendList[i].MAIN_COLOR_VIEW_NM
                    ,
                    "고객님께서 선택한 결과에 따라 차량을 추천해 드릴게요",
                    new CardAction(ActionTypes.ImBack, "다시 선택 하기", value: "다시 선택 하기"),
                    new CardAction(ActionTypes.ImBack, "차량 추천 결과 보기", value: "차량 추천 결과 보기")
                    )
                    );

                    await context.PostAsync(reply);
                    context.Done(optionSelected);
                }

            }
            else
            {
                context.Done(optionSelected);
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