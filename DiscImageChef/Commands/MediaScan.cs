// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MediaScan.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'media-scan' verb.
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
using CommandAndConquer.CLI.Attributes;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Core.Devices.Scanning;
using DiscImageChef.Devices;

namespace DiscImageChef.Commands
{
    public static partial class Media
    {
        [CliCommand("scan", "Scans the media inserted on a device.")]
        public static void Scan([CliParameter('i', "Device path.")] string devicePath,
                                [CliParameter('m',
                                    "Write a log of the scan in the format used by MHDD.")]
                                string mhddLogPath = null, [CliParameter('b',
                                    "Write a log of the scan in the format used by ImgBurn.")]
                                string ibgLogPath = null, [CliParameter('d', "Shows debug output from plugins.")]
                                bool debug = false,       [CliParameter('v', "Shows verbose output.")]
                                bool verbose = false)
        {
            DicConsole.DebugWriteLine("Media-Scan command", "--debug={0}",    debug);
            DicConsole.DebugWriteLine("Media-Scan command", "--verbose={0}",  verbose);
            DicConsole.DebugWriteLine("Media-Scan command", "--device={0}",   devicePath);
            DicConsole.DebugWriteLine("Media-Scan command", "--mhdd-log={0}", mhddLogPath);
            DicConsole.DebugWriteLine("Media-Scan command", "--ibg-log={0}",  ibgLogPath);

            if(devicePath.Length == 2 && devicePath[1] == ':' && devicePath[0] != '/' && char.IsLetter(devicePath[0]))
                devicePath = "\\\\.\\" + char.ToUpper(devicePath[0]) + ':';

            Devices.Device dev = new Devices.Device(devicePath);

            if(dev.Error)
            {
                DicConsole.ErrorWriteLine("Error {0} opening device.", dev.LastError);
                return;
            }

            Statistics.AddDevice(dev);

            ScanResults results;

            switch(dev.Type)
            {
                case DeviceType.ATA:
                    results = Ata.Scan(mhddLogPath, ibgLogPath, devicePath, dev);
                    break;
                case DeviceType.MMC:
                case DeviceType.SecureDigital:
                    results = SecureDigital.Scan(mhddLogPath, ibgLogPath, devicePath, dev);
                    break;
                case DeviceType.NVMe:
                    results = Nvme.Scan(mhddLogPath, ibgLogPath, devicePath, dev);
                    break;
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    results = Scsi.Scan(mhddLogPath, ibgLogPath, devicePath, dev);
                    break;
                default: throw new NotSupportedException("Unknown device type.");
            }

            DicConsole.WriteLine("Took a total of {0} seconds ({1} processing commands).", results.TotalTime,
                                 results.ProcessingTime);
            DicConsole.WriteLine("Avegare speed: {0:F3} MiB/sec.",       results.AvgSpeed);
            DicConsole.WriteLine("Fastest speed burst: {0:F3} MiB/sec.", results.MaxSpeed);
            DicConsole.WriteLine("Slowest speed burst: {0:F3} MiB/sec.", results.MinSpeed);
            DicConsole.WriteLine("Summary:");
            DicConsole.WriteLine("{0} sectors took less than 3 ms.",                        results.A);
            DicConsole.WriteLine("{0} sectors took less than 10 ms but more than 3 ms.",    results.B);
            DicConsole.WriteLine("{0} sectors took less than 50 ms but more than 10 ms.",   results.C);
            DicConsole.WriteLine("{0} sectors took less than 150 ms but more than 50 ms.",  results.D);
            DicConsole.WriteLine("{0} sectors took less than 500 ms but more than 150 ms.", results.E);
            DicConsole.WriteLine("{0} sectors took more than 500 ms.",                      results.F);
            DicConsole.WriteLine("{0} sectors could not be read.",
                                 results.UnreadableSectors.Count);
            if(results.UnreadableSectors.Count > 0)
                foreach(ulong bad in results.UnreadableSectors)
                    DicConsole.WriteLine("Sector {0} could not be read", bad);

            DicConsole.WriteLine();

            #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
            if(results.SeekTotal != 0 || results.SeekMin != double.MaxValue || results.SeekMax != double.MinValue)
                #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
                DicConsole.WriteLine("Testing {0} seeks, longest seek took {1:F3} ms, fastest one took {2:F3} ms. ({3:F3} ms average)",
                                     results.SeekTimes, results.SeekMax, results.SeekMin, results.SeekTotal / 1000);

            Statistics.AddMediaScan((long)results.A, (long)results.B, (long)results.C, (long)results.D, (long)results.E,
                                    (long)results.F, (long)results.Blocks, (long)results.Errored,
                                    (long)(results.Blocks - results.Errored));
            Statistics.AddCommand("media-scan");
        }
    }
}