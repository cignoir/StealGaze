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

        DnDSizeChanger dAndDSizeChanger;
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

                

                ClearActorList(row);
                Mark(log);
                RejectPTMember(log);

                ParseActor(row, ActorType.FROM);
                ParseActor(row, ActorType.TO);

                Display();
            }

            if(logCount < logParser.ds.Anaylzed.Count - 100)
            {
                logCount = logParser.ds.Anaylzed.Count;
            }
            logReader.Start();
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
            dAndDSizeChanger = new DnDSizeChanger(this, this, DAndDArea.All, 4);

            // ログフォルダ
            //if(!System.IO.Directory.Exists(LogFolder))
            //{
            //    System.IO.Directory.CreateDirectory(LogFolder);
            //}

            //LoadSettings();

            this.TopMost = true;
            SetStatus("ログ読込中...");

            dataGridView.Rows.Clear();


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
            targetImage.Image = Properties.Resources.eye;

            dataGridView.Rows.Clear();
        }

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            lastMousePoint = e.Location;
        }

        private void label1_MouseMove(object sender, MouseEventArgs e)
        {
            if((e.Button & MouseButtons.Left) == System.Windows.Forms.MouseButtons.Left && dAndDSizeChanger.Status == DAndDArea.None)
            {
                this.Location += (Size)e.Location - (Size)lastMousePoint;
            }
        }

        private void targetImage_MouseDown(object sender, MouseEventArgs e)
        {
            lastMousePoint = e.Location;
        }

        private void targetImage_MouseMove(object sender, MouseEventArgs e)
        {
            if((e.Button & MouseButtons.Left) == System.Windows.Forms.MouseButtons.Left && dAndDSizeChanger.Status == DAndDArea.None)
            {
                this.Location += (Size)e.Location - (Size)lastMousePoint;
            }
        }

        private void dataGridView_MouseDown(object sender, MouseEventArgs e)
        {
            lastMousePoint = e.Location;
        }

        private void dataGridView_MouseMove(object sender, MouseEventArgs e)
        {
            if((e.Button & MouseButtons.Left) == System.Windows.Forms.MouseButtons.Left && dAndDSizeChanger.Status == DAndDArea.None)
            {
                this.Location += (Size)e.Location - (Size)lastMousePoint;
            }
        }
    }
}
