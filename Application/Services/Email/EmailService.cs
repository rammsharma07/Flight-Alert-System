using Application.Interfaces.Email;
using Domain.Email;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;

namespace Application.Services.Email
{
	public class EmailService : IEmailService
	{
		/// <summary>
		/// Send Email Details
		/// </summary>
		/// <param name="emailDetails"></param>
		/// <returns></returns>
		public async Task SendEmailDetails(EmailDetails emailDetails)
		{
			try
			{
				var client = new SendGridClient(emailDetails.APIKey);
				var from = new EmailAddress(emailDetails.FromMailNew, emailDetails.EmailFromName);
				var to = new EmailAddress(emailDetails.EmailID, emailDetails.FullName);
				var msg = MailHelper.CreateSingleEmail(from, to, emailDetails.Subject, emailDetails.MessageBody, emailDetails.MessageBody);
				var response = await client.SendEmailAsync(msg);
			}
			catch
			{
			}
		}

		/// <summary>
		///  SendEmailAsync
		/// </summary>
		/// <param name="emailDetails"></param>
		/// <returns></returns>
		public async Task<bool> SendEmailAsync(EmailDetails emailDetails)
		{
			try
			{
				var client = new SendGridClient(emailDetails.APIKey);
				var from = new EmailAddress(emailDetails.FromMailNew, emailDetails.EmailFromName);
				var to = new EmailAddress(emailDetails.EmailID, emailDetails.FullName);
				var msg = MailHelper.CreateSingleEmail(from, to, emailDetails.Subject, emailDetails.MessageBody, emailDetails.MessageBody);
				var response = await client.SendEmailAsync(msg);


				return response.StatusCode == HttpStatusCode.Accepted
					|| response.StatusCode == HttpStatusCode.OK;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		///  SendEmailAsyncnew
		/// </summary>
		/// <param name="emailDetails"></param>
		/// <returns></returns>
		public async Task<string> SendEmailAsyncnew(EmailDetails emailDetails)
		{
			try
			{
				var client = new SendGridClient(emailDetails.APIKey);

				var from = new EmailAddress(emailDetails.FromMailNew, emailDetails.EmailFromName);
				var to = new EmailAddress(emailDetails.EmailID, emailDetails.FullName);

				var msg = MailHelper.CreateSingleEmail(
					from,
					to,
					emailDetails.Subject,
					emailDetails.MessageBody,
					emailDetails.MessageBody
				);

				var response = await client.SendEmailAsync(msg);

				if (response.Headers.TryGetValues("X-Message-Id", out var values))
				{
					return values.FirstOrDefault();
				}

				return null;
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Bulk Send Email Details
		/// </summary>
		/// <param name="messages"></param>
		/// <param name="APIKey"></param>
		/// <returns></returns>
		public async Task BulkSendEmailDetails(List<SendGridMessage> messages, string APIKey)
		{
			try
			{
				var client = new SendGridClient(APIKey);
				// Send emails asynchronously
				var tasks = messages.Select(async msg => await client.SendEmailAsync(msg));
				await Task.WhenAll(tasks);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error occurred while sending bulk emails: {ex.Message}");
				throw;
			}
		}

	}
}
