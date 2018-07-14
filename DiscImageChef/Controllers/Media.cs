using CommandAndConquer.CLI.Attributes;
using DiscImageChef.Commands;

namespace DiscImageChef.Controllers
{
    [CliController("media", "Handles medias")]
    public class Media
    {
        [CliCommand("info", "Gets information about the media inserted on a device.")]
        public static void Info([CliParameter('i', "Device path.")] string DevicePath,
                                [CliParameter('w',
                                    "Write binary responses from device with that prefix.")]
                                string OutputPrefix = null, [CliParameter('d', "Shows debug output from plugins.")]
                                bool debug = false,         [CliParameter('v', "Shows verbose output.")]
                                bool verbose = false)
        {
            MediaInfo.DoMediaInfo(new MediaInfoOptions
            {
                Verbose      = verbose,
                Debug        = debug,
                DevicePath   = DevicePath,
                OutputPrefix = OutputPrefix
            });
        }

        [CliCommand("scan", "Scans the media inserted on a device.")]
        public static void Scan([CliParameter('i', "Device path.")] string DevicePath,
                                [CliParameter('m',
                                    "Write a log of the scan in the format used by MHDD.")]
                                string MhddLogPath = null, [CliParameter('b',
                                    "Write a log of the scan in the format used by ImgBurn.")]
                                string IbgLogPath = null, [CliParameter('d', "Shows debug output from plugins.")]
                                bool debug = false,       [CliParameter('v', "Shows verbose output.")]
                                bool verbose = false)
        {
            MediaScan.DoMediaScan(new MediaScanOptions
            {
                Verbose     = verbose,
                Debug       = debug,
                DevicePath  = DevicePath,
                MhddLogPath = MhddLogPath,
                IbgLogPath  = IbgLogPath
            });
        }

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
            DumpMedia.DoDumpMedia(new DumpMediaOptions
            {
                Verbose      = verbose,
                Debug        = debug,
                DevicePath   = DevicePath,
                OutputFile   = OutputFile,
                StopOnError  = StopOnError,
                Force        = Force,
                RetryPasses  = RetryPasses,
                Persistent   = Persistent,
                Resume       = Resume,
                LeadIn       = LeadIn,
                EncodingName = EncodingName,
                OutputFormat = OutputFormat,
                Options      = Options,
                CicmXml      = CicmXml,
                Skip         = Skip,
                NoMetadata   = NoMetadata,
                NoTrim       = NoTrim
            });
        }
    }
}