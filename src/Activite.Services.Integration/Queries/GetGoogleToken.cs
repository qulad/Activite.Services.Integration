using Activite.Services.Integration.Attributes;
using Activite.Services.Integration.DTOs;
using Convey.CQRS.Queries;

namespace Activite.Services.Integration.Queries;

[Contract]
public class GetGoogleToken : IQuery<GoogleTokenDto>
{
    public string AccessToken { get; set; }

    public GetGoogleToken(string accessToken)
    {
        AccessToken = accessToken;
    }
}