using System.Net;
using Newtonsoft.Json;
using Ubee.Service.Interfaces;
using Ubee.Service.DTOs.Phones;
using Microsoft.Extensions.Configuration;

namespace Ubee.Service.Services
{
    public class PhoneService : IPhoneService
    {
        private readonly string BASE_URL = "";
        private readonly string API_KEY = "";
        private readonly string SENDER = "";
        private readonly string EMAIL = "";
        private readonly string PASSWORD = "";
        private string TOKEN = "";
        public PhoneService(IConfiguration config)
        {
            BASE_URL = config["Sms:BaseURL"]!;
            SENDER = config["Sms:Sender"]!;
            EMAIL = config["Sms:Email"]!;
            PASSWORD = config["Sms:Password"]!;
        }

        public async Task<bool> SendMessageAsync(PhoneMessage smsMessage)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(BASE_URL);
            var request = new HttpRequestMessage(HttpMethod.Post, "api/message/sms/send");
            request.Headers.Add("Authorization", $"Bearer {TOKEN}");

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(smsMessage.PhoneNumber), "mobile_phone");
            content.Add(new StringContent(smsMessage.Title + " " + smsMessage.Content), "message");
            content.Add(new StringContent(SENDER), "from");
            content.Add(new StringContent("http://0000.uz/test.php"), "callback_url");
            request.Content = content;
            var response = await client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await LoginAsync();
                return await SendMessageAsync(smsMessage);
            }
            else if (response.IsSuccessStatusCode) return true;
            else return false;
        }

        private async Task LoginAsync()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(BASE_URL);
            var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/login");

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(EMAIL), "email");
            content.Add(new StringContent(PASSWORD), "password");
            request.Content = content;
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                EskizLoginDto dto = JsonConvert.DeserializeObject<EskizLoginDto>(json)!;
                TOKEN = dto.Data.Token;
            }
        }
    }
}
