using System;
using System.IO;
using System.Linq;
using WebStack.QA.Common.FileSystem;
using Xunit;

namespace WebStack.QA.Common.Tests
{
    public class FileSystemTests
    {
        [Fact]
        public void MemoryFileCanReadAndWrite()
        {
            var expected = "test";
            string actual;
            MemoryFile file = new MemoryFile("test.txt", null);
            using (StreamWriter writer = new StreamWriter(file.StreamProvider.OpenWrite()))
            {
                writer.Write(expected);
            }

            using (StreamReader reader = new StreamReader(file.StreamProvider.OpenRead()))
            {
                actual = reader.ReadToEnd();
            }

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ExtensionCanBeParsedCorrectly()
        {
            var expected = ".config";
            var fileName = "web.Debug.config";
            MemoryFile file = new MemoryFile(fileName, null);
            Assert.Equal(expected, file.Extension);
        }

        [Fact]
        public void FilePathWithDirectoryCanBeParsed()
        {
            var path = "a/b/c.txt";
            var expected = "test";
            var dir = new MemoryDirectory("root", null);
            var file = dir.CreateFileFromText(path, expected);
            string actual;
            using (StreamReader reader = new StreamReader(file.StreamProvider.OpenRead()))
            {
                actual = reader.ReadToEnd();
            }

            Assert.Equal(expected, actual);
            Assert.Equal("b", file.Directory.Name);
            Assert.Equal("a", file.Directory.Parent.Name);
            Assert.Equal(dir, file.Directory.Parent.Parent);
        }

        [Fact]
        public void DirectoryPathCanBeParsed()
        {
            var path = "a/b/c";
            var root = new MemoryDirectory("root", null);
            var dir = root.CreateDirectory(path);

            Assert.Equal("c", dir.Name);
            Assert.Equal("b", dir.Parent.Name);
            Assert.Equal("a", dir.Parent.Parent.Name);
            Assert.Equal(root, dir.Parent.Parent.Parent);
        }

        [Fact]
        public void DiskFileCanBeRead()
        {
            string expected = "test";
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var subDir = dir.CreateSubdirectory(expected);
            using (var writer = File.CreateText(Path.Combine(subDir.FullName, "test.html")))
            {
                writer.Write(expected);
            }

            var root = new MemoryDirectory("root", null);
            var file = root.CreateFileFromDisk("test.html", subDir.GetFiles().First());
            string actual;
            using (StreamReader reader = new StreamReader(file.StreamProvider.OpenRead()))
            {
                actual = reader.ReadToEnd();
            }

            Assert.Equal(expected, actual);

            subDir.Delete(true);
        }

        [Fact]
        public void DiskDirectoryCanBeCopied()
        {
            string expected = "test";
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var subDir = dir.CreateSubdirectory(expected);
            using (var writer = File.CreateText(Path.Combine(subDir.FullName, "test.html")))
            {
                writer.Write(expected);
            }
            var subSubDir = subDir.CreateSubdirectory(expected);
            using (var writer = File.CreateText(Path.Combine(subSubDir.FullName, "test.html")))
            {
                writer.Write(expected);
            }

            var root = new MemoryDirectory("root", null);
            root.CopyFromDisk(subDir);

            Assert.True(root.DirectoryExists(expected));

            var file = root.FindFile("test.html");
            Assert.Equal(expected, file.ReadAsString());

            file = root.FindDirectory(expected).FindFile("test.html");
            Assert.Equal(expected, file.ReadAsString());

            subDir.Delete(true);
        }

        [Fact]
        public void DirectoryCanBeCopiedToDisk()
        {
            string expected = "test";
            string expectedFileName = "test.html";
            var root = new MemoryDirectory("root", null);
            root.CreateFileFromText(expectedFileName, expected);
            var dir = root.CreateDirectory(expected);
            dir.CreateFileFromText(expectedFileName, expected);

            var diskDir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "root"));
            root.CopyToDisk(diskDir);

            Assert.True(Directory.Exists(Path.Combine(diskDir.FullName, expected)));
            Assert.True(File.Exists(Path.Combine(diskDir.FullName, expectedFileName)));
            Assert.True(File.Exists(Path.Combine(diskDir.FullName, expected, expectedFileName)));

            string content;
            using (var reader = new StreamReader(Path.Combine(diskDir.FullName, expectedFileName)))
            {
                content = reader.ReadToEnd();
            }
            Assert.Equal(expected, content);

            using (var reader = new StreamReader(Path.Combine(diskDir.FullName, expected, expectedFileName)))
            {
                content = reader.ReadToEnd();
            }
            Assert.Equal(expected, content);
            diskDir.Delete(true);
        }
    }
}
