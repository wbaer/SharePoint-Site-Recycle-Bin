//-----------------------------------------------------------------------------
// <copyright file="Utility.cs" company="Microsoft Corporation">
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
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System.Web;
    using System.Xml;
    
    /// <summary>
    /// Utility class that provides methods for event logging and retreiving settings from a source Xml document.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Specifies the path where the application log is written.
        /// </summary>
        private static string path;

        /// <summary>
        /// Specifies the name of the application log.
        /// </summary>
        private static string logFileName = "RecycleBin.log";

        /// <summary>
        /// Specifies the full path of the application log.
        /// </summary>
        private static string logFilePath;

        /// <summary>
        /// Method that provides functions for writing to the application log.
        /// </summary>
        /// <param name="message">The message body written to the application log.</param>
        public static void WriteLog(string message)
        {
            string startTime = string.Empty;
            try
            {
                if (path == null)
                {
                    path = GetConfigValues(Constants.BackupFolder);
                    logFilePath = path + "\\Log";
                }

                if (!Directory.Exists(logFilePath))
                {
                    Directory.CreateDirectory(logFilePath);
                }

                FileIOPermission f = new FileIOPermission(PermissionState.None);
                f.AddPathList(FileIOPermissionAccess.Read, logFilePath);
                f.AddPathList(FileIOPermissionAccess.Write, logFilePath);
                f.AddPathList(FileIOPermissionAccess.PathDiscovery, logFilePath);
                f.PermitOnly();
                f.Demand();
                lock ("LogLock")
                {
                    FileStream fileStream = new FileStream(logFilePath + "\\" + logFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    StreamWriter streamWriter = new StreamWriter(fileStream);
                    streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                    startTime = System.DateTime.Now.ToString("(yyyy:MM:dd hh:mm:ss.", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                    startTime += System.DateTime.Now.Millisecond.ToString(CultureInfo.InvariantCulture) + "):";
                    streamWriter.Write(startTime + " " + message + "\r\n");
                    streamWriter.Flush();
                    streamWriter.Close();
                    fileStream.Dispose();                   
                }
            }
            catch (SecurityException se)
            {
                throw new SecurityException(string.Format(CultureInfo.InvariantCulture, Constants.SecurityException, se.FailedAssemblyInfo.ToString(), se.RefusedSet.ToString(), se));
            }
        }

        /// <summary>
        /// Initializes a new instance of the GetConfigValues class.
        /// </summary>
        /// <param name="keyName">Specifies the XmlNode to retrieve in the source Xml.</param>
        /// <returns>Returns the concatenated value the node and its child nodes.</returns>
        public static string GetConfigValues(string keyName)
        {
            string xmlValue = string.Empty;
            try
            {
                string configPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles) + @"\Microsoft Shared\web server extensions\14\TEMPLATE\FEATURES\SiteRecycleBinDeleteFeature\Configuration.xml";
                XmlDocument xmlConfig = new XmlDocument();
                xmlConfig.Load(configPath);
                xmlValue = xmlConfig.SelectSingleNode(keyName).InnerText;
            }
            catch (FileNotFoundException exception)
            {
                throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, Constants.ConfigurationException, exception.ToString()));
            }

            return xmlValue;
        }
    }
}