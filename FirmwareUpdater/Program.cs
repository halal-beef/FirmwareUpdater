using System.Diagnostics;
using System.Threading;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("[INFO] Extracting Flasher...");
        Directory.CreateDirectory("tmp");
        ExtractResource("FirmwareUpdater.Resources.cygiconv-2.dll", Directory.GetCurrentDirectory() + "\\tmp\\cygiconv-2.dll");
        ExtractResource("FirmwareUpdater.Resources.cygintl-8.dll", Directory.GetCurrentDirectory() + "\\tmp\\cygintl-8.dll");
        ExtractResource("FirmwareUpdater.Resources.cygwin1.dll", Directory.GetCurrentDirectory() + "\\tmp\\cygwin1.dll");
        ExtractResource("FirmwareUpdater.Resources.dd.exe", Directory.GetCurrentDirectory() + "\\tmp\\dd.exe");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[INFO] Sucessfully Extracted!");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[INFO] Flashing Will Commence In 3 Seconds! If You Do Not Want To Flash Press Any Key To Abort!");

        Console.ForegroundColor = ConsoleColor.Blue;

        Task countdownTask = CountdownAsync();
        Task keyPressTask = WaitForKeyPressAsync();

        await Task.WhenAny(countdownTask, keyPressTask);

        if (countdownTask.IsCompleted)
        {
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("[INFO] Flash Confirmed! Flashing Will Now Start!");

            Console.ForegroundColor = ConsoleColor.Blue;

            Console.WriteLine("[INFO] Downloading UEFI...");
            DownloadFile("https://github.com/erdilS/Port-Windows-11-Xiaomi-Pad-5/releases/download/1.0/boot-nabu.img", Directory.GetCurrentDirectory() + "\\tmp\\uefi.img");

            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("[INFO] Download Complete!");

            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("[WARNING] Do Not Turn Off Your Tablet Or Interrupt The Installation Process!");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("[INFO] Flashing Slot A...");
            ProcessStartInfo slotA = new ProcessStartInfo { CreateNoWindow = true, RedirectStandardOutput = false, RedirectStandardError = false, FileName = "cmd.exe", Arguments = $@"/c {Directory.GetCurrentDirectory()}\\tmp\\dd.exe if={Directory.GetCurrentDirectory() + "\\tmp\\uefi.img"} of=/dev/sde14 bs=16M" };
            Process.Start(slotA);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[INFO] Slot A Flashed!");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("[INFO] Flashing Slot B...");
            ProcessStartInfo slotB = new ProcessStartInfo { CreateNoWindow = true, RedirectStandardOutput = false, RedirectStandardError = false, FileName = "cmd.exe", Arguments = $@"/c {Directory.GetCurrentDirectory()}\\tmp\\dd.exe if={Directory.GetCurrentDirectory() + "\\tmp\\uefi.img"} of=/dev/sde37 bs=16M" };
            Process.Start(slotB);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[INFO] Slot B Flashed!");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("[INFO] Cleaning Up Directories...");
            Directory.Delete(Directory.GetCurrentDirectory() + "\\tmp", true);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[INFO] Directories Cleaned!");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("[INFO] Do you want to reboot Windows? [Y/N]");
            char response = Char.ToLower(Console.ReadKey().KeyChar);

            switch (response)
            {
                case 'y':
                    ProcessStartInfo reboot = new ProcessStartInfo { CreateNoWindow = true, RedirectStandardOutput = false, RedirectStandardError = false, FileName = "cmd.exe", Arguments = $@"/c shutdown.exe /r /f /t 0" };
                    Process.Start(reboot);
                    break;

                case 'n':
                    Environment.Exit(0);
                    break;

                default:
                    break;
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;


            Console.WriteLine("[INFO] Key Pressed! Exiting...");
            Environment.Exit(0);
            cancellationTokenSource.Cancel();
        }

        Thread.Sleep(-1);

      
    }

    private static async Task CountdownAsync()
    {
        for (int i = 3; i > 0; i--)
        {
            Console.WriteLine($"[INFO] {i}...");
            await Task.Delay(1000);
        }
    }

    private static CancellationTokenSource cancellationTokenSource;
    private static async Task WaitForKeyPressAsync()
    {
        cancellationTokenSource = new CancellationTokenSource();
        while (!Console.KeyAvailable)
        {
            await Task.Delay(100);
        }
        cancellationTokenSource.Cancel();
    }
    private static void DownloadFile(string Url, string Filename)
    {
        HttpClient hc = new();
        hc.Timeout = Timeout.InfiniteTimeSpan;
        using (FileStream fs0 = File.Create(Filename))
        {
            HttpResponseMessage hrm4 = hc.GetAsync(Url).GetAwaiter().GetResult();
            hrm4.Content.CopyToAsync(fs0).GetAwaiter().GetResult();
        }
    }

    private static void ExtractResource(string resourceName, string outputPath)
    {
        Stream stream = typeof(Program).Assembly.GetManifestResourceStream(resourceName);
        byte[] bytes = new byte[(int)stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        File.WriteAllBytes(outputPath, bytes);
    }
}