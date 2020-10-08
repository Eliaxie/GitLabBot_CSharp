using Bot;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot
{
    class Program
    {
        static TelegramBotClient botClient = null;

        static Dictionary<long, Conversation> dict = new Dictionary<long, Conversation>(); //inizializzazione del dizionario <utente, Conversation>

        static void Main(string[] args)
        {
            botClient = new TelegramBotClient("1307723925:AAGoudgP99mVb0BWFlggHojxyJWi5psbfbU");
            botClient.OnMessage += BotClient_OnMessageAsync; //gestisce i messaggi in entrata
            botClient.OnCallbackQuery += BotOnCallbackQueryReceived; //gestisce le CallbackQuery delle InlineKeyboard
            botClient.StartReceiving();
            Thread t = new Thread(Checkmessage);
            t.Start();
            Console.ReadLine();
        }

        private async static void Checkmessage() //funzione di gestione del database 
        {
            while (true)
            {
                string q = "SELECT * FROM main.FILE WHERE Approved = 'N'";
                System.Data.DataTable r = SqLite.ExecuteSelect(q);
                if (r != null && r.Rows.Count > 0)
                {
                    foreach (DataRow dr in r.Rows)
                    {
                        string pathFile = (string)dr["Path"];
                        System.IO.File.Delete(pathFile);

                        string q2 = "UPDATE main.FILE SET Approved = 'C' WHERE Id_message = " + ((int)dr["Id_message"]).ToString();
                        SqLite.ExecuteSelect(q2);
                    }
                }
                Thread.Sleep(1000 * 1);
            }
        }

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs) 
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;
            switch (callbackQuery.Data)
            {
                case "y":
                    {
                        await botClient.AnswerCallbackQueryAsync(callbackQueryId: callbackQuery.Id, text: $"Modification Accepted"); //Mostra un messaggio all'utente
                        botClient.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "<b>MERGED</b>", ParseMode.Html); //modifica il messaggio in modo che non sia più riclickabile
                        var fileName = @"C:\Repos\doc\" + callbackQuery.Message.ReplyToMessage.Document.FileName; 
                        using FileStream fileStream = System.IO.File.OpenWrite(fileName);
                        await botClient.GetInfoAndDownloadFileAsync(callbackQuery.Message.ReplyToMessage.Document.FileId, destination: fileStream); //salva il file 
                        //  await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "File Saved in " + fileName);
                        //  InputOnlineFile inputOnlineFile = new InputOnlineFile(fileStream, e.Message.Document.FileName);
                        //  await botClient.SendDocumentAsync(-1001403617749, inputOnlineFile);
                        //  using (var sendFileStream = File.Open(fileName, FileMode.Open))
                        string q = "SELECT * FROM main.FILE WHERE Id_message = " + callbackQuery.Message.ReplyToMessage.Document.FileId.ToString();
                        System.Data.DataTable r = SqLite.ExecuteSelect(q);
                        string q2 = "UPDATE main.FILE SET Approved = 'Y' WHERE Id_message = " + callbackQuery.Message.ReplyToMessage.Document.FileId.ToString();
                        SqLite.ExecuteSelect(q2); //aggiorna il database con il file
                    }
                    break;
                case "n":
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId: callbackQuery.Id, text: $"Modification Denied"); //Mostra un messaggio all'utente
                    botClient.EditMessageTextAsync(callbackQuery.Message.Chat.Id ,callbackQuery.Message.MessageId, "<b>DENIED</b>", ParseMode.Html); //modifica il messaggio in modo che non sia più riclickabile
                    break;
            }
        }
        private static async void BotClient_OnMessageAsync(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            Console.WriteLine(e.Message.Text);
            Conversation conv = new Conversation();
            dict.TryAdd(e.Message.From.Id, conv); //aggiunge una conversazione al dizionario, questa parte è WIP
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
            try {  //gestisce l'arrivo del messaggio dall'utente
                Message messageFW = await botClient.ForwardMessageAsync(-1001403617749, e.Message.Chat.Id, e.Message.MessageId); //inoltra il file sul gruppo degli admin
                List<InlineKeyboardButton> inlineKeyboardButton = new List<InlineKeyboardButton>() {
                    new InlineKeyboardButton() {Text = "Yes", CallbackData = "y" },
                    new InlineKeyboardButton() {Text = "No", CallbackData = "n" },
                };        
                InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(inlineKeyboardButton);
                Message queryAW = await botClient.SendTextMessageAsync(-1001403617749, "Approvi l'inserimento del documento in %PATH%?", ParseMode.Default, false, false, messageFW.MessageId, inlineKeyboardMarkup, default(CancellationToken)); //aggiunge sotto la InlineKeyboard per la selezione del what to do
                var fileName = @"C:\Repos\doc\" + e.Message.Document.FileName;
                using FileStream fileStream = System.IO.File.OpenWrite(fileName);
                //questa roba è stata spostata nello switch della query in entrata
                //   await botClient.SendTextMessageAsync(e.Message.Chat.Id, e.Message.Document.FileId);
                // await botClient.GetInfoAndDownloadFileAsync(e.Message.Document.FileId, destination: fileStream);
                // await botClient.SendTextMessageAsync(e.Message.Chat.Id, "File Saved in " + fileName);
                //  InputOnlineFile inputOnlineFile = new InputOnlineFile(fileStream, e.Message.Document.FileName);
                //  await botClient.SendDocumentAsync(-1001403617749, inputOnlineFile);
                //  using (var sendFileStream = File.Open(fileName, FileMode.Open)) 
                string q = "INSERT INTO Main.FILE (Path, Id_owner, Data, Approved, Id_cs, Desc, Id_message) VALUES (@p, @io, @d, @a, @ic, @d, @im)";
                Dictionary<string, object> keyValuePairs = new Dictionary<string, object>() {
                    {"@p", fileName },
                    {"@io", e.Message.From.Id },
                    {"@a", "Q" },
                    {"@d", DateTime.Now},
                    {"@ic", 1},
                    {"@d", "Default Desc"},
                    {"@im", e.Message.Document.FileId}
                };
                SqLite.Execute(q, keyValuePairs); //aggiorna il database con il nuovo documento
                fileStream.Close();
                //using FileStream fileStream1 = System.IO.File.OpenRead(fileName);
            }
            catch { }

        }

        private static void BotClient_OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}




//fileStream1.Seek(0, SeekOrigin.Begin);
//await botClient.SendDocumentAsync(-1001403617749, new Telegram.Bot.Types.InputFiles.InputOnlineFile(fileStream1, fileName));
//await botClient.SendTextMessageAsync(e.Message.Chat.Id, "File Sent");

/*List<InlineKeyboardButton> keyboardButtons = new List<InlineKeyboardButton>() { 


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
