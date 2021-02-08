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
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using System.Linq;
using Markdig.Syntax;
using Telegram.Bot.Requests;

namespace Bot
{
    public class Program
    {
        static TelegramBotClient botClient = null;

        public static Dictionary<long, Conversation> dict = new Dictionary<long, Conversation>(); //inizializzazione del dizionario <utente, Conversation>

        private static object lock1 = new object();
        static void Main(string[] args)
        {

            var t = new Thread(() => Main2(args));
            t.Start();
        }
        static void Main2(string[] args)
        {
            botClient = PrivateKey.newKey();
            try
            {
                botClient.OnMessage += BotClient_OnMessageAsync; //gestisce i messaggi in entrata
                botClient.OnCallbackQuery += BotOnCallbackQueryReceived; //gestisce le CallbackQuery delle InlineKeyboard
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            botClient.StartReceiving();
            Console.ReadLine();
        }
        private static void GitHandler(CallbackQueryEventArgs e)
        {
            lock (lock1)
            {
                var callbackQuery = e.CallbackQuery;
                String[] callbackdata = callbackQuery.Data.Split("|");
                long FromId = Int64.Parse(callbackdata[1]);
                try
                {
                    string directory = PrivateKey.root + @"/" + dict[FromId].getcorso() + @"/" + dict[FromId].getGit() + @"/"; // directory of the git repository
                    Console.WriteLine(directory);
                    botClient.SendTextMessageAsync(-1001399914655, "Log: " + directory + System.Environment.NewLine);
                    Console.WriteLine("GetGit: " + dict[FromId].getGit());
                    botClient.SendTextMessageAsync(-1001399914655, "Log: " + "GetGit: " + dict[FromId].getGit() + System.Environment.NewLine);
                    using (PowerShell powershell = PowerShell.Create())
                    {
                        // this changes from the user folder that PowerShell starts up with to your git repository
                        powershell.AddScript("cd " + directory);
                        powershell.AddScript(@"git pull");
                        List<PSObject> results = powershell.Invoke().ToList();
                        for (int i = 0; i < results.Count(); i++)
                        {
                            Console.WriteLine(results[i].ToString());
                            botClient.SendTextMessageAsync(-1001399914655, "Log: " + results[i].ToString() + System.Environment.NewLine);
                        }
                        powershell.Commands.Clear();
                        powershell.AddScript(@"git add . --ignore-errors");
                        results = powershell.Invoke().ToList();
                        for (int i = 0; i < results.Count(); i++)
                        {
                            Console.WriteLine(results[i].ToString());
                            botClient.SendTextMessageAsync(-1001399914655, "Log: " + results[i].ToString() + System.Environment.NewLine);

                        }
                        powershell.Commands.Clear();
                        Console.WriteLine(whatChanged(e));
                        botClient.SendTextMessageAsync(-1001399914655, "Log: " + whatChanged(e) + System.Environment.NewLine);

                        string commit = @"git commit -m 'git commit by bot updated file: " + whatChanged(e) + @"' --author=""polinetwork2@gmail.com""";
                        powershell.AddScript(commit);
                        results = powershell.Invoke().ToList();
                        for (int i = 0; i < results.Count(); i++)
                        {
                            Console.WriteLine(results[i].ToString());
                            botClient.SendTextMessageAsync(-1001399914655, "Log: " + results[i].ToString() + System.Environment.NewLine);
                        }
                        powershell.Commands.Clear();
                        powershell.AddScript(@"git push https://polibot:" + PrivateKey.getPassword() + "@gitlab.com/polinetwork/" + dict[FromId].getGit() + @".git --all");
                        for (int i = 0; i < powershell.Commands.Commands.Count(); i++)
                        {
                            Console.WriteLine(powershell.Commands.Commands[i].ToString());
                            botClient.SendTextMessageAsync(-1001399914655, "Log: " + powershell.Commands.Commands[i].ToString() + System.Environment.NewLine, ParseMode.Default, true);
                        }
                        results = powershell.Invoke().ToList();
                        for (int i = 0; i < results.Count(); i++)
                        {
                            Console.WriteLine(results[i].ToString());
                            botClient.SendTextMessageAsync(-1001399914655, "Log: " + results[i].ToString() + System.Environment.NewLine);
                        }
                        powershell.Commands.Clear();
                        powershell.Stop();
                    }
                   
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    botClient.SendTextMessageAsync(-1001399914655, "Log: " + ex.Message + System.Environment.NewLine);
                }
            }
        }

        private static string whatChanged(CallbackQueryEventArgs e)
        {
            if (e.CallbackQuery.Message.ReplyToMessage.Document != null)
            {
                Console.WriteLine(e.CallbackQuery.Message.ReplyToMessage.Document.FileName);
                botClient.SendTextMessageAsync(-1001399914655, "Log: " + e.CallbackQuery.Message.ReplyToMessage.Document.FileName + System.Environment.NewLine);
                return e.CallbackQuery.Message.ReplyToMessage.Document.FileName;
            }
            else if (e.CallbackQuery.Message.ReplyToMessage.Photo != null)
                return "foto";
            else return "name unknown";
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
            try
            {
                await BotOnCallbackQueryReceived2(sender, callbackQueryEventArgs);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                await botClient.SendTextMessageAsync(-1001455083049, "Exception generated:" + System.Environment.NewLine + exception.Message + System.Environment.NewLine + "Stack trace: " + System.Environment.NewLine + exception.StackTrace);
            }
        }

        private static async Task BotOnCallbackQueryReceived2(object sender, CallbackQueryEventArgs callbackQueryEventArgs) 
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;
            String[] callbackdata = callbackQuery.Data.Split("|");
            long FromId = Int64.Parse(callbackdata[1]);
            if (!userIsAdmin(callbackQuery.From.Id, callbackQueryEventArgs.CallbackQuery.Message.Chat.Id))
            {
                await botClient.AnswerCallbackQueryAsync(callbackQueryId: callbackQuery.Id, text: $"Modification Denied! You need to be admin of this channel"); //Mostra un messaggio all'utente
                return;
            }
            switch (callbackdata[0]) // FORMATO: Y o N | ID PERSONA | ID MESSAGGIO (DEL DOC)
            {
                case "y":
                    {
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId: callbackQuery.Id, text: $"Modification Accepted"); //Mostra un messaggio all'utente
                    var message = botClient.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "<b>MERGED</b> by " + callbackQuery.From.FirstName, ParseMode.Html); //modifica il messaggio in modo che non sia più riclickabile
                    if (callbackQuery.Message.ReplyToMessage.Document.FileSize > 20000000)
                        {
                            await botClient.SendTextMessageAsync(ChannelsForApproval.getChannel(dict[FromId].getcorso()), "Can't upload " + callbackQuery.Message.ReplyToMessage.Document.FileName + ". file size exceeds maximum allowed size. You can upload it manually from GitLab.", ParseMode.Default, false, false); //aggiunge sotto la InlineKeyboard per la selezione del what to do
                        }
                        var fileName = PrivateKey.root + dict[FromId].getcorso() + "/" + dict[FromId].getPercorso() + "/" + callbackQuery.Message.ReplyToMessage.Document.FileName;
                    var fileOnlyName = dict[FromId].getcorso() + "/" + dict[FromId].getPercorso() + "/" + callbackQuery.Message.ReplyToMessage.Document.FileName;
                    try
                    {
                        int endOfPath = fileName.Split(@"/").Last().Split(@"/").Last().Length;
                        //string a = fileName.ToCharArray().Take(fileName.Length - endOfPath).ToString();
                        System.IO.Directory.CreateDirectory(fileName.Substring(0, fileName.Length - endOfPath));
                        using FileStream fileStream = System.IO.File.OpenWrite(fileName);
                        await botClient.GetInfoAndDownloadFileAsync(callbackQuery.Message.ReplyToMessage.Document.FileId, destination: fileStream);
                        fileStream.Close();
                        await botClient.SendTextMessageAsync(FromId, "File Saved in " + fileOnlyName + System.Environment.NewLine, ParseMode.Default, false, false);
                    }
                    catch {
                        
                        await botClient.SendTextMessageAsync(FromId, @"Couldn't save the file. Bot only support files up to 20MBs, although you can open a Pull Request on GitLab to upload it or ask an Admin to do it. " + "Send other files to upload them in the same folder or write anything to go back to the main menu");
                    }
                    //salva il file 
                    //  InputOnlineFile inputOnlineFile = new InputOnlineFile(fileStream, e.Message.Document.FileName);
                    //  await botClient.SendDocumentAsync(-1001403617749, inputOnlineFile);
                    //  using (var sendFileStream = File.Open(fileName, FileMode.Open))
                    // string q = "SELECT * FROM main.FILE WHERE Id_message = " + callbackQuery.Message.ReplyToMessage.Document.FileId.ToString();
                    // System.Data.DataTable r = SqLite.ExecuteSelect(q);
                    // string q2 = "UPDATE main.FILE SET Approved = 'Y' WHERE Id_message = " + callbackQuery.Message.ReplyToMessage.Document.FileId.ToString();
                    // SqLite.ExecuteSelect(q2); //aggiorna il database con il file
                    GitHandler(callbackQueryEventArgs);
                    }
                    break;
                case "n":
                    try
                    {
                        string fileOnlyName = callbackQuery.Message.ReplyToMessage.Document.FileName;
                        await botClient.AnswerCallbackQueryAsync(callbackQueryId: callbackQuery.Id, text: $"Modification Denied");
                        botClient.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "<b>DENIED</b> by " + callbackQuery.From.FirstName, ParseMode.Html); //modifica il messaggio in modo che non sia più riclickabile
                        await botClient.SendTextMessageAsync(FromId, "The file: " + fileOnlyName + " was rejected by an Admin", ParseMode.Default, false, false);
                    }
                    catch
                    {
                        await botClient.SendTextMessageAsync(FromId, @"Couldn't save the file. " + "Send other files to upload them in the same folder or write anything to go back to the main menu");
                    }
                    break;
            }
        }

        private static bool userIsAdmin(int sender, long chatId)
        {
            try
            {
                var admin = botClient.GetChatMemberAsync(chatId, sender).Result;
                return admin.Status == ChatMemberStatus.Administrator || admin.Status == ChatMemberStatus.Creator;
            }
            catch
            {
                return false;
            }
        }


        private static async void BotClient_OnMessageAsync(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            try
            {
                await BotClient_OnMessageAsync2Async(e);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                await botClient.SendTextMessageAsync(-1001455083049, "Exception generated:" + System.Environment.NewLine + exception.Message + System.Environment.NewLine + "Stack trace: " + System.Environment.NewLine + exception.StackTrace);
            }
        }

        private static async System.Threading.Tasks.Task BotClient_OnMessageAsync2Async(MessageEventArgs e)
        {
            if (e.Message.Text == "/start")
            {
                generaStartAsync(e);
            }
            Console.WriteLine(e.Message.Text);
            await botClient.SendTextMessageAsync(-1001399914655, "Log: " + e.Message.Text + System.Environment.NewLine);
            if (!dict.ContainsKey(e.Message.From.Id))
            {
                generaStartAsync(e);
            }
            var stato = dict[e.Message.From.Id].getStato();
          //  await botClient.SendTextMessageAsync(e.Message.Chat.Id, stato?.ToString()); //DEBUG

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
                case stati.AttesaFile:
                    gestisciFileAsync(e);
                    break;
                case stati.newCartella:
                    gestisciNewCartellaAsync(e);
                    break;
            }
        }

        private static async Task gestisciNewCartellaAsync(MessageEventArgs e)
        {
            dict[e.Message.From.Id].scesoDiUnLivello(e.Message.Text);
            generaNewCartella(e);
            dict[e.Message.From.Id].setStato(stati.Cartella);
        }

        private static void generaNewCartella(MessageEventArgs e)
        {
            List<List<KeyboardButton>> replyKeyboard = Keyboards.getKeyboardPercorsi(e.Message.From.Id);
            InviaCartellaAsync(e, replyKeyboard);
        }

        private static async System.Threading.Tasks.Task gestisciFileAsync(MessageEventArgs e)
        {
            //gestisce l'arrivo del messaggio dall'utente
            if (e.Message.Photo != null)
            {
                await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Photos can only be sent without compression", ParseMode.Default, false, false, 0);
                return;
            }
            if (e.Message == null || e.Message.Document == null)
            {
                await botClient.SendTextMessageAsync(e.Message.From.Id, "Going back to the main menu"); //aggiunge sotto la InlineKeyboard per la selezione del what to do
                generaStartOnBackAndNull(e);
                return;
            }
            else
            {
                await botClient.SendTextMessageAsync(e.Message.Chat.Id, "File sent for approval", ParseMode.Default, false, false, e.Message.MessageId);
                Message messageFW = await botClient.ForwardMessageAsync(ChannelsForApproval.getChannel(dict[e.Message.From.Id].getcorso()), e.Message.Chat.Id, e.Message.MessageId); //inoltra il file sul gruppo degli admin
                List<InlineKeyboardButton> inlineKeyboardButton = new List<InlineKeyboardButton>() {
                new InlineKeyboardButton() {Text = "Yes", CallbackData = "y|" + e.Message.From.Id}, // y/n|From.Id
                new InlineKeyboardButton() {Text = "No", CallbackData = "n|" + + e.Message.From.Id},
                };
                InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(inlineKeyboardButton);
                Message queryAW = await botClient.SendTextMessageAsync(ChannelsForApproval.getChannel(dict[e.Message.From.Id].getcorso()), "Approvi l'inserimento del documento in " + dict[e.Message.From.Id].getcorso() + "/" + dict[e.Message.From.Id].getPercorso() + " ?", ParseMode.Default, false, false, messageFW.MessageId, inlineKeyboardMarkup, default(CancellationToken)); //aggiunge sotto la InlineKeyboard per la selezione del what to do
                var fileName = PrivateKey.root + @"/doc/" + e.Message.Document.FileName;
                /*   string q = "INSERT INTO Main.FILE (Path, Id_owner, Data, Approved, Id_cs, Desc, Id_message) VALUES (@p, @io, @d, @a, @ic, @d, @im)";
                Dictionary<string, object> keyValuePairs = new Dictionary<string, object>() {
                    {"@p", fileName },
                    {"@io", e.Message.From.Id },
                    {"@a", "Q" },
                    {"@d", DateTime.Now},
                    {"@ic", 1},
                    {"@d", "Default Desc"},
                    {"@im", e.Message.Document.FileId}
                };
                SqLite.Execute(q, keyValuePairs); //aggiorna il database con il nuovo documento */
                //using FileStream fileStream1 = System.IO.File.OpenRead(fileName);
            }
        }


        private static async Task generaStartAsync(MessageEventArgs e)
        {
            if (!dict.ContainsKey(e.Message.From.Id))
            {
                Conversation conv = new Conversation();
                dict.TryAdd(e.Message.From.Id, conv);
            }
            else
            {
                dict[e.Message.From.Id].setStato(stati.start);
                dict[e.Message.From.Id].resetPercorso();
            }
        }
        private static void generaStartOnCallback(CallbackQueryEventArgs e)
        {
            if (!dict.ContainsKey(e.CallbackQuery.Message.From.Id))
            {
                Conversation conv = new Conversation();
                dict.TryAdd(e.CallbackQuery.Message.From.Id, conv); //aggiunge una conversazione al dizionario, questa parte è WIP
            }
            else
            {
                dict[e.CallbackQuery.Message.From.Id].setStato(stati.start);
            }
        }

        private static async System.Threading.Tasks.Task gestisciCartellaAsync(MessageEventArgs e)
        {
            if(e.Message.Text == null)
            {
                generaStartOnBackAndNull(e);
                return;
            }
            if (e.Message.Text.StartsWith("🆗"))
            {
                dict[e.Message.From.Id].setStato(stati.AttesaFile);
                await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Send your file (can be multiple). Write anything to go back to the main menu", ParseMode.Default, false, false, 0);
            }
            else if (e.Message.Text.StartsWith("🔙"))
            {
                generaStartAsync(e);
                BotClient_OnMessageAsync2Async(e);
            }
            else if (e.Message.Text.StartsWith("🆕"))
            {
                generaCartellaAsync(e);
            }
            else
            {
                if (!verificaSottoCartelle(e))
                {
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Folder not recognized. Use the button to create a new one.", ParseMode.Default, false, false, 0);
                }
                else
                {
                    dict[e.Message.From.Id].scesoDiUnLivello(e.Message.Text);
                    List<List<KeyboardButton>> replyKeyboard = Keyboards.getKeyboardPercorsi(e.Message.From.Id);
                    InviaCartellaAsync(e, replyKeyboard);
                }
            }
        }

        private static bool verificaSottoCartelle(MessageEventArgs e)
        {
            string[] sottoCartelle = Keyboards.getDir(e.Message.From.Id);
            foreach (string a in sottoCartelle)
            {
                if(a.Split(@"\").Last().Split(@"/").Last().Equals(e.Message.Text.Split(@"\").Last().Split(@"/").Last())) return true;
            }
            return false;
        }

        private static async Task generaCartellaAsync(MessageEventArgs e)
        {
            dict[e.Message.From.Id].setStato(stati.newCartella);
            await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Write the name of the new folder", ParseMode.Default, false, false, e.Message.MessageId);
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
            if(e.Message.Text == null)
            {
                generaStartOnBackAndNull(e);
                return;
            }
            if (e.Message.Text.StartsWith("🔙"))
            {
                generaStartOnBackAndNull(e);
                return;
            }
            if (corsienum == null)
            {
                await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Unknown path. Going back to beginning. Use the Keyboard to navigate the folders.", ParseMode.Default, false, false, e.Message.MessageId);
                generaStartOnBackAndNull(e);
                return;
            }
            else
            {
                dict[e.Message.From.Id].setCorso(corsienum);
                List<List<KeyboardButton>> replyKeyboard = Keyboards.getKeyboardPercorsi(e.Message.From.Id);
                if (replyKeyboard == null)
                {
                    generaStartOnBackAndNull(e);
                    return;
                }
                else
                {
                    dict[e.Message.From.Id].setStato(stati.Cartella);
                    InviaCartellaAsync(e, replyKeyboard);
                }
            }
        }

        private static async Task generaStartOnBackAndNull(MessageEventArgs e)
        {
            generaStartAsync(e);
            BotClient_OnMessageAsync2Async(e);
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
                if(e.Message.Text == "3I")
                {
                    e.Message.Text = "TREI";
                }
                ScuoleEnums? scuoleEnums = (ScuoleEnums?)reconEnum(e.Message.Text, typeof(ScuoleEnums));
                if (scuoleEnums == null)
                {
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Unknown path. Going back to beginning. Use the Keyboard to navigate the folders.", ParseMode.Default, false, false, e.Message.MessageId);
                    generaStartOnBackAndNull(e);
                    return;
                }
                else
                {
                    dict[e.Message.From.Id].setStato(stati.Corso);
                    dict[e.Message.From.Id].setScuola(scuoleEnums);
                    List<List<KeyboardButton>> replyKeyboard = Keyboards.getKeyboardCorsi(scuoleEnums);
                    IReplyMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(replyKeyboard, true, true);
                    string text = scuoleEnums.ToString();
                    if (text == "TREI")
                    {
                        text = "3I";
                    }
                    await botClient.SendTextMessageAsync(e.Message.From.Id, "Selezionata " + 
                        text, ParseMode.Default, false, false, 0, replyKeyboardMarkup);
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