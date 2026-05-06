using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Webhook
{
	public class SendGridEvent
	{
		[JsonProperty("email")]
		public string Email { get; set; }

		[JsonProperty("event")]
		public string Event { get; set; }

		[JsonProperty("timestamp")]
		public long Timestamp { get; set; }

		[JsonProperty("smtp-id")]
		public string SmtpId { get; set; }

		[JsonProperty("sg_message_id")]
		public string MessageId { get; set; }
	}
}
