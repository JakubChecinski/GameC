using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Engine.Interactions.InteractionFactories
{
    [Serializable]
    class SkillForgetFactory : InteractionFactory
    {
        public List<Interaction> CreateInteractionsGroup(GameSession ses)
        {
            var ans = new SkillForgetInteraction(ses);
            Index.Interactions.Add(ans);
            return new List<Interaction>() { ans };
        }
    }
}
