using System;
using System.Collections.Generic;

namespace StealAMillion.Core
{
    public static class RunnerProgress
    {
        public static RunnerSave Migrate(SaveData previous)
        {
            // Unity can instantiate missing nested objects while deserializing V1 JSON.
            if (previous.version == 2 && previous.runner != null && previous.runner.saveVersion == 2) { Repair(previous.runner); return previous.runner; }
            var result = new RunnerSave();
            if (previous.loadedFromDisk || previous.hasRun || previous.highestMoney > 0 || previous.completedRuns > 0)
            {
                result.money = new BigMoney(previous.currentMoney);
                result.highest = new BigMoney(Math.Max(previous.highestMoney, previous.currentMoney));
                result.coins = Math.Max(0, previous.coins);
                result.migrated = true;
                result.level = result.highestLevel = Math.Max(1, previous.currentLevel);
                if (previous.unlockedCosmetics != null)
                    foreach (var id in previous.unlockedCosmetics)
                        if (id != "classic") result.owned.Add("vault_" + id);
                if (result.owned.Contains("vault_" + previous.equippedCosmetic)) result.vault = "vault_" + previous.equippedCosmetic;
            }
            previous.version = 2;
            previous.runner = result;
            Repair(result);
            return result;
        }
        public static void Repair(RunnerSave s)
        {
            s.money = new BigMoney(s.money.digits); s.highest = Max(s.money, new BigMoney(s.highest.digits));
            s.lifetime = new BigMoney(s.lifetime.digits); s.biggestReward = new BigMoney(s.biggestReward.digits);
            s.level = Math.Max(1, s.level); s.highestLevel = Math.Max(s.level, s.highestLevel);
            s.xp = Math.Max(0, s.xp); s.coins = Math.Max(0, s.coins); s.keys = Math.Max(0, s.keys);
            if (s.owned == null) s.owned = new RunnerSave().owned;
            if (s.claims == null) s.claims = new List<string>();
            if (s.milestones == null) s.milestones = new List<int>();
            if (s.worlds == null) s.worlds = new List<int> { 0 };
            if (s.records == null) s.records = new List<LevelRecord>();
            if (s.stats == null) s.stats = new RunnerStats();
            if (s.dailyBaseline == null) s.dailyBaseline = new RunnerStats();
            if (s.dailyClaims == null) s.dailyClaims = new List<string>();
            if (s.run == null) s.run = new ActiveRun();
            if (s.run.consumed == null) s.run.consumed = new List<string>();
            if (s.run.investments == null) s.run.investments = new List<InvestmentData>();
            var run = s.run;
            run.money = new BigMoney(run.money.digits); run.startMoney = new BigMoney(run.startMoney.digits); run.chain = new BigMoney(run.chain.digits);
            run.level = Math.Max(1, run.level); run.chunk = Math.Max(0, run.chunk);
            run.consumed.RemoveAll(string.IsNullOrEmpty);
            run.investments.RemoveAll(item => item == null || float.IsNaN(item.due) || float.IsInfinity(item.due));
            if (float.IsNaN(run.z) || float.IsInfinity(run.z) || run.z < 0 || float.IsNaN(run.x) || float.IsInfinity(run.x)
                || !Enum.IsDefined(typeof(RunnerState), run.state) || (run.state == RunnerState.Suspense && run.pending == null))
                s.run = new ActiveRun();
            else
            {
                run.x = Math.Max(-3.6f, Math.Min(3.6f, run.x));
                if (run.endless) run.chunk = (int)Math.Min(int.MaxValue, Math.Floor(run.z / 288.0));
                if (run.active && run.money.IsZero && run.state == RunnerState.Running) run.state = RunnerState.Broke;
            }
            if (string.IsNullOrEmpty(s.language)) s.language = "auto";
            foreach (string id in new RunnerSave().owned) if (!s.owned.Contains(id)) s.owned.Add(id);
        }
        public static BigMoney Max(BigMoney a, BigMoney b) { return a > b ? a : b; }
        public static int PlayerLevel(int xp) { return 1 + (int)Math.Sqrt(Math.Max(0, xp) / 80.0); }
        public static int NextMilestone(BigMoney peak) { return Math.Max(3, peak.Canonical().Length); }
        public static int Rank(BigMoney peak)
        {
            int exponent = peak.Canonical().Length - 1;
            return exponent < 3 ? 0 : exponent < 14 ? Math.Min(10, exponent - 2) : 11;
        }
        public static BigMoney CashValue(int level, RunnerEconomy economy = null)
        {
            economy = economy ?? new RunnerEconomy();
            level = Math.Max(1, level);
            BigMoney value = new BigMoney(economy.pickupBase + level / Math.Max(1, economy.pickupLevelStep));
            for (int i = economy.growthStartsAt; i < level; i++) value = Max(value + new BigMoney(1), value.Times(economy.pickupGrowth));
            return value;
        }
        public static float Difficulty(int level) { return (float)Math.Min(1, Math.Log10(1 + Math.Max(0, level - 1) / 8.0) / 1.8); }
        public static void AddCoins(RunnerSave s, int count)
        {
            count = Math.Max(0, count);
            s.coins = (int)Math.Min(int.MaxValue, (long)s.coins + count);
            s.stats.coinsEarned = (int)Math.Min(int.MaxValue, (long)s.stats.coinsEarned + count);
        }
        public static void AddXp(RunnerSave s, int count) { s.xp = (int)Math.Min(int.MaxValue, (long)s.xp + Math.Max(0, count)); }
        public static bool DailyRefresh(RunnerSave s, DateTime utc)
        {
            string today = utc.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            if (string.CompareOrdinal(today, s.dailyDate) <= 0) return false;
            s.dailyDate = today; s.dailyClaims.Clear(); s.dailyBaseline = s.stats.Copy(); return true;
        }
        public static int ChallengeValue(RunnerSave s, ChallengeData mission)
        {
            if (mission.metric == "wealth") return s.highest.Canonical().Length - 1;
            if (mission.metric == "collection") return s.owned.Count;
            return s.stats.Get(mission.metric);
        }
        public static bool Claim(RunnerSave s, ChallengeData mission)
        {
            if (s.claims.Contains(mission.id) || ChallengeValue(s, mission) < mission.target) return false;
            s.claims.Add(mission.id); AddCoins(s, mission.coins); AddXp(s, mission.xp);
            if (!string.IsNullOrEmpty(mission.cosmetic) && !s.owned.Contains(mission.cosmetic)) s.owned.Add(mission.cosmetic);
            return true;
        }
        public static bool Unlocked(RunnerSave save, RunnerCosmetic item)
        {
            return PlayerLevel(save.xp) >= item.playerLevel && save.highestLevel >= item.levels
                && (item.wealthExponent == 0 || save.highest >= BigMoney.Power10(item.wealthExponent));
        }
        public static bool Buy(RunnerSave save, RunnerCosmetic item)
        {
            if (!save.owned.Contains(item.id))
            {
                if (!Unlocked(save, item) || save.coins < item.cost) return false;
                save.coins -= item.cost; save.owned.Add(item.id);
            }
            switch (item.slot) {
                case CosmeticSlot.Character: save.character = item.id; break;
                case CosmeticSlot.Outfit: save.outfit = item.id; break;
                case CosmeticSlot.Trail: save.trail = item.id; break;
                case CosmeticSlot.MoneyEffect: save.effect = item.id; break;
                case CosmeticSlot.Victory: save.victory = item.id; break;
                case CosmeticSlot.Vault: save.vault = item.id; break;
                case CosmeticSlot.Gate: save.gate = item.id; break;
            }
            return true;
        }
    }
}
