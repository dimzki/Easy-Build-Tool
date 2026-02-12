using System;

namespace Dimzki.Easybuildtool
{
    [Serializable]
    public struct SemanticVersion : IComparable<SemanticVersion>, IEquatable<SemanticVersion>
    {
        public int Major;
        public int Minor;
        public int Hotfix;

        public SemanticVersion(int major, int minor, int hotfix)
        {
            Major = Math.Max(0, major);
            Minor = Math.Max(0, minor);
            Hotfix = Math.Max(0, hotfix);
        }

        public static SemanticVersion Default => new SemanticVersion(0, 0, 0);

        public static SemanticVersion Parse(string versionString)
        {
            if (TryParse(versionString, out var result))
                return result;
            return Default;
        }

        public static bool TryParse(string versionString, out SemanticVersion result)
        {
            result = Default;
            if (string.IsNullOrWhiteSpace(versionString))
                return false;

            string[] parts = versionString.Trim().Split('.');
            if (parts.Length != 3)
                return false;

            if (!int.TryParse(parts[0], out int major) ||
                !int.TryParse(parts[1], out int minor) ||
                !int.TryParse(parts[2], out int hotfix))
                return false;

            result = new SemanticVersion(major, minor, hotfix);
            return true;
        }

        public override string ToString() => $"{Major}.{Minor}.{Hotfix}";

        public int CompareTo(SemanticVersion other)
        {
            int cmp = Major.CompareTo(other.Major);
            if (cmp != 0) return cmp;
            cmp = Minor.CompareTo(other.Minor);
            if (cmp != 0) return cmp;
            return Hotfix.CompareTo(other.Hotfix);
        }

        public bool Equals(SemanticVersion other) =>
            Major == other.Major && Minor == other.Minor && Hotfix == other.Hotfix;

        public override bool Equals(object obj) => obj is SemanticVersion other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Major, Minor, Hotfix);

        public static bool operator ==(SemanticVersion a, SemanticVersion b) => a.Equals(b);
        public static bool operator !=(SemanticVersion a, SemanticVersion b) => !a.Equals(b);
        public static bool operator <(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) < 0;
        public static bool operator >(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) > 0;
        public static bool operator <=(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) <= 0;
        public static bool operator >=(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) >= 0;
    }
}
