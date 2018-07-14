using CommandAndConquer.CLI.Attributes;
using DiscImageChef.Commands;

namespace DiscImageChef.Controllers
{
    [CliController("device", "Handles devices")]
    public class Device
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

        [CliCommand("report", "Tests the device capabilities and creates an XML report of them.")]
        public static void Info([CliParameter(                    'i', "Device path.")] string DevicePath,
                                [CliParameter(                    'd', "Shows debug output from plugins.")]
                                bool debug = false, [CliParameter('v', "Shows verbose output.")]
                                bool verbose = false)
        {
            DeviceReport.DoDeviceReport(new DeviceReportOptions
            {
                Verbose    = verbose,
                Debug      = debug,
                DevicePath = DevicePath
            });
        }

        [CliCommand("list", "Lists all connected devices.")]
        public static void List([CliParameter(                    'd', "Shows debug output from plugins.")]
                                bool debug = false, [CliParameter('v', "Shows verbose output.")]
                                bool verbose = false)
        {
            ListDevices.DoListDevices(new ListDevicesOptions {Verbose = verbose, Debug = debug});
        }
    }
}