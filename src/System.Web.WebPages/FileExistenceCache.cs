// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Web.Hosting;

namespace System.Web.WebPages
{
    /// <summary>
    /// This class caches the result of VirtualPathProvider.FileExists for a short
    /// period of time, and recomputes it if necessary.
    /// 
    /// The default VPP MapPathBasedVirtualPathProvider caches the result of
    /// the FileExists call with the appropriate dependencies, so it is less
    /// expensive on subsequent calls, but it still needs to do MapPath which can 
    /// take quite some time.
    /// </summary>
    internal class FileExistenceCache
    {
        private const int TickPerMiliseconds = 10000;
        private readonly VirtualPathProvider _virtualPathProvider;
        private ConcurrentDictionary<string, bool> _cache;
        private long _creationTick;
        private int _ticksBeforeReset;

        public FileExistenceCache(VirtualPathProvider virtualPathProvider, int milliSecondsBeforeReset = 1000)
        {
            _virtualPathProvider = virtualPathProvider;
            _ticksBeforeReset = milliSecondsBeforeReset * TickPerMiliseconds;
            Reset();
        }

        // Use the VPP returned by the HostingEnvironment unless a custom vpp is passed in (mainly for testing purposes)
        public VirtualPathProvider VirtualPathProvider
        {
            get { return _virtualPathProvider; }
        }

        public int MilliSecondsBeforeReset
        {
            get { return _ticksBeforeReset / TickPerMiliseconds; }
            internal set { _ticksBeforeReset = value * TickPerMiliseconds; }
        }

        internal IDictionary<string, bool> CacheInternal
        {
            get { return _cache; }
        }

        public bool TimeExceeded
        {
            get { return (DateTime.UtcNow.Ticks - Interlocked.Read(ref _creationTick)) > _ticksBeforeReset; }
        }

        public void Reset()
        {
            _cache = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            DateTime now = DateTime.UtcNow;
            long tick = now.Ticks;

            Interlocked.Exchange(ref _creationTick, tick);
        }

        public bool FileExists(string virtualPath)
        {
            if (TimeExceeded)
            {
                Reset();
            }
            // The right way to do this is to verify in the constructor that the VirtualPathProvider argument is not null.
            // However when unit testing this, we often new up instances when not running under Asp.Net when HostingEnvironment.VirtualPathProvider is null.
            Debug.Assert(_virtualPathProvider != null);
            return _cache.GetOrAdd(virtualPath, _virtualPathProvider.FileExists);
        }
    }
}
