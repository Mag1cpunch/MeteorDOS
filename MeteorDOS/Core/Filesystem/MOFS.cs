using Cosmos.HAL.BlockDevice;
using System;
using System.Collections.Generic;

namespace MeteorDOS.Core.Filesystem
{
    public struct FSSector
    {
        public ulong SectorStart;
        public ulong SectorLength;
        public ushort TotalSpace;
        public ushort UsedSpace;
    }
    public struct FSCluster
    {
        public ulong ClusterStart;
        public ulong ClusterLength;
        public ushort TotalSpace;
        public ushort UsedSpace;
    }
    public struct FSInfo
    {
        public ulong TotalSectors;
        public ulong TotalClusters;
        public byte SectorsPerCluster;
        public ulong FreeSectors;
    }
    public class MOFS
    {
        public FSSector bs;
        public BlockDevice maindisk;
        public static readonly byte[] Signature = new byte[4] { 0x4D, 0x4F, 0x46, 0x53 };
        public FSInfo fsinfo;
        public MOFS(BlockDevice disk)
        {
            maindisk = disk;
            bs = new FSSector();
            byte[] bsdata = new byte[512];
            Console.WriteLine("Reading Boot Sector...");
            disk.ReadBlock(0, 1, ref bsdata);
            Console.WriteLine("Done!");
            Console.WriteLine("Checking FS signature...");
            for (int i = 0; i < 3; i++) 
            {
                Console.WriteLine($"Checking sector offset {i}...");
                if (bsdata[i] == Signature[i])
                {
                    Console.WriteLine($"Matched");
                    continue;
                }
                throw new Exception("Invalid FS signature, Make sure your disk is formatted with MOFS set as a file system");
            }
            Console.WriteLine("Done!");
            maindisk = disk;
            Console.WriteLine("Setting up boot sector and FSInfo...");
            bs.SectorStart = 0;
            bs.SectorLength = 1;
            bs.TotalSpace = 512;
            foreach (byte b in bsdata)
            {
                if (b > 0) bs.UsedSpace++;
            }
            fsinfo = new FSInfo();
            fsinfo.TotalSectors = maindisk.BlockCount;
            fsinfo.FreeSectors = fsinfo.TotalSectors;
            fsinfo.SectorsPerCluster = bsdata[4];
            fsinfo.TotalClusters = fsinfo.TotalSectors / fsinfo.SectorsPerCluster;
            Console.WriteLine($"Done!");
            Console.Clear();
        }
        public FSCluster[] GetClusters()
        {
            List<FSCluster> clusters = new List<FSCluster>();
            for (ulong i = 0; i < fsinfo.TotalSectors; i++)
            {
                FSCluster cluster = new FSCluster();
                cluster.ClusterStart = i;
                cluster.ClusterLength = (ulong)fsinfo.SectorsPerCluster - 1;
                cluster.TotalSpace = 4096;
                byte[] clusterdata = new byte[4096];
                maindisk.ReadBlock(i, (ulong)fsinfo.SectorsPerCluster - 1 , ref clusterdata);
                foreach (byte b in clusterdata) 
                {
                    if (b > 0) cluster.UsedSpace++;
                }
                clusters.Add(cluster);
            }
            return clusters.ToArray();
        }
        public FSSector[] GetSectors() 
        {
            List<FSSector> sectors = new List<FSSector>();
            for (ulong i = 0; i < fsinfo.TotalSectors; i++)
            {
                FSSector sector = new FSSector();
                sector.SectorStart = i;
                sector.SectorLength = 1;
                sector.TotalSpace = 512;
                byte[] sectordata = new byte[512];
                maindisk.ReadBlock(i, 1, ref sectordata);
                foreach (byte b in sectordata)
                {
                    if (b > 0) sector.UsedSpace++;
                }
                sectors.Add(sector);
            }
            return sectors.ToArray();
        }
        public byte[] ReadCluster(ulong index)
        {
            FSCluster[] clusters = GetClusters();
            if (index < (ulong)clusters.Length) 
            {
                byte[] data = new byte[4096];
                maindisk.ReadBlock(clusters[index].ClusterStart, clusters[index].ClusterLength, ref data);
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
                maindisk.ReadBlock(sectors[index].SectorStart, sectors[index].SectorLength, ref data);
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
                    maindisk.WriteBlock(clusters[index].ClusterStart, clusters[index].ClusterLength, ref data);
                }
                else
                {
                    throw new Exception("Data size cannot be bigger than 4096 bytes");
                }
            }
            else
            {
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
                    maindisk.WriteBlock(sectors[index].SectorStart, sectors[index].SectorLength, ref data);
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
        public static void FormatDisk(BlockDevice disk, byte[] bsdata)
        {
            if (bsdata.Length > 508)
            {
                throw new Exception("Boot Sector Data cannot be bigger than 508 bytes, if there's already an FS signature at the start of the boot sector you should remove it and provide a raw boot sector data with a size of 508 bytes");
            }
            ulong sectors = disk.BlockCount;
            Console.WriteLine("Formatting Hard Drive with MOFS set as file system...");
            byte[] data = new byte[512];
            for (ulong i = 0; i <= sectors; i++) 
            {
                Console.WriteLine($"Cleaning sector {i} / {sectors}...");
                disk.WriteBlock(i, 1, ref data);
                Console.WriteLine("Cleaned");
            }
            Console.WriteLine("Done!");
            byte[] bootsector = new byte[512];
            ushort index = 0;
            Console.WriteLine("Aligning boot sector...");
            for (ushort i = 0; i < 512; i++)
            {
                Console.WriteLine($"Processing boot sector offset {i} / {512}...");
                if (i < 4)
                {
                    bootsector[i] = Signature[i];
                    Console.WriteLine("Processed");
                    continue;
                }
                bootsector[i] = bsdata[index];
                index++;
                Console.WriteLine("Processed");
            }
            Console.WriteLine("Done!");
            Console.WriteLine("Saving boot sector to disk...");
            disk.WriteBlock(0, 1, ref bootsector);
            Console.WriteLine("Done!");
            Console.Clear();
        }
    }
}
