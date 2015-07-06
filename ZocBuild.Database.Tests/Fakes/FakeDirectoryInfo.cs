using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Tests.Fakes
{
    class FakeDirectoryInfo : DirectoryInfoBase
    {
        public List<FileInfoBase> Files = new List<FileInfoBase>();
        private readonly string _name;
        private readonly FakeDirectoryInfo _parent;

        public FakeDirectoryInfo(string name)
        {
            _name = name;
        }

        public FakeDirectoryInfo(FakeDirectoryInfo parent, string name)
        {
            _parent = parent;
            _name = name;
        }

        public override void Delete()
        {
            throw new NotImplementedException();
        }

        public override void Refresh()
        {
            throw new NotImplementedException();
        }

        public override FileAttributes Attributes { get; set; }
        public override DateTime CreationTime { get; set; }
        public override DateTime CreationTimeUtc { get; set; }

        public override bool Exists
        {
            get { throw new NotImplementedException(); }
        }

        public override string Extension
        {
            get { throw new NotImplementedException(); }
        }

        public override string FullName
        {
            get { throw new NotImplementedException(); }
        }

        public override DateTime LastAccessTime { get; set; }
        public override DateTime LastAccessTimeUtc { get; set; }
        public override DateTime LastWriteTime { get; set; }
        public override DateTime LastWriteTimeUtc { get; set; }

        public override string Name
        {
            get { return _name; }
        }

        public override void Create()
        {
            throw new NotImplementedException();
        }

        public override void Create(DirectorySecurity directorySecurity)
        {
            throw new NotImplementedException();
        }

        public override DirectoryInfoBase CreateSubdirectory(string path)
        {
            throw new NotImplementedException();
        }

        public override DirectoryInfoBase CreateSubdirectory(string path, DirectorySecurity directorySecurity)
        {
            throw new NotImplementedException();
        }

        public override void Delete(bool recursive)
        {
            throw new NotImplementedException();
        }

        public override DirectorySecurity GetAccessControl()
        {
            throw new NotImplementedException();
        }

        public override DirectorySecurity GetAccessControl(AccessControlSections includeSections)
        {
            throw new NotImplementedException();
        }

        public override DirectoryInfoBase[] GetDirectories()
        {
            throw new NotImplementedException();
        }

        public override DirectoryInfoBase[] GetDirectories(string searchPattern)
        {
            throw new NotImplementedException();
        }

        public override DirectoryInfoBase[] GetDirectories(string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }

        public override FileInfoBase[] GetFiles()
        {
            return Files.ToArray();
        }

        public override FileInfoBase[] GetFiles(string searchPattern)
        {
            return Files.ToArray();
        }

        public override FileInfoBase[] GetFiles(string searchPattern, SearchOption searchOption)
        {
            return Files.ToArray();
        }

        public override FileSystemInfoBase[] GetFileSystemInfos()
        {
            throw new NotImplementedException();
        }

        public override FileSystemInfoBase[] GetFileSystemInfos(string searchPattern)
        {
            throw new NotImplementedException();
        }

        public override void MoveTo(string destDirName)
        {
            throw new NotImplementedException();
        }

        public override void SetAccessControl(DirectorySecurity directorySecurity)
        {
            throw new NotImplementedException();
        }

        public override DirectoryInfoBase Parent
        {
            get { return _parent; }
        }

        public override DirectoryInfoBase Root
        {
            get { throw new NotImplementedException(); }
        }
    }
}
