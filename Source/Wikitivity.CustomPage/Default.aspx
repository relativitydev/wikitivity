<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Wikitivity.CustomPage.Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
		<title>Wikitivity</title>
		<script src="Content/jquery.js" type="text/javascript"></script>
		<script src="Content/lab.js" type="text/javascript"></script>
		<link href="Content/buttermilk.css" rel="stylesheet" />
	 
</head>
<body class="login-body" style="overflow: auto; width: 100%;">
<form id="form1" runat="server">
		<div id="_mainBody" class="login-container" style="text-align: center; margin: 100px;">
				<div style="margin: 50px;">
						<img src="Wikitivity_2a.png" alt="Wikitivity_logo" style="width: 375px; height: 175px;"/>
						<p>Use the field below to add the names of any Wikipedia categories you’d like to import articles from.  <br />Use a semicolon ( ; ) as the delimiter between multiple category names.  When finished, click the <b>Preview Request</b> button.</p>
				 
						<p><b>Please note:</b> Category names are case sensitive. For example, <i>Star Wars</i> will return results, but <i>star wars</i> or <i>Star wars</i> will not. <br/>
								 <a href="#" onClick="MyWindow=window.open('https://en.wikipedia.org/wiki/Portal:Contents/Categories','MyWindow',width=600,height=300); return false;">Click here to explore the categories within Wikipedia.</a></p>
						<p/>
							 <asp:TextBox ID="pageRequestText" runat="server" CssClass="auto-style1" Height="80px" Width="629px"></asp:TextBox> 
					<br />
					<br />
					<span>Prefix:  <asp:TextBox ID="prefixText" runat="server" CssClass="auto-style1" Height="20px" Width="566px"></asp:TextBox></span>
					<br />
						<br/>
						<b>Note:</b> To ensure stable system performance, this application will return a maximum of 200 categories per request.<br /If importing a large number of categories, consider separating them into multiple requests.
				 <br />
								<br />
						<br/>
						<asp:Label ID="countLabel" runat="server" ForeColor="#003300" Visible="False"></asp:Label>
						 <asp:ListBox ID="previewPane" runat="server"    Height="275px" Width="472px" Visible="False" Rows="30" OnSelectedIndexChanged="previewPane_SelectedIndexChanged"></asp:ListBox>
					 <div class="login-container" style="text-align: center; margin: 100px;">
							
						 
							 <p>
		<asp:ScriptManager runat="server">
</asp:ScriptManager>
<asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
<ProgressTemplate>
		<div class="modal">
				<div class="center">
						<img alt="" src="Wikitivity_Loading.gif" />
						<p><b>Submitting results, please wait...</b></p>
				</div>
		</div>
</ProgressTemplate>
</asp:UpdateProgress>
										<asp:Button ID="previewButton" runat="server" OnClick="previewButton_OnClick" OnClientClick="previewButton_OnClick" Text="Preview Request" />
<asp:UpdatePanel ID="UpdatePanel1" runat="server">
<ContentTemplate>
		<div align="center">
			 
							 <asp:Button ID="submitJobButton" runat="server" OnClick="submitJobButton_Click" Text="Submit" Width="143px" Visible="False" />
							 <asp:Label ID="requestLabel" runat="server" ></asp:Label>
		</div>
</ContentTemplate>
</asp:UpdatePanel>
			 
							 </p>
				
		
					 </div>
				</div>  
		</div>
		
</form>
</body>

</html>
