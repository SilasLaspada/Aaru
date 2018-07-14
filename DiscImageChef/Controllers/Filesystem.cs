using CommandAndConquer.CLI.Attributes;
using DiscImageChef.Commands;

namespace DiscImageChef.Controllers
{
    [CliController("filesystem", "Handles filesystem contents")]
    public class Filesystem
    {
        [CliCommand("info", "Gets information about a device.")]
        public static void Info([CliParameter('i', "Device path.")] string DevicePath,
                                [CliParameter('w',
                                    "Write binary responses from device with that prefix.")]
                                string OutputPrefix = null, [CliParameter('d', "Shows debug output from plugins.")]
                                bool debug = false,         [CliParameter('v', "Shows verbose output.")]
                                bool verbose = false)
        {
            DeviceInfo.DoDeviceInfo(new DeviceInfoOptions
            {
                Verbose      = verbose,
                Debug        = debug,
                DevicePath   = DevicePath,
                OutputPrefix = OutputPrefix
            });
        }

        [CliCommand("list", "Lists files in disc image.")]
        public static void List([CliParameter(                            'i', "Disc image.")] string InputFile,
                                [CliParameter(                            'l', "Uses long format.")]
                                bool Long = false,          [CliParameter('e', "Name of character encoding to use.")]
                                string EncodingName = null, [CliParameter('O',
                                    "Comma separated name=value pairs of options to pass to filesystem plugin")]
                                string Options = null, [CliParameter('d', "Shows debug output from plugins.")]
                                bool debug = false,    [CliParameter('v', "Shows verbose output.")]
                                bool verbose = false)
        {
            Ls.DoLs(new LsOptions
            {
                Verbose      = verbose,
                Debug        = debug,
                InputFile    = InputFile,
                Long         = Long,
                EncodingName = EncodingName,
                Options      = Options
            });
        }

        [CliCommand("extract-files", "Extracts all files in disc image.")]
        public static void Extract([CliParameter('i', "Disc image.")] string InputFile,
                                   [CliParameter('o',
                                       "Directory where extracted files will be created. Will abort if it exists.")]
                                   string OutputDir,
                                   [CliParameter(                            'x', "Extract extended attributes if present.")]
                                   bool Xattrs = false,        [CliParameter('e', "Name of character encoding to use.")]
                                   string EncodingName = null, [CliParameter('O',
                                       "Comma separated name=value pairs of options to pass to filesystem plugin")]
                                   string Options = null, [CliParameter('d', "Shows debug output from plugins.")]
                                   bool debug = false,    [CliParameter('v', "Shows verbose output.")]
                                   bool verbose = false)
        {
            ExtractFiles.DoExtractFiles(new ExtractFilesOptions
            {
                Verbose      = verbose,
                Debug        = debug,
                InputFile    = InputFile,
                Xattrs       = Xattrs,
                EncodingName = EncodingName,
                Options      = Options,
                OutputDir    = OutputDir
            });
        }
    }
}