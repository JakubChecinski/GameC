using System;
using System.Collections.Generic;
using Game.Display;
using Game.Engine.Monsters;
using Game.Engine.Skills;
using Game.Engine.Interactions;
using Game.Engine.CharacterClasses;
using Game.Sound;

namespace Game.Engine
{
    // class representing a battle event
    class Battle : InteractionWithImage
    {
        protected BattleScene battleScene;
        protected int hpCopy, strCopy, armCopy, prCopy, mgCopy, staCopy; // after the battle, all statistics of the player are restored
        protected bool rewards, possibleToEscape, firstBlood = false;
        public Monster Monster { get; set; }
        public bool battleResult { get; private set; } = false; // has the player won?
        public SoundEngine SoundEngine;
        public Battle(GameSession ses, BattleScene scene, Monster monster, bool rewards = true, bool possibleToEscape = true) : base(ses)
        {
            Monster = monster;
            Name = "battle0001";
            this.rewards = rewards;
            this.possibleToEscape = possibleToEscape;
            battleScene = scene;
            battleScene.ImgSetup = GetImage();
            SoundEngine = new SoundEngine(SoundContext.Battle);
            try
            {
                parentSession.ChildSoundEngines.Add(SoundEngine);
            }
            catch (Exception e)
            {
                parentSession.SendText("Nie udalo sie zainicjalizowac dzwieku: " + e.Message);
            }
        }

        protected override void RunContent()
        {
            // stop game music and play battle music
            parentSession.SoundEngine.PauseBackgroundMusic();
            SoundEngine.PlayBackgroundMusic();

            parentSession.SendText("\nPojedynek!");
            battleScene.SetupDisplay();
            CopyPlayerState();
            // battle
            if (Monster.BattleGreetings != null)
            {
                battleScene.SendColorText(Monster.BattleGreetings, "red");
                // play battle monster greetings 
                SoundEngine.WaitAndPlay(Monster.Name, SoundType.MonsterInit);
                battleScene.SendBattleText("");
            }
            if(!possibleToEscape)
            {
                battleScene.SendColorText("Ostrzezenie - z tego pojedynku nie mozna uciec", "yellow");
                battleScene.SendBattleText("");
            }
            battleScene.SetSkills(parentSession.CurrentPlayer.ListAvailableSkills(possibleToEscape));
            while (Monster.Health > 0) // reminder: there will be a separate mechanism for what happens when Player.Health == 0 
            {
                if(parentSession.CurrentPlayer.ListAvailableSkills(possibleToEscape).Count == 0) // player has run out of stamina
                {
                    RestorePlayerState();
                    battleScene.SendColorText("Zadne dalsze ruchy nie sa dostepne - porazka! (Nacisnij dowolny klawisz, aby kontynuowac)", "red");
                    parentSession.GetKeyResponse();
                    parentSession.SendText("Zadne dalsze ruchy nie sa dostepne - porazka!");
                    battleScene.EndDisplay();
                    // stop battle music and resume main game music
                    SoundEngine.StopBackgroundMusic();
                    parentSession.SoundEngine.ResumeBackgroundMusic();
                    return;
                }
                // player attacks first
                Skill playerResponse = parentSession.GetListBoxResponse();
                if (playerResponse.PublicName == "Ucieczka (tylko polowa odniesionych obrazen sie zagoi!)")
                {
                    battleScene.EndDisplay();
                    if (firstBlood) parentSession.SendText("Potwor sciga cie przez jakis czas i nie jestes w stanie opatrzyc swoich ran...");
                    else parentSession.SendText("Wyglada dosyc groznie, lepiej trzymac sie z daleka.");
                    battleResult = false;
                    RestorePlayerState(false);
                    // stop battle music and resume main game music
                    SoundEngine.StopBackgroundMusic();
                    parentSession.SoundEngine.ResumeBackgroundMusic();
                    return;
                }
                // play item sound
                SoundEngine.PlaySound(playerResponse.ReqItem.ToString(), SoundType.BattleRequiredItem);
                firstBlood = true;
                // calculate damage
                List<StatPackage> playerAttack = playerResponse.BattleMove(parentSession.CurrentPlayer, parentSession.GetActiveItemNames());
                List<StatPackage> memorizedAttack = new List<StatPackage>();
                foreach (StatPackage pack in playerAttack) memorizedAttack.Add(pack.Copy());
                playerAttack = parentSession.ModifyOffensive(playerAttack);
                for (int i = 0; i < playerAttack.Count && i < memorizedAttack.Count; i++)
                {
                    battleScene.SendColorText(playerAttack[i].CustomText, "green");
                    if (memorizedAttack[i].HealthDmg != playerAttack[i].HealthDmg)
                    {
                        battleScene.SendColorText("Twoje przedmioty pozwalaja zadac dodatkowe " + (-memorizedAttack[i].HealthDmg + playerAttack[i].HealthDmg) + " dmg!", "yellow");
                        memorizedAttack[i].HealthDmg = playerAttack[i].HealthDmg;
                    }
                    if (memorizedAttack[i].StrengthDmg != playerAttack[i].StrengthDmg)
                    {
                        battleScene.SendColorText("Twoje przedmioty pozwalaja zadac dodatkowe " + (-memorizedAttack[i].StrengthDmg + playerAttack[i].StrengthDmg) + " debuffa do sily!", "yellow");
                        memorizedAttack[i].StrengthDmg = playerAttack[i].StrengthDmg;
                    }
                    if (memorizedAttack[i].ArmorDmg != playerAttack[i].ArmorDmg)
                    {
                        battleScene.SendColorText("Twoje przedmioty pozwalaja zadac dodatkowe " + (-memorizedAttack[i].ArmorDmg + playerAttack[i].ArmorDmg) + " debuffa do pancerza!", "yellow");
                        memorizedAttack[i].ArmorDmg = playerAttack[i].ArmorDmg;
                    }
                    if (memorizedAttack[i].PrecisionDmg != playerAttack[i].PrecisionDmg)
                    {
                        battleScene.SendColorText("Twoje przedmioty pozwalaja zadac dodatkowe " + (-memorizedAttack[i].PrecisionDmg + playerAttack[i].PrecisionDmg) + " debuffa do precyzji!", "yellow");
                        memorizedAttack[i].PrecisionDmg = playerAttack[i].PrecisionDmg;
                    }
                    if (memorizedAttack[i].MagicPowerDmg != playerAttack[i].MagicPowerDmg)
                    {
                        battleScene.SendColorText("Twoje przedmioty pozwalaja zadac dodatkowe " + (-memorizedAttack[i].MagicPowerDmg + playerAttack[i].MagicPowerDmg) + " debuffa do mocy magicznej!", "yellow");
                        memorizedAttack[i].MagicPowerDmg = playerAttack[i].MagicPowerDmg;
                    }
                }
                playerAttack = Monster.React(playerAttack);
                for (int i = 0; i < playerAttack.Count && i < memorizedAttack.Count; i++)
                {
                    if (memorizedAttack[i].HealthDmg != playerAttack[i].HealthDmg) battleScene.SendColorText("Specjalne umiejetnosci potwora zatrzymuja " +
                        (memorizedAttack[i].HealthDmg - playerAttack[i].HealthDmg) + " dmg!", "yellow");
                    if (memorizedAttack[i].StrengthDmg != playerAttack[i].StrengthDmg) battleScene.SendColorText("Specjalne umiejetnosci potwora zatrzymuja " +
                        (memorizedAttack[i].StrengthDmg - playerAttack[i].StrengthDmg) + " debuffa do sily!", "yellow");
                    if (memorizedAttack[i].ArmorDmg != playerAttack[i].ArmorDmg) battleScene.SendColorText("Specjalne umiejetnosci potwora zatrzymuja " +
                        (memorizedAttack[i].ArmorDmg - playerAttack[i].ArmorDmg) + " debuffa do pancerza!", "yellow");
                    if (memorizedAttack[i].PrecisionDmg != playerAttack[i].PrecisionDmg) battleScene.SendColorText("Specjalne umiejetnosci potwora zatrzymuja " +
                        (memorizedAttack[i].PrecisionDmg - playerAttack[i].PrecisionDmg) + " debuffa do precyzji!", "yellow");
                    if (memorizedAttack[i].MagicPowerDmg != playerAttack[i].MagicPowerDmg) battleScene.SendColorText("Specjalne umiejetnosci potwora zatrzymuja " +
                        (memorizedAttack[i].MagicPowerDmg - playerAttack[i].MagicPowerDmg) + " debuffa do mocy magicznej!", "yellow");
                }
                parentSession.UpdateStat(6, -1*playerResponse.StaminaCost);
                battleScene.SetSkills(parentSession.CurrentPlayer.ListAvailableSkills(possibleToEscape));
                battleScene.ResetChoice();
                // monster attack
                if (Monster.Health == 0) continue;
                List<StatPackage> effectiveAttack = Monster.BattleMove();
                List<StatPackage> monsterAttack = new List<StatPackage>();
                foreach (StatPackage pack in effectiveAttack) monsterAttack.Add(pack.Copy());
                effectiveAttack = parentSession.ModifyDefensive(effectiveAttack);
                effectiveAttack = parentSession.CurrentPlayer.React(effectiveAttack);
                for (int i = 0; i < monsterAttack.Count && i < effectiveAttack.Count; i++) 
                {
                    battleScene.SendColorText(monsterAttack[i].CustomText, "red");
                    if (monsterAttack[i].HealthDmg != effectiveAttack[i].HealthDmg) battleScene.SendColorText("Twoj pancerz i przedmioty zatrzymuja " +
                        (monsterAttack[i].HealthDmg - effectiveAttack[i].HealthDmg) + " dmg!", "yellow");
                    if (monsterAttack[i].StrengthDmg != effectiveAttack[i].StrengthDmg) battleScene.SendColorText("Twoj pancerz i przedmioty zatrzymuja " +
                        (monsterAttack[i].StrengthDmg - effectiveAttack[i].StrengthDmg) + " debuffa do sily!", "yellow");
                    if (monsterAttack[i].ArmorDmg != effectiveAttack[i].ArmorDmg) battleScene.SendColorText("Twoj pancerz i przedmioty zatrzymuja " +
                        (monsterAttack[i].ArmorDmg - effectiveAttack[i].ArmorDmg) + " debuffa do pancerza!", "yellow");
                    if (monsterAttack[i].PrecisionDmg != effectiveAttack[i].PrecisionDmg) battleScene.SendColorText("Twoj pancerz i przedmioty zatrzymuja " +
                        (monsterAttack[i].PrecisionDmg - effectiveAttack[i].PrecisionDmg) + " debuffa do precyzji!", "yellow");
                    if (monsterAttack[i].MagicPowerDmg != effectiveAttack[i].MagicPowerDmg) battleScene.SendColorText("Twoj pancerz i przedmioty zatrzymuja " +
                        (monsterAttack[i].MagicPowerDmg - effectiveAttack[i].MagicPowerDmg) + " debuffa do mocy magicznej!", "yellow");
                }
                // play monster bite sound
                SoundEngine.WaitAndPlay(Monster.Name, SoundType.MonsterBite);
                battleScene.SendBattleText("");
                battleScene.RefreshStats();
                parentSession.RefreshStats();
            }
            // after battle - clean up
            SoundEngine.WaitAndPlay(Monster.Name, SoundType.MonsterDeath);
            battleResult = true;
            RestorePlayerState();
            battleScene.SendColorText("Zwyciestwo! (Nacisnij dowolny klawisz, aby kontynuowac)", "green");
            SoundEngine.WaitAndPlay(SoundNames.PLAYER_WIN, SoundType.Player);
            parentSession.GetKeyResponse();
            battleScene.EndDisplay();
            parentSession.ResetItemsAfterBattle();
            parentSession.SendText("Zwyciestwo! Zyskane punkty doswiadczenia: " + Monster.XPValue);
            if(rewards) VictoryReward();
            //parentSession.UpdateStat(7, Monster.XPValue); // for smoother display, this one was moved to GameSession.cs
            // stop battle music and resume main game music
            SoundEngine.StopBackgroundMusic();
            parentSession.SoundEngine.ResumeBackgroundMusic();
        }
        protected void CopyPlayerState()
        {
            // not very pretty, but I can't think of another solution that wouldn't make things more complicated
            hpCopy = parentSession.CurrentPlayer.Health - parentSession.CurrentPlayer.HealthBuff;
            strCopy = parentSession.CurrentPlayer.Strength - parentSession.CurrentPlayer.StrengthBuff;
            armCopy = parentSession.CurrentPlayer.Armor - parentSession.CurrentPlayer.ArmorBuff;
            prCopy = parentSession.CurrentPlayer.Precision - parentSession.CurrentPlayer.PrecisionBuff;
            mgCopy = parentSession.CurrentPlayer.MagicPower - parentSession.CurrentPlayer.MagicPowerBuff;
            staCopy = parentSession.CurrentPlayer.Stamina - parentSession.CurrentPlayer.StaminaBuff;
        }
        protected void RestorePlayerState(bool fullHP = true)
        {
            // restore statistics
            if (fullHP)
            {
                parentSession.CurrentPlayer.Health = hpCopy;
            }
            else
            {
                parentSession.CurrentPlayer.Health = (int)((parentSession.CurrentPlayer.Health + hpCopy) / 2);
                if (parentSession.CurrentPlayer.Health > hpCopy) parentSession.CurrentPlayer.Health = hpCopy;
                parentSession.CurrentPlayer.LostHP += hpCopy - parentSession.CurrentPlayer.Health + parentSession.CurrentPlayer.BattleBuffHealth;
                if (parentSession.CurrentPlayer.LostHP < 0) parentSession.CurrentPlayer.LostHP = 0; //workaround
            }
            parentSession.CurrentPlayer.Strength = strCopy;
            parentSession.CurrentPlayer.Armor = armCopy;
            parentSession.CurrentPlayer.Precision = prCopy;
            parentSession.CurrentPlayer.MagicPower = mgCopy;
            parentSession.CurrentPlayer.Stamina = staCopy;
            parentSession.CurrentPlayer.ResetBattleBuffs();
            parentSession.RefreshStats();
        }

        protected void VictoryReward()
        {
            Random RNG = new Random();
            int test = RNG.Next(100);
            if (test < 10)
            {
                parentSession.SendText("Wyglada na to, ze potwor strzegl interesujacego przedmiotu.");
                parentSession.AddRandomItem();
            }
            else if (test < 50)
            {
                int gold = 5 * (RNG.Next(9) + 1); ;
                parentSession.SendText("Wyglada na to, ze potwor strzegl torby ze zlotem (+" + gold + " sztuk zlota).");
                parentSession.CurrentPlayer.Gold += gold;
            }
        }

    }
}
