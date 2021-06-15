using Game.Engine.Items;
using System;
using System.Collections.Generic;

namespace Game.Engine.Interactions
{
    // a special interaction used for buying and selling items
    // if you want a clear example of how to write your own interesting interaction, this is probably NOT the right place 
    // see Gymir and Hymir files instead
    [Serializable]
    class ShopInteraction : PlayerInteraction
    {
        private Item it1, it2, it3;
        public ShopInteraction(GameSession parentSession) : base(parentSession)
        {
            while (it1 == null) it1 = Index.RandomClassItem(parentSession);
            while (it2 == null) it2 = Index.RandomClassItem(parentSession);
            while (it3 == null) it3 = Index.RandomClassItem(parentSession);
            Name = "interaction0001";
        }
        protected override void RunContent()
        {
            parentSession.SendText("\nWitaj! Mozesz tutaj sprzedac swoje przedmioty poprzez przeciagniecie ich na zewnatrz planszy z ekwipunkiem.");
            parentSession.SendText("Mozesz takze nacisnac I aby sprawdzic wartosc swoich przedmiotow, B aby kupic nowe przedmioty oraz ENTER aby wyjsc.");
            parentSession.RemovableItems = true;
            parentSession.ItemSellFlag = true;
            while (true)
            {
                string key = parentSession.GetValidKeyResponse(new List<string>() { "Return", "I", "B" }).Item1;
                if (key == "Return") break;
                else if (key == "I") parentSession.ListAllItemsCost();
                else
                {
                    parentSession.SendText("Mamy w ofercie nastepujace przedmioty: ");
                    if (it1 != null) parentSession.SendText(it1.PublicName + " za " + (it1.GoldValue + 20) + " sztuk zlota (nacisnij 1)");
                    if (it2 != null) parentSession.SendText(it2.PublicName + " za " + (it2.GoldValue + 20) + " sztuk zlota (nacisnij 2)");
                    if (it3 != null) parentSession.SendText(it3.PublicName + " za " + (it3.GoldValue + 20) + " sztuk zlota (nacisnij 3)");
                    while (true)
                    {
                        string key2 = parentSession.GetValidKeyResponse(new List<string>() { "Return", "1", "2", "3" }).Item1;
                        if (key2 == "Return")
                        {
                            parentSession.RemovableItems = false;
                            parentSession.ItemSellFlag = false;
                            return;
                        }
                        else if (key2 == "1") SellItem(1);
                        else if (key2 == "2") SellItem(2);
                        else if (key2 == "3") SellItem(3);
                    }
                }
            }
            parentSession.RemovableItems = false;
            parentSession.ItemSellFlag = false;
        }
        protected void SellItem(int i)
        {
            Item it = null;
            switch (i)
            {
                case 1:
                    it = it1;
                    break;
                case 2:
                    it = it2;
                    break;
                case 3:
                    it = it3;
                    break;
            }
            if (it == null) return;
            if (parentSession.CurrentPlayer.Gold >= it.GoldValue + 20)
            {
                parentSession.AddThisItem(it);
                parentSession.UpdateStat(8, -1 * it.GoldValue - 20);
                switch (i)
                {
                    case 1:
                        it1 = null;
                        break;
                    case 2:
                        it2 = null;
                        break;
                    case 3:
                        it3 = null;
                        break;
                }
            }
            else parentSession.SendText("Nie masz wystarczajaco wiele zlota!");
        }
    }
}
