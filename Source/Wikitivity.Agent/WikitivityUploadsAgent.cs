using kCura.Agent;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
			IObjectManager proxy = Helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System);
			QueryResultSlim getWorkloadResultSlim = new QueryResultSlim();
			QueryResultSlim getInitialJobResultSet = new QueryResultSlim();
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

				Task.Run(async () =>
				{
					CasesWithWikitivity = await wAHelper.GetCasesWithWikitivityOM(proxy);
				}).Wait();
				if (CasesWithWikitivity.Count > 0)
				{
					RaiseMessage("Found cases with wikitivity installed", 1);
			
					foreach (int singleWorkspace in CasesWithWikitivity)
					{
						// Set the workspaceID to this workspace for reuse
						int workspaceID = singleWorkspace;
						RaiseMessage("WorkspaceID: " + workspaceID, 1);


						getInitialJobResultSet = WikitivityAgentHelper.FindAllWikiCases(proxy, workspaceID).Result;


						if (getInitialJobResultSet.TotalCount > 0)
						{
							RaiseMessage("Retrieved initial job", 1);
							int InitJobArtiID = getInitialJobResultSet.Objects.FirstOrDefault().ArtifactID;
							string InitRequestID = getInitialJobResultSet.Objects.FirstOrDefault().Values[1].ToString();
							string InitPageTitle = getInitialJobResultSet.Objects.FirstOrDefault().Values[2].ToString();
							string InitRequestUrl = getInitialJobResultSet.Objects.FirstOrDefault().Values[0].ToString();
							RaiseMessage(InitJobArtiID + " | " + InitRequestID + "  |  " + InitPageTitle + "  |  " + InitRequestUrl, 1);



							//we have a list of jobs! We now want to pull the request ID from one so we can set a variable for reuse
							SingleRequestObject seedRequest = new SingleRequestObject()
							{
								ArtifactID = InitJobArtiID,
								PageTitle = InitPageTitle,
								RequestID = InitRequestID,
								RequestUrl = InitRequestUrl
							};

							getWorkloadResultSlim = WikitivityAgentHelper.FindAllRequestsbyID(proxy, workspaceID, seedRequest.RequestID).Result;

							RaiseMessage("Retrieved full list of jobs: " + getWorkloadResultSlim.TotalCount, 1);

							// Create a list holding the entirety of the operations for the agent to perform.
							List<SingleRequestObject> completeListByRequestID = new List<SingleRequestObject>();
							foreach (var singleArticleResult in getWorkloadResultSlim.Objects)
							{
								completeListByRequestID.Add(new SingleRequestObject
								{


									ArtifactID = singleArticleResult.ArtifactID,
									PageTitle = singleArticleResult.Values[2].ToString(),
									RequestID = singleArticleResult.Values[1].ToString(),
									RequestUrl = singleArticleResult.Values[0].ToString(),
									ControlNumVal = singleArticleResult.Values[3].ToString(),

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
										var deletionSuccess = wAHelper.DeleteUploadedRequestsFromRelativityOM(proxy, workspaceID, batchedRequestList);

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
				}
				else
				{
					RaiseMessage("No jobs currently available", 1);
				}


			}
			catch (Exception ex)
			{
				RaiseError("Agent failed in execution: " + ex.ToString(), ex.ToString() + "\r\n" + ex.InnerException + "\r\n" + ex.Message + "\r\n" + ex.Source);
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