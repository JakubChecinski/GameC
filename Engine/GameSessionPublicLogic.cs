using System;
using System.Windows.Controls;
using System.Collections.Generic;
using Game.Engine.Items;
using Game.Engine.Skills;
using Game.Engine.Monsters;
using Game.Engine.Monsters.MonsterFactories;
using Game.Engine.Interactions;

namespace Game.Engine
{
    // publicly available methods for GameSession class
    // this is how other classes should interact with the gameplay
    partial class GameSession
    {
        public void SendText(string text)
        {
            // sends a string to the game console
            // no need to put an enter at the end/beginning
            parentPage.AddConsoleText(text);
        }
        public void SendColorText(string text, string color)
        {
            // same as above, but in color
            // available colors: "yellow", "red", "green", "blue"
            // warning - do not insert enters into the text or you may break the color formatting
            parentPage.AddConsoleColorText(text, color);
        }


        /***************************        KEYBOARD      ***************************/
        public Tuple<string,int> GetKeyResponse()
        {
            // usage: GetKeyResponse().Item1 will return the capital letter corresponding to the next key pressed by the user
            // GetKeyResponse().Item2 will return time before getting the response (in milliseconds)
            // do not use for keys other than A-Z or 0-9 (unless you know their windows codes)
            parentPage.Movable = false;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CurrentKey = "";
            while (CurrentKey == "")
            {
                Wait(100);
            }
            watch.Stop();
            if (CurrentKey.Length == 2 && CurrentKey[0] == 'D') CurrentKey = CurrentKey.Remove(0, 1); // remove windows code for digits
            parentPage.Movable = true;
            return new Tuple<string, int>(CurrentKey, (int)watch.ElapsedMilliseconds);
        }
        public Tuple<string,int> GetValidKeyResponse(List<string> validKeys)
        {
            // same as above, but only keys from the validKeys list will be accepted
            // program will not proceed until a correct key is pressed so make sure the user knows what to do
            if (validKeys.Count == 0) return null;
            parentPage.Movable = false;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CurrentKey = "";
            bool stillTesting = true;
            while (stillTesting)
            {
                Wait(100);
                if (CurrentKey.Length == 2 && CurrentKey[0] == 'D') CurrentKey = CurrentKey.Remove(0, 1);
                foreach (string vk in validKeys)
                {
                    if (CurrentKey == vk)
                    {
                        stillTesting = false;
                        break;
                    }
                }
            }
            watch.Stop();
            parentPage.Movable = true;
            return new Tuple<string, int>(CurrentKey, (int)watch.ElapsedMilliseconds);
        }


        /***************************       CLICKABLE    ***************************/
        public int GetListBoxChoice(List<string> choices)
        {
            // display a list of choices on game screen 
            // return choice clicked by player as int (number on the list, starts from zero)
            if (choices.Count == 0) return -1;
            Display.ListBoxInteractionDisplay inter = new Display.ListBoxInteractionDisplay(parentPage);
            inter.SetChoices(choices);
            while (inter.ChosenNumber < 0)
            {
                Wait(100);
            }
            inter.Finish();
            parentPage.IgnoreNextKey = true;
            return inter.ChosenNumber;
        }


        /***************************        ITEMS      ***************************/
        public List<string> GetActiveItemNames()
        {
            // return names of all currently active items (max 5) as List<string>
            List<string> ans = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                Image img = parentPage.GetImageFromGrid(i, 0);
                if (img != null)
                {
                    if (img.Name != "")
                    {
                        ans.Add(img.Name);
                    }
                }
            }
            return ans;
        }

        public List<string> GetAllItemNames()
        {
            // return names of all items (max 30) as List<string>
            List<string> ans = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Image img = parentPage.GetImageFromGrid(j, i);
                    if (img != null)
                    {
                        if (img.Name != "")
                        {
                            ans.Add(img.Name);
                        }
                    }
                }
            }
            return ans;
        }

        public void RemoveThisItem(Item it)
        {
            // player will lose a particular item (if currently owned)
            // example usage: RemoveThisItem(new BasicSpear());

            RemoveItemFromGrid(it);
            // reset itemPositions
            itemPositions.Clear();
            for (int i = 0; i < 30; i++)
            {
                Image img = parentPage.GetImageFromGrid(i, 0);
                if (img != null)
                {
                    itemPositions.Add(i);
                }
            }
        }

        public bool TestForItem(string name)
        {
            // check if a particular item is currently equipped as active
            List<string> actives = GetActiveItemNames();
            foreach (string s in actives)
            {
                if (s == name) return true;
            }
            return false;
        }
        public bool TestForItemClass(RequiredItem name)
        {
            // check if ANY item from a given special class is currently equipped as active
            // possible arguments: RequiredItem.Sword, RequiredItem.Axe, RequiredItem.Spear, RequiredItem.Staff
            switch (name)
            {
                case RequiredItem.None:
                    return true;
                case RequiredItem.Sword:
                    foreach (Item item in activeItems) if (item.IsSword) return true;
                    break;
                case RequiredItem.Axe:
                    foreach (Item item in activeItems) if (item.IsAxe) return true;
                    break;
                case RequiredItem.Spear:
                    foreach (Item item in activeItems) if (item.IsSpear) return true;
                    break;
                case RequiredItem.Staff:
                    foreach (Item item in activeItems) if (item.IsStaff) return true;
                    break;
            }
            return false;
        }
        public void AddRandomClassItem()
        {
            // player receives a random item that is guaranteed to fit their class
            // (e.g. a warrior will not get magic staffs and a mage will not get axes, swords or spears)
            Item it = Index.RandomClassItem(this);
            for (int i = 0; i < 30; i++)
            {
                if (!itemPositions.Contains(i))
                {
                    InsertItemToGrid(it, i);
                    break;
                }
            }
        }
        public void AddRandomItem()
        {
            // player receives a random item (no class restrictions)
            Item it = Index.RandomItem(this);
            for (int i = 0; i < 30; i++)
            {
                if (!itemPositions.Contains(i))
                {
                    InsertItemToGrid(it, i);
                    break;
                }
            }
        }
        public void AddThisItem(Item it)
        {
            // player receives a particular item
            // you may want to use this method together with Index methods for item production:
            // Index.RandomItem() for random item
            // Index.RandomClassItem() for random class-suitable item
            for (int i = 0; i < 30; i++)
            {
                if (!itemPositions.Contains(i))
                {
                    InsertItemToGrid(it, i);
                    break;
                }
            }
        }

        /***************************        SKILLS      ***************************/
        public void LearnThisSkill(Skill sk)
        {
            // player learns a particular skill
            // if you use this method, you should know exactly what skill you want the player to learn
            CurrentPlayer.Learn(sk);
        }

        /***************************        FIGHTS      ***************************/
        public bool FightRandomMonster(bool possibleToEscape = false)
        {
            // player will fight against a random monster
            // returns battle result (true = victory)
            // xp can be gained here, but gold/items cannot (you can do this separately inside your interaction)
            // unlike fights with wild monsters, in this fight it is impossible to run away by default
            // if you want to make it possible to run away, provide the optional argument and set it to true
            try
            {
                Monster monster = Index.RandomMonsterFactory().Clone().Create();
                if (monster != null)
                {
                    Display.BattleScene newBattleScene = new Display.BattleScene(parentPage, this, CurrentPlayer, monster);
                    Battle newBattle = new Battle(this, newBattleScene, monster, false, possibleToEscape);
                    newBattle.Run();
                    if (newBattle.battleResult) UpdateStat(7, monster.XPValue);
                    return newBattle.battleResult;
                }
            }
            catch (IndexOutOfRangeException e)
            {
                parentPage.AddConsoleText("Podjeto nieudana probe wygenerowania potwora. Czy klasa Index zostala zaktualizowana?");
                parentPage.AddConsoleText(e.Message);     
            }
            return false;
        }

        public bool FightThisMonster(Monster monster, bool possibleToEscape = false)
        {
            // player will fight against a particular monster
            // returns battle result (true = victory)
            // xp can be gained here, but gold/items cannot (you can do this separately inside your interaction)
            // unlike fights with wild monsters, in this fight it is impossible to run away by default
            // if you want to make it possible to run away, provide the optional second argument and set it to true
            if (monster != null)
            {
                Display.BattleScene newBattleScene = new Display.BattleScene(parentPage, this, CurrentPlayer, monster);
                Battle newBattle = new Battle(this, newBattleScene, monster, false, possibleToEscape);
                newBattle.Run();
                if (newBattle.battleResult) UpdateStat(7, monster.XPValue);
                return newBattle.battleResult;
            }
            return true;
        }

        /***************************        MAP MODIFICATIONS      ***************************/
        public void AddMonstersToMap(MonsterFactory monsterFactory)
        {
            // add new monster factory to a random place in the game
            metaMapMatrix.AddMonsterToRandomMap(monsterFactory);
        }
        public void AddInteractionToMap(Interaction interaction)
        {
            // add new interaction to a random place in the game
            // remember to set all important parameters inside the interaction first
            metaMapMatrix.AddInteractionToRandomMap(interaction);
        }
        public void RemoveCurrentlyVisitedInteraction()
        {
            // permanently remove currently visited interaction from the game
            // can be used if your interaction has been completed and is no longer needed
            // not intended for monsters or other map elements
            mapMatrix.Matrix[PlayerPosTop, PlayerPosLeft] = 1;
            InitializeMapDisplay(-1);
        }

        /***************************        PLAYER STATISTICS      ***************************/
        public void UpdateStat(int number, int value)
        {
            // use this method to change player statistics
            // important: this method is for PERMANENT changes, do not use it to apply conditional buffs (e.g. from items)
            // if you are writing an item class and you want to apply a conditional buff, see Item.ApplyBuffs for an example of how to do that

            // usage: UpdateStat(statCode, changeValue)
            // statCodes for each statistic:
            // 1 - health
            // 2 - strength
            // 3 - armor
            // 4 - precision
            // 5 - magic power
            // 6 - stamina
            // 7 - experience points (character level is updated automatically and NOT meant to be changed directly)
            // 8 - gold
            // the method itself allows for any change (negative stat values will default to zero), but external balance guidelines may apply
            switch (number)
            {
                case 1:
                    CurrentPlayer.Health += value - CurrentPlayer.HealthBuff;
                    break;
                case 2:
                    CurrentPlayer.Strength += value - CurrentPlayer.StrengthBuff;
                    break;
                case 3:
                    CurrentPlayer.Armor += value - CurrentPlayer.ArmorBuff;
                    break;
                case 4:
                    CurrentPlayer.Precision += value - CurrentPlayer.PrecisionBuff;
                    break;
                case 5:
                    CurrentPlayer.MagicPower += value - CurrentPlayer.MagicPowerBuff;
                    break;
                case 6:
                    CurrentPlayer.Stamina += value - CurrentPlayer.StaminaBuff - CurrentPlayer.BattleBuffStamina;
                    break;
                case 7:
                    CurrentPlayer.XP += value;
                    break;
                case 8:
                    CurrentPlayer.Gold += value;
                    break;
            }
            RefreshStats();
        }

        public int CheckStat(int number)
        {
            // check any given statistic
            // use the same codes as in UpdateStat
            // except for code 7, which returns player level instead of XP
            switch (number)
            {
                case 1:
                    return CurrentPlayer.Health;
                case 2:
                    return CurrentPlayer.Strength;
                case 3:
                    return CurrentPlayer.Armor;
                case 4:
                    return CurrentPlayer.Precision;
                case 5:
                    return CurrentPlayer.MagicPower;
                case 6:
                    return CurrentPlayer.Stamina;
                case 7:
                    return CurrentPlayer.Level;
                case 8:
                    return CurrentPlayer.Gold;
            }
            return 0;
        }

    }
}
