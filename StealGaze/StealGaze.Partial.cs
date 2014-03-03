using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using FFXIV_Tools;

namespace StealGaze
{
    enum ActorType { FROM, TO }

    public partial class StealGaze: Form
    {
        private void UpdateTargetImage(FFXIVLog log)
        {
            var regex = new Regex(@"^:(?<name>[A-Z][a-z']+? [A-Z][a-z']+?)$");
            var match = regex.Match(log.LogBodyReplaceTabCode);
            if(match.Success)
            {
                string name = match.Groups["name"].Value;
                if(actors.Keys.Contains(name))
                {
                    targetImage.Image = actors[name].jobList.Count == 1 ? actors[name].GetJobImage() : actors[name].GetRoleImage();
                }
            }
        }

        private void SetStatus(string text)
        {
            if(this.InvokeRequired)
            {
                if(this.Disposing || this.IsDisposed)
                {
                    return;
                }
                Invoke((Action)(() => statusLabel.Text = text));
            }
            else
            {
                statusLabel.Text = text;
            }
        }

        private void SetProgress(int val, bool visible)
        {
            if(this.InvokeRequired)
            {
                Invoke((Action)(() => { progressBar.Visible = visible; progressBar.Value = val; }));
            }
            else
            {
                try
                {
                    progressBar.Visible = visible; progressBar.Value = val;
                }
                catch
                {
                }
            }
        }

        private void InitializeData()
        {
            this.logCount = 0;
            int memoryLogCount = logmemoryInfo.GetLogCount();

            FF14LogParser logParser = new FF14LogParser();

            if(memoryLogCount > 1000)
            {
                SetStatus("ログ読込中...");

                FFXIVUserFolder userfolder = new FFXIVUserFolder();
                CharacterFolder playnow = null;
                foreach(CharacterFolder cf in userfolder.GetCharacterFolders())
                {
                    if(playnow == null) playnow = cf;
                    if(cf.LastPlayTimeFromLogFile > playnow.LastPlayTimeFromLogFile)
                    {
                        playnow = cf;
                    }
                }
                List<FFXIVLog> LogsFromFile = new List<FFXIVLog>();
                memoryLogCount = logmemoryInfo.GetLogCount();
                if(playnow.LastPlayTimeFromLogFile > FFXIVLog.StartDateTime)
                {
                    foreach(FFXIVLog log in FFXIVLogFileReader.GetLogsFromFile(playnow.GeLastWriteLogFile()))
                    {
                        logParser.Add(log);
                    }
                }
                SetStatus("ログ読込中...");
                foreach(byte[] data in logmemoryInfo.GetNewLogsData())
                {
                    logParser.Add(FFXIVLog.ParseSingleLog(data));
                }
                SetStatus("読込完了");
            }
            SetProgress(100, true);
            this.logParser = logParser;
            SetProgress(0, false);
        }

        private void ClearActorList(FFXIVLogDataSet.AnaylzedRow row)
        {
            if(row.IsPvPStart && !row.IsPvPEnd)
            {
                SetStatus("バトル開始 - リストを初期化しました");
                foreach(string key in actors.Keys)
                {
                    actorsLog[key] = actors[key];
                }
                actors = new Dictionary<string, PvPActor>();
                targetImage.Image = Properties.Resources.eye;

                dataGridView.Rows.Clear();
            }

            if(row.IsPvPEnd)
            {
                SetStatus("バトル終了");
            }
        }

        private PvPActor FindOrCreatePvPActor(string name)
        {
            PvPActor pvpActor;

            if(actors.Keys.Contains(name))
            {
                pvpActor = actors[name];
            }
            else
            {
                if(actorsLog.Keys.Contains(name))
                {
                    pvpActor = actorsLog[name];
                    pvpActor.isPTMember = false;
                    pvpActor.marker = Marker.なし;
                }
                else
                {
                    pvpActor = new PvPActor(name);
                }
            }
            return pvpActor;
        }

        private void Mark(FFXIVLog log)
        {
            if(log.LogBodyReplaceTabCode.Contains("設定しました"))
            {
                var regex = new Regex(@".*?が(?<name>[A-Z][a-z']+? [A-Z][a-z']+?)に『(?<marker>.+?)』を設定しました。");
                var match = regex.Match(log.LogBodyReplaceTabCode);
                if(match.Success)
                {
                    string name = match.Groups["name"].Value;
                    string marker = match.Groups["marker"].Value;

                    if(!FF14LogParser.IsPet(name))
                    {
                        var actorsStatusChanged = new Dictionary<string, PvPActor>();
                        foreach(string key in actors.Keys)
                        {
                            var pvpActor = FindOrCreatePvPActor(key);
                            if(!pvpActor.isPTMember)
                            {
                                if(key == name)
                                {
                                    pvpActor.marker = PvPActor.GetMarkerEnum(marker);
                                    actorsStatusChanged[name] = pvpActor;
                                }
                                else if(pvpActor.marker.ToString() == marker)
                                {
                                    pvpActor.marker = Marker.なし;
                                    actorsStatusChanged[name] = pvpActor;
                                }
                            }
                        }

                        foreach(string key in actorsStatusChanged.Keys)
                        {
                            actors[key] = actorsStatusChanged[key];
                        }
                    }
                }
            }
        }

        private void RejectPTMember(FFXIVLog log)
        {
            Regex regex = new Regex(@"(?<name>[A-Z][a-z']+? [A-Z][a-z']+?):.*");
            Match match = regex.Match(log.LogBodyReplaceTabCode);
            if(match.Success)
            {
                string name = match.Groups["name"].Value;
                var pvpActor = FindOrCreatePvPActor(name);
                pvpActor.isPTMember = true;
                actors[name] = pvpActor;
            }
        }

        private void ParseActor(FFXIVLogDataSet.AnaylzedRow row, ActorType type)
        {
            if(!row.IsFromNull())
            {
                var actorName = type == ActorType.FROM ? row.From : row.To;
                var actor = logParser.ds.Actor.FindByName(actorName);
                if(actor != null && !FF14LogParser.IsPet(actorName))
                {
                    Regex regex = new Regex(@"(?<name>[A-Z][a-z']+? [A-Z][a-z']+?)");
                    Match match = regex.Match(actorName);
                    if(!match.Success)
                    {
                        return;
                    }
                    var pvpActor = FindOrCreatePvPActor(actorName);
                    if(pvpActor.jobList.Count == 0 && actorsLog.Keys.Contains(actorName))
                    {
                        pvpActor.jobList = actorsLog[actorName].jobList;
                    }

                    if(type == ActorType.FROM && !actor.IsClassJobNull())
                    {
                        if(!row.IsActionNameNull())
                        {
                            if(row.ActionName == "プロテス" || row.ActionName == "ストンスキン")
                            {
                                pvpActor.isHealer = true;
                                actor.ClassJob = "白魔道士 学者";
                            }
                        }
                        pvpActor.SetJobList(PvPActor.ParseJob(actor.ClassJob));
                    }
                    actors[actorName] = pvpActor;
                }
            }
        }

        private void Display()
        {
            dataGridView.Rows.Clear();
            int currentRowIndex = 0;
            foreach(string name in actors.Keys)
            {
                if(actors[name].isPTMember)
                {
                    continue;
                }
                dataGridView.Rows.Add();
                dataGridView["marker", currentRowIndex].Value = actors[name].GetMarkerImage();
                dataGridView["role", currentRowIndex].Value = actors[name].GetRoleImage();
                dataGridView["job", currentRowIndex].Value = actors[name].GetJobImage();
                dataGridView["name", currentRowIndex].Value = name;
                currentRowIndex++;
            }
        }
    }
}
