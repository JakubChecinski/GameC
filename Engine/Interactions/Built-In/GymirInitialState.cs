﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Engine.Interactions.Built_In
{
    [Serializable]
    class GymirInitialState : GymirState
    {
        private int payment = 0;
        public override void RunContent(GameSession parentSession, GymirEncounter myself, HymirEncounter myBrother)
        {
            parentSession.SendText("\nHello adventurer. My name is Gymir. Could you help me chop this wood?");
            // get player choice
            int choice = parentSession.GetListBoxChoice(new List<string>() { "Sure, no problem!", "Everything comes with a price, you know.", "Do I look like a lumberjack to you?" });
            switch (choice)
            {
                case 0:
                    payment = 0;
                    ChopWood(parentSession, myself, myBrother);
                    break;
                case 1:
                    parentSession.SendText("Come on, I don't have much money... is 15 gold enough?");
                    int choice2 = parentSession.GetListBoxChoice(new List<string>() { "*Sighs* Fine.", "Sorry, that's not enough." });
                    if (choice2 == 0)
                    {
                        payment = 15;
                        ChopWood(parentSession, myself, myBrother);
                    }
                    else parentSession.SendText("People these days... go away then!");
                    break;
                default:
                    parentSession.SendText("No, you look like a useless vagabond. Go away!");
                    break;
            }
        }

        private void ChopWood(GameSession parentSession, GymirEncounter myself, HymirEncounter myBrother)
        {
            parentSession.SendText("Great! You can use my axe over there.");
            int choice = parentSession.GetListBoxChoice(new List<string>() { "Spend the next hour chopping wood", "Wait until he goes back to his home and run away with the axe" });
            if (choice == 0)
            {
                if (payment == 0)
                {
                    parentSession.SendText("Thank you so much for your help! You should meet my brother Hymir, he is a really nice person just like you.");
                    myBrother.Strategy = new HymirFriendlyStrategy(); // Hymir will hear about this and he will like you now
                    myself.ChangeState(new GymirCompleteState(), true); // this interaction is now complete
                }
                else
                {
                    parentSession.SendText("Good. Here is your gold.");
                    parentSession.UpdateStat(8, payment); // +15 gold
                    myself.ChangeState(new GymirNoMoneyState()); // this interaction is still not complete, but the player can return here
                }    
            }
            else
            {
                parentSession.SendText("Wait, what are you doing? COME BACK HERE!");
                parentSession.AddThisItem(Index.ProduceSpecificItem("item0009")); //gymir's axe
                myBrother.Strategy = new HymirHostileStrategy(); // Hymir will hear about this and he will hate you now
                myself.ChangeState(new GymirHostileState(), true); // this interaction is now complete, but Gymir will no longer let you work here
            }
        }

    }
}
