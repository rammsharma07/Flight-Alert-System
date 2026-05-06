using Domain.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.SMS
{
	public interface ISmsServices
	{
		string SendSms(string toPhoneNumber, string message);
	}
}
