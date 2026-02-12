using System;
using System.Diagnostics;
using UnityEngine;

namespace Dimzki.Easybuildtool.Editor
{
    public static class GitHelper
    {
        public static bool IsGitRepository()
        {
            string projectRoot = System.IO.Directory.GetParent(Application.dataPath).FullName;
            return System.IO.Directory.Exists(System.IO.Path.Combine(projectRoot, ".git"));
        }

        public static int? GetCommitCount()
        {
            if (!IsGitRepository())
                return null;

            string output = RunGitCommand("rev-list --count HEAD");
            if (string.IsNullOrEmpty(output))
                return null;

            if (int.TryParse(output.Trim(), out int count))
                return count;

            return null;
        }

        public static string GetShortCommitHash()
        {
            if (!IsGitRepository())
                return null;

            string output = RunGitCommand("rev-parse --short HEAD");
            return string.IsNullOrEmpty(output) ? null : output.Trim();
        }

        private static string RunGitCommand(string arguments)
        {
            try
            {
                string projectRoot = System.IO.Directory.GetParent(Application.dataPath).FullName;

                var startInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    WorkingDirectory = projectRoot,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                    return null;

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(5000);

                return process.ExitCode == 0 ? output : null;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[EasyBuildTool] Git command failed: {e.Message}");
                return null;
            }
        }
    }
}
