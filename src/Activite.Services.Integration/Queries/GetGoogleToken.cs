using System;
using Activite.Services.Integration.Attributes;
using Activite.Services.Integration.DTOs;
using Convey.CQRS.Queries;

namespace Activite.Services.Integration.Queries;

[Contract]
public class GetGoogleToken : IQuery<GoogleTokenDto>
{
    public Guid UserId { get; set; }

    public string AccessToken { get; set; }

    public GetGoogleToken(string accessToken, Guid userId)
    {
        AccessToken = accessToken;
        UserId = userId;
    }
}