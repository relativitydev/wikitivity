using kCura.Relativity.Client;
using Relativity.API;
using Relativity.CustomPages;
using System;
using System.Collections.Generic;
using System.Net;

namespace Wikitivity.CustomPage
{
	public partial class Default : System.Web.UI.Page
	{
		public string urlTest;
		public static List<string> PreviewPaneList = new List<string>();
		public static WikitivityHelper wHelper = new WikitivityHelper();
		public static int WorkspaceID;
		public static string[] parsedRequestText;
		public static string RequestPagesURL;
		public static string FullRequestURL;
		public static int totalCount;
		protected void Page_Load(object sender, EventArgs e)
		{

			ServicePointManager.Expect100Continue = true;
			//ServicePointManager.SecurityProtocol =
			//	SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls;
			//ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
			System.Net.ServicePointManager.ServerCertificateValidationCallback =
				((send, certificate, chain, sslPolicyErrors) => true);
			//Nothing done on PLoad   
		}

		public static void requestWork(WikitivityHelper wHelper, string term)
		{
			////delete this too
			/// url

			string url = "u";
			wHelper.previewRequest(url, term);
		}

		protected void previewPane_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Nothing done
		}

		protected void previewButton_OnClick(object sender, EventArgs e)
		{
			ServicePointManager.Expect100Continue = true;
			//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			System.Net.ServicePointManager.ServerCertificateValidationCallback += (send, certificate, chain, sslPolicyErrors) => { return true; };

			countLabel.Font.Bold = false;
			string prefixContent = prefixText.Text;
			string pageContent = pageRequestText.Text;

			if (!string.IsNullOrEmpty(prefixText.Text))
			{
				Session["finalPrefix"] = prefixContent;
			}

			if (!string.IsNullOrEmpty(pageRequestText.Text))
			{
				var PreviewPaneItemsSessionList = Session["PreviewPaneItems"] as List<string>;
				//Create new, if null
				if (PreviewPaneItemsSessionList == null)
				{
					PreviewPaneItemsSessionList = new List<string>();
				}
				requestLabel.Text = "";
				try
				{
					if (submitJobButton.Visible == false) //Added
					{
						submitJobButton.Visible = true;
					}
					if (previewPane.Items.Count > 0)
					{
						previewPane.Items.Clear();

					}
					countLabel.Text = "";
					countLabel.Visible = false;

					var requestText = pageRequestText.Text;
					parsedRequestText = requestText.Split(';');
					foreach (var pageRequest in parsedRequestText)
					{

						string requestTerm = pageRequest;
						//remove this later
						urlTest =
						   $"http://en.wikipedia.org/w/api.php?format=json&action=query&list=categorymembers&cmtitle=Category:{requestTerm}&cmlimit=500";


						//Foreach parsed value... do the thing
						foreach (string PageTitle in wHelper.previewRequest(pageRequest, urlTest))
						{
							//This needs to be fixed to read template, list, etc
							if (!PageTitle.Contains("Category:") && !PageTitle.Contains("Template:"))
							{
								previewPane.Items.Add(PageTitle);
								PreviewPaneItemsSessionList.Add(PageTitle);
							}
						}
					}

					Session["PreviewPaneItems"] = PreviewPaneItemsSessionList;

					if (previewPane.Visible != true && countLabel.Visible != true)
					{
						previewPane.Visible = true;
						previewPane.Height = 250;
						countLabel.Text = "Total count of Articles: " + previewPane.Items.Count.ToString();
						countLabel.Visible = true;
						totalCount = previewPane.Items.Count;
					}
					else
					{
						countLabel.Text = "Total count of Articles: " + previewPane.Items.Count.ToString();
						countLabel.Visible = true;
						totalCount = previewPane.Items.Count;
					}
				}
				catch (Exception exception)
				{
					requestLabel.Text = exception.ToString() + " \r\n\r\n" + ServicePointManager.SecurityProtocol.ToString() + " \r\n\r\n " + urlTest;
				}

			}
			else
			{
				submitJobButton.Visible = false;
				previewPane.Visible = false;
				countLabel.Font.Bold = true;
				countLabel.Text = "Please enter a valid search term!";
			}

		}

		protected void submitJobButton_Click(object sender, EventArgs e)
		{
			requestLabel.Text = "Submitting request...";

			if (submitJobButton.Visible == true) //Added
			{
				submitJobButton.Visible = false;
			}
			previewPane.Visible = true;

			// CUSTOM DEBUGGING LOGIC HERE REMOVE AFTER USE



			// if statement here for if the preview pane is empty or not to fork the logic? if empty, use article title, if populated use the prefix textbox 
			try
			{
				IRSAPIClient proxy = ConnectionHelper.Helper().GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System);
				int workspaceID = ConnectionHelper.Helper().GetActiveCaseID();
				//Logic to write each row to the table. we must include a 
				string requestIDGuid = Guid.NewGuid().ToString();
				int count = 000001;

				foreach (object page in previewPane.Items)
				{
					try
					{
						wHelper.WriteToTable(requestIDGuid, page.ToString(), proxy, workspaceID, prefixText.Text, count);
						count++;
					}
					catch (Exception ex)
					{
						countLabel.Text = (ex.ToString());
					}
				}
				wHelper.UpdateRequestHistory(proxy, requestIDGuid, ConnectionHelper.Helper().GetAuthenticationManager().UserInfo.FullName, previewPane.Items.Count, workspaceID, pageRequestText.Text, prefixText.Text);
				requestLabel.Text = "Successfully submitted job!";
			}
			catch (Exception ex)
			{
				requestLabel.Text = ex.ToString();
			}
		}

		protected void viewCategoriesButton_Click(object sender, EventArgs e)
		{
			//ScriptManager.RegisterStartupScript(this, typeof(string), "OPEN_WINDOW", "var Mleft = (screen.width/2)-(760/2);var Mtop = (screen.height/2)-(700/2);window.open( 'https://en.wikipedia.org/wiki/Portal:Contents/Categories', null, 'height=700,width=760,status=yes,toolbar=no,scrollbars=yes,menubar=no,location=no,top=\'+Mtop+\', left=\'+Mleft+\'' );", true);

		}
	}
}