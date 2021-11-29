using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramBotAvailabilityCheck
{
    public class AvailabilityChecker
    {
        public event EventHandler<WebsiteAvailableEventArgs> WebsiteAvailableHandler;

        private readonly Dictionary<string, List<long>> _dependencies;
        private readonly TimeSpan _periodOfCheck;
        private CancellationTokenSource _cancellationTokenSource;

        public AvailabilityChecker(TimeSpan periodOfCheck)
        {
            _dependencies = new Dictionary<string, List<long>>();
            _periodOfCheck = periodOfCheck;
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
            PeriodicalCheck(token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _dependencies.Clear();
        }

        public void AddDependency(string url, long chatId)
        {
            if (WebsiteHelper.IsAvailable(url))
            {
                OnWebsiteAvailable(url, chatId);
                return;
            }

            var found = _dependencies.TryGetValue(url, out var listOfUsers);
            if (!found)
            {
                listOfUsers = new List<long>();
                _dependencies[url] = listOfUsers;
            }

            listOfUsers.Add(chatId);
        }

        public void RemoveDependency(string url, long chatId)
        {
            var found = _dependencies.TryGetValue(url, out var listOfUsers);
            if (found)
            {
                listOfUsers.Remove(chatId);
            }
        }

        private async void PeriodicalCheck(CancellationToken token)
        {
            while (true)
            {
                try
                {
                    await Task.Delay(_periodOfCheck, token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }

                CheckDependencies();
            }
        }

        private void CheckDependencies()
        {
            var toBeDeleted = new List<string>();

            foreach (var (url, chatIdList) in _dependencies)
            {
                if (!WebsiteHelper.IsAvailable(url)) continue;

                foreach (var chatId in chatIdList)
                    OnWebsiteAvailable(url, chatId);
                
                toBeDeleted.Add(url);
            }

            foreach (var url in toBeDeleted) _dependencies.Remove(url);
        }

        private void OnWebsiteAvailable(string url, long chatId)
        {
            var args = new WebsiteAvailableEventArgs
                {Url = url, ChatId = chatId};
            WebsiteAvailableHandler?.Invoke(this, args);
        }
    }

    public class WebsiteAvailableEventArgs : EventArgs
    {
        public string Url { get; set; }
        public long ChatId { get; set; }
    }
}