using System;
using Nuwa.Sdk;
using Xunit.Sdk;

namespace Nuwa.WebStack.Descriptor
{
    public class DeploymentDescriptor
    {
        private ITypeInfo _testClassType;

        public DeploymentDescriptor(ITypeInfo testClassType)
        {
            _testClassType = testClassType;

            var scopeAttr = testClassType.GetFirstCustomAttribute<NuwaWebDeploymentScopeAttribute>();
            if (scopeAttr != null)
            {
                this.DeploymentType = scopeAttr.DeploymentType;
                ScopePath = scopeAttr.ScopePath;
                ScopeResourceType = scopeAttr.ResourceType;
            }
        }

        public DeploymentType DeploymentType
        {
            get;
            private set;
        }

        public string ScopePath
        {
            get;
            private set;
        }

        public Type ScopeResourceType
        {
            get;
            private set;
        }
    }
}
