using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Activite.Services.Integration.Options;
using Convey.CQRS.Commands;
using Microsoft.Extensions.Options;

namespace Activite.Services.Integration.Commands.Handlers;

public class SendEmailVerificationHandler : ICommandHandler<SendEmailVerification>
{
	private const string Url = "https://api.brevo.com/v3/smtp/email";

	private readonly EmailVerificationOptions _options;
	private readonly IHttpClientFactory _httpClientFactory;

	public SendEmailVerificationHandler(IOptions<EmailVerificationOptions> options, IHttpClientFactory httpClientFactory)
	{
		_options = options.Value;
		_httpClientFactory = httpClientFactory;
	}

	public async Task HandleAsync(SendEmailVerification command, CancellationToken cancellationToken = default)
	{
		using var httpClient = _httpClientFactory.CreateClient();

		var apiKey = _options.ApiKey;
		var senderEmail = _options.SenderEmail;

		var emailBody = GetEmailBody(command.Username, command.Code);

		var verificationRequest = new VerificationRequest
		{
			Name = "Etkinlikçe",
			Subject = "Eposta Adresinizi Doğrulayın",
			Sender = new SenderInfo
			{
				Name = "Etkinlikçe",
				Email = senderEmail
			},
			To = new List<ReceiverInfo>
			{
				new ReceiverInfo
				{
					Name = command.Username,
					Email = command.Email
				}
			},
			Type = "classic",
			HtmlContent = emailBody
		};

		using var request = new HttpRequestMessage
		{
			Method = HttpMethod.Post,
			RequestUri = new Uri(Url),
			Content = new StringContent(JsonSerializer.Serialize(verificationRequest), Encoding.UTF8, "application/json")
		};

		request.Headers.Add("api-key", apiKey);

		using var response = await httpClient.SendAsync(request, cancellationToken);
	}

	private static string GetEmailBody(string username, string code)
	{
		return @"
		<!DOCTYPE html>
		<html lang=\""tr\"">
		<head>
		  <meta charset=\""UTF-8\"">
		  <meta name=\""viewport\"" content=\""width=device-width, initial-scale=1.0\"">
		  <title>Etkinlikçe'ye Hoşgeldiniz!</title>
		  <style>
		    body {
		      font-family: Arial, sans-serif;
		      background-color: #f4f4f4;
		      margin: 0;
		      padding: 0;
		    }
		    .email-container {
		      max-width: 600px;
		      margin: 20px auto;
		      background-color: #ffffff;
		      border: 1px solid #dddddd;
		      border-radius: 8px;
		      overflow: hidden;
		    }
		    .email-header {
		      background-color: #4CAF50;
		      color: #ffffff;
		      text-align: center;
		      padding: 20px;
		    }
		    .email-body {
		      padding: 20px;
		      color: #333333;
		      line-height: 1.6;
		    }
		    .email-body h1 {
		      font-size: 24px;
		      margin: 0 0 20px;
		    }
		    .email-body p {
		      margin: 0 0 20px;
		    }
		    .verify-button {
		      text-align: center;
		      padding: 10px 20px;
		      background-color: #4CAF50;
		      color: #ffffff;
		      text-decoration: none;
		      border-radius: 5px;
		      font-size: 16px;
		    }
		    .verify-button:hover {
		      background-color: #45a049;
		    }
		    .email-footer {
		      background-color: #f9f9f9;
		      color: #555555;
		      text-align: center;
		      padding: 10px;
		      font-size: 12px;
		    }
		  </style>
		</head>
		<body>
		  <div class=\""email-container\"">
		    <div class=\""email-header\"">
		      <h1>Etkinlikçe'ye Hoşgeldiniz!</h1>
		    </div>
		    <div class=\""email-body\"">
		      <h1>Eposta adresinizi doğrulayınız</h1>
		      <p>Merhaba {{username}}, kaydın için teşekkür ederiz. Kayıt işleminizi tamamlamak için lütfen aşağıdaki kod ile hesabınızı onaylayın</p>

		      <div class=\""email-header\"">
		          {{code}}
		      </div>
		      <div style=\""margin-top: 20px;\"">
		          <p style=\""margin-top: 20px\"">Eğer bu hesabı siz oluşturmadıysanız, bu emaili güvenle yok sayabilirsiniz.</p>
		      </div>
		    </div>
		    <div class=\""email-footer\"">
		      <p>&copy; 2024 Etkinlikçe. Tüm hakları saklıdır.</p>
		    </div>
		  </div>
		</body>
		</html>
		".Replace("{{username}}", username).Replace("{{code}}", code);
	}
}

sealed file class VerificationRequest
{
	public string Name { get; set; }
	public string Subject { get; set; }
	public SenderInfo Sender { get; set; }
	public IList<ReceiverInfo> To { get; set; }
	public string Type { get; set; }
	public string HtmlContent { get; set; }
}

sealed file class SenderInfo
{
	public string Name { get; set; }
	public string Email { get; set; }
}

sealed file class ReceiverInfo
{
	public string Name { get; set; }
	public string Email { get; set; }
}