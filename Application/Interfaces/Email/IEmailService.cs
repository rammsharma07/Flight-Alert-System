using Domain.Email;
using SendGrid.Helpers.Mail;

namespace Application.Interfaces.Email
{
	public interface IEmailService
	{
		/// <summary>
		/// Send Email Details
		/// </summary>
		/// <param name="emailDetails"></param>
		/// <returns></returns>
		Task SendEmailDetails(EmailDetails emailDetails);

		/// <summary>
		/// SendEmailAsync
		/// </summary>
		/// <param name="emailDetails"></param>
		/// <returns></returns>
		Task<bool> SendEmailAsync(EmailDetails emailDetails);

		/// <summary>
		/// SendEmailAsyncnew
		/// </summary>
		/// <param name="emailDetails"></param>
		/// <returns></returns>
		Task<string> SendEmailAsyncnew(EmailDetails emailDetails);

		/// <summary>
		/// Bulk Send Email Details
		/// </summary>
		/// <param name="messages"></param>
		/// <param name="APIKey"></param>
		/// <returns></returns>
		Task BulkSendEmailDetails(List<SendGridMessage> messages, string APIKey);
	}
}
