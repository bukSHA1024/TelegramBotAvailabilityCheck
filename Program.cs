using System;
using System.IO;
using System.Linq;
using Telegram.Bot;

namespace TelegramBotAvailabilityCheck
{
    class Program
    {
        static void Main(string[] args)
        {
            var secretToken = ReadSecretToken();
            var bot = CreateBot(secretToken);
        }

        private static TelegramBotClient CreateBot(string secretToken)
        {
            return new TelegramBotClient(secretToken);
        }

        private static string ReadSecretToken()
        {
            var pathToSecretFile = @"./secret.token";
            string[] token;
            try
            {
                token = File.ReadAllLines(pathToSecretFile);

            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to read secret file.", e);
                return string.Empty;
            }

            if (token.Any())
                return token.First();

            return string.Empty;
        }
    }
}