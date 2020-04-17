﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Engine.Items.BasicArmor
{
    class AntiMagicArmor : Item
    {
        // extra 30% reduction of magic damage
        public AntiMagicArmor() : base("item0006")
        {
            PublicName = "AntiMagicArmor";
            GoldValue = 40;
            arMod = 20;
        }
        public override StatPackage ModifyDefensive(StatPackage pack, List<string> otherItems)
        {
            if (pack.DamageType == "fire" || pack.DamageType == "water" || pack.DamageType == "air" || pack.DamageType == "earth")
            {
                pack.HealthDmg = 70 * pack.HealthDmg / 100;
            }
            return pack;
        }
    }
}
