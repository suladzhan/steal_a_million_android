using System;
using System.Globalization;

namespace StealAMillion.Core
{
    public sealed class MoneyManager
    {
        public const long Limit = 999999999999;
        public long Current { get; private set; }
        public event Action<long, long> Changed;

        public MoneyManager(long initial) { Current = Clamp(initial); }
        public void Set(long amount)
        {
            long before = Current;
            Current = Clamp(amount);
            if (Changed != null && before != Current) Changed(before, Current);
        }
        public void Add(long amount) { Set(new Reward { mode = RewardMode.Add, amount = amount }.Apply(Current)); }
        public void Subtract(long amount) { if (amount > 0) Add(-Math.Min(amount, Limit)); }
        public static long Clamp(long amount) { return Math.Max(0, Math.Min(Limit, amount)); }
        public static string Format(long amount) { return "$" + Clamp(amount).ToString("N0", CultureInfo.InvariantCulture); }
    }
}
