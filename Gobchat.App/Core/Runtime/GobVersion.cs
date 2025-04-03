/*******************************************************************************
 * Copyright (C) 2019-2025 MarbleBag
 *
 * This program is free software: you can redistribute it and/or modify it under
 * the terms of the GNU Affero General Public License as published by the Free
 * Software Foundation, version 3.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>
 *
 * SPDX-License-Identifier: AGPL-3.0-only
 *******************************************************************************/

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Gobchat.Core.Runtime
{
    public sealed class GobVersion : ICloneable, IComparable, IComparable<GobVersion>, IEquatable<GobVersion>
    {
        public uint Major { get; }
        public uint Minor { get; }
        public uint Patch { get; }

        public uint PreRelease { get; }

        public bool IsPreRelease { get => PreRelease != 0; }

        public GobVersion() : this(0, 0, 0, 0)
        {
        }

        public GobVersion(Version version) : this((uint)Math.Max(0, version.Major), (uint)Math.Max(0, version.Minor), (uint)Math.Max(0, version.Build), (uint)Math.Max(0, version.Revision))
        {
        }

        public GobVersion(uint major, uint minor) : this(major, minor, 0, 0)
        {
        }

        public GobVersion(uint major, uint minor, uint patch) : this(major, minor, patch, 0)
        {
        }

        public GobVersion(uint major, uint minor, uint patch, uint preRelease)
        {
            if (major < 0) throw new ArgumentException("Less than 0", nameof(major));
            if (minor < 0) throw new ArgumentException("Less than 0", nameof(minor));
            if (patch < 0) throw new ArgumentException("Less than 0", nameof(patch));
            if (preRelease < 0) throw new ArgumentException("Less than 0", nameof(preRelease));

            Major = major;
            Minor = minor;
            Patch = patch;
            PreRelease = preRelease;
        }

        public GobVersion(string version)
        {
            if (!TryParse(version, out var gobVersion))
                throw new ArgumentException("Is not a valid version", nameof(version));

            Major = gobVersion.Major;
            Minor = gobVersion.Minor;
            Patch = gobVersion.Patch;
            PreRelease = gobVersion.PreRelease;
        }

        private static uint Parse(GroupCollection collection, string groupName)
        {
            var capture = collection[groupName];
            return capture.Success ? uint.Parse(capture.Value, CultureInfo.InvariantCulture) : 0;
        }

        public static bool TryParse(string version, out GobVersion gobVersion)
        {
            gobVersion = null;

            var regex = new Regex(@"^v?(?<major>[1-9]*[0-9])(?:\.(?<minor>[1-9]*[0-9]))?(?:\.(?<patch>[1-9]*[0-9]))?(?:-([a-zA-Z]*\.)?(?<prerelease>[1-9]*[0-9]))?$");
            var match = regex.Match(version);

            if (!match.Success)
                return false;

            var groups = match.Groups;
            var major = Parse(groups, "major");
            var minor = Parse(groups, "minor");
            var patch = Parse(groups, "patch");
            var preRelease = Parse(groups, "prerelease");

            gobVersion = new GobVersion(major, minor, patch, preRelease);
            return true;
        }

        public static bool operator ==(GobVersion v1, GobVersion v2)
        {
            if (ReferenceEquals(v1, v2))
                return true;
            if (v1 is null || v2 is null)
                return false;

            return v1.Major == v2.Major && v1.Minor == v2.Minor && v1.Patch == v2.Patch && v1.PreRelease == v2.PreRelease;
        }

        public static bool operator !=(GobVersion v1, GobVersion v2)
        {
            return !(v1 == v2);
        }

        public static bool operator <(GobVersion v1, GobVersion v2)
        {
            if (v1.Major < v2.Major) return true;
            if (v1.Major > v2.Major) return false;

            if (v1.Minor < v2.Minor) return true;
            if (v1.Minor > v2.Minor) return false;

            if (v1.Patch < v2.Patch) return true;
            if (v1.Patch > v2.Patch) return false;

            if (v1.IsPreRelease)
            {
                if (v2.IsPreRelease)
                    return v1.PreRelease < v2.PreRelease;
                else
                    return true;
            }
            else
            {
                return false;
            }
        }

        public static bool operator >(GobVersion v1, GobVersion v2)
        {
            return v2 < v1;
        }

        public static bool operator <=(GobVersion v1, GobVersion v2)
        {
            if (v1.Major < v2.Major) return true;
            if (v1.Major > v2.Major) return false;

            if (v1.Minor < v2.Minor) return true;
            if (v1.Minor > v2.Minor) return false;

            if (v1.Patch < v2.Patch) return true;
            if (v1.Patch > v2.Patch) return false;

            if (v1.IsPreRelease)
            {
                if (v2.IsPreRelease)
                    return v1.PreRelease <= v2.PreRelease;
                else
                    return true;
            }
            else
            {
                if (v2.IsPreRelease)
                    return false;
                return true;
            }
        }

        public static bool operator >=(GobVersion v1, GobVersion v2)
        {
            return v2 <= v1;
        }

        public int CompareTo(GobVersion other)
        {
            if (other == null) return 1;
            if (ReferenceEquals(this, other)) return 0;
            if (this < other) return -1;
            if (this > other) return 1;
            return 0;
        }

        public object Clone()
        {
            return this;
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as GobVersion);
        }

        public bool Equals(GobVersion other)
        {
            return other is GobVersion version &&
                   Major == version.Major &&
                   Minor == version.Minor &&
                   Patch == version.Patch &&
                   PreRelease == version.PreRelease;
        }

        public override bool Equals(object obj)
        {
            return obj is GobVersion version &&
                   Major == version.Major &&
                   Minor == version.Minor &&
                   Patch == version.Patch &&
                   PreRelease == version.PreRelease;
        }

        public override int GetHashCode()
        {
            var hashCode = 829241888;
            hashCode = hashCode * -1521134295 + Major.GetHashCode();
            hashCode = hashCode * -1521134295 + Minor.GetHashCode();
            hashCode = hashCode * -1521134295 + Patch.GetHashCode();
            hashCode = hashCode * -1521134295 + PreRelease.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            if (IsPreRelease)
                return $"{Major}.{Minor}.{Patch}-{PreRelease}";
            else
                return $"{Major}.{Minor}.{Patch}";
        }
    }
}