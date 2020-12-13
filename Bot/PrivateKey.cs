using System;
using Telegram.Bot;

namespace Bot
{
    internal class PrivateKey
    {
        internal static TelegramBotClient newKey()
        {
            return new TelegramBotClient("1307723925:AAGoudgP99mVb0BWFlggHojxyJWi5psbfbU");
        }

        internal static string getPassword()
        {
            return "\"&\"MbTTJv3nhJc";
        }
    }
}