﻿// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ListDevices.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'media-info' verb.
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

using System.Linq;
using CommandAndConquer.CLI.Attributes;
using DiscImageChef.Console;
using DiscImageChef.Devices;

namespace DiscImageChef.Commands
{
    public static partial class Device
    {
        [CliCommand("list", "Lists all connected devices.")]
        public static void List([CliParameter(                    'd', "Shows debug output from plugins.")]
                                bool debug = false, [CliParameter('v', "Shows verbose output.")]
                                bool verbose = false)
        {
            DicConsole.DebugWriteLine("Media-Info command", "--debug={0}",   debug);
            DicConsole.DebugWriteLine("Media-Info command", "--verbose={0}", verbose);

            Devices.DeviceInfo[] devices = DiscImageChef.Devices.Device.ListDevices();

            if(devices == null || devices.Length == 0) DicConsole.WriteLine("No known devices attached.");
            else
            {
                devices = devices.OrderBy(d => d.Path).ToArray();

                DicConsole.WriteLine("{0,-22}|{1,-16}|{2,-24}|{3,-24}|{4,-10}|{5,-10}", "Path", "Vendor", "Model",
                                     "Serial", "Bus", "Supported?");
                DicConsole.WriteLine("{0,-22}+{1,-16}+{2,-24}+{3,-24}+{4,-10}+{5,-10}", "----------------------",
                                     "----------------", "------------------------", "------------------------",
                                     "----------", "----------");
                foreach(Devices.DeviceInfo dev in devices)
                    DicConsole.WriteLine("{0,-22}|{1,-16}|{2,-24}|{3,-24}|{4,-10}|{5,-10}", dev.Path, dev.Vendor,
                                         dev.Model, dev.Serial, dev.Bus, dev.Supported);
            }

            Core.Statistics.AddCommand("list-devices");
        }
    }
}