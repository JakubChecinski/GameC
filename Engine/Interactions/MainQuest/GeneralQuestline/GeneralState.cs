using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Engine.Interactions.GeneralQuestline
{
    [Serializable]
    abstract class GeneralState 
    {
        // trzeba podawa� jako argument interakcje �eby p�niej m�c sprawdzi� np. czy s� sko�czone
        public abstract void RunContent(GameSession ses, GeneralEncounter myself, TrainingEncounter training, CampEncounter camp, LibrarianEncounter librarian, TreantEncounter treant);
    }
} 
