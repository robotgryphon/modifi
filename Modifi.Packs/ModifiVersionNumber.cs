using HashidsNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace Modifi.Packs {

    internal class ModifiVersionHelper {
        protected const string VersionKey = "modifi-versions";
        internal static HashidsNet.Hashids VersionHasher = new Hashids(VersionKey, 8, "abcdefghijklmnopqrstuvwxyz");
    }

    /// <summary>
    /// Represents a pack version.
    /// </summary>
    public struct ModifiVersionNumber {
        public int Major;
        public int Minor;

        internal static readonly string VERSION_1 = "opdckibo";

        public ModifiVersionNumber(int major = 1, int minor = 0) {
            this.Major = major;
            this.Minor = minor;
        }

        public ModifiVersionNumber(int[] nums) : this() {
            switch (nums.Length) {
                case 0:
                    break;

                case 1:
                    this.Major = nums[0];
                    break;

                case 2:
                default:
                    this.Major = nums[0];
                    this.Minor = nums[1];
                    break;
            }
        }

        public static bool operator >(ModifiVersionNumber a, ModifiVersionNumber b) {
            switch (a.Major.CompareTo(b.Major)) {
                case -1:
                default:
                    return false;

                case 0:
                    return a.Minor > b.Minor;

                case 1:
                    return true;
            }
        }

        public static bool operator <(ModifiVersionNumber a, ModifiVersionNumber b) {
            switch (a.Major.CompareTo(b.Major)) {
                case -1:
                default:
                    return true;

                case 0:
                    return a.Minor < b.Minor;

                case 1:
                    return false;
            }
        }

        public override bool Equals(object obj) {
            if (obj is ModifiVersionNumber) {
                ModifiVersionNumber other = (ModifiVersionNumber)obj;
                return other.Major == this.Major && other.Minor == this.Minor;
            }

            return false;
        }

        public string ToHash() {
            // PATCH included if added later
            return ModifiVersionHelper.VersionHasher.Encode(this.Major, this.Minor, 0);
        }

        public static ModifiVersionNumber FromHash(string hash) {
            int[] nums = ModifiVersionHelper.VersionHasher.Decode(hash);
            return new ModifiVersionNumber(nums);
        }

        public override int GetHashCode() {
            return -1;
        }

        public override string ToString() {
            return String.Format("{0}.{1}.0", Major, Minor);
        }
    }
}
