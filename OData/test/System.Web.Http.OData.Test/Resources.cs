// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;

namespace System.Web.Http
{
    internal static class Resources
    {
        public static string ArrayOfBooleanInAtom
        {
            get
            {
                return GetString("ArrayOfBooleanInAtom.xml");
            }
        }

        public static string ArrayOfBooleanInJsonLight
        {
            get
            {
                return GetString("ArrayOfBooleanInJsonLight.json");
            }
        }

        public static string ArrayOfInt32InAtom
        {
            get
            {
                return GetString("ArrayOfInt32InAtom.xml");
            }
        }

        public static string ArrayOfInt32InJsonLight
        {
            get
            {
                return GetString("ArrayOfInt32InJsonLight.json");
            }
        }

        public static string CollectionOfPersonInAtom
        {
            get
            {
                return GetString("CollectionOfPersonInAtom.xml");
            }
        }

        public static string CollectionOfPersonInJsonLight
        {
            get
            {
                return GetString("CollectionOfPersonInJsonLight.json");
            }
        }

        public static string EmployeeEntryInAtom
        {
            get
            {
                return GetString("EmployeeEntryInAtom.xml");
            }
        }

        public static string EmployeeEntryInJsonLight
        {
            get
            {
                return GetString("EmployeeEntryInJsonLight.json");
            }
        }

        public static string FeedOfEmployeeInAtom
        {
            get
            {
                return GetString("FeedOfEmployeeInAtom.xml");
            }
        }

        public static string FeedOfEmployeeInJsonLight
        {
            get
            {
                return GetString("FeedOfEmployeeInJsonLight.json");
            }
        }

        public static string ListOfStringInAtom
        {
            get
            {
                return GetString("ListOfStringInAtom.xml");
            }
        }

        public static string ListOfStringInJsonLight
        {
            get
            {
                return GetString("ListOfStringInJsonLight.json");
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

        public static string PersonComplexTypeInAtom
        {
            get
            {
                return GetString("PersonComplexTypeInAtom.xml");
            }
        }

        public static string PersonComplexTypeInJsonLight
        {
            get
            {
                return GetString("PersonComplexTypeInJsonLight.json");
            }
        }

        public static string PersonEntryInAtom
        {
            get
            {
                return GetString("PersonEntryInAtom.xml");
            }
        }

        public static string PersonEntryInJsonFullMetadata
        {
            get
            {
                return GetString("PersonEntryInJsonFullMetadata.json");
            }
        }

        public static string PersonEntryInJsonVerbose
        {
            get
            {
                return GetString("PersonEntryInJsonVerbose.json");
            }
        }

        public static string PersonEntryInPlainOldJson
        {
            get
            {
                return GetString("PersonEntryInPlainOldJson.json");
            }
        }

        public static string PersonRequestEntryInPlainOldJson
        {
            get
            {
                return GetString("PersonRequestEntryInPlainOldJson.json");
            }
        }

        public static string ProductRequestEntryInAtom
        {
            get
            {
                return GetString("ProductRequestEntryInAtom.xml");
            }
        }

        public static string ProductRequestEntryInPlainOldJson
        {
            get
            {
                return GetString("ProductRequestEntryInPlainOldJson.json");
            }
        }

        public static string ProductsCsdl
        {
            get
            {
                return GetString("ProductsCsdl.xml");
            }
        }

        public static string SupplierPatchInAtom
        {
            get
            {
                return GetString("SupplierPatchInAtom.xml");
            }
        }

        public static string SupplierPatchInPlainOldJson
        {
            get
            {
                return GetString("SupplierPatchInPlainOldJson.json");
            }
        }

        public static string SupplierRequestEntryInAtom
        {
            get
            {
                return GetString("SupplierRequestEntryInAtom.xml");
            }
        }

        public static string SupplierRequestEntryInPlainOldJson
        {
            get
            {
                return GetString("SupplierRequestEntryInPlainOldJson.json");
            }
        }

        public static string WorkItemEntryInAtom
        {
            get
            {
                return GetString("WorkItemEntryInAtom.xml");
            }
        }

        public static string WorkItemEntryInJsonLight
        {
            get
            {
                return GetString("WorkItemEntryInJsonLight.json");
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
            const string projectDefaultNamespace = "System.Web.Http";
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
