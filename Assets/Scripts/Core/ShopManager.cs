using System;
using System.Collections.Generic;

namespace StealAMillion.Core
{
    [Serializable]
    public sealed class CosmeticData
    {
        public string id;
        public string name;
        public int price;
        public string color;
    }

    [Serializable]
    public sealed class ShopCatalog { public CosmeticData[] items; }

    public sealed class ShopManager
    {
        private readonly SaveData save;
        public readonly CosmeticData[] Items;
        public ShopManager(SaveData data, ShopCatalog catalog)
        {
            save = data;
            var valid = new List<CosmeticData>();
            var ids = new HashSet<string>();
            if (catalog != null && catalog.items != null)
                foreach (var item in catalog.items)
                    if (item != null && !string.IsNullOrEmpty(item.id) && item.price >= 0 && ids.Add(item.id)) valid.Add(item);
            if (!ids.Contains("classic")) valid.Insert(0, new CosmeticData { id = "classic", name = "CLASSIC", price = 0, color = "#70E0AF" });
            Items = valid.ToArray();
        }
        public bool BuyOrEquip(string id)
        {
            var item = Array.Find(Items, value => value != null && value.id == id);
            if (item == null || item.price < 0) return false;
            if (!save.unlockedCosmetics.Contains(id))
            {
                if (save.coins < item.price) return false;
                save.coins -= item.price;
                save.unlockedCosmetics.Add(id);
            }
            save.equippedCosmetic = id;
            return true;
        }
    }
}
