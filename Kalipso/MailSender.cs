using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

namespace Kalipso
{
	class MailSender
	{

		MailMessage message;
		SmtpClient smtp;

		public MailSender()
		{
			message = new MailMessage();
			message.Subject = "News from Kalypso";
			message.From = new MailAddress("kalypso.assistant@gmail.com");

			smtp = new SmtpClient("smtp.gmail.com");
			smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
			smtp.UseDefaultCredentials = false;
			smtp.Credentials = new NetworkCredential("kalypso.assistant@gmail.com", "kalypsomail111");
			smtp.Port = 587;
			smtp.EnableSsl = true;
		}

		static Dictionary<string, string> mails = new Dictionary<string, string>
		{
			{"anna", "anamaria.ghidarcea95@gmail.com"},
			{"theodore", "teoxyz95@gmail.com" },
			{"laura", "laurahuzumcomanici@gmail.com" },
			{"boss", "bianca.ioana.boboc@gmail.com" }

		};

		public void SendMail(string command)
		{
			try
			{
				string person = command.Substring(0, command.IndexOf(' '));
				if (string.IsNullOrEmpty(person))
					throw new Exception("Mail usage: Send mail to [person] [body]");

				string body = command.Substring(command.IndexOf(' ') + 1);
				if (string.IsNullOrEmpty(person))
					throw new Exception("Mail usage: Send mail [person] [body]");

				string to = mails[person];
				if (string.IsNullOrEmpty(to))
					throw new Exception("Nu s-a gasit destinatarul " + person);

				message.Body = body;
				message.To.Add(to);

				smtp.Send(message);
				Console.WriteLine("Mail sent!");
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}
	}
}
