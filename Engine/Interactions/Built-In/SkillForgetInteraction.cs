using System;
using System.Collections.Generic;
using Game.Engine.Skills;

namespace Game.Engine.Interactions
{
    // a special interaction used for fogetting skills
    // if you want a clear example of how to write your own interesting interaction, this is probably NOT the right place 
    // see Gymir and Hymir files instead

    [Serializable]
    class SkillForgetInteraction : PlayerInteraction
    {
        public SkillForgetInteraction(GameSession parentSession) : base(parentSession) 
        { 
            Name = "interaction0002";
            Enterable = false;
        }

        protected override void RunContent()
        {
            List<Skill> tmp = parentSession.CurrentPlayer.ListOfSkills;
            List<string> choices = new List<string>();
            foreach (Skill sk in tmp)
            {
                choices.Add("Zapomnij: " + sk.ToString() + ".");
            }
            parentSession.SendText("\nNapis na podstawie fontanny glosi: ta woda sprawia, ze zapomnisz jednego ze swoich zaklec.");
            if (parentSession.CurrentPlayer is CharacterClasses.Mage && choices.Count > 2)
            {
                choices.RemoveAt(0); // remove running away, which is always first on the list
                choices.Add("Chyba jednak poszukam innego miejsca.");
                int a = parentSession.GetListBoxChoice(choices);
                if (a < choices.Count - 1) parentSession.CurrentPlayer.ListOfSkills.RemoveAt(a + 1);
            }
            else if (parentSession.CurrentPlayer is CharacterClasses.Warrior) parentSession.SendText("Nie zajmujesz sie jednak magia, wiec wzruszasz ramionami i pijesz z fontanny do woli.");
            else parentSession.SendText("Obecnie znasz jednak tylko jedno zaklecie - twoj umysl jest prosty i czysty, wiec woda z fontanny nie ma nad toba wladzy.");
        }
    }
}
