// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DeviceReport.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'device-report' verb.
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
using System.IO;
using System.Xml.Serialization;
using CommandAndConquer.CLI.Attributes;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Core.Devices.Report;
using DiscImageChef.Core.Devices.Report.SCSI;
using DiscImageChef.Devices;

namespace DiscImageChef.Commands
{
    public static partial class Device
    {
        [CliCommand("report", "Tests the device capabilities and creates an XML report of them.")]
        public static void Info([CliParameter(                    'i', "Device path.")] string DevicePath,
                                [CliParameter(                    'd', "Shows debug output from plugins.")]
                                bool debug = false, [CliParameter('v', "Shows verbose output.")]
                                bool verbose = false)
        {
            DicConsole.DebugWriteLine("Device-Report command", "--debug={0}",   debug);
            DicConsole.DebugWriteLine("Device-Report command", "--verbose={0}", verbose);
            DicConsole.DebugWriteLine("Device-Report command", "--device={0}",  DevicePath);

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

            CommonTypes.Metadata.DeviceReport report    = new CommonTypes.Metadata.DeviceReport();
            bool                              removable = false;
            string                            xmlFile;
            if(!string.IsNullOrWhiteSpace(dev.Manufacturer) && !string.IsNullOrWhiteSpace(dev.Revision))
                xmlFile =
                    dev.Manufacturer + "_" + dev.Model + "_" + dev.Revision + ".xml";
            else if(!string.IsNullOrWhiteSpace(dev.Manufacturer))
                xmlFile = dev.Manufacturer + "_" + dev.Model + ".xml";
            else if(!string.IsNullOrWhiteSpace(dev.Revision))
                xmlFile = dev.Model + "_" + dev.Revision + ".xml";
            else
                xmlFile =
                    dev.Model + ".xml";

            xmlFile = xmlFile.Replace('\\', '_').Replace('/', '_').Replace('?', '_');

            switch(dev.Type)
            {
                case DeviceType.ATA:
                    Ata.Report(dev, ref report, debug, ref removable);
                    break;
                case DeviceType.MMC:
                case DeviceType.SecureDigital:
                    SecureDigital.Report(dev, ref report);
                    break;
                case DeviceType.NVMe:
                    Nvme.Report(dev, ref report, debug, ref removable);
                    break;
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    General.Report(dev, ref report, debug, ref removable);
                    break;
                default: throw new NotSupportedException("Unknown device type.");
            }

            FileStream xmlFs = new FileStream(xmlFile, FileMode.Create);

            XmlSerializer xmlSer = new XmlSerializer(typeof(CommonTypes.Metadata.DeviceReport));
            xmlSer.Serialize(xmlFs, report);
            xmlFs.Close();
            Core.Statistics.AddCommand("device-report");

            if(Settings.Settings.Current.ShareReports) Remote.SubmitReport(report);
        }
    }
}