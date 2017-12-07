﻿// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MicroDOS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Description
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;

namespace DiscImageChef.Filesystems
{
    // Information from http://www.owg.ru/mkt/BK/MKDOS.TXT
    // Thanks to tarlabnor for translating it
    public class MicroDOS : Filesystem
    {
        const ushort magic = 0xA72E;
        const ushort magic2 = 0x530C;

        public MicroDOS()
        {
            Name = "MicroDOS file system";
            PluginUUID = new Guid("9F9A364A-1A27-48A3-B730-7A7122000324");
            CurrentEncoding = Encoding.GetEncoding("koi8-r");
        }

        public MicroDOS(Encoding encoding)
        {
            Name = "MicroDOS file system";
            PluginUUID = new Guid("9F9A364A-1A27-48A3-B730-7A7122000324");
            if(encoding == null)
                CurrentEncoding = Encoding.GetEncoding("koi8-r");
            else
                CurrentEncoding = encoding;
        }

        public MicroDOS(ImagePlugins.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "MicroDOS file system";
            PluginUUID = new Guid("9F9A364A-1A27-48A3-B730-7A7122000324");
            if(encoding == null)
                CurrentEncoding = Encoding.GetEncoding("koi8-r");
            else
                CurrentEncoding = encoding;
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, Partition partition)
        {
            if((1 + partition.Start) >= partition.End)
                return false;

            if(imagePlugin.GetSectorSize() < 512)
                return false;

            MicroDOSBlock0 block0 = new MicroDOSBlock0();

            byte[] bk0 = imagePlugin.ReadSector(0 + partition.Start);

            GCHandle handle = GCHandle.Alloc(bk0, GCHandleType.Pinned);
            block0 = (MicroDOSBlock0)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(MicroDOSBlock0));
            handle.Free();

            return block0.label == magic && block0.mklabel == magic2;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, Partition partition, out string information)
        {
            information = "";

            StringBuilder sb = new StringBuilder();
            MicroDOSBlock0 block0 = new MicroDOSBlock0();

            byte[] bk0 = imagePlugin.ReadSector(0 + partition.Start);

            GCHandle handle = GCHandle.Alloc(bk0, GCHandleType.Pinned);
            block0 = (MicroDOSBlock0)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(MicroDOSBlock0));
            handle.Free();

            sb.AppendLine("MicroDOS filesystem");
            sb.AppendFormat("Volume has {0} blocks ({1} bytes)", block0.blocks, block0.blocks * 512).AppendLine();
            sb.AppendFormat("Volume has {0} blocks used ({1} bytes)", block0.usedBlocks, block0.usedBlocks * 512).AppendLine();
            sb.AppendFormat("Volume contains {0} files", block0.files).AppendLine();
            sb.AppendFormat("First used block is {0}", block0.firstUsedBlock).AppendLine();

            xmlFSType = new Schemas.FileSystemType
            {
                Type = "MicroDOS",
                ClusterSize = 512,
                Clusters = block0.blocks,
                Files = block0.files,
                FilesSpecified = true,
                FreeClusters = block0.blocks - block0.usedBlocks,
                FreeClustersSpecified = true
            };

            information = sb.ToString();
        }

        // Followed by directory entries
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct MicroDOSBlock0
        {
            /// <summary>BK starts booting here</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            public byte[] bootCode;
            /// <summary>Number of files in directory</summary>
            public ushort files;
            /// <summary>Total number of blocks in files of the directory</summary>
            public ushort usedBlocks;
            /// <summary>Unknown</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 228)]
            public byte[] unknown;
            /// <summary>Ownership label (label that shows it belongs to Micro DOS format)</summary>
            public ushort label;
            /// <summary>MK-DOS directory format label</summary>
            public ushort mklabel;
            /// <summary>Unknown</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public byte[] unknown2;
            /// <summary>Disk size in blocks (absolute value for the system unlike NORD, NORTON etc.) that
            /// doesn't use two fixed values 40 or 80 tracks, but i.e. if you drive works with 76 tracks
            /// this field will contain an appropriate number of blocks</summary>
            public ushort blocks;
            /// <summary> Number of the first file's block. Value is changable</summary>
            public ushort firstUsedBlock;
            /// <summary>Unknown</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] unknown3;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DirectoryEntry
        {
            /// <summary>File status</summary>
            public byte status;
            /// <summary>Directory number (0 - root)</summary>
            public byte directory;
            /// <summary>File name 14. symbols in ASCII KOI8</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] filename;
            /// <summary>Block number</summary>
            public ushort blockNo;
            /// <summary>Length in blocks</summary>
            public ushort blocks;
            /// <summary>Address</summary>
            public ushort address;
            /// <summary>Length</summary>
            public ushort length;
        }

        enum FileStatus : byte
        {
            CommonFile = 0,
            Protected = 1,
            LogicalDisk = 2,
            BadFile = 0x80,
            Deleted = 0xFF
        }

        public override Errno Mount()
        {
            return Errno.NotImplemented;
        }

        public override Errno Mount(bool debug)
        {
            return Errno.NotImplemented;
        }

        public override Errno Unmount()
        {
            return Errno.NotImplemented;
        }

        public override Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            return Errno.NotImplemented;
        }

        public override Errno ListXAttr(string path, ref List<string> xattrs)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadDir(string path, ref List<string> contents)
        {
            return Errno.NotImplemented;
        }

        public override Errno StatFs(ref FileSystemInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno Stat(string path, ref FileEntryInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadLink(string path, ref string dest)
        {
            return Errno.NotImplemented;
        }
    }
}