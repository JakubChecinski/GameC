using System;
using System.Collections.Generic;

namespace Game.Engine.Skills
{
    // utility class for spells
    [Serializable]
    public abstract class Spell : Skill
    {
        public string SpecialItem { get; protected set; } = "none";
        protected Spell(string name, int stamina, int minLevel) : base(name, stamina, minLevel) 
        {
            ReqItem = RequiredItem.Staff;
        }
    }
}
