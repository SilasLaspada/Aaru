// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Formats.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'formats' verb.
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

using System.Collections.Generic;
using System.Linq;
using CommandAndConquer.CLI.Attributes;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using DiscImageChef.Partitions;

namespace DiscImageChef.Commands
{
    public partial class About
    {
        [CliCommand("formats", "Lists all supported disc images, partition schemes and file systems.")]
        public static void Formats([CliParameter(                    'd', "Shows debug output from plugins.")]
                                   bool debug = false, [CliParameter('v', "Shows verbose output.")]
                                   bool verbose = false)
        {
            PluginBase  plugins     = new PluginBase();
            FiltersList filtersList = new FiltersList();

            DicConsole.WriteLine("Supported filters ({0}):", filtersList.Filters.Count);
            if(verbose) DicConsole.VerboseWriteLine("GUID\t\t\t\t\tFilter");
            foreach(KeyValuePair<string, IFilter> kvp in filtersList.Filters)
                if(verbose)
                    DicConsole.VerboseWriteLine("{0}\t{1}", kvp.Value.Id, kvp.Value.Name);
                else
                    DicConsole.WriteLine(kvp.Value.Name);

            DicConsole.WriteLine();
            DicConsole.WriteLine("Read-only media image formats ({0}):",
                                 plugins.ImagePluginsList.Count(t => !t.Value.GetType().GetInterfaces()
                                                                       .Contains(typeof(IWritableImage))));
            if(verbose) DicConsole.VerboseWriteLine("GUID\t\t\t\t\tPlugin");
            foreach(KeyValuePair<string, IMediaImage> kvp in plugins.ImagePluginsList.Where(t => !t.Value.GetType()
                                                                                                   .GetInterfaces()
                                                                                                   .Contains(typeof(
                                                                                                                 IWritableImage
                                                                                                             ))))
                if(verbose)
                    DicConsole.VerboseWriteLine("{0}\t{1}", kvp.Value.Id, kvp.Value.Name);
                else
                    DicConsole.WriteLine(kvp.Value.Name);

            DicConsole.WriteLine();
            DicConsole.WriteLine("Read/write media image formats ({0}):", plugins.WritableImages.Count);
            if(verbose) DicConsole.VerboseWriteLine("GUID\t\t\t\t\tPlugin");
            foreach(KeyValuePair<string, IWritableImage> kvp in plugins.WritableImages)
                if(verbose)
                    DicConsole.VerboseWriteLine("{0}\t{1}", kvp.Value.Id, kvp.Value.Name);
                else
                    DicConsole.WriteLine(kvp.Value.Name);

            DicConsole.WriteLine();
            DicConsole.WriteLine("Supported filesystems for identification and information only ({0}):",
                                 plugins.PluginsList.Count(t => !t.Value.GetType().GetInterfaces()
                                                                  .Contains(typeof(IReadOnlyFilesystem))));
            if(verbose) DicConsole.VerboseWriteLine("GUID\t\t\t\t\tPlugin");
            foreach(KeyValuePair<string, IFilesystem> kvp in plugins.PluginsList.Where(t => !t.Value.GetType()
                                                                                              .GetInterfaces()
                                                                                              .Contains(typeof(
                                                                                                            IReadOnlyFilesystem
                                                                                                        ))))
                if(verbose)
                    DicConsole.VerboseWriteLine("{0}\t{1}", kvp.Value.Id, kvp.Value.Name);
                else
                    DicConsole.WriteLine(kvp.Value.Name);

            DicConsole.WriteLine();
            DicConsole.WriteLine("Supported filesystems that can read their contents ({0}):",
                                 plugins.ReadOnlyFilesystems.Count);
            if(verbose) DicConsole.VerboseWriteLine("GUID\t\t\t\t\tPlugin");
            foreach(KeyValuePair<string, IReadOnlyFilesystem> kvp in plugins.ReadOnlyFilesystems)
                if(verbose)
                    DicConsole.VerboseWriteLine("{0}\t{1}", kvp.Value.Id, kvp.Value.Name);
                else
                    DicConsole.WriteLine(kvp.Value.Name);

            DicConsole.WriteLine();
            DicConsole.WriteLine("Supported partitioning schemes ({0}):", plugins.PartPluginsList.Count);
            if(verbose) DicConsole.VerboseWriteLine("GUID\t\t\t\t\tPlugin");
            foreach(KeyValuePair<string, IPartition> kvp in plugins.PartPluginsList)
                if(verbose)
                    DicConsole.VerboseWriteLine("{0}\t{1}", kvp.Value.Id, kvp.Value.Name);
                else
                    DicConsole.WriteLine(kvp.Value.Name);

            Core.Statistics.AddCommand("formats");
        }
    }
}