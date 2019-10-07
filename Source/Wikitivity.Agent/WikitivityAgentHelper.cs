using kCura.Agent;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using Newtonsoft.Json;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;

namespace Wikitivity.Agent
{
	class WikitivityAgentHelper
	{
		public static string IAPIMessages = "";

		public bool DeleteUploadedRequestsFromRelativity(IRSAPIClient proxy, int workspaceID, List<WikitivityUploadsAgent.SingleRequestObject> listOfCompletedUploads)
		{
			bool success = false;
			try
			{
				List<Document> DocsToDelete = new List<Document>();
				foreach (var singleDoc in listOfCompletedUploads)
				{
					DocsToDelete.Add(new Document(singleDoc.ArtifactID));
				}

				var DeleteResultSet = proxy.Repositories.Document.Delete(DocsToDelete);
				if (DeleteResultSet.Success)
				{
					success = true;
				}
			}
			catch (Exception ex)
			{
				//That guy
			}
			proxy.APIOptions.WorkspaceID = workspaceID;

			return success;
		}
		public List<WikitivityUploadsAgent.DataObtainedSingleRequestObject> PopulateArticleData(List<WikitivityUploadsAgent.SingleRequestObject> singleRequestObjects, AgentBase agentBase)
		{
			List<WikitivityUploadsAgent.DataObtainedSingleRequestObject> ListForUpload = new List<WikitivityUploadsAgent.DataObtainedSingleRequestObject>();
			agentBase.RaiseMessage("Testing this message", 1);

			try
			{
				foreach (var singlePage in singleRequestObjects)
				{
					var client = new WebClient();

					string newTitleURL = singlePage.RequestUrl;

					// DEBUG


					agentBase.RaiseMessage("Working on this Stuff: " + newTitleURL, 1);

					//
					try
					{

						var response2 = client.DownloadString(newTitleURL);

						agentBase.RaiseMessage("Here's the stuff!" + response2.ToString(), 1);
						var response2Json = JsonConvert.DeserializeObject<WikiCategoryRequest2>(response2);

						foreach (var singleArticleWithInfo in response2Json.Query.pages.Values)
						{

							agentBase.RaiseMessage("Working on this Stuff: " + singleArticleWithInfo.title + "|||" + singlePage.ControlNumVal, 1);
							try
							{
								ListForUpload.Add(new WikitivityUploadsAgent.DataObtainedSingleRequestObject
								{
									extractedText = singleArticleWithInfo.extract,
									PageTitle = singleArticleWithInfo.title,
									ControlNumVal = singlePage.ControlNumVal


								});

							}
							catch (Exception e)
							{
								agentBase.RaiseMessage(e.ToString(), 1);
							}

						}
						string showTitles = "";
						foreach (var test in ListForUpload)
						{
							showTitles += test.PageTitle;
						}
						agentBase.RaiseMessage(showTitles, 1);
					}
					catch (Exception e)
					{
						agentBase.RaiseMessage(e.ToString(), 1);
					}

				}
				return ListForUpload;

			}
			catch (Exception e)
			{
				agentBase.RaiseMessage(e.ToString(), 1);
				return ListForUpload;

			}

		}



		//public List<WikitivityUploadsAgent.DataObtainedSingleRequestObject> PopulateArticleData(List<WikitivityUploadsAgent.SingleRequestObject> singleRequestObjects, AgentBase agentBase)
		//{
		//	List<WikitivityUploadsAgent.DataObtainedSingleRequestObject> ListForUpload = new List<WikitivityUploadsAgent.DataObtainedSingleRequestObject>();
		//	agentBase.RaiseMessage("Testing this message", 1);

		//	try
		//	{
		//		foreach (var singlePage in singleRequestObjects)
		//		{
		//			var client = new WebClient();

		//			string newTitleURL = singlePage.RequestUrl;

		//			// DEBUG


		//			agentBase.RaiseMessage("Working on this Stuff: " + newTitleURL, 1);

		//			//
		//			try
		//			{

		//			}
		//			catch (Exception e)
		//			{
		//				agentBase.RaiseMessage(e.ToString(), 1);
		//			}
		//			var response2 = client.DownloadString(newTitleURL);

		//			agentBase.RaiseMessage("Here's the stuff!" + response2.ToString(), 1);
		//			var response2Json = JsonConvert.DeserializeObject<WikiCategoryRequest2>(response2);

		//			foreach (var singleArticleWithInfo in response2Json.Query.pages.Values)
		//			{

		//				agentBase.RaiseMessage("Working on this Stuff: " + singleArticleWithInfo.title + "|||" + singleArticleWithInfo.controlNumVal, 1);

		//				ListForUpload.Add(new WikitivityUploadsAgent.DataObtainedSingleRequestObject
		//				{
		//					extractedText = singleArticleWithInfo.extract,
		//					PageTitle = singleArticleWithInfo.title,
		//					ControlNumVal = singlePage.ControlNumVal
		//				});
		//			}
		//		}
		//		return ListForUpload;

		//	}
		//	catch (Exception e)
		//	{
		//		agentBase.RaiseMessage(e.ToString(), 1);
		//		return ListForUpload;

		//	}

		//}

		public static string GetInstanceUrl(IAgentHelper helper, AgentBase aB)
		{
			string instanceUrl = "";
			try
			{
				aB.RaiseMessage("Attempting to get URL...", 1);
				instanceUrl = helper.GetInstanceSettingBundle().GetString("Relativity.Core", "KeplerServicesUri");
				aB.RaiseMessage("Acquired URL: " + instanceUrl, 1);
			}
			catch (Exception e)
			{
				aB.RaiseMessage(e.ToString(), 1);
			}


			return instanceUrl;
		}

		public static string GetWebApiUrl(IAgentHelper helper, AgentBase aB)
		{
			string baseUrl = GetInstanceUrl(helper, aB);
			baseUrl = baseUrl.ToLower();
			baseUrl = baseUrl.Replace(".rest/api/", "webapi/");
			return baseUrl;
		}
		public bool ImportDocument(IAgentHelper helper, int workspaceID, List<WikitivityUploadsAgent.DataObtainedSingleRequestObject> batchedArticleList, AgentBase aB)
		{
			var success = false;

			Int32 workspaceArtifactID = workspaceID;
			Int32 identifyFieldArtifactID = 1003667;    // 'Control Number' Field
			aB.RaiseMessage("Getting webAPI URL -- Inside ImportDocument - WAHelper-" + workspaceID, 1);
			String relativityWebAPIUrl = GetWebApiUrl(helper, aB);
			aB.RaiseMessage(relativityWebAPIUrl, 1);
			String relativityFolderName = "Name of the Destination Folder";
			var url = relativityWebAPIUrl;
			ImportAPI iapi = ImportAPI.CreateByRsaBearerToken(url);

			try
			{
				var importJob = iapi.NewNativeDocumentImportJob();

				importJob.OnMessage += ImportJobOnMessage;

				importJob.OnComplete += ImportJobOnComplete;
				importJob.OnFatalException += ImportJobOnFatalException;
				importJob.Settings.CaseArtifactId = workspaceArtifactID;
				importJob.Settings.ExtractedTextFieldContainsFilePath = false;

				//Leave in case we want to do natives
				//importJob.Settings.DisableNativeLocationValidation = true; Use these two lines for disabling native import & validation.
				//importJob.Settings.DisableNativeValidation = true;
				// importJob.Settings.NativeFilePathSourceFieldName = "Original Folder Path";
				importJob.Settings.NativeFileCopyMode = NativeFileCopyModeEnum.DoNotImportNativeFiles; // NativeFileCopyModeEnum.CopyFiles; NativeFileCopyModeEnum.DoNotImportNativeFiles
				importJob.Settings.OverwriteMode = OverwriteModeEnum.Append;
				importJob.Settings.IdentityFieldId = identifyFieldArtifactID;
				importJob.SourceData.SourceData = GetDocumentDataTable(batchedArticleList, aB).CreateDataReader();

				importJob.Execute();

				success = true;

			}
			catch (Exception ex)
			{
				aB.RaiseMessage(ex.ToString(), 1);
			}
			return success;
		}

		public static DataTable GetDocumentDataTable(List<WikitivityUploadsAgent.DataObtainedSingleRequestObject> batchedArticleEnum, AgentBase aB)
		{
			DataTable table = new DataTable();

			// The document identifer column name must match the field name in the workspace.
			table.Columns.Add("Control Number", typeof(string));
			table.Columns.Add("Extracted Text", typeof(string));
			table.Columns.Add("Article Title", typeof(string));


			foreach (var singleArticle in batchedArticleEnum)
			{
				aB.RaiseMessage(singleArticle.ControlNumVal + " | " + singleArticle.extractedText + " | " + singleArticle.PageTitle, 1); // do we want to have this raised as a message? Can be removed, was for debugging purposes
				table.Rows.Add(singleArticle.ControlNumVal, singleArticle.extractedText, singleArticle.PageTitle);
			}
			return table;
		}
		public static void ImportJobOnMessage(Status status)
		{
			IAPIMessages += status.Message;
		}

		public static void ImportJobOnFatalException(JobReport jobReport)
		{
			IAPIMessages += jobReport.ToString();
		}

		public static void ImportJobOnComplete(JobReport jobReport)
		{
			IAPIMessages += jobReport.ToString();
		}

		//public void UploadBatchSetToRelativity(IRSAPIClient proxy, IEnumerable batchedArticles)
		//{
		//    List<Document> listOfArticlesasDocs = new List<Document>();
		//    foreach (var singleArticle in batchedArticles)
		//    {
		//     Document articleAsDocument = new Document();
		//        articleAsDocument.
		//    }
		//    var uploadArticleResultSet = proxy.Repositories.Document.C
		//}


		public List<int> GetCasesWithWikitivity(IRSAPIClient proxy)
		{
			List<int> CasesWithWikitivity = new List<int>();

			#region Obtain a full list of Case IDs where Wikitivity is installed (CasesWithWikitivity)

			Query<Workspace> getCaseIDs = new Query<Workspace>();
			getCaseIDs.Condition = new WholeNumberCondition("Artifact ID", NumericConditionEnum.IsSet);
			getCaseIDs.Fields = FieldValue.AllFields;
			QueryResultSet<Workspace> CaseQueryResults = new QueryResultSet<Workspace>();
			proxy.APIOptions.WorkspaceID = -1;
			try
			{
				CaseQueryResults = proxy.Repositories.Workspace.Query(getCaseIDs, 0);
			}
			catch (Exception ex)
			{
			}
			if (CaseQueryResults.Success)
			{

				foreach (var caseID in CaseQueryResults.Results)
				{
					var singleWorkSpace = caseID.Artifact.ArtifactID;

					proxy.APIOptions.WorkspaceID = singleWorkSpace;

					Query<ObjectType> checkForWikitivity = new Query<ObjectType>();
					checkForWikitivity.Condition =
						new TextCondition("Name", TextConditionEnum.EqualTo, "Wikitivity Job Progress"); //this feels le jank
					checkForWikitivity.Fields = FieldValue.AllFields;

					QueryResultSet<ObjectType> QueryForWikitivity = new QueryResultSet<ObjectType>();
					try
					{
						QueryForWikitivity = proxy.Repositories.ObjectType.Query(checkForWikitivity, 0);
					}
					catch (Exception e)
					{
					}
					if (QueryForWikitivity.TotalCount > 0)
					{
						CasesWithWikitivity.Add(singleWorkSpace);
					}
				}
			}
			return CasesWithWikitivity;
			#endregion
		}

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
			public string controlNumVal { get; set; }

			//I need to add the control number val in here
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
	}
}