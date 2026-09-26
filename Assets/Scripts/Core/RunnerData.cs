using System;
using System.Collections.Generic;

namespace StealAMillion.Core
{
    public enum RunnerState { Menu, Running, Suspense, Finished, Broke }
    public enum SegmentKind { Cash, Choice, Tax, Police, Moving, Thief, Split, Investment, Jackpot, Power, Chain, Market, Keys, Bonus, Finish }
    public enum GateKind { Safe, Risk, AddMoney, SubtractMoney, Multiplier, Tax, Investment, Business, Market, Insurance, Shield, Luck, Mystery, Jackpot, DoubleOrNothing, CashOut, RiskChain, Bonus, KeyGate }
    public enum PowerKind { Shield, Magnet, Luck, DoubleCash, SlowMotion }
    public enum CosmeticSlot { Character, Outfit, Trail, MoneyEffect, Victory, Vault, Gate }

    [Serializable] public sealed class RunnerSave
    {
        public int saveVersion = 2;
        public BigMoney money = new BigMoney(100), highest = new BigMoney(100), lifetime, biggestReward;
        public int level = 1, highestLevel = 1, world, xp, coins;
        public string language = "auto";
        public List<int> worlds = new List<int> { 0 };
        public List<string> owned = new List<string> { "runner", "outfit_classic", "dust", "cash_green", "jump", "vault_classic", "gate_classic" };
        public string character = "runner", outfit = "outfit_classic", trail = "dust", effect = "cash_green", victory = "jump", vault = "vault_classic", gate = "gate_classic";
        public List<string> claims = new List<string>();
        public List<int> milestones = new List<int>();
        public List<LevelRecord> records = new List<LevelRecord>();
        public RunnerStats stats = new RunnerStats();
        public string dailyDate;
        public RunnerStats dailyBaseline = new RunnerStats();
        public List<string> dailyClaims = new List<string>();
        public int keys;
        public bool tutorialMove, tutorialGate, migrated;
        public ActiveRun run = new ActiveRun();
    }
    [Serializable] public sealed class RunnerStats
    {
        public int completed, cash, safe, risks, wins, jackpots, obstacles, avoided, perfect, coinsEarned, powers, investments, keys, bonus, rareWins;
        public float bestEndless;
        public int bestCombo;
        public RunnerStats Copy() { return (RunnerStats)MemberwiseClone(); }
        public int Get(string metric)
        {
            switch (metric) {
                case "completed": return completed; case "cash": return cash; case "safe": return safe;
                case "risks": return risks; case "wins": return wins; case "jackpots": return jackpots;
                case "avoided": return avoided; case "perfect": return perfect; case "coins": return coinsEarned;
                case "powers": return powers; case "investments": return investments; case "keys": return keys;
                case "bonus": return bonus; case "rareWins": return rareWins; case "combo": return bestCombo;
                default: return 0;
            }
        }
    }
    [Serializable] public sealed class LevelRecord { public int level, stars, score; }
    [Serializable] public sealed class ActiveRun
    {
        public bool active, endless, practice, bonus, secondChance, rewardGranted, doubled;
        public int level = 1, seed, chunk, combo, score, hits, collected, coins, keys, shields;
        public float z, x, distance, magnet, luck, doubleCash, slowMotion;
        public BigMoney startMoney, money, chain;
        public RunnerState state;
        public List<string> consumed = new List<string>();
        public List<InvestmentData> investments = new List<InvestmentData>();
        public PendingRisk pending;
    }
    [Serializable] public sealed class PendingRisk
    {
        public bool won;
        public BigMoney before, after;
        public float probability;
        public GateKind kind;
    }
    [Serializable] public sealed class InvestmentData { public float due; public BigMoney payout; }
    [Serializable] public sealed class GateData
    {
        public string id;
        public GateKind kind;
        public string title;
        public float probability = .6f, multiplier = 1.5f, loss = .25f, scale = 1;
        public int minLevel = 1;
    }
    [Serializable] public sealed class GateCatalog { public GateData[] gates; }
    [Serializable] public sealed class LevelData
    {
        public int number, world, seed;
        public string name;
        public string[] segments;
        public float speed = 7;
    }
    [Serializable] public sealed class LevelCatalog { public LevelData[] levels; }
    [Serializable] public sealed class WorldData
    {
        public string id, title, ground, building, accent;
        public int level, exponent;
    }
    [Serializable] public sealed class WorldCatalog { public WorldData[] worlds; }
    [Serializable] public sealed class RunnerCosmetic
    {
        public string id, title, color, secondary, shape;
        public CosmeticSlot slot;
        public int cost, playerLevel = 1, wealthExponent, levels;
    }
    [Serializable] public sealed class RunnerShopCatalog { public RunnerCosmetic[] items; }
    [Serializable] public sealed class ChallengeData
    {
        public string id, title, metric, cosmetic;
        public int target, coins, xp;
        public bool achievement;
    }
    [Serializable] public sealed class ChallengeCatalog { public ChallengeData[] missions; }
}
