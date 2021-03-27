//-----------------------------------------------------------------------------
// <copyright file="Constants.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//      Licensed under the Microsoft Extensibility Code 
//      Sharing License (see license included in distribution)
// </copyright>
// <author>Bill Baer</author>
//-----------------------------------------------------------------------------


namespace Microsoft.SharePoint.Site.RecycleBin
{
    /// <summary>
    /// Initializes a new instance of the Constants class.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Specifies the event message body of security exception.
        /// </summary>
        public const string SecurityException = "A security exception occurred in {0}. The refused permission set is {1}. Details: {2} ";

        /// <summary>
        /// Specifies the event message body of a configuration exception.
        /// </summary>
        public const string ConfigurationException = "Error in reading configuration from Configuration.xml.  Details: {0}.";

        /// <summary>
        /// Specifies the event message body of when a SiteDeleting method is invoked.
        /// </summary>
        public const string EnteringSiteDelete = "Entering SPSite delete method on {0}.\n\tRequested by user:  {1} ";

        /// <summary>
        /// Specifies the event message body of when a SiteDeleting method is completed.
        /// </summary>
        public const string ExitingSiteDelete = "Backup and delete of SPSite {0} completed successfully.";

        /// <summary>
        /// Specifies the event message body of when a WebDeleting method is invoked.
        /// </summary>
        public const string EnteringWebDelete = "Entering SPWeb delete method on {0}.\n\tRequested by user: {1}";

        /// <summary>
        /// Specfies the event message body og when a WebDeleting method is completed.
        /// </summary>
        public const string ExitingWebDelete = "Backup and delete of SPWeb {0} completed successfully.";

        /// <summary>
        /// Specifies the event message body of an unhandled exception.
        /// </summary>
        public const string GeneralException = "The backup operation terminated abnormally due to {0}";

        /// <summary>
        /// Specifies the event message body when an attempt is made to delete a root site collection.
        /// </summary>
        public const string RootSiteException = "Root site collection backup prohibited {0}";

        /// <summary>
        /// Specifies the backup folder local variable.
        /// </summary>
        public const string BackupFolder = "//backupFolder";
    }
}