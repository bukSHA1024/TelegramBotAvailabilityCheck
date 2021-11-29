using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace TelegramBotAvailabilityCheck
{
    class Program
    {
        static void Main(string[] args)
        {
            var secretToken = ReadSecretToken();
            var bot = CreateBot(secretToken);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            ConfigureBot(bot, cancellationToken);

            Console.ReadLine();
        }

        private static void ConfigureBot(TelegramBotClient bot, CancellationToken cancellationToken)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // receive all update types
            };

            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
        }

        static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is { } message)
            {
                await botClient.SendTextMessageAsync(message.Chat, "Hello", cancellationToken: cancellationToken);
            }
        }

        static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ApiRequestException apiRequestException)
            {
                //await botClient.SendTextMessageAsync(adminId, apiRequestException.ToString(), cancellationToken: cancellationToken);
            }
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