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
                    string[] path = l3.Split(@"\");
                    var len = path.Length;
                    x2.Add(new KeyboardButton(path[len-1]));
                }
                replyKeyboard.Add(x2);
            }
            return replyKeyboard;
        }

        /*
        static public List<List<KeyboardButton>> getKeyboard(ScuoleEnums? keyboardType)
        {
            List<List<KeyboardButton>> replyKeyboard = new List<List<KeyboardButton>>();
            if (keyboardType == null)
            {
                return null;
            }
            switch (keyboardType)
            {
                case ScuoleEnums.start:
                    replyKeyboard = new List<List<KeyboardButton>>()
                     {
                        new List<KeyboardButton>()
                        {
                            new KeyboardButton("3I"),
                        },
                        new List<KeyboardButton>()
                        {
                            new KeyboardButton("AUIC")
                        },
                        new List<KeyboardButton>()
                        {
                            new KeyboardButton("Design")
                        },
                        new List<KeyboardButton>()
                        {
                            new KeyboardButton("CAT")
                        }
                     };
                    break;
                case ScuoleEnums.TREI:
                    replyKeyboard = new List<List<KeyboardButton>>()
                    {
                        new List<KeyboardButton>()
                        {
                            new KeyboardButton("MatNano"),
                        },
                        new List<KeyboardButton>()
                        {
                            new KeyboardButton("----")
                        },
                        new List<KeyboardButton>()
                        {
                            new KeyboardButton("----")
                        },
                        new List<KeyboardButton>()
                        {
                            new KeyboardButton("----")
                        },
                        new List<KeyboardButton>()
                        {
                            new KeyboardButton("Go Back")
                        }
                    };
                    break;
                case ScuoleEnums.AIUC:
                    break;
                case ScuoleEnums.Design:
                    break;
                case ScuoleEnums.CAT:
                    break;
                case ScuoleEnums.MatNano:
                    string rootMatNano = @"C:\Repos\matnanorepo";
                    string[] subdirectoryEntries = Directory.GetDirectories(rootMatNano);
                    replyKeyboard = Keyboards.getKeyboard(subdirectoryEntries);
                    break;
                default:
                    break;
            }
            return replyKeyboard;
        }
        */

        internal static List<List<KeyboardButton>> getKeyboardCorsi(ScuoleEnums? scuoleEnums)
        {
            List<List<KeyboardButton>> r = new List<List<KeyboardButton>>();
            foreach (var v in Enum.GetValues(typeof(CorsiEnum)))
            {
                r.Add(new List<KeyboardButton>() { new KeyboardButton() {  Text = v.ToString()} });
            }
            return r;
                
        }

        internal static List<List<KeyboardButton>> getKeyboardPercorsi(int id)
        {
            string corso = Program.dict[id].getcorso();
            if (string.IsNullOrEmpty(corso))
                return null;
            corso = corso.ToLower();
            string rootMatNano = @"C:\Repos\"+corso;
            string percorso = Program.dict[id].getPercorso();
            if (!string.IsNullOrEmpty(percorso))
            {
                rootMatNano += "/" + percorso;
            }
            string[] subdirectoryEntries = Directory.GetDirectories(rootMatNano);
            var k  =  Keyboards.getKeyboard(subdirectoryEntries);
            if (k == null) { } //do nothing
            k.Insert(0, new List<KeyboardButton>() { 
                new KeyboardButton(){  Text = "🔙 back"},
                new KeyboardButton(){  Text = "🆗 Cartella Corrente"}
            });
            return k;
        }

        internal static List<List<KeyboardButton>> getKeyboardScuole()
        {
            List<List<KeyboardButton>> r = new List<List<KeyboardButton>>();
            foreach (var v in Enum.GetValues(typeof(ScuoleEnums)))
            {
                r.Add(new List<KeyboardButton>() { new KeyboardButton() { Text = v.ToString() } });
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
