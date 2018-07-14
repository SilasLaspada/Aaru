// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : PrintHex.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'printhex' verb.
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
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using DiscImageChef.Core;

namespace DiscImageChef.Commands
{
    public static partial class Image
    {
        [CliCommand("printhex", "Prints a sector, in hexadecimal values, to the console.")]
        public static void PrintHex([CliParameter(                          'i', "Disc image.")]   string input,
                                    [CliParameter(                          's', "Start sector.")] ulong  startSector,
                                    [CliParameter(                          'l', "How many sectors to print.")]
                                    ulong length = 1,         [CliParameter('r', "Print sectors with tags included.")]
                                    bool longSectors = false, [CliParameter('w', "How many bytes to print per line.")]
                                    ushort widthBytes = 32,   [CliParameter('d', "Shows debug output from plugins.")]
                                    bool debug = false,       [CliParameter('v', "Shows verbose output.")]
                                    bool verbose = false)
        {
            DicConsole.DebugWriteLine("PrintHex command", "--debug={0}",        debug);
            DicConsole.DebugWriteLine("PrintHex command", "--verbose={0}",      verbose);
            DicConsole.DebugWriteLine("PrintHex command", "--input={0}",        input);
            DicConsole.DebugWriteLine("PrintHex command", "--start={0}",        startSector);
            DicConsole.DebugWriteLine("PrintHex command", "--length={0}",       length);
            DicConsole.DebugWriteLine("PrintHex command", "--long-sectors={0}", longSectors);
            DicConsole.DebugWriteLine("PrintHex command", "--WidthBytes={0}",   widthBytes);

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
                DicConsole.ErrorWriteLine("Unable to recognize image format, not verifying");
                return;
            }

            inputFormat.Open(inputFilter);

            for(ulong i = 0; i < length; i++)
            {
                DicConsole.WriteLine("Sector {0}", startSector + i);

                if(inputFormat.Info.ReadableSectorTags == null)
                {
                    DicConsole
                       .WriteLine("Requested sectors with tags, unsupported by underlying image format, printing only user data.");
                    longSectors = false;
                }
                else
                {
                    if(inputFormat.Info.ReadableSectorTags.Count == 0)
                    {
                        DicConsole
                           .WriteLine("Requested sectors with tags, unsupported by underlying image format, printing only user data.");
                        longSectors = false;
                    }
                }

                byte[] sector = longSectors
                                    ? inputFormat.ReadSectorLong(startSector + i)
                                    : inputFormat.ReadSector(startSector     + i);

                DiscImageChef.PrintHex.PrintHexArray(sector, widthBytes);
            }

            Statistics.AddCommand("print-hex");
        }
    }
}