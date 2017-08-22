using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace BasicMultiDialogBot.Dialogs
{
    [Serializable]
    public class EchoChainDialog
    {
        public static readonly IDialog<string> dialog = Chain
            .PostToChain()
            //.Select(p => p.Text)
            .Switch(
                new Case<IMessageActivity, IDialog<string>>((msg) =>
                {
                    var regex = new Regex("^코나 추천!", RegexOptions.IgnoreCase);
                    return regex.IsMatch(msg.Text);
                }, (ctx, msg) =>
                {
                    // User wants to login, send the message to Facebook Auth Dialog
                    return Chain.From(() => new StageDialog())
                    .ContinueWith(async (context, result) =>
                    {
                        var sresult = await result;
                        return Chain.ContinueWith(new Stage0Dialog(), async (c0, r0) =>
                        {
                            var s0result = await r0;
                            return Chain.Return(s0result);
                            
                        });
                    });

                }),
                new DefaultCase<IMessageActivity, IDialog<string>>((ctx, msg) =>
                {
                    return Chain.Return("어떤 말씀이신지?");
                }
            ))
            .Unwrap()
            .PostToUser();
    }
}