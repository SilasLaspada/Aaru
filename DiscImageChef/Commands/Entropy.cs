// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Entropy.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'entropy' verb.
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
using System.Linq;
using CommandAndConquer.CLI.Attributes;
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using DiscImageChef.Core;

namespace DiscImageChef.Commands
{
    public static partial class Image
    {
        [CliCommand("entropy", "Calculates entropy and/or duplicated sectors of an image.")]
        public static void Entropy([CliParameter('i', "Disc image.")] string InputFile,
                                   [CliParameter('p',
                                       "Calculates how many sectors are duplicated (have same exact data in user area).")]
                                   bool DuplicatedSectors = true,
                                   [CliParameter('t', "Calculates entropy for each track separately.")]
                                   bool SeparatedTracks = true,
                                   [CliParameter(                       'w', "Calculates entropy for  the whole disc.")]
                                   bool WholeDisc = true, [CliParameter('d', "Shows debug output from plugins.")]
                                   bool debug = false,    [CliParameter('v', "Shows verbose output.")]
                                   bool verbose = false)
        {
            DicConsole.DebugWriteLine("Entropy command", "--debug={0}",              debug);
            DicConsole.DebugWriteLine("Entropy command", "--verbose={0}",            verbose);
            DicConsole.DebugWriteLine("Entropy command", "--separated-tracks={0}",   SeparatedTracks);
            DicConsole.DebugWriteLine("Entropy command", "--whole-disc={0}",         WholeDisc);
            DicConsole.DebugWriteLine("Entropy command", "--input={0}",              InputFile);
            DicConsole.DebugWriteLine("Entropy command", "--duplicated-sectors={0}", DuplicatedSectors);

            FiltersList filtersList = new FiltersList();
            IFilter     inputFilter = filtersList.GetFilter(InputFile);

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
            Core.Statistics.AddMediaFormat(inputFormat.Format);
            Core.Statistics.AddMedia(inputFormat.Info.MediaType, false);
            Core.Statistics.AddFilter(inputFilter.Name);
            double  entropy = 0;
            ulong[] entTable;
            ulong   sectors;

            if(SeparatedTracks)
                try
                {
                    List<Track> inputTracks = inputFormat.Tracks;

                    foreach(Track currentTrack in inputTracks)
                    {
                        entTable = new ulong[256];
                        ulong        trackSize             = 0;
                        List<string> uniqueSectorsPerTrack = new List<string>();

                        sectors = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                        DicConsole.WriteLine("Track {0} has {1} sectors", currentTrack.TrackSequence, sectors);

                        for(ulong i = currentTrack.TrackStartSector; i <= currentTrack.TrackEndSector; i++)
                        {
                            DicConsole.Write("\rEntropying sector {0} of track {1}", i + 1, currentTrack.TrackSequence);
                            byte[] sector = inputFormat.ReadSector(i, currentTrack.TrackSequence);

                            if(DuplicatedSectors)
                            {
                                string sectorHash = Sha1Context.Data(sector, out _);
                                if(!uniqueSectorsPerTrack.Contains(sectorHash)) uniqueSectorsPerTrack.Add(sectorHash);
                            }

                            foreach(byte b in sector) entTable[b]++;

                            trackSize += (ulong)sector.LongLength;
                        }

                        entropy += entTable.Select(l => (double)l / (double)trackSize)
                                           .Select(frequency => -(frequency * Math.Log(frequency, 2))).Sum();

                        DicConsole.WriteLine("Entropy for track {0} is {1:F4}.", currentTrack.TrackSequence, entropy);

                        if(DuplicatedSectors)
                            DicConsole.WriteLine("Track {0} has {1} unique sectors ({1:P3})",
                                                 currentTrack.TrackSequence, uniqueSectorsPerTrack.Count,
                                                 (double)uniqueSectorsPerTrack.Count / (double)sectors);

                        DicConsole.WriteLine();
                    }
                }
                catch(Exception ex)
                {
                    if(debug) DicConsole.DebugWriteLine("Could not get tracks because {0}", ex.Message);
                    else DicConsole.ErrorWriteLine("Unable to get separate tracks, not calculating their entropy");
                }

            if(!WholeDisc) return;

            entTable = new ulong[256];
            ulong        diskSize      = 0;
            List<string> uniqueSectors = new List<string>();

            sectors = inputFormat.Info.Sectors;
            DicConsole.WriteLine("Sectors {0}", sectors);

            for(ulong i = 0; i < sectors; i++)
            {
                DicConsole.Write("\rEntropying sector {0}", i + 1);
                byte[] sector = inputFormat.ReadSector(i);

                if(DuplicatedSectors)
                {
                    string sectorHash = Sha1Context.Data(sector, out _);
                    if(!uniqueSectors.Contains(sectorHash)) uniqueSectors.Add(sectorHash);
                }

                foreach(byte b in sector) entTable[b]++;

                diskSize += (ulong)sector.LongLength;
            }

            entropy += entTable.Select(l => (double)l / (double)diskSize)
                               .Select(frequency => -(frequency * Math.Log(frequency, 2))).Sum();

            DicConsole.WriteLine();

            DicConsole.WriteLine("Entropy for disk is {0:F4}.", entropy);

            if(DuplicatedSectors)
                DicConsole.WriteLine("Disk has {0} unique sectors ({1:P3})", uniqueSectors.Count,
                                     (double)uniqueSectors.Count / (double)sectors);

            Core.Statistics.AddCommand("entropy");
        }
    }
}