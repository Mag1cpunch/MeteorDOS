using Cosmos.HAL;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MeteorDOS.Core.Filesystem
{
    public enum DirectoryEntryType
    {
        Directory = 0,
        File = 1,
    }
    public enum Permissions
    {
        None = 0,
        Read = 1,
        Write = 2,
        ReadWrite = 3,
    }
    public struct FSSector
    {
        public ulong SectorStart;
        public ulong SectorLength;
    }
    public struct FSCluster
    {
        public ulong ClusterStart;
        public ulong ClusterLength;
    }
    public struct FSInfo
    {
        public ulong TotalSectors;
        public ulong TotalClusters;
        public ulong TotalSuperClusters;
        public byte SectorsPerCluster;
        public ulong FreeSectors;
        public byte ClustersPerSuperCluster;
        public ulong UsedEntryBlocks;
        public ulong UsedMetadataBlocks;
        public ulong UsedDataBlocks;
    }
    public struct DirectoryEntry
    {
        public char[] Name;
        public byte Type;
        public ulong Size;
        public byte Permissions;
        public ulong CreationDate;
        public ulong ModificationDate;
        public ulong AccessDate;
        public ulong MetadataOffset;
    }
    public struct MetadataBlock
    {
        public ulong Id;
        public byte[] ParentDirectoryName;
        public ulong Version;
        public ulong DataBlockStartOffset;
        public ulong DataBlockSize;
        public ulong DataBlockCount;
        public ulong DirectoryEntryStartOffset;
        public byte AllocationType;
    }
    public struct DataBlock
    {
        public byte[] Data;
        public ulong MetadataStartOffset;
    }
    public class MOFS
    {
        public FSSector bs;
        public Disk maindisk;
        public BlockDevice blockdisk;
        public static readonly byte[] Signature = new byte[4] { 0x4D, 0x4F, 0x46, 0x53 };
        public FSInfo fsinfo;
        MBR mbrdisk;
        GPT gptdisk;
        MBR.PartInfo mbrpart;
        GPT.GPartInfo gptpart;
        private ulong RootEntriesCount;
        private const double DIRECTORY_ENTRIES_PROPORTION = 0.10; // 10% for directory entries
        private const double METADATA_BLOCKS_PROPORTION = 0.20; // 20% for metadata blocks
        private const double DATA_BLOCKS_PROPORTION = 0.70; // 70% for data blocks
        private long DIRECTORY_ENTRIES_SIZE;
        private long METADATA_BLOCKS_SIZE;
        private long DATA_BLOCKS_SIZE;
        ulong DIRECTORY_ENTRIES_START_OFFSET;
        ulong DIRECTORY_ENTRIES_END_OFFSET;
        ulong METADATA_BLOCKS_START_OFFSET;
        ulong METADATA_BLOCKS_END_OFFSET;
        ulong DATA_BLOCKS_START_OFFSET;
        ulong DATA_BLOCKS_END_OFFSET;
        public MOFS(Disk disk, int part)
        {
            maindisk = disk;
            blockdisk = disk.Host;
            if (maindisk.IsMBR)
            {
                mbrdisk = new MBR(blockdisk);
                if (part < mbrdisk.Partitions.Count)
                {
                    mbrpart = mbrdisk.Partitions[part];
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Partition index out of range");
                }
            }
            else if (!maindisk.IsMBR)
            {
                gptdisk = new GPT(blockdisk);
                if (part < gptdisk.Partitions.Count)
                {
                    gptpart = gptdisk.Partitions[part];
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Partition index out of range");
                }
            }
            else
            {
                throw new NotSupportedException("Disk partitioning style not supported");
            }
            InitializeBootSector();
        }

        private void InitializeBootSector()
        {
            bs = new FSSector();
            byte[] bsdata = new byte[512];
            if (mbrpart != null)
            {
                blockdisk.ReadBlock(mbrpart.StartSector, 1, ref bsdata);
                ValidateSignature(bsdata);
                SetupBootSector(mbrpart.StartSector, mbrpart.SectorCount, bsdata);
                DIRECTORY_ENTRIES_SIZE = (long)(mbrpart.SectorCount * 512 * DIRECTORY_ENTRIES_PROPORTION);
                METADATA_BLOCKS_SIZE = (long)(mbrpart.SectorCount * 512 * METADATA_BLOCKS_PROPORTION);
                DATA_BLOCKS_SIZE =  mbrpart.SectorCount * 512 - DIRECTORY_ENTRIES_SIZE - METADATA_BLOCKS_SIZE;
                DIRECTORY_ENTRIES_START_OFFSET = 1;
                DIRECTORY_ENTRIES_END_OFFSET = DIRECTORY_ENTRIES_START_OFFSET + ((ulong)DIRECTORY_ENTRIES_SIZE / 512) - 1;
                METADATA_BLOCKS_START_OFFSET = DIRECTORY_ENTRIES_END_OFFSET + 1;
                METADATA_BLOCKS_END_OFFSET = METADATA_BLOCKS_START_OFFSET + ((ulong)METADATA_BLOCKS_SIZE / 512) - 1;
                DATA_BLOCKS_START_OFFSET = METADATA_BLOCKS_END_OFFSET + 1;
                DATA_BLOCKS_END_OFFSET = DATA_BLOCKS_START_OFFSET + ((ulong)DATA_BLOCKS_SIZE / 512) - 1;
            }
            else if (gptpart != null)
            {
                blockdisk.ReadBlock(gptpart.StartSector, 1, ref bsdata);
                ValidateSignature(bsdata);
                SetupBootSector(gptpart.StartSector, gptpart.SectorCount, bsdata);
                DIRECTORY_ENTRIES_SIZE = (long)(gptpart.SectorCount * 512 * DIRECTORY_ENTRIES_PROPORTION - 512);
                METADATA_BLOCKS_SIZE = (long)(gptpart.SectorCount * 512 * METADATA_BLOCKS_PROPORTION - 512);
                DATA_BLOCKS_SIZE = (long)gptpart.SectorCount * 512 - DIRECTORY_ENTRIES_SIZE - METADATA_BLOCKS_SIZE - 512;
                DIRECTORY_ENTRIES_START_OFFSET = 1;
                DIRECTORY_ENTRIES_END_OFFSET = DIRECTORY_ENTRIES_START_OFFSET + ((ulong)DIRECTORY_ENTRIES_SIZE / 512) - 1;
                METADATA_BLOCKS_START_OFFSET = DIRECTORY_ENTRIES_END_OFFSET + 1;
                METADATA_BLOCKS_END_OFFSET = METADATA_BLOCKS_START_OFFSET + ((ulong)METADATA_BLOCKS_SIZE / 512) - 1;
                DATA_BLOCKS_START_OFFSET = METADATA_BLOCKS_END_OFFSET + 1;
                DATA_BLOCKS_END_OFFSET = DATA_BLOCKS_START_OFFSET + ((ulong)DATA_BLOCKS_SIZE / 512) - 1;
            }
        }

        private void ValidateSignature(byte[] data)
        {
            for (int i = 0; i < 3; i++)
            {
                if (data[i] != Signature[i])
                {
                    throw new Exception("Invalid FS signature. Ensure your disk is formatted with MOFS.");
                }
            }
        }

        private void SetupBootSector(ulong startSector, ulong sectorCount, byte[] bootsector)
        {
            bs.SectorStart = startSector;
            bs.SectorLength = 1;
            fsinfo = new FSInfo
            {
                TotalSectors = sectorCount,
                FreeSectors = sectorCount - 1 // Subtract 1 for boot sector
            };
            for (int i = 4; i < bootsector.Length; i++) 
            {
                if (i == 4) fsinfo.SectorsPerCluster = bootsector[i];
                else if (i == 5) fsinfo.ClustersPerSuperCluster = bootsector[i];
                else if (i == 6) fsinfo.TotalSectors = (ulong)BitConverter.ToInt64(bootsector, i);
                else if (i == 14) fsinfo.TotalClusters = (ulong)BitConverter.ToInt64(bootsector, i);
                else if (i == 22) fsinfo.TotalSuperClusters = (ulong)BitConverter.ToInt64(bootsector, i);
            }
        }
        public FSCluster[] GetSuperClusters()
        {
            List<FSCluster> clusters = new List<FSCluster>();
            for (ulong i = 1; i < fsinfo.TotalSectors; i++)
            {
                if (i + 7 > fsinfo.TotalSectors) break;
                FSCluster cluster = new FSCluster();
                cluster.ClusterStart = i;
                cluster.ClusterLength = (ulong)fsinfo.SectorsPerCluster * fsinfo.ClustersPerSuperCluster;
                clusters.Add(cluster);
            }
            return clusters.ToArray();
        }
        public FSCluster[] GetClusters()
        {
            List<FSCluster> clusters = new List<FSCluster>();
            for (ulong i = 1; i < fsinfo.TotalSectors; i++)
            {
                if (i + 7 > fsinfo.TotalSectors) break;
                FSCluster cluster = new FSCluster();
                cluster.ClusterStart = i;
                cluster.ClusterLength = (ulong)fsinfo.SectorsPerCluster;
                clusters.Add(cluster);
            }
            return clusters.ToArray();
        }
        public FSSector[] GetSectors()
        {
            List<FSSector> sectors = new List<FSSector>();
            for (ulong i = 0; i < fsinfo.TotalSectors; i++)
            {
                sectors.Add(new FSSector { SectorStart = i, SectorLength = 1});
            }
            return sectors.ToArray();
        }
        public byte[] ReadCluster(ulong index)
        {
            FSCluster[] clusters = GetClusters();
            if (index < (ulong)clusters.Length)
            {
                byte[] data = new byte[4096];
                blockdisk.ReadBlock(clusters[index].ClusterStart, clusters[index].ClusterLength, ref data);
                return data;
            }
            else
            {
                throw new IndexOutOfRangeException("Cannot read from cluster: Index out of bounds");
            }
        }
        public byte[] ReadSector(ulong index)
        {
            FSSector[] sectors = GetSectors();
            if (index < (ulong)sectors.Length)
            {
                byte[] data = new byte[512];
                blockdisk.ReadBlock(sectors[index].SectorStart, sectors[index].SectorLength, ref data);
                return data;
            }
            else
            {
                throw new IndexOutOfRangeException("Cannot read from sector: Index out of bounds");
            }
        }
        public void WriteCluster(ulong index, byte[] data)
        {
            FSCluster[] clusters = GetClusters();
            if (index < (ulong)clusters.Length)
            {
                if (data.Length <= 4096)
                {
                    blockdisk.WriteBlock(clusters[index].ClusterStart, clusters[index].ClusterLength, ref data);
                    clusters = null;
                }
                else
                {
                    clusters = null;
                    throw new Exception("Data size cannot be bigger than 4096 bytes");
                }
            }
            else
            {
                clusters = null;
                throw new IndexOutOfRangeException("Cannot write to cluster: Index out of bounds");
            }
        }
        public void WriteSector(ulong index, byte[] data)
        {
            FSSector[] sectors = GetSectors();
            if (index < (ulong)sectors.Length)
            {
                if (data.Length <= 512)
                {
                    blockdisk.WriteBlock(sectors[index].SectorStart, sectors[index].SectorLength, ref data);
                    sectors = null;
                }
                else
                {
                    sectors = null;
                    throw new Exception("Data size cannot be bigger than 512 bytes");
                }
            }
            else
            {
                sectors = null;
                throw new IndexOutOfRangeException("Cannot write to sector: Index out of bounds");
            }
        }
        public byte[] ReadSuperCluster(ulong index)
        {
            FSCluster[] clusters = GetSuperClusters();
            if (index < (ulong)clusters.Length)
            {
                byte[] data = new byte[32768];
                blockdisk.ReadBlock(clusters[index].ClusterStart, clusters[index].ClusterLength, ref data);
                return data;
            }
            else
            {
                throw new IndexOutOfRangeException("Cannot read from cluster: Index out of bounds");
            }
        }
        public void WriteSuperCluster(ulong index, byte[] data)
        {
            FSCluster[] clusters = GetSuperClusters();
            if (index < (ulong)clusters.Length)
            {
                if (data.Length <= 32768)
                {
                    blockdisk.WriteBlock(clusters[index].ClusterStart, clusters[index].ClusterLength, ref data);
                    clusters = null;
                }
                else
                {
                    clusters = null;
                    throw new Exception("Data size cannot be bigger than 32768 bytes");
                }
            }
            else
            {
                clusters = null;
                throw new IndexOutOfRangeException("Cannot write to supercluster: Index out of bounds");
            }
        }
        public (ulong, ushort) GetFreeDataOffset()
        {
            ulong start = DATA_BLOCKS_START_OFFSET;
            ulong end = DATA_BLOCKS_END_OFFSET;
            ulong offset = 296 * fsinfo.UsedDataBlocks;

            for (ulong i = start; i <= end; i++)
            {
                byte[] data = ReadSector(i);
                for (ushort j = 0; j < data.Length; j += 296)
                {
                    if (j > 512) break;
                    bool isFree = true;
                    for (ushort k = 0; k < 296; k++)
                    {
                        if (data[j + k] != 0)
                        {
                            isFree = false;
                            break;
                        }
                    }
                    if (isFree)
                    {
                        return (i, j);
                    }
                }
            }
            throw new Exception();
        }
        public (ulong, ushort) GetFreeMetadataOffset()
        {
            ulong start = METADATA_BLOCKS_START_OFFSET;
            ulong end = METADATA_BLOCKS_END_OFFSET;
            ulong offset = 296 * fsinfo.UsedMetadataBlocks;

            for (ulong i = start; i <= end; i++)
            {
                byte[] data = ReadSector(i);
                for (ushort j = 0; j < data.Length; j += 296)
                {
                    if (j > 512) break;
                    bool isFree = true;
                    for (ushort k = 0; k < 296; k++)
                    {
                        if (data[j + k] != 0)
                        {
                            isFree = false;
                            break;
                        }
                    }
                    if (isFree)
                    {
                        return (i, j);
                    }
                }
            }
            throw new Exception();
        }
        public (ulong, ushort) GetFreeDirectoryEntryOffset()
        {
            ulong start = DIRECTORY_ENTRIES_START_OFFSET;
            ulong end = DIRECTORY_ENTRIES_END_OFFSET;
            ulong offset = 297 * fsinfo.UsedEntryBlocks;

            for (ulong i = start; i <= end; i++)
            {
                byte[] data = ReadSector(i);
                for (ushort j = 0; j < data.Length; j += 297)
                {
                    if (j > 512) break;
                    bool isFree = true;
                    for (ushort k = 0; k < 297; k++)
                    {
                        if (data[j + k] != 0)
                        {
                            isFree = false;
                            break;
                        }
                    }
                    if (isFree)
                    {
                        return (i, j);
                    }
                }
            }
            throw new Exception();
        }
        public ulong AddDirectoryEntry(string name, DirectoryEntryType type, Permissions permissions)
        {
            if (name.Length > 255) throw new Exception("Directory name cannot be longer than 255 characters");
            (ulong cluster, ushort startoffset) metadataoffset = (0, 0);
            (ulong cluster, ushort startoffset) direntryoffset = (0, 0);
            try
            {
                metadataoffset = GetFreeMetadataOffset();
                direntryoffset = GetFreeDirectoryEntryOffset();
            }
            catch (Exception ex) { }
            char[] namechars = new char[255];
            for (byte i = 0; i < name.Length; i++)
            {
                namechars[i] = name[i];
            }
            byte[] direntry = new byte[297];
            using (MemoryStream ms = new MemoryStream(direntry))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(namechars); // Name
                    bw.Write((byte)type); // Type
                    bw.Write((ulong)0); // Size
                    bw.Write((byte)permissions); // Permissions
                    bw.Write(PackRTC(RTC.Year, RTC.Month, RTC.DayOfTheMonth, RTC.Hour, RTC.Minute, RTC.Second)); // Creation time
                    bw.Write(PackRTC(RTC.Year, RTC.Month, RTC.DayOfTheMonth, RTC.Hour, RTC.Minute, RTC.Second)); // Modification time
                    bw.Write(PackRTC(RTC.Year, RTC.Month, RTC.DayOfTheMonth, RTC.Hour, RTC.Minute, RTC.Second)); // Last access time
                    bw.Write(metadataoffset.Item1); // Metadata block offset

                }
            }
        }
        private void WriteDirectory(string name, bool IsParent, string parentdirectoryname, bool ParentIsSubdirectory = false)
        {
            if (!IsParent)
            {
                if (name.Length > 255) throw new Exception("Directory name cannot be longer than 255 characters");
                byte[] data = new byte[4096];
                using (MemoryStream ms = new MemoryStream(data))
                {
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        bw.Write(name);
                        bw.Write((byte)0);
                        bw.Write((ulong)data.Length);
                        bw.Write((byte)0);
                        bw.Write(PackRTC(RTC.Year, RTC.Month, RTC.DayOfTheMonth, RTC.Hour, RTC.Minute, RTC.Second));
                        bw.Write(PackRTC(RTC.Year, RTC.Month, RTC.DayOfTheMonth, RTC.Hour, RTC.Minute, RTC.Second));
                        bw.Write(PackRTC(RTC.Year, RTC.Month, RTC.DayOfTheMonth, RTC.Hour, RTC.Minute, RTC.Second));

                    }
                }
            }
        }
        private ulong PackRTC(int year, int month, int day, int hour, int minute, int second)
        {
            ulong packedValue = 0;
            packedValue |= ((ulong)year << 48);    // 16 bits for year
            packedValue |= ((ulong)month << 44);   // 4 bits for month
            packedValue |= ((ulong)day << 39);     // 5 bits for day
            packedValue |= ((ulong)hour << 34);    // 5 bits for hour
            packedValue |= ((ulong)minute << 28);  // 6 bits for minute
            packedValue |= ((ulong)second << 22);  // 6 bits for second
            return packedValue;
        }
        private void UnpackRTC(ulong packedValue, out int year, out int month, out int day, out int hour, out int minute, out int second)
        {
            year = (int)((packedValue >> 48) & 0xFFFF);      // Extracting the year (16 bits)
            month = (int)((packedValue >> 44) & 0xF);        // Extracting the month (4 bits)
            day = (int)((packedValue >> 39) & 0x1F);         // Extracting the day (5 bits)
            hour = (int)((packedValue >> 34) & 0x1F);        // Extracting the hour (5 bits)
            minute = (int)((packedValue >> 28) & 0x3F);      // Extracting the minute (6 bits)
            second = (int)((packedValue >> 22) & 0x3F);      // Extracting the second (6 bits)
        }

        public (ulong, ulong) GetFreeCluster()
        {
            byte[] data;
            byte freesectors = 0;
            for (ulong i = 0; i < fsinfo.TotalClusters; i++)
            {
                data = ReadCluster(i);
                for (ushort j = 0; j < data.Length; j++) 
                {
                    if (freesectors == 8) return (i, 8);
                    if (data[j] == 0)
                    {
                        freesectors++;
                        j += 512;
                    }
                    else
                    {
                        freesectors = 0;
                        break;
                    }
                }
            }
            return (0, 0);
        }
        public bool IsSectorUsed(ulong index)
        {
            FSSector[] sectors = GetSectors();
            if (index < (ulong)sectors.Length) {
                byte[] data = new byte[512];
                blockdisk.ReadBlock(sectors[index].SectorStart, sectors[index].SectorLength, ref data);
                foreach (byte b in data)
                {
                    if (b > 0) return true;
                }
            }
            else
            {
                throw new IndexOutOfRangeException("Index out of bounds");
            }
            return false;
        }
        public static void FormatDisk(Disk disk, int part, byte[] bsdata)
        {
            if (bsdata.Length > 508)
            {
                throw new Exception("Boot Sector Data cannot be bigger than 508 bytes.");
            }
            BlockDevice blockdevice = disk.Host;
            ulong sectors = 0;
            ulong startSector = 0;
            ulong totalClusters = 0;

            if (disk.IsMBR)
            {
                MBR mbrdisk = new MBR(blockdevice);
                if (part < mbrdisk.Partitions.Count)
                {
                    var mbrpart = mbrdisk.Partitions[part];
                    sectors = mbrpart.SectorCount;
                    startSector = mbrpart.StartSector;
                    totalClusters = sectors / 4096;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Partition index out of range");
                }
            }
            else if (!disk.IsMBR)
            {
                GPT gptdisk = new GPT(blockdevice);
                if (part < gptdisk.Partitions.Count)
                {
                    var gptpart = gptdisk.Partitions[part];
                    sectors = gptpart.SectorCount;
                    startSector = gptpart.StartSector;
                    totalClusters = sectors / 4096;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Partition index out of range");
                }
            }
            else
            {
                throw new NotSupportedException("Partitioning type not supported");
            }
            byte[] data = new byte[4096];
            for (ulong i = 0; i < sectors; i++)
            {
                if (i + 7 > sectors) break;
                blockdevice.WriteBlock(i, 8, ref data);
            }
            byte[] bootsector = new byte[512];
            using (MemoryStream stream = new MemoryStream(bootsector))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Signature); // OEM name
                    writer.Write((byte)8); // SectorsPerCluster
                    writer.Write((byte)8); // ClustersPerSuperCluster
                    writer.Write(sectors); // Total Sectors
                    writer.Write(sectors / 8); // Total Clusters
                    writer.Write(sectors / 8 / 8); // Total SuperClusters
                    writer.Write((ulong)0); // Number of root entries
                }
            }

            blockdevice.WriteBlock(startSector, 1, ref bootsector);
        }
    }
}
