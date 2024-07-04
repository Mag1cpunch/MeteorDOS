using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorDOS.Core.Filesystem.NTFS
{
    public enum MFTEntryFlags
    {
        FILE_RECORD_SEGMENT_IN_USE = 0x0001,
        MFT_RECORD_IN_USE = 0x0001,
        FILE_NAME_INDEX_PRESENT = 0x0002,
        MFT_RECORD_IS_DIRECTORY = 0x0002,
        MFT_RECORD_IN_EXTEND = 0x0004,
        MFT_RECORD_IS_VIEW_INDEX = 0x0008
    }
    public enum MFTAttributeDataFlags
    {
        ATTRIBUTE_IS_COMPRESSED = 0x0001,
        ATTRIBUTE_FLAG_COMPRESSION_MASK = 0x00ff,
        ATTRIBUTE_FLAG_ENCRYPTED = 0x4000,
        ATTRIBUTE_FLAG_SPARSE = 0x8000
    }
    public struct MFTRecord
    {
        public char[] RecordType;
        public ushort UpdateSequenceOffset;
        public ushort UpdateSequenceLength;
        public ulong LogFileSequenceNumber;
        public ushort RecordSequenceNumber;
        public ushort HardLinkCount;
        public ushort AttributesOffset;
        public ushort Flags;
        public uint BytesInUse;
        public uint BytesAllocated;
        public ulong ParentRecordNumber;
        public uint NextAttributeIndex;
        public uint Reserved;
        public ulong RecordNumber;
    }
    public struct MFTAttribute
    {
        public uint AttributeType;
        public uint Size;
        public byte NameLength;
        public ushort NameOffset;
        public ushort AttributeDataFlags;
        public ushort AttributeIdentifier;
    }
}
