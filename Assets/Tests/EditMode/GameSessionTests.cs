using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using StealAMillion.Core;
using UnityEngine;

namespace StealAMillion.Tests
{
    public sealed class GameSessionTests
    {
        private DecisionCatalog catalog;
        private GameSession session;

        [SetUp]
        public void SetUp()
        {
            catalog = JsonUtility.FromJson<DecisionCatalog>(Resources.Load<TextAsset>("Data/decisions").text);
            session = new GameSession(catalog, new SaveData(), 100, 1000000);
            session.NewRun();
        }

        [Test]
        public void UnityJsonLoadsThirtyDecisionsAcrossTenLevels()
        {
            Assert.That(catalog.Validate(), Is.Empty);
            Assert.That(catalog.decisions.Length, Is.EqualTo(30));
            Assert.That(catalog.decisions.GroupBy(d => d.level).Count(), Is.EqualTo(10));
        }

        [Test]
        public void UnityJsonPreservesPendingLossAndPermanentSettings()
        {
            session.Save.soundEnabled = false;
            session.Save.unlockedCosmetics.Add("gold");
            session.Save.equippedCosmetic = "gold";
            session.Choose(true, .9f);
            var data = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(session.Save));
            var restored = new GameSession(catalog, data, 100, 1000000);
            Assert.That(restored.Save.state, Is.EqualTo(RunState.ShowingResult));
            Assert.That(restored.Save.pendingResult.after, Is.Zero);
            Assert.That(restored.Save.soundEnabled, Is.False);
            Assert.That(restored.Save.equippedCosmetic, Is.EqualTo("gold"));
            restored.CompleteResult();
            Assert.That(restored.Save.state, Is.EqualTo(RunState.GameOver));
        }

        [Test]
        public void AllSafeRouteWinsOnTheThirtiethDecision()
        {
            for (int i = 0; i < 30; i++)
            {
                Assert.That(session.Save.state, Is.EqualTo(RunState.Playing));
                Assert.That(session.Choose(false, 0), Is.Not.Null);
                session.CompleteResult();
            }
            Assert.That(session.Save.state, Is.EqualTo(RunState.Victory));
            Assert.That(session.Money.Current, Is.EqualTo(1000000));
            Assert.That(session.Save.coins, Is.EqualTo(1475));
        }

        [Test]
        public void DuplicateInputCannotChangeACommittedOutcome()
        {
            session.Choose(false, 0);
            Assert.That(session.Choose(true, 1), Is.Null);
            session.CompleteResult();
            Assert.That(session.CompleteResult(), Is.False);
            Assert.That(session.Money.Current, Is.EqualTo(200));
            Assert.That(session.Save.decisionIndex, Is.EqualTo(1));
        }

        [TestCase(.799f, true)]
        [TestCase(.8f, false)]
        [TestCase(1f, false)]
        public void RiskMatchesDisplayedProbability(float sample, bool win)
        {
            Assert.That(session.Choose(true, sample).riskWon, Is.EqualTo(win));
        }

        [Test]
        public void SecondChanceCanOnlyBeUsedOncePerRun()
        {
            session.Choose(true, 1);
            session.CompleteResult();
            Assert.That(session.SecondChance(500), Is.True);
            session.Choose(true, 1);
            session.CompleteResult();
            Assert.That(session.SecondChance(500), Is.False);
        }

        [Test]
        public void InvalidSaveValuesAreSanitized()
        {
            var data = new SaveData { hasRun = true, currentMoney = -100, coins = -1, decisionIndex = int.MaxValue, unlockedCosmetics = null };
            var restored = new GameSession(catalog, data, 100, 1000000);
            Assert.That(restored.Money.Current, Is.Zero);
            Assert.That(restored.Save.coins, Is.Zero);
            Assert.That(restored.Save.decisionIndex, Is.EqualTo(29));
            Assert.That(restored.Save.unlockedCosmetics, Does.Contain("classic"));
        }

        [Test]
        public void DiskSaveRecoversBackupUsingUnityJson()
        {
            string folder = Path.Combine(Application.temporaryCachePath, "SAM-Test-" + Guid.NewGuid().ToString("N"));
            string file = Path.Combine(folder, "save.dat");
            var store = new SaveStore(file, data => JsonUtility.ToJson(data), json => JsonUtility.FromJson<SaveData>(json));
            try
            {
                Assert.That(store.Write(session.Save), Is.True);
                session.Choose(false, 0);
                Assert.That(store.Write(session.Save), Is.True);
                File.WriteAllText(file, "broken");
                Assert.That(store.Load().currentMoney, Is.EqualTo(100));
            }
            finally
            {
                if (Directory.Exists(folder)) Directory.Delete(folder, true);
            }
        }
    }
}
