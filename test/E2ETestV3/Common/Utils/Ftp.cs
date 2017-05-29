using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WebStack.QA.Common.FileSystem;

namespace WebStack.QA.Common.Utils
{
    public class Ftp
    {
        public Uri CurrentDirectoryUri { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public void Connect(string ftpHostUri)
        {
            this.CurrentDirectoryUri = new Uri(ftpHostUri);
        }

        public void SetCredential(string userName, string password)
        {
            this.UserName = userName;
            this.Password = password;
        }

        public void ChangeDirectory(string path)
        {
            this.CurrentDirectoryUri = new Uri(CurrentDirectoryUri, path);
        }

        public async Task<bool> FileExistsAsync(string path)
        {
            var request = CreateFtpRequest(path, WebRequestMethods.Ftp.GetFileSize);

            try
            {
                using (var response = await request.GetResponseAsync())
                {
                    return true;
                }
            }
            catch (WebException ex)
            {
                using (FtpWebResponse response = (FtpWebResponse)ex.Response)
                {
                    if (response.StatusCode ==
                        FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        return false;
                    }
                }
                throw;
            }
        }

        public async Task<bool> DirectoryExistsAsync(string path)
        {
            var request = CreateFtpRequest(path, WebRequestMethods.Ftp.ListDirectoryDetails);

            try
            {
                using (var response = (FtpWebResponse) await request.GetResponseAsync())
                {
                    return true;
                }
            }
            catch (WebException ex)
            {
                using (FtpWebResponse response = (FtpWebResponse)ex.Response)
                {
                    if (response.StatusCode ==
                        FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        return false;
                    }
                }
                throw;
            }
        }

        public async Task DeleteFileAsync(string path)
        {
            var request = CreateFtpRequest(path, WebRequestMethods.Ftp.DeleteFile);
            using (var response = (FtpWebResponse) await request.GetResponseAsync())
            { 
            }
        }

        public async Task UploadFileAsync(string path, byte[] content)
        {
            var request = CreateFtpRequest(path, WebRequestMethods.Ftp.UploadFile);
            Stream requestStream = await request.GetRequestStreamAsync();
            requestStream.Write(content, 0, content.Length);
            requestStream.Close();

            using (var response = (FtpWebResponse) await request.GetResponseAsync())
            {
            }
        }

        public async Task UploadFileAsync(string path, string localPath)
        {
            var request = CreateFtpRequest(path, WebRequestMethods.Ftp.UploadFile);
            using (var localStream = File.OpenRead(localPath))
            {                
                using (var requestStream = await request.GetRequestStreamAsync())
                {
                    await localStream.CopyToAsync(requestStream);
                }
            }

            using (var response = (FtpWebResponse) await request.GetResponseAsync())
            {
            }
        }

        public async Task UploadFileAsync(string path, IFile file)
        {
            var request = CreateFtpRequest(path, WebRequestMethods.Ftp.UploadFile);
            using (var localStream = file.StreamProvider.OpenRead())
            {
                using (var requestStream = await request.GetRequestStreamAsync())
                {
                    await localStream.CopyToAsync(requestStream);
                }
            }

            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            {
            }
        }

        public async Task RemoveDirectoryAsync(string path)
        {
            var request = CreateFtpRequest(path, WebRequestMethods.Ftp.RemoveDirectory);
            using (var response = (FtpWebResponse) await request.GetResponseAsync())
            {
            }
        }

        public async Task MakeDirectoryAsync(string path)
        {
            var request = CreateFtpRequest(path, WebRequestMethods.Ftp.MakeDirectory);
            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            {
            }
        }

        public async Task<string[]> ListDirectoryAsync(string path)
        {
            var list = new List<string>();
            var request = CreateFtpRequest(path, WebRequestMethods.Ftp.ListDirectory);
            using (var response = (FtpWebResponse) await request.GetResponseAsync())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    while (!reader.EndOfStream)
                    {
                        list.Add(reader.ReadLine());
                    }
                }
            }
            return list.ToArray();
        }

        public async Task<string[]> ListDirectoryDetailsAsync(string path)
        {
            var list = new List<string>();
            var request = CreateFtpRequest(path, WebRequestMethods.Ftp.ListDirectoryDetails);
            using (var response = (FtpWebResponse) await request.GetResponseAsync())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    while (!reader.EndOfStream)
                    {
                        list.Add(reader.ReadLine());
                    }
                }
            }
            return list.ToArray();
        }

        public async Task RemoveDirectoryRecursivelyAsync(string path, bool includeRootFolder = true)
        {
            var files = await ListDirectoryAsync(path);
            await Task.WhenAll(files.Select(async file =>
                {
                    var filePath = Combine(path, file);
                    if (await FileExistsAsync(filePath))
                    {
                        await DeleteFileAsync(filePath);
                    }
                    else
                    {
                        await RemoveDirectoryRecursivelyAsync(filePath, true);
                    }
                }));

            if (includeRootFolder)
            {
                await RemoveDirectoryAsync(path);
            }
        }

        public async Task UploadDirectoryRecursivelyAsync(string path, string localPath, string searchPattern = "*.*")
        {
            var dir = new DirectoryInfo(localPath);
            var files = dir.EnumerateFiles(searchPattern);
            var subDirs = dir.EnumerateDirectories();

            var uploadFileTasks = files.Select(async file =>
                {
                    var filePath = Combine(path, file.Name);
                    await UploadFileAsync(filePath, file.FullName);
                });
            var uploadDirTasks = subDirs.Select(async subDir =>
                {
                    var subDirPath = Combine(path, subDir.Name);
                    await MakeDirectoryAsync(subDirPath);
                    await UploadDirectoryRecursivelyAsync(subDirPath, subDir.FullName, searchPattern);
                });

            await Task.WhenAll(uploadFileTasks.Concat(uploadDirTasks));
        }

        public async Task UploadDirectoryRecursivelyAsync(string path, IDirectory directory)
        {
            var files = directory.GetSubFiles();
            var subDirs = directory.GetSubDirectories();

            var uploadFileTasks = files.Select(async file =>
            {
                var filePath = Combine(path, file.Name);
                await UploadFileAsync(filePath, file);
            });
            var uploadDirTasks = subDirs.Select(async subDir =>
            {
                var subDirPath = Combine(path, subDir.Name);
                await MakeDirectoryAsync(subDirPath);
                await UploadDirectoryRecursivelyAsync(subDirPath, subDir);
            });

            await Task.WhenAll(uploadFileTasks.Concat(uploadDirTasks));
        }

        public FtpWebRequest CreateFtpRequest(string path, string method)
        {
            var request = (FtpWebRequest)WebRequest.Create(new Uri(CurrentDirectoryUri, path));
            Console.WriteLine(request.RequestUri);
            request.Credentials = new NetworkCredential(UserName, Password);
            request.Method = method;
            return request;
        }

        public string Combine(string path1, string path2)
        {
            return path1.TrimEnd('/') + "/" + path2.TrimStart('/');
        }
    }
}
