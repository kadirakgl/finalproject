using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Services
{
    public class EmailService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public EmailService(IConfiguration configuration)
        {
            _apiKey = configuration["Brevo:ApiKey"];
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string htmlContent, string from = "noreply@bitirmeproje.com", string fromName = "TaskManager")
        {
            var payload = new
            {
                sender = new { name = fromName, email = from },
                to = new[] { new { email = to } },
                subject = subject,
                htmlContent = htmlContent
            };
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.brevo.com/v3/smtp/email", content);
            return response.IsSuccessStatusCode;
        }
    }
}
