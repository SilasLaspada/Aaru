// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Decode.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'decode' verb.
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

using CommandAndConquer.CLI.Attributes;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Decoders.CD;
using DiscImageChef.Decoders.SCSI;

namespace DiscImageChef.Commands
{
    public static partial class Image
    {
        [CliCommand("decode", "Decodes and pretty prints disk and/or sector tags.")]
        public static void Decode([CliParameter(                        'i', "Disc image.")]   string InputFile,
                                  [CliParameter(                        's', "Start sector.")] ulong  StartSector = 0,
                                  [CliParameter(                        'l', "How many sectors to decode, or \"all\".")]
                                  string Length = "all",  [CliParameter('k', "Decode disk tags.")]
                                  bool DiskTags = true,   [CliParameter('t', "Decode sector tags.")]
                                  bool SectorTags = true, [CliParameter('d', "Shows debug output from plugins.")]
                                  bool debug = false,     [CliParameter('v', "Shows verbose output.")]
                                  bool verbose = false)
        {
            DicConsole.DebugWriteLine("Decode command", "--debug={0}",       debug);
            DicConsole.DebugWriteLine("Decode command", "--verbose={0}",     verbose);
            DicConsole.DebugWriteLine("Decode command", "--input={0}",       InputFile);
            DicConsole.DebugWriteLine("Decode command", "--start={0}",       StartSector);
            DicConsole.DebugWriteLine("Decode command", "--length={0}",      Length);
            DicConsole.DebugWriteLine("Decode command", "--disk-tags={0}",   DiskTags);
            DicConsole.DebugWriteLine("Decode command", "--sector-tags={0}", SectorTags);

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
                DicConsole.ErrorWriteLine("Unable to recognize image format, not decoding");
                return;
            }

            inputFormat.Open(inputFilter);
            Core.Statistics.AddMediaFormat(inputFormat.Format);
            Core.Statistics.AddMedia(inputFormat.Info.MediaType, false);
            Core.Statistics.AddFilter(inputFilter.Name);

            if(DiskTags)
                if(inputFormat.Info.ReadableMediaTags.Count == 0)
                    DicConsole.WriteLine("There are no disk tags in chosen disc image.");
                else
                    foreach(MediaTagType tag in inputFormat.Info.ReadableMediaTags)
                        switch(tag)
                        {
                            case MediaTagType.SCSI_INQUIRY:
                            {
                                byte[] inquiry = inputFormat.ReadDiskTag(MediaTagType.SCSI_INQUIRY);
                                if(inquiry == null)
                                    DicConsole.WriteLine("Error reading SCSI INQUIRY response from disc image");
                                else
                                {
                                    DicConsole.WriteLine("SCSI INQUIRY command response:");
                                    DicConsole
                                       .WriteLine("================================================================================");
                                    DicConsole.WriteLine(Inquiry.Prettify(inquiry));
                                    DicConsole
                                       .WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.ATA_IDENTIFY:
                            {
                                byte[] identify = inputFormat.ReadDiskTag(MediaTagType.ATA_IDENTIFY);
                                if(identify == null)
                                    DicConsole.WriteLine("Error reading ATA IDENTIFY DEVICE response from disc image");
                                else
                                {
                                    DicConsole.WriteLine("ATA IDENTIFY DEVICE command response:");
                                    DicConsole
                                       .WriteLine("================================================================================");
                                    DicConsole.WriteLine(Identify.Prettify(identify));
                                    DicConsole
                                       .WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.ATAPI_IDENTIFY:
                            {
                                byte[] identify = inputFormat.ReadDiskTag(MediaTagType.ATAPI_IDENTIFY);
                                if(identify == null)
                                    DicConsole
                                       .WriteLine("Error reading ATA IDENTIFY PACKET DEVICE response from disc image");
                                else
                                {
                                    DicConsole.WriteLine("ATA IDENTIFY PACKET DEVICE command response:");
                                    DicConsole
                                       .WriteLine("================================================================================");
                                    DicConsole.WriteLine(Identify.Prettify(identify));
                                    DicConsole
                                       .WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.CD_ATIP:
                            {
                                byte[] atip = inputFormat.ReadDiskTag(MediaTagType.CD_ATIP);
                                if(atip == null) DicConsole.WriteLine("Error reading CD ATIP from disc image");
                                else
                                {
                                    DicConsole.WriteLine("CD ATIP:");
                                    DicConsole
                                       .WriteLine("================================================================================");
                                    DicConsole.WriteLine(ATIP.Prettify(atip));
                                    DicConsole
                                       .WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.CD_FullTOC:
                            {
                                byte[] fulltoc = inputFormat.ReadDiskTag(MediaTagType.CD_FullTOC);
                                if(fulltoc == null) DicConsole.WriteLine("Error reading CD full TOC from disc image");
                                else
                                {
                                    DicConsole.WriteLine("CD full TOC:");
                                    DicConsole
                                       .WriteLine("================================================================================");
                                    DicConsole.WriteLine(FullTOC.Prettify(fulltoc));
                                    DicConsole
                                       .WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.CD_PMA:
                            {
                                byte[] pma = inputFormat.ReadDiskTag(MediaTagType.CD_PMA);
                                if(pma == null) DicConsole.WriteLine("Error reading CD PMA from disc image");
                                else
                                {
                                    DicConsole.WriteLine("CD PMA:");
                                    DicConsole
                                       .WriteLine("================================================================================");
                                    DicConsole.WriteLine(PMA.Prettify(pma));
                                    DicConsole
                                       .WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.CD_SessionInfo:
                            {
                                byte[] sessioninfo = inputFormat.ReadDiskTag(MediaTagType.CD_SessionInfo);
                                if(sessioninfo == null)
                                    DicConsole.WriteLine("Error reading CD session information from disc image");
                                else
                                {
                                    DicConsole.WriteLine("CD session information:");
                                    DicConsole
                                       .WriteLine("================================================================================");
                                    DicConsole.WriteLine(Session.Prettify(sessioninfo));
                                    DicConsole
                                       .WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.CD_TEXT:
                            {
                                byte[] cdtext = inputFormat.ReadDiskTag(MediaTagType.CD_TEXT);
                                if(cdtext == null) DicConsole.WriteLine("Error reading CD-TEXT from disc image");
                                else
                                {
                                    DicConsole.WriteLine("CD-TEXT:");
                                    DicConsole
                                       .WriteLine("================================================================================");
                                    DicConsole.WriteLine(CDTextOnLeadIn.Prettify(cdtext));
                                    DicConsole
                                       .WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.CD_TOC:
                            {
                                byte[] toc = inputFormat.ReadDiskTag(MediaTagType.CD_TOC);
                                if(toc == null) DicConsole.WriteLine("Error reading CD TOC from disc image");
                                else
                                {
                                    DicConsole.WriteLine("CD TOC:");
                                    DicConsole
                                       .WriteLine("================================================================================");
                                    DicConsole.WriteLine(TOC.Prettify(toc));
                                    DicConsole
                                       .WriteLine("================================================================================");
                                }

                                break;
                            }
                            default:
                                DicConsole.WriteLine("Decoder for disk tag type \"{0}\" not yet implemented, sorry.",
                                                     tag);
                                break;
                        }

            if(SectorTags)
            {
                if(Length.ToLowerInvariant() == "all") { }
                else
                {
                    if(!ulong.TryParse(Length, out ulong _))
                    {
                        DicConsole.WriteLine("Value \"{0}\" is not a valid number for length.", Length);
                        DicConsole.WriteLine("Not decoding sectors tags");
                        return;
                    }
                }

                if(inputFormat.Info.ReadableSectorTags.Count == 0)
                    DicConsole.WriteLine("There are no sector tags in chosen disc image.");
                else
                    foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags)
                        switch(tag)
                        {
                            default:
                                DicConsole.WriteLine("Decoder for disk tag type \"{0}\" not yet implemented, sorry.",
                                                     tag);
                                break;
                        }

                // TODO: Not implemented
            }

            Core.Statistics.AddCommand("decode");
        }
    }
}