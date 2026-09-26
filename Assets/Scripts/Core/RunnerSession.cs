using System;
using System.Collections.Generic;

namespace StealAMillion.Core
{
    public sealed class RunnerSession
    {
        public readonly RunnerSave Save;
        public readonly RunnerEconomy Economy;
        public ActiveRun Run { get { return Save.run; } }
        public BigMoney Money { get { return Run.active ? Run.money : Save.money; } }
        public BigMoney Cash { get; private set; }
        public event Action<BigMoney, BigMoney> MoneyChanged;
        public event Action<int> Milestone;
        private readonly HashSet<string> consumed = new HashSet<string>();

        public RunnerSession(RunnerSave save, RunnerEconomy economy = null)
        {
            Economy = economy ?? new RunnerEconomy();
            Save = save; RunnerProgress.Repair(save);
            foreach (string id in Run.consumed) consumed.Add(id);
            Cash = RunnerProgress.CashValue(Run.level, Economy);
        }
        public void Start(int level, int seed, bool endless = false, bool bonus = false)
        {
            bool practice = !endless && !bonus && level < Save.highestLevel;
            BigMoney money = practice ? new BigMoney(Economy.startingMoney) : RunnerProgress.Max(Save.money, new BigMoney(Economy.startingMoney));
            if (!practice) { Save.money = money; Save.highest = RunnerProgress.Max(Save.highest, money); }
            Save.run = new ActiveRun { active = true, level = level, seed = seed, state = RunnerState.Running,
                startMoney = money, money = money, practice = practice, endless = endless, bonus = bonus };
            Cash = RunnerProgress.CashValue(level, Economy); consumed.Clear();
        }
        public bool Consume(string id)
        {
            if (!Run.active || Run.state != RunnerState.Running || !consumed.Add(id)) return false;
            Run.consumed.Add(id); return true;
        }
        public bool WasConsumed(string id) { return consumed.Contains(id); }
        public void ForgetBefore(int chunkStart)
        {
            // Only retire IDs behind the oldest chunk that can be reconstructed on resume.
            Run.consumed.RemoveAll(id => {
                int split = id == null ? -1 : id.IndexOf(':');
                int start;
                bool expired = split > 0 && int.TryParse(id.Substring(0, split), out start) && start < chunkStart;
                if (expired) consumed.Remove(id);
                return expired;
            });
        }
        public void SetMoney(BigMoney value, bool countIncome = true)
        {
            BigMoney before = Run.money; Run.money = value;
            if (!Run.practice)
            {
                Save.money = value;
                Save.highest = RunnerProgress.Max(Save.highest, value);
                if (countIncome && value > before)
                {
                    Save.lifetime += value - before;
                    Save.biggestReward = RunnerProgress.Max(Save.biggestReward, value - before);
                }
                int exponent = value.Canonical().Length - 1;
                for (int e = 3; e <= exponent; e++)
                {
                    if (Save.milestones.Contains(e)) continue;
                    Save.milestones.Add(e); RunnerProgress.AddCoins(Save, Economy.milestoneCoins + e * 10); RunnerProgress.AddXp(Save, Economy.milestoneXp + e * 10);
                    if (Milestone != null) Milestone(e);
                }
            }
            if (MoneyChanged != null) MoneyChanged(before, value);
        }
        public void Collect(float scale = 1)
        {
            SetMoney(Run.money + Cash.Times(scale * (Run.doubleCash > 0 ? 2 : 1)));
            Run.collected++; Run.combo++; Run.score += 10;
            if (!Run.practice) { Save.stats.cash++; Save.stats.bestCombo = Math.Max(Save.stats.bestCombo, Run.combo); }
        }
        public bool Hit(float percentage)
        {
            Run.combo = 0;
            if (Run.shields > 0) { Run.shields--; return false; }
            Run.hits++;
            if (!Run.practice) Save.stats.obstacles++;
            SetMoney(Run.money - RunnerProgress.Max(new BigMoney(5), Run.money.Times(percentage)));
            if (Run.money.IsZero) Run.state = RunnerState.Broke;
            return true;
        }
        public float Probability(GateData gate) { return gate.kind == GateKind.Market ? .7f : Math.Min(1, Math.Max(0, gate.probability + (Run.luck > 0 ? Economy.luckBonus : 0))); }
        public static bool Roll(float probability, double sample)
        {
            if (float.IsNaN(probability) || probability < 0 || probability > 1 || double.IsNaN(sample) || sample < 0 || sample >= 1)
                throw new ArgumentOutOfRangeException("probability/sample");
            return sample < probability;
        }
        public PendingRisk EnterRisk(GateData gate, double sample)
        {
            float probability = Probability(gate);
            bool won = Roll(probability, sample);
            BigMoney before = Run.money;
            bool chain = gate.kind == GateKind.RiskChain;
            BigMoney stake = chain ? (Run.chain.IsZero ? Cash.Times(10) : Run.chain) : before;
            BigMoney after;
            if (gate.kind == GateKind.Market)
                after = sample < .2 ? before.Times(5) : sample < .7 ? before.Times(1.2) : before.Times(.75);
            else after = won ? stake.Times(gate.multiplier) : stake.Times(1 - gate.loss);
            if (!won && Run.shields > 0 && !chain) { Run.shields--; after = before; }
            if (chain) Run.chain = won ? after : BigMoney.Zero;
            Run.pending = new PendingRisk { before = before, after = chain ? before : after, won = won,
                probability = probability, kind = gate.kind };
            Run.state = RunnerState.Suspense;
            if (gate.kind != GateKind.Market) Run.luck = 0;
            if (!Run.practice)
            {
                Save.stats.risks++;
                if (won) { Save.stats.wins++; if (probability <= .25f) Save.stats.rareWins++; }
                if (gate.kind == GateKind.Jackpot && won) Save.stats.jackpots++;
            }
            // The rolled outcome is saved before animation. Resume applies this same result once.
            return Run.pending;
        }
        public PendingRisk ResolveRisk()
        {
            if (Run.state != RunnerState.Suspense || Run.pending == null) return null;
            var pending = Run.pending; Run.pending = null;
            SetMoney(pending.after);
            Run.state = Run.money.IsZero ? RunnerState.Broke : RunnerState.Running;
            return pending;
        }
        public BigMoney SafeReward(GateData gate)
        {
            var fixedReward = Cash.Times(Economy.safeCashUnits * gate.scale);
            return Run.level < Economy.percentageSafeStartsAt ? fixedReward : RunnerProgress.Max(fixedReward, Run.money.Times(Economy.lateSafeFraction * gate.scale));
        }
        public void Safe(GateData gate)
        {
            if (!Run.practice) Save.stats.safe++;
            Run.score += 40;
            SetMoney(Run.money + SafeReward(gate));
        }
        public void Invest(float atDistance, bool business)
        {
            BigMoney cost = Cash.Times(business ? Economy.businessUnits : Economy.investmentUnits);
            if (Run.money < cost) return;
            SetMoney(Run.money - cost);
            Run.investments.Add(new InvestmentData { due = atDistance + Economy.investmentDistance, payout = cost.Times(business ? Economy.businessReturn : Economy.investmentReturn) });
            if (!Run.practice) Save.stats.investments++;
        }
        public void Tick(float delta, float distance)
        {
            Run.distance = distance;
            if (Run.endless && !Run.practice) Save.stats.bestEndless = Math.Max(Save.stats.bestEndless, distance);
            Run.magnet = Math.Max(0, Run.magnet - delta); Run.doubleCash = Math.Max(0, Run.doubleCash - delta);
            Run.luck = Math.Max(0, Run.luck - delta); Run.slowMotion = Math.Max(0, Run.slowMotion - delta);
            for (int i = Run.investments.Count - 1; i >= 0; i--)
                if (distance >= Run.investments[i].due) { SetMoney(Run.money + Run.investments[i].payout); Run.investments.RemoveAt(i); }
        }
        public void Power(PowerKind power, float duration)
        {
            if (!Run.practice) Save.stats.powers++;
            switch (power) {
                case PowerKind.Shield: Run.shields++; break;
                case PowerKind.Magnet: Run.magnet = duration; break;
                case PowerKind.Luck: Run.luck = duration * 3; break;
                case PowerKind.DoubleCash: Run.doubleCash = duration; break;
                case PowerKind.SlowMotion: Run.slowMotion = duration; break;
            }
        }
        public bool Finish()
        {
            if (!Run.active || Run.rewardGranted || Run.state != RunnerState.Running) return false;
            Run.rewardGranted = true; Run.state = RunnerState.Finished;
            if (!Run.chain.IsZero) { SetMoney(Run.money + Run.chain); Run.chain = BigMoney.Zero; }
            foreach (var investment in Run.investments) SetMoney(Run.money + investment.payout);
            Run.investments.Clear();
            if (!Run.practice)
            {
                Save.stats.completed++; if (Run.hits == 0) Save.stats.perfect++;
                if (Run.bonus) Save.stats.bonus++;
                if (Run.endless) Save.stats.bestEndless = Math.Max(Save.stats.bestEndless, Run.distance);
                Run.coins += Economy.finishCoins + Math.Min(40, Run.level / 2) + Run.combo / 10;
                RunnerProgress.AddCoins(Save, Run.coins); RunnerProgress.AddXp(Save, Economy.finishXp + Run.score / 10);
                if (!Run.endless && !Run.bonus) Save.level = Save.highestLevel = Math.Max(Save.highestLevel, Run.level + 1);
            }
            int stars = Run.hits == 0 ? 3 : Run.hits < 3 ? 2 : 1;
            if (!Run.endless && !Run.bonus)
            {
                var record = Save.records.Find(item => item.level == Run.level);
                if (record == null) { record = new LevelRecord { level = Run.level }; Save.records.Add(record); }
                record.stars = Math.Max(record.stars, stars); record.score = Math.Max(record.score, Run.score);
            }
            return true;
        }
        public bool SecondChance()
        {
            if (!Run.active || Run.state != RunnerState.Broke || Run.secondChance) return false;
            Run.secondChance = true; SetMoney(RunnerProgress.Max(new BigMoney(Economy.startingMoney), Run.startMoney.Times(.25)), false);
            Run.shields = Math.Max(1, Run.shields); Run.state = RunnerState.Running; return true;
        }
    }
}
