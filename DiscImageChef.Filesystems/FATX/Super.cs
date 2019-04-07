// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Super.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FATX filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles mounting and umounting the FATX filesystem.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using Schemas;
using Marshal = DiscImageChef.Helpers.Marshal;

namespace DiscImageChef.Filesystems.FATX
{
    public partial class XboxFatPlugin
    {
        public Errno Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                           Dictionary<string, string> options)
        {
            Encoding     = Encoding.GetEncoding("iso-8859-15");
            littleEndian = true;

            if(imagePlugin.Info.SectorSize < 512) return Errno.InvalidArgument;

            DicConsole.DebugWriteLine("Xbox FAT plugin", "Reading superblock");

            byte[] sector = imagePlugin.ReadSector(partition.Start);

            superblock = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

            if(superblock.magic == FATX_CIGAM)
            {
                superblock   = Marshal.ByteArrayToStructureBigEndian<Superblock>(sector);
                littleEndian = false;
            }

            if(superblock.magic != FATX_MAGIC) return Errno.InvalidArgument;

            DicConsole.DebugWriteLine("Xbox FAT plugin",
                                      littleEndian ? "Filesystem is little endian" : "Filesystem is big endian");

            int logicalSectorsPerPhysicalSectors = partition.Offset == 0 ? 8 : 1;
            DicConsole.DebugWriteLine("Xbox FAT plugin", "logicalSectorsPerPhysicalSectors = {0}",
                                      logicalSectorsPerPhysicalSectors);

            string volumeLabel = StringHandlers.CToString(superblock.volumeLabel,
                                                          !littleEndian ? Encoding.BigEndianUnicode : Encoding.Unicode,
                                                          true);

            XmlFsType = new FileSystemType
            {
                Type = "FATX filesystem",
                ClusterSize =
                    (int)(superblock.sectorsPerCluster * logicalSectorsPerPhysicalSectors *
                          imagePlugin.Info.SectorSize),
                VolumeName   = volumeLabel,
                VolumeSerial = $"{superblock.id:X8}"
            };
            XmlFsType.Clusters = (long)((partition.End - partition.Start + 1) * imagePlugin.Info.SectorSize /
                                        (ulong)XmlFsType.ClusterSize);

            stat = new FileSystemInfo
            {
                Blocks         = XmlFsType.Clusters,
                FilenameLength = MAX_FILENAME,
                Files          = 0, // Requires traversing all directories
                FreeFiles      = 0,
                Id             = {IsInt = true, Serial32 = superblock.magic},
                PluginId       = Id,
                Type           = littleEndian ? "Xbox FAT" : "Xbox 360 FAT",
                FreeBlocks     = 0 // Requires traversing the FAT
            };

            DicConsole.DebugWriteLine("Xbox FAT plugin", "XmlFsType.ClusterSize",  XmlFsType.ClusterSize);
            DicConsole.DebugWriteLine("Xbox FAT plugin", "XmlFsType.VolumeName",   XmlFsType.VolumeName);
            DicConsole.DebugWriteLine("Xbox FAT plugin", "XmlFsType.VolumeSerial", XmlFsType.VolumeSerial);
            DicConsole.DebugWriteLine("Xbox FAT plugin", "stat.Blocks",            stat.Blocks);
            DicConsole.DebugWriteLine("Xbox FAT plugin", "stat.FilenameLength",    stat.FilenameLength);
            DicConsole.DebugWriteLine("Xbox FAT plugin", "stat.Id",                stat.Id);
            DicConsole.DebugWriteLine("Xbox FAT plugin", "stat.Type",              stat.Type);

            byte[] buffer;
            fatStartSector = FAT_START / imagePlugin.Info.SectorSize + partition.Start;
            uint fatSize;

            DicConsole.DebugWriteLine("Xbox FAT plugin", "fatStartSector", fatStartSector);

            if(stat.Blocks > MAX_XFAT16_CLUSTERS)
            {
                DicConsole.DebugWriteLine("Xbox FAT plugin", "Reading FAT32");

                fatSize = (uint)(stat.Blocks * sizeof(uint) / imagePlugin.Info.SectorSize);
                if((uint)(stat.Blocks        * sizeof(uint) % imagePlugin.Info.SectorSize) > 0) fatSize++;
                DicConsole.DebugWriteLine("Xbox FAT plugin", "FAT is {0} sectors", fatSize);

                buffer = imagePlugin.ReadSectors(fatStartSector, fatSize);

                DicConsole.DebugWriteLine("Xbox FAT plugin", "Casting FAT");
                fat32 = MemoryMarshal.Cast<byte, uint>(buffer).ToArray();
                if(!littleEndian)
                    for(int i = 0; i < fat32.Length; i++)
                        fat32[i] = Swapping.Swap(fat32[i]);

                DicConsole.DebugWriteLine("Xbox FAT plugin", "fat32[0] == FATX32_ID = {0}", fat32[0] == FATX32_ID);
                if(fat32[0] != FATX32_ID) return Errno.InvalidArgument;
            }
            else
            {
                DicConsole.DebugWriteLine("Xbox FAT plugin", "Reading FAT16");

                fatSize = (uint)(stat.Blocks * sizeof(ushort) / imagePlugin.Info.SectorSize);
                if((uint)(stat.Blocks        * sizeof(ushort) % imagePlugin.Info.SectorSize) > 0) fatSize++;
                DicConsole.DebugWriteLine("Xbox FAT plugin", "FAT is {0} sectors", fatSize);

                buffer = imagePlugin.ReadSectors(fatStartSector, fatSize);

                DicConsole.DebugWriteLine("Xbox FAT plugin", "Casting FAT");
                fat16 = MemoryMarshal.Cast<byte, ushort>(buffer).ToArray();
                if(!littleEndian)
                    for(int i = 0; i < fat16.Length; i++)
                        fat16[i] = Swapping.Swap(fat16[i]);

                DicConsole.DebugWriteLine("Xbox FAT plugin", "fat16[0] == FATX16_ID = {0}", fat16[0] == FATX16_ID);
                if(fat16[0] != FATX16_ID) return Errno.InvalidArgument;
            }

            sectorsPerCluster  = (uint)(superblock.sectorsPerCluster * logicalSectorsPerPhysicalSectors);
            this.partition     = partition;
            this.imagePlugin   = imagePlugin;
            firstClusterSector = fatStartSector + fatSize;

            DicConsole.DebugWriteLine("Xbox FAT plugin", "sectorsPerCluster = {0}",  sectorsPerCluster);
            DicConsole.DebugWriteLine("Xbox FAT plugin", "firstClusterSector = {0}", firstClusterSector);

            throw new NotImplementedException();
        }

        public Errno Unmount()
        {
            if(!mounted) return Errno.AccessDenied;

            fat16       = null;
            fat32       = null;
            imagePlugin = null;
            partition   = new Partition();
            mounted     = false;

            return Errno.NoError;
        }

        public Errno StatFs(out FileSystemInfo stat)
        {
            stat = this.stat;

            return !mounted ? Errno.AccessDenied : Errno.NoError;
        }
    }
}