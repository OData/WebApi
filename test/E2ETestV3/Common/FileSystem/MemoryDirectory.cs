using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebStack.QA.Common.FileSystem
{
    /// <summary>
    /// In memory directory which can represent directory structure in memory
    /// </summary>
    public class MemoryDirectory : IDirectory
    {
        private List<IFile> _files = new List<IFile>();
        private List<IDirectory> _subDirs = new List<IDirectory>();

        public MemoryDirectory(string name, IDirectory parent)
        {
            Name = name;
            Parent = parent;
        }

        public string Name
        {
            get;
            set;
        }

        public IDirectory Parent
        {
            get;
            set;
        }

        public string FullName
        {
            get
            {
                if (Parent == null)
                {
                    return Name;
                }
                return Path.Combine(Parent.FullName, Name);
            }
        }

        public IEnumerable<IFile> GetSubFiles()
        {
            return _files;
        }

        public IEnumerable<IDirectory> GetSubDirectories()
        {
            return _subDirs;
        }

        public IDirectory CreateDirectory(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            name = name.Trim('/');
            var index = name.IndexOf('/');
            string dirName, subPath = null;
            if (index > 0)
            {
                dirName = name.Substring(0, index);
                subPath = name.Substring(index + 1);
            }
            else
            {
                dirName = name;
            }

            IDirectory dir;
            if (DirectoryExists(dirName))
            {
                dir = _subDirs.First(d => string.Equals(d.Name, dirName, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                dir = new MemoryDirectory(dirName, this);
                _subDirs.Add(dir);
            }

            if (string.IsNullOrEmpty(subPath))
            {
                return dir;
            }
            else
            {
                return dir.CreateDirectory(subPath);
            }
        }

        public IFile CreateFile(IFile file)
        {
            file.Directory = this;
            _files.Add(file);
            return file;
        }

        public bool DirectoryExists(string name)
        {
            return _subDirs.Any(d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public bool FileExists(string name)
        {
            return _files.Any(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
        }


        public void RemoveDirectory(IDirectory directory)
        {
            _subDirs.Remove(directory);
        }

        public void RemoveFile(IFile file)
        {
            _files.Remove(file);
        }
    }
}
