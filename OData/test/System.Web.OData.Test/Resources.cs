// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Web.Http;

namespace System.Web.OData
{
    internal static class Resources
    {
        public static string ArrayOfBoolean
        {
            get
            {
                return GetString("ArrayOfBoolean.json");
            }
        }

        public static string ArrayOfInt32
        {
            get
            {
                return GetString("ArrayOfInt32.json");
            }
        }

        public static string CollectionOfPerson
        {
            get
            {
                return GetString("CollectionOfPerson.json");
            }
        }

        public static string EmployeeEntry
        {
            get
            {
                return GetString("EmployeeEntry.json");
            }
        }

        public static string EnumComplexType
        {
            get
            {
                return GetString("EnumComplexType.json");
            }
        }

        public static string FeedOfEmployee
        {
            get
            {
                return GetString("FeedOfEmployee.json");
            }
        }

        public static string ListOfString
        {
            get
            {
                return GetString("ListOfString.json");
            }
        }

        public static string MainEntryFeedInJsonFullMetadata
        {
            get
            {
                return GetString("MainEntryFeedInJsonFullMetadata.json");
            }
        }

        public static string MainEntryFeedInJsonNoMetadata
        {
            get
            {
                return GetString("MainEntryFeedInJsonNoMetadata.json");
            }
        }

        public static string PersonComplexType
        {
            get
            {
                return GetString("PersonComplexType.json");
            }
        }

        public static string PersonEntryInJsonLightNoMetadata
        {
            get
            {
                return GetString("PersonEntryInJsonLightNoMetadata.json");
            }
        }

        public static string PersonEntryInJsonLightMinimalMetadata
        {
            get
            {
                return GetString("PersonEntryInJsonLightMinimalMetadata.json");
            }
        }
        
        public static string PersonEntryInJsonLightFullMetadata
        {
            get
            {
                return GetString("PersonEntryInJsonLightFullMetadata.json");
            }
        }

        public static string PersonEntryInPlainOldJson
        {
            get
            {
                return GetString("PersonEntryInPlainOldJson.json");
            }
        }

        public static string PersonEntryInJsonLight
        {
            get
            {
                return GetString("PersonEntryInJsonLight.json");
            }
        }

        public static string ProductRequestEntry
        {
            get
            {
                return GetString("ProductRequestEntry.json");
            }
        }

        public static string ProductsCsdl
        {
            get
            {
                return GetString("ProductsCsdl.xml");
            }
        }

        public static string SupplierPatch
        {
            get
            {
                return GetString("SupplierPatch.json");
            }
        }

        public static string SupplierRequestEntry
        {
            get
            {
                return GetString("SupplierRequestEntry.json");
            }
        }

        public static string WorkItemEntry
        {
            get
            {
                return GetString("WorkItemEntry.json");
            }
        }

        public static string SingletonSelectAndExpand
        {
            get
            {
                return GetString("SingletonSelectAndExpand.json");
            }
        }

        public static string MetadataWithSingleton
        {
            get
            {
                return GetString("MetadataWithSingleton.xml");
            }
        }

        public static string SingletonNavigationToEntitysetFullMetadata
        {
            get
            {
                return GetString("SingletonNavigationToEntitysetFullMetadata.json");
            }
        }

        public static string EntityNavigationToSingletonFullMetadata
        {
            get
            {
                return GetString("EntityNavigationToSingletonFullMetadata.json");
            }
        }

        public static string GetString(string fileName)
        {
            using (Stream stream = GetStream(fileName))
            using (TextReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static string GetPath(string fileName)
        {
            const string projectDefaultNamespace = "System.Web.OData.Test";
            const string resourcesFolderName = "Resources";
            const string pathSeparator = ".";
            return projectDefaultNamespace + pathSeparator + resourcesFolderName + pathSeparator + fileName;
        }

        private static Stream GetStream(string fileName)
        {
            string path = GetPath(fileName);
            Stream stream = typeof(Resources).Assembly.GetManifestResourceStream(path);

            if (stream == null)
            {
                string message = Error.Format("The embedded resource '{0}' was not found.", path);
                throw new FileNotFoundException(message, path);
            }

            return stream;
        }
    }
}
