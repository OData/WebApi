using System;

namespace Nuwa
{
    public enum DeploymentType
    {
        Assembly,
        Directory,
        Resource
    }

    /// <summary>
    /// Define the deployment source scope which can be disk files, assemblies or resource files 
    /// from assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NuwaWebDeploymentScopeAttribute : Attribute
    {

        public NuwaWebDeploymentScopeAttribute()
            : this(DeploymentType.Assembly)
        {
        }

        public NuwaWebDeploymentScopeAttribute(DeploymentType type, string path = null)
        {
            if (type == DeploymentType.Directory)
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new ArgumentNullException("path");
                }
            }

            this.DeploymentType = type;
            ScopePath = path;

        }

        public DeploymentType DeploymentType { get; set; }

        /// <summary>
        /// The scope path can be relative directory path to QARoot or 
        /// relative resource path to solution.
        /// It's not used in assembly only deployment
        /// </summary>
        public string ScopePath { get; set; }

        /// <summary>
        /// This is used when resource files are from different assembly than 
        /// the current test assembly.
        /// </summary>
        public Type ResourceType { get; set; }
    }
}
