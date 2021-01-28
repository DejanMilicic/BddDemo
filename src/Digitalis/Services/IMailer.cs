namespace Digitalis.Services
{
    public interface IMailer
    {
        public void SendMail(string address, string subject, string body);
    }
}
