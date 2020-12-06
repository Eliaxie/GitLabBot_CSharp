using Bot;
using Bot.Enums;
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
    public class Program
    {
        static TelegramBotClient botClient = null;

        public static Dictionary<long, Conversation> dict = new Dictionary<long, Conversation>(); //inizializzazione del dizionario <utente, Conversation>

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
            BotClient_OnMessageAsync2Async(e);
        }

        private static async System.Threading.Tasks.Task BotClient_OnMessageAsync2Async(MessageEventArgs e)
        {
            ;
            if (e.Message.Text == "/start")
            {
                generaStart(e);
            }
            Console.WriteLine(e.Message.Text);
            if (!dict.ContainsKey(e.Message.From.Id))
            {
                generaStart(e);
            }
            var stato = dict[e.Message.From.Id].getStato();
            await botClient.SendTextMessageAsync(e.Message.Chat.Id, stato?.ToString()); //DEBUG

            switch (stato)
            {
                case stati.start:
                    gestisciStartAsync(e);
                    break;
                case stati.Scuola:
                    gestisciScuolaAsync(e);
                    break;
                case stati.Corso:
                    gestisciCorsoAsync(e);
                    break;
                case stati.Cartella:
                    gestisciCartellaAsync(e);
                    break;
            }
        }

        private static void generaStart(MessageEventArgs e)
        {
            if (!dict.ContainsKey(e.Message.From.Id))
            {
                Conversation conv = new Conversation();
                dict.TryAdd(e.Message.From.Id, conv); //aggiunge una conversazione al dizionario, questa parte è WIP
            }
            else
            {
                dict[e.Message.From.Id].setStato(stati.start);
            }
        }

        private static async System.Threading.Tasks.Task gestisciCartellaAsync(MessageEventArgs e)
        {
            if (e.Message.Text.StartsWith("🆗"))
            {
                await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Inserisci il file", ParseMode.Default, false, false, 0);
            }
            else if (e.Message.Text.StartsWith("🔙"))
            {
                generaStart(e);
                BotClient_OnMessageAsync2Async(e);
            }
            else
            {
                //nome di una cartella 
                dict[e.Message.From.Id].scesoDiUnLivello(e.Message.Text);
                List<List<KeyboardButton>> replyKeyboard = Keyboards.getKeyboardPercorsi(e.Message.From.Id);
                InviaCartellaAsync(e, replyKeyboard);
            }
        }

        private static async System.Threading.Tasks.Task gestisciStartAsync(MessageEventArgs e)
        {
            dict[e.Message.From.Id].setStato(stati.Scuola);
  
            List<List<KeyboardButton>> replyKeyboard = Keyboards.getKeyboardScuole();
            IReplyMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(replyKeyboard, true, true);
            await botClient.SendTextMessageAsync(e.Message.From.Id, "Scegli una scuola" ,
                ParseMode.Default, false, false, 0, replyKeyboardMarkup);
        }

        private static async System.Threading.Tasks.Task gestisciCorsoAsync(MessageEventArgs e)
        {
            CorsiEnum? corsienum = (CorsiEnum?)reconEnum(e.Message.Text, typeof(CorsiEnum));
            if (corsienum == null)
            {
                //todo: dire all'utente che ha sbagliato
                return;
            }
            else
            {
                dict[e.Message.From.Id].setCorso(corsienum);
                List<List<KeyboardButton>> replyKeyboard = Keyboards.getKeyboardPercorsi(e.Message.From.Id);
                if (replyKeyboard == null)
                {
                    //dire all'utente che c'è stato un errore
                }
                else
                {
                    dict[e.Message.From.Id].setStato(stati.Cartella);


                    InviaCartellaAsync(e, replyKeyboard);
                }
            }
        }

        private static async System.Threading.Tasks.Task InviaCartellaAsync(MessageEventArgs e, List<List<KeyboardButton>> replyKeyboard)
        {
            if (replyKeyboard == null)
                return;

            IReplyMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(replyKeyboard, true, true);
            await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Seleziona un percorso", ParseMode.Default, false, false, 0, replyKeyboardMarkup);
        }

        private static async System.Threading.Tasks.Task gestisciScuolaAsync(MessageEventArgs e)
        {
            try
            {
                ScuoleEnums? scuoleEnums = (ScuoleEnums?)reconEnum(e.Message.Text, typeof(ScuoleEnums));
                if (scuoleEnums == null)
                {
                    //todo: dire all'utente che ha sbagliato a cliccare
                    return;
                }
                else
                {
                    dict[e.Message.From.Id].setStato(stati.Corso);
                    dict[e.Message.From.Id].setScuola(scuoleEnums);
                    List<List<KeyboardButton>> replyKeyboard = Keyboards.getKeyboardCorsi(scuoleEnums);
                    IReplyMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(replyKeyboard, true, true);
                    await botClient.SendTextMessageAsync(e.Message.From.Id, "Selezionata " + 
                        scuoleEnums.ToString(), ParseMode.Default, false, false, 0, replyKeyboardMarkup);
                }
            }
            catch
            {
                /*
                  */
            }
        }

        private static object reconEnum(string text, Type type)
        { 

            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            try
            {
                object r;
                Enum.TryParse(type, text, out r);
                return r;
            }
            catch
            {
                ;
            }

            return null;

        }
    }
}




//questa roba è stata spostata nello switch della query in entrata
//   await botClient.SendTextMessageAsync(e.Message.Chat.Id, e.Message.Document.FileId);
// await botClient.GetInfoAndDownloadFileAsync(e.Message.Document.FileId, destination: fileStream);
// await botClient.SendTextMessageAsync(e.Message.Chat.Id, "File Saved in " + fileName);
//  InputOnlineFile inputOnlineFile = new InputOnlineFile(fileStream, e.Message.Document.FileName);
//  await botClient.SendDocumentAsync(-1001403617749, inputOnlineFile);
//  using (var sendFileStream = File.Open(fileName, FileMode.Open)) 

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

/*
                    List<string> cartelle = Directory.EnumerateDirectories("percorso");

                    foreach (var cartella in cartelle)
                    {
                        replyKeyboardMarkup.Add(new KeyboardButton(cartella))
;                    }
                    */