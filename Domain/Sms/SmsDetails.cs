using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Sms
{
	public class SmsSenderOptions
	{
		public string AccountSid { get; set; }
		public string AuthToken { get; set; }
		public string FromPhoneNumber { get; set; }
	}

}
