using System;
using System.Linq;
using NUnit.Framework;
using StealAMillion.Core;
using UnityEngine;

namespace StealAMillion.Tests
{
    public sealed class RunnerCoreTests
    {
        [Test] public void SerializedV1MigrationPreservesDeviceProgress()
        {
            const string json="{\"version\":1,\"hasRun\":true,\"currentLevel\":3,\"currentMoney\":0,\"highestMoney\":1016300,\"completedRuns\":1,\"coins\":25,\"musicEnabled\":false,\"unlockedCosmetics\":[\"classic\",\"gold\",\"diamond\"],\"equippedCosmetic\":\"diamond\"}";
            var old=JsonUtility.FromJson<SaveData>(json);old.loadedFromDisk=true;
            var next=RunnerProgress.Migrate(old);
            Assert.That(old.version,Is.EqualTo(2));Assert.That(next.migrated,Is.True);
            Assert.That(next.money.IsZero,Is.True);Assert.That(next.highest.Equals(new BigMoney(1016300)),Is.True);
            Assert.That(next.coins,Is.EqualTo(25));Assert.That(next.level,Is.EqualTo(3));
            Assert.That(next.owned,Does.Contain("vault_gold"));Assert.That(next.vault,Is.EqualTo("vault_diamond"));Assert.That(old.musicEnabled,Is.False);
            var restored=JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(old));
            Assert.That(RunnerProgress.Migrate(restored).coins,Is.EqualTo(25));
        }
        [Test] public void V1EnvelopeWithDefaultRunnerStillMigratesExactlyOnce()
        {
            var old=new SaveData{version=1,loadedFromDisk=true,coins=25,highestMoney=1016300,runner=new RunnerSave()};
            var next=RunnerProgress.Migrate(old);
            Assert.That(next.migrated,Is.True);Assert.That(next.coins,Is.EqualTo(25));Assert.That(old.version,Is.EqualTo(2));
            next.coins=40;
            Assert.That(RunnerProgress.Migrate(old),Is.SameAs(next));Assert.That(next.coins,Is.EqualTo(40));
        }
        [Test] public void RuntimeUiMaterialHasSerializedShaderReference()
        {
            var material=Resources.Load<Material>("Runner/Materials/UI");
            Assert.That(material,Is.Not.Null);Assert.That(material.shader.name,Is.EqualTo("UI/Default"));
            Assert.That(UIFactory.SurfaceMaterial,Is.SameAs(material));
        }
        [Test] public void RetryStartingBankrollMatchesMenuAndSaveBeforeFirstPickup()
        {
            var save=new RunnerSave{money=new BigMoney(20)};var session=new RunnerSession(save);session.Start(1,1);
            Assert.That(save.money.Equals(session.Money),Is.True);Assert.That(save.money.Equals(new BigMoney(100)),Is.True);
            session.Finish();Assert.That(save.money.Equals(session.Money),Is.True);
        }
        [Test] public void PrimaryTranslationsCoverEveryKeyAndFallbackIsOffline()
        {
            var english=JsonUtility.FromJson<LocaleTable>(Resources.Load<TextAsset>("Localization/en").text);
            foreach(string code in new[]{"ru","tr","de","ar","zh"})
            {
                var table=JsonUtility.FromJson<LocaleTable>(Resources.Load<TextAsset>("Localization/"+code).text);
                Assert.That(table.entries.Select(e=>e.key).Distinct().Count(),Is.EqualTo(table.entries.Length));
                Assert.That(english.entries.Select(e=>e.key).Except(table.entries.Select(e=>e.key)),Is.Empty,code);
                foreach(var entry in table.entries)Assert.DoesNotThrow(()=>string.Format(entry.value,10,20,30),entry.key);
            }
            var locale=new LocalizationManager(new RunnerSave{language="unknown"});Assert.That(locale.Text("UI_PLAY"),Is.EqualTo("PLAY"));
            locale.Select("ar");Assert.That(locale.IsRtl,Is.True);Assert.That(locale.Text("UI_LEVEL",1),Does.Not.Contain("UI_LEVEL"));
        }
        [Test] public void SaveRepairRejectsBrokenRunWithoutDiscardingBankroll()
        {
            var save=new RunnerSave{money=BigMoney.Power10(15)};
            save.run=new ActiveRun{active=true,state=RunnerState.Suspense,pending=null,z=400};
            RunnerProgress.Repair(save);Assert.That(save.run.active,Is.False);Assert.That(save.money.Equals(BigMoney.Power10(15)),Is.True);
            save.run=new ActiveRun{active=true,state=RunnerState.Running,money=new BigMoney(100),z=float.NaN};
            RunnerProgress.Repair(save);Assert.That(save.run.active,Is.False);
        }
        [Test] public void EndlessRetiresOnlyPastChunksAndKeepsRecordAfterLoss()
        {
            var save=new RunnerSave();var session=new RunnerSession(save);session.Start(20,34,true);
            session.Consume("0:2:cash");session.Consume("288:1:cash");session.Consume("576:1:cash");session.ForgetBefore(288);
            Assert.That(session.WasConsumed("0:2:cash"),Is.False);Assert.That(session.WasConsumed("288:1:cash"),Is.True);
            session.Tick(.1f,710);session.Hit(1);Assert.That(save.stats.bestEndless,Is.EqualTo(710));Assert.That(session.Finish(),Is.False);
            session.SecondChance();session.Finish();Assert.That(save.records,Is.Empty);
        }
        [Test] public void BonusRoutesContainNoLossGatesOrHazards()
        {
            for(int seed=0;seed<200;seed++)
                Assert.That(LevelGenerator.Generate(50,seed,0,0,true).segments.All(s=>s=="Cash"||s=="Bonus"||s=="Keys"||s=="Finish"),Is.True);
        }
        [Test] public void LateSafeRewardUsesBankrollAndBalanceIsConfigurable()
        {
            var economy=new RunnerEconomy{startingMoney=200,pickupBase=10};
            var session=new RunnerSession(new RunnerSave(),economy);session.Start(1,1);
            Assert.That(session.Money.Equals(new BigMoney(200)),Is.True);Assert.That(session.Cash.Equals(new BigMoney(10)),Is.True);
            session.Start(70,3);session.SetMoney(BigMoney.Power10(30));var gate=new GateData{kind=GateKind.Safe};
            var reward=session.SafeReward(gate);Assert.That(reward.Equals(BigMoney.Power10(30).Times(.05)),Is.True);
            var before=session.Money;session.Safe(gate);Assert.That(session.Money.Equals(before+reward),Is.True);
        }
        [Test] public void DoubleOrNothingPaysExactlyTwiceAndMarketIgnoresLuck()
        {
            var gates=JsonUtility.FromJson<GateCatalog>(Resources.Load<TextAsset>("RunnerData/gates").text).gates;
            var session=new RunnerSession(new RunnerSave());session.Start(1,1);
            session.EnterRisk(gates.First(g=>g.kind==GateKind.DoubleOrNothing),0);session.ResolveRisk();Assert.That(session.Money.Equals(new BigMoney(200)),Is.True);
            session.Power(PowerKind.Luck,9);var market=gates.First(g=>g.kind==GateKind.Market);
            Assert.That(session.Probability(market),Is.EqualTo(.7f));session.EnterRisk(market,.69);session.ResolveRisk();Assert.That(session.Money.Equals(new BigMoney(240)),Is.True);
        }
        [TestCase("0","$0")][TestCase("999","$999")][TestCase("1000","$1K")][TestCase("1250","$1.25K")]
        [TestCase("999000","$999K")][TestCase("1000000","$1M")][TestCase("999000000","$999M")]
        [TestCase("1000000000","$1B")][TestCase("1000000000000","$1T")][TestCase("1000000000000000","$1Qa")]
        public void FormatsLargeValues(string value,string expected){Assert.That(new BigMoney(value).Format(),Is.EqualTo(expected));}
        [Test] public void UnboundedArithmeticAndSerialization()
        {
            var large=BigMoney.Power10(400);var result=large.Times(5)+new BigMoney(9);
            Assert.That(result>large,Is.True);Assert.That((result-large.Times(4))>=large,Is.True);
            Assert.That((new BigMoney(3)-new BigMoney(5)).IsZero,Is.True);
            Assert.That(result.Format(),Is.EqualTo("$5e400"));
            Assert.That(JsonUtility.FromJson<RunnerSave>(JsonUtility.ToJson(new RunnerSave{money=result})).money.Equals(result),Is.True);
            Assert.Throws<ArgumentOutOfRangeException>(()=>large.Times(double.NaN));
        }
        [TestCase(.1f)][TestCase(.25f)][TestCase(.3f)][TestCase(.5f)][TestCase(.7f)][TestCase(.9f)]
        public void ProbabilityMatchesDisplayedOdds(float probability)
        {
            var random=new System.Random(913);int wins=0;
            for(int i=0;i<100000;i++)if(RunnerSession.Roll(probability,random.NextDouble()))wins++;
            Assert.That(wins/100000f,Is.EqualTo(probability).Within(.007f));
            Assert.That(RunnerSession.Roll(probability,probability),Is.False);
        }
        [Test] public void MigrationKeepsWealthSettingsAndPurchases()
        {
            var old=new SaveData{hasRun=true,currentMoney=987654321,highestMoney=1000000000,coins=432,musicEnabled=false,currentLevel=8,equippedCosmetic="gold"};old.unlockedCosmetics.Add("gold");
            var next=RunnerProgress.Migrate(old);
            Assert.That(next.money.Format(),Is.EqualTo("$987M"));Assert.That(next.highest.Format(),Is.EqualTo("$1B"));
            Assert.That(next.coins,Is.EqualTo(432));Assert.That(next.owned,Does.Contain("vault_gold"));Assert.That(old.musicEnabled,Is.False);
            Assert.That(next.vault,Is.EqualTo("vault_gold"));
            Assert.That(RunnerProgress.Migrate(old),Is.SameAs(next));
        }
        [Test] public void MillionIsMilestoneAndNeverEndsRun()
        {
            var save=new RunnerSave();var session=new RunnerSession(save);session.Start(10,10);int celebrations=0;session.Milestone+=e=>celebrations++;
            session.SetMoney(new BigMoney(1000000));Assert.That(session.Run.state,Is.EqualTo(RunnerState.Running));
            session.SetMoney(BigMoney.Power10(30));Assert.That(session.Run.state,Is.EqualTo(RunnerState.Running));
            Assert.That(RunnerProgress.NextMilestone(save.highest),Is.EqualTo(31));int count=celebrations;
            session.SetMoney(BigMoney.Power10(30));Assert.That(celebrations,Is.EqualTo(count));
        }
        [Test] public void CommittedRiskSurvivesRestartAndCannotPayTwice()
        {
            var save=new RunnerSave();var session=new RunnerSession(save);session.Start(2,44);
            var gate=new GateData{kind=GateKind.Risk,probability=.3f,multiplier=5,loss=.25f};
            Assert.That(session.Consume("gate"),Is.True);session.EnterRisk(gate,.2);Assert.That(session.Consume("gate"),Is.False);
            var restored=new RunnerSession(JsonUtility.FromJson<RunnerSave>(JsonUtility.ToJson(save)));
            restored.ResolveRisk();Assert.That(restored.Money.Equals(new BigMoney(500)),Is.True);Assert.That(restored.ResolveRisk(),Is.Null);
        }
        [Test] public void ShieldLuckInvestmentAndChainAreIndependent()
        {
            var s=new RunnerSession(new RunnerSave());s.Start(1,1);s.Power(PowerKind.Shield,9);Assert.That(s.Hit(.5f),Is.False);Assert.That(s.Money.Equals(new BigMoney(100)),Is.True);
            s.Power(PowerKind.Luck,9);var gate=new GateData{kind=GateKind.RiskChain,probability=.3f,multiplier=2,loss=1};Assert.That(s.Probability(gate),Is.EqualTo(.4f).Within(.0001));
            s.EnterRisk(gate,.35);s.ResolveRisk();Assert.That(s.Money.Equals(new BigMoney(100)),Is.True);Assert.That(s.Run.chain.Equals(new BigMoney(100)),Is.True);
            s.Invest(0,false);Assert.That(s.Money.Equals(new BigMoney(70)),Is.True);s.Tick(1,46);Assert.That(s.Money.Equals(new BigMoney(115)),Is.True);
        }
        [Test] public void ReplayDoesNotFarmBankrollXpOrCoins()
        {
            var save=new RunnerSave{highestLevel=20,level=20,money=BigMoney.Power10(9)};var session=new RunnerSession(save);session.Start(1,1);
            session.Collect();session.Finish();Assert.That(save.money.Equals(BigMoney.Power10(9)),Is.True);Assert.That(save.xp,Is.Zero);Assert.That(save.coins,Is.Zero);
        }
        [Test] public void FinishSecondChanceAndDailyRewardsAreIdempotent()
        {
            var save=new RunnerSave();var s=new RunnerSession(save);s.Start(1,1);s.SetMoney(BigMoney.Zero);s.Hit(.2f);
            Assert.That(s.SecondChance(),Is.True);Assert.That(s.SecondChance(),Is.False);Assert.That(s.Finish(),Is.True);int coins=save.coins;Assert.That(s.Finish(),Is.False);Assert.That(save.coins,Is.EqualTo(coins));
            var day=new DateTime(2026,9,17);Assert.That(RunnerProgress.DailyRefresh(save,day),Is.True);save.dailyClaims.Add("daily_0");
            Assert.That(RunnerProgress.DailyRefresh(save,day),Is.False);Assert.That(RunnerProgress.DailyRefresh(save,day.AddDays(-1)),Is.False);Assert.That(save.dailyClaims.Count,Is.EqualTo(1));
            Assert.That(RunnerProgress.DailyRefresh(save,day.AddDays(1)),Is.True);Assert.That(save.dailyClaims,Is.Empty);
        }
        [Test] public void EarlyEconomyAndGeneratedContentRemainBoundedAndValid()
        {
            BigMoney money=new BigMoney(100);
            for(int level=1;level<=5;level++)money+=RunnerProgress.CashValue(level).Times(40);
            Assert.That(money<new BigMoney(2000),Is.True);
            var catalog=JsonUtility.FromJson<LevelCatalog>(Resources.Load<TextAsset>("RunnerData/levels").text);Assert.That(catalog.levels.Length,Is.GreaterThanOrEqualTo(20));
            foreach(var level in catalog.levels)Assert.That(LevelGenerator.Validate(level),Is.Empty);
            for(int level=21;level<=500;level++)
            {
                var a=LevelGenerator.Generate(level,713+level*1949,0);var b=LevelGenerator.Generate(level,713+level*1949,0);
                Assert.That(LevelGenerator.Validate(a),Is.Empty);Assert.That(a.segments,Is.EqualTo(b.segments));
            }
        }
        [Test] public void CosmeticPurchaseChecksUnlocksAndNeverChangesMoney()
        {
            var save=new RunnerSave{coins=1000};var skin=new RunnerCosmetic{id="test",slot=CosmeticSlot.Character,cost=200,playerLevel=2};
            Assert.That(RunnerProgress.Buy(save,skin),Is.False);save.xp=100;Assert.That(RunnerProgress.Buy(save,skin),Is.True);Assert.That(save.coins,Is.EqualTo(800));
            Assert.That(RunnerProgress.Buy(save,skin),Is.True);Assert.That(save.coins,Is.EqualTo(800));Assert.That(save.money.Equals(new BigMoney(100)),Is.True);
        }
    }
}
