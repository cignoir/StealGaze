using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ffxivlib;

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

        public Bitmap GetRoleImage(JOB job)
        {
            Bitmap img;
            switch(job)
            {
                case JOB.GLD:
                case JOB.MRD:
                case JOB.PLD:
                case JOB.WAR:
                    img = Properties.Resources.tank;
                    break;
                case JOB.PGL:
                case JOB.MNK:
                case JOB.LNC:
                case JOB.DRG:
                case JOB.ARC:
                case JOB.BRD:
                case JOB.THM:
                case JOB.BLM:
                case JOB.ACN:
                case JOB.SMN:
                    img = Properties.Resources.dps;
                    break;
                case JOB.CNJ:
                case JOB.WHM:
                case JOB.SCH:
                    img = Properties.Resources.healer;
                    break;
                default:
                    img = Properties.Resources.blank;
                    break;
            }
            return img;
        }

        public static Bitmap GetJobImage(JOB job)
        {
            Bitmap img;
            switch(job)
            {
                case JOB.GLD:
                case JOB.PLD:
                    img = Properties.Resources.knight;
                    break;
                case JOB.MRD:
                case JOB.WAR:
                    img = Properties.Resources.warrior;
                    break;
                case JOB.PGL:
                case JOB.MNK:
                    img = Properties.Resources.monk;
                    break;
                case JOB.LNC:
                case JOB.DRG:
                    img = Properties.Resources.dragoon;
                    break;
                case JOB.ARC:
                case JOB.BRD:
                    img = Properties.Resources.bird;
                    break;
                case JOB.CNJ:
                case JOB.WHM:
                    img = Properties.Resources.whitemage;
                    break;
                case JOB.THM:
                case JOB.BLM:
                    img = Properties.Resources.blackmage;
                    break;
                case JOB.ACN:
                case JOB.SMN:
                    img = Properties.Resources.summoner;
                    break;
                case JOB.SCH:
                    img = Properties.Resources.scholar;
                    break;
                default:
                    img = Properties.Resources.blank;
                    break;
            }
            return img;
        }

        public static string GetJobShortNameJP(JOB job)
        {
            string jobName = string.Empty;
            switch(job)
            {
                case JOB.GLD:
                    jobName = "剣";
                    break;
                case JOB.PLD:
                    jobName = "ナ";
                    break;
                case JOB.MRD:
                    jobName = "斧";
                    break;
                case JOB.WAR:
                    jobName = "戦";
                    break;
                case JOB.PGL:
                    jobName = "格";
                    break;
                case JOB.MNK:
                    jobName = "モ";
                    break;
                case JOB.LNC:
                    jobName = "槍";
                    break;
                case JOB.DRG:
                    jobName = "竜";
                    break;
                case JOB.ARC:
                    jobName = "弓";
                    break;
                case JOB.BRD:
                    jobName = "詩";
                    break;
                case JOB.CNJ:
                    jobName = "幻";
                    break;
                case JOB.WHM:
                    jobName = "白";
                    break;
                case JOB.THM:
                    jobName = "呪";
                    break;
                case JOB.BLM:
                    jobName = "黒";
                    break;
                case JOB.ACN:
                    jobName = "巴";
                    break;
                case JOB.SMN:
                    jobName = "召";
                    break;
                case JOB.SCH:
                    jobName = "学";
                    break;
            }
            return jobName;
        }
    }
}
