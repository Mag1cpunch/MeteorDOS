using Cosmos.HAL;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MeteorDOS.Core.Filesystem.NTFS
{
    public class NTFSManager
    {
        private static NTFSBootSector bs;
        private static NTFSHeader header;
        private static BlockDevice blockdevice;
        private static MBR mbrdisk;
        private static GPT gptdisk;
        private static MBR.PartInfo mbrpart;
        private static GPT.GPartInfo gptpart;
        private const string Signature = "NTFS\x20\x20\x20\x20";
        private static readonly byte[] BSSignature = new byte[2] { 0x55, 0xAA };
        public static byte[] BootCode;
        public static void Init(Disk disk, int part)
        {
            if (mbrdisk != null || gptdisk != null) 
            {
                throw new AccessViolationException("NTFS is already initialized on another drive");
            }

            blockdevice = disk.Host;

            if (disk.IsMBR) mbrdisk = new MBR(blockdevice);
            else if (!disk.IsMBR) gptdisk = new GPT(blockdevice);
            else throw new Exception("Partitioning style not supported");

            if (mbrdisk != null) 
            {
                if (part < mbrdisk.Partitions.Count)
                {
                    mbrpart = mbrdisk.Partitions[part];
                }
                else throw new Exception("Partition index out of bounds");
                bs = new NTFSBootSector();
                header = new NTFSHeader();
                bs.JMP = new sbyte[3];
                bs.OEM = new char[8];
                byte[] bsdata = new byte[512];
                blockdevice.ReadBlock(mbrpart.StartSector, 1, ref bsdata);
                int signatureindex = 0;
                for (int i = 3; i <= 10; i++)
                {
                    if (bsdata[i] == Signature[signatureindex])
                    {
                        signatureindex++;
                        continue;
                    }
                    throw new Exception("Can't initialize NTFS: Invalid signature");
                }
                byte[] bytespersector = new byte[2];
                BootCode = new byte[426];
                for (int i = 0; i < bsdata.Length; i++) 
                {
                    if (i >= 0 && i <= 2) // Jump instruction
                    {
                        bs.JMP[i] = (sbyte)bsdata[i];
                    }
                    else if (i >= 3 && i <= 10) // OEM name
                    {
                        bs.OEM[i - 3] = (char)bsdata[i];
                    }
                    else if (i == 11) // Bytes per sector
                    {
                        bs.BytesPerSector = BitConverter.ToUInt16(bsdata, i);
                        i += 2;
                    }
                    else if (i == 13) // Sectors per cluster
                    {
                        bs.SectorsPerCluster = (sbyte)bsdata[i];
                    }
                    else if (i == 14) // Reserved sectors
                    {
                        bs.ReservedSectorCount = BitConverter.ToUInt16(bsdata, i);
                        i += 2;
                    }
                    else if (i == 17)
                    {
                        bs.RootEntryCount = BitConverter.ToUInt16(bsdata, i);
                        i += 2;
                    }
                    else if (i == 19)
                    {
                        bs.SectorCount = BitConverter.ToUInt16(bsdata, i);
                        i += 2;
                    }
                    else if (i == 21)
                    {
                        bs.MediaType = (sbyte)bsdata[i];
                    }
                    else if (i == 24)
                    {
                        bs.SectorsPerTrack = BitConverter.ToUInt16(bsdata, i);
                        i += 2;
                    }
                    else if (i == 26)
                    {
                        bs.Heads = BitConverter.ToUInt16(bsdata, i);
                        i += 2;
                    }
                    else if (i == 28)
                    {
                        bs.HiddenSectorCount = BitConverter.ToUInt32(bsdata, i);
                        i += 4;
                    }
                    else if (i == 32)
                    {
                        bs.SectorCount32 = BitConverter.ToUInt32(bsdata, i);
                        i += 4;
                    }
                    else if (i == 40) 
                    {
                        bs.SectorCount64 = BitConverter.ToUInt64(bsdata, i);
                        i += 8;
                    }
                    else if (i == 48)
                    {
                        header.MFTCluster = BitConverter.ToUInt64(bsdata, i);
                        i += 8;
                    }
                    else if (i == 56)
                    {
                        header.MFTMirrorCluster = BitConverter.ToUInt64(bsdata, i);
                        i += 8;
                    }
                    else if (i == 64)
                    {
                        header.MFTEntrySize = bsdata[i];
                    }
                    else if (i == 68)
                    {
                        header.IndexEntrySize = bsdata[i];
                    }
                    else if (i == 72)
                    {
                        header.NTFSVolumeSerialNumber = BitConverter.ToUInt64(bsdata, i);
                        i += 8;
                    }
                    else if (i >= 84 && i <= 509)
                    {
                        BootCode[i - 84] = bsdata[i];
                    }
                    else if (i >= 510 && i <= 511)
                    {
                        if (bsdata[i] == BSSignature[i - 510]) { }
                        else throw new Exception("Can't initialize NTFS: Invalid sector signature");
                    }
                }
                
            }
            else if (gptdisk != null)
            {
                if (part < gptdisk.Partitions.Count)
                {
                    gptpart = gptdisk.Partitions[part];
                }
                else throw new Exception("Partition index out of bounds");
                bs = new NTFSBootSector();
                header = new NTFSHeader();
                bs.JMP = new sbyte[3];
                bs.OEM = new char[8];
                byte[] bsdata = new byte[512];
                blockdevice.ReadBlock(gptpart.StartSector, 1, ref bsdata);
                int signatureindex = 0;
                for (int i = 3; i <= 10; i++)
                {
                    if (bsdata[i] == Signature[signatureindex])
                    {
                        signatureindex++;
                        continue;
                    }
                    throw new Exception("Can't initialize NTFS: Invalid signature");
                }
                byte[] bytespersector = new byte[2];
                BootCode = new byte[426];
                for (int i = 0; i < bsdata.Length; i++)
                {
                    if (i >= 0 && i <= 2) // Jump instruction
                    {
                        bs.JMP[i] = (sbyte)bsdata[i];
                    }
                    else if (i >= 3 && i <= 10) // OEM name
                    {
                        bs.OEM[i - 3] = (char)bsdata[i];
                    }
                    else if (i == 11) // Bytes per sector
                    {
                        bs.BytesPerSector = BitConverter.ToUInt16(bsdata, i);
                        i += 2;
                    }
                    else if (i == 13) // Sectors per cluster
                    {
                        bs.SectorsPerCluster = (sbyte)bsdata[i];
                    }
                    else if (i == 14) // Reserved sectors
                    {
                        bs.ReservedSectorCount = BitConverter.ToUInt16(bsdata, i);
                        i += 2;
                    }
                    else if (i == 17)
                    {
                        bs.RootEntryCount = BitConverter.ToUInt16(bsdata, i);
                        i += 2;
                    }
                    else if (i == 19)
                    {
                        bs.SectorCount = BitConverter.ToUInt16(bsdata, i);
                        i += 2;
                    }
                    else if (i == 21)
                    {
                        bs.MediaType = (sbyte)bsdata[i];
                    }
                    else if (i == 24)
                    {
                        bs.SectorsPerTrack = BitConverter.ToUInt16(bsdata, i);
                        i += 2;
                    }
                    else if (i == 26)
                    {
                        bs.Heads = BitConverter.ToUInt16(bsdata, i);
                        i += 2;
                    }
                    else if (i == 28)
                    {
                        bs.HiddenSectorCount = BitConverter.ToUInt32(bsdata, i);
                        i += 4;
                    }
                    else if (i == 32)
                    {
                        bs.SectorCount32 = BitConverter.ToUInt32(bsdata, i);
                        i += 4;
                    }
                    else if (i == 40)
                    {
                        bs.SectorCount64 = BitConverter.ToUInt64(bsdata, i);
                        i += 8;
                    }
                    else if (i == 48)
                    {
                        header.MFTCluster = BitConverter.ToUInt64(bsdata, i);
                        i += 8;
                    }
                    else if (i == 56)
                    {
                        header.MFTMirrorCluster = BitConverter.ToUInt64(bsdata, i);
                        i += 8;
                    }
                    else if (i == 64)
                    {
                        header.MFTEntrySize = bsdata[i];
                    }
                    else if (i == 68)
                    {
                        header.IndexEntrySize = bsdata[i];
                    }
                    else if (i == 72)
                    {
                        header.NTFSVolumeSerialNumber = BitConverter.ToUInt64(bsdata, i);
                        i += 8;
                    }
                    else if (i >= 84 && i <= 509)
                    {
                        BootCode[i - 84] = bsdata[i];
                    }
                    else if (i >= 510 && i <= 511)
                    {
                        if (bsdata[i] == BSSignature[i - 510]) { }
                        else throw new Exception("Can't initialize NTFS: Invalid sector signature");
                    }
                }

            }
            else throw new Exception("Can't initialize NTFS: Partitioning type is not supported");

        }
        public byte[] ReadSector(uint index)
        {
            byte[] data = new byte[bs.BytesPerSector];
            if (index < bs.SectorCount64)
            {
                blockdevice.ReadBlock(index, 1, ref data);
            }
            else throw new Exception("Can't read from sector: Index out of bounds");
            return data;
        }
        public byte[] WriteSector(uint index, byte[] data)
        {
            if (data.Length > bs.BytesPerSector) 
            {
                throw new Exception($"Can't write to sector: Data can't be bigger than {bs.BytesPerSector} bytes");
            }
            if (index < bs.SectorCount64)
            {
                if (index > 0)
                {
                    blockdevice.WriteBlock(index, 1, ref data);
                }
                else throw new Exception("Can't write to sector: Can't write to boot sector");
            }
            else throw new Exception("Can't write to sector: Index out of bounds");
            return data;
        }
        public byte[] ReadCluster(uint index) 
        {
            byte[] data = new byte[bs.BytesPerSector * bs.SectorsPerCluster];
            if ((ulong)(index + bs.SectorsPerCluster - 1) < bs.SectorCount64)
            {
                blockdevice.ReadBlock(index, (ulong)bs.SectorsPerCluster - 1, ref data);
            }
            else throw new Exception("Can't read from cluster: Index out of bounds");
            return data;
        }
        public void WriteCluster(uint index, byte[] data)
        {
            if (data.Length > bs.BytesPerSector * bs.SectorsPerCluster) throw new Exception($"Can't write to cluster: Data can't be bigger than {bs.BytesPerSector * bs.SectorsPerCluster} bytes");
            if ((ulong)(index + bs.SectorsPerCluster - 1) < bs.SectorCount64)
            {
                if (index > 0) blockdevice.ReadBlock(index, (ulong)bs.SectorsPerCluster - 1, ref data);
                else throw new AccessViolationException("Can't write to cluster: Can't write to boot sector");
            }
            else throw new Exception("Can't read from cluster: Index out of bounds");
        }
        public void Format(Disk disk, int part, byte[] bootcode)
        {
            Console.WriteLine($"Formatting partition {part} with NTFS set as file system...");
            if (bootcode.Length > 426) throw new Exception($"Can't format the disk: Boot code size can't be bigger than 426 bytes");
            else if (bootcode.Length < 426) throw new Exception($"Can't format the disk: Boot code size can't be smaller than 426 bytes");
            MBR mbrdisk = null;
            GPT gptdisk = null;
            MBR.PartInfo mbrpart;
            GPT.GPartInfo gptpart;
            BlockDevice bc = disk.Host;
            if (disk.IsMBR) mbrdisk = new MBR(bc);
            else if (!disk.IsMBR) gptdisk = new GPT(bc);
            else throw new Exception("Can't format the disk: Partitioning type is not supported");
            if (mbrdisk != null) 
            {
                if (part < mbrdisk.Partitions.Count) mbrpart = mbrdisk.Partitions[part];
                else throw new Exception("Can't format the disk: Partition index out of bounds");
                byte[] clusterdata = new byte[4096];
                for (ulong i = 0; i < mbrpart.SectorCount; i++)
                {
                    if (i + 7 > mbrpart.SectorCount - 1) { break; }
                    bc.WriteBlock(i, 8, ref clusterdata);
                }
                byte[] bsdata = new byte[512];
                using (MemoryStream ms = new MemoryStream(bsdata))
                {
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        bw.Write(new byte[3] { 0xEB, 0x52, 0x90 }); // Jump instruction
                        bw.Write(Encoding.ASCII.GetBytes(Signature)); // OEM name
                        bw.Write((ushort)512); // Bytes per sector
                        bw.Write((byte)8); // Sectors per cluster
                        bw.Write((ushort)0); // Reserved Sectors
                        bw.Write((byte)0); // Number of FATs
                        bw.Write((ushort)0); // Root directory entries
                        bw.Write((ushort)0); // Total sectors 16 bit
                        if (disk.Type == BlockDeviceType.HardDrive) bw.Write((byte)0xF8); // Media type
                        else if (disk.Type == BlockDeviceType.RemovableCD) bw.Write((byte)0xF0);
                        else if (disk.Type == BlockDeviceType.Removable) bw.Write((byte)0xF8);
                        else throw new Exception("Can't format the disk: Disk type is not supported");
                        bw.Write((ushort)0); // Sectors per FAT
                        bw.Write((ushort)63); // Sectors per track
                        bw.Write((ushort)255); // Number of heads
                        bw.Write(mbrpart.StartSector);
                        bw.Write((uint)0); // Sector count 32 bit
                        bw.Write((byte)0); // Disc unit number
                        bw.Write((byte)0); // Flags
                        bw.Write((byte)0); // BPB version signature byte
                        bw.Write((byte)0); // Reserved
                        bw.Write((ulong)mbrpart.SectorCount); // Sector count 64 bit
                        bw.Write((ulong)0x04); // MFT cluster
                        bw.Write((ulong)mbrpart.SectorCount - 9); // MFT mirror cluster
                        bw.Write((byte)0x01); // MFT entry size
                        bw.Write((ushort)0); // Empty space
                        bw.Write((byte)0); // Empty space
                        bw.Write((byte)0); // Index entry size
                        bw.Write((ushort)0); // Empty space
                        bw.Write((byte)0); // Empty space
                        bw.Write(GenerateVolumeSerialNumber()); // Volume serial number
                        bw.Write((uint)CalculateChecksum(bsdata)); // Checksum
                        bw.Write(bootcode); // Boot code
                        bw.Write(BSSignature); // Sector signature
                        bw.Close();
                    }
                }
                bc.WriteBlock(0, 1, ref bsdata);
            }
            else if (gptdisk != null)
            {
                if (part < gptdisk.Partitions.Count) gptpart = gptdisk.Partitions[part];
                else throw new Exception("Can't format the disk: Partition index out of bounds");
                byte[] clusterdata = new byte[4096];
                for (ulong i = 0; i < gptpart.SectorCount; i++) 
                {
                    if (i + 7 > gptpart.SectorCount - 1) { break; }
                    bc.WriteBlock(i, 8, ref clusterdata);
                }
                byte[] bsdata = new byte[512];
                using (MemoryStream ms = new MemoryStream(bsdata))
                {
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        bw.Write(new byte[3] { 0xEB, 0x52, 0x90 }); // Jump instruction
                        bw.Write(Encoding.ASCII.GetBytes(Signature)); // OEM name
                        bw.Write((ushort)512); // Bytes per sector
                        bw.Write((byte)8); // Sectors per cluster
                        bw.Write((ushort)0); // Reserved Sectors
                        bw.Write((byte)0); // Number of FATs
                        bw.Write((ushort)0); // Root directory entries
                        bw.Write((ushort)0); // Total sectors 16 bit
                        if (disk.Type == BlockDeviceType.HardDrive) bw.Write((byte)0xF8); // Media type
                        else if (disk.Type == BlockDeviceType.RemovableCD) bw.Write((byte)0xF0);
                        else if (disk.Type == BlockDeviceType.Removable) bw.Write((byte)0xF8);
                        else throw new Exception("Can't format the disk: Disk type is not supported");
                        bw.Write((ushort)0); // Sectors per FAT
                        bw.Write((ushort)63); // Sectors per track
                        bw.Write((ushort)255); // Number of heads
                        bw.Write(gptpart.StartSector);
                        bw.Write((uint)0); // Sector count 32 bit
                        bw.Write((byte)0); // Disc unit number
                        bw.Write((byte)0); // Flags
                        bw.Write((byte)0); // BPB version signature byte
                        bw.Write((byte)0); // Reserved
                        bw.Write((ulong)gptpart.SectorCount); // Sector count 64 bit
                        bw.Write((ulong)0x04); // MFT cluster
                        bw.Write((ulong)gptpart.SectorCount - 9); // MFT mirror cluster
                        bw.Write((byte)0x01); // MFT entry size
                        bw.Write((ushort)0); // Empty space
                        bw.Write((byte)0); // Empty space
                        bw.Write((byte)0); // Index entry size
                        bw.Write((ushort)0); // Empty space
                        bw.Write((byte)0); // Empty space
                        bw.Write(GenerateVolumeSerialNumber()); // Volume serial number
                        bw.Write((uint)CalculateChecksum(bsdata)); // Checksum
                        bw.Write(bootcode); // Boot code
                        bw.Write(BSSignature); // Sector signature
                        bw.Close();
                    }
                }
                bc.WriteBlock(0, 1, ref bsdata);
            }
            Console.WriteLine($"Done!");
        }
        private static ulong GenerateVolumeSerialNumber()
        {
            // Get the current date and time from the RTC
            int year = RTC.Century * 100 + RTC.Year;
            int month = RTC.Month;
            int day = RTC.DayOfTheMonth;
            int hour = RTC.Hour;
            int minute = RTC.Minute;
            int second = RTC.Second;

            // Combine these values into a unique 64-bit serial number
            // This example simply combines them in a specific format for demonstration purposes
            // You might want to use a different method to ensure uniqueness
            ulong serialNumber = (ulong)year << 32 | (ulong)month << 24 | (ulong)day << 16 | (ulong)hour << 8 | (ulong)minute;

            return serialNumber;
        }
        private static uint CalculateChecksum(byte[] bootSector)
        {
            uint checksum = 0;

            for (int i = 0; i < bootSector.Length; i++)
            {
                if (i < 0x50 || i > 0x53) // Skip the checksum field
                {
                    checksum += bootSector[i];
                }
            }

            return checksum;
        }
    }
}
