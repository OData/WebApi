using System;
using System.Collections.Generic;
using WebStack.QA.Common.FileSystem;

namespace WebStack.QA.Common.WebHost
{
    /// <summary>
    /// Deployment options which is designed to control the deployment process 
    /// which takes an IDirectory as source and deploy it to any target. The class 
    /// supports web.config transformation.
    /// </summary>
    public abstract class DeploymentOptions
    {
        private const string WebConfigFileName = "web.config";

        protected DeploymentOptions()
        {
            CleanTargetDirectory = true;
            RemoveEmptyDirectory = true;
            WebConfigTransformers = new List<WebConfigTransformer>();
        }

        public bool CleanTargetDirectory { get; set; }
        public bool RemoveEmptyDirectory { get; set; }
        public List<WebConfigTransformer> WebConfigTransformers { get; set; }

        public virtual void Deploy(IDirectory source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (RemoveEmptyDirectory)
            {
                source.RemoveEmptyDirectories();
            }

            if (WebConfigTransformers.Count > 0)
            {
                WebConfigHelper config;
                var file = source.FindFile(WebConfigFileName);
                if (file == null)
                {
                    file = source.CreateFile(WebConfigFileName);
                    config = WebConfigHelper.New();
                }
                else
                {
                    config = WebConfigHelper.Load(file.ReadAsString());
                }

                WebConfigTransformers.Transform(config);

                file.WriteString(config.ToString());
            }

            DeployCore(source);
        }

        protected abstract void DeployCore(IDirectory source);

        public abstract void Remove();
    }
}
