
using Models;

namespace Services
{
    public interface IMailService
    {
        Task<bool> SendAsync(MailData mailData, CancellationToken ct = default);
        Task<bool> SendMailAsync(string toAddress, string Subject, string Body);
    }
}