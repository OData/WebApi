using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebStack.QA.Common.FileSystem
{
    public static class FileSystemExtensions
    {
        public static IFile CreateFile(this IDirectory directory, string name)
        {
            if (directory == null)
            {
                throw new ArgumentNullException("directory");
            }

            IDirectory dir;
            string fileName;
            directory.EnsureDirectory(name, out dir, out fileName);

            var file = new MemoryFile(fileName, dir);
            dir.CreateFile(file);
            return file;
        }

        public static IFile CreateFileFromText(this IDirectory directory, string name, string text)
        {
            if (directory == null)
            {
                throw new ArgumentNullException("directory");
            }

            var file = directory.CreateFile(name);
            using (StreamWriter writer = new StreamWriter(file.StreamProvider.OpenWrite()))
            {
                writer.Write(text);
            }

            return file;
        }

        public static IFile CreateFileFromDisk(this IDirectory directory, string name, FileInfo fileInfo)
        {
            if (directory == null)
            {
                throw new ArgumentNullException("directory");
            }

            IDirectory dir;
            string fileName;
            directory.EnsureDirectory(name, out dir, out fileName);

            var file = new MemoryFile(fileName, dir, new DiskFileStreamProvider(fileInfo));
            dir.CreateFile(file);
            return file;
        }

        public static void CopyFromDisk(this IDirectory directory, DirectoryInfo sourceDir)
        {
            if (directory == null)
            {
                throw new ArgumentNullException("directory");
            }

            if (sourceDir == null)
            {
                return;
            }

            foreach (var fileInfo in sourceDir.GetFiles())
            {
                directory.CreateFileFromDisk(fileInfo.Name, fileInfo);
            }

            foreach (var subDir in sourceDir.GetDirectories())
            {
                var dir = directory.CreateDirectory(subDir.Name) as MemoryDirectory;
                dir.CopyFromDisk(subDir);
            }
        }

        public static void CopyToDisk(this IDirectory directory, DirectoryInfo targetDir)
        {
            if (directory == null)
            {
                throw new ArgumentNullException("directory");
            }

            if (targetDir == null)
            {
                throw new ArgumentNullException("targetDir");
            }

            targetDir.Refresh();
            if (!targetDir.Exists)
            {
                targetDir.Create();
            }

            foreach (var file in directory.GetSubFiles())
            {
                var fileInfo = new FileInfo(Path.Combine(targetDir.FullName, file.Name));
                file.CopyToDisk(fileInfo);
            }

            foreach (var dir in directory.GetSubDirectories())
            {
                var subDirInfo = new DirectoryInfo(Path.Combine(targetDir.FullName, dir.Name));
                dir.CopyToDisk(subDirInfo);
            }
        }

        public static IDirectory FindDirectory(this IDirectory directory, string name)
        {
            if (directory == null)
            {
                throw new ArgumentNullException("directory");
            }

            return directory.GetSubDirectories().FirstOrDefault(d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public static IFile FindFile(this IDirectory directory, string name)
        {
            if (directory == null)
            {
                throw new ArgumentNullException("directory");
            }

            return directory.GetSubFiles().FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public static string ReadAsString(this IFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            using (var reader = new StreamReader(file.StreamProvider.OpenRead()))
            {
                return reader.ReadToEnd();
            }
        }

        public static void WriteString(this IFile file, string text)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            using (var writer = new StreamWriter(file.StreamProvider.OpenWrite()))
            {
                writer.Write(text);
            }
        }

        public static void CopyToDisk(this IFile file, FileInfo targetFile)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            if (targetFile == null)
            {
                throw new ArgumentNullException("targetFile");
            }

            targetFile.Refresh();
            if (targetFile.Exists)
            {
                targetFile.Delete();
            }

            using (var fileStream = targetFile.Create())
            {
                file.StreamProvider.OpenRead().CopyTo(fileStream);
            }
        }

        public static void RemoveEmptyDirectories(this IDirectory directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException("directory");
            }

            IEnumerable<IDirectory> subDirectories = directory.GetSubDirectories().ToList();
            foreach (var subDir in subDirectories)
            {
                RemoveEmptyDirectories(subDir);
            }

            if (directory.Parent != null
                && !directory.GetSubDirectories().Any()
                && !directory.GetSubFiles().Any())
            {
                directory.Parent.RemoveDirectory(directory);
            }
        }

        internal static void EnsureDirectory(this IDirectory directory, string filePath, out IDirectory dir, out string fileName)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            filePath = filePath.Trim('/');

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("filePath");
            }

            var index = filePath.LastIndexOf('/');
            if (index < 0)
            {
                dir = directory;
                fileName = filePath;
            }
            else
            {
                dir = directory.CreateDirectory(filePath.Substring(0, index));
                fileName = filePath.Substring(index + 1);
            }
        }
    }
}
