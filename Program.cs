using System;
using System.Linq;
using File = System.IO.File;

namespace TelegramBotAvailabilityCheck
{
    internal class Program
    {
        private static void Main()
        {
            var availabilityChecker = new AvailabilityChecker(TimeSpan.FromSeconds(60));
            availabilityChecker.Start();

            var secretToken = ReadSecretToken();
            var bot = new AvailabilityBot(secretToken, availabilityChecker);
            
            bot.Start();

            Console.ReadLine();
            
            bot.Stop();
            availabilityChecker.Stop();
        }

        private static string ReadSecretToken()
        {
            const string pathToSecretFile = @"./secret.token";
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