using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Game.Engine
{
    // this class would be better designed as an interface
    // but it would break compatibility with C# 7.3 and earlier versions so I decided to use an abstract class
    [Serializable]
    public abstract class DisplayItem
    {
        public string Name { get; protected set;  }
        public virtual Image GetImage()
        {
            Image img = new Image();
            img.Source = new BitmapImage(new Uri(("Assets/" + Name + ".png"), UriKind.Relative));
            img.Name = Name;
            return img;
        }
        public virtual BitmapImage GetImageSource()
        {
            return new BitmapImage(new Uri(("Assets/" + Name + ".png"), UriKind.Relative));
        }

    }
}
