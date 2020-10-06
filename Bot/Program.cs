using Bot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot
{
    class Program
    {
        static TelegramBotClient botClient = null;
        
        static Dictionary<long, Conversation> dict = new Dictionary<long, Conversation>();

        static void Main(string[] args)
        {
            botClient = new TelegramBotClient("1307723925:AAGoudgP99mVb0BWFlggHojxyJWi5psbfbU");
            botClient.OnMessage += BotClient_OnMessageAsync;         
            botClient.StartReceiving();
            Console.ReadLine();
        }
        private static async void BotClient_OnMessageAsync(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            Console.WriteLine(e.Message.Text);
            switch (dict[e.Message.From.Id].getStato())
            {
                case 1:
                    if (e.Message.Text?.ToUpper() == "ONE")
                    {
                        botClient.SendTextMessageAsync(e.Message.Chat.Id, "1");
                        dict[e.Message.From.Id].setStato(2);
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(e.Message.Chat.Id, "Rinserisci");
                    }
                    break;
                case 2:
                    if (e.Message.Text?.ToUpper() == "TWO")
                    {
                        botClient.SendTextMessageAsync(e.Message.Chat.Id, "2");
                        dict[e.Message.From.Id].setStato(3);
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(e.Message.Chat.Id, "Rinserisci");
                    }
                    break;
                default:
                    botClient.SendTextMessageAsync(e.Message.Chat.Id, "Rinserisci");
                    break;
            }
            await botClient.ForwardMessageAsync(e.Message.Chat.Id, e.Message.Chat.Id, e.Message.MessageId);
            
        }
    }
}



/*List<InlineKeyboardButton> keyboardButtons = new List<InlineKeyboardButton>() { 
new InlineKeyboardButton() { Text = "si" },
                new InlineKeyboardButton() { Text = "no" }
            };

Telegram.Bot.Types.ReplyMarkups.IReplyMarkup replyMarkup = new InlineKeyboardMarkup(keyboardButtons);
botClient.SendTextMessageAsync(e.Message.Chat.Id, "Ti piace?", Telegram.Bot.Types.Enums.ParseMode.Default, true, true, e.Message.MessageId, replyMarkup);

switch (e.Message.Type)
{
    case Telegram.Bot.Types.Enums.MessageType.Unknown:
        break;
    case Telegram.Bot.Types.Enums.MessageType.Text:
        break;
    case Telegram.Bot.Types.Enums.MessageType.Photo:
        break;
    case Telegram.Bot.Types.Enums.MessageType.Audio:
        break;
    case Telegram.Bot.Types.Enums.MessageType.Video:
        break;
    case Telegram.Bot.Types.Enums.MessageType.Voice:
        break;
    case Telegram.Bot.Types.Enums.MessageType.Document:
        break;
    case Telegram.Bot.Types.Enums.MessageType.Sticker:
        break;
    case Telegram.Bot.Types.Enums.MessageType.Location:
        break;
    case Telegram.Bot.Types.Enums.MessageType.Contact:
        break;
    case Telegram.Bot.Types.Enums.MessageType.Venue:
        break;
    case Telegram.Bot.Types.Enums.MessageType.Game:
        break;
    case Telegram.Bot.Types.Enums.MessageType.VideoNote:
        break;
    case Telegram.Bot.Types.Enums.MessageType.Invoice:
        break;
    case Telegram.Bot.Types.Enums.MessageType.SuccessfulPayment:
        break;
    case Telegram.Bot.Types.Enums.MessageType.WebsiteConnected:
        break;
    case Telegram.Bot.Types.Enums.MessageType.ChatMembersAdded:
        break;
    case Telegram.Bot.Types.Enums.MessageType.ChatMemberLeft:
        break;
    case Telegram.Bot.Types.Enums.MessageType.ChatTitleChanged:
        break;
    case Telegram.Bot.Types.Enums.MessageType.ChatPhotoChanged:
        break;
    case Telegram.Bot.Types.Enums.MessageType.MessagePinned:
        break;
    case Telegram.Bot.Types.Enums.MessageType.ChatPhotoDeleted:
        break;
    case Telegram.Bot.Types.Enums.MessageType.GroupCreated:
        break;
    case Telegram.Bot.Types.Enums.MessageType.SupergroupCreated:
        break;
    case Telegram.Bot.Types.Enums.MessageType.ChannelCreated:
        break;
    case Telegram.Bot.Types.Enums.MessageType.MigratedToSupergroup:
        break;
    case Telegram.Bot.Types.Enums.MessageType.MigratedFromGroup:
        break;
    case Telegram.Bot.Types.Enums.MessageType.Animation:
        break;
    case Telegram.Bot.Types.Enums.MessageType.Poll:
        break;
    case Telegram.Bot.Types.Enums.MessageType.Dice:
        break;
}
        }*/
