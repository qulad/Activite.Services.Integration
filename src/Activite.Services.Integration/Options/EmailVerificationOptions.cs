namespace Activite.Services.Integration.Options;

public class EmailVerificationOptions
{
    public const string EmailVerification = "EmailVerification";

    public string ApiKey { get; set; }
    
    public string SenderEmail { get; set; }
}