﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Engine.Items
{
    class BasicAxe : Axe
    {
        // simple axe
        public BasicAxe() : base("item0003")
        {
            strMod = 10;
            GoldValue = 10;
            PublicName = "Basic Axe"; 
        }
    }
}
