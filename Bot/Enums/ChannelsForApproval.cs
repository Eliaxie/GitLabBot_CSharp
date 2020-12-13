using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.Enums
{
    class ChannelsForApproval
    {
        static ChatId defaultChannel = 0;
        static ChatId matNanoChannel = -1001403617749;
   
        internal static ChatId getChannel(string v)
        {
            switch (v.ToLower())
            {
                case "matnano":
                    return ChannelsForApproval.matNanoChannel;
                    break;
                default:
                    throw new NotImplementedException();
                    return ChannelsForApproval.defaultChannel;
            }
        }
    }
}
