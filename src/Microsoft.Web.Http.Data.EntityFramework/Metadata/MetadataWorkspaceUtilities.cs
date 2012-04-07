// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Mapping;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Http;

namespace Microsoft.Web.Http.Data.EntityFramework.Metadata
{
    /// <summary>
    /// EF metadata utilities class.
    /// </summary>
    internal static class MetadataWorkspaceUtilities
    {
        /// <summary>
        /// Creates a metadata workspace for the specified context.
        /// </summary>
        /// <param name="contextType">The type of the object context.</param>
        /// <param name="isDbContext">Set to <c>true</c> if context is a database context.</param>
        /// <returns>The metadata workspace.</returns>
        public static MetadataWorkspace CreateMetadataWorkspace(Type contextType, bool isDbContext)
        {
            MetadataWorkspace metadataWorkspace = null;

            if (!isDbContext)
            {
                metadataWorkspace = MetadataWorkspaceUtilities.CreateMetadataWorkspaceFromResources(contextType, typeof(ObjectContext));
            }
            else
            {
                metadataWorkspace = MetadataWorkspaceUtilities.CreateMetadataWorkspaceFromResources(contextType, typeof(System.Data.Entity.DbContext));
                if (metadataWorkspace == null && typeof(System.Data.Entity.DbContext).IsAssignableFrom(contextType))
                {
                    if (contextType.GetConstructor(Type.EmptyTypes) == null)
                    {
                        throw Error.InvalidOperation(Resource.DefaultCtorNotFound, contextType.FullName);
                    }

                    try
                    {
                        System.Data.Entity.DbContext dbContext = Activator.CreateInstance(contextType) as System.Data.Entity.DbContext;
                        ObjectContext objectContext = (dbContext as System.Data.Entity.Infrastructure.IObjectContextAdapter).ObjectContext;
                        metadataWorkspace = objectContext.MetadataWorkspace;
                    }
                    catch (Exception efException)
                    {
                        throw Error.InvalidOperation(efException, Resource.MetadataWorkspaceNotFound, contextType.FullName);
                    }
                }
            }
            if (metadataWorkspace == null)
            {
                throw Error.InvalidOperation(Resource.LinqToEntitiesProvider_UnableToRetrieveMetadata, contextType.Name);
            }
            else
            {
                return metadataWorkspace;
            }
        }

        /// <summary>
        /// Creates the MetadataWorkspace for the given context type and base context type.
        /// </summary>
        /// <param name="contextType">The type of the context.</param>
        /// <param name="baseContextType">The base context type (DbContext or ObjectContext).</param>
        /// <returns>The generated <see cref="MetadataWorkspace"/></returns>
        public static MetadataWorkspace CreateMetadataWorkspaceFromResources(Type contextType, Type baseContextType)
        {
            // get the set of embedded mapping resources for the target assembly and create
            // a metadata workspace info for each group
            IEnumerable<string> metadataResourcePaths = FindMetadataResources(contextType.Assembly);
            IEnumerable<MetadataWorkspaceInfo> workspaceInfos = GetMetadataWorkspaceInfos(metadataResourcePaths);

            // Search for the correct EntityContainer by name and if found, create
            // a comlete MetadataWorkspace and return it
            foreach (var workspaceInfo in workspaceInfos)
            {
                EdmItemCollection edmItemCollection = new EdmItemCollection(workspaceInfo.Csdl);

                Type currentType = contextType;
                while (currentType != baseContextType && currentType != typeof(object))
                {
                    EntityContainer container;
                    if (edmItemCollection.TryGetEntityContainer(currentType.Name, out container))
                    {
                        StoreItemCollection store = new StoreItemCollection(workspaceInfo.Ssdl);
                        StorageMappingItemCollection mapping = new StorageMappingItemCollection(edmItemCollection, store, workspaceInfo.Msl);
                        MetadataWorkspace workspace = new MetadataWorkspace();
                        workspace.RegisterItemCollection(edmItemCollection);
                        workspace.RegisterItemCollection(store);
                        workspace.RegisterItemCollection(mapping);
                        workspace.RegisterItemCollection(new ObjectItemCollection());
                        return workspace;
                    }

                    currentType = currentType.BaseType;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the specified resource paths as metadata workspace info objects.
        /// </summary>
        /// <param name="resourcePaths">The metadata resource paths.</param>
        /// <returns>The metadata workspace info objects.</returns>
        private static IEnumerable<MetadataWorkspaceInfo> GetMetadataWorkspaceInfos(IEnumerable<string> resourcePaths)
        {
            // for file paths, you would want to group without the path or the extension like Path.GetFileNameWithoutExtension, but resource names can contain
            // forbidden path chars, so don't use it on resource names
            foreach (var group in resourcePaths.GroupBy(p => p.Substring(0, p.LastIndexOf('.')), StringComparer.InvariantCultureIgnoreCase))
            {
                yield return MetadataWorkspaceInfo.Create(group);
            }
        }

        /// <summary>
        /// Find all the EF metadata resources.
        /// </summary>
        /// <param name="assembly">The assembly to find the metadata resources in.</param>
        /// <returns>The metadata paths that were found.</returns>
        private static IEnumerable<string> FindMetadataResources(Assembly assembly)
        {
            List<string> result = new List<string>();
            foreach (string name in assembly.GetManifestResourceNames())
            {
                if (MetadataWorkspaceInfo.IsMetadata(name))
                {
                    result.Add(String.Format(CultureInfo.InvariantCulture, "res://{0}/{1}", assembly.FullName, name));
                }
            }

            return result;
        }

        /// <summary>
        /// Represents the paths for a single metadata workspace.
        /// </summary>
        private class MetadataWorkspaceInfo
        {
            private const string CsdlExtension = ".csdl";
            private const string MslExtension = ".msl";
            private const string SsdlExtension = ".ssdl";

            public MetadataWorkspaceInfo(string csdlPath, string mslPath, string ssdlPath)
            {
                if (csdlPath == null)
                {
                    throw Error.ArgumentNull("csdlPath");
                }

                if (mslPath == null)
                {
                    throw Error.ArgumentNull("mslPath");
                }

                if (ssdlPath == null)
                {
                    throw Error.ArgumentNull("ssdlPath");
                }

                Csdl = csdlPath;
                Msl = mslPath;
                Ssdl = ssdlPath;
            }

            public string Csdl { get; private set; }

            public string Msl { get; private set; }

            public string Ssdl { get; private set; }

            public static MetadataWorkspaceInfo Create(IEnumerable<string> paths)
            {
                string csdlPath = null;
                string mslPath = null;
                string ssdlPath = null;
                foreach (string path in paths)
                {
                    if (path.EndsWith(CsdlExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        csdlPath = path;
                    }
                    else if (path.EndsWith(MslExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        mslPath = path;
                    }
                    else if (path.EndsWith(SsdlExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        ssdlPath = path;
                    }
                }

                return new MetadataWorkspaceInfo(csdlPath, mslPath, ssdlPath);
            }

            public static bool IsMetadata(string path)
            {
                return path.EndsWith(CsdlExtension, StringComparison.OrdinalIgnoreCase) ||
                       path.EndsWith(MslExtension, StringComparison.OrdinalIgnoreCase) ||
                       path.EndsWith(SsdlExtension, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
