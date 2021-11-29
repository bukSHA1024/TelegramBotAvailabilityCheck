using System.Net;

namespace TelegramBotAvailabilityCheck
{
    public static class WebsiteHelper
    {
        public static bool IsAvailable(string url)
        {
            var request = (HttpWebRequest) WebRequest.Create(url);
            request.Timeout = 15000;
            request.Method = "HEAD";
            try
            {
                using var response = (HttpWebResponse) request.GetResponse();
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (WebException)
            {
                return false;
            }
        }
    }
}