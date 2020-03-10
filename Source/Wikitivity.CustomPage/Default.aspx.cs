using kCura.Relativity.Client;
using Relativity.API;
using Relativity.CustomPages;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Relativity.Services.Objects;

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
		public static List<WikiConstants.WikiRequest> CreateRequestList = new List<WikiConstants.WikiRequest>();

		protected void Page_Load(object sender, EventArgs e)
		{
		}
		protected void previewPane_SelectedIndexChanged(object sender, EventArgs e)
		{
		}

		protected void previewButton_OnClick(object sender, EventArgs e)
		{
			countLabel.Font.Bold = false;
			string prefixContent = prefixText.Text;

			if (!string.IsNullOrEmpty(prefixText.Text))
			{
				Session["finalPrefix"] = prefixContent;
			}

			if (!string.IsNullOrEmpty(pageRequestText.Text))
			{
				var PreviewPaneItemsSessionList = Session["PreviewPaneItems"] as List<string>;
				// Create new, if null
				if (PreviewPaneItemsSessionList == null)
				{
					PreviewPaneItemsSessionList = new List<string>();
				}
				requestLabel.Text = "";
				try
				{
					if (submitJobButton.Visible == false)
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
						// Foreach parsed value, preview the request objects
						foreach (string PageTitle in wHelper.previewRequest(pageRequest))
						{
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

			if (submitJobButton.Visible == true)
			{
				submitJobButton.Visible = false;
			}
			previewPane.Visible = true;

			try
			{
				IObjectManager proxy = ConnectionHelper.Helper().GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System);
				int workspaceID = ConnectionHelper.Helper().GetActiveCaseID();
				string requestIDGuid = Guid.NewGuid().ToString();
				int count = 000001;
				//TODO: Can I turn this into a mass op using OM?
				foreach (object page in previewPane.Items)
				{

					WikiConstants.WikiRequest singleWikiRequest = new WikiConstants.WikiRequest()
					{
						prefixText = prefixText.Text,
						count = count,
						Page = page.ToString(),
						Proxy = proxy,
						RequestIDGuid = requestIDGuid,
						workspaceID = workspaceID
					};


					CreateRequestList.Add(singleWikiRequest);
					count++;
				} // at this point this will create a full list of the objects I need from the preview pane. I'll then need to push these to the request method. 

				try
				{
					Task.Run(async () =>
					{

						await wHelper.WriteToTableOM(CreateRequestList);

					});

				}
				catch (Exception ex)
				{
					countLabel.Text = (ex.ToString());
				}
			//	IReadOnlyList<object> ListOfRequestsToCreate = new List<object>() { requestIDGuid, page.ToString(), proxy, workspaceID, prefixText.Text, count };

					//try
					//{
					//	wHelper.WriteToTable(requestIDGuid, page.ToString(), proxy, workspaceID, prefixText.Text, count);
					//	//count++;
					//}
					//catch (Exception ex)
					//{
					//	countLabel.Text = (ex.ToString());
					//}
				
				//wHelper.UpdateRequestHistory(proxy, requestIDGuid, ConnectionHelper.Helper().GetAuthenticationManager().UserInfo.FullName, previewPane.Items.Count, workspaceID, pageRequestText.Text, prefixText.Text);
				requestLabel.Text = "Successfully submitted job!";
			}
			catch (Exception ex)
			{
				requestLabel.Text = ex.ToString();
			}
		}
		//		protected void submitJobButton_Click(object sender, EventArgs e)
		//		{
		//			requestLabel.Text = "Submitting request...";

		//			if (submitJobButton.Visible == true)
		//			{
		//				submitJobButton.Visible = false;
		//			}
		//			previewPane.Visible = true;

		//			try
		//			{
		//				IRSAPIClient proxy = ConnectionHelper.Helper().GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System);
		//				int workspaceID = ConnectionHelper.Helper().GetActiveCaseID();
		//				string requestIDGuid = Guid.NewGuid().ToString();
		//				int count = 000001;
		////TODO: Can I turn this into a mass op using OM?
		//				foreach (object page in previewPane.Items)
		//				{
		//					try
		//					{
		//						wHelper.WriteToTable(requestIDGuid, page.ToString(), proxy, workspaceID, prefixText.Text, count);
		//						count++;
		//					}
		//					catch (Exception ex)
		//					{
		//						countLabel.Text = (ex.ToString());
		//					}
		//				}
		//				wHelper.UpdateRequestHistory(proxy, requestIDGuid, ConnectionHelper.Helper().GetAuthenticationManager().UserInfo.FullName, previewPane.Items.Count, workspaceID, pageRequestText.Text, prefixText.Text);
		//				requestLabel.Text = "Successfully submitted job!";
		//			}
		//			catch (Exception ex)
		//			{
		//				requestLabel.Text = ex.ToString();
		//			}
		//		}
		protected void viewCategoriesButton_Click(object sender, EventArgs e)
		{
		}
	}
}