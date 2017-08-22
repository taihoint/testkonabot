using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace BasicMultiDialogBot.Dialogs
{
    [Serializable]
    public class Stage0Dialog : IDialog<string>
    {
        public async Task StartAsync(IDialogContext context)
        {
            var cat1 = "s1";
            await context.PostAsync(cat1);

            context.Done("");
        }
        
    }
}