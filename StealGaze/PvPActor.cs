using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StealGaze
{
    enum Marker { 攻撃1, 攻撃2, 攻撃3, 攻撃4, 攻撃5, 足止め1, 足止め2, 足止め3, 禁止1, 禁止2, 汎用シカク, 汎用マル, 汎用バツ, 汎用サンカク, なし }

    class PvPActor
    {
        public string name {get; set;}
        public List<String> jobList { get; set; }
        public bool isPTMember {get; set;}
        public Marker marker { get; set; }

        public bool isTank { get; set; }
        public bool isMeleeDPS { get; set; }
        public bool isRangedDPS { get; set; }
        public bool isHealer { get; set; }

        public PvPActor(string name)
        {
            this.name = name;
            this.jobList = new List<String>();
            this.isPTMember = false;
            this.marker = Marker.なし;
        }

        public void SetJobList(List<String> jobs)
        {
            var intersected = new List<string>();
            foreach(string job in jobs)
            {
                if(this.jobList.Contains(job)){
                    intersected.Add(job);
                }
            }
            this.jobList = intersected.Count > 0 ? intersected : jobs;
            this.jobList = this.jobList.Distinct().ToList<string>();

            if(this.jobList.Count == 2 && this.jobList.Contains("巴")
                && (this.jobList.Contains("召") || this.jobList.Contains("学")))
            {
                this.jobList.Remove("巴");
            }

            if(this.jobList.Count == 1 && this.jobList[0] != "巴"){
                isTank = false;
                isMeleeDPS = false;
                isRangedDPS = false;
                isHealer = false;
                var job = this.jobList[0];
                switch(job)
                {
                    case "ナ":
                    case "戦":
                        isTank = true;
                        break;
                    case "モ":
                    case "竜":
                        isMeleeDPS = true;
                        break;
                    case "詩":
                    case "黒":
                    case "召":
                        isRangedDPS = true;
                        break;
                    case "幻":
                    case "白":
                    case "学":
                        isHealer = true;
                        break;
                }
            } else if(jobList.Count == 2){
                if(jobList.Contains("ナ") && jobList.Contains("戦"))
                {
                    isTank = true;
                }
                else if(jobList.Contains("モ") && jobList.Contains("竜"))
                {
                    isMeleeDPS = true;
                }
                else if(jobList.Contains("白") && jobList.Contains("学"))
                {
                    isHealer = true;
                }
            }
        }

        public static List<string> ParseJob(string classJob){
            var jobArray = classJob.Split(' ');
            var newJobList = new List<string>();

            foreach(string job in jobArray){
                switch(job){
                    case "剣術士":
                    case "剣":
                    case "ナイト":
                    case "ナ":
                        newJobList.Add("ナ");
                        break;
                    case "斧術士":
                    case "斧":
                    case "戦士":
                    case "戦":
                        newJobList.Add("戦");
                        break;
                    case "格闘士":
                    case "格":
                    case "モンク":
                    case "モ":
                        newJobList.Add("モ");
                        break;
                    case "槍術士":
                    case "槍":
                    case "竜騎士":
                    case "竜":
                        newJobList.Add("竜");
                        break;
                    case "弓術士":
                    case "弓":
                    case "吟遊詩人":
                    case "詩":
                        newJobList.Add("詩");
                        break;
                    case "呪術士":
                    case "呪":
                    case "黒魔道士":
                    case "黒":
                        newJobList.Add("黒");
                        break;
                    case "幻術士":
                    case "幻":
                    case "白魔道士":
                    case "白":
                        newJobList.Add("白");
                        break;
                    case "巴術士":
                    case "巴":
                        newJobList.Add("巴");
                        break;
                    case "召喚士":
                    case "召":
                        newJobList.Add("召");
                        break;
                    case "学者":
                    case "学":
                        newJobList.Add("学");
                        break;
                }
            }
            return newJobList;
        }

        public static Marker GetMarkerEnum(string marker)
        {
            Marker markerEnum = Marker.なし;
            switch(marker)
            {
                case "攻撃1":
                    markerEnum = Marker.攻撃1;
                    break;
                case "攻撃2":
                    markerEnum = Marker.攻撃2;
                    break;
                case "攻撃3":
                    markerEnum = Marker.攻撃3;
                    break;
                case "攻撃4":
                    markerEnum = Marker.攻撃4;
                    break;
                case "攻撃5":
                    markerEnum = Marker.攻撃5;
                    break;
                case "足止め1":
                    markerEnum = Marker.足止め1;
                    break;
                case "足止め2":
                    markerEnum = Marker.足止め2;
                    break;
                case "足止め3":
                    markerEnum = Marker.足止め3;
                    break;
                case "禁止1":
                    markerEnum = Marker.禁止1;
                    break;
                case "禁止2":
                    markerEnum = Marker.禁止2;
                    break;
                case "汎用シカク":
                    markerEnum = Marker.汎用シカク;
                    break;
                case "汎用マル":
                    markerEnum = Marker.汎用マル;
                    break;
                case "汎用バツ":
                    markerEnum = Marker.汎用バツ;
                    break;
                case "汎用サンカク":
                    markerEnum = Marker.汎用サンカク;
                    break;
            }
            return markerEnum;
        }

        public Bitmap GetRoleImage()
        {
            Bitmap img = null;
            if(isTank)
            {
                img = Properties.Resources.tank;
            }
            else if(isMeleeDPS || isRangedDPS)
            {
                img = Properties.Resources.dps;
            }
            else if(isHealer)
            {
                img = Properties.Resources.healer;
            }
            else
            {
                img = Properties.Resources.blank;
            }
            return img;
        }

        public Bitmap GetJobImage()
        {
            Bitmap img;
            if(jobList.Count == 1)
            {
                switch(jobList[0])
                {
                    case "ナ":
                        img = Properties.Resources.knight;
                        break;
                    case "戦":
                        img = Properties.Resources.warrior;
                        break;
                    case "モ":
                        img = Properties.Resources.monk;
                        break;
                    case "竜":
                        img = Properties.Resources.dragoon;
                        break;
                    case "詩":
                        img = Properties.Resources.bird;
                        break;
                    case "黒":
                        img = Properties.Resources.blackmage;
                        break;
                    case "白":
                        img = Properties.Resources.whitemage;
                        break;
                    case "召":
                        img = Properties.Resources.summoner;
                        break;
                    case "学":
                        img = Properties.Resources.scholar;
                        break;
                    default:
                        img = Properties.Resources.blank;
                        break;
                }
            }
            else
            {
                img = Properties.Resources.blank;
            }
            return img;
        }

        public Bitmap GetMarkerImage()
        {
            Bitmap markerBmp = null;
            switch(this.marker)
            {
                case Marker.なし:
                    markerBmp = Properties.Resources.blank;
                    break;
                case Marker.攻撃1:
                    markerBmp = Properties.Resources.attack1;
                    break;
                case Marker.攻撃2:
                    markerBmp = Properties.Resources.attack2;
                    break;
                case Marker.攻撃3:
                    markerBmp = Properties.Resources.attack3;
                    break;
                case Marker.攻撃4:
                    markerBmp = Properties.Resources.attack4;
                    break;
                case Marker.攻撃5:
                    markerBmp = Properties.Resources.attack5;
                    break;
                case Marker.足止め1:
                    markerBmp = Properties.Resources.bind1;
                    break;
                case Marker.足止め2:
                    markerBmp = Properties.Resources.bind2;
                    break;
                case Marker.足止め3:
                    markerBmp = Properties.Resources.bind3;
                    break;
                case Marker.禁止1:
                    markerBmp = Properties.Resources.forbidden1;
                    break;
                case Marker.禁止2:
                    markerBmp = Properties.Resources.forbidden2;
                    break;
                case Marker.汎用サンカク:
                    markerBmp = Properties.Resources.triangle;
                    break;
                case Marker.汎用マル:
                    markerBmp = Properties.Resources.circle;
                    break;
                case Marker.汎用シカク:
                    markerBmp = Properties.Resources.square;
                    break;
                case Marker.汎用バツ:
                    markerBmp = Properties.Resources.cross;
                    break;
            }
            return markerBmp;
        }
    }
}
