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
        static ChatId matNanoChannel = -1001227167038;
        static ChatId aesChannel = -1001413802045;
        static ChatId infoChannel = -1001422638605;
        static ChatId mobilityMDChannel = -1001401676534;
        static ChatId electronicsChannel = -1001165704108;
        static ChatId automationChannel = -1001541302296;
        static ChatId chimicaChannel = -1001510001243;
        static ChatId debug = -1001403617749;

        internal static ChatId getChannel(string v)
        {
            switch (v.ToLower())
            {
                case "matnano":
                    return ChannelsForApproval.matNanoChannel;
                    break;
                case "info":
                    return ChannelsForApproval.infoChannel;
                    break;
                case "mobilitymd":
                    return ChannelsForApproval.mobilityMDChannel;
                    break;
                case "aes":
                    return ChannelsForApproval.aesChannel;
                    break;
                case "electronics":
                    return ChannelsForApproval.electronicsChannel;
                    break;
                case "automazione":
                    return ChannelsForApproval.automationChannel;
                    break;
                case "chimica":
                    return ChannelsForApproval.chimicaChannel;
                    break;
                default:
                    throw new NotImplementedException();
                    return ChannelsForApproval.defaultChannel;
            }
        }
    }
}
