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
            this.genderAge = message.Text;
            var reply = context.MakeMessage();

            // Db
            DbConnect db = new DbConnect();
            //List<RecommendList> RecommendList = db.SelectRecommendList();
            List<RecommendList> RecommendList = db.SelectedRecommendList(usage, importance, genderAge);
            RecommendList recommend = new RecommendList();

            //입력받은 단어들로 3가지 질문에 모두 일치 하는 항목이 있을 경우의 값을 리스트에 담고 Break
            for (var i = 0; i < RecommendList.Count; i++)
            {
                //첫번쌔 이미지
                List<CardImage> cardImages = new List<CardImage>();
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                reply.Attachments = new List<Attachment>();

                if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_1))
                {
                    reply.Attachments.Add(
                    GetHeroCard(RecommendList[i].TRIM_DETAIL, "가격: " + RecommendList[i].TRIM_DETAIL_PRICE, "trim",
                    new CardImage(url: "https://bottest.hyundai.com/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_1 + "/00001.jpg"),
                    new CardImage(url: RecommendList[i].OPTION_1_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_2_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_3_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_4_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_5_IMG_URL))
                   );
                }

                if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_2))
                {
                    reply.Attachments.Add(
                    GetHeroCard(RecommendList[i].TRIM_DETAIL, "가격: " + RecommendList[i].TRIM_DETAIL_PRICE, "trim",
                    new CardImage(url: "https://bottest.hyundai.com/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_2 + "/00001.jpg"),
                    new CardImage(url: RecommendList[i].OPTION_1_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_2_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_3_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_4_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_5_IMG_URL))
                   );
                }

                if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_3))
                {
                    reply.Attachments.Add(
                    GetHeroCard(RecommendList[i].TRIM_DETAIL, "가격: " + RecommendList[i].TRIM_DETAIL_PRICE, "trim",
                    new CardImage(url: "https://bottest.hyundai.com/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_3 + "/00001.jpg"),
                    new CardImage(url: RecommendList[i].OPTION_1_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_2_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_3_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_4_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_5_IMG_URL))
                   );
                }

                if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_4))
                {
                    reply.Attachments.Add(
                    GetHeroCard(RecommendList[i].TRIM_DETAIL, "가격: " + RecommendList[i].TRIM_DETAIL_PRICE, "trim",
                    new CardImage(url: "https://bottest.hyundai.com/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_4 + "/00001.jpg"),
                    new CardImage(url: RecommendList[i].OPTION_1_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_2_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_3_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_4_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_5_IMG_URL))
                   );
                }

                if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_5))
                {
                    reply.Attachments.Add(
                    GetHeroCard(RecommendList[i].TRIM_DETAIL, "가격: " + RecommendList[i].TRIM_DETAIL_PRICE, "trim",
                    new CardImage(url: "https://bottest.hyundai.com/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_5 + "/00001.jpg"),
                    new CardImage(url: RecommendList[i].OPTION_1_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_2_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_3_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_4_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_5_IMG_URL))
                   );
                }

                if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_6))
                {
                    reply.Attachments.Add(
                    GetHeroCard(RecommendList[i].TRIM_DETAIL, "가격: " + RecommendList[i].TRIM_DETAIL_PRICE, "trim",
                    new CardImage(url: "https://bottest.hyundai.com/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_6 + "/00001.jpg"),
                    new CardImage(url: RecommendList[i].OPTION_1_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_2_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_3_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_4_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_5_IMG_URL))
                   );
                }

                if (!string.IsNullOrEmpty(RecommendList[i].MAIN_COLOR_VIEW_7))
                {
                    reply.Attachments.Add(
                    GetHeroCard(RecommendList[i].TRIM_DETAIL, "가격: " + RecommendList[i].TRIM_DETAIL_PRICE, "trim",
                    new CardImage(url: "https://bottest.hyundai.com/assets/images/price/360/" + RecommendList[i].MAIN_COLOR_VIEW_7 + "/00001.jpg"),
                    new CardImage(url: RecommendList[i].OPTION_1_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_2_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_3_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_4_IMG_URL),
                    new CardImage(url: RecommendList[i].OPTION_5_IMG_URL))
                   );
                }

                await context.PostAsync(reply);
            }

            List<CardAction> cardButtons = new List<CardAction>();
            //List<CardImage> cardImages_reply = new List<CardImage>();
            cardButtons.Add(new CardAction()
            {
                Value = "다시 선택 하기",
                Type = ActionTypes.ImBack,
                Title = "다시 선택 하기"
            });
            cardButtons.Add(new CardAction()
            {
                Value = "차량 추천 결과 보기",
                Type = ActionTypes.ImBack,
                Title = "차량 추천 결과 보기"
            });
            HeroCard card_reply = new HeroCard()
            {
                Title = null,
                Subtitle = null,
                Text = "고객님께서 선택한 결과에 따라 차량을 추천해 드릴게요",
                //Images = cardImages,
                Buttons = cardButtons
            };
            var attachment_reply = card_reply.ToAttachment();

            reply.Attachments = new List<Attachment>();
            reply.Attachments.Add(attachment_reply);

            await context.PostAsync(reply);

            context.Done(message.Text);


            //hard coding start
            /*
            if (usage == "출퇴근")
            {
                List<CardImage> cardImages = new List<CardImage>();
                CardImage img = new CardImage();
                img.Url = "file:///C:/Users/Taiho/Desktop/image/bsd.JPG";
                cardImages.Add(img);
                HeroCard card = new HeroCard()
                {
                    Title = null,
                    Subtitle = null,
                    Text = "BSD, HUD",
                    Images = cardImages,
                    Buttons = null
                };
                var attachment = card.ToAttachment();

                reply.Attachments = new List<Attachment>();
                reply.Attachments.Add(attachment);

                await context.PostAsync(reply);
            }
            else
            {
                List<CardImage> cardImages = new List<CardImage>();
                CardImage img = new CardImage();
                img.Url = "file:///C:/Users/Taiho/Desktop/image/csd.JPG";
                cardImages.Add(img);
                HeroCard card = new HeroCard()
                {
                    Title = null,
                    Subtitle = null,
                    Text = "CSD, IUD",
                    Images = cardImages,
                    Buttons = null
                };
                var attachment = card.ToAttachment();

                reply.Attachments = new List<Attachment>();
                reply.Attachments.Add(attachment);

                await context.PostAsync(reply);
            }

            if (importance == "고급 사양")
            {
                List<CardImage> cardImages = new List<CardImage>();
                CardImage img = new CardImage();
                img.Url = "file:///C:/Users/Taiho/Desktop/image/wheel.JPG";
                cardImages.Add(img);
                HeroCard card = new HeroCard()
                {
                    Title = null,
                    Subtitle = null,
                    Text = "휠, 투톤루프",
                    Images = cardImages,
                    Buttons = null
                };
                var attachment = card.ToAttachment();

                reply.Attachments = new List<Attachment>();
                reply.Attachments.Add(attachment);

                await context.PostAsync(reply);
            }
            else
            {
                List<CardImage> cardImages = new List<CardImage>();
                CardImage img = new CardImage();
                img.Url = "file:///C:/Users/Taiho/Desktop/image/handle.JPG";
                cardImages.Add(img);
                HeroCard card = new HeroCard()
                {
                    Title = null,
                    Subtitle = null,
                    Text = "핸들, 싱글루프",
                    Images = cardImages,
                    Buttons = null
                };
                var attachment = card.ToAttachment();

                reply.Attachments = new List<Attachment>();
                reply.Attachments.Add(attachment);

                await context.PostAsync(reply);
            }

            if (genderAge == "30대 남성")
            {
                List<CardImage> cardImages = new List<CardImage>();
                CardImage img = new CardImage();
                img.Url = "file:///C:/Users/Taiho/Desktop/image/inner.JPG";
                cardImages.Add(img);
                HeroCard card = new HeroCard()
                {
                    Title = null,
                    Subtitle = null,
                    Text = "실내 디자인",
                    Images = cardImages,
                    Buttons = null
                };
                var attachment = card.ToAttachment();

                reply.Attachments = new List<Attachment>();
                reply.Attachments.Add(attachment);

                await context.PostAsync(reply);
            }
            else
            {
                List<CardImage> cardImages = new List<CardImage>();
                CardImage img = new CardImage();
                img.Url = "file:///C:/Users/Taiho/Desktop/image/outer.JPG";
                cardImages.Add(img);
                HeroCard card = new HeroCard()
                {
                    Title = null,
                    Subtitle = null,
                    Text = "실외 디자인",
                    Images = cardImages,
                    Buttons = null
                };
                var attachment = card.ToAttachment();

                reply.Attachments = new List<Attachment>();
                reply.Attachments.Add(attachment);

                await context.PostAsync(reply);
            }
            */

            //    if (usage == "출퇴근" || usage == "장거리 이동")
            //    {
            //        Debug.WriteLine("usage ::::::::::::::::::::::::::::::::::::::::::::: " + usage);
            //    }
            //    else
            //    {
            //        Debug.WriteLine("usage ::::::::::::::::::::::::::::::::::::::::::::: " + usage);
            //    }

            //    if (importance == "가성비" || importance == "안전성" || importance == "고급사양")
            //    {
            //        Debug.WriteLine("importance ::::::::::::::::::::::::::::::::::::::::::::: " + importance);
            //    }
            //    else
            //    {
            //        Debug.WriteLine("importance ::::::::::::::::::::::::::::::::::::::::::::: " + importance);
            //    }

            //    if (genderAge == "남성" || genderAge == "여성")
            //    {
            //        Debug.WriteLine("genderAge ::::::::::::::::::::::::::::::::::::::::::::: " + genderAge);
            //    }
            //    else
            //    {
            //        Debug.WriteLine("genderAge ::::::::::::::::::::::::::::::::::::::::::::: " + genderAge);
            //    }

            //    if (usage == "1" && importance == "1" && genderAge == "1")
            //    {
            //        List<CardImage> cardImages = new List<CardImage>();
            //        CardImage img = new CardImage();
            //        img.Url = "https://bottest.hyundai.com/assets/images/price/360/phantom black/00001.jpg";
            //        cardImages.Add(img);
            //        img = new CardImage();
            //        img.Url = "https://bottest.hyundai.com/assets/images/price/option/Infotainment.jpg";
            //        cardImages.Add(img);
            //        img = new CardImage();
            //        img.Url = "https://bottest.hyundai.com/assets/images/price/option/SmartSense_III.jpg";
            //        cardImages.Add(img);
            //        img = new CardImage();
            //        img.Url = "https://bottest.hyundai.com/assets/images/price/option/Sunroof.jpg";
            //        cardImages.Add(img);
            //        HeroCard card = new HeroCard()
            //        {
            //            Title = "코나 TUIX 가솔린 / 프리미엄 7단 DCT",
            //            Subtitle = "가격: 24,850,000 원",
            //            Text = "trim",
            //            Images = cardImages,
            //            Buttons = null
            //        };
            //        List<CardImage> cardImages2 = new List<CardImage>();
            //        CardImage img2 = new CardImage();
            //        img2.Url = "https://bottest.hyundai.com/assets/images/price/360/chalk white/00001.jpg";
            //        cardImages2.Add(img2);
            //        img2 = new CardImage();
            //        img2.Url = "https://bottest.hyundai.com/assets/images/safety/USP_safety_08.jpg";
            //        cardImages2.Add(img2);
            //        img2 = new CardImage();
            //        img2.Url = "https://bottest.hyundai.com/assets/images/safety/USP_safety_02.jpg";
            //        cardImages2.Add(img2);
            //        img2 = new CardImage();
            //        img2.Url = "https://bottest.hyundai.com/assets/images/safety/USP_safety_05.jpg";
            //        cardImages2.Add(img2);
            //        img2 = new CardImage();
            //        img2.Url = "https://bottest.hyundai.com/assets/images/safety/USP_safety_01.jpg";
            //        cardImages2.Add(img2);
            //        var attachment = card.ToAttachment();
            //        HeroCard card2 = new HeroCard()
            //        {
            //            Title = "코나 TUIX 디젤 / 모던 2단 DCT",
            //            Subtitle = "가격: 18,150,000 원",
            //            Text = "trim",
            //            Images = cardImages2,
            //            Buttons = null
            //        };
            //        var attachment2 = card2.ToAttachment();
            //        List<CardImage> cardImages3 = new List<CardImage>();
            //        CardImage img3 = new CardImage();
            //        img3.Url = "https://bottest.hyundai.com/assets/images/price/360/ceramic blue/00001.jpg";
            //        cardImages3.Add(img3);
            //        img3 = new CardImage();
            //        img3.Url = "https://bottest.hyundai.com/assets/images/convenience/USP_convenience_01.jpg";
            //        cardImages3.Add(img3);
            //        img3 = new CardImage();
            //        img3.Url = "https://bottest.hyundai.com/assets/images/convenience/USP_convenience_02.jpg";
            //        cardImages3.Add(img3);
            //        img3 = new CardImage();
            //        img3.Url = "https://bottest.hyundai.com/assets/images/convenience/USP_convenience_03.jpg";
            //        cardImages3.Add(img3);
            //        img3 = new CardImage();
            //        img3.Url = "https://bottest.hyundai.com/assets/images/convenience/USP_convenience_04.jpg";
            //        cardImages3.Add(img3);
            //        img3 = new CardImage();
            //        img3.Url = "https://bottest.hyundai.com/assets/images/convenience/USP_convenience_05.jpg";
            //        cardImages3.Add(img3);
            //        img3 = new CardImage();
            //        img3.Url = "https://bottest.hyundai.com/assets/images/convenience/USP_convenience_06.jpg";
            //        cardImages3.Add(img3);
            //        HeroCard card3 = new HeroCard()
            //        {
            //            Title = "코나 TUIX 가솔린 / 프리미엄 1단 DCT",
            //            Subtitle = "가격: 20,000,000 원",
            //            Text = "trim",
            //            Images = cardImages3,
            //            Buttons = null
            //        };
            //        var attachment3 = card3.ToAttachment();

            //        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            //        reply.Attachments = new List<Attachment>();
            //        reply.Attachments.Add(attachment);
            //        reply.Attachments.Add(attachment2);
            //        reply.Attachments.Add(attachment3);
            //        await context.PostAsync(reply);
            //    }
            //    else
            //    {
            //        List<CardImage> cardImages = new List<CardImage>();
            //        CardImage img = new CardImage();
            //        img.Url = "https://bottest.hyundai.com/assets/images/price/trim/modernart.jpg";
            //        cardImages.Add(img);
            //        HeroCard card = new HeroCard()
            //        {
            //            Title = "코나 TUIX 디젤 / 프리미엄 3단 DCT",
            //            Subtitle = "가격: 20,500,000 원",
            //            Text = "trim",
            //            Images = cardImages,
            //            Buttons = null
            //        };
            //        var attachment = card.ToAttachment();
            //        HeroCard card2 = new HeroCard()
            //        {
            //            Title = "코나 TUIX 가솔린 / 모던 2단 DCT",
            //            Subtitle = "가격: 17,500,000 원",
            //            Text = "trim",
            //            Images = cardImages,
            //            Buttons = null
            //        };
            //        var attachment2 = card2.ToAttachment();
            //        HeroCard card3 = new HeroCard()
            //        {
            //            Title = "코나 TUIX 가솔린 / 프리미엄 7단 DCT",
            //            Subtitle = "가격: 19,850,000 원",
            //            Text = "trim",
            //            Images = cardImages,
            //            Buttons = null
            //        };
            //        var attachment3 = card3.ToAttachment();

            //        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            //        reply.Attachments = new List<Attachment>();
            //        reply.Attachments.Add(attachment);
            //        reply.Attachments.Add(attachment2);
            //        reply.Attachments.Add(attachment3);

            //        await context.PostAsync(reply);
            //    }


            //    //hard coding end
            //    if ((message.Text != null) && (message.Text.Trim().Length > 0))
            //    {
            //        List<CardAction> cardButtons = new List<CardAction>();
            //        List<CardImage> cardImages = new List<CardImage>();
            //        cardButtons.Add(new CardAction()
            //        {
            //            Value = "다시 선택 하기",
            //            Type = ActionTypes.ImBack,
            //            Title = "다시 선택 하기"
            //        });
            //        cardButtons.Add(new CardAction()
            //        {
            //            Value = "차량 추천 결과 보기",
            //            Type = ActionTypes.ImBack,
            //            Title = "차량 추천 결과 보기"
            //        });
            //        HeroCard card = new HeroCard()
            //        {
            //            Title = null,
            //            Subtitle = null,
            //            Text = "고객님께서 선택한 결과에 따라 차량을 추천해 드릴게요",
            //            Images = cardImages,
            //            Buttons = cardButtons
            //        };
            //        var attachment = card.ToAttachment();

            //        reply.Attachments = new List<Attachment>();
            //        reply.Attachments.Add(attachment);

            //        await context.PostAsync(reply);

            //        context.Done(message.Text);
            //    }
            //    /*else
            //    {
            //        --attempts;
            //        if (attempts > 0)
            //        {
            //            await context.PostAsync("I'm sorry, I don't understand your reply. What is your age (e.g. '42')?");

            //            context.Wait(this.MessageReceivedAsync);
            //        }
            //        else
            //        {
            //            context.Fail(new TooManyAttemptsException("Message was not a valid age."));
            //        }
            //    }*/
        }

        private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage1, CardImage cardImage2, CardImage cardImage3, CardImage cardImage4, CardImage cardImage5, CardImage cardImage6)
        {
            var heroCard = new UserHeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage1, cardImage2, cardImage3, cardImage4, cardImage5, cardImage6 }

            };
            return heroCard.ToAttachment();
        }
    }
}