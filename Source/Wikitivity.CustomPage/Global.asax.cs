using System;
using System.Net;

namespace Wikitivity.CustomPage
{
	public class Global : System.Web.HttpApplication
	{
		protected void Application_Start(object sender, EventArgs e)
		{
			// ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			ServicePointManager.Expect100Continue = true;
			//  ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			ServicePointManager.Expect100Continue = true;
			//ServicePointManager.SecurityProtocol =
			// SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls;
			//	ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
			System.Net.ServicePointManager.ServerCertificateValidationCallback =
				((send, certificate, chain, sslPolicyErrors) => true);

		}
	}
}