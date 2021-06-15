using System;
using System.Collections.Generic;
using Game.Engine.Skills;
using Game.Engine.Items;
using Game.Engine.CharacterClasses;
using Game.Engine.Monsters;
using Game.Engine.Monsters.MonsterFactories;
using Game.Engine.Interactions;

namespace Game.Engine
{
    // methods used to generate skills, items and monsters by the game engine
    public static partial class Index
    {
        // safe random number generator
        private static Random rnd = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
        public static int RNG(int x1, int x2)
        {
            return rnd.Next(x1, x2);
        }

        // create a list of three random magic spells
        public static List<Skill> MagicSpell(Player player)
        {
            List<Skill> newSkills = new List<Skill>();
            int counter = 0;
            while (newSkills.Count < 3)
            {
                Skill generated = skillFactories[RNG(0, skillFactories.Count)].CreateSkill(player);
                if (generated != null) newSkills.Add(generated);
                counter++;
                if (counter > 10) break; // safety check if there are less than three skills left in the library
            }
            return newSkills;
        }
        
        // produce a specific item based on its name - important for the display-engine interactions
        public static Item ProduceSpecificItem(string name)
        {
            foreach(Item item in Items)
            {
                if (item.Name == name) return item;
            }
            return null;
        }
        
        // produce random class-specific or non-class-specific items - this one will be used to populate the game world
        public static Item RandomItem(GameSession parentSession)
        {
            return itemFactories[RNG(0, itemFactories.Count)].CreateAnyItem(parentSession);
        }
        public static Item RandomClassItem(GameSession parentSession)
        {
            Player player = parentSession.CurrentPlayer;
            if (player.ClassName == "Mage") return itemFactories[RNG(0, itemFactories.Count)].CreateNonWeaponItem(parentSession);
            else if (player.ClassName == "Warrior") return itemFactories[RNG(0, itemFactories.Count)].CreateNonMagicItem(parentSession);
            else return itemFactories[RNG(0, itemFactories.Count)].CreateAnyItem(parentSession);
        }

        // produce random monster factory
        public static MonsterFactory RandomMonsterFactory()
        {
            return MonsterFactories[RNG(0, MonsterFactories.Count)].Clone();
        }

        // produce an interaction or a group of interactions
        public static List<Interaction> DrawInteractions(GameSession parentSession)
        {
            return InteractionFactories[RNG(0, InteractionFactories.Count)].CreateInteractionsGroup(parentSession);
        }

        // returns an interaction (no quest limitations) from the factory based on the specified number 
        // example: number=4 will return "interaction0004" if it exists (or ShopInteraction by default)
        public static Interaction GetInteractionByNumber(GameSession parentSession, int number)
        {
            int interNumber;
            foreach (Interaction inter in Interactions)
            {
                int.TryParse(inter.Name.Substring(11), out interNumber);
                if (interNumber == number) return inter;
            }
            return new ShopInteraction(parentSession);
        }

        // for MetaMapMatrix
        public static int NumberOfMonsters()
        {
            return MonsterFactories.Count;
        }

    }
}
