using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StealGaze
{
    class StealGazeUtils
    {
        public static Bitmap GetMarkerImage(Marker marker)
        {
            Bitmap img = null;
            switch(marker)
            {
                case Marker.なし:
                    img = Properties.Resources.blank;
                    break;
                case Marker.攻撃1:
                    img = Properties.Resources.attack1;
                    break;
                case Marker.攻撃2:
                    img = Properties.Resources.attack2;
                    break;
                case Marker.攻撃3:
                    img = Properties.Resources.attack3;
                    break;
                case Marker.攻撃4:
                    img = Properties.Resources.attack4;
                    break;
                case Marker.攻撃5:
                    img = Properties.Resources.attack5;
                    break;
                case Marker.足止め1:
                    img = Properties.Resources.bind1;
                    break;
                case Marker.足止め2:
                    img = Properties.Resources.bind2;
                    break;
                case Marker.足止め3:
                    img = Properties.Resources.bind3;
                    break;
                case Marker.禁止1:
                    img = Properties.Resources.forbidden1;
                    break;
                case Marker.禁止2:
                    img = Properties.Resources.forbidden2;
                    break;
                case Marker.汎用サンカク:
                    img = Properties.Resources.triangle;
                    break;
                case Marker.汎用マル:
                    img = Properties.Resources.circle;
                    break;
                case Marker.汎用シカク:
                    img = Properties.Resources.square;
                    break;
                case Marker.汎用バツ:
                    img = Properties.Resources.cross;
                    break;
            }
            return img;
        }
    }
}
