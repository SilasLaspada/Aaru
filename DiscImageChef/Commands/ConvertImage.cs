// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ConvertImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Converts from one media image to another.
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
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CommandAndConquer.CLI.Attributes;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using DiscImageChef.Core;
using Schemas;
using ImageInfo = DiscImageChef.CommonTypes.Structs.ImageInfo;
using Version = DiscImageChef.CommonTypes.Interop.Version;

namespace DiscImageChef.Commands
{
    public static partial class Image
    {
        [CliCommand("convert", "Converts one image to another format.")]
        public static void Convert([CliParameter('i', "Input image.")]  string input,
                                   [CliParameter('o', "Output image.")] string output, [CliParameter('p',
                                       "Format of the output image, as plugin name or plugin id. If not present, will try to detect it from output image extension.")]
                                   string outputFormat = null,
                                   [CliParameter(                'c', "How many sectors to convert at once.")]
                                   int count = 64, [CliParameter('f',
                                       "Continue conversion even if sector or media tags will be lost in the process.")]
                                   bool force = false, [CliParameter('W', "Who (person) created the image?")]
                                   string creator = null,
                                   [CliParameter('t', "Title of the media represented by the image")]
                                   string mediaTitle = null,
                                   [CliParameter('C', "Image comments")] string comments = null,
                                   [CliParameter('a', "Manufacturer of the media represented by the image")]
                                   string mediaManufacturer = null,
                                   [CliParameter('l',
                                       "Model of the media represented by the image")]
                                   string mediaModel = null, [CliParameter('s',
                                       "Serial number of the media represented by the image")]
                                   string mediaSerialNumber = null,
                                   [CliParameter('b',
                                       "Barcode of the media represented by the image")]
                                   string mediaBarcode = null, [CliParameter('n',
                                       "Part number of the media represented by the image")]
                                   string mediaPartNumber = null, [CliParameter('q',
                                       "Number in sequence for the media represented by the image")]
                                   int mediaSequence = 0, [CliParameter('z',
                                       "Last media of the sequence the media represented by the image corresponds to")]
                                   int lastMediaSequence = 0, [CliParameter('u',
                                       "Manufacturer of the drive used to read the media represented by the image")]
                                   string driveManufacturer = null, [CliParameter('g',
                                       "Model of the drive used to read the media represented by the image")]
                                   string driveModel = null, [CliParameter('h',
                                       "Serial number of the drive used to read the media represented by the image")]
                                   string driveSerialNumber = null, [CliParameter('y',
                                       "Firmware revision of the drive used to read the media represented by the image")]
                                   string driveFirmwareRevision = null, [CliParameter('O',
                                       "Comma separated name=value pairs of options to pass to output image plugin")]
                                   string options = null,
                                   [CliParameter('x',
                                       "Take metadata from existing CICM XML sidecar.")]
                                   string cicmXml = null, [CliParameter('r',
                                       "Take list of dump hardware from existing resume file.")]
                                   string resumeFile = null, [CliParameter('d', "Shows debug output from plugins.")]
                                   bool debug = false,       [CliParameter('v', "Shows verbose output.")]
                                   bool verbose = false)
        {
            DicConsole.DebugWriteLine("Analyze command", "--debug={0}",              debug);
            DicConsole.DebugWriteLine("Analyze command", "--verbose={0}",            verbose);
            DicConsole.DebugWriteLine("Analyze command", "--input={0}",              input);
            DicConsole.DebugWriteLine("Analyze command", "--output={0}",             output);
            DicConsole.DebugWriteLine("Analyze command", "--format={0}",             outputFormat);
            DicConsole.DebugWriteLine("Analyze command", "--count={0}",              count);
            DicConsole.DebugWriteLine("Analyze command", "--force={0}",              force);
            DicConsole.DebugWriteLine("Analyze command", "--creator={0}",            creator);
            DicConsole.DebugWriteLine("Analyze command", "--media-title={0}",        mediaTitle);
            DicConsole.DebugWriteLine("Analyze command", "--comments={0}",           comments);
            DicConsole.DebugWriteLine("Analyze command", "--media-manufacturer={0}", mediaManufacturer);
            DicConsole.DebugWriteLine("Analyze command", "--media-model={0}",        mediaModel);
            DicConsole.DebugWriteLine("Analyze command", "--media-serial={0}",       mediaSerialNumber);
            DicConsole.DebugWriteLine("Analyze command", "--media-barcode={0}",      mediaBarcode);
            DicConsole.DebugWriteLine("Analyze command", "--media-partnumber={0}",   mediaPartNumber);
            DicConsole.DebugWriteLine("Analyze command", "--media-sequence={0}",     mediaSequence);
            DicConsole.DebugWriteLine("Analyze command", "--media-lastsequence={0}", lastMediaSequence);
            DicConsole.DebugWriteLine("Analyze command", "--drive-manufacturer={0}", driveManufacturer);
            DicConsole.DebugWriteLine("Analyze command", "--drive-model={0}",        driveModel);
            DicConsole.DebugWriteLine("Analyze command", "--drive-serial={0}",       driveSerialNumber);
            DicConsole.DebugWriteLine("Analyze command", "--drive-revision={0}",     driveFirmwareRevision);
            DicConsole.DebugWriteLine("Analyze command", "--cicm-xml={0}",           cicmXml);
            DicConsole.DebugWriteLine("Analyze command", "--resume-file={0}",        resumeFile);
            DicConsole.DebugWriteLine("Analyze command", "--options={0}",            options);

            Dictionary<string, string> parsedOptions = Options.Parse(options);
            DicConsole.DebugWriteLine("Analyze command", "Parsed options:");
            foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
                DicConsole.DebugWriteLine("Analyze command", "{0} = {1}", parsedOption.Key, parsedOption.Value);

            if(count == 0)
            {
                DicConsole.ErrorWriteLine("Need to specify more than 0 sectors to copy at once");
                return;
            }

            Resume           resume  = null;
            CICMMetadataType sidecar = null;

            XmlSerializer xs = new XmlSerializer(typeof(CICMMetadataType));
            if(cicmXml != null)
                if(File.Exists(cicmXml))
                    try
                    {
                        StreamReader sr = new StreamReader(cicmXml);
                        sidecar = (CICMMetadataType)xs.Deserialize(sr);
                        sr.Close();
                    }
                    catch
                    {
                        DicConsole.ErrorWriteLine("Incorrect metadata sidecar file, not continuing...");
                        return;
                    }
                else
                {
                    DicConsole.ErrorWriteLine("Could not find metadata sidecar, not continuing...");
                    return;
                }

            xs = new XmlSerializer(typeof(Resume));
            if(resumeFile != null)
                if(File.Exists(resumeFile))
                    try
                    {
                        StreamReader sr = new StreamReader(resumeFile);
                        resume = (Resume)xs.Deserialize(sr);
                        sr.Close();
                    }
                    catch
                    {
                        DicConsole.ErrorWriteLine("Incorrect resume file, not continuing...");
                        return;
                    }
                else
                {
                    DicConsole.ErrorWriteLine("Could not find resume file, not continuing...");
                    return;
                }

            FiltersList filtersList = new FiltersList();
            IFilter     inputFilter = filtersList.GetFilter(input);

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open specified file.");
                return;
            }

            if(File.Exists(output))
            {
                DicConsole.ErrorWriteLine("Output file already exists, not continuing.");
                return;
            }

            PluginBase  plugins     = new PluginBase();
            IMediaImage inputFormat = ImageFormat.Detect(inputFilter);

            if(inputFormat == null)
            {
                DicConsole.WriteLine("Input image format not identified, not proceeding with conversion.");
                return;
            }

            if(verbose)
                DicConsole.VerboseWriteLine("Input image format identified by {0} ({1}).", inputFormat.Name,
                                            inputFormat.Id);
            else DicConsole.WriteLine("Input image format identified by {0}.", inputFormat.Name);

            try
            {
                if(!inputFormat.Open(inputFilter))
                {
                    DicConsole.WriteLine("Unable to open image format");
                    DicConsole.WriteLine("No error given");
                    return;
                }

                DicConsole.DebugWriteLine("Convert-image command", "Correctly opened image file.");
                DicConsole.DebugWriteLine("Convert-image command", "Image without headers is {0} bytes.",
                                          inputFormat.Info.ImageSize);
                DicConsole.DebugWriteLine("Convert-image command", "Image has {0} sectors.", inputFormat.Info.Sectors);
                DicConsole.DebugWriteLine("Convert-image command", "Image identifies media type as {0}.",
                                          inputFormat.Info.MediaType);

                Statistics.AddMediaFormat(inputFormat.Format);
                Statistics.AddMedia(inputFormat.Info.MediaType, false);
                Statistics.AddFilter(inputFilter.Name);
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine("Unable to open image format");
                DicConsole.ErrorWriteLine("Error: {0}", ex.Message);
                DicConsole.DebugWriteLine("Convert-image command", "Stack trace: {0}", ex.StackTrace);
                return;
            }

            List<IWritableImage> candidates = new List<IWritableImage>();

            // Try extension
            if(string.IsNullOrEmpty(outputFormat))
                candidates.AddRange(plugins.WritableImages.Values.Where(t =>
                                                                            t.KnownExtensions
                                                                             .Contains(Path.GetExtension(output))));
            // Try Id
            else if(Guid.TryParse(outputFormat, out Guid outId))
                candidates.AddRange(plugins.WritableImages.Values.Where(t => t.Id.Equals(outId)));
            // Try name
            else
                candidates.AddRange(plugins.WritableImages.Values.Where(t => string.Equals(t.Name, outputFormat,
                                                                                           StringComparison
                                                                                              .InvariantCultureIgnoreCase)));

            if(candidates.Count == 0)
            {
                DicConsole.WriteLine("No plugin supports requested extension.");
                return;
            }

            if(candidates.Count > 1)
            {
                DicConsole.WriteLine("More than one plugin supports requested extension.");
                return;
            }

            IWritableImage outFormat = candidates[0];

            if(verbose) DicConsole.VerboseWriteLine("Output image format: {0} ({1}).", outFormat.Name, outFormat.Id);
            else DicConsole.WriteLine("Output image format: {0}.", outFormat.Name);

            if(!outFormat.SupportedMediaTypes.Contains(inputFormat.Info.MediaType))
            {
                DicConsole.ErrorWriteLine("Output format does not support media type, cannot continue...");
                return;
            }

            foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags)
            {
                if(outFormat.SupportedMediaTags.Contains(mediaTag) || force) continue;

                DicConsole.ErrorWriteLine("Converting image will lose media tag {0}, not continuing...", mediaTag);
                DicConsole.ErrorWriteLine("If you don't care, use force option.");
                return;
            }

            bool useLong = inputFormat.Info.ReadableSectorTags.Count != 0;

            foreach(SectorTagType sectorTag in inputFormat.Info.ReadableSectorTags)
            {
                if(outFormat.SupportedSectorTags.Contains(sectorTag)) continue;

                if(force)
                {
                    if(sectorTag != SectorTagType.CdTrackFlags && sectorTag != SectorTagType.CdTrackIsrc &&
                       sectorTag != SectorTagType.CdSectorSubchannel) useLong = false;
                    continue;
                }

                DicConsole.ErrorWriteLine("Converting image will lose sector tag {0}, not continuing...", sectorTag);
                DicConsole
                   .ErrorWriteLine("If you don't care, use force option. This will skip all sector tags converting only user data.");
                return;
            }

            if(!outFormat.Create(output, inputFormat.Info.MediaType, parsedOptions, inputFormat.Info.Sectors,
                                 inputFormat.Info.SectorSize))
            {
                DicConsole.ErrorWriteLine("Error {0} creating output image.", outFormat.ErrorMessage);
                return;
            }

            ImageInfo metadata = new ImageInfo
            {
                Application           = "DiscImageChef",
                ApplicationVersion    = Version.GetVersion(),
                Comments              = comments              ?? inputFormat.Info.Comments,
                Creator               = creator               ?? inputFormat.Info.Creator,
                DriveFirmwareRevision = driveFirmwareRevision ?? inputFormat.Info.DriveFirmwareRevision,
                DriveManufacturer     = driveManufacturer     ?? inputFormat.Info.DriveManufacturer,
                DriveModel            = driveModel            ?? inputFormat.Info.DriveModel,
                DriveSerialNumber     = driveSerialNumber     ?? inputFormat.Info.DriveSerialNumber,
                LastMediaSequence     = lastMediaSequence != 0 ? lastMediaSequence : inputFormat.Info.LastMediaSequence,
                MediaBarcode          = mediaBarcode      ?? inputFormat.Info.MediaBarcode,
                MediaManufacturer     = mediaManufacturer ?? inputFormat.Info.MediaManufacturer,
                MediaModel            = mediaModel        ?? inputFormat.Info.MediaModel,
                MediaPartNumber       = mediaPartNumber   ?? inputFormat.Info.MediaPartNumber,
                MediaSequence         = mediaSequence != 0 ? mediaSequence : inputFormat.Info.MediaSequence,
                MediaSerialNumber     = mediaSerialNumber ?? inputFormat.Info.MediaSerialNumber,
                MediaTitle            = mediaTitle        ?? inputFormat.Info.MediaTitle
            };

            if(!outFormat.SetMetadata(metadata))
            {
                DicConsole.ErrorWrite("Error {0} setting metadata, ", outFormat.ErrorMessage);
                if(!force)
                {
                    DicConsole.ErrorWriteLine("not continuing...");
                    return;
                }

                DicConsole.ErrorWriteLine("continuing...");
            }

            List<Track> tracks;

            try { tracks = inputFormat.Tracks; }
            catch(Exception) { tracks = null; }

            CICMMetadataType       cicmMetadata = inputFormat.CicmMetadata;
            List<DumpHardwareType> dumpHardware = inputFormat.DumpHardware;

            if(tracks != null)
                if(!outFormat.SetTracks(tracks))
                {
                    DicConsole.ErrorWriteLine("Error {0} sending tracks list to output image.", outFormat.ErrorMessage);
                    return;
                }

            foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags)
            {
                if(force && !outFormat.SupportedMediaTags.Contains(mediaTag)) continue;

                DicConsole.WriteLine("Converting media tag {0}", mediaTag);
                byte[] tag = inputFormat.ReadDiskTag(mediaTag);
                if(outFormat.WriteMediaTag(tag, mediaTag)) continue;

                if(force)
                    DicConsole.ErrorWriteLine("Error {0} writing media tag, continuing...", outFormat.ErrorMessage);
                else
                {
                    DicConsole.ErrorWriteLine("Error {0} writing media tag, not continuing...", outFormat.ErrorMessage);
                    return;
                }
            }

            DicConsole.WriteLine("{0} sectors to convert", inputFormat.Info.Sectors);
            ulong doneSectors = 0;

            if(tracks == null)
            {
                DicConsole.WriteLine("Setting geometry to {0} cylinders, {1} heads and {2} sectors per track",
                                     inputFormat.Info.Cylinders, inputFormat.Info.Heads,
                                     inputFormat.Info.SectorsPerTrack);
                if(!outFormat.SetGeometry(inputFormat.Info.Cylinders, inputFormat.Info.Heads,
                                          inputFormat.Info.SectorsPerTrack))
                    DicConsole.ErrorWriteLine("Error {0} setting geometry, image may be incorrect, continuing...",
                                              outFormat.ErrorMessage);

                while(doneSectors < inputFormat.Info.Sectors)
                {
                    byte[] sector;

                    uint sectorsToDo;
                    if(inputFormat.Info.Sectors - doneSectors >= (ulong)count) sectorsToDo = (uint)count;
                    else
                        sectorsToDo =
                            (uint)(inputFormat.Info.Sectors - doneSectors);

                    DicConsole.Write("\rConverting sectors {0} to {1} ({2:P2} done)", doneSectors,
                                     doneSectors + sectorsToDo, doneSectors / (double)inputFormat.Info.Sectors);

                    bool result;
                    if(useLong)
                        if(sectorsToDo == 1)
                        {
                            sector = inputFormat.ReadSectorLong(doneSectors);
                            result = outFormat.WriteSectorLong(sector, doneSectors);
                        }
                        else
                        {
                            sector = inputFormat.ReadSectorsLong(doneSectors, sectorsToDo);
                            result = outFormat.WriteSectorsLong(sector, doneSectors, sectorsToDo);
                        }
                    else
                    {
                        if(sectorsToDo == 1)
                        {
                            sector = inputFormat.ReadSector(doneSectors);
                            result = outFormat.WriteSector(sector, doneSectors);
                        }
                        else
                        {
                            sector = inputFormat.ReadSectors(doneSectors, sectorsToDo);
                            result = outFormat.WriteSectors(sector, doneSectors, sectorsToDo);
                        }
                    }

                    if(!result)
                        if(force)
                            DicConsole.ErrorWriteLine("Error {0} writing sector {1}, continuing...",
                                                      outFormat.ErrorMessage, doneSectors);
                        else
                        {
                            DicConsole.ErrorWriteLine("Error {0} writing sector {1}, not continuing...",
                                                      outFormat.ErrorMessage, doneSectors);
                            return;
                        }

                    doneSectors += sectorsToDo;
                }

                DicConsole.Write("\rConverting sectors {0} to {1} ({2:P2} done)", inputFormat.Info.Sectors,
                                 inputFormat.Info.Sectors, 1.0);
                DicConsole.WriteLine();

                foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags)
                {
                    if(!useLong) break;

                    switch(tag)
                    {
                        case SectorTagType.AppleSectorTag:
                        case SectorTagType.CdSectorSync:
                        case SectorTagType.CdSectorHeader:
                        case SectorTagType.CdSectorSubHeader:
                        case SectorTagType.CdSectorEdc:
                        case SectorTagType.CdSectorEccP:
                        case SectorTagType.CdSectorEccQ:
                        case SectorTagType.CdSectorEcc:
                            // This tags are inline in long sector
                            continue;
                    }

                    if(force && !outFormat.SupportedSectorTags.Contains(tag)) continue;

                    doneSectors = 0;
                    while(doneSectors < inputFormat.Info.Sectors)
                    {
                        byte[] sector;

                        uint sectorsToDo;
                        if(inputFormat.Info.Sectors - doneSectors >= (ulong)count) sectorsToDo = (uint)count;
                        else
                            sectorsToDo =
                                (uint)(inputFormat.Info.Sectors - doneSectors);

                        DicConsole.Write("\rConverting tag {2} for sectors {0} to {1} ({2:P2} done)", doneSectors,
                                         doneSectors + sectorsToDo, doneSectors / (double)inputFormat.Info.Sectors,
                                         tag);

                        bool result;
                        if(sectorsToDo == 1)
                        {
                            sector = inputFormat.ReadSectorTag(doneSectors, tag);
                            result = outFormat.WriteSectorTag(sector, doneSectors, tag);
                        }
                        else
                        {
                            sector = inputFormat.ReadSectorsTag(doneSectors, sectorsToDo, tag);
                            result = outFormat.WriteSectorsTag(sector, doneSectors, sectorsToDo, tag);
                        }

                        if(!result)
                            if(force)
                                DicConsole.ErrorWriteLine("Error {0} writing sector {1}, continuing...",
                                                          outFormat.ErrorMessage, doneSectors);
                            else
                            {
                                DicConsole.ErrorWriteLine("Error {0} writing sector {1}, not continuing...",
                                                          outFormat.ErrorMessage, doneSectors);
                                return;
                            }

                        doneSectors += sectorsToDo;
                    }

                    DicConsole.Write("\rConverting tag {2} for sectors {0} to {1} ({2:P2} done)",
                                     inputFormat.Info.Sectors, inputFormat.Info.Sectors, 1.0, tag);
                    DicConsole.WriteLine();
                }
            }
            else
            {
                foreach(Track track in tracks)
                {
                    doneSectors = 0;
                    ulong trackSectors = track.TrackEndSector - track.TrackStartSector + 1;

                    while(doneSectors < trackSectors)
                    {
                        byte[] sector;

                        uint sectorsToDo;
                        if(trackSectors - doneSectors >= (ulong)count) sectorsToDo = (uint)count;
                        else sectorsToDo                                           = (uint)(trackSectors - doneSectors);

                        DicConsole.Write("\rConverting sectors {0} to {1} in track {3} ({2:P2} done)",
                                         doneSectors               + track.TrackStartSector,
                                         doneSectors + sectorsToDo + track.TrackStartSector,
                                         (doneSectors + track.TrackStartSector) / (double)inputFormat.Info.Sectors,
                                         track.TrackSequence);

                        bool result;
                        if(useLong)
                            if(sectorsToDo == 1)
                            {
                                sector = inputFormat.ReadSectorLong(doneSectors        + track.TrackStartSector);
                                result = outFormat.WriteSectorLong(sector, doneSectors + track.TrackStartSector);
                            }
                            else
                            {
                                sector = inputFormat.ReadSectorsLong(doneSectors + track.TrackStartSector, sectorsToDo);
                                result = outFormat.WriteSectorsLong(sector, doneSectors + track.TrackStartSector,
                                                                    sectorsToDo);
                            }
                        else
                        {
                            if(sectorsToDo == 1)
                            {
                                sector = inputFormat.ReadSector(doneSectors        + track.TrackStartSector);
                                result = outFormat.WriteSector(sector, doneSectors + track.TrackStartSector);
                            }
                            else
                            {
                                sector = inputFormat.ReadSectors(doneSectors + track.TrackStartSector, sectorsToDo);
                                result = outFormat.WriteSectors(sector, doneSectors + track.TrackStartSector,
                                                                sectorsToDo);
                            }
                        }

                        if(!result)
                            if(force)
                                DicConsole.ErrorWriteLine("Error {0} writing sector {1}, continuing...",
                                                          outFormat.ErrorMessage, doneSectors);
                            else
                            {
                                DicConsole.ErrorWriteLine("Error {0} writing sector {1}, not continuing...",
                                                          outFormat.ErrorMessage, doneSectors);
                                return;
                            }

                        doneSectors += sectorsToDo;
                    }
                }

                DicConsole.Write("\rConverting sectors {0} to {1} in track {3} ({2:P2} done)", inputFormat.Info.Sectors,
                                 inputFormat.Info.Sectors, 1.0, tracks.Count);
                DicConsole.WriteLine();

                foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.OrderBy(t => t))
                {
                    if(!useLong) break;

                    switch(tag)
                    {
                        case SectorTagType.AppleSectorTag:
                        case SectorTagType.CdSectorSync:
                        case SectorTagType.CdSectorHeader:
                        case SectorTagType.CdSectorSubHeader:
                        case SectorTagType.CdSectorEdc:
                        case SectorTagType.CdSectorEccP:
                        case SectorTagType.CdSectorEccQ:
                        case SectorTagType.CdSectorEcc:
                            // This tags are inline in long sector
                            continue;
                    }

                    if(force && !outFormat.SupportedSectorTags.Contains(tag)) continue;

                    foreach(Track track in tracks)
                    {
                        doneSectors = 0;
                        ulong  trackSectors = track.TrackEndSector - track.TrackStartSector + 1;
                        byte[] sector;
                        bool   result;

                        switch(tag)
                        {
                            case SectorTagType.CdTrackFlags:
                            case SectorTagType.CdTrackIsrc:
                                DicConsole.Write("\rConverting tag {0} in track {1} ({2:P2} done).", tag,
                                                 track.TrackSequence, track.TrackSequence / (double)tracks.Count);
                                sector = inputFormat.ReadSectorTag(track.TrackStartSector, tag);
                                result = outFormat.WriteSectorTag(sector, track.TrackStartSector, tag);
                                if(!result)
                                    if(force)
                                        DicConsole.ErrorWriteLine("Error {0} writing tag, continuing...",
                                                                  outFormat.ErrorMessage);
                                    else
                                    {
                                        DicConsole.ErrorWriteLine("Error {0} writing tag, not continuing...",
                                                                  outFormat.ErrorMessage);
                                        return;
                                    }

                                continue;
                        }

                        while(doneSectors < trackSectors)
                        {
                            uint sectorsToDo;
                            if(trackSectors - doneSectors >= (ulong)count) sectorsToDo = (uint)count;
                            else
                                sectorsToDo =
                                    (uint)(trackSectors - doneSectors);

                            DicConsole.Write("\rConverting tag {4} for sectors {0} to {1} in track {3} ({2:P2} done)",
                                             doneSectors               + track.TrackStartSector,
                                             doneSectors + sectorsToDo + track.TrackStartSector,
                                             (doneSectors + track.TrackStartSector) / (double)inputFormat.Info.Sectors,
                                             track.TrackSequence, tag);

                            if(sectorsToDo == 1)
                            {
                                sector = inputFormat.ReadSectorTag(doneSectors        + track.TrackStartSector, tag);
                                result = outFormat.WriteSectorTag(sector, doneSectors + track.TrackStartSector, tag);
                            }
                            else
                            {
                                sector = inputFormat.ReadSectorsTag(doneSectors + track.TrackStartSector, sectorsToDo,
                                                                    tag);
                                result = outFormat.WriteSectorsTag(sector, doneSectors + track.TrackStartSector,
                                                                   sectorsToDo, tag);
                            }

                            if(!result)
                                if(force)
                                    DicConsole.ErrorWriteLine("Error {0} writing tag for sector {1}, continuing...",
                                                              outFormat.ErrorMessage, doneSectors);
                                else
                                {
                                    DicConsole.ErrorWriteLine("Error {0} writing tag for sector {1}, not continuing...",
                                                              outFormat.ErrorMessage, doneSectors);
                                    return;
                                }

                            doneSectors += sectorsToDo;
                        }
                    }

                    switch(tag)
                    {
                        case SectorTagType.CdTrackFlags:
                        case SectorTagType.CdTrackIsrc:
                            DicConsole.Write("\rConverting tag {0} in track {1} ({2:P2} done).", tag, tracks.Count,
                                             1.0);
                            break;
                        default:
                            DicConsole.Write("\rConverting tag {4} for sectors {0} to {1} in track {3} ({2:P2} done)",
                                             inputFormat.Info.Sectors, inputFormat.Info.Sectors, 1.0, tracks.Count,
                                             tag);
                            break;
                    }

                    DicConsole.WriteLine();
                }
            }

            bool ret = false;
            if(resume != null || dumpHardware != null)
            {
                if(resume            != null) ret = outFormat.SetDumpHardware(resume.Tries);
                else if(dumpHardware != null) ret = outFormat.SetDumpHardware(dumpHardware);
                if(ret) DicConsole.WriteLine("Written dump hardware list to output image.");
            }

            ret = false;
            if(sidecar != null || cicmMetadata != null)
            {
                if(sidecar           != null) ret = outFormat.SetCicmMetadata(sidecar);
                else if(cicmMetadata != null) ret = outFormat.SetCicmMetadata(cicmMetadata);
                if(ret) DicConsole.WriteLine("Written CICM XML metadata to output image.");
            }

            DicConsole.WriteLine("Closing output image.");

            if(!outFormat.Close())
                DicConsole.ErrorWriteLine("Error {0} closing output image... Contents are not correct.",
                                          outFormat.ErrorMessage);

            DicConsole.WriteLine();
            DicConsole.WriteLine("Conversion done.");

            Statistics.AddCommand("convert-image");
        }
    }
}