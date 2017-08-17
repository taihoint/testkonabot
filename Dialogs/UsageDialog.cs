namespace BasicMultiDialogBot.Dialogs
{
    using Microsoft.Bot.Builder.Dialogs;
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class UsageDialog : IDialog<string>
    {
        private string normal_reply = "(N)";
        private string exit_reply = "(X)";
        private string origin_message;

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Kona를 주로 어떤 용도로 사용하실 계획이세요?\n\n(예) 출퇴근, 장거리 이동)");

            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var PromptOptions = new string[] { "예", "아니오" };

            if ((message.Text != null) && (message.Text.Trim().Length > 0))
            {
                origin_message = message.Text;
                if (message.Text.Contains("출퇴근") || message.Text.Contains("출근") || message.Text.Contains("퇴근") || message.Text.Contains("장거리") || message.Text.Contains("통학")) {
                    //1번질문 파라메터를 입력된 값으로 세팅
                    //정상값이 입력되었을때 구분자를 붙여서 리턴
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
    }
}