// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ImageInfo.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'image-info' verb.
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
using DiscImageChef.Commands;

namespace DiscImageChef.Controllers
{
    [CliController("image", "Handles images")]
    public static class Image
    {
        [CliCommand("info", "Opens a media image and shows information about the media it represents and metadata.")]
        public static void Info([CliParameter(                    'i', "Media image.")] string input,
                                [CliParameter(                    'd', "Shows debug output from plugins.")]
                                bool debug = false, [CliParameter('v', "Shows verbose output.")]
                                bool verbose = false)
        {
            ImageInfo.GetImageInfo(new ImageInfoOptions {Verbose = verbose, Debug = debug, InputFile = input});
        }

        [CliCommand("analyze", "Analyzes a disc image and searches for partitions and/or filesystems.")]
        public static void Analyze([CliParameter(                      'i', "Media image.")] string input,
                                   [CliParameter(                      'd', "Shows debug output from plugins")]
                                   bool debug = false,   [CliParameter('v', "Shows verbose output")]
                                   bool verbose = false, [CliParameter('e', "Name of character encoding to use.")]
                                   string EncodingName = null,
                                   [CliParameter('f', "Searches and interprets filesystems.")]
                                   bool SearchForFilesystems = true,
                                   [CliParameter('p', "Searches and interprets partitions.")]
                                   bool SearchForPartitions = true)
        {
            Commands.Analyze.DoAnalyze(new AnalyzeOptions
            {
                Verbose              = verbose,
                Debug                = debug,
                InputFile            = input,
                SearchForFilesystems = SearchForFilesystems,
                SearchForPartitions  = SearchForPartitions,
                EncodingName         = EncodingName
            });
        }

        [CliCommand("checksum", "Checksums an image.")]
        public static void Checksum([CliParameter(                             'i', "Disc image.")] string InputFile,
                                    [CliParameter(                             't', "Checksums each track separately.")]
                                    bool SeparatedTracks = true, [CliParameter('w', "Checksums the whole disc.")]
                                    bool WholeDisc = true,       [CliParameter('a', "Calculates Adler-32.")]
                                    bool DoAdler32 = true,       [CliParameter('6', "Calculates CRC16.")]
                                    bool DoCrc16 = true,         [CliParameter('c', "Calculates CRC32.")]
                                    bool DoCrc32 = true,         [CliParameter('4', "Calculates CRC64 (ECMA).")]
                                    bool DoCrc64 = false,        [CliParameter('t', "Calculates Fletcher-16.")]
                                    bool DoFletcher16 = false,   [CliParameter('h', "Calculates Fletcher-32.")]
                                    bool DoFletcher32 = false,
                                    [CliParameter(                          'm', "Calculates MD5.")] bool DoMd5 = true,
                                    [CliParameter(                          'r', "Calculates RIPEMD160.")]
                                    bool DoRipemd160 = false, [CliParameter('s', "Calculates SHA1.")]
                                    bool DoSha1 = true,       [CliParameter('2', "Calculates SHA256.")]
                                    bool DoSha256 = false,    [CliParameter('3', "Calculates SHA384.")]
                                    bool DoSha384 = false,    [CliParameter('5', "Calculates SHA512.")]
                                    bool DoSha512 = false,    [CliParameter('f', "Calculates SpamSum fuzzy hash.")]
                                    bool DoSpamSum = true,    [CliParameter('d', "Shows debug output from plugins.")]
                                    bool debug = false,       [CliParameter('v', "Shows verbose output.")]
                                    bool verbose = false)
        {
            Commands.Checksum.DoChecksum(new ChecksumOptions
            {
                Verbose         = verbose,
                Debug           = debug,
                InputFile       = InputFile,
                SeparatedTracks = SeparatedTracks,
                WholeDisc       = WholeDisc,
                DoAdler32       = DoAdler32,
                DoCrc16         = DoCrc16,
                DoCrc32         = DoCrc32,
                DoCrc64         = DoCrc64,
                DoFletcher16    = DoFletcher16,
                DoFletcher32    = DoFletcher32,
                DoMd5           = DoMd5,
                DoRipemd160     = DoRipemd160,
                DoSha1          = DoSha1,
                DoSha256        = DoSha256,
                DoSha384        = DoSha384,
                DoSha512        = DoSha512,
                DoSpamSum       = DoSpamSum
            });
        }

        [CliCommand("entropy", "Calculates entropy and/or duplicated sectors of an image.")]
        public static void Entropy([CliParameter('i', "Disc image.")] string InputFile,
                                   [CliParameter('p',
                                       "Calculates how many sectors are duplicated (have same exact data in user area).")]
                                   bool DuplicatedSectors = true,
                                   [CliParameter('t', "Calculates entropy for each track separately.")]
                                   bool SeparatedTracks = true,
                                   [CliParameter(                       'w', "Calculates entropy for  the whole disc.")]
                                   bool WholeDisc = true, [CliParameter('d', "Shows debug output from plugins.")]
                                   bool debug = false,    [CliParameter('v', "Shows verbose output.")]
                                   bool verbose = false)
        {
            Commands.Entropy.DoEntropy(new EntropyOptions
            {
                Verbose           = verbose,
                Debug             = debug,
                InputFile         = InputFile,
                SeparatedTracks   = SeparatedTracks,
                WholeDisc         = WholeDisc,
                DuplicatedSectors = DuplicatedSectors
            });
        }

        [CliCommand("verify", "Verifies a disc image integrity, and if supported, sector integrity.")]
        public static void Verify([CliParameter(                           'i', "Disc image.")] string InputFile,
                                  [CliParameter(                           'w', "Verify disc image if supported.")]
                                  bool VerifyDisc = true,    [CliParameter('s', "Verify all sectors if supported.")]
                                  bool VerifySectors = true, [CliParameter('d', "Shows debug output from plugins.")]
                                  bool debug = false,        [CliParameter('v', "Shows verbose output.")]
                                  bool verbose = false)
        {
            Commands.Verify.DoVerify(new VerifyOptions
            {
                Verbose       = verbose,
                Debug         = debug,
                InputFile     = InputFile,
                VerifyDisc    = VerifyDisc,
                VerifySectors = VerifySectors
            });
        }

        [CliCommand("printhex", "Prints a sector, in hexadecimal values, to the console.")]
        public static void PrintHex([CliParameter(                          'i', "Disc image.")]   string InputFile,
                                    [CliParameter(                          's', "Start sector.")] ulong  StartSector,
                                    [CliParameter(                          'l', "How many sectors to print.")]
                                    ulong Length = 1,         [CliParameter('r', "Print sectors with tags included.")]
                                    bool LongSectors = false, [CliParameter('w', "How many bytes to print per line.")]
                                    ushort WidthBytes = 32,   [CliParameter('d', "Shows debug output from plugins.")]
                                    bool debug = false,       [CliParameter('v', "Shows verbose output.")]
                                    bool verbose = false)
        {
            Commands.PrintHex.DoPrintHex(new PrintHexOptions
            {
                Verbose     = verbose,
                Debug       = debug,
                InputFile   = InputFile,
                StartSector = StartSector,
                Length      = Length,
                LongSectors = LongSectors,
                WidthBytes  = WidthBytes
            });
        }

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
            Commands.Decode.DoDecode(new DecodeOptions
            {
                Verbose     = verbose,
                Debug       = debug,
                InputFile   = InputFile,
                StartSector = StartSector,
                Length      = Length,
                DiskTags    = DiskTags,
                SectorTags  = SectorTags
            });
        }

        [CliCommand("create-sidecar", "Creates CICM Metadata XML sidecar.")]
        public static void CreateSidecar([CliParameter('i', "Disc image.")] string InputFile, [CliParameter('t',
                                             "When used indicates that input is a folder containing alphabetically sorted files extracted from a linear block-based tape with fixed block size (e.g. a SCSI tape device).")]
                                         bool Tape = false, [CliParameter('b',
                                             "Only used for tapes, indicates block size. Files in the folder whose size is not a multiple of this value will simply be ignored.")]
                                         int BlockSize = 512, [CliParameter('e', "Name of character encoding to use.")]
                                         string EncodingName = null,
                                         [CliParameter(                    'd', "Shows debug output from plugins.")]
                                         bool debug = false, [CliParameter('v', "Shows verbose output.")]
                                         bool verbose = false)
        {
            Commands.CreateSidecar.DoSidecar(new CreateSidecarOptions
            {
                Verbose      = verbose,
                Debug        = debug,
                InputFile    = InputFile,
                Tape         = Tape,
                BlockSize    = BlockSize,
                EncodingName = EncodingName
            });
        }

        [CliCommand("convert", "Converts one image to another format.")]
        public static void Convert([CliParameter('i', "Input image.")]  string InputFile,
                                   [CliParameter('o', "Output image.")] string OutputFile, [CliParameter('p',
                                       "Format of the output image, as plugin name or plugin id. If not present, will try to detect it from output image extension.")]
                                   string OutputFormat = null,
                                   [CliParameter(                'c', "How many sectors to convert at once.")]
                                   int Count = 64, [CliParameter('f',
                                       "Continue conversion even if sector or media tags will be lost in the process.")]
                                   bool Force = false, [CliParameter('W', "Who (person) created the image?")]
                                   string Creator = null,
                                   [CliParameter('t', "Title of the media represented by the image")]
                                   string MediaTitle = null,
                                   [CliParameter('C', "Image comments")] string Comments = null,
                                   [CliParameter('a', "Manufacturer of the media represented by the image")]
                                   string MediaManufacturer = null,
                                   [CliParameter('l',
                                       "Model of the media represented by the image")]
                                   string MediaModel = null, [CliParameter('s',
                                       "Serial number of the media represented by the image")]
                                   string MediaSerialNumber = null,
                                   [CliParameter('b',
                                       "Barcode of the media represented by the image")]
                                   string MediaBarcode = null, [CliParameter('n',
                                       "Part number of the media represented by the image")]
                                   string MediaPartNumber = null, [CliParameter('q',
                                       "Number in sequence for the media represented by the image")]
                                   int MediaSequence = 0, [CliParameter('z',
                                       "Last media of the sequence the media represented by the image corresponds to")]
                                   int LastMediaSequence = 0, [CliParameter('u',
                                       "Manufacturer of the drive used to read the media represented by the image")]
                                   string DriveManufacturer = null, [CliParameter('g',
                                       "Model of the drive used to read the media represented by the image")]
                                   string DriveModel = null, [CliParameter('h',
                                       "Serial number of the drive used to read the media represented by the image")]
                                   string DriveSerialNumber = null, [CliParameter('y',
                                       "Firmware revision of the drive used to read the media represented by the image")]
                                   string DriveFirmwareRevision = null, [CliParameter('O',
                                       "Comma separated name=value pairs of options to pass to output image plugin")]
                                   string Options = null,
                                   [CliParameter('x',
                                       "Take metadata from existing CICM XML sidecar.")]
                                   string CicmXml = null, [CliParameter('r',
                                       "Take list of dump hardware from existing resume file.")]
                                   string ResumeFile = null, [CliParameter('d', "Shows debug output from plugins.")]
                                   bool debug = false,       [CliParameter('v', "Shows verbose output.")]
                                   bool verbose = false)
        {
            ConvertImage.DoConvert(new ConvertImageOptions
            {
                Verbose               = verbose,
                Debug                 = debug,
                InputFile             = InputFile,
                OutputFile            = OutputFile,
                OutputFormat          = OutputFormat,
                Count                 = Count,
                Force                 = Force,
                Creator               = Creator,
                MediaTitle            = MediaTitle,
                Comments              = Comments,
                MediaManufacturer     = MediaManufacturer,
                MediaModel            = MediaModel,
                MediaSerialNumber     = MediaSerialNumber,
                DriveManufacturer     = DriveManufacturer,
                DriveModel            = DriveModel,
                DriveSerialNumber     = DriveSerialNumber,
                MediaBarcode          = MediaBarcode,
                MediaPartNumber       = MediaPartNumber,
                MediaSequence         = MediaSequence,
                LastMediaSequence     = LastMediaSequence,
                DriveFirmwareRevision = DriveFirmwareRevision,
                Options               = Options,
                CicmXml               = CicmXml,
                ResumeFile            = ResumeFile
            });
        }
    }
}