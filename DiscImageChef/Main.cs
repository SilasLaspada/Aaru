// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Main.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Main program loop.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains the main program loop.
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
using System.Reflection;
using CommandAndConquer.CLI.Core;
using DiscImageChef.Commands;
using DiscImageChef.Console;
using DiscImageChef.Settings;
using Statistics = DiscImageChef.Core.Statistics;

namespace DiscImageChef
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            DicConsole.WriteLineEvent      += System.Console.WriteLine;
            DicConsole.WriteEvent          += System.Console.Write;
            DicConsole.ErrorWriteLineEvent += System.Console.Error.WriteLine;

            PrintCopyright();

            Settings.Settings.LoadSettings();
            if(Settings.Settings.Current.GdprCompliance < DicSettings.GdprLevel) Configure.DoConfigure(true);
            Statistics.LoadStats();
            if(Settings.Settings.Current.Stats != null && Settings.Settings.Current.Stats.ShareStats)
                Statistics.SubmitStats();

            Processor.ProcessArguments(args);

            Statistics.SaveStats();
        }

        static void PrintCopyright()
        {
            object[] attributes =
                typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            string assemblyTitle = ((AssemblyTitleAttribute)attributes[0]).Title;
            attributes = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            AssemblyInformationalVersionAttribute assemblyVersion =
                Attribute.GetCustomAttribute(typeof(MainClass).Assembly, typeof(AssemblyInformationalVersionAttribute))
                    as AssemblyInformationalVersionAttribute;
            string assemblyCopyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;

            DicConsole.WriteLine("{0} {1}", assemblyTitle, assemblyVersion?.InformationalVersion);
            DicConsole.WriteLine("{0}",     assemblyCopyright);
            DicConsole.WriteLine();
        }
    }
}