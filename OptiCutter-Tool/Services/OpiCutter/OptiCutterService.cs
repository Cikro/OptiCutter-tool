using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace OptiCutter_Tool.Services.OptiCutter
{
    public interface IOptiCutterService
    {
        Task<OptiCutterLinearCutCalculatorResponse> GetCalculatedBoardsUrl(OptiCutterLinearCutCalculatorRequest data);
        Task<byte[]> GetCalculatedBoardsPdf(Cookie sessionCookie);
    }

    public class OptiCutterService : IOptiCutterService
    {
        public const string HttpClientFactoryName = "OptiCutter";
        private const string SessionCookieName = "JSESSIONID";

        private readonly IHttpClientFactory _clientFactory;
        private readonly string _clientFactoryName;

        public OptiCutterService(IHttpClientFactory clientFactory, string httpClientFactoryName = HttpClientFactoryName)
        {
            _clientFactory = clientFactory;
            _clientFactoryName = httpClientFactoryName;
        }

        public async Task<OptiCutterLinearCutCalculatorResponse> GetCalculatedBoardsUrl(OptiCutterLinearCutCalculatorRequest data)
        {
            using (var client = _clientFactory.CreateClient(_clientFactoryName))
            {
                var endPoint = "/linear-cut-list-calculator";
                var request = new HttpRequestMessage(HttpMethod.Post, endPoint);

                request.Content = GetCalculatedBoardsFormData(data);

                var response = await client.SendAsync(request);

                if (response.StatusCode != HttpStatusCode.Redirect && response.StatusCode != HttpStatusCode.RedirectMethod)
                {
                    throw new HttpRequestException($"Request to {endPoint}:  Expected {HttpStatusCode.Redirect} or {HttpStatusCode.RedirectMethod}  but recieved {response.StatusCode}");
                }

                Cookie sessionCookie = null;
                // Find Session Cookie. Not a general way to parse cookies.
                foreach (string cookie in response.Headers.GetValues("Set-Cookie"))
                {
                    var cookieItems = cookie.Split(";");

                    var cookieNameVal = cookieItems[0].Split("=");
                    var cookieName = cookieNameVal[0];
                    var cookieVal = cookieNameVal[1];

                    if (cookieName == SessionCookieName)
                    {
                        sessionCookie = new Cookie(cookieName, cookieVal);
                        break;
                    }
                }

                return new OptiCutterLinearCutCalculatorResponse
                {
                    SessionCookie = sessionCookie,
                    CalculatorResultUrl = response.Headers.Location
                };

            }
        }
        public async Task<byte[]> GetCalculatedBoardsPdf(Cookie sessionCookie)
        {
            using (var client = _clientFactory.CreateClient(_clientFactoryName))
            {
                var endPoint = "/linear-cut-list-calculator/pdf";
                var request = new HttpRequestMessage(HttpMethod.Get, endPoint);
                request.Headers.Add("Cookie", $"{sessionCookie.Name}={sessionCookie.Value}");

                var response = await client.SendAsync(request);

                response.EnsureSuccessStatusCode();

                var contentType = response.Content.Headers.ContentType.MediaType;
                if (contentType != "application/pdf")
                {
                    throw new HttpRequestException($"Request to {endPoint}:  Expected content type \"application/pdf\" but recieved \"{contentType}\"");
                }

                return await response.Content.ReadAsByteArrayAsync();
            }
        }

        private MultipartFormDataContent GetCalculatedBoardsFormData(OptiCutterLinearCutCalculatorRequest data)
        {
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(data.Kerf.ToString()), "settings.kerf");
            formData.Add(new StringContent("."), "name");

            for (int i = 0; i < data.Stock.Count; i++)
            {
                formData.Add(new StringContent(data.Stock[i].Length.ToString()), $"stocks[{i}].length");
                formData.Add(new StringContent(data.Stock[i].Quantity.ToString()), $"stocks[{i}].count");
            }

            for (int i = 0; i < data.Requirements.Count; i++)
            {
                formData.Add(new StringContent(data.Requirements[i].Length.ToString()), $"requirements[{i}].length");
                formData.Add(new StringContent(data.Requirements[i].Quantity.ToString()), $"requirements[{i}].count");
            }


            return formData;
        }

    }
}
