using CommandAndConquer.CLI.Attributes;

namespace DiscImageChef.Controllers
{
    [CliController("about", "Shows information about DiscImageChef")]
    public class About
    {
        [CliCommand("formats", "Lists all supported disc images, partition schemes and file systems.")]
        public static void Formats([CliParameter(                    'd', "Shows debug output from plugins.")]
                                   bool debug = false, [CliParameter('v', "Shows verbose output.")]
                                   bool verbose = false)
        {
            Commands.Formats.ListFormats(new FormatsOptions {Verbose = verbose, Debug = debug});
        }

        [CliCommand("benchmark", "Benchmarks hashing and entropy calculation.")]
        public static void Benchmark([CliParameter(                      'b', "Block size.")] int BlockSize = 512,
                                     [CliParameter(                      's', "Buffer size in mebibytes.")]
                                     int BufferSize = 128, [CliParameter('d', "Shows debug output from plugins.")]
                                     bool debug = false,   [CliParameter('v', "Shows verbose output.")]
                                     bool verbose = false)
        {
            Commands.Benchmark.DoBenchmark(new BenchmarkOptions {Verbose = verbose, Debug = debug});
        }

        [CliCommand("list-encodings", "Lists all supported text encodings and code pages.")]
        public static void ListEncodings()
        {
            Commands.ListEncodings.DoList();
        }

        [CliCommand("list-options", "Lists all options supported by read-only filesystems and writable media images.")]
        public static void ListOptions()
        {
            Commands.ListOptions.DoList();
        }

        [CliCommand("configure", "Configures user settings and statistics.")]
        public static void Configure()
        {
            Commands.Configure.DoConfigure(false);
        }

        [CliCommand("stats", "Shows statistics.")]
        public static void Statistics()
        {
            Commands.Statistics.ShowStats();
        }
    }
}