using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorDOS.Core.Filesystem.NTFS
{
    public struct NTFSHeader
    {
        public ulong MFTCluster;
        public ulong MFTMirrorCluster;
        public byte MFTEntrySize;
        public byte IndexEntrySize;
        public ulong NTFSVolumeSerialNumber;
        public sbyte ClustersPerRecord;
        public sbyte[] Reserved1;
        public sbyte ClustersPerIndexBuffer;
        public sbyte[] Reserved2;
        public ulong SerialNumber;
        public uint Checksum;
    }
}
