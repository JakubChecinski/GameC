﻿using System;
using System.Collections.Generic;
using Game.Display;
using Game.Engine.Monsters;
using Game.Engine.Skills;
using Game.Engine.Interactions;
using Game.Engine.CharacterClasses;

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
        public Battle(GameSession ses, BattleScene scene, Monster monster, bool rewards = true, bool possibleToEscape = true) : base(ses)
        {
            Monster = monster;
            Name = "battle0001";
            this.rewards = rewards;
            this.possibleToEscape = possibleToEscape;
            battleScene = scene;
            battleScene.ImgSetup = GetImage();
        }
        protected override void RunContent()
        {
            parentSession.SendText("\nPojedynek!");
            battleScene.SetupDisplay();
            CopyPlayerState();
            // battle
            if (Monster.BattleGreetings != null)
            {
                battleScene.SendColorText(Monster.BattleGreetings, "red");
                battleScene.SendBattleText("");
            }
            if(!possibleToEscape)
            {
                battleScene.SendColorText("Ostrzezenie - z tego pojedynku nie mozna uciec", "yellow");
                battleScene.SendBattleText("");
            }
            battleScene.SetSkills(parentSession.currentPlayer.ListAvailableSkills(possibleToEscape));
            while (Monster.Health > 0) // reminder: there will be a separate mechanism for what happens when Player.Health == 0 
            {
                if(parentSession.currentPlayer.ListAvailableSkills(possibleToEscape).Count == 0) // player has run out of stamina
                {
                    RestorePlayerState();
                    battleScene.SendColorText("Zadne dalsze ruchy nie sa dostepne - porazka! (Nacisnij dowolny klawisz, aby kontynuowac)", "red");
                    parentSession.GetKeyResponse();
                    parentSession.SendText("Zadne dalsze ruchy nie sa dostepne - porazka!");
                    battleScene.EndDisplay();
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
                    return;
                }
                firstBlood = true;
                List<StatPackage> playerAttack = playerResponse.BattleMove(parentSession.currentPlayer);
                List<StatPackage> memorizedAttack = new List<StatPackage>();
                foreach (StatPackage pack in playerAttack) memorizedAttack.Add(pack.Copy());
                playerAttack = parentSession.ModifyOffensive(playerAttack);
                playerAttack = Monster.React(playerAttack);
                for (int i = 0; i < playerAttack.Count && i < memorizedAttack.Count; i++)
                {
                    battleScene.SendColorText(playerAttack[i].CustomText, "green");
                    if (memorizedAttack[i].HealthDmg != playerAttack[i].HealthDmg) battleScene.SendColorText("Specjalne umiejetnosci potwora zatrzymuja " +
                        (memorizedAttack[i].HealthDmg - playerAttack[i].HealthDmg) + " obrazen!", "yellow");
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
                battleScene.SetSkills(parentSession.currentPlayer.ListAvailableSkills(possibleToEscape));
                battleScene.ResetChoice();
                // now monster
                if (Monster.Health == 0) continue;
                battleScene.SendBattleText("");
                List<StatPackage> effectiveAttack = Monster.BattleMove();
                List<StatPackage> monsterAttack = new List<StatPackage>();
                foreach (StatPackage pack in effectiveAttack) monsterAttack.Add(pack.Copy());
                effectiveAttack = parentSession.ModifyDefensive(effectiveAttack);
                effectiveAttack = parentSession.currentPlayer.React(effectiveAttack);
                for (int i = 0; i < monsterAttack.Count && i < effectiveAttack.Count; i++) 
                {
                    battleScene.SendColorText(monsterAttack[i].CustomText, "red");
                    if (monsterAttack[i].HealthDmg != effectiveAttack[i].HealthDmg) battleScene.SendColorText("Twoj pancerz i przedmioty zatrzymuja " +
                        (monsterAttack[i].HealthDmg - effectiveAttack[i].HealthDmg) + " obrazen!", "yellow");
                    if (monsterAttack[i].StrengthDmg != effectiveAttack[i].StrengthDmg) battleScene.SendColorText("Twoj pancerz i przedmioty zatrzymuja " +
                        (monsterAttack[i].StrengthDmg - effectiveAttack[i].StrengthDmg) + " debuffa do sily!", "yellow");
                    if (monsterAttack[i].ArmorDmg != effectiveAttack[i].ArmorDmg) battleScene.SendColorText("Twoj pancerz i przedmioty zatrzymuja " +
                        (monsterAttack[i].ArmorDmg - effectiveAttack[i].ArmorDmg) + " debuffa do pancerza!", "yellow");
                    if (monsterAttack[i].PrecisionDmg != effectiveAttack[i].PrecisionDmg) battleScene.SendColorText("Twoj pancerz i przedmioty zatrzymuja " +
                        (monsterAttack[i].PrecisionDmg - effectiveAttack[i].PrecisionDmg) + " debuffa do precyzji!", "yellow");
                    if (monsterAttack[i].MagicPowerDmg != effectiveAttack[i].MagicPowerDmg) battleScene.SendColorText("Twoj pancerz i przedmioty zatrzymuja " +
                        (monsterAttack[i].MagicPowerDmg - effectiveAttack[i].MagicPowerDmg) + " debuffa do mocy magicznej!", "yellow");
                }
                battleScene.RefreshStats();
                parentSession.RefreshStats();
            }
            // restore player state
            battleResult = true;
            RestorePlayerState();
            battleScene.SendColorText("Zwyciestwo! (Nacisnij dowolny klawisz, aby kontynuowac)", "green");
            parentSession.GetKeyResponse();
            battleScene.EndDisplay();
            parentSession.SendText("Zwyciestwo! Zyskane punkty doswiadczenia: " + Monster.XPValue);
            if(rewards) VictoryReward();
            //parentSession.UpdateStat(7, Monster.XPValue); // for smoother display, this one was moved to GameSession.cs
        }
        protected void CopyPlayerState()
        {
            // not very pretty, but I can't think of another solution that wouldn't make things more complicated
            hpCopy = parentSession.currentPlayer.Health - parentSession.currentPlayer.HealthBuff;
            strCopy = parentSession.currentPlayer.Strength - parentSession.currentPlayer.StrengthBuff;
            armCopy = parentSession.currentPlayer.Armor - parentSession.currentPlayer.ArmorBuff;
            prCopy = parentSession.currentPlayer.Precision - parentSession.currentPlayer.PrecisionBuff;
            mgCopy = parentSession.currentPlayer.MagicPower - parentSession.currentPlayer.MagicPowerBuff;
            staCopy = parentSession.currentPlayer.Stamina - parentSession.currentPlayer.StaminaBuff;
        }
        protected void RestorePlayerState(bool fullHP = true)
        {
            // restore statistics
            if (fullHP)
            {
                parentSession.currentPlayer.Health = hpCopy;
            }
            else
            {
                parentSession.currentPlayer.Health = (int)((parentSession.currentPlayer.Health + hpCopy) / 2);
                if (parentSession.currentPlayer.Health > hpCopy) parentSession.currentPlayer.Health = hpCopy;
                parentSession.currentPlayer.LostHP += hpCopy - parentSession.currentPlayer.Health + parentSession.currentPlayer.BattleBuffHealth;
                if (parentSession.currentPlayer.LostHP < 0) parentSession.currentPlayer.LostHP = 0; //workaround
            }
            parentSession.currentPlayer.Strength = strCopy;
            parentSession.currentPlayer.Armor = armCopy;
            parentSession.currentPlayer.Precision = prCopy;
            parentSession.currentPlayer.MagicPower = mgCopy;
            parentSession.currentPlayer.Stamina = staCopy;
            parentSession.currentPlayer.ResetBattleBuffs();
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
                parentSession.currentPlayer.Gold += gold;
            }
        }

    }
}
