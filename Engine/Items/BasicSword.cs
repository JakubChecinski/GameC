﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Engine.Items
{
    class BasicSword : Sword
    {
        // simple sword
        public BasicSword() : base("item0004") 
        {
            strMod = 5;
            prMod = 2;
            GoldValue = 10;
            PublicName = "Basic Sword"; 
        }
    }
}
