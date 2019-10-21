
function getServiceUrl(labNumber) {
var hostname = window.location.hostname;
var host = "https://festportal.relativity.com/service/" + hostname + "/lab/";
return host + labNumber;
}

function hidePageElements() {
	$("#_mainBody").css("display", "none");
	$("#_processingDiv").css("display", "");
}

function startLab(serviceUrl) {
	if (serviceUrl === "") {
		return false;
	} else {
		var startUrl = appendStartedToUrl(serviceUrl);
		sendToService(startUrl);
		return true;
	}
}

function startLab(serviceUrl) {
	if (serviceUrl === "") {
		return false;
	} else {
		var startUrl = appendStartedToUrl(serviceUrl);
		sendToService(startUrl, function () { });
		return true;
	}
}

function completeLab(serviceUrl) {
	hidePageElements();

	if (serviceUrl === "") {
		return false;
	} else {
		var completeUrl = appendCompletedToUrl(serviceUrl);
		sendToService(completeUrl, redirectToWelcomePage);
		return true;
	}
}

function verifyCode(secretPhrase) {
    if (secretPhrase === "") {
	    alert("Please provide a code prior to clicking the Verify button.");
		return false;
	} else {
		if (secretPhrase.toUpperCase() === "ELIZA") {
            alert("Congratulations!!!  You entered the correct code.  Please notify a lab attendant after completing the survey.");
			document.getElementById('secretCode').value = "";
			return true;
		} else {
            alert("You have entered an incorrect code.  Please try again.");
			document.getElementById('secretCode').value = "";
			return false;
		}
	}
}

function sendToService(serviceUrl, redirectFunc) {
	$.ajax({
		type: 'GET',
		url: serviceUrl,
		async: false,
		dataType: 'jsonp',
		timeout: 10000,
		success: function() { redirectFunc(); },
		error: function (data) { alertUser(data, alert); }
	});
}

function appendCompletedToUrl(serviceUrl) {
	return serviceUrl + "/labaction/Completed";
}

function appendStartedToUrl(serviceUrl) {
	return serviceUrl + "/labaction/Started";
}

function alertUser(data, alert) {
	if (data.statusText === "timeout") {
		alert("Request timed out.  Please contact your lab administrator");
	}
	if (data.status === "500") {
		alert("Failed to submit.  Please contact your lab administrator");
	}
	if (data.status === "404") {
		alert("Failed to find resource.  Please contact your lab administrator");
	}
}

function redirectToWelcomePage() {
	var redirectUrl = getRedirectUrl();
	window.top.location = redirectUrl;
}

function getRedirectUrl() {
	var host = window.top.location.hostname;
	return window.top.location.protocol + "//" + host + "/Relativity/default.aspx";
}

function getSurveyUrl(labNumber) {

	if (labNumber < 82 || labNumber > 87) {
		return "";
	}

	var surveyIdentifier = {
        82: "FMYNYSB", // The Power of Active Learning
        83: "FXPDL9Q", // Accelerating Investigation with Clustering
		84: "FNJMM3G", // Wonderful World of Searching
        85: "FZR9HLM", // Curing Common Review Pain Points
		86: "FRT6KDC", // Case Dynamics: Hamilton vs. Burr
        87: "FZPYMSC"  // Spotlight on Transcripts
	}

	return "https://www.surveymonkey.com/r/" + surveyIdentifier[labNumber];
}

function openSurvey(url) {
	if (url === "") {
		return false;
	} else {
		window.open(url, "SurveyWindow", "width=800, height=400");
	}
}