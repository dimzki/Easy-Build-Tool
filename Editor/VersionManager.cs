using System;

namespace Dimzki.Easybuildtool.Editor
{
    public class VersionManager
    {
        public SemanticVersion LastVersion { get; private set; }
        public SemanticVersion NewVersion { get; private set; }

        public bool IsVersionUnchanged => NewVersion == LastVersion;

        public void LoadFromConfig()
        {
            LastVersion = ConfigHandler.LoadLastVersion();
            NewVersion = LastVersion;
        }

        public void CommitVersion()
        {
            ConfigHandler.SaveLastVersion(NewVersion);
            LastVersion = NewVersion;
        }

        // === INCREMENT ===

        public void IncrementMajor()
        {
            NewVersion = new SemanticVersion(NewVersion.Major + 1, 0, 0);
        }

        public void IncrementMinor()
        {
            NewVersion = new SemanticVersion(NewVersion.Major, NewVersion.Minor + 1, 0);
        }

        public void IncrementHotfix()
        {
            NewVersion = new SemanticVersion(NewVersion.Major, NewVersion.Minor, NewVersion.Hotfix + 1);
        }

        // === DECREMENT ===

        public void DecrementMajor()
        {
            var candidate = new SemanticVersion(NewVersion.Major - 1, NewVersion.Minor, NewVersion.Hotfix);
            NewVersion = ClampToMinimum(candidate);
        }

        public void DecrementMinor()
        {
            var candidate = new SemanticVersion(NewVersion.Major, NewVersion.Minor - 1, NewVersion.Hotfix);
            NewVersion = ClampToMinimum(candidate);
        }

        public void DecrementHotfix()
        {
            var candidate = new SemanticVersion(NewVersion.Major, NewVersion.Minor, NewVersion.Hotfix - 1);
            NewVersion = ClampToMinimum(candidate);
        }

        // === RESET ===

        public void ResetMajor()
        {
            NewVersion = LastVersion;
        }

        public void ResetMinor()
        {
            int minMinor = GetMinimumMinor(NewVersion.Major);
            int minHotfix = GetMinimumHotfix(NewVersion.Major, minMinor);
            NewVersion = new SemanticVersion(NewVersion.Major, minMinor, minHotfix);
        }

        public void ResetHotfix()
        {
            int minHotfix = GetMinimumHotfix(NewVersion.Major, NewVersion.Minor);
            NewVersion = new SemanticVersion(NewVersion.Major, NewVersion.Minor, minHotfix);
        }

        private SemanticVersion ClampToMinimum(SemanticVersion candidate)
        {
            candidate = new SemanticVersion(
                Math.Max(candidate.Major, 0),
                Math.Max(candidate.Minor, 0),
                Math.Max(candidate.Hotfix, 0)
            );

            if (candidate < LastVersion)
                return LastVersion;

            return candidate;
        }

        private int GetMinimumMinor(int major)
        {
            if (major > LastVersion.Major) return 0;
            return LastVersion.Minor;
        }

        private int GetMinimumHotfix(int major, int minor)
        {
            if (major > LastVersion.Major) return 0;
            if (major == LastVersion.Major && minor > LastVersion.Minor) return 0;
            return LastVersion.Hotfix;
        }
    }
}
