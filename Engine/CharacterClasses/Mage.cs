using System;
using System.Collections.Generic;
using Game.Engine.Skills;

namespace Game.Engine.CharacterClasses
{
    [Serializable]
    class Mage : Player
    {
        // mage class  - overrides only initial statistics and levelling up 
        public Mage(GameSession ses) : base(ses)
        {
            // initial class statistics
            ClassName = "Mage"; 
            Health = 100;
            Strength = 20;
            Armor = 0;
            Precision = 50;
            MagicPower = 50;
            Stamina = 100;
            Level = 1;
            Gold = 0;
            Name = "playerMage";
        }

        protected override void LevelUp()
        {
            Level++;
            parentSession.SendText("\n");
            parentSession.SendColorText("Nowy poziom! Poziom: " + Level, "yellow");
            List<string> validInputs = new List<string>() { "1", "2", "3", "4" }; // only accept these inputs
            parentSession.SendColorText("Wybierz statystyke do ulepszenia: +10 Zdrowia (nacisnij 1), +5 Precyzji (nacisnij 2), +10 Mocy Magicznej (nacisnij 3), +20 Energii (nacisnij 4)", "yellow");
            string key = parentSession.GetValidKeyResponse(validInputs).Item1;
            // don't make changes directly, ask GameSession to do it right
            if (key == "1") parentSession.UpdateStat(1, 10);
            else if (key == "2") parentSession.UpdateStat(4, 5);
            else if (key == "3") parentSession.UpdateStat(5, 10);
            else if (key == "4") parentSession.UpdateStat(6, 20);
            LearnNewSkill(Index.MagicSpell(this)); // mages learn a new spell every time
        }

    }
}
