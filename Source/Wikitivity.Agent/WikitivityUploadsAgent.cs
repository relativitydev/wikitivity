using kCura.Agent;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Wikitivity.Agent
{
	[kCura.Agent.CustomAttributes.Name("Wikitivity Upload Agent")]
	[System.Runtime.InteropServices.Guid("fb87a077-88ae-4c7b-aed7-d4abdf692b74")]
	public class WikitivityUploadsAgent : AgentBase
	{

		public static Guid WikitivityRDOGUID = new Guid("C6196733-E2A6-48F4-9443-37990972EBA3");

		WikitivityAgentHelper wAHelper = new WikitivityAgentHelper();

		public class SingleRequestObject
		{
			public string ControlNumVal { get; set; }
			public string RequestUrl { get; set; }
			public int ArtifactID { get; set; }
			public string PageTitle { get; set; }
			public string RequestID { get; set; }
		}

		public class DataObtainedSingleRequestObject
		{

			public string extractedText { get; set; }
			public string PageTitle { get; set; }
			public string ControlNumVal { get; set; }
			// public string RequestID { get; set; }
		}
		/// <summary>
		/// Agent logic goes here
		/// </summary>
		public override void Execute()
		{
			List<int> CasesWithWikitivity = new List<int>();
			/* Steps: 
			 * 0: Obtain all workspaces with the Wikitivity Requests RDO
			 * 1: Query for top 50 entries in the Wikitivity Requests RDO
			 * 2: Store top 50 entries in a list
			 * 3: perform upload actions for those 50 entries, then delete them
			 */

			try
			{
				RaiseMessage("Obtaining Case IDs", 1);

				using (IRSAPIClient proxy = Helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System))
				{
					CasesWithWikitivity = wAHelper.GetCasesWithWikitivity(proxy);
					if (CasesWithWikitivity.Count > 0)
					{
						RaiseMessage("Found cases with wikitivity installed", 1);
						//this means we returned a workspace that has wikitivity installed. I'm of the opinion that we should "cheat" here 
						//and just pretend like theres only ever going to be one workspace with this installed for now.

						// Form the query to get the specific case we're going to work on
						Query<RDO> getInitialJobQuery = new Query<RDO>();
						getInitialJobQuery.ArtifactTypeGuid = WikitivityRDOGUID;
						getInitialJobQuery.Condition = new TextCondition("Request Url", TextConditionEnum.IsSet);
						getInitialJobQuery.Fields = FieldValue.AllFields;
						getInitialJobQuery.ArtifactTypeGuid = WikitivityRDOGUID;

						// Set the workspaceID to this workspace for reuse
						int workspaceID = CasesWithWikitivity[0];
						proxy.APIOptions.WorkspaceID = workspaceID;

						var getInitialJobResultSet = proxy.Repositories.RDO.Query(getInitialJobQuery);

						if (getInitialJobResultSet.Success && getInitialJobResultSet.TotalCount > 0)
						{
							RaiseMessage("Retrieved initial job", 1);
							int InitJobArtiID = getInitialJobResultSet.Results.FirstOrDefault().Artifact.ArtifactID;
							string InitRequestID = getInitialJobResultSet.Results.FirstOrDefault().Artifact.Fields
								.Get("Request ID").ValueAsLongText;
							string InitPageTitle = getInitialJobResultSet.Results.FirstOrDefault().Artifact.Fields
								.Get("Page Title").ValueAsLongText;
							string InitRequestUrl = getInitialJobResultSet.Results.FirstOrDefault().Artifact.Fields
								.Get("Request Url").ValueAsLongText;



							//we have a list of jobs! We now want to pull the request ID from one so we can set a variable for reuse
							SingleRequestObject seedRequest = new SingleRequestObject()
							{
								ArtifactID = InitJobArtiID,
								PageTitle = InitPageTitle,
								RequestID = InitRequestID,
								RequestUrl = InitRequestUrl
							};

							//Now I want to query for all requests with the same ID

							Query<RDO> getArticlesByRequestID = new Query<RDO>();
							getArticlesByRequestID.Condition = new TextCondition("Request ID",
								TextConditionEnum.EqualTo,
								seedRequest.RequestID);
							getArticlesByRequestID.Fields = FieldValue.AllFields;
							getArticlesByRequestID.ArtifactTypeGuid = WikitivityRDOGUID;

							var ArticlesByRequestIDResultSet =
								proxy.Repositories.RDO.Query(
									getArticlesByRequestID); //this resultset will hold the entirety of the requested operations

							RaiseMessage("Retrieved full list of jobs" + ArticlesByRequestIDResultSet.TotalCount, 1);


							// Create a list holding the entirety of the operations for the agent to perform.
							List<SingleRequestObject> completeListByRequestID = new List<SingleRequestObject>();
							foreach (var singleArticleResult in ArticlesByRequestIDResultSet.Results)
							{
								var singleArticle = singleArticleResult.Artifact;
								completeListByRequestID.Add(new SingleRequestObject
								{
									ArtifactID = singleArticle.ArtifactID,
									PageTitle = singleArticle.Fields.Get("Page Title").ValueAsLongText,
									RequestID = singleArticle.Fields.Get("Request ID").ValueAsLongText,
									RequestUrl = singleArticle.Fields.Get("Request Url").ValueAsLongText,
									ControlNumVal = singleArticle.Fields.Get("Name").ValueAsLongText

								});
							}
							RaiseMessage(
								"Creation of master list complete - Total Count of Articles " +
								completeListByRequestID.Count, 1);
							//I now have a full list of articles for a specific request ID stored in CompleteListByRequestID
							//The next step is to batch them for more manageable agent functionality

							List<SingleRequestObject> batchedRequestList = new List<SingleRequestObject>();
							while (completeListByRequestID.Count >= 1)
							{
								try
								{
									//Steps:
									// 1. I need to create my batch set (batchedRequestList)
									batchedRequestList = completeListByRequestID.Take(50).ToList();
									RaiseMessage("Batched list count: " + batchedRequestList.Count, 1);

									// 2. Perform the actual uploads to Relativity for the batchedRequestList 
									var fullyPopulatedArticleList = wAHelper.PopulateArticleData(batchedRequestList, agentBase: this);

									// This kicks off the import job with a list of articles with full text
									var importSuccess = wAHelper.ImportDocument(this.Helper, workspaceID, fullyPopulatedArticleList, this);

									if (importSuccess)
									{
										RaiseMessage("Import successful", 1);
										// AT THIS POINT EVERYTHING IN THE BATCH SHOULD BE UPLOADED TO RELATIVITY
										// 3. Delete the articles in the batch set from the RDO in the workspace

										var deletionSuccess =
											wAHelper.DeleteUploadedRequestsFromRelativity(proxy, workspaceID,
												batchedRequestList);

										if (deletionSuccess)
										{
											// 4. Remove the entries put into the batch set from the master list & clear out the batch set
											if (completeListByRequestID.Any())
											{
												completeListByRequestID.RemoveAll(item => batchedRequestList.Contains(item));
												RaiseMessage("Removed batch set contents from master list", 1);
											}
											batchedRequestList.Clear();
										}
									}

									// 6. Repopulate the batchset and repeat the process til the master list is empty. HANDLED BY THE WHILE LOOP

								}
								catch (Exception ex)
								{
									RaiseError("Agent failed in execution: " + ex.ToString(), ex.ToString());
								}
							}
						}
						else
						{
							RaiseMessage("No jobs currently available", 1);
						}
					}
					else
					{
						RaiseMessage("No jobs currently available", 1);
					}
				}
			}
			catch (Exception ex)
			{
				RaiseError("Agent failed in execution: " + ex.ToString(), ex.ToString());
			}
		}

		/// <summary>
		/// Returns the name of agent
		/// </summary>
		public override string Name
		{
			get
			{
				return "Wikitivity Upload Agent";
			}
		}
	}
}