using System;
using System.Collections.Generic;
using System.Net;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Newtonsoft.Json;

namespace Wikitivity.CustomPage
{
	public class WikitivityHelper
	{

		public static Guid WikitivityRequestHistoryRDOGuid = new Guid("EF1DF3CD-2E84-48BE-9E02-AC07DD0481C1");
		public static Guid WikitivityRDOGuid = new Guid("C6196733-E2A6-48F4-9443-37990972EBA3");
		#region classes

		public class CurrentJob
		{
			public string Requests { get; set; }
			public string workspaceID { get; set; }
			public string artifactID { get; set; }
		}

		public partial class WikiCategoryMemberRequest
		{
			public string Batchcomplete { get; set; }
			public Continue Continue { get; set; }
			public Query query { get; set; }
		}

		public partial class Continue
		{
			public string cmcontinue { get; set; }
			public string continueContinue { get; set; }
		}

		public partial class Query
		{
			public List<Categorymember> categorymembers { get; set; }
		}

		public partial class Categorymember
		{
			public long pageid { get; set; }
			public long ns { get; set; }
			public string title { get; set; }
		}

		public partial class WikiCategoryRequest2
		{
			public string Batchcomplete { get; set; }
			public Query2 Query { get; set; }
			public Limits Limits { get; set; }
		}

		public partial class Limits
		{
			public long Extracts { get; set; }
		}

		public partial class Query2
		{
			public Dictionary<string, pageval> pages { get; set; }
		}

		public class pageval
		{
			public int pageid { get; set; }
			public int ns { get; set; }
			public string title { get; set; }
			public string extract { get; set; }
		}



		public class WikiEntry
		{
			public string title { get; set; }
			public string extract { get; set; }
			public int pageID { get; set; }
		}

		public class WikiRequest
		{
			public string ArtifactID { get; set; }
			public string WorkspaceID { get; set; }
			public string RequestURL { get; set; }
			public string Status { get; set; }

		}

		#endregion

		public List<string> previewRequest(string requestTerm, string url)
		{
			//Determine why this can't reach out of R1


			ServicePointManager.Expect100Continue = true;
			//ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
			//ServicePointManager.SecurityProtocol =
			//	SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls;
			System.Net.ServicePointManager.ServerCertificateValidationCallback =
				((sender, certificate, chain, sslPolicyErrors) => true);
		//	System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };


			// ADDED THIS STRING TO TEST AGAINST r1 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
			List<string> TitleCollection = new List<string>();
			int categoryCounter = 0;

			var client = new WebClient();
//removed the s from https :shrug:

			string url111 = url;
		//		$"http://en.wikipedia.org/w/api.php?format=json&action=query&list=categorymembers&cmtitle=Category:{requestTerm}&cmlimit=500";
			var response = client.DownloadString(url111);//$"http://en.wikipedia.org/w/api.php?format=json&action=query&list=categorymembers&cmtitle=Category:{requestTerm}&cmlimit=500");
			var responseJson = JsonConvert.DeserializeObject<WikiCategoryMemberRequest>(response);

			if (responseJson.query.categorymembers.Count > 0)
			{
				foreach (Categorymember singleCategoryMember in responseJson.query.categorymembers)
				{
					if (singleCategoryMember.title.Contains("Category:"))
					{
						categoryCounter += 1;
						if (categoryCounter <= 100) //Mod this to control how intense the search is. 100 has been ok it seems?
						{
							GetPagesFromCategories(singleCategoryMember, client, TitleCollection);
						}

					}
					else
					{
						if (!singleCategoryMember.title.Contains("Category:") && !singleCategoryMember.title.Contains("File:") && !singleCategoryMember.title.Contains("Template:"))
						{
							TitleCollection.Add(singleCategoryMember.title);

						}
					}
				}
			}
			return TitleCollection;
		}

		public void GetPagesFromCategories(Categorymember singleCategoryMember, WebClient client, List<string> TitleCollection)
		{
			// Modifications testing


			//client.Headers.Add("Origin", "");


			var retrievePageMembers = singleCategoryMember.title.Replace(' ', '_');
			var response4 = client.DownloadString(
					$"https://en.wikipedia.org/w/api.php?format=json&action=query&list=categorymembers&cmtitle={retrievePageMembers}&cmlimit=500");
			WikiCategoryMemberRequest additionalCategories = JsonConvert.DeserializeObject<WikiCategoryMemberRequest>(response4);
			if (additionalCategories.query.categorymembers.Count > 0)
			{
				foreach (Categorymember singleCatMem in additionalCategories.query.categorymembers)
				{
					if (!singleCatMem.title.Contains("Category:") && !singleCatMem.title.Contains("File:") && !singleCategoryMember.title.Contains("Template:"))
					{
						TitleCollection.Add(singleCatMem.title);
					}
				}
			}
		}
		public void UpdateRequestHistory(IRSAPIClient proxy, string requestID, string requestUser, int articleCount, int workspaceID, string requestedCategories, string prefix)
		{
			proxy.APIOptions.WorkspaceID = workspaceID;
			RDO wikitivityRequestHistoryRDO = new RDO(WikitivityRequestHistoryRDOGuid);
			List<Guid> guidList = new List<Guid>();
			guidList.Add(WikitivityRequestHistoryRDOGuid);
			wikitivityRequestHistoryRDO.ArtifactTypeGuids = guidList;
			wikitivityRequestHistoryRDO.Fields.Add(new FieldValue() { Name = "Request ID", Value = requestID });
			wikitivityRequestHistoryRDO.Fields.Add(new FieldValue() { Name = "Request User", Value = requestUser });
			wikitivityRequestHistoryRDO.Fields.Add(new FieldValue() { Name = "Request Date", Value = DateTime.Today.ToShortDateString() });
			wikitivityRequestHistoryRDO.Fields.Add(new FieldValue() { Name = "Article Count", Value = articleCount });
			wikitivityRequestHistoryRDO.Fields.Add(new FieldValue() { Name = "Requested Categories", Value = requestedCategories });
			wikitivityRequestHistoryRDO.Fields.Add(new FieldValue() { Name = "Prefix", Value = prefix });


			try
			{
				WriteResultSet<RDO> writeResultSet = proxy.Repositories.RDO.Create(wikitivityRequestHistoryRDO);

			}
			catch (Exception ex)
			{

			}
		}
		public void WriteToTable(string requestID, string pageTitle, IRSAPIClient proxy, int workspaceID, string prefix, int count)
		{

			string requestUrl = $"https://en.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exlimit=max&explaintext&titles={pageTitle}&redirects=";

			proxy.APIOptions.WorkspaceID = workspaceID;
			RDO wikitivityRequestRDO = new RDO(WikitivityRDOGuid);
			
			List<Guid> guidList = new List<Guid>();
			guidList.Add(WikitivityRDOGuid);
			wikitivityRequestRDO.ArtifactTypeGuids = guidList;
			wikitivityRequestRDO.Fields.Add(new FieldValue() { Name = "Request ID", Value = requestID });
			wikitivityRequestRDO.Fields.Add(new FieldValue() { Name = "Request Url", Value = requestUrl });
			// Specify page title
			//wikitivityRequestRDO.Fields.Add(new FieldValue() { Name = "Page Title", Value = pageTitle });

			if (prefix == String.Empty)
			{
				prefix = "WIKI";
			}
			string docID = prefix + count.ToString("D7");
			//else
			//{
			wikitivityRequestRDO.Fields.Add(new FieldValue() { Name = "Name", Value = docID });


			wikitivityRequestRDO.Fields.Add(new FieldValue() { Name = "Page Title", Value = pageTitle });

			//wikitivityRequestRDO.Fields.Add(new FieldValue() { Name = "Page Title", Value = pageTitle });
			//}

			//wikitivityRequestRDO.Fields.Add(new FieldValue() { Name = "Page Title", Value = docID });
			try
			{
				WriteResultSet<RDO> writeResultSet = proxy.Repositories.RDO.Create(wikitivityRequestRDO);

			}
			catch (Exception ex)
			{

			}
		}
	}
}
