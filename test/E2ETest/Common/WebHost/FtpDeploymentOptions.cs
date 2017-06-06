using System;
using WebStack.QA.Common.FileSystem;
using WebStack.QA.Common.Utils;

namespace WebStack.QA.Common.WebHost
{
    /// <summary>
    /// Deploy IDirectory source to remote FTP site
    /// </summary>
    public class FtpDeploymentOptions : DeploymentOptions
    {
        public FtpDeploymentOptions(string ftpTargetPath, string ftpUserName, string ftpPassword)
        {
            if (string.IsNullOrEmpty(ftpTargetPath))
            {
                throw new ArgumentNullException("ftpTargetPath");
            }

            FtpTargetPath = ftpTargetPath;
            FtpUserName = ftpUserName;
            FtpPassword = ftpPassword;
        }

        public string FtpTargetPath { get; set; }
        public string FtpUserName { get; set; }
        public string FtpPassword { get; set; }

        protected override void DeployCore(IDirectory source)
        {
            Ftp ftp = new Ftp();
            ftp.Connect(FtpTargetPath);
            ftp.SetCredential(FtpUserName, FtpPassword);

            if (CleanTargetDirectory)
            {
                ftp.RemoveDirectoryRecursivelyAsync("./", false).Wait();
            }

            ftp.UploadDirectoryRecursivelyAsync("./", source).Wait();
        }

        public override void Remove()
        {
            Ftp ftp = new Ftp();
            ftp.Connect(FtpTargetPath);
            ftp.SetCredential(FtpUserName, FtpPassword);
            ftp.RemoveDirectoryRecursivelyAsync("./", false).Wait();
        }
    }
}
