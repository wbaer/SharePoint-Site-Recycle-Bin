//-----------------------------------------------------------------------------
// <copyright file="DeleteFeatureCallout.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//      Licensed under the Microsoft Extensibility Code 
//      Sharing License (see license included in distribution)
// </copyright>
// <author>Bill Baer</author>
//-----------------------------------------------------------------------------


namespace Microsoft.SharePoint.Site.RecycleBin
{
    using System;
    using System.Globalization;
    using System.Security.Permissions;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.Security;

    /// <summary>
    /// Initializes a new instance of the DeleteFeatureCallout class.
    /// </summary>
    [SharePointPermission(SecurityAction.LinkDemand,
        ObjectModel = true)]
    [SharePointPermission(SecurityAction.InheritanceDemand,
        ObjectModel = true)]
    [CLSCompliant(false)]

    public class DeleteFeatureCallout : SPFeatureReceiver
    {
        /// <summary>
        /// Occurs after a Feature is activated.
        /// </summary>
        /// <param name="properties">Represents the properties of a Feature activation event.</param>
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            if (properties.Feature.Parent.GetType().ToString() == "Microsoft.SharePoint.Administration.SPWebApplication")
            {
                object currentApp = properties.Feature.Parent;
                SPWebApplication webApp = (SPWebApplication)currentApp;
                foreach (SPSite siteColl in webApp.Sites)
                {
                    foreach (SPWeb web in siteColl.AllWebs)
                    {
                        RegisterEventReceiver(web);
                        web.Dispose();
                    }

                    siteColl.Dispose();
                }
            }
            else
            {
                object currentWeb = properties.Feature.Parent;
                SPWeb web = (SPWeb)currentWeb;
                RegisterEventReceiver(web);
                web.Dispose();
            }
        }

        /// <summary>
        /// Occurs when a Feature is deactivated.
        /// </summary>
        /// <param name="properties">Represents the properties of a Feature deactivation event. </param>
        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            object currentApp = properties.Feature.Parent;
            SPWebApplication webApp = (SPWebApplication)currentApp;
            foreach (SPSite siteColl in webApp.Sites)
            {
                foreach (SPWeb web in siteColl.AllWebs)
                {
                    UnRegisterEventReceiver(web);
                    web.Dispose();
                }

                siteColl.Dispose();
            }
        }

        /// <summary>
        /// Occurs after a Feature is installed.
        /// </summary>
        /// <param name="properties">Represents the properties of a Feature installation event. </param>
        public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        {
            return;
        }

        /// <summary>
        /// Occures when a Feature is uninstalled.
        /// </summary>
        /// <param name="properties">Represents the properties of a Feature uninstallation event.</param>
        public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        {
            return;
        }

        /// <summary>
        /// Registers the Event Receiver.
        /// </summary>
        /// <param name="web">Represents the scope of where the Event Receiver is registered.</param>
        private static void RegisterEventReceiver(SPWeb web)
        {
            string assemblyName = Utility.GetConfigValues("//assemblyName");
            string sequenceNumber = Utility.GetConfigValues("//sequenceNumber");
            SPEventReceiverDefinition newReceiver = web.EventReceivers.Add();
            newReceiver.Class = "Microsoft.SharePoint.Site.RecycleBin.DeleteEventReceiver";
            newReceiver.Assembly = assemblyName;
            newReceiver.SequenceNumber = Convert.ToInt32(sequenceNumber, CultureInfo.InvariantCulture);
            if (web.IsRootWeb == true)
            {
                newReceiver.Type = SPEventReceiverType.SiteDeleting;
            }
            else
            {
                newReceiver.Type = SPEventReceiverType.WebDeleting;
            }

            newReceiver.Update();
            web.Dispose();
        }

        /// <summary>
        /// Unregisters the Event Receiver.
        /// </summary>
        /// <param name="web">Represents the scope of where the Event Receiver is unregistered.</param>
        private static void UnRegisterEventReceiver(SPWeb web)
        {
            string sequenceNumber = Utility.GetConfigValues("//sequenceNumber");
            foreach (SPEventReceiverDefinition eventReceiver in web.EventReceivers)
            {
                if (eventReceiver.SequenceNumber == Convert.ToInt32(sequenceNumber, CultureInfo.InvariantCulture))
                {
                    SPEventReceiverDefinition deleteReceiver = web.EventReceivers[eventReceiver.Id];
                    deleteReceiver.Delete();
                    web.Dispose();
                }
            }
        }
    }
}