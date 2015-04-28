using System;
using System.IO;
using WebStack.QA.Common.FileSystem;

namespace WebStack.QA.Common.WebHost
{
    /// <summary>
    /// Deploy the IDirectory source to local disk directory.
    /// </summary>
    public class LocalDeploymentOptions : DeploymentOptions
    {
        public LocalDeploymentOptions(DirectoryInfo target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            Target = target;
        }

        public DirectoryInfo Target { get; private set; }

        protected override void DeployCore(IDirectory source)
        {
            if (CleanTargetDirectory)
            {
                Remove();
            }

            Target.Refresh();
            if (!Target.Exists)
            {
                Target.Create();
            }

            source.CopyToDisk(Target);
        }

        public override void Remove()
        {
            Target.Refresh();
            if (Target.Exists)
            {
                Target.Delete(true);
            }
        }
    }
}
