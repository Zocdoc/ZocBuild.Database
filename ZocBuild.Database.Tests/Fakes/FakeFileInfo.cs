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
    public class FakeFileInfo : FileInfoBase
    {
        private readonly string _fullName;
        private readonly DirectoryInfoBase _directory;

        public FakeFileInfo(string fullname, DirectoryInfoBase directoryInfo)
        {
            _fullName = fullname;
            _directory = directoryInfo;
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
            get { return _fullName; }
        }

        public override DateTime LastAccessTime { get; set; }
        public override DateTime LastAccessTimeUtc { get; set; }
        public override DateTime LastWriteTime { get; set; }
        public override DateTime LastWriteTimeUtc { get; set; }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }

        public override StreamWriter AppendText()
        {
            throw new NotImplementedException();
        }

        public override FileInfoBase CopyTo(string destFileName)
        {
            throw new NotImplementedException();
        }

        public override FileInfoBase CopyTo(string destFileName, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public override Stream Create()
        {
            throw new NotImplementedException();
        }

        public override StreamWriter CreateText()
        {
            throw new NotImplementedException();
        }

        public override void Decrypt()
        {
            throw new NotImplementedException();
        }

        public override void Encrypt()
        {
            throw new NotImplementedException();
        }

        public override FileSecurity GetAccessControl()
        {
            throw new NotImplementedException();
        }

        public override FileSecurity GetAccessControl(AccessControlSections includeSections)
        {
            throw new NotImplementedException();
        }

        public override void MoveTo(string destFileName)
        {
            throw new NotImplementedException();
        }

        public override Stream Open(FileMode mode)
        {
            throw new NotImplementedException();
        }

        public override Stream Open(FileMode mode, FileAccess access)
        {
            throw new NotImplementedException();
        }

        public override Stream Open(FileMode mode, FileAccess access, FileShare share)
        {
            throw new NotImplementedException();
        }

        private Stream _fileContents = null;

        public void SetFileContent(string contents)
        {
            _fileContents = new MemoryStream();
            var writer = new StreamWriter(_fileContents);
            writer.Write(contents);
            writer.Flush();
            _fileContents.Position = 0;
        }

        public override Stream OpenRead()
        {
            return _fileContents ?? new MemoryStream();
        }

        public override StreamReader OpenText()
        {
            throw new NotImplementedException();
        }

        public override Stream OpenWrite()
        {
            throw new NotImplementedException();
        }

        public override FileInfoBase Replace(string destinationFileName, string destinationBackupFileName)
        {
            throw new NotImplementedException();
        }

        public override FileInfoBase Replace(string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
        {
            throw new NotImplementedException();
        }

        public override void SetAccessControl(FileSecurity fileSecurity)
        {
            throw new NotImplementedException();
        }

        public override DirectoryInfoBase Directory
        {
            get { return _directory; }
        }

        public override string DirectoryName
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsReadOnly { get; set; }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }
    }
}
