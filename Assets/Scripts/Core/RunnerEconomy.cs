using System;

namespace StealAMillion.Core
{
    [Serializable]
    public sealed class RunnerEconomy
    {
        public int startingMoney = 100, pickupBase = 5, pickupLevelStep = 3, growthStartsAt = 20;
        public double pickupGrowth = 1.065;
        public float safeCashUnits = 5, lateSafeFraction = .05f;
        public int percentageSafeStartsAt = 50;
        public float taxLoss = .15f, obstacleLoss = .10f, luckBonus = .10f;
        public float investmentUnits = 6, businessUnits = 10, insuranceUnits = 3;
        public float investmentReturn = 1.5f, businessReturn = 1.8f, investmentDistance = 45;
        public int finishCoins = 20, finishXp = 70, milestoneCoins = 30, milestoneXp = 40;
        public RiskTier[] riskTiers = {
            new RiskTier { level = 1, probability = .6f, multiplier = 1.5f },
            new RiskTier { level = 4, probability = .6f, multiplier = 2 },
            new RiskTier { level = 20, probability = .45f, multiplier = 3 },
            new RiskTier { level = 50, probability = .3f, multiplier = 4 }
        };
    }
    [Serializable] public sealed class RiskTier { public int level; public float probability, multiplier; }
}
