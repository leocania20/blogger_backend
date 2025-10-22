using SendGrid;
using SendGrid.Helpers.Mail;

namespace blogger_backend.Utils
{
    public class SendGridEmailServices
    {
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public SendGridEmailServices(IConfiguration config)
        {
            _apiKey = config["SendGrid:ApiKey"] ?? Environment.GetEnvironmentVariable("send_email_blogger") ?? throw new Exception("SendGrid API Key não configurada!");

            _fromEmail = config["SendGrid:FromEmail"] ?? "no-reply@seusite.com";
            _fromName = config["SendGrid:FromName"] ?? "Blogger";
        }
        public async Task<Response> SendEmailWithResponseAsync(string toEmail, string subject, string htmlContent, string plainText = "")
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, htmlContent);

            var response = await client.SendEmailAsync(msg);

            Console.WriteLine($"[SendGrid] To: {toEmail} Status: {response.StatusCode}");
            var body = await response.Body.ReadAsStringAsync();
            Console.WriteLine($"[SendGrid] Body: {body}");

            return response;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent, string plainText = "")
        {
            var response = await SendEmailWithResponseAsync(toEmail, subject, htmlContent, plainText);
            return response.IsSuccessStatusCode;
        }
    }
}
