﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Engine.Interactions
{
    // class representing an event (or a chain of events) that will be executed at some map location
    // console interaction means that the player will only use the console for interacting
    class ConsoleInteraction : IInteraction
    {
        protected GameSession parentSession;
        public ConsoleInteraction(GameSession ses)
        {
            parentSession = ses;
        }
        public void Run()
        {
            parentSession.StopMoving();
            RunContent();
            parentSession.StartMoving();
        }

        // content
        protected virtual void RunContent()
        {
            // this is where all events should happen
            // refer to GameSessionPublicLogic for the possible ways of interacting with user and game
        }
    }
}
