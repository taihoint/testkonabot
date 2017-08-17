namespace BasicMultiDialogBot.Dialogs
{
    using Microsoft.Bot.Builder.Dialogs;
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class ImDialog : IDialog<string>
    {
        private string normal_reply = "(N)";
        private string exit_reply = "(X)";
        private string origin_message;

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("가성비, 안전성, 고급사양 중 어떤 점을 중시하시나요 ?");

            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var PromptOptions = new string[] { "예", "아니오" };

            if ((message.Text != null) && (message.Text.Trim().Length > 0))
            {
                origin_message = message.Text;
                if (message.Text.Contains("가성비") || message.Text.Contains("안전성") || message.Text.Contains("고급사양")) {
                    //2번질문 파라메터를 입력된 값으로 세팅
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
                    //2번질문 파라메터를 기타로 처리
                    context.Done(normal_reply + origin_message);
                    break;
                default:
                    //추천로직에서 나가려는 유저이기때문에 입력된 쿼리를 일반질문으로 이전
                    context.Done(exit_reply + origin_message);
                    break;
            }
        }
    }
}