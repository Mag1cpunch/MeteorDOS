using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorDOS.Core.Filesystem.NTFS
{
    public struct NTFSBootSector
    {
        public sbyte[] JMP;
        public char[] OEM;
        public ushort BytesPerSector;
        public sbyte SectorsPerCluster;
        public ushort ReservedSectorCount;
        public sbyte TableCount;
        public ushort RootEntryCount;
        public ushort SectorCount;
        public sbyte MediaType;
        public ushort SectorsPerTable;
        public ushort SectorsPerTrack;
        public ushort Heads;
        public uint HiddenSectorCount;
        public uint SectorCount32;
        public uint Reserved;
        public ulong SectorCount64;
    }
}
