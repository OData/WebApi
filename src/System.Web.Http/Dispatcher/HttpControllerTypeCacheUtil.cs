using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Provides various utilities for the <see cref="HttpControllerTypeCache"/>.
    /// </summary>
    internal static class HttpControllerTypeCacheUtil
    {
        public static List<Type> GetFilteredTypesFromAssemblies(string cacheName, Predicate<Type> predicate, IBuildManager buildManager)
        {
            HttpControllerTypeCacheSerializer serializer = new HttpControllerTypeCacheSerializer();

            // first, try reading from the cache on disk
            List<Type> matchingTypes = ReadTypesFromCache(cacheName, predicate, buildManager, serializer);
            if (matchingTypes != null)
            {
                return matchingTypes;
            }

            // if reading from the cache failed, enumerate over every assembly looking for a matching type
            matchingTypes = FilterTypesInAssemblies(buildManager, predicate).ToList();

            // finally, save the cache back to disk
            SaveTypesToCache(cacheName, matchingTypes, buildManager, serializer);

            return matchingTypes;
        }

        private static IEnumerable<Type> FilterTypesInAssemblies(IBuildManager buildManager, Predicate<Type> predicate)
        {
            // Go through all assemblies referenced by the application and search for types matching a predicate
            IEnumerable<Type> typesSoFar = Type.EmptyTypes;

            ICollection assemblies = buildManager.GetReferencedAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.IsDynamic)
                {
                    // can't call GetExportedTypes on a dynamic assembly
                    continue;
                }

                typesSoFar = typesSoFar.Concat(assembly.GetExportedTypes());
            }

            return typesSoFar.Where(type => predicate(type));
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Cache failures are not fatal, and the code should continue executing normally.")]
        private static List<Type> ReadTypesFromCache(string cacheName, Predicate<Type> predicate, IBuildManager buildManager, HttpControllerTypeCacheSerializer serializer)
        {
            try
            {
                Stream stream = buildManager.ReadCachedFile(cacheName);
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        List<Type> deserializedTypes = serializer.DeserializeTypes(reader);
                        if (deserializedTypes != null && deserializedTypes.All(type => predicate(type)))
                        {
                            // If all read types still match the predicate, success!
                            return deserializedTypes;
                        }
                    }
                }
            }
            catch
            {
                // cache failures are not considered fatal -- keep running.
            }

            return null;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Cache failures are not fatal, and the code should continue executing normally.")]
        private static void SaveTypesToCache(string cacheName, IList<Type> matchingTypes, IBuildManager buildManager, HttpControllerTypeCacheSerializer serializer)
        {
            try
            {
                Stream stream = buildManager.CreateCachedFile(cacheName);
                if (stream != null)
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        serializer.SerializeTypes(matchingTypes, writer);
                    }
                }
            }
            catch
            {
                // cache failures are not considered fatal -- keep running.
            }
        }
    }
}
