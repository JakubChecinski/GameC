using System;
using System.Collections.Generic;
using Game.Engine.Skills.SkillFactories;
using Game.Engine.Monsters.MonsterFactories;
using Game.Engine.Items;
using Game.Engine.Items.ItemFactories;
using Game.Engine.Items.BasicArmor;
using Game.Engine.Interactions.InteractionFactories;
using Game.Engine.Interactions;


namespace Game.Engine
{
    // contains information about skills, items and monsters that will be available in the game
    public static partial class Index
    {
        private readonly static List<SkillFactory> skillFactories = new List<SkillFactory>()
        {
            new BasicSpellFactory(),
        };

        public static List<Item> Items = new List<Item>()
        {
            new BasicStaff(),
            new BasicSpear(),
            new BasicAxe(),
            new BasicSword(),
            new SteelArmor(),
            new AntiMagicArmor(),
            new BerserkerArmor(),
            new GrowingStoneArmor(),
            // quest items (if applicable) start below
            new GymirAxe(),
        };

        private readonly static List<ItemFactory> itemFactories = new List<ItemFactory>()
        {
            new BasicArmorFactory(),
        };

        public readonly static List<MonsterFactory> MonsterFactories = new List<MonsterFactory>()
        {
            new Monsters.MonsterFactories.RatFactory(),
            new Monsters.MonsterFactories.BatFactory(),
            new Monsters.MonsterFactories.SpiderFactory(),
        };

        public readonly static List<InteractionFactory> InteractionFactories = new List<InteractionFactory>()
        {
            new SkillForgetFactory(),
            new HealInteractionFactory(),
        };

        public readonly static InteractionFactory MainQuestFactory = new GymirHymirFactory();

        public readonly static List<InteractionFactory> SideQuestFactory = new List<InteractionFactory>()
        {
            new GymirHymirFactory()
        };

        public static List<Interaction> Interactions = new List<Interaction>()
        {
        };

    }
}
