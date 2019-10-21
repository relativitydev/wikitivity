#Wikitivity

**Overview**

This application allows you to pull Wikipedia articles into Relativity. It is intended to help users construct demos and tests using Wikipedia articles as the data. While is application is freely available, we strongly recommend that only experienced Relativity users use it. Additionally you will need to have sufficient permissions to install applications and add agents to be able to set this application up in your environment.

While this is an open source project on the Relativity GitHub account, support is only available through the Relativity developer community. You are welcome to use the code and solution as you see fit within the confines of the license it is released under. However, if you are looking for support or modifications to the solution, we suggest reaching out to a [Relativity Development Partner](https://www.relativity.com/ediscovery-software/app-hub/).

**Accessing the Application**

1. Go to [https://relativitydev.github.io](https://relativitydev.github.io/)
2. Select the project named Wikitivity
3. Click the folder named &quot;Deployment.&quot; This will allow you to download the RAP file.

**Installing the Application**

1. Open the workspace you would like to install the app in and navigate to the **Relativity Applications** tab.
2. Click **New Relativity Application**.
3. Select **Import from**
4. Upload the Wikitivity RAP file you downloaded previously in the **File** field.
5. Click **Import**. The app should take a minute to install.



**Adding the Agent**

1. Return to Admin mode by clicking **the Relativity logo at the top-left of the screen**.
2. Navigate to the **Agents** tab.
3. Click **New Agent**.
4. In the **Agent Type** field, click the **ellipsis** button.
5. Select the agent type named **Wikitivity Upload Agent** and then click **Set**.
6. Keep **Number of Agents** set at the default value of 1
7. In the **Agent Server** field, click the **ellipsis** button and select the agent server to add this agent to. Depending on your environment setup you may have one option here or several. Speak to your system administrators if you aren&#39;t sure which agent server to choose.
8. Set the **Run Interval** to 5.
9. In the **Logging Level** field, select **Log critical errors only**.
10. Click **Save** at the top of the screen. Wikitivity is now ready for use!

**Using the Application**

The Wikitivity application contains 3 tabs: Wikitivty, Job Progress, and Job History.

- **Wikitivity:** This tab is where you will choose and submit which categories of articles that you want to pull into your workspace
- **Job Progress:** This tab lets you monitor the progress of any submitted jobs. If this tab is empty, that means there are no pending articles waiting to be imported.
- **Job History:** This tab shows you all jobs you&#39;ve submitted in this workspace.

Let&#39;s step through an example job to see how to use the app.

**Example Job**

1. Navigate to the **Wikitivity | Wikitivity**
2. In the large box in the middle of the screen, enter the names of the category of article you would like to pull from Wikipedia. You can use the link on the tab itself or navigate to [https://en.wikipedia.org/wiki/Portal:Contents/Categories](https://en.wikipedia.org/wiki/Portal:Contents/Categories) to explore the available categories on Wikipedia. Let&#39;s say that we decide to grab the category &quot;Star Wars.&quot;
3. On the Wikitivity tab, **enter Star Wars into the box**. Note that Wikipedia is case sensitive and not always consistent with capitalization. So make sure you enter the name of the category _exactly_ as it appears in Wikipedia.
  1. NOTE: You can enter multiple categories at once if you would like, just separate each with a semicolon. So if you wanted to pull all Star Wars and Baseball articles, you would enter Star Wars;Baseball.
4. In the Prefix field, we can configure the Control Number that these documents will receive. All documents in this job will have a control number containing this prefix, and then a number padded to 7 digits. If you enter nothing in this field, the prefix will default to &quot;WIKI\_&quot;. Enter **SW\_** into the prefix field.
  1.  NOTE: In the first version of this application there is no logic for identifying control numbers already in use and picking the next available number. Thus we recommend you use different prefixes for each job. If you use the same prefix for 2 jobs, the documents in the 2nd job will end up being skipped since a document with that control number will already exist.
5. Click **Preview Request**. This will display a list of the articles that will be pulled into Relativity by submitting this request. In our case, we will have 379 documents created.
  1. NOTE: If no articles are displayed in the list, you know that the category you submitted either doesn&#39;t exist, or was misspelled.
6. Click **Submit**. The job will be submitted and the agent will begin pulling the documents into Relativity.
7. To monitor the progress of the job, navigate to the **Job Progress** tab. Each entry on this tab corresponds to one of the articles that will be pulled into Relativity. If you refresh the page several times, you will see the **Total Items** value in the bottom-right go up as the system adds the articles from the job, and then go down as the agent pulls the articles into Relativity. Once the tab is empty, the job has completed.
8. Navigate to the **Document&#39;s** You&#39;ll see our 379 documents, each with a control number that begins with our configured prefix. To help identify which document is which, the Wikitivty app will auto-create a field called **Article Title** and will put the name of each article into that field. Add this field to your Document List view.
9. At this point you can use these documents in any workflows you would like!