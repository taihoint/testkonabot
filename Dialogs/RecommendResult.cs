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
    public class RecommendResult : IDialog<string>
    {
        private string normal_reply = "(N)";
        private string exit_reply = "(X)";
        private string origin_message;
        private string usage;
        private string importance;
        private string genderAge;

        public RecommendResult(string usage, string importance, string genderAge)
        {
            this.usage = usage;
            this.importance = importance;
            this.genderAge = genderAge;
        }

        public async Task StartAsync(IDialogContext context)
        {
            //var message = await result;
            //this.genderAge = message.Text;
            var reply = context.MakeMessage();
            var reply1 = context.MakeMessage();
            //var reply2 = context.MakeMessage();

            //reply.Attachments.Add(
            //GetHeroCard_button(
            //"",
            //"",
            //"고객님께서 선택한 결과에 따라 차량을 추천해 드릴게요",
            //new CardAction(ActionTypes.ImBack, "다시 선택 하기", value: "다시 선택 하기"),
            //new CardAction(ActionTypes.ImBack, "차량 추천 결과 보기", value: "차량 추천 결과 보기")
            //)
            //);

            //await context.PostAsync(reply);

            //context.Done(message.Text);


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

                //첫번쌔 이미지
                List<CardImage> cardImages = new List<CardImage>();
                reply1.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                reply1.Attachments = new List<Attachment>();

                if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_1))
                {
                    reply1.Attachments.Add(
                    GetHeroCard(RecommendList[i].TRIM_DETAIL, "가격: " + RecommendList[i].TRIM_DETAIL_PRICE, "trim",
                        new CardImage(url: "https://bottest.hyundai.com/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_1 + "/00001.jpg"),
                        new CardImage(url: RecommendList[i].OPTION_1_IMG_URL),
                        new CardImage(url: RecommendList[i].OPTION_2_IMG_URL),
                        new CardImage(url: RecommendList[i].OPTION_3_IMG_URL),
                        new CardImage(url: RecommendList[i].OPTION_4_IMG_URL),
                        new CardImage(url: RecommendList[i].OPTION_5_IMG_URL),
                        new CardAction(ActionTypes.ImBack, RecommendList[i].MAIN_COLOR_VIEW_NM, value: RecommendList[i].MAIN_COLOR_VIEW_NM),
                        new CardAction(ActionTypes.ImBack, RecommendList[i].OPTION_5, value: RecommendList[i].OPTION_5),
                        new CardAction(ActionTypes.ImBack, RecommendList[i].OPTION_1, value: RecommendList[i].OPTION_1),
                        new CardAction(ActionTypes.OpenUrl, "견적 바로가기", value: "https://logon.hyundai.com/kr/quotation/main.do?carcode=RV104"))
                    );

                }

                await context.PostAsync(reply1);
                context.Done("");
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
                Buttons = new List<CardAction>() { cardAction1, cardAction2, cardAction3, cardAction4 }

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