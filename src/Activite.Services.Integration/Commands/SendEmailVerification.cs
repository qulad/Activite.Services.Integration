using Activite.Services.Integration.Attributes;
using Convey.CQRS.Commands;

namespace Activite.Services.Integration.Commands;

[Contract]
public class SendEmailVerification : ICommand
{
    public string Username { get; set; }

    public string Email { get; set; }

    public string Code { get; set; }

    public SendEmailVerification(string username, string email, string code)
    {
        Username = username;
        Email = email;
        Code = code;
    }
}