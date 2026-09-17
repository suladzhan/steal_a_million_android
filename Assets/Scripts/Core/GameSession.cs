using System;

namespace StealAMillion.Core
{
    public sealed class GameSession
    {
        public readonly SaveData Save;
        public readonly MoneyManager Money;
        public readonly DecisionCatalog Catalog;
        public readonly long Target;
        private readonly long startingMoney;

        public GameSession(DecisionCatalog catalog, SaveData save, long initial, long target)
        {
            var errors = catalog.Validate();
            if (errors.Count != 0) throw new ArgumentException(string.Join("\n", errors.ToArray()));
            Catalog = catalog;
            Save = save ?? new SaveData();
            startingMoney = Math.Max(1, Math.Min(initial, MoneyManager.Limit - 1));
            Target = Math.Max(startingMoney + 1, Math.Min(target, MoneyManager.Limit));
            Save.Sanitize(catalog);
            Money = new MoneyManager(Save.currentMoney);
            if (Save.hasRun && Save.state != RunState.ShowingResult) SetTerminalState();
        }

        public DecisionData Decision { get { return Catalog.decisions[Save.decisionIndex]; } }
        public int TotalLevels { get { return Catalog.decisions[Catalog.decisions.Length - 1].level; } }
        public bool CanContinue { get { return Save.hasRun; } }

        public void NewRun()
        {
            Save.hasRun = true;
            Save.decisionIndex = 0;
            Save.currentLevel = 1;
            Save.runPeak = 0;
            Save.secondChanceUsed = false;
            Save.pendingResult = null;
            Save.state = RunState.Playing;
            SetMoney(startingMoney);
        }

        public ResultData Choose(bool risk, float sample)
        {
            if (!Save.hasRun || Save.state != RunState.Playing) return null;
            if (float.IsNaN(sample) || sample < 0 || sample > 1) throw new ArgumentOutOfRangeException("sample");
            var decision = Decision;
            bool won = risk && (decision.riskProbability >= 1 || sample < decision.riskProbability);
            var reward = risk ? (won ? decision.riskWinReward : decision.riskLossReward) : decision.safeReward;
            var result = new ResultData {
                decisionId = decision.id, choseRisk = risk, riskWon = won,
                before = Money.Current, after = reward.Apply(Money.Current)
            };
            // Persist this committed result before suspense starts, so reopening cannot reroll it.
            Save.state = RunState.ShowingResult;
            Save.pendingResult = result;
            Save.tutorialComplete = true;
            SetMoney(result.after);
            return result;
        }

        public bool CompleteResult()
        {
            if (Save.state != RunState.ShowingResult) return false;
            Save.pendingResult = null;
            SetTerminalState();
            if (Save.state != RunState.Playing) return true;
            int oldLevel = Save.currentLevel;
            if (Save.decisionIndex < Catalog.decisions.Length - 1) Save.decisionIndex++;
            else
            {
                // Repeat the final level until the player reaches the target or loses.
                while (Save.decisionIndex > 0 && Catalog.decisions[Save.decisionIndex - 1].level == TotalLevels)
                    Save.decisionIndex--;
            }
            Save.currentLevel = Decision.level;
            if (Save.currentLevel > oldLevel) AwardCoins(25);
            return true;
        }

        public bool SecondChance(long amount)
        {
            if (Save.state != RunState.GameOver || Save.secondChanceUsed || amount <= 0) return false;
            Save.secondChanceUsed = true;
            Save.state = RunState.Playing;
            Save.pendingResult = null;
            SetMoney(Math.Min(amount, Target - 1));
            return true;
        }

        public void AwardCoins(int amount)
        {
            if (amount > 0) Save.coins = (int)Math.Min(100000000L, (long)Save.coins + amount);
        }

        private void SetTerminalState()
        {
            var previous = Save.state;
            Save.state = Money.Current <= 0 ? RunState.GameOver : Money.Current >= Target ? RunState.Victory : RunState.Playing;
            if (Save.state == RunState.Victory && previous != RunState.Victory)
            {
                Save.completedRuns++;
                AwardCoins(250);
            }
        }

        private void SetMoney(long amount)
        {
            Money.Set(amount);
            Save.currentMoney = Money.Current;
            Save.runPeak = Math.Max(Save.runPeak, Money.Current);
            Save.highestMoney = Math.Max(Save.highestMoney, Money.Current);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void DebugSetMoney(long amount)
        {
            SetMoney(amount);
            Save.pendingResult = null;
            SetTerminalState();
        }

        public void DebugNextLevel()
        {
            int level = Save.currentLevel;
            while (Save.decisionIndex < Catalog.decisions.Length - 1 && Decision.level <= level) Save.decisionIndex++;
            Save.currentLevel = Decision.level;
            Save.pendingResult = null;
            SetTerminalState();
        }
#endif
    }
}
