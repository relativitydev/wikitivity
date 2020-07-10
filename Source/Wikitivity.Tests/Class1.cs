using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Vendor.Castle.Core.Internal;
using NUnit.Framework;
using Relativity.API;
using Relativity.Kepler.Logging;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Test.Helpers;
using Relativity.Test.Helpers.ServiceFactory.Extentions;
using ILog = Relativity.Kepler.Logging.ILog;
using NullLogger = Relativity.Services.Pipeline.Logger.NullLogger;

namespace Wikitivity.Tests
{

    public class Class1
    {
	    private IHelper _helper = Relativity.Test.Helpers.TestHelper.System();
	    private ILog _logger;
	    //;.GetLoggerFactory().GetLogger().ForContext<CreateTestingWorkspace>();//();.Test.Helpers.Logging.TestLogFactory 
	   // private IRSAPIClient proxy;
	    private TestHelper Helper;

	    [SetUp]
	    public void SetUp()
	    {
		    _logger = new NullLogger().ForContext<string>();
		    Helper = new TestHelper("Relativity.Admin@relativity.com", "Test1234!");
		   // proxy = Helper.GetServicesManager().GetProxy<IRSAPIClient>("Test@test.co", "Test1234!");
	    }

	    [Test]
	    public void TestCreation()
	    {
		    try
		    {
			    string instanceUrl = Helper.GetInstanceSettingBundle().GetString("Relativity.Wikivity", "WikitivityWebApi");
Console.WriteLine(instanceUrl);Debug.WriteLine(instanceUrl);
			    Assert.That(!instanceUrl.IsNullOrEmpty());
			}
		    catch (Exception e)
		    {
			    Console.WriteLine(e);
			    
		    }
		   


	    }

		[Test]
		public void ReadSetting()
		{
			try
			{
				string instanceUrl = Helper.GetInstanceSettingBundle().GetString("appSettings", "Password");
				Console.WriteLine(instanceUrl); Debug.WriteLine(instanceUrl);
				Assert.That(!instanceUrl.IsNullOrEmpty());
			}
			catch (Exception e)
			{
				Console.WriteLine(e);

			}



		}

		public static Guid WikitivityRDOGuid = new Guid("C6196733-E2A6-48F4-9443-37990972EBA3");


		[Test]

	    public List<WikiRequest> TestVoid()
		{
		//	var proxy = Helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser);
			Guid g = new Guid();
			var test = new WikiRequest {PageTitle = "1234567", Name = "Test", RequestIDGuid = g.ToString(), Url = "test.com"};
			var listTst = new List<WikiRequest>() {test};


			return listTst;







		}

	    public class WikiRequest
	    {
		    public string PageTitle { get; set; }
			public string Url { get; set; }
		   
		    public string Name { get; set; }
		    public string RequestIDGuid { get; set; }

	    }

		[Test]
		public async Task WriteToTableOM()
		{
	var massCreateRequest = new MassCreateRequest();
			//Now I need to construct the individual values to be added in the masscreate call?
			var proxy = Helper.GetServicesManager().GetProxy<IObjectManager>("Relativity.admin@relativity.com", "Test1234!");
			var workspaceID = 1017898;
			//TODO: Refactor to OM
			//	proxy.APIOptions.WorkspaceID = workspaceID;
			Guid g = new Guid();
			var test = new WikiRequest { PageTitle = "1234567", Name = "Test", RequestIDGuid = g.ToString(), Url = "test.com" };
			var test2 = new WikiRequest { PageTitle = "7654321", Name = "Te2323232323st", RequestIDGuid = g.ToString(), Url = "test.com2" };

List<WikiRequest> wikReqList = new List<WikiRequest>() {test, test2};
			//Construct the fields
			FieldRef RequestID = new FieldRef();
			//field1.Guid = DocumentCountFieldGuid;
			RequestID.Name = "Request ID";
			FieldRef RequestUrl = new FieldRef();
			//field2.Guid = UserCountFieldGuid;
			RequestUrl.Name = "RequestUrl";
			FieldRef PageTitle = new FieldRef();
			PageTitle.Name = "Page Title";
			//field3.ArtifactID = 1084090;
			FieldRef RequestName = new FieldRef();
			RequestName.Name = "Name";
			List<FieldRef> fieldList = new List<FieldRef>() { RequestID, RequestUrl, RequestName, PageTitle };

			// Individual operations now?
//IReadOnlyList<IReadOnlyList<object>> FieldValues = new List<IReadOnlyList<object>>() { singleRequestList };



			//Construct the values THESE MUST BE IN ORDER?


			//List<object> FieldVals = new List<object>();

			//	List<FieldRef> test = new List<FieldRef>();

		
			massCreateRequest.ObjectType = new ObjectTypeRef() { Guid = WikitivityRDOGuid };
			massCreateRequest.Fields = fieldList;
var listofRequests = new List<List<object>> {};
			foreach (var singleReq in wikReqList)
			{
				listofRequests.Add(new List<object>()
				{
					singleReq.RequestIDGuid,
					singleReq.Url,
					singleReq.Name,
					singleReq.PageTitle
				});
			}
			massCreateRequest.ValueLists = listofRequests;/*new List<List<object>> {new List<object>() {test.RequestIDGuid, test.Url, test.Name, test.PageTitle}};
*/


			var variable = new List<WikiRequest>() { test };

			List<object> singleRequestList = new List<object>();

			
			//foreach (WikiRequest item in variable)
			//{

			//	string requestUrl = $"https://en.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exlimit=max&explaintext&titles={item.PageTitle}&redirects=";

			//	//if (item.prefixText == String.Empty)
			//	//{
			//	//	item.prefixText = "WIKI";
			//	//}
			//	//string docID = item.prefixText + item.count.ToString("D7");
			//	singleRequestList.Add(item);

			////	singleRequestList.Add(new List<object>() { item.RequestIDGuid, requestUrl, item.Name, item.PageTitle });
			//	FieldVals.Add(singleRequestList);
			//}
			

//FieldValues;


			//List<Guid> guidList = new List<Guid>();
			//guidList.Add(WikitivityRDOGuid);
			//wikitivityRequestRDO.ArtifactTypeGuids = guidList;
			//test.Add(new FieldRef() { Name = "Request ID", Value = requestID });
			//test.Add(new FieldValue() { Name = "Request Url", Value = requestUrl });

			//if (prefix == String.Empty)
			//{ 
			//	prefix = "WIKI";
			//}
			////string docID = prefix + count.ToString("D7");

			//wikitivityRequestRDO.Fields.Add(new FieldValue() { Name = "Name", Value = docID });
			//wikitivityRequestRDO.Fields.Add(new FieldValue() { Name = "Page Title", Value = pageTitle });
			//massCreateRequest.Fields = fieldList;
			try
			{
				await proxy.CreateAsync(workspaceID, massCreateRequest);
			}
			catch (Exception ex)
			{

			}
		}

	}
}
