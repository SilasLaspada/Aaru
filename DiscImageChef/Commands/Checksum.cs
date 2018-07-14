// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Checksum.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'checksum' verb.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using CommandAndConquer.CLI.Attributes;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using DiscImageChef.Core;
using Schemas;

namespace DiscImageChef.Commands
{
    public static partial class Image
    {
        // How many sectors to read at once
        const uint SECTORS_TO_READ = 256;

        [CliCommand("checksum", "Checksums an image.")]
        public static void Checksum([CliParameter(                             'i', "Disc image.")] string input,
                                    [CliParameter(                             't', "Checksums each track separately.")]
                                    bool separatedTracks = true, [CliParameter('w', "Checksums the whole disc.")]
                                    bool wholeDisc = true,       [CliParameter('a', "Calculates Adler-32.")]
                                    bool adler32 = true,         [CliParameter('6', "Calculates CRC16.")]
                                    bool crc16 = true,           [CliParameter('c', "Calculates CRC32.")]
                                    bool crc32 = true,           [CliParameter('4', "Calculates CRC64 (ECMA).")]
                                    bool crc64 = false,          [CliParameter('t', "Calculates Fletcher-16.")]
                                    bool fletcher16 = false,     [CliParameter('h', "Calculates Fletcher-32.")]
                                    bool fletcher32 = false,     [CliParameter('m', "Calculates MD5.")] bool md5 = true,
                                    [CliParameter(                        'r', "Calculates RIPEMD160.")]
                                    bool ripemd160 = false, [CliParameter('s', "Calculates SHA1.")]
                                    bool sha1 = true,       [CliParameter('2', "Calculates SHA256.")]
                                    bool sha256 = false,    [CliParameter('3', "Calculates SHA384.")]
                                    bool sha384 = false,    [CliParameter('5', "Calculates SHA512.")]
                                    bool sha512 = false,    [CliParameter('f', "Calculates SpamSum fuzzy hash.")]
                                    bool spamSum = true,    [CliParameter('d', "Shows debug output from plugins.")]
                                    bool debug = false,     [CliParameter('v', "Shows verbose output.")]
                                    bool verbose = false)
        {
            DicConsole.DebugWriteLine("Checksum command", "--debug={0}",            debug);
            DicConsole.DebugWriteLine("Checksum command", "--verbose={0}",          verbose);
            DicConsole.DebugWriteLine("Checksum command", "--separated-tracks={0}", separatedTracks);
            DicConsole.DebugWriteLine("Checksum command", "--whole-disc={0}",       wholeDisc);
            DicConsole.DebugWriteLine("Checksum command", "--input={0}",            input);
            DicConsole.DebugWriteLine("Checksum command", "--adler32={0}",          adler32);
            DicConsole.DebugWriteLine("Checksum command", "--crc16={0}",            crc16);
            DicConsole.DebugWriteLine("Checksum command", "--crc32={0}",            crc32);
            DicConsole.DebugWriteLine("Checksum command", "--crc64={0}",            crc64);
            DicConsole.DebugWriteLine("Checksum command", "--md5={0}",              md5);
            DicConsole.DebugWriteLine("Checksum command", "--ripemd160={0}",        ripemd160);
            DicConsole.DebugWriteLine("Checksum command", "--sha1={0}",             sha1);
            DicConsole.DebugWriteLine("Checksum command", "--sha256={0}",           sha256);
            DicConsole.DebugWriteLine("Checksum command", "--sha384={0}",           sha384);
            DicConsole.DebugWriteLine("Checksum command", "--sha512={0}",           sha512);
            DicConsole.DebugWriteLine("Checksum command", "--spamsum={0}",          spamSum);
            DicConsole.DebugWriteLine("Checksum command", "--fletcher16={0}",       fletcher16);
            DicConsole.DebugWriteLine("Checksum command", "--fletcher32={0}",       fletcher32);

            FiltersList filtersList = new FiltersList();
            IFilter     inputFilter = filtersList.GetFilter(input);

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open specified file.");
                return;
            }

            IMediaImage inputFormat = ImageFormat.Detect(inputFilter);

            if(inputFormat == null)
            {
                DicConsole.ErrorWriteLine("Unable to recognize image format, not checksumming");
                return;
            }

            inputFormat.Open(inputFilter);
            Statistics.AddMediaFormat(inputFormat.Format);
            Statistics.AddMedia(inputFormat.Info.MediaType, false);
            Statistics.AddFilter(inputFilter.Name);
            EnableChecksum enabledChecksums = new EnableChecksum();

            if(adler32) enabledChecksums    |= EnableChecksum.Adler32;
            if(crc16) enabledChecksums      |= EnableChecksum.Crc16;
            if(crc32) enabledChecksums      |= EnableChecksum.Crc32;
            if(crc64) enabledChecksums      |= EnableChecksum.Crc64;
            if(md5) enabledChecksums        |= EnableChecksum.Md5;
            if(ripemd160) enabledChecksums  |= EnableChecksum.Ripemd160;
            if(sha1) enabledChecksums       |= EnableChecksum.Sha1;
            if(sha256) enabledChecksums     |= EnableChecksum.Sha256;
            if(sha384) enabledChecksums     |= EnableChecksum.Sha384;
            if(sha512) enabledChecksums     |= EnableChecksum.Sha512;
            if(spamSum) enabledChecksums    |= EnableChecksum.SpamSum;
            if(fletcher16) enabledChecksums |= EnableChecksum.Fletcher16;
            if(fletcher32) enabledChecksums |= EnableChecksum.Fletcher32;

            Checksum mediaChecksum = null;

            if(inputFormat.Info.HasPartitions)
                try
                {
                    Checksum trackChecksum = null;

                    if(wholeDisc) mediaChecksum = new Checksum(enabledChecksums);

                    ulong previousTrackEnd = 0;

                    List<Track> inputTracks = inputFormat.Tracks;
                    foreach(Track currentTrack in inputTracks)
                    {
                        if(currentTrack.TrackStartSector - previousTrackEnd != 0 && wholeDisc)
                            for(ulong i = previousTrackEnd + 1; i < currentTrack.TrackStartSector; i++)
                            {
                                DicConsole.Write("\rHashing track-less sector {0}", i);

                                byte[] hiddenSector = inputFormat.ReadSector(i);

                                mediaChecksum?.Update(hiddenSector);
                            }

                        DicConsole.DebugWriteLine("Checksum command",
                                                  "Track {0} starts at sector {1} and ends at sector {2}",
                                                  currentTrack.TrackSequence, currentTrack.TrackStartSector,
                                                  currentTrack.TrackEndSector);

                        if(separatedTracks) trackChecksum = new Checksum(enabledChecksums);

                        ulong sectors     = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                        ulong doneSectors = 0;
                        DicConsole.WriteLine("Track {0} has {1} sectors", currentTrack.TrackSequence, sectors);

                        while(doneSectors < sectors)
                        {
                            byte[] sector;

                            if(sectors - doneSectors >= SECTORS_TO_READ)
                            {
                                sector = inputFormat.ReadSectors(doneSectors, SECTORS_TO_READ,
                                                                 currentTrack.TrackSequence);
                                DicConsole.Write("\rHashings sectors {0} to {2} of track {1}", doneSectors,
                                                 currentTrack.TrackSequence, doneSectors + SECTORS_TO_READ);
                                doneSectors += SECTORS_TO_READ;
                            }
                            else
                            {
                                sector = inputFormat.ReadSectors(doneSectors, (uint)(sectors - doneSectors),
                                                                 currentTrack.TrackSequence);
                                DicConsole.Write("\rHashings sectors {0} to {2} of track {1}", doneSectors,
                                                 currentTrack.TrackSequence, doneSectors + (sectors - doneSectors));
                                doneSectors += sectors - doneSectors;
                            }

                            if(wholeDisc) mediaChecksum?.Update(sector);

                            if(separatedTracks) trackChecksum?.Update(sector);
                        }

                        DicConsole.WriteLine();

                        if(separatedTracks)
                            if(trackChecksum != null)
                                foreach(ChecksumType chk in trackChecksum.End())
                                    DicConsole.WriteLine("Track {0}'s {1}: {2}", currentTrack.TrackSequence, chk.type,
                                                         chk.Value);

                        previousTrackEnd = currentTrack.TrackEndSector;
                    }

                    if(inputFormat.Info.Sectors - previousTrackEnd != 0 && wholeDisc)
                        for(ulong i = previousTrackEnd + 1; i < inputFormat.Info.Sectors; i++)
                        {
                            DicConsole.Write("\rHashing track-less sector {0}", i);

                            byte[] hiddenSector = inputFormat.ReadSector(i);
                            mediaChecksum?.Update(hiddenSector);
                        }

                    if(wholeDisc)
                        if(mediaChecksum != null)
                            foreach(ChecksumType chk in mediaChecksum.End())
                                DicConsole.WriteLine("Disk's {0}: {1}", chk.type, chk.Value);
                }
                catch(Exception ex)
                {
                    if(debug) DicConsole.DebugWriteLine("Could not get tracks because {0}", ex.Message);
                    else DicConsole.WriteLine("Unable to get separate tracks, not checksumming them");
                }
            else
            {
                mediaChecksum = new Checksum(enabledChecksums);

                ulong sectors = inputFormat.Info.Sectors;
                DicConsole.WriteLine("Sectors {0}", sectors);
                ulong doneSectors = 0;

                while(doneSectors < sectors)
                {
                    byte[] sector;

                    if(sectors - doneSectors >= SECTORS_TO_READ)
                    {
                        sector = inputFormat.ReadSectors(doneSectors, SECTORS_TO_READ);
                        DicConsole.Write("\rHashings sectors {0} to {1}", doneSectors, doneSectors + SECTORS_TO_READ);
                        doneSectors += SECTORS_TO_READ;
                    }
                    else
                    {
                        sector = inputFormat.ReadSectors(doneSectors, (uint)(sectors - doneSectors));
                        DicConsole.Write("\rHashings sectors {0} to {1}", doneSectors,
                                         doneSectors + (sectors - doneSectors));
                        doneSectors += sectors - doneSectors;
                    }

                    mediaChecksum.Update(sector);
                }

                DicConsole.WriteLine();

                foreach(ChecksumType chk in mediaChecksum.End())
                    DicConsole.WriteLine("Disk's {0}: {1}", chk.type, chk.Value);
            }

            Statistics.AddCommand("checksum");
        }
    }
}