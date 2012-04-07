// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Web.WebPages.Deployment;
using System.Web.WebPages.Resources;

namespace System.Web.WebPages
{
    internal sealed class WebPageRoute
    {
        private static readonly Lazy<bool> _isRootExplicitlyDisabled = new Lazy<bool>(() => WebPagesDeployment.IsExplicitlyDisabled("~/"));
        private IVirtualPathFactory _virtualPathFactory;
        private bool? _isExplicitlyDisabled;

        internal IVirtualPathFactory VirtualPathFactory
        {
            get { return _virtualPathFactory ?? VirtualPathFactoryManager.Instance; }
            set { _virtualPathFactory = value; }
        }

        internal bool IsExplicitlyDisabled
        {
            get { return _isExplicitlyDisabled ?? _isRootExplicitlyDisabled.Value; }
            set { _isExplicitlyDisabled = value; }
        }

        internal void DoPostResolveRequestCache(HttpContextBase context)
        {
            if (IsExplicitlyDisabled)
            {
                // If the root config is explicitly disabled, do not process the request.
                return;
            }

            // Parse incoming URL (we trim off the first two chars since they're always "~/")
            string requestPath = context.Request.AppRelativeCurrentExecutionFilePath.Substring(2) + context.Request.PathInfo;
            var registeredExtensions = WebPageHttpHandler.GetRegisteredExtensions();

            // Check if this request matches a file in the app
            WebPageMatch webpageRouteMatch = MatchRequest(requestPath, registeredExtensions, VirtualPathFactory, context, DisplayModeProvider.Instance);
            if (webpageRouteMatch != null)
            {
                // If it matches then save some data for the WebPage's UrlData
                context.Items[typeof(WebPageMatch)] = webpageRouteMatch;

                string virtualPath = "~/" + webpageRouteMatch.MatchedPath;

                // Verify that this path is enabled before remapping
                if (!WebPagesDeployment.IsExplicitlyDisabled(virtualPath))
                {
                    IHttpHandler handler = WebPageHttpHandler.CreateFromVirtualPath(virtualPath);
                    if (handler != null)
                    {
                        SessionStateUtil.SetUpSessionState(context, handler);
                        // Remap to our handler
                        context.RemapHandler(handler);
                    }
                }
            }
            else
            {
                // Bug:904704 If its not a match, but to a supported extension, we want to return a 404 instead of a 403
                string extension = PathUtil.GetExtension(requestPath);
                foreach (string supportedExt in registeredExtensions)
                {
                    if (String.Equals("." + supportedExt, extension, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new HttpException(404, null);
                    }
                }
            }
        }

        private static bool FileExists(string virtualPath, IVirtualPathFactory virtualPathFactory)
        {
            var path = "~/" + virtualPath;
            return virtualPathFactory.Exists(path);
        }

        internal static WebPageMatch GetWebPageMatch(HttpContextBase context)
        {
            WebPageMatch webPageMatch = (WebPageMatch)context.Items[typeof(WebPageMatch)];
            return webPageMatch;
        }

        private static string GetRouteLevelMatch(string pathValue, IEnumerable<string> supportedExtensions, IVirtualPathFactory virtualPathFactory, HttpContextBase context, DisplayModeProvider displayModeProvider)
        {
            foreach (string supportedExtension in supportedExtensions)
            {
                string virtualPath = "~/" + pathValue;

                // Only add the extension if it's not already there
                if (!virtualPath.EndsWith("." + supportedExtension, StringComparison.OrdinalIgnoreCase))
                {
                    virtualPath += "." + supportedExtension;
                }
                DisplayInfo virtualPathDisplayInfo = displayModeProvider.GetDisplayInfoForVirtualPath(virtualPath, context, virtualPathFactory.Exists, currentDisplayMode: null);

                if (virtualPathDisplayInfo != null)
                {
                    // If there's an exact match on disk, return it unless it starts with an underscore
                    if (Path.GetFileName(virtualPathDisplayInfo.FilePath).StartsWith("_", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new HttpException(404, WebPageResources.WebPageRoute_UnderscoreBlocked);
                    }

                    string resolvedVirtualPath = virtualPathDisplayInfo.FilePath;

                    // Matches are not expected to be virtual paths so remove the ~/ from the match
                    if (resolvedVirtualPath.StartsWith("~/", StringComparison.OrdinalIgnoreCase))
                    {
                        resolvedVirtualPath = resolvedVirtualPath.Remove(0, 2);
                    }

                    DisplayModeProvider.SetDisplayMode(context, virtualPathDisplayInfo.DisplayMode);

                    return resolvedVirtualPath;
                }
            }

            return null;
        }

        internal static WebPageMatch MatchRequest(string pathValue, IEnumerable<string> supportedExtensions, IVirtualPathFactory virtualPathFactory, HttpContextBase context, DisplayModeProvider displayModes)
        {
            string currentLevel = String.Empty;
            string currentPathInfo = pathValue;

            // We can skip the file exists check and normal lookup for empty paths, but we still need to look for default pages
            if (!String.IsNullOrEmpty(pathValue))
            {
                // If the file exists and its not a supported extension, let the request go through
                if (FileExists(pathValue, virtualPathFactory))
                {
                    // TODO: Look into switching to RawURL to eliminate the need for this issue
                    bool foundSupportedExtension = false;
                    foreach (string supportedExtension in supportedExtensions)
                    {
                        if (pathValue.EndsWith("." + supportedExtension, StringComparison.OrdinalIgnoreCase))
                        {
                            foundSupportedExtension = true;
                            break;
                        }
                    }

                    if (!foundSupportedExtension)
                    {
                        return null;
                    }
                }

                // For each trimmed part of the path try to add a known extension and
                // check if it matches a file in the application.
                currentLevel = pathValue;
                currentPathInfo = String.Empty;
                while (true)
                {
                    // Does the current route level patch any supported extension?
                    string routeLevelMatch = GetRouteLevelMatch(currentLevel, supportedExtensions, virtualPathFactory, context, displayModes);
                    if (routeLevelMatch != null)
                    {
                        return new WebPageMatch(routeLevelMatch, currentPathInfo);
                    }

                    // Try to remove the last path segment (e.g. go from /foo/bar to /foo)
                    int indexOfLastSlash = currentLevel.LastIndexOf('/');
                    if (indexOfLastSlash == -1)
                    {
                        // If there are no more slashes, we're done
                        break;
                    }
                    else
                    {
                        // Chop off the last path segment to get to the next one
                        currentLevel = currentLevel.Substring(0, indexOfLastSlash);

                        // And save the path info in case there is a match
                        currentPathInfo = pathValue.Substring(indexOfLastSlash + 1);
                    }
                }
            }

            return MatchDefaultFiles(pathValue, supportedExtensions, virtualPathFactory, context, displayModes, currentLevel);
        }

        private static WebPageMatch MatchDefaultFiles(string pathValue, IEnumerable<string> supportedExtensions, IVirtualPathFactory virtualPathFactory, HttpContextBase context, DisplayModeProvider displayModes, string currentLevel)
        {
            // If we haven't found anything yet, now try looking for default.* or index.* at the current url
            currentLevel = pathValue;
            string currentLevelDefault;
            string currentLevelIndex;
            if (String.IsNullOrEmpty(currentLevel))
            {
                currentLevelDefault = "default";
                currentLevelIndex = "index";
            }
            else
            {
                if (currentLevel[currentLevel.Length - 1] != '/')
                {
                    currentLevel += "/";
                }
                currentLevelDefault = currentLevel + "default";
                currentLevelIndex = currentLevel + "index";
            }

            // Does the current route level match any supported extension?
            string defaultMatch = GetRouteLevelMatch(currentLevelDefault, supportedExtensions, virtualPathFactory, context, displayModes);
            if (defaultMatch != null)
            {
                return new WebPageMatch(defaultMatch, String.Empty);
            }

            string indexMatch = GetRouteLevelMatch(currentLevelIndex, supportedExtensions, virtualPathFactory, context, displayModes);
            if (indexMatch != null)
            {
                return new WebPageMatch(indexMatch, String.Empty);
            }

            return null;
        }
    }
}
