using System;
using System.Globalization;
using System.Numerics;

namespace StealAMillion.Core
{
    [Serializable]
    public struct BigMoney : IComparable<BigMoney>, IEquatable<BigMoney>
    {
        public string digits;
        private static readonly string[] Suffixes = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };
        public BigMoney(long value) { digits = Math.Max(0, value).ToString(CultureInfo.InvariantCulture); }
        public BigMoney(string value) { digits = Parse(value).ToString(CultureInfo.InvariantCulture); }
        private BigMoney(BigInteger value) { digits = BigInteger.Max(BigInteger.Zero, value).ToString(CultureInfo.InvariantCulture); }
        private static BigInteger Parse(string value)
        {
            BigInteger result;
            return BigInteger.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out result)
                ? BigInteger.Max(BigInteger.Zero, result) : BigInteger.Zero;
        }
        public bool IsZero { get { return string.IsNullOrEmpty(digits) || digits == "0"; } }
        public static BigMoney Zero { get { return new BigMoney(0); } }
        public static BigMoney Power10(int exponent) { return new BigMoney(BigInteger.Pow(10, Math.Max(0, exponent))); }
        public BigMoney Times(double factor)
        {
            if (double.IsNaN(factor) || double.IsInfinity(factor) || factor < 0 || factor > 1000000)
                throw new ArgumentOutOfRangeException("factor");
            return new BigMoney(Parse(digits) * (long)Math.Round(factor * 1000000) / 1000000);
        }
        public double Ratio(BigMoney total)
        {
            if (total.IsZero) return 0;
            string a = Canonical(), b = total.Canonical();
            int delta = a.Length - b.Length;
            if (delta > 10) return 1e10;
            if (delta < -10) return 0;
            double ma = double.Parse(a.Substring(0, Math.Min(12, a.Length)), CultureInfo.InvariantCulture) / Math.Pow(10, Math.Min(12, a.Length) - 1);
            double mb = double.Parse(b.Substring(0, Math.Min(12, b.Length)), CultureInfo.InvariantCulture) / Math.Pow(10, Math.Min(12, b.Length) - 1);
            return ma / mb * Math.Pow(10, delta);
        }
        public string Canonical() { return string.IsNullOrEmpty(digits) ? "0" : digits; }
        public string Format(bool currency = true)
        {
            string value = Canonical();
            string prefix = currency ? "$" : "";
            if (value.Length <= 3) return prefix + value;
            int group = (value.Length - 1) / 3;
            int whole = group < Suffixes.Length ? value.Length - group * 3 : 1;
            int decimalCount = Math.Min(2, Math.Max(0, 3 - whole));
            string fraction = value.Substring(whole, Math.Min(decimalCount, value.Length - whole)).TrimEnd('0');
            return prefix + value.Substring(0, whole) + (fraction.Length == 0 ? "" : "." + fraction)
                + (group < Suffixes.Length ? Suffixes[group] : "e" + (value.Length - 1));
        }
        public int CompareTo(BigMoney other) { return Parse(digits).CompareTo(Parse(other.digits)); }
        public bool Equals(BigMoney other) { return CompareTo(other) == 0; }
        public override bool Equals(object value) { return value is BigMoney && Equals((BigMoney)value); }
        public override int GetHashCode() { return Parse(digits).GetHashCode(); }
        public override string ToString() { return Format(); }
        public static BigMoney operator +(BigMoney a, BigMoney b) { return new BigMoney(Parse(a.digits) + Parse(b.digits)); }
        public static BigMoney operator -(BigMoney a, BigMoney b) { return new BigMoney(Parse(a.digits) - Parse(b.digits)); }
        public static bool operator >(BigMoney a, BigMoney b) { return a.CompareTo(b) > 0; }
        public static bool operator <(BigMoney a, BigMoney b) { return a.CompareTo(b) < 0; }
        public static bool operator >=(BigMoney a, BigMoney b) { return a.CompareTo(b) >= 0; }
        public static bool operator <=(BigMoney a, BigMoney b) { return a.CompareTo(b) <= 0; }
    }
}
