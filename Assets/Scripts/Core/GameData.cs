using System;
using System.Collections.Generic;

namespace StealAMillion.Core
{
    public enum RewardMode { Add, Set, Multiply }
    public enum RunState { Playing, ShowingResult, GameOver, Victory }

    [Serializable]
    public sealed class Reward
    {
        public RewardMode mode;
        public long amount;

        public long Apply(long current)
        {
            if (mode == RewardMode.Set) return MoneyManager.Clamp(amount);
            if (mode == RewardMode.Multiply)
                return amount <= 0 ? 0 : current > MoneyManager.Limit / amount
                    ? MoneyManager.Limit : MoneyManager.Clamp(current * amount);
            if (amount > 0 && current > MoneyManager.Limit - amount) return MoneyManager.Limit;
            if (amount < 0 && amount < -current) return 0;
            return MoneyManager.Clamp(current + amount);
        }

        public string Describe()
        {
            if (mode == RewardMode.Multiply) return amount == 1 ? "KEEP YOUR TOTAL" : "TOTAL x" + amount;
            if (mode == RewardMode.Set) return amount == 0 ? "LOSE EVERYTHING" : "TOTAL " + MoneyManager.Format(amount);
            if (amount == 0) return "NO CHANGE";
            return (amount > 0 ? "+" : "-") + MoneyManager.Format(Math.Abs(amount));
        }
    }

    [Serializable]
    public sealed class DecisionData
    {
        public string id;
        public string title;
        public string description;
        public string specialBehavior;
        public int level;
        public int difficulty;
        public float riskProbability;
        public Reward safeReward;
        public Reward riskWinReward;
        public Reward riskLossReward;
    }

    [Serializable]
    public sealed class DecisionCatalog
    {
        public DecisionData[] decisions;

        public List<string> Validate()
        {
            var errors = new List<string>();
            if (decisions == null || decisions.Length == 0)
            {
                errors.Add("The decision catalog is empty.");
                return errors;
            }
            var ids = new HashSet<string>();
            int previousLevel = 0;
            foreach (var d in decisions)
            {
                if (d == null) { errors.Add("Null decision."); continue; }
                if (string.IsNullOrEmpty(d.id) || !ids.Add(d.id)) errors.Add("Missing or duplicate decision ID: " + d.id);
                if (string.IsNullOrEmpty(d.title)) errors.Add("Missing title: " + d.id);
                if (d.level < 1 || d.level < previousLevel || d.level > previousLevel + 1)
                    errors.Add("Levels must be ordered, contiguous and start at 1: " + d.id);
                previousLevel = d.level;
                if (float.IsNaN(d.riskProbability) || float.IsInfinity(d.riskProbability)
                    || d.riskProbability < 0 || d.riskProbability > 1) errors.Add("Invalid probability: " + d.id);
                if (d.difficulty < 1 || d.difficulty > 10) errors.Add("Invalid difficulty: " + d.id);
                ValidateReward(d.safeReward, d.id, errors);
                ValidateReward(d.riskWinReward, d.id, errors);
                ValidateReward(d.riskLossReward, d.id, errors);
            }
            return errors;
        }

        private static void ValidateReward(Reward reward, string id, List<string> errors)
        {
            if (reward == null || !Enum.IsDefined(typeof(RewardMode), reward.mode)
                || reward.amount < -MoneyManager.Limit || reward.amount > MoneyManager.Limit
                || (reward.mode != RewardMode.Add && reward.amount < 0))
                errors.Add("Invalid reward: " + id);
        }
    }

    [Serializable]
    public sealed class ResultData
    {
        public string decisionId;
        public bool choseRisk;
        public bool riskWon;
        public long before;
        public long after;
    }

    [Serializable]
    public sealed class SaveData
    {
        public int version = 1;
        public bool hasRun;
        public int decisionIndex;
        public int currentLevel = 1;
        public long currentMoney;
        public long highestMoney;
        public long runPeak;
        public int completedRuns;
        public int coins = 1000;
        public bool soundEnabled = true;
        public bool musicEnabled = true;
        public bool vibrationEnabled = true;
        public bool tutorialComplete;
        public bool secondChanceUsed;
        public RunState state;
        public ResultData pendingResult;
        public List<string> unlockedCosmetics = new List<string> { "classic" };
        public string equippedCosmetic = "classic";

        public void Sanitize(DecisionCatalog catalog)
        {
            currentMoney = MoneyManager.Clamp(currentMoney);
            highestMoney = Math.Max(currentMoney, MoneyManager.Clamp(highestMoney));
            runPeak = Math.Max(currentMoney, MoneyManager.Clamp(runPeak));
            coins = Math.Max(0, Math.Min(coins, 100000000));
            completedRuns = Math.Max(0, completedRuns);
            decisionIndex = Math.Max(0, Math.Min(decisionIndex, catalog.decisions.Length - 1));
            currentLevel = catalog.decisions[decisionIndex].level;
            if (!Enum.IsDefined(typeof(RunState), state)) state = RunState.Playing;
            if (pendingResult == null && state == RunState.ShowingResult) state = RunState.Playing;
            if (pendingResult != null)
            {
                pendingResult.before = MoneyManager.Clamp(pendingResult.before);
                pendingResult.after = currentMoney;
            }
            if (unlockedCosmetics == null) unlockedCosmetics = new List<string>();
            if (!unlockedCosmetics.Contains("classic")) unlockedCosmetics.Add("classic");
            if (string.IsNullOrEmpty(equippedCosmetic) || !unlockedCosmetics.Contains(equippedCosmetic))
                equippedCosmetic = "classic";
        }
    }
}
