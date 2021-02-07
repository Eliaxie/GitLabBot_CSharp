using Bot.Enums;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot
{
    static public class Keyboards
    {
        static public List<List<KeyboardButton>> getKeyboard(string[] keyboardList)
        { 
            int i = 0;

            if (keyboardList == null)
            {
                return null;
            }
            List<string> keyboardToList = keyboardList.ToList();

            List<List<string>> keyboadToArray = KeyboardMarkup.ArrayToMatrixString(keyboardToList);

     
            if (keyboadToArray == null || keyboadToArray.Count == 0)
                return null;

            List<List<KeyboardButton>> replyKeyboard = new List<List<KeyboardButton>>();

            foreach (var l2 in keyboadToArray)
            {
                List<KeyboardButton> x2 = new List<KeyboardButton>();
                foreach (string l3 in l2)
                {   
                    string[] path = l3.Split(@"/");
                    var len = path.Length;
                    x2.Add(new KeyboardButton(path[len-1]));
                }
                replyKeyboard.Add(x2);
            }
            return replyKeyboard;
        }

        internal static List<List<KeyboardButton>> getKeyboardCorsi(ScuoleEnums? scuoleEnums)
        {
            List<List<KeyboardButton>> r = new List<List<KeyboardButton>>();
            switch (scuoleEnums)
            {
                case ScuoleEnums.TREI:
                    foreach (var v in Enum.GetValues(typeof(CorsiEnumTrei)))
                    {
                        r.Add(new List<KeyboardButton>() { new KeyboardButton() { Text = v.ToString() } });
                    }
                    break;
                case ScuoleEnums.AIUC:
                    foreach (var v in Enum.GetValues(typeof(CorsiEnumAIUC)))
                    {
                        r.Add(new List<KeyboardButton>() { new KeyboardButton() { Text = v.ToString() } });
                    }
                    break;
                case ScuoleEnums.CAT:
                    foreach (var v in Enum.GetValues(typeof(CorsiEnumCAT)))
                    {
                        r.Add(new List<KeyboardButton>() { new KeyboardButton() { Text = v.ToString() } });
                    }
                    break;
                case ScuoleEnums.Design:
                    foreach (var v in Enum.GetValues(typeof(CorsiEnumDesign)))
                    {
                        r.Add(new List<KeyboardButton>() { new KeyboardButton() { Text = v.ToString() } });
                    }
                    break;
            }
            r.Add(new List<KeyboardButton>() { new KeyboardButton() { Text = "🔙 back" } });
            return r;
        }

        internal static String[] getDir(int id)
        {
            string corso = Program.dict[id].getcorso();
            if (string.IsNullOrEmpty(corso))
                return null;
            corso = corso.ToLower();
            string root = PrivateKey.root + corso;
            string percorso = Program.dict[id].getPercorso();
            if (!string.IsNullOrEmpty(percorso))
            {
                root += @"/" + percorso;
            }
            string[] subdirectoryEntries = null;
            if (Program.dict[id].getStato() != stati.newCartella)
            {
                subdirectoryEntries = Directory.GetDirectories(root);
            }
            if (subdirectoryEntries != null)
            {
                subdirectoryEntries = removeGit(subdirectoryEntries);
            }
            return subdirectoryEntries;
        }
        internal static List<List<KeyboardButton>> getKeyboardPercorsi(int id)
        {
            string[] subdirectoryEntries = getDir(id);
            string percorso = Program.dict[id].getPercorso();
            List<List<KeyboardButton>> k  =  Keyboards.getKeyboard(subdirectoryEntries);
            if (k == null) { k = new List<List<KeyboardButton>>(); } 
            if (percorso == null)
            {
                k.Insert(0, new List<KeyboardButton>() {
                new KeyboardButton(){  Text = "🔙 back"}
                });
                return k;
            }
            k.Insert(0, new List<KeyboardButton>() { 
                new KeyboardButton(){  Text = "🔙 back"},
                new KeyboardButton(){  Text = "🆗 Cartella Corrente"},
                new KeyboardButton(){  Text = "🆕 New Folder"}
            });
            return k;
        }

        private static string[] removeGit(string[] subdirectoryEntries)
        {
            List<String> listadir = subdirectoryEntries.ToList();
            for (int i = 0; i < listadir.Count(); i++)
            {
                if (listadir[i].Contains(".git"))
                {
                    listadir.Remove(listadir[i]);
                    i--;
                }
            }
            return listadir.ToArray();
        }

        internal static List<List<KeyboardButton>> getKeyboardScuole()
        {
            List<List<KeyboardButton>> r = new List<List<KeyboardButton>>();
            foreach (var v in Enum.GetValues(typeof(ScuoleEnums)))
            {
                if (v.ToString() == "TREI")
                {
                    r.Add(new List<KeyboardButton>() { new KeyboardButton() { Text = "3I" } });
                }
                else
                {
                    r.Add(new List<KeyboardButton>() { new KeyboardButton() { Text = v.ToString() } });
                }
            }
            return r;
        }
        /*
replyKeyboard.Keyboard = new KeyboardButton[][]
{
new KeyboardButton[]
{
new KeyboardButton("MATNANO"),
},
new KeyboardButton[]
{
new KeyboardButton("--")
},
new KeyboardButton[]
{
new KeyboardButton("--")
},
new KeyboardButton[]
{
new KeyboardButton("--")
}
};
*/
    }
}
