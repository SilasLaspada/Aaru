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
        public static void Dump([CliParameter('i', "Device path.")]  string DevicePath,
                                [CliParameter('o', "Output image.")] string OutputFile,

                                // TODO: Disabled temporarily
                                /*        [CliParameter('r', "Dump sectors with tags included. For optical media, dump scrambled sectors")]
                                        bool Raw = false,*/
                                [CliParameter(                          's', "Stop media dump on first error.")]
                                bool StopOnError = false, [CliParameter('f', "Continue dump whatever happens.")]
                                bool Force = false,       [CliParameter('p', "How many retry passes to do.")]
                                ushort RetryPasses = 5,
                                [CliParameter(                         'P', "Try to recover partial or incorrect data.")]
                                bool Persistent = false, [CliParameter('m', "Create/use resume mapfile.")]
                                bool Resume = true,
                                [CliParameter('l',
                                    "Try to read lead-in. Only applicable to CD/DDCD/GD.")]
                                bool LeadIn = false,        [CliParameter('e', "Name of character encoding to use.")]
                                string EncodingName = null, [CliParameter('t',
                                    "Format of the output image, as plugin name or plugin id. If not present, will try to detect it from output image extension.")]
                                string OutputFormat = null, [CliParameter('O',
                                    "Comma separated name=value pairs of options to pass to output image plugin")]
                                string Options = null,
                                [CliParameter('x',
                                    "Take metadata from existing CICM XML sidecar.")]
                                string CicmXml = null, [CliParameter('k',
                                    "When an unreadable sector is found skip this many sectors.")]
                                int Skip = 512, [CliParameter('a', "Disables creating CICM XML sidecar.")]
                                bool NoMetadata = false,
                                [CliParameter('t',
                                    "Disables trimming errored from skipped sectors.")]
                                bool NoTrim = false, [CliParameter('d', "Shows debug output from plugins.")]
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
            DicConsole.DebugWriteLine("Dump-Media command", "--device={0}",  DevicePath);
            // TODO: Disabled temporarily
            //DicConsole.DebugWriteLine("Dump-Media command", "--raw={0}",           Raw);
            DicConsole.DebugWriteLine("Dump-Media command", "--stop-on-error={0}", StopOnError);
            DicConsole.DebugWriteLine("Dump-Media command", "--force={0}",         Force);
            DicConsole.DebugWriteLine("Dump-Media command", "--retry-passes={0}",  RetryPasses);
            DicConsole.DebugWriteLine("Dump-Media command", "--persistent={0}",    Persistent);
            DicConsole.DebugWriteLine("Dump-Media command", "--resume={0}",        Resume);
            DicConsole.DebugWriteLine("Dump-Media command", "--lead-in={0}",       LeadIn);
            DicConsole.DebugWriteLine("Dump-Media command", "--encoding={0}",      EncodingName);
            DicConsole.DebugWriteLine("Dump-Media command", "--output={0}",        OutputFile);
            DicConsole.DebugWriteLine("Dump-Media command", "--format={0}",        OutputFormat);
            DicConsole.DebugWriteLine("Dump-Media command", "--options={0}",       Options);
            DicConsole.DebugWriteLine("Dump-Media command", "--cicm-xml={0}",      CicmXml);
            DicConsole.DebugWriteLine("Dump-Media command", "--skip={0}",          Skip);
            DicConsole.DebugWriteLine("Dump-Media command", "--no-metadata={0}",   NoMetadata);
            DicConsole.DebugWriteLine("Dump-Media command", "--no-trim={0}", NoTrim);

            Dictionary<string, string> parsedOptions = Core.Options.Parse(Options);
            DicConsole.DebugWriteLine("Dump-Media command", "Parsed options:");
            foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
                DicConsole.DebugWriteLine("Dump-Media command", "{0} = {1}", parsedOption.Key, parsedOption.Value);

            Encoding encoding = null;

            if(EncodingName != null)
                try
                {
                    encoding = Claunia.Encoding.Encoding.GetEncoding(EncodingName);
                    if(verbose) DicConsole.VerboseWriteLine("Using encoding for {0}.", encoding.EncodingName);
                }
                catch(ArgumentException)
                {
                    DicConsole.ErrorWriteLine("Specified encoding is not supported.");
                    return;
                }

            if(DevicePath.Length == 2 && DevicePath[1] == ':' && DevicePath[0] != '/' &&
               char.IsLetter(DevicePath[0]))
                DevicePath = "\\\\.\\" + char.ToUpper(DevicePath[0]) + ':';

            DiscImageChef.Devices.Device dev = new DiscImageChef.Devices.Device(DevicePath);

            if(dev.Error)
            {
                DicConsole.ErrorWriteLine("Error {0} opening device.", dev.LastError);
                return;
            }

            Core.Statistics.AddDevice(dev);

            string outputPrefix = Path.Combine(Path.GetDirectoryName(OutputFile),
                                               Path.GetFileNameWithoutExtension(OutputFile));

            Resume        resume = null;
            XmlSerializer xs     = new XmlSerializer(typeof(Resume));
            if(File.Exists(outputPrefix + ".resume.xml") && Resume)
                try
                {
                    StreamReader sr = new StreamReader(outputPrefix + ".resume.xml");
                    resume = (Resume)xs.Deserialize(sr);
                    sr.Close();
                }
                catch
                {
                    DicConsole.ErrorWriteLine("Incorrect resume file, not continuing...");
                    return;
                }

            if(resume != null && resume.NextBlock > resume.LastBlock && resume.BadBlocks.Count == 0)
            {
                DicConsole.WriteLine("Media already dumped correctly, not continuing...");
                return;
            }

            CICMMetadataType sidecar   = null;
            XmlSerializer    sidecarXs = new XmlSerializer(typeof(CICMMetadataType));
            if(CicmXml != null)
                if(File.Exists(CicmXml))
                    try
                    {
                        StreamReader sr = new StreamReader(CicmXml);
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
            if(string.IsNullOrEmpty(OutputFormat))
                candidates.AddRange(plugins.WritableImages.Values.Where(t =>
                                                                            t.KnownExtensions
                                                                             .Contains(Path.GetExtension(OutputFile))));
            // Try Id
            else if(Guid.TryParse(OutputFormat, out Guid outId))
                candidates.AddRange(plugins.WritableImages.Values.Where(t => t.Id.Equals(outId)));
            // Try name
            else
                candidates.AddRange(plugins.WritableImages.Values.Where(t => string.Equals(t.Name, OutputFormat,
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

            IWritableImage outputFormat = candidates[0];

            DumpLog dumpLog = new DumpLog(outputPrefix + ".log", dev);

            if(verbose)
            {
                dumpLog.WriteLine("Output image format: {0} ({1}).", outputFormat.Name, outputFormat.Id);
                DicConsole.VerboseWriteLine("Output image format: {0} ({1}).", outputFormat.Name, outputFormat.Id);
            }
            else
            {
                dumpLog.WriteLine("Output image format: {0}.", outputFormat.Name);
                DicConsole.WriteLine("Output image format: {0}.", outputFormat.Name);
            }

            switch(dev.Type)
            {
                case DeviceType.ATA:
                    Ata.Dump(dev, DevicePath, outputFormat, RetryPasses, Force,
                             false, /*Raw,*/
                             Persistent, StopOnError, ref resume, ref dumpLog, encoding, outputPrefix,
                             OutputFile, parsedOptions, sidecar, (uint)Skip, NoMetadata,
                             NoTrim);
                    break;
                case DeviceType.MMC:
                case DeviceType.SecureDigital:
                    SecureDigital.Dump(dev, DevicePath, outputFormat, RetryPasses, Force,
                                       false, /*Raw,*/ Persistent, StopOnError, ref resume,
                                       ref dumpLog, encoding, outputPrefix, OutputFile, parsedOptions, sidecar,
                                       (uint)Skip, NoMetadata, NoTrim);
                    break;
                case DeviceType.NVMe:
                    NvMe.Dump(dev, DevicePath, outputFormat, RetryPasses, Force,
                              false, /*Raw,*/
                              Persistent, StopOnError, ref resume, ref dumpLog, encoding, outputPrefix,
                              OutputFile, parsedOptions, sidecar, (uint)Skip, NoMetadata,
                              NoTrim);
                    break;
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    Scsi.Dump(dev, DevicePath, outputFormat, RetryPasses, Force,
                              false, /*Raw,*/
                              Persistent, StopOnError, ref resume, ref dumpLog, LeadIn,
                              encoding, outputPrefix, OutputFile, parsedOptions, sidecar, (uint)Skip,
                              NoMetadata, NoTrim);
                    break;
                default:
                    dumpLog.WriteLine("Unknown device type.");
                    dumpLog.Close();
                    throw new NotSupportedException("Unknown device type.");
            }

            if(resume != null && Resume)
            {
                resume.LastWriteDate = DateTime.UtcNow;
                resume.BadBlocks.Sort();

                if(File.Exists(outputPrefix + ".resume.xml")) File.Delete(outputPrefix + ".resume.xml");

                FileStream fs = new FileStream(outputPrefix + ".resume.xml", FileMode.Create, FileAccess.ReadWrite);
                xs = new XmlSerializer(resume.GetType());
                xs.Serialize(fs, resume);
                fs.Close();
            }

            dumpLog.Close();

            Core.Statistics.AddCommand("dump-media");
        }
    }
}