using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MeteorDOS.Core.Filesystem
{
    public struct FSSector
    {
        public ulong SectorStart;
        public ulong SectorLength;
        public ushort TotalSpace;
    }
    public struct FSCluster
    {
        public ulong ClusterStart;
        public ulong ClusterLength;
        public ushort TotalSpace;
    }
    public struct FSInfo
    {
        public ulong TotalSectors;
        public ulong TotalClusters;
        public byte SectorsPerCluster;
        public ulong FreeSectors;
    }
    public struct DirectoryEntry
    {
        public byte[] Name;
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
                SetupBootSector(mbrpart.StartSector, mbrpart.SectorCount);
            }
            else if (gptpart != null)
            {
                blockdisk.ReadBlock(gptpart.StartSector, 1, ref bsdata);
                ValidateSignature(bsdata);
                SetupBootSector(gptpart.StartSector, gptpart.SectorCount);
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

        private void SetupBootSector(ulong startSector, ulong sectorCount)
        {
            bs.SectorStart = startSector;
            bs.SectorLength = 1;
            bs.TotalSpace = 512;
            fsinfo = new FSInfo
            {
                TotalSectors = sectorCount,
                FreeSectors = sectorCount - 1 // Subtract 1 for boot sector
            };
        }
        //public FSCluster[] GetClusters()
        //{
        //    List<FSCluster> clusters = new List<FSCluster>();
        //    for (ulong i = 0; i < fsinfo.TotalSectors; i++)
        //    {
        //        FSCluster cluster = new FSCluster();
        //        cluster.ClusterStart = i;
        //        cluster.ClusterLength = (ulong)fsinfo.SectorsPerCluster - 1;
        //        cluster.TotalSpace = 4096;
        //        byte[] clusterdata = new byte[4096];
        //        blockdisk.ReadBlock(i, (ulong)fsinfo.SectorsPerCluster - 1 , ref clusterdata);
        //        foreach (byte b in clusterdata) 
        //        {
        //            if (b > 0) cluster.UsedSpace++;
        //        }
        //        clusters.Add(cluster);
        //    }
        //    return clusters.ToArray();
        //}
        public FSSector[] GetSectors()
        {
            List<FSSector> sectors = new List<FSSector>();
            for (ulong i = 0; i < fsinfo.TotalSectors; i++)
            {
                sectors.Add(new FSSector { SectorStart = i, SectorLength = 1, TotalSpace = 512 });
            }
            return sectors.ToArray();
        }
        //public byte[] ReadCluster(ulong index)
        //{
        //    FSCluster[] clusters = GetClusters();
        //    if (index < (ulong)clusters.Length) 
        //    {
        //        byte[] data = new byte[4096];
        //        maindisk.ReadBlock(clusters[index].ClusterStart, clusters[index].ClusterLength, ref data);
        //        return data;
        //    }
        //    else
        //    {
        //        throw new IndexOutOfRangeException("Cannot read from cluster: Index out of bounds");
        //    }
        //}
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
        //public void WriteCluster(ulong index, byte[] data)
        //{
        //    FSCluster[] clusters = GetClusters();
        //    if (index < (ulong)clusters.Length)
        //    {
        //        if (data.Length <= 4096) 
        //        {
        //            maindisk.WriteBlock(clusters[index].ClusterStart, clusters[index].ClusterLength, ref data);
        //        }
        //        else
        //        {
        //            throw new Exception("Data size cannot be bigger than 4096 bytes");
        //        }
        //    }
        //    else
        //    {
        //        throw new IndexOutOfRangeException("Cannot write to cluster: Index out of bounds");
        //    }
        //}
        public void WriteSector(ulong index, byte[] data)
        {
            FSSector[] sectors = GetSectors();
            if (index < (ulong)sectors.Length)
            {
                if (data.Length <= 512)
                {
                    blockdisk.WriteBlock(sectors[index].SectorStart, sectors[index].SectorLength, ref data);
                }
                else
                {
                    throw new Exception("Data size cannot be bigger than 512 bytes");
                }
            }
            else
            {
                throw new IndexOutOfRangeException("Cannot write to sector: Index out of bounds");
            }
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

            if (disk.IsMBR)
            {
                MBR mbrdisk = new MBR(blockdevice);
                if (part < mbrdisk.Partitions.Count)
                {
                    var mbrpart = mbrdisk.Partitions[part];
                    sectors = mbrpart.SectorCount;
                    startSector = mbrpart.StartSector;
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

            byte[] bootsector = new byte[512];
            using (MemoryStream stream = new MemoryStream(bootsector))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Signature);
                    writer.Write(8);
                    writer.Write(sectors);
                }
            }

            blockdevice.WriteBlock(startSector, 1, ref bootsector);
        }
    }
}
