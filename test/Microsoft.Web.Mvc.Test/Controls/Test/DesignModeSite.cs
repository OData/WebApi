// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace Microsoft.Web.Mvc.Controls.Test
{
    public class DesignModeSite : ISite
    {
        IComponent ISite.Component
        {
            get { throw new NotImplementedException(); }
        }

        IContainer ISite.Container
        {
            get { throw new NotImplementedException(); }
        }

        bool ISite.DesignMode
        {
            get { return true; }
        }

        string ISite.Name
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}
