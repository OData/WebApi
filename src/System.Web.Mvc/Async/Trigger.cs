// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc.Async
{
    // Provides a trigger for the TriggerListener class.

    internal sealed class Trigger
    {
        private readonly Action _fireAction;

        // Constructor should only be called by TriggerListener.
        internal Trigger(Action fireAction)
        {
            _fireAction = fireAction;
        }

        public void Fire()
        {
            _fireAction();
        }
    }
}
