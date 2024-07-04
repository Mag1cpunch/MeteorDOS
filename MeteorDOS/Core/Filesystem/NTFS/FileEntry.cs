using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorDOS.Core.Filesystem.NTFS
{
    public struct FileEntry
    {
        public ulong CreationDate;
        public ulong LastWrittenDate;
        public ulong MFTEntryLastWrittenDate;
        public ulong LastAccessDate;
        public uint FileAttributeFlags;
        public uint MajorVersion;
        public uint MinorVersion;
        public uint ClassIdentifier;
        public uint OwnerIdentifier;
        public uint SecurityDescriptorIdentifier;
        public ulong QuotaCharged;
        public ulong UpdateSequenceNumber;
    }
}
