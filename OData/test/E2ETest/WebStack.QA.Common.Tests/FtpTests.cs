using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WebStack.QA.Common.FileSystem;
using WebStack.QA.Common.Utils;
using Xunit;

namespace WebStack.QA.Common.Tests
{
    public class FtpTests
    {
        private Ftp CreateFtp()
        {
            var ftp = new Ftp();
            ftp.Connect("ftp://waws-prod-ch1-001.ftp.azurewebsites.windows.net");
            ftp.SetCredential(@"webstacktest01\hongyes", "Password01!");

            return ftp;
        }

        [Fact]
        public async Task FileDoesNotExistShouldWork()
        {
            var ftp = CreateFtp();
            Assert.False(await ftp.FileExistsAsync("site/wwwroot/doesnotexistfile.html"));
        }

        [Fact]
        public async Task FolderExistsShouldWork()
        {
            var ftp = CreateFtp();
            Assert.True(await ftp.DirectoryExistsAsync("site/wwwroot/"));
        }

        [Fact]
        public async Task FolderDoesNotExistShouldWork()
        {
            var ftp = CreateFtp();
            Assert.False(await ftp.DirectoryExistsAsync("site/doesnotexist/"));
        }

        [Fact]
        public async Task CreateAndDeleteFileShouldWork()
        {
            var ftp = CreateFtp();
            var path = "site/wwwroot/test.html";
            Assert.False(await ftp.FileExistsAsync(path));
            var content = Encoding.UTF8.GetBytes("test");
            await ftp.UploadFileAsync(path, content);
            Assert.True(await ftp.FileExistsAsync(path));
            await ftp.DeleteFileAsync(path);
            Assert.False(await ftp.FileExistsAsync(path));
        }

        [Fact]
        public async Task MakeAndRemoveDirectoryShouldWork()
        {
            var ftp = CreateFtp();
            var path = "site/wwwroot/test/";
            Assert.False(await ftp.DirectoryExistsAsync(path));
            await ftp.MakeDirectoryAsync(path);
            Assert.True(await ftp.DirectoryExistsAsync(path));
            await ftp.RemoveDirectoryAsync(path);
            Assert.False(await ftp.DirectoryExistsAsync(path));
        }

        [Fact]
        public async Task ListDirectoryShouldWork()
        {
            var ftp = CreateFtp();
            var path = "site/wwwroot/";
            var list = await ftp.ListDirectoryAsync(path);
            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }

        [Fact]
        public async Task ListDirectoryDetailsShouldWork()
        {
            var ftp = CreateFtp();
            var path = "site/wwwroot/";
            var list = await ftp.ListDirectoryDetailsAsync(path);
            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }

        [Fact]
        public async Task RemoveDirectoryRecursivelyShouldWork()
        {
            var ftp = CreateFtp();
            var path = "site/wwwroot/";
            var folderName = "test1/";
            var fileName = "test1.html";
            ftp.ChangeDirectory(path);
            Assert.False(await ftp.DirectoryExistsAsync(folderName));
            await ftp.MakeDirectoryAsync(folderName);
            Assert.True(await ftp.DirectoryExistsAsync(folderName));
            ftp.ChangeDirectory(folderName);
            var content = Encoding.UTF8.GetBytes(folderName);
            await ftp.UploadFileAsync(fileName, content);
            Assert.True(await ftp.FileExistsAsync(fileName));
            await ftp.MakeDirectoryAsync(folderName);
            ftp.ChangeDirectory(folderName);
            await ftp.UploadFileAsync(fileName, content);
            ftp.ChangeDirectory("../../");
            await ftp.RemoveDirectoryRecursivelyAsync(folderName);
            Assert.False(await ftp.DirectoryExistsAsync(folderName));
        }

        [Fact]
        public async Task UploadDiskDirectoryRecursivelyShouldWork()
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var subDir = dir.CreateSubdirectory("test");
            using (var writer = File.CreateText(Path.Combine(subDir.FullName, "test.html")))
            {
                writer.Write("Test");
            }
            var subSubDir = subDir.CreateSubdirectory("test");
            using (var writer = File.CreateText(Path.Combine(subSubDir.FullName, "test.html")))
            {
                writer.Write("Test");
            }
            
            var ftp = CreateFtp();
            var path = "site/wwwroot/";
            var folderName = "test/";
            ftp.ChangeDirectory(path);
            await ftp.MakeDirectoryAsync(folderName);
            Assert.True(await ftp.DirectoryExistsAsync(folderName));
            await ftp.UploadDirectoryRecursivelyAsync(folderName, subDir.FullName);
            Assert.True(await ftp.FileExistsAsync("test/test/test.html"));

            await ftp.RemoveDirectoryRecursivelyAsync(folderName);
            Assert.False(await ftp.DirectoryExistsAsync(folderName));

            subDir.Delete(true);
        }

        [Fact]
        public async Task UploadMemoryDirectoryRecursivelyShouldWork()
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var subDir = dir.CreateSubdirectory("test");
            using (var writer = File.CreateText(Path.Combine(subDir.FullName, "test.html")))
            {
                writer.Write("Test");
            }
            var subSubDir = subDir.CreateSubdirectory("test");
            using (var writer = File.CreateText(Path.Combine(subSubDir.FullName, "test.html")))
            {
                writer.Write("Test");
            }

            var directory = new MemoryDirectory("root", null);
            directory.CopyFromDisk(subDir);
            directory.CreateDirectory("memory").CreateFileFromText("memory.txt", "memory");

            var ftp = CreateFtp();
            var path = "site/wwwroot/";
            var folderName = "test/";
            ftp.ChangeDirectory(path);
            await ftp.MakeDirectoryAsync(folderName);
            Assert.True(await ftp.DirectoryExistsAsync(folderName));
            await ftp.UploadDirectoryRecursivelyAsync(folderName, directory);
            Assert.True(await ftp.FileExistsAsync("test/test/test.html"));
            Assert.True(await ftp.FileExistsAsync("test/memory/memory.txt"));
            await ftp.RemoveDirectoryRecursivelyAsync(folderName);
            Assert.False(await ftp.DirectoryExistsAsync(folderName));

            subDir.Delete(true);
        }
    }
}
