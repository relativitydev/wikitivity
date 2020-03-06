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
using Relativity.Test.Helpers;
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

	}
}
