using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Dimzki.Easybuildtool.Editor
{
    public static class BuildExecutor
    {
        public static bool Build(
            BuildPlatformTarget platform,
            ScriptingImplementation scriptingBackend,
            bool developmentBuild,
            string[] scenePaths,
            SemanticVersion version,
            bool zipOutput)
        {
            BuildTarget target = BuildSettingsData.ToBuildTarget(platform);
            BuildTargetGroup targetGroup = BuildSettingsData.ToBuildTargetGroup(platform);

            if (!BuildPipeline.IsBuildTargetSupported(targetGroup, target))
            {
                EditorUtility.DisplayDialog(
                    "Build Error",
                    $"Build target {platform} is not installed.\nInstall it via Unity Hub.",
                    "OK");
                return false;
            }

            string folderName = GenerateBuildFolderName(version);
            string productName = SanitizeProductName(PlayerSettings.productName);
            string extension = BuildSettingsData.GetExecutableExtension(platform);

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string buildFolder = Path.Combine(projectRoot, "Builds", platform.ToString(), folderName);
            Directory.CreateDirectory(buildFolder);

            string executableName = productName + extension;
            string locationPath = Path.Combine(buildFolder, executableName);

            PlayerSettings.SetScriptingBackend(targetGroup, scriptingBackend);

            if (platform == BuildPlatformTarget.AndroidAAB)
                EditorUserBuildSettings.buildAppBundle = true;
            else if (platform == BuildPlatformTarget.AndroidAPK)
                EditorUserBuildSettings.buildAppBundle = false;

            BuildOptions options = BuildOptions.None;
            if (developmentBuild)
                options |= BuildOptions.Development;

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenePaths,
                locationPathName = locationPath,
                target = target,
                options = options
            };

            Debug.Log($"[EasyBuildTool] Starting build: {platform} -> {locationPath}");

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (report.summary.result != BuildResult.Succeeded)
            {
                EditorUtility.DisplayDialog(
                    "Build Failed",
                    $"Build for {platform} failed.\nErrors: {report.summary.totalErrors}\nCheck the Console for details.",
                    "OK");
                return false;
            }

            Debug.Log($"[EasyBuildTool] Build succeeded: {locationPath} ({report.summary.totalSize} bytes)");

            if (zipOutput)
            {
                string zipPath = buildFolder + ".zip";
                if (!CompressToZip(buildFolder, zipPath))
                    return false;

                Debug.Log($"[EasyBuildTool] ZIP created: {zipPath}");
            }

            EditorUtility.RevealInFinder(buildFolder);

            return true;
        }

        public static string GenerateBuildFolderName(SemanticVersion version)
        {
            string productName = SanitizeProductName(PlayerSettings.productName);
            string versionStr = version.ToString();
            string dateStr = DateTime.Now.ToString("dd-MM-yyyy");

            string shortHash = GitHelper.GetShortCommitHash();

            if (!string.IsNullOrEmpty(shortHash))
                return $"{productName}_{versionStr}_{shortHash}_{dateStr}";

            return $"{productName}_{versionStr}_{dateStr}";
        }

        private static string SanitizeProductName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "Build";
            return Regex.Replace(name, @"[^a-zA-Z0-9_\-]", "");
        }

        private static bool CompressToZip(string sourceDirectory, string zipFilePath)
        {
            try
            {
                if (File.Exists(zipFilePath))
                    File.Delete(zipFilePath);

                ZipFile.CreateFromDirectory(sourceDirectory, zipFilePath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EasyBuildTool] Failed to create zip: {ex.Message}");
                EditorUtility.DisplayDialog(
                    "ZIP Error",
                    $"Build succeeded but ZIP creation failed:\n{ex.Message}",
                    "OK");
                return false;
            }
        }
    }
}
