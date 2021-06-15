using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Engine.Interactions
{
    // a special interaction used for healing 
    // if you want a clear example of how to write your own interesting interaction, this is probably NOT the right place 
    // see Gymir and Hymir files instead

    [Serializable]
    class HealInteraction : PlayerInteraction
    {
        public HealInteraction(GameSession parentSession) : base(parentSession)
        {
            Name = "interaction0005";
            Enterable = false;
        }

        protected override void RunContent()
        {
            int hpToHeal = parentSession.CurrentPlayer.LostHP;
            if(hpToHeal == 0)
            {
                parentSession.SendText("\nWitaj. Jestem lekarzem zajmujacym sie chorymi i rannymi... ty chyba jednak nie potrzebujesz teraz mojej pomocy.");
                return;
            }
            parentSession.SendText("\nWitaj. Jestem lekarzem zajmujacym sie chorymi i rannymi... zobaczmy, co moge dla ciebie zrobic.");
            parentSession.SendText("Hmm... moge uleczyc " + hpToHeal + " punktow twojego zdrowia, ale bedzie to kosztowalo " + 2 * hpToHeal + " sztuk zlota.");
            List<string> choices = new List<string>() { "Zgoda.", "Dziekuje, nie skorzystam." };
            int a = parentSession.GetListBoxChoice(choices);
            if (a == 0)
            {
                if (parentSession.CheckStat(8) >= 2 * hpToHeal)
                {
                    parentSession.UpdateStat(1, hpToHeal);
                    parentSession.UpdateStat(8, -2 * hpToHeal);
                    parentSession.CurrentPlayer.LostHP = 0;
                    parentSession.SendText("Juz po wszystkim! Uwazaj na siebie nastepnym razem.");
                }
                else parentSession.SendText("Wybacz, ale nie masz wystarczajaco duzo zlota. Rozumiesz chyba, gildia lekarzy nie pozwala mi pracowac za darmo.");
            }
            else parentSession.SendText("To nie jest madry wybor, ale jak sobie chcesz.");
        }
    }
}
