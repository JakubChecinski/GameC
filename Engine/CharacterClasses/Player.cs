﻿using System;
using System.Collections.Generic;
using System.Windows;
using Game.Display;
using Game.Engine.Skills;

namespace Game.Engine.CharacterClasses
{
    [Serializable]
    public abstract class Player : Subject
    {
        // statistics: Health, Strength, Armor, Precision, MagicPower, Stamina, XP (hidden), Level, Gold
        public List<Skill> ListOfSkills { get; set; }
        public int LostHP { get; set; } // sum of HP lost in battles
        protected GameSession parentSession;
        protected int xp, gold;

        // public properties
        public override int Health 
        {
            get 
            { return health + HealthBuff + parentSession.TmpBattleBuffs.Health; }
            set 
            {
                if (value < 0) health = 0;
                else health = value;
                if (health == 0)
                {
                    parentSession.SoundEngine.PlaySound(Sound.SoundNames.PLAYER_DEATH, Sound.SoundType.Player);
                    parentSession.SendText("");
                    parentSession.SendText("***********************************************************************************************");
                    parentSession.SendText("You lost! Press any key to continue.");
                    parentSession.GetKeyResponse();
                    parentSession.EndGame();
                    //Application.Current.Shutdown();
                }
            }
        }
        public override int Strength
        {
            get
            { return strength + StrengthBuff + parentSession.TmpBattleBuffs.Strength; }
            set
            {
                if (value < 0) strength = 0;
                else strength = value;
            }
        }
        public override int Armor
        {
            get
            { return armor + ArmorBuff + parentSession.TmpBattleBuffs.Armor; }
            set
            {
                if (value < 0) armor = 0;
                else armor = value;
            }
        }
        public override int Precision
        {
            get
            { return precision + PrecisionBuff + parentSession.TmpBattleBuffs.Precision; }
            set
            {
                if (value < 0) precision = 0;
                else precision = value;
            }
        }
        public override int MagicPower
        {
            get
            { return magicPower + MagicPowerBuff + parentSession.TmpBattleBuffs.MagicPower; }
            set
            {
                if (value < 0) magicPower = 0;
                else magicPower = value;
            }
        }
        public override int Stamina
        {
            get { return stamina + StaminaBuff + parentSession.TmpBattleBuffs.Stamina; }
            set
            {
                if (value < 0) stamina = 0;
                else stamina = value;
            }
        }
        public int XP
        {
            get { return xp; }
            set
            {
                xp = value;
                while (Level < LevelBasedOnXP())
                {
                    LevelUp();
                }
            }
        }
        public int Level { get; protected set; }
        public int Gold
        {
            get { return gold; }
            set
            {
                if (value < 0) gold = 0;
                else gold = value;
            }
        }
        public string ClassName { get; protected set; }

        // core stats and temporary buffs are stored separately so that they can be updated easily
        public int HealthBuff { get; set; }
        public int StrengthBuff { get; set; }
        public int ArmorBuff { get; set; }
        public int PrecisionBuff { get; set; }
        public int MagicPowerBuff { get; set; }
        public int StaminaBuff { get; set; }


        // methods
        protected Player(GameSession ses) // for derived classes
        {
            parentSession = ses;

            // this has to be the first skill 
            // if you need to change this, change SkillForgetInteraction as well
            ListOfSkills = new List<Skill>() { new RunAway() }; 
           
            Name = "player";
            Level = 1;  
        }
        private int LevelBasedOnXP()
        {
            // how much xp do you need to level up?
            // 100xp for level 2
            // 100+141 = 241xp for level 3 (this is total xp so you you begin level 2 with 100xp already)
            // 100+141+173 = 414xp for level 4 etc.
            int lvl = 1;
            int x = 0;
            while(x <= XP)
            {
                x += (int)(100 * Math.Sqrt(lvl));
                lvl++;
            }
            return lvl-1;
        }
        public void ResetBuffs() 
        {
            // convenience method that resets all buffs
            HealthBuff = 0;
            StrengthBuff = 0;
            ArmorBuff = 0;
            PrecisionBuff = 0;
            MagicPowerBuff = 0;
            StaminaBuff = 0;
        }
        public virtual void React(List<StatPackage> packs) 
        {
            // receive the result of your opponent's action
            foreach (StatPackage pack in packs)
            {
                strength -= pack.StrengthDmg;
                armor -= pack.ArmorDmg;
                precision -= pack.PrecisionDmg;
                magicPower -= pack.MagicPowerDmg;
                if (Strength < 0) strength = -StrengthBuff;
                if (Armor < 0) armor = -ArmorBuff;
                if (Precision < 0) precision = -PrecisionBuff;
                if (MagicPower < 0) magicPower = -MagicPowerBuff;
                // damageAfterArmor = 100.0 * damageBeforeArmor / (100.0 + Armor)
                // parentSession.UpdateStat(1, -1 * (100 * pack.HealthDmg) / (100 + Armor)); // not used here, because it would reset skill buffs
                parentSession.currentPlayer.Health += -1 * (100 * pack.HealthDmg) / (100 + Armor) - parentSession.currentPlayer.HealthBuff; // we use this instead
            }
        }
        protected virtual void LevelUp()
        {
            // override this for specific character classes, which may have easier time learning one stat vs another... 
            Level++;
            parentSession.SendText("\n");
            parentSession.SendColorText("Level Up! Level: " + Level, "yellow");
            List<string> validInputs = new List<string>() { "1", "2", "3", "4", "5" }; // only accept these inputs
            parentSession.SendColorText("Choose a statistic to improve: +10 Health (press 1), +10 Strength (press 2), +5 Precision (press 3), +10 Magic Power (press 4), +10 Stamina (press 5)", "yellow");
            string key = parentSession.GetValidKeyResponse(validInputs).Item1;
            // don't make changes directly, ask GameSession to do it right
            if (key == "1") parentSession.UpdateStat(1, 10);
            else if (key == "2") parentSession.UpdateStat(2, 10);
            else if (key == "3") parentSession.UpdateStat(4, 5);
            else if (key == "4") parentSession.UpdateStat(5, 10);
            else if (key == "5") parentSession.UpdateStat(6, 10);
        }
        public virtual void LearnNewSkill(List<Skill> learningSkills)
        {
            // learn a new skill from the list (maximum three choices)
            if (learningSkills.Count > 2) 
            {
                parentSession.SendColorText("Choose a skill to learn:", "yellow");
                parentSession.SendColorText(learningSkills[0]+ " (press 1)", "yellow");
                parentSession.SendColorText(learningSkills[1]+ " (press 2)", "yellow");
                parentSession.SendColorText(learningSkills[2]+ " (press 3)", "yellow");
                string key = parentSession.GetValidKeyResponse(new List<string>() { "1", "2", "3" }).Item1;
                if (key == "1") Learn(learningSkills[0]);
                else if (key == "2") Learn(learningSkills[1]);
                else if (key == "3") Learn(learningSkills[2]);
            }
            else if(learningSkills.Count > 1)
            {
                parentSession.SendColorText("Choose a skill to learn:", "yellow");
                parentSession.SendColorText(learningSkills[0] + " (press 1)", "yellow");
                parentSession.SendColorText(learningSkills[1] + " (press 2)", "yellow");
                string key = parentSession.GetValidKeyResponse(new List<string>() { "1", "2"}).Item1;
                if (key == "1") Learn(learningSkills[0]);
                else if (key == "2") Learn(learningSkills[1]);
            }
            else if (learningSkills.Count > 0)
            {
                parentSession.SendColorText("Choose a skill to learn:", "yellow");
                parentSession.SendColorText(learningSkills[0] + " (press 1)", "yellow");
                string key = parentSession.GetValidKeyResponse(new List<string>() {"1"}).Item1;
                if (key == "1") Learn(learningSkills[0]);
            }
        }
        public virtual void Learn(Skill skill)
        {
            // a method that helps LearnNewSkill
            // new skill means we just add it to the list
            if (skill.decoratedSkill == null) ListOfSkills.Add(skill);
            // otherwise we also need to remove the old one
            else
            {
                ListOfSkills.Remove(skill.decoratedSkill);
                ListOfSkills.Add(skill);
            }
        }
        public List<Skill> ListAvailableSkills(bool canRunAway = true) 
        {
            // return list of currently available skills (based on items and stamina)
            // to be used during battles
            List<Skill> tmp = new List<Skill>();
            foreach (Skill skill in ListOfSkills)
            {
                if (Stamina >= skill.StaminaCost && parentSession.TestForItemClass(skill.RequiredItem)) tmp.Add(skill);
            }
            if (tmp.Count > 1 && !canRunAway) tmp.RemoveAt(0);
            return tmp;
        }
    }
}
