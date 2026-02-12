using System.Collections.Generic;
using System.IO;

namespace Dimzki.Easybuildtool
{
    public static class ConfigHandler
    {
        private const string CONFIG_FILE_PATH = "Assets/Resources/BuildConfig.ini";
        private const string SECTION_VERSION = "Version";
        private const string KEY_LAST_VERSION = "LastVersion";

        public static SemanticVersion LoadLastVersion()
        {
            if (!File.Exists(CONFIG_FILE_PATH))
                return SemanticVersion.Default;

            string value = ReadIniValue(CONFIG_FILE_PATH, SECTION_VERSION, KEY_LAST_VERSION);
            if (string.IsNullOrEmpty(value))
                return SemanticVersion.Default;

            return SemanticVersion.Parse(value);
        }

        public static void SaveLastVersion(SemanticVersion version)
        {
            string directory = Path.GetDirectoryName(CONFIG_FILE_PATH);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            WriteIniValue(CONFIG_FILE_PATH, SECTION_VERSION, KEY_LAST_VERSION, version.ToString());

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        private static string ReadIniValue(string filePath, string section, string key)
        {
            string[] lines = File.ReadAllLines(filePath);
            string currentSection = "";

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentSection = line.Substring(1, line.Length - 2);
                    continue;
                }

                if (currentSection != section)
                    continue;

                int eqIndex = line.IndexOf('=');
                if (eqIndex < 0)
                    continue;

                string k = line.Substring(0, eqIndex).Trim();
                if (k == key)
                    return line.Substring(eqIndex + 1).Trim();
            }

            return null;
        }

        private static void WriteIniValue(string filePath, string section, string key, string value)
        {
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, $"[{section}]\n{key}={value}\n");
                return;
            }

            List<string> lines = new List<string>(File.ReadAllLines(filePath));
            string currentSection = "";
            bool found = false;
            bool sectionFound = false;

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentSection = line.Substring(1, line.Length - 2);
                    if (currentSection == section)
                        sectionFound = true;
                    continue;
                }

                if (currentSection != section)
                    continue;

                int eqIndex = line.IndexOf('=');
                if (eqIndex < 0)
                    continue;

                string k = line.Substring(0, eqIndex).Trim();
                if (k == key)
                {
                    lines[i] = $"{key}={value}";
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                if (sectionFound)
                {
                    // Find section header and insert after it
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].Trim() == $"[{section}]")
                        {
                            lines.Insert(i + 1, $"{key}={value}");
                            break;
                        }
                    }
                }
                else
                {
                    lines.Add("");
                    lines.Add($"[{section}]");
                    lines.Add($"{key}={value}");
                }
            }

            File.WriteAllText(filePath, string.Join("\n", lines) + "\n");
        }
    }
}
