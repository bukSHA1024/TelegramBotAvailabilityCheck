using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace TelegramBotAvailabilityCheck
{
    public class AvailabilityBot : TelegramBotClient
    {
        private readonly CancellationTokenSource _cts;
        private readonly AvailabilityChecker _checker;

        public AvailabilityBot([NotNull] string token, AvailabilityChecker checker, [CanBeNull] HttpClient? httpClient = null, [CanBeNull] string? baseUrl = null) : base(token, httpClient, baseUrl)
        {
            _checker = checker;
            _cts = new CancellationTokenSource();

            _checker.WebsiteAvailableHandler += MustSentStatus;
        }

        public void Start()
        {
            var cancellationToken = _cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // receive all update types
            };

            this.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is { } message)
            {
                if (message.Text == null)
                    return;

                if (message.Text.StartsWith("del"))
                {
                    var splittedCommand = message.Text.Split();
                    if (splittedCommand.Length != 2)
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Wrong command",
                            cancellationToken: cancellationToken);
                        return;
                    }

                    var url = splittedCommand.ElementAt(1);
                    if (IsUrl(url))
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Removed",
                            cancellationToken: cancellationToken);
                        _checker.RemoveDependency(message.Text, message.Chat.Id);
                        return;
                    }
                }

                if (IsUrl(message.Text))
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Accepted",
                        cancellationToken: cancellationToken);
                    _checker.AddDependency(message.Text, message.Chat.Id);
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Not accepted",
                        cancellationToken: cancellationToken);
                }
            }
        }

        private async void MustSentStatus(object sender, WebsiteAvailableEventArgs args)
        {
            await this.SendTextMessageAsync(args.ChatId, $"Website: {args.Url} is available!");
        }

        private static bool IsUrl(string uriName)
        {
            var result = Uri.TryCreate(uriName, UriKind.Absolute, out var uriResult)
                          && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            return result;
        }

        private async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ApiRequestException apiRequestException)
            {
                //await botClient.SendTextMessageAsync(adminId, apiRequestException.ToString(), cancellationToken: cancellationToken);
            }
        }
    }
}