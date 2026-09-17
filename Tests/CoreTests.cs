using System;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using StealAMillion.Core;

internal static class CoreTests
{
    private static readonly JavaScriptSerializer Json = new JavaScriptSerializer();
    private static DecisionCatalog catalog;
    private static int count;

    private static void Check(bool value, string name)
    {
        if (!value) throw new Exception("FAIL: " + name);
        count++;
        Console.WriteLine("PASS: " + name);
    }

    private static GameSession NewSession()
    {
        var game = new GameSession(catalog, new SaveData(), 100, 1000000);
        game.NewRun();
        return game;
    }

    public static int Main(string[] args)
    {
        try
        {
            catalog = Json.Deserialize<DecisionCatalog>(File.ReadAllText(args[0]));
            Check(catalog.Validate().Count == 0, "Production decision catalog validates");
            Check(catalog.decisions.Length >= 30, "At least 30 decisions");
            Check(catalog.decisions.GroupBy(d => d.level).Count() == 10, "10 levels");
            Check(catalog.decisions.GroupBy(d => d.level).All(g => g.Count() == 3), "Three decisions per level");
            Check(MoneyManager.Format(1000000) == "$1,000,000", "Invariant money formatting");
            var money = new MoneyManager(100);
            int events = 0;
            money.Changed += (a, b) => events++;
            money.Subtract(200);
            Check(money.Current == 0 && events == 1, "Loss clamps to zero and notifies");
            money.Add(long.MaxValue);
            Check(money.Current == MoneyManager.Limit, "Addition does not overflow");
            money.Add(long.MinValue);
            Check(money.Current == 0, "Minimum long does not underflow");
            Check(new Reward { mode = RewardMode.Multiply, amount = long.MaxValue }.Apply(100) == MoneyManager.Limit, "Multiplier does not overflow");

            var game = NewSession();
            var result = game.Choose(false, 0);
            Check(result.after == 200, "First SAFE adds $100");
            Check(game.Choose(true, 0) == null, "Double selection is rejected");
            Check(game.Save.decisionIndex == 0, "Result retains original decision until acknowledged");
            game.CompleteResult();
            Check(game.Save.decisionIndex == 1, "Result advances exactly one decision");
            Check(!game.CompleteResult() && game.Save.decisionIndex == 1, "Double result completion is rejected");

            game = NewSession();
            result = game.Choose(true, .799f);
            Check(result.riskWon && result.after == 350, "Risk below 80 percent wins");
            game = NewSession();
            result = game.Choose(true, .80f);
            Check(!result.riskWon && result.after == 0, "Exact probability boundary loses");
            Check(game.Save.pendingResult != null, "Resolved outcome is stored before animation");
            var reloaded = new GameSession(catalog, Json.Deserialize<SaveData>(Json.Serialize(game.Save)), 100, 1000000);
            Check(reloaded.Save.state == RunState.ShowingResult && reloaded.Money.Current == 0, "Pending loss survives restart without reroll");
            reloaded.CompleteResult();
            Check(reloaded.Save.state == RunState.GameOver && reloaded.Save.runPeak == 100, "Zero triggers Game Over and preserves peak");
            Check(reloaded.SecondChance(500) && reloaded.Money.Current == 500, "Second chance restores configured money");
            reloaded.Choose(true, 1);
            reloaded.CompleteResult();
            Check(!reloaded.SecondChance(500), "Second chance cannot be repeated");

            game = NewSession();
            int steps = 0;
            while (game.Save.state == RunState.Playing && steps < 100)
            {
                game.Choose(false, 0);
                game.CompleteResult();
                steps++;
            }
            Check(game.Save.state == RunState.Victory && steps == 30 && game.Money.Current == 1000000, "All SAFE reaches exactly one million on decision 30");
            Check(game.Save.coins == 1475 && game.Save.completedRuns == 1, "Level and victory coins granted once");
            reloaded = new GameSession(catalog, game.Save, 100, 1000000);
            Check(reloaded.Save.coins == 1475, "Victory reload does not duplicate coins");
            game.NewRun();
            Check(game.Money.Current == 100 && game.Save.coins == 1475 && game.Save.highestMoney == 1000000, "New game preserves permanent progression");

            var loopSave = new SaveData { hasRun = true, decisionIndex = 29, currentMoney = 100, state = RunState.Playing };
            game = new GameSession(catalog, loopSave, 100, 1000000);
            game.Choose(false, 0);
            game.CompleteResult();
            Check(game.Save.decisionIndex == 27 && game.Save.currentLevel == 10, "Below-target final decision returns to final level");

            var bad = Json.Deserialize<DecisionCatalog>(Json.Serialize(catalog));
            bad.decisions[0].riskProbability = float.NaN;
            Check(bad.Validate().Count > 0, "NaN probability rejected");
            bad.decisions[0].riskProbability = 1.1f;
            Check(bad.Validate().Count > 0, "Out-of-range probability rejected");
            bad.decisions[0].riskProbability = .8f;
            bad.decisions[0].level = 0;
            Check(bad.Validate().Count > 0, "Invalid level rejected");
            bad.decisions[0].level = 2;
            Check(bad.Validate().Count > 0, "Catalog must start at level one");
            bad.decisions[0].level = 1;
            bad.decisions[0].riskLossReward.mode = (RewardMode)99;
            Check(bad.Validate().Count > 0, "Unknown reward mode rejected");

            game = NewSession();
            var shop = new ShopManager(game.Save, Json.Deserialize<ShopCatalog>(File.ReadAllText(args[1])));
            Check(shop.BuyOrEquip("gold") && game.Save.coins == 400, "Cosmetic purchase spends coins");
            Check(shop.BuyOrEquip("gold") && game.Save.coins == 400, "Owned cosmetic is not charged again");
            Check(!shop.BuyOrEquip("diamond") && !shop.BuyOrEquip("unknown"), "Unaffordable and missing cosmetics rejected");
            var brokenShop = new ShopManager(game.Save, new ShopCatalog { items = new CosmeticData[] { null, new CosmeticData { id = "bad", price = -1 } } });
            Check(brokenShop.Items.Length == 1 && brokenShop.BuyOrEquip("classic"), "Missing or invalid cosmetics recover to Classic");
            shop.BuyOrEquip("gold");

            string folder = Path.Combine(args[2], "save-tests");
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, "progress.sav");
            var store = new SaveStore(path, s => Json.Serialize(s), s => Json.Deserialize<SaveData>(s));
            Check(store.Reset(), "Reset initializes both save generations");
            Check(store.Write(game.Save), "Save written to disk");
            Check(store.Load().equippedCosmetic == "gold", "Cosmetic persisted");
            game.Choose(true, 0);
            Check(store.Write(game.Save) && store.Load().pendingResult.after == 350, "Pending result persisted to disk");
            File.WriteAllText(path, "corrupt");
            Check(store.Load().equippedCosmetic == "gold", "Corrupt primary recovers backup");
            File.WriteAllText(path + ".bak", "corrupt");
            Check(store.Load().coins == 1000 && !store.Load().hasRun, "Both corrupt saves recover safe defaults");
            Check(store.Reset() && store.Load().equippedCosmetic == "classic", "Reset clears all progression");

            var random = new Random(713);
            int victories = 0;
            int losses = 0;
            for (int run = 0; run < 10000; run++)
            {
                game = NewSession();
                for (int step = 0; step < 300 && game.Save.state == RunState.Playing; step++)
                {
                    game.Choose(random.NextDouble() < .5, (float)random.NextDouble());
                    game.CompleteResult();
                    if (game.Money.Current < 0 || game.Save.decisionIndex >= 30) throw new Exception("Invalid simulation state");
                }
                if (game.Save.state == RunState.Victory) victories++;
                else if (game.Save.state == RunState.GameOver) losses++;
                else throw new Exception("Run failed to terminate");
            }
            Check(victories > 0 && losses > 0, "10,000 mixed-strategy runs terminate with valid states");
            Console.WriteLine("Simulation: " + victories + " victories / " + losses + " losses (50% SAFE, 50% RISK).");
            Console.WriteLine("All " + count + " checks passed.");
            return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e); return 1; }
    }
}
