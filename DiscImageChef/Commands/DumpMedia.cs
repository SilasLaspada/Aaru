// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DumpMedia.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'dump-media' verb.
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
using System.Text;
using System.Xml.Serialization;
using CommandAndConquer.CLI.Attributes;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Core.Devices.Dumping;
using DiscImageChef.Core.Logging;
using DiscImageChef.Devices;
using Schemas;

namespace DiscImageChef.Commands
{
    [CliController("media", "Handles medias")]
    public static partial class Media
    {
        [CliCommand("dump", "Dumps the media inserted on a device to a media image.")]
        public static void Dump([CliParameter('i', "Device path.")]  string devicePath,
                                [CliParameter('o', "Output image.")] string output,

                                // TODO: Disabled temporarily
                                /*        [CliParameter('r', "Dump sectors with tags included. For optical media, dump scrambled sectors")]
                                        bool raw = false,*/
                                [CliParameter(                          's', "Stop media dump on first error.")]
                                bool stopOnError = false, [CliParameter('f', "Continue dump whatever happens.")]
                                bool force = false,       [CliParameter('p', "How many retry passes to do.")]
                                ushort retryPasses = 5,
                                [CliParameter(                         'P', "Try to recover partial or incorrect data.")]
                                bool persistent = false, [CliParameter('m', "Create/use resume mapfile.")]
                                bool resume = true,
                                [CliParameter('l',
                                    "Try to read lead-in. Only applicable to CD/DDCD/GD.")]
                                bool leadIn = false,        [CliParameter('e', "Name of character encoding to use.")]
                                string encodingName = null, [CliParameter('t',
                                    "Format of the output image, as plugin name or plugin id. If not present, will try to detect it from output image extension.")]
                                string outputFormat = null, [CliParameter('O',
                                    "Comma separated name=value pairs of options to pass to output image plugin")]
                                string options = null,
                                [CliParameter('x',
                                    "Take metadata from existing CICM XML sidecar.")]
                                string cicmXml = null, [CliParameter('k',
                                    "When an unreadable sector is found skip this many sectors.")]
                                int skip = 512, [CliParameter('a', "Disables creating CICM XML sidecar.")]
                                bool noMetadata = false,
                                [CliParameter('t',
                                    "Disables trimming errored from skipped sectors.")]
                                bool noTrim = false, [CliParameter('d', "Shows debug output from plugins.")]
                                bool debug = false,  [CliParameter('v', "Shows verbose output.")]
                                bool verbose = false)
        {
            // TODO: Be able to cancel hashing
            Sidecar.InitProgressEvent    += Progress.InitProgress;
            Sidecar.UpdateProgressEvent  += Progress.UpdateProgress;
            Sidecar.EndProgressEvent     += Progress.EndProgress;
            Sidecar.InitProgressEvent2   += Progress.InitProgress2;
            Sidecar.UpdateProgressEvent2 += Progress.UpdateProgress2;
            Sidecar.EndProgressEvent2    += Progress.EndProgress2;
            Sidecar.UpdateStatusEvent    += Progress.UpdateStatus;

            DicConsole.DebugWriteLine("Dump-Media command", "--debug={0}",   debug);
            DicConsole.DebugWriteLine("Dump-Media command", "--verbose={0}", verbose);
            DicConsole.DebugWriteLine("Dump-Media command", "--device={0}",  devicePath);
            // TODO: Disabled temporarily
            //DicConsole.DebugWriteLine("Dump-Media command", "--raw={0}",           raw);
            DicConsole.DebugWriteLine("Dump-Media command", "--stop-on-error={0}", stopOnError);
            DicConsole.DebugWriteLine("Dump-Media command", "--force={0}",         force);
            DicConsole.DebugWriteLine("Dump-Media command", "--retry-passes={0}",  retryPasses);
            DicConsole.DebugWriteLine("Dump-Media command", "--persistent={0}",    persistent);
            DicConsole.DebugWriteLine("Dump-Media command", "--resume={0}",        resume);
            DicConsole.DebugWriteLine("Dump-Media command", "--lead-in={0}",       leadIn);
            DicConsole.DebugWriteLine("Dump-Media command", "--encoding={0}",      encodingName);
            DicConsole.DebugWriteLine("Dump-Media command", "--output={0}",        output);
            DicConsole.DebugWriteLine("Dump-Media command", "--format={0}",        outputFormat);
            DicConsole.DebugWriteLine("Dump-Media command", "--options={0}",       options);
            DicConsole.DebugWriteLine("Dump-Media command", "--cicm-xml={0}",      cicmXml);
            DicConsole.DebugWriteLine("Dump-Media command", "--skip={0}",          skip);
            DicConsole.DebugWriteLine("Dump-Media command", "--no-metadata={0}",   noMetadata);
            DicConsole.DebugWriteLine("Dump-Media command", "--no-trim={0}",       noTrim);

            Dictionary<string, string> parsedOptions = Options.Parse(options);
            DicConsole.DebugWriteLine("Dump-Media command", "Parsed options:");
            foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
                DicConsole.DebugWriteLine("Dump-Media command", "{0} = {1}", parsedOption.Key, parsedOption.Value);

            Encoding encoding = null;

            if(encodingName != null)
                try
                {
                    encoding = Claunia.Encoding.Encoding.GetEncoding(encodingName);
                    if(verbose) DicConsole.VerboseWriteLine("Using encoding for {0}.", encoding.EncodingName);
                }
                catch(ArgumentException)
                {
                    DicConsole.ErrorWriteLine("Specified encoding is not supported.");
                    return;
                }

            if(devicePath.Length == 2 && devicePath[1] == ':' && devicePath[0] != '/' && char.IsLetter(devicePath[0]))
                devicePath = "\\\\.\\" + char.ToUpper(devicePath[0]) + ':';

            Devices.Device dev = new Devices.Device(devicePath);

            if(dev.Error)
            {
                DicConsole.ErrorWriteLine("Error {0} opening device.", dev.LastError);
                return;
            }

            Statistics.AddDevice(dev);

            string outputPrefix = Path.Combine(Path.GetDirectoryName(output), Path.GetFileNameWithoutExtension(output));

            Resume        resumeData = null;
            XmlSerializer xs         = new XmlSerializer(typeof(Resume));
            if(File.Exists(outputPrefix + ".resume.xml") && resume)
                try
                {
                    StreamReader sr = new StreamReader(outputPrefix + ".resume.xml");
                    resumeData = (Resume)xs.Deserialize(sr);
                    sr.Close();
                }
                catch
                {
                    DicConsole.ErrorWriteLine("Incorrect resume file, not continuing...");
                    return;
                }

            if(resumeData != null && resumeData.NextBlock > resumeData.LastBlock && resumeData.BadBlocks.Count == 0)
            {
                DicConsole.WriteLine("Media already dumped correctly, not continuing...");
                return;
            }

            CICMMetadataType sidecar   = null;
            XmlSerializer    sidecarXs = new XmlSerializer(typeof(CICMMetadataType));
            if(cicmXml != null)
                if(File.Exists(cicmXml))
                    try
                    {
                        StreamReader sr = new StreamReader(cicmXml);
                        sidecar = (CICMMetadataType)sidecarXs.Deserialize(sr);
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

            PluginBase           plugins    = new PluginBase();
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

            DumpLog dumpLog = new DumpLog(outputPrefix + ".log", dev);

            if(verbose)
            {
                dumpLog.WriteLine("Output image format: {0} ({1}).", outFormat.Name, outFormat.Id);
                DicConsole.VerboseWriteLine("Output image format: {0} ({1}).", outFormat.Name, outFormat.Id);
            }
            else
            {
                dumpLog.WriteLine("Output image format: {0}.", outFormat.Name);
                DicConsole.WriteLine("Output image format: {0}.", outFormat.Name);
            }

            switch(dev.Type)
            {
                case DeviceType.ATA:
                    Ata.Dump(dev, devicePath, outFormat, retryPasses, force, false, /*Raw,*/
                             persistent, stopOnError, ref resumeData, ref dumpLog, encoding, outputPrefix, output,
                             parsedOptions, sidecar, (uint)skip, noMetadata, noTrim);
                    break;
                case DeviceType.MMC:
                case DeviceType.SecureDigital:
                    SecureDigital.Dump(dev, devicePath, outFormat, retryPasses, force, false, /*Raw,*/ persistent,
                                       stopOnError, ref resumeData, ref dumpLog, encoding, outputPrefix, output,
                                       parsedOptions, sidecar, (uint)skip, noMetadata, noTrim);
                    break;
                case DeviceType.NVMe:
                    NvMe.Dump(dev, devicePath, outFormat, retryPasses, force, false, /*Raw,*/
                              persistent, stopOnError, ref resumeData, ref dumpLog, encoding, outputPrefix, output,
                              parsedOptions, sidecar, (uint)skip, noMetadata, noTrim);
                    break;
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    Scsi.Dump(dev, devicePath, outFormat, retryPasses, force, false, /*Raw,*/
                              persistent, stopOnError, ref resumeData, ref dumpLog, leadIn, encoding, outputPrefix,
                              output, parsedOptions, sidecar, (uint)skip, noMetadata, noTrim);
                    break;
                default:
                    dumpLog.WriteLine("Unknown device type.");
                    dumpLog.Close();
                    throw new NotSupportedException("Unknown device type.");
            }

            if(resumeData != null && resume)
            {
                resumeData.LastWriteDate = DateTime.UtcNow;
                resumeData.BadBlocks.Sort();

                if(File.Exists(outputPrefix + ".resume.xml")) File.Delete(outputPrefix + ".resume.xml");

                FileStream fs = new FileStream(outputPrefix + ".resume.xml", FileMode.Create, FileAccess.ReadWrite);
                xs = new XmlSerializer(resumeData.GetType());
                xs.Serialize(fs, resumeData);
                fs.Close();
            }

            dumpLog.Close();

            Statistics.AddCommand("dump-media");
        }
    }
}