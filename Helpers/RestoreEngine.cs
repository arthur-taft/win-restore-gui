using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using win_restore.Models;

namespace win_restore.Helpers
{
    public static class RestoreEngine
    {
        public static readonly string TempLogPath =
            Path.Combine(Path.GetTempPath(), "robo_restore.log");

        // Same as BackupEngine.GetDrives() but excludes C: 
        // since that's always the destination, never the source
        public static Dictionary<string, DriveInfo> GetDrives()
        {
            var driveMapping = new Dictionary<string, DriveInfo>();

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady) continue;
                if (drive.Name.StartsWith("C", StringComparison.OrdinalIgnoreCase)) continue;

                string label = string.IsNullOrEmpty(drive.VolumeLabel) ? "Local Volume" : drive.VolumeLabel;
                double freeGB = Math.Round(drive.AvailableFreeSpace / 1073741824.0, 1);
                double totalGB = Math.Round(drive.TotalSize / 1073741824.0, 1);

                string displayText = $"Drive {drive.Name[0]}: [{label}] ({freeGB} GB Free of {totalGB} GB) - {drive.DriveType}";
                driveMapping[displayText] = drive;
            }

            return driveMapping;
        }

        // Scans the backup drive for folders that exist under DriveLetter:\username\
        // and builds restore items pointing them back to C:\Users\username\
        public static List<BackupItem> BuildRestoreItems(string backupRoot)
        {
            var items = new List<BackupItem>();
            string destRoot = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (!Directory.Exists(backupRoot))
                return items;

            // Scan every folder found in the backup root and offer it for restore
            foreach (var dir in Directory.GetDirectories(backupRoot, "*", SearchOption.TopDirectoryOnly))
            {
                string folderName = Path.GetFileName(dir);

                items.Add(new BackupItem
                {
                    Name = folderName,
                    Src = dir,                                    // FROM: backup drive
                    Dest = Path.Combine(destRoot, folderName),     // TO: C:\Users\username\
                    Enabled = true
                });
            }

            return items;
        }

        // Mirrors RunBackup but Src/Dest are already set correctly by BuildRestoreItems
        public static Dictionary<string, List<string>> RunRestore(
            List<BackupItem> items,
            IProgress<CopyProgress> progress)
        {
            int threads = Environment.ProcessorCount;
            string tempLog = TempLogPath;
            int totalEnabled = items.Count(i => i.Enabled);
            int completed = 0;

            var auditLogs = new Dictionary<string, List<string>>();

            foreach (var item in items)
            {
                if (!item.Enabled)
                {
                    progress.Report(new CopyProgress
                    { ItemName = item.Name, Status = "Skipped", ItemIndex = completed, TotalItems = totalEnabled });
                    continue;
                }

                if (!Directory.Exists(item.Src))
                {
                    progress.Report(new CopyProgress
                    { ItemName = item.Name, Status = "Not Found", ItemIndex = completed, TotalItems = totalEnabled });
                    completed++;
                    continue;
                }

                progress.Report(new CopyProgress
                { ItemName = item.Name, Status = "Copying", ItemIndex = completed, TotalItems = totalEnabled });

                // Ensure destination folder exists
                Directory.CreateDirectory(item.Dest);
                if (File.Exists(tempLog)) File.Delete(tempLog);

                // Robocopy in reverse — Src is the backup, Dest is the user profile
                var args = new List<string>
                {
                    $"\"{item.Src}\"",
                    $"\"{item.Dest}\"",
                    "/MIR", "/W:1", "/R:1", "/J",
                    $"/MT:{threads}",
                    "/NP", "/NDL",
                    $"/UNILOG:\"{tempLog}\""
                };

                var process = Process.Start(new ProcessStartInfo(
                    "robocopy.exe", string.Join(" ", args))
                {
                    WindowStyle = ProcessWindowStyle.Hidden
                });

                int lastLineCount = 0;

                while (!process.HasExited)
                {
                    if (File.Exists(tempLog))
                    {
                        try
                        {
                            string lastLine;
                            using (var fs = new FileStream(tempLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            using (var sr = new StreamReader(fs, Encoding.Unicode))
                            {
                                var lines = sr.ReadToEnd()
                                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                lastLine = lines.LastOrDefault(l => !string.IsNullOrWhiteSpace(l));
                                lastLineCount = lines.Length;
                            }

                            if (lastLine != null)
                            {
                                string clean = lastLine.Trim();
                                if (clean.Length > 80) clean = clean.Substring(0, 77) + "...";

                                progress.Report(new CopyProgress
                                {
                                    ItemName = item.Name,
                                    CurrentFile = clean,
                                    Status = "Copying",
                                    ItemIndex = completed,
                                    TotalItems = totalEnabled
                                });
                            }
                        }
                        catch { }
                    }
                    Thread.Sleep(150);
                }

                completed++;
                progress.Report(new CopyProgress
                { ItemName = item.Name, Status = "Complete", ItemIndex = completed, TotalItems = totalEnabled });

                if (File.Exists(tempLog))
                    auditLogs[item.Name] = new List<string>(File.ReadAllLines(tempLog));
            }

            return auditLogs;
        }

        // Identical to BackupEngine.VerifyLogs — parses robocopy summary for failures
        public static Dictionary<string, List<string>> VerifyLogs(Dictionary<string, List<string>> auditLogs)
        {
            var failures = new Dictionary<string, List<string>>();

            foreach (var kvp in auditLogs)
            {
                var log = kvp.Value;
                string fileLine = log.FirstOrDefault(l => l.TrimStart().StartsWith("Files :"));
                if (fileLine == null) continue;

                var parts = fileLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 7) continue;
                if (parts[6] == "0") continue;

                var errorLines = log.Where(l => l.Contains("ERROR")).ToList();
                if (!errorLines.Any())
                    errorLines.Add("Exact file names could not be parsed from the log output.");

                failures[kvp.Key] = errorLines;
            }

            return failures;
        }
    }
}