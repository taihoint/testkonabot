using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{
    public static partial class Extensions
    {

        //public static Attachment ToAttachment(this VideoCard card)
        //{
        //    return CreateAttachment(card, VideoCard.ContentType);
        //}
        //public static Attachment ToAttachment(this HeroCard card)
        //{
        //    return CreateAttachment(card, HeroCard.ContentType);
        //}
        private static Attachment CreateAttachment<T>(T card, string contentType)
        {
            return new Attachment
            {
                Content = card,
                ContentType = contentType
            };
        }
    }
}