using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.Extensions.Options;
using Domain.Sms;
using Application.Interfaces.SMS;
using Serilog;

namespace Web.Services.SMS
{
	public class SmsService:ISmsServices
    {
		private readonly string _accountSid;
		private readonly string _authToken;
		private readonly string _fromPhoneNumber;

		// Constructor with Dependency Injection
		public SmsService(IOptions<SmsSenderOptions> smsSenderOptions)
		{
			_accountSid = smsSenderOptions.Value.AccountSid;
			_authToken = smsSenderOptions.Value.AuthToken;
			_fromPhoneNumber = smsSenderOptions.Value.FromPhoneNumber;

			// Initialize Twilio Client
			TwilioClient.Init(_accountSid, _authToken);
		}


		// Function to send SMS
		public string SendSms(string toPhoneNumber, string message)
		{
			try
			{
				var messageOptions = new CreateMessageOptions(
					new PhoneNumber(toPhoneNumber))
				{
					From = new PhoneNumber(_fromPhoneNumber),
					Body = message,

					StatusCallback = new Uri("https://staging.pvgtracker.com/api/webhook/events")
				};

				var msg = MessageResource.Create(messageOptions);

				
				return msg.Sid;
			}
			catch (Exception ex)
			{
				Log.Information("SMS Send Error: " + ex.Message);
				return null;
			}
		}

	}
}
