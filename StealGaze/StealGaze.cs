using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FFXIV_Tools;

namespace StealGaze
{
    public partial class StealGaze: Form
    {
        FFXIVLogMemoryInfo logmemoryInfo;
        FF14LogParser logParser;

        Dictionary<string, PvPActor> actors = new Dictionary<string, PvPActor>();
        Dictionary<string, PvPActor> actorsLog = new Dictionary<string, PvPActor>();

        DAndDSizeChanger dAndDSizeChanger;
        Point lastMousePoint;

        bool OnMouse
        {
            get
            {
                return this.Bounds.Contains(Cursor.Position);
            }
        }

        //public string LogFolder
        //{
        //    get
        //    {
        //        return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");
        //    }
        //}

        int logCount;

        public StealGaze()
        {
            InitializeComponent();
            logParser = new FF14LogParser();
        }

        private void logReader_Tick(object sender, EventArgs e)
        {
            logReader.Stop();
            foreach(byte[] logdata in logmemoryInfo.GetNewLogsData())
            {
                FFXIVLog log = FFXIVLog.ParseSingleLog(logdata);
                FFXIVLogDataSet.AnaylzedRow row = logParser.Add(log);

                var msg = log.LogBodyReplaceTabCode;
                SetStatus(msg.StartsWith(":") ? msg.Substring(1) : msg);

                var regex = new Regex(@"^:(?<name>[A-Z][a-z']+? [A-Z][a-z']+?)$");
                var match = regex.Match(log.LogBodyReplaceTabCode);
                if(match.Success)
                {
                    string name = match.Groups["name"].Value;
                    if(actors.Keys.Contains(name))
                    {
                        if(actors[name].jobList.Count == 1)
                        {
                            targetImage.Image = actors[name].GetJobImage();
                        }
                        else
                        {
                            targetImage.Image = actors[name].GetRoleImage();
                        }
                    }
                }

                InitializeCurrentActors(row);
                Mark(log);
                RejectPTMember(log);

                ParseActorFrom(row);
                ParseActorTo(row);

                Display();
            }

            if(logCount < logParser.ds.Anaylzed.Count - 100)
            {
                logCount = logParser.ds.Anaylzed.Count;
            }
            logReader.Start();
        }

        private void InitializeCurrentActors(FFXIVLogDataSet.AnaylzedRow row)
        {
            if(row.IsPvPStart && !row.IsPvPEnd)
            {
                SetStatus("バトル開始 - リストを初期化しました");
                foreach(string key in actors.Keys)
                {
                    actorsLog[key] = actors[key];
                }
                actors = new Dictionary<string, PvPActor>();
                targetImage.Image = Properties.Resources.blank;

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

                        foreach(string key in actorsStatusChanged.Keys){
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

        private void ParseActorFrom(FFXIVLogDataSet.AnaylzedRow row)
        {
            if(!row.IsFromNull())
            {
                var actor = logParser.ds.Actor.FindByName(row.From);
                if(actor != null && !FF14LogParser.IsPet(actor.Name))
                {
                    Regex regex = new Regex(@"(?<name>[A-Z][a-z']+? [A-Z][a-z']+?)");
                    Match match = regex.Match(actor.Name);
                    if(!match.Success)
                    {
                        return;
                    }
                    var pvpActor = FindOrCreatePvPActor(actor.Name);
                    if(pvpActor.jobList.Count == 0 && actorsLog.Keys.Contains(actor.Name))
                    {
                        pvpActor.jobList = actorsLog[actor.Name].jobList;
                    }

                    if(!actor.IsClassJobNull())
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
                    actors[actor.Name] = pvpActor;
                }
            }
        }

        private void ParseActorTo(FFXIVLogDataSet.AnaylzedRow row)
        {
            if(!row.IsToNull())
            {
                var actor = logParser.ds.Actor.FindByName(row.To);
                if(actor != null && !FF14LogParser.IsPet(actor.Name) && !actors.Keys.Contains(actor.Name))
                {
                    Regex regex = new Regex(@"(?<name>[A-Z][a-z']+? [A-Z][a-z']+?)");
                    Match match = regex.Match(actor.Name);
                    if(!match.Success)
                    {
                        return;
                    }
                    var pvpActor = FindOrCreatePvPActor(actor.Name);
                    if(pvpActor.jobList.Count == 0 && actorsLog.Keys.Contains(actor.Name))
                    {
                        pvpActor.jobList = actorsLog[actor.Name].jobList;
                    }
                    actors[actor.Name] = pvpActor;
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

        private void logFinder_DoWork(object sender, DoWorkEventArgs e)
        {
            while(logmemoryInfo == null && !logFinder.CancellationPending)
            {
                logmemoryInfo = FFXIVLogMemoryInfo.Create();
                if(logmemoryInfo == null)
                {
                    SetStatus(String.Format("ログイン確認中..."));
                    SetProgress(0, false);
                    for(int i = 10; i >= 0 && !logFinder.CancellationPending; i--)
                    {
                        System.Threading.Thread.Sleep(10000);
                        //SetStatus(String.Format("プロセス確認中...", i));
                    }
                    continue;
                }
                //SetStatus("ログイン確認中...");
                SetProgress(0, false);
                bool success = false;
                System.Threading.ThreadStart action = () =>
                {
                    success = logmemoryInfo.SearchLogMemoryInfo();
                };
                System.Threading.Thread th = new System.Threading.Thread(action);
                th.Start();
                while(th.IsAlive)
                {
                    if(logFinder.CancellationPending)
                    {//キャンセルされた
                        SetStatus("停止中...");
                        logmemoryInfo.CancelSearching();
                        th.Join();
                        logmemoryInfo = null;
                        SetProgress(0, false);
                        return;
                    }
                    SetProgress(logmemoryInfo.Progress, true);
                    System.Threading.Thread.Sleep(100);
                }

                SetProgress(0, false);
                //ログのサーチに成功したか
                if(!success)
                {
                    logmemoryInfo = null;
                    for(int i = 10; i >= 0 && !logFinder.CancellationPending; i--)
                    {
                        System.Threading.Thread.Sleep(10000);
                        SetStatus("ログイン確認中...");
                    }
                }
                else
                {
                    InitializeData();
                }
            }
        }

        private void logFinder_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(logmemoryInfo == null)
            {
                SetStatus("キャンセルしました");
            }
            else
            {
                logReader.Start();
            }
        }

        private void StealGaze_Load(object sender, EventArgs e)
        {
            //バージョン情報
            //SetVersionInfo();
            dAndDSizeChanger = new DAndDSizeChanger(this, this, DAndDArea.All, 4);

            // ログフォルダ
            //if(!System.IO.Directory.Exists(LogFolder))
            //{
            //    System.IO.Directory.CreateDirectory(LogFolder);
            //}

            //LoadSettings();

            SetStatus("ログ読込中...");

            dataGridView.Rows.Clear();

            // TODO:
            this.TopMost = true;
            this.Size = new Size(this.Size.Width, 250);
            dataGridView.Size = new Size(210, 193);

            logFinder.RunWorkerAsync();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (NullReferenceException)
            {
            }
            catch(Exception)
            {
            }
        }

        private void StealGaze_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if(logFinder.IsBusy)
                {
                    logFinder.CancelAsync();
                }
                logReader.Stop();
            }
            catch(Exception _e)
            {
                MessageBox.Show("ログファイルの保存に失敗しました。" + _e.Message);
            }
        }

        private void StealGaze_MouseMove(object sender, MouseEventArgs e)
        {
            if((e.Button & MouseButtons.Left) == System.Windows.Forms.MouseButtons.Left && dAndDSizeChanger.Status == DAndDArea.None)
            {
                this.Location += (Size)e.Location - (Size)lastMousePoint;
            }
        }

        private void StealGaze_MouseDown(object sender, MouseEventArgs e)
        {
            lastMousePoint = e.Location;
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            SetStatus("リストを初期化しました");
            foreach(string key in actors.Keys)
            {
                actorsLog[key] = actors[key];
            }
            actors = new Dictionary<string, PvPActor>();
            targetImage.Image = Properties.Resources.blank;

            dataGridView.Rows.Clear();
        }
    }
}
