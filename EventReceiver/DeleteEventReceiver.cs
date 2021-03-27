//-----------------------------------------------------------------------------
// <copyright file="DeleteEventReceiver.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//      Licensed under the Microsoft Extensibility Code 
//      Sharing License (see license included in distribution)
// </copyright>
// <author>Bill Baer</author>
//-----------------------------------------------------------------------------

namespace Microsoft.SharePoint.Site.RecycleBin
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.Deployment;
    using Microsoft.SharePoint.Diagnostics;
    using Microsoft.SharePoint.Security;

    /// <summary>
    /// Initializes a new instance of the DeleteEventReceiver class.
    /// </summary>
    [SharePointPermission(SecurityAction.LinkDemand,
    ObjectModel = true)]
    [SharePointPermission(SecurityAction.InheritanceDemand,
        ObjectModel = true)]
    [CLSCompliant(false)]

    public class DeleteEventReceiver : SPWebEventReceiver
    {
        /// <summary>
        /// Occurs when a site collection is being deleted.
        /// </summary>
        /// <param name="properties">Represents properties of the event handler</param>
        public override void SiteDeleting(SPWebEventProperties properties)
        {
            if (!EventLog.SourceExists("SharePoint Site Recycle Bin"))
            {
                EventLog.CreateEventSource("SharePoint Site Recycle Bin", "Application");
            }

            SPWebApplication webApp = null;
            string backUpFile = string.Empty;
            string backUpFolder = string.Empty;
            Utility.WriteLog(string.Format(CultureInfo.InvariantCulture, Constants.EnteringSiteDelete, properties.FullUrl, properties.UserLoginName));
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    backUpFile = properties.ServerRelativeUrl.Substring(properties.ServerRelativeUrl.IndexOf("/", 1, StringComparison.Ordinal) + 1);
                    backUpFolder = Utility.GetConfigValues(Constants.BackupFolder);
                    if (!Directory.Exists(backUpFolder + "\\Sites"))
                    {
                        Directory.CreateDirectory(backUpFolder + "\\Sites");
                    }

                    Uri uri = new Uri(properties.FullUrl);
                    webApp = SPWebApplication.Lookup(uri);
                    SPSiteCollection siteColl = webApp.Sites;

                    if (System.IO.File.Exists(backUpFolder + "\\Sites\\" + backUpFile + ".bak"))
                    {
                        backUpFile += DateTime.Now.ToString("(yyyy-MM-dd-hh-mm-ss-", System.Globalization.DateTimeFormatInfo.InvariantInfo) + DateTime.Now.Millisecond.ToString(CultureInfo.InvariantCulture) + ")";
                    }

                    siteColl.Backup(properties.FullUrl, backUpFolder + "\\Sites\\" + backUpFile + ".bak", true);
                });
                Utility.WriteLog(string.Format(CultureInfo.InvariantCulture, Constants.ExitingSiteDelete, properties.FullUrl, properties.UserLoginName));
            }
            catch (SPException ex)
            {
                EventLog.WriteEntry("SharePoint Site Recycle Bin", ex.Message.ToString(), EventLogEntryType.Error, 1000);
                Utility.WriteLog("Exception Occured " + ex.ToString());
                properties.Cancel = true;
                properties.ErrorMessage = string.Format(CultureInfo.InvariantCulture, Constants.GeneralException, ex.ToString());
            }
        }

        /// <summary>
        /// Synchronous before event that occurs before an existing Web site is completely deleted.
        /// </summary>
        /// <param name="properties">Represents properties of the event handler.</param>
        public override void WebDeleting(SPWebEventProperties properties)
        {
            if (!EventLog.SourceExists("SharePoint Site Recycle Bin"))
            {
                EventLog.CreateEventSource("SharePoint Site Recycle Bin", "Application");
            }

            string backUpFile = string.Empty;
            string subFolder = string.Empty;
            string backUpFolder = string.Empty;
            SPWebApplication webApp = null;
            Utility.WriteLog(string.Format(CultureInfo.InvariantCulture, Constants.EnteringWebDelete, properties.FullUrl, properties.UserLoginName));
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    SPWeb web = new SPSite(properties.FullUrl).OpenWeb();

                    if (!web.IsRootWeb && web.Webs.Count > 0)
                    {
                        return;
                    }

                    backUpFile = properties.Web.Name;
                    backUpFolder = Utility.GetConfigValues(Constants.BackupFolder);
                    Uri uri = new Uri(properties.FullUrl);
                    webApp = SPWebApplication.Lookup(uri);
                    string relativeURL = properties.ServerRelativeUrl;
                    subFolder = relativeURL.Substring(relativeURL.IndexOf("/", 0, StringComparison.Ordinal), relativeURL.LastIndexOf("/", StringComparison.Ordinal) - relativeURL.IndexOf("/", 0, StringComparison.Ordinal));
                    string backUpFileLocation = backUpFolder + "\\" + subFolder.Replace(@"/", @"\");
                    if (!Directory.Exists(backUpFileLocation))
                    {
                        Directory.CreateDirectory(backUpFileLocation);
                    }

                    if (String.IsNullOrEmpty(backUpFolder) && System.IO.File.Exists(backUpFolder + subFolder.Replace(@"/", @"\") + "\\" + backUpFile + ".bak"))
                    {
                        backUpFile += DateTime.Now.ToString("(yyyy-MM-dd-hh-mm-ss-", System.Globalization.DateTimeFormatInfo.InvariantInfo) + DateTime.Now.Millisecond.ToString(CultureInfo.InvariantCulture) + ")";
                    }
                    
                    SPExportSettings exportSettings = new SPExportSettings();
                    exportSettings.ExportMethod = SPExportMethodType.ExportAll;
                    exportSettings.BaseFileName = backUpFile + ".bak";
                    exportSettings.FileLocation = backUpFileLocation;
                    exportSettings.ExcludeDependencies = false;
                    exportSettings.IncludeSecurity = SPIncludeSecurity.All;
                    exportSettings.SiteUrl = properties.Web.Url;
                    SPExportObject exportObject = new SPExportObject();
                    exportObject.Type = SPDeploymentObjectType.Web;
                    exportObject.Url = properties.Web.Url;
                    exportObject.ExcludeChildren = false;
                    exportSettings.ExportObjects.Add(exportObject);
                    SPExport export = new SPExport(exportSettings);
                    export.Run();
                    web.Dispose();
                });
                Utility.WriteLog(string.Format(CultureInfo.InvariantCulture, Constants.ExitingWebDelete, properties.FullUrl, properties.UserLoginName));
            }
            catch (SPException exception)
            {
                EventLog.WriteEntry("SharePoint Site Recycle Bin", string.Format(CultureInfo.InvariantCulture, "An unhandled exception has occurred in the SPWeb delete method {0}", exception.ToString()), EventLogEntryType.Error, 1000);
                Utility.WriteLog(string.Format(CultureInfo.InvariantCulture, "An unhandled exception has occurred in the SPWeb delete method {0}", exception.ToString()));
                properties.Cancel = true;
                properties.ErrorMessage = string.Format(CultureInfo.InvariantCulture, Constants.GeneralException, exception.ToString());
            }
        }
    }
}