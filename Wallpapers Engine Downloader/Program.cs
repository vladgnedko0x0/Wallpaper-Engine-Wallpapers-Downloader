using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace WallpaperEngineDownloader
{
    class Program
    {
        private static string saveLocation = "Not set";
        private static readonly Dictionary<string, string> accounts = new Dictionary<string, string>
        {
            { "ruiiixx", DecodeBase64("UzY3R0JUQjgzRDNZ") },
            { "premexilmenledgconis", DecodeBase64("M3BYYkhaSmxEYg==") },
            { "vAbuDy", DecodeBase64("Qm9vbHE4dmlw") },
            { "adgjl1182", DecodeBase64("UUVUVU85OTk5OQ==") },
            { "gobjj16182", DecodeBase64("enVvYmlhbzgyMjI=") },
            { "787109690", DecodeBase64("SHVjVXhZTVFpZzE1") }
        };

        static void Main(string[] args)
        {
            LoadSaveLocation();

            Console.WriteLine("Wallpaper Engine Workshop Downloader");
            Console.WriteLine("------------------------------------");

            while (true)
            {
                Console.WriteLine("\nOptions:");
                Console.WriteLine("1. Set save location");
                Console.WriteLine("2. Enter workshop items (file IDs or links)");
                Console.WriteLine("3. Exit");
                Console.Write("Select an option: ");

                var input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        SetSaveLocation();
                        break;
                    case "2":
                        StartDownload();
                        break;
                    case "3":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        private static string DecodeBase64(string encoded)
        {
            var data = Convert.FromBase64String(encoded);
            return System.Text.Encoding.UTF8.GetString(data);
        }

        private static void LoadSaveLocation()
        {
            try
            {
                if (File.Exists("lastsavelocation.cfg"))
                {
                    var directory = File.ReadAllText("lastsavelocation.cfg").Trim();
                    if (Directory.Exists(directory))
                        saveLocation = directory;
                }
            }
            catch
            {
                saveLocation = "Not set";
            }

            Console.WriteLine($"Save location: {saveLocation}");
        }

        private static void SetSaveLocation()
        {
            Console.Write("Enter the path to Wallpaper Engine (e.g., C:\\Program Files\\Wallpaper Engine\\projects\\myprojects): ");
            var directory = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                Console.WriteLine("Invalid save location: The specified directory does not exist.");
                return;
            }

            // Проверка, содержит ли путь поддиректории "projects\myprojects"
            var projectsPath = Path.Combine(directory, "projects", "myprojects");
            if (!directory.Contains("projects\\myprojects") && !Directory.Exists(projectsPath))
            {
                Console.WriteLine("Invalid save location: The path does not contain \\projects\\myprojects.");
                return;
            }

            saveLocation = directory.Contains("projects\\myprojects") ? directory : projectsPath;
            File.WriteAllText("lastsavelocation.cfg", saveLocation);
            Console.WriteLine($"Save location set to: {saveLocation}");
        }


        private static void StartDownload()
        {
            Console.WriteLine("Enter workshop links or file IDs (one per line, type 'done' to finish):");
            var links = new List<string>();
            while (true)
            {
                var input = Console.ReadLine();
                if (input?.ToLower() == "done")
                    break;
                links.Add(input);
            }

            Console.WriteLine("Available accounts:");
            foreach (var account in accounts.Keys)
            {
                Console.WriteLine($"- {account}");
            }
            Console.Write("Select an account: ");
            var accountName = Console.ReadLine();

            if (!accounts.ContainsKey(accountName))
            {
                Console.WriteLine("Invalid account. Please try again.");
                return;
            }

            foreach (var link in links)
            {
                var match = Regex.Match(link, @"\b\d{8,10}\b");
                if (match.Success)
                {
                    RunCommand(match.Value, accountName);
                }
                else
                {
                    Console.WriteLine($"Invalid link: {link}");
                }
            }
        }

        private static void RunCommand(string pubFileId, string accountName)
        {
            Console.WriteLine($"----------Downloading {pubFileId}----------");

            if (string.IsNullOrWhiteSpace(saveLocation) || saveLocation == "Not set")
            {
                Console.WriteLine("Error: Save location is not set correctly.");
                return;
            }

            // Проверяем, содержит ли путь "projects\myprojects"
            var targetDirectory = saveLocation.Contains("projects\\myprojects")
                ? saveLocation
                : Path.Combine(saveLocation, "projects", "myprojects");

            if (!Directory.Exists(targetDirectory))
            {
                Console.WriteLine("Invalid save location: The specified directory does not contain \\projects\\myprojects");
                return;
            }

            // Опция для директории загрузки
            var pubfileDirectory = Path.Combine(targetDirectory, pubFileId);
            var dirOption = $"-dir \"{pubfileDirectory}\"";

            // Команда для выполнения
            var command = $"DepotDownloadermod.exe -app 431960 -pubfile {pubFileId} -verify-all -username {accountName} -password {accounts[accountName]} {dirOption}";

            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = $"/C {command}";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = false;

                    process.Start();

                    // Чтение и вывод результата выполнения команды
                    string line;
                    while ((line = process.StandardOutput.ReadLine()) != null)
                    {
                        Console.WriteLine(line);
                    }

                    process.WaitForExit();
                }

                Console.WriteLine($"----------Download of {pubFileId} finished----------");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while running the command: {ex.Message}");
            }
        }

    }
}
