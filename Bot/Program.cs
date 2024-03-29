﻿using Bot;
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
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Markdig.Syntax;
using Microsoft.VisualBasic;
using Telegram.Bot.Requests;

namespace Bot
{
    [Serializable]
    public class Program
    {
        static TelegramBotClient botClient = null;

        public static Dictionary<long, Conversation>
            dict = new Dictionary<long, Conversation>(); //inizializzazione del dizionario <utente, Conversation>

        public static Dictionary<string, string>
            dictPaths =
                new Dictionary<string, string>(); //inizializzazione del dizionario <ID univoco file, stringa documento>

        public static void Serialize<Object>(Object dictionary, Stream stream)
        {
            try // try to serialize the collection to a file
            {
                using (stream)
                {
                    // create BinaryFormatter
                    BinaryFormatter bin = new BinaryFormatter();
                    // serialize the collection (EmployeeList1) to file (stream)
                    bin.Serialize(stream, dictionary);
                }
            }
            catch (IOException)
            {
                Console.WriteLine("dict non esistente? ser");
            }
        }

        public static Object Deserialize<Object>(Stream stream) where Object : new()
        {
            Object ret = CreateInstance<Object>();
            try
            {
                using (stream)
                {
                    // create BinaryFormatter
                    BinaryFormatter bin = new BinaryFormatter();
                    // deserialize the collection (Employee) from file (stream)
                    ret = (Object) bin.Deserialize(stream);
                }
            }
            catch (IOException)
            {
                Console.WriteLine("dict non esistente? deser");
            }

            return ret;
        }

        // function to create instance of T
        public static Object CreateInstance<Object>() where Object : new()
        {
            return (Object) Activator.CreateInstance(typeof(Object));
        }

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
                Dictionary<string, string> deserializeObject =
                    Deserialize<Dictionary<string, string>>(System.IO.File.Open("/home/ubuntu/bot/dictPath.bin",
                        FileMode.Open));
                dictPaths = deserializeObject;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

            try
            {
                botClient.OnMessage += BotClient_OnMessageAsync; //gestisce i messaggi in entrata
                botClient.OnCallbackQuery +=
                    BotOnCallbackQueryReceived; //gestisce le CallbackQuery delle InlineKeyboard
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
                string directory;
                if (!dictPaths.TryGetValue(callbackdata[2], out directory))
                    throw new Exception("Errore nel dizionario dei Path in GITHANDLER!");
                string[] a = directory.Split("/");
                directory = "";
                for (int i = 0; i < a.Length - 1; i++)
                {
                    directory = directory + a[i] + "/";
                }

                string[] b = directory.Split("'");
                directory = "";
                for (int i = 0; i < b.Length; i++)
                {
                    directory = directory + b[i] + "\'\'";
                }

                directory = directory.Substring(0, directory.Length - 2);
                try
                {
                    // string directory = PrivateKey.root + @"/" + dict[FromId].getcorso() + @"/" + dict[FromId].getGit() + @"/"; // directory of the git repository
                    Console.WriteLine(directory);
                    botClient.SendTextMessageAsync(-1001399914655, "Log: " + directory + System.Environment.NewLine);
                    Console.WriteLine("GetGit: " + getGit(directory));
                    botClient.SendTextMessageAsync(-1001399914655,
                        "Log: " + "GetGit: " + getGit(directory) + System.Environment.NewLine);
                    using (PowerShell powershell = PowerShell.Create())
                    {
                        // this changes from the user folder that PowerShell starts up with to your git repository
                        string dirCD = "/" + getRoot(directory) + "/" + getCorso(directory) + "/" + getGit(directory) +
                                       "/";
                        powershell.AddScript("cd " + dirCD);
                        botClient.SendTextMessageAsync(-1001399914655,
                            "Log: CD result: " + dirCD + System.Environment.NewLine);
                        powershell.AddScript(@"git pull");
                        List<PSObject> results = powershell.Invoke().ToList();
                        for (int i = 0; i < results.Count(); i++)
                        {
                            Console.WriteLine(results[i].ToString());
                            botClient.SendTextMessageAsync(-1001399914655,
                                "Log: Git Pull result: " + results[i].ToString() + System.Environment.NewLine);
                        }

                        powershell.Commands.Clear();
                        powershell.AddScript(@"git add . --ignore-errors");
                        results = powershell.Invoke().ToList();
                        for (int i = 0; i < results.Count(); i++)
                        {
                            Console.WriteLine(results[i].ToString());
                            botClient.SendTextMessageAsync(-1001399914655,
                                "Log: Git Add result: " + results[i].ToString() + System.Environment.NewLine);
                        }

                        powershell.Commands.Clear();
                        Console.WriteLine(whatChanged(e));
                        botClient.SendTextMessageAsync(-1001399914655,
                            "Log: WhatChanged: " + whatChanged(e) + System.Environment.NewLine);
                        botClient.SendTextMessageAsync(-1001399914655,
                            "Log: Commit: " + @"git commit -m 'git commit by bot updated file: " + whatChanged(e) +
                            @"' --author=""PoliNetworkBot <polinetwork2@gmail.com>""");
                        string commit = @"git commit -m 'git commit by bot updated file: " + whatChanged(e) +
                                        @"' --author=""PoliBot <polinetwork2@gmail.com>""";
                        powershell.AddScript(commit);
                        results = powershell.Invoke().ToList();
                        for (int i = 0; i < results.Count(); i++)
                        {
                            Console.WriteLine(results[i].ToString());
                            botClient.SendTextMessageAsync(-1001399914655,
                                "Log: Result commit: " + results[i].ToString() + System.Environment.NewLine);
                        }

                        powershell.Commands.Clear();
                        powershell.AddScript(@"git push https://polibot:" + PrivateKey.getPassword() +
                                             "@gitlab.com/polinetwork/" + getGit(directory) + @".git --all");
                        for (int i = 0; i < powershell.Commands.Commands.Count(); i++)
                        {
                            Console.WriteLine(powershell.Commands.Commands[i].ToString());
                            botClient.SendTextMessageAsync(-1001399914655,
                                "Log: push commands: " + powershell.Commands.Commands[i].ToString() +
                                System.Environment.NewLine, ParseMode.Default, true);
                        }

                        results = powershell.Invoke().ToList();
                        for (int i = 0; i < results.Count(); i++)
                        {
                            Console.WriteLine(results[i].ToString());
                            botClient.SendTextMessageAsync(-1001399914655,
                                "Log: Result push: " + results[i].ToString() + System.Environment.NewLine);
                        }

                        powershell.Commands.Clear();
                        powershell.Stop();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    botClient.SendTextMessageAsync(-1001399914655,
                        "Log: Exception! " + System.Environment.NewLine + ex.Message + System.Environment.NewLine);
                }
            }
        }

        private static object getCorso(string directory)
        {
            return directory.Split("/").GetValue(2);
        }

        private static object getRoot(string directory)
        {
            return directory.Split("/").GetValue(1);
        }

        private static object getGit(string directory)
        {
            return directory.Split("/").GetValue(3);
        }

        private static string whatChanged(CallbackQueryEventArgs e)
        {
            if (e.CallbackQuery.Message.ReplyToMessage.Document != null)
            {
                Console.WriteLine(e.CallbackQuery.Message.ReplyToMessage.Document.FileName);
                botClient.SendTextMessageAsync(-1001399914655,
                    "Log: " + e.CallbackQuery.Message.ReplyToMessage.Document.FileName + System.Environment.NewLine);
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
                        string pathFile = (string) dr["Path"];
                        System.IO.File.Delete(pathFile);

                        string q2 = "UPDATE main.FILE SET Approved = 'C' WHERE Id_message = " +
                                    ((int) dr["Id_message"]).ToString();
                        SqLite.ExecuteSelect(q2);
                    }
                }

                Thread.Sleep(1000 * 1);
            }
        }

        private static async void BotOnCallbackQueryReceived(object sender,
            CallbackQueryEventArgs callbackQueryEventArgs)
        {
            try
            {
                await BotOnCallbackQueryReceived2(sender, callbackQueryEventArgs);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                await botClient.SendTextMessageAsync(-1001455083049,
                    "Exception generated:" + System.Environment.NewLine + exception.Message +
                    System.Environment.NewLine + "Stack trace: " + System.Environment.NewLine + exception.StackTrace);
            }
        }

        private static async Task BotOnCallbackQueryReceived2(object sender,
            CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;
            String[] callbackdata = callbackQuery.Data.Split("|");
            long FromId = Int64.Parse(callbackdata[1]);
            string fileNameWithPath;
            if (!dictPaths.TryGetValue(callbackdata[2], out fileNameWithPath))
                throw new Exception("Errore nel dizionario dei Path!");
            if (!userIsAdmin(callbackQuery.From.Id, callbackQueryEventArgs.CallbackQuery.Message.Chat.Id))
            {
                await botClient.AnswerCallbackQueryAsync(callbackQueryId: callbackQuery.Id,
                    text: $"Modification Denied! You need to be admin of this channel"); //Mostra un messaggio all'utente
                return;
            }

            switch (callbackdata[0]) // FORMATO: Y o N | ID PERSONA | ID MESSAGGIO (DEL DOC) | fileUniqueID
            {
                case "y":
                {
                    string nameApprover = callbackQuery.From.FirstName;
                    if (nameApprover != null && nameApprover.Length > 1)
                    {
                        nameApprover = nameApprover[0].ToString();
                    }

                    await botClient.AnswerCallbackQueryAsync(callbackQueryId: callbackQuery.Id,
                        text: $"Modification Accepted"); //Mostra un messaggio all'utente
                    var message = botClient.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
                        callbackQuery.Message.MessageId, "<b>MERGED</b> by " + nameApprover,
                        ParseMode.Html); //modifica il messaggio in modo che non sia più riclickabile
                    if (callbackQuery.Message.ReplyToMessage.Document.FileSize > 20000000)
                    {
                        await botClient.SendTextMessageAsync(ChannelsForApproval.getChannel(dict[FromId].getcorso()),
                            "Can't upload " + callbackQuery.Message.ReplyToMessage.Document.FileName +
                            ". file size exceeds maximum allowed size. You can upload it manually from GitLab.",
                            ParseMode.Default, false,
                            false); //aggiunge sotto la InlineKeyboard per la selezione del what to do
                    }

                    var fileName = fileNameWithPath;
                    var fileOnlyName = fileName.Substring(PrivateKey.root.Length);
                    try
                    {
                        int endOfPath = fileName.Split(@"/").Last().Split(@"/").Last().Length;
                        //string a = fileName.ToCharArray().Take(fileName.Length - endOfPath).ToString();
                        System.IO.Directory.CreateDirectory(fileName.Substring(0, fileName.Length - endOfPath));
                        using FileStream fileStream = System.IO.File.OpenWrite(fileName);
                        await botClient.GetInfoAndDownloadFileAsync(
                            callbackQuery.Message.ReplyToMessage.Document.FileId, destination: fileStream);
                        fileStream.Close();
                        await botClient.SendTextMessageAsync(FromId,
                            "File Saved in " + fileOnlyName + System.Environment.NewLine, ParseMode.Default, false,
                            false);
                    }
                    catch
                    {
                        await botClient.SendTextMessageAsync(FromId,
                            @"Couldn't save the file. Bot only support files up to 20MBs, although you can open a Pull Request on GitLab to upload it or ask an Admin to do it. " +
                            "Send other files to upload them in the same folder or write anything to go back to the main menu");
                    }

                    GitHandler(callbackQueryEventArgs);
                }
                    break;
                case "n":
                    try
                    {
                        string fileOnlyName = callbackQuery.Message.ReplyToMessage.Document.FileName;
                        await botClient.AnswerCallbackQueryAsync(callbackQueryId: callbackQuery.Id,
                            text: $"Modification Denied");
                        botClient.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId,
                            "<b>DENIED</b> by " + callbackQuery.From.FirstName,
                            ParseMode.Html); //modifica il messaggio in modo che non sia più riclickabile
                        await botClient.SendTextMessageAsync(FromId,
                            "The file: " + fileOnlyName + " was rejected by an Admin", ParseMode.Default, false, false);
                    }
                    catch
                    {
                        await botClient.SendTextMessageAsync(FromId,
                            @"Couldn't save the file. " +
                            "Send other files to upload them in the same folder or write anything to go back to the main menu");
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
                await botClient.SendTextMessageAsync(-1001455083049,
                    "Exception generated:" + System.Environment.NewLine + exception.Message +
                    System.Environment.NewLine + "Stack trace: " + System.Environment.NewLine + exception.StackTrace);
            }
        }

        private static async System.Threading.Tasks.Task BotClient_OnMessageAsync2Async(MessageEventArgs e)
        {
            if (e.Message.Text == "/start")
            {
                await generaStartAsync(e);
            }

            Console.WriteLine(e.Message.Text);
            //await botClient.SendTextMessageAsync(-1001399914655, "Log: " + e.Message.Text + System.Environment.NewLine);
            if (!dict.ContainsKey(e.Message.From.Id))
            {
                await generaStartAsync(e);
            }

            var stato = dict[e.Message.From.Id].getStato();
            //  await botClient.SendTextMessageAsync(e.Message.Chat.Id, stato?.ToString()); //DEBUG

            switch (stato)
            {
                case stati.start:
                    await gestisciStartAsync(e);
                    break;
                case stati.Scuola:
                    await gestisciScuolaAsync(e);
                    break;
                case stati.Corso:
                    await gestisciCorsoAsync(e);
                    break;
                case stati.Cartella:
                    await gestisciCartellaAsync(e);
                    break;
                case stati.AttesaFile:
                    await gestisciFileAsync(e);
                    break;
                case stati.newCartella:
                    await gestisciNewCartellaAsync(e);
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
                await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Photos can only be sent without compression",
                    ParseMode.Default, false, false, 0);
                return;
            }

            if (e.Message == null || e.Message.Document == null)
            {
                await botClient.SendTextMessageAsync(e.Message.From.Id,
                    "Going back to the main menu"); //aggiunge sotto la InlineKeyboard per la selezione del what to do
                generaStartOnBackAndNull(e);
                return;
            }
            else
            {
                var file = PrivateKey.root + dict[e.Message.From.Id].getcorso().ToLower() + "/" +
                           dict[e.Message.From.Id].getPercorso() + "/" + e.Message.Document.FileName;
                string FileUniqueAndGit = e.Message.Document.FileUniqueId + getGit(file);
                Boolean fileAlreadyPresent = false;
                string oldPath = null;
                if (!dictPaths.TryAdd(FileUniqueAndGit, file))
                {
                    //Verifica anti-SPAM, da attivare se servisse
                    if (dictPaths.TryGetValue(FileUniqueAndGit, out oldPath))
                    {
                        fileAlreadyPresent = true;
                    }
                    else
                    {
                        throw new Exception("Fatal error while handling path dictionary");
                    }
                }

                ;
                try
                {
                    Serialize(dictPaths, System.IO.File.Open("/home/ubuntu/bot/dictPath.bin", FileMode.Create));
                }
                catch
                {
                    await botClient.SendTextMessageAsync(-1001399914655,
                        "Errore nel salvataggio su file del dizionario! " + e.Message.Text +
                        System.Environment.NewLine);
                }

                List<InlineKeyboardButton> inlineKeyboardButton = new List<InlineKeyboardButton>()
                {
                    new InlineKeyboardButton()
                    {
                        Text = "Yes", CallbackData = "y|" + e.Message.From.Id + "|" + FileUniqueAndGit
                    }, // y/n|From.Id|fileUniqueID
                    new InlineKeyboardButton()
                        {Text = "No", CallbackData = "n|" + +e.Message.From.Id + "|" + FileUniqueAndGit},
                };
                InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(inlineKeyboardButton);
                if ((!fileAlreadyPresent && oldPath == null) || (fileAlreadyPresent && oldPath != null))
                {
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "File sent for approval", ParseMode.Default,
                        false, false, e.Message.MessageId);
                    Message messageFW = await botClient.ForwardMessageAsync(
                        ChannelsForApproval.getChannel(dict[e.Message.From.Id].getcorso()), e.Message.Chat.Id,
                        e.Message.MessageId); //inoltra il file sul gruppo degli admin
                    Message queryAW = await botClient.SendTextMessageAsync(
                        ChannelsForApproval.getChannel(dict[e.Message.From.Id].getcorso()),
                        "Approvi l'inserimento del documento in " + dict[e.Message.From.Id].getcorso() + "/" +
                        dict[e.Message.From.Id].getPercorso() + " ?", ParseMode.Default, false, false,
                        messageFW.MessageId, inlineKeyboardMarkup,
                        default(CancellationToken)); //aggiunge sotto la InlineKeyboard per la selezione del what to do
                }
                /*
                else if(fileAlreadyPresent && oldPath != null)
                {
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "The file is already present in " + oldPath + " or it has been already refused. If you think this is a mistake, contact your head admin from this page https://polinetwork.github.io/it/about_us/index.html", ParseMode.Default, true, false, e.Message.MessageId);
                }
                */
                else
                {
                    throw new Exception(
                        "Fatal error while handling path dictionary -> fileAlreadyPresent && oldPath != null");
                }
                
                Thread.Sleep(200);

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
                dict.TryAdd(e.CallbackQuery.Message.From.Id,
                    conv); //aggiunge una conversazione al dizionario, questa parte è WIP
            }
            else
            {
                dict[e.CallbackQuery.Message.From.Id].setStato(stati.start);
            }
        }

        private static async System.Threading.Tasks.Task gestisciCartellaAsync(MessageEventArgs e)
        {
            if (e.Message.Text == null)
            {
                generaStartOnBackAndNull(e);
                return;
            }

            if (e.Message.Text.StartsWith("🆗"))
            {
                dict[e.Message.From.Id].setStato(stati.AttesaFile);
                await botClient.SendTextMessageAsync(e.Message.Chat.Id,
                    "Send your file (can be multiple). Write anything to go back to the main menu", ParseMode.Default,
                    false, false, 0);
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
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id,
                        "Folder not recognized. Use the button to create a new one.", ParseMode.Default, false, false,
                        0);
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
                if (a.Split(@"/").Last().Equals(e.Message.Text.Split(@"/").Last())) return true;
            }

            return false;
        }

        private static async Task generaCartellaAsync(MessageEventArgs e)
        {
            dict[e.Message.From.Id].setStato(stati.newCartella);
            await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Write the name of the new folder",
                ParseMode.Default, false, false, e.Message.MessageId);
        }

        private static async System.Threading.Tasks.Task gestisciStartAsync(MessageEventArgs e)
        {
            dict[e.Message.From.Id].setStato(stati.Scuola);
            List<List<KeyboardButton>> replyKeyboard = Keyboards.getKeyboardScuole();
            IReplyMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(replyKeyboard, true, true);
            await botClient.SendTextMessageAsync(e.Message.From.Id, "Scegli una scuola",
                ParseMode.Default, false, false, 0, replyKeyboardMarkup);
        }

        private static async System.Threading.Tasks.Task gestisciCorsoAsync(MessageEventArgs e)
        {
            CorsiEnum? corsienum = (CorsiEnum?) reconEnum(e.Message.Text, typeof(CorsiEnum));
            if (e.Message.Text == null)
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
                await botClient.SendTextMessageAsync(e.Message.Chat.Id,
                    "Unknown path. Going back to beginning. Use the Keyboard to navigate the folders.",
                    ParseMode.Default, false, false, e.Message.MessageId);
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

        private static async System.Threading.Tasks.Task InviaCartellaAsync(MessageEventArgs e,
            List<List<KeyboardButton>> replyKeyboard)
        {
            if (replyKeyboard == null)
                return;

            IReplyMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(replyKeyboard, true, true);
            await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Seleziona un percorso", ParseMode.Default, false,
                false, 0, replyKeyboardMarkup);
        }

        private static async System.Threading.Tasks.Task gestisciScuolaAsync(MessageEventArgs e)
        {
            try
            {
                if (e.Message.Text == "3I")
                {
                    e.Message.Text = "TREI";
                }

                ScuoleEnums? scuoleEnums = (ScuoleEnums?) reconEnum(e.Message.Text, typeof(ScuoleEnums));
                if (scuoleEnums == null)
                {
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id,
                        "Unknown path. Going back to beginning. Use the Keyboard to navigate the folders.",
                        ParseMode.Default, false, false, e.Message.MessageId);
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
                                                                            text, ParseMode.Default, false, false, 0,
                        replyKeyboardMarkup);
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
