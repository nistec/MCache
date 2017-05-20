using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MControl.Util;
using MControl.Data;
using MControl.Data.SqlClient;
using MControl.Charts;
using MControl.GridView;
using MControl.WinForms;
using MControl.Caching.Remote;

namespace MControl.Caching.Remote.UI
{
    public partial class CacheMonitor : McForm
    {
        public CacheMonitor()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //Config();
            LoadCache();
            InitChart();
            this.timer1.Interval = interval;
            this.timer1.Enabled = true;
        }

 
        private void LoadCache()
        {
            this.statusStrip.Text = "";
            lblUsage1.Text = "Usage";
            lblUsage2.Text = "Call Count";

            try
            {

                //ICacheRemote remote = CacheRemoteClient.RemoteClient;
                Statistic = RemoteCacheClient.RemoteClient.GetStatistic();
                //count = remote.Count;
                //freeSize = remote.FreeSize;
                //maxSize = remote.MaxSize;
                //usage = remote.Usage;

                //DataView dv = remote.CacheView;//.CacheDataView();
                this.gridItems.SetDataBinding(Statistic.CacheView, "");
                this.gridSummarize.SetDataBinding(Statistic.ItemsCall, "");
            }
            catch(Exception ex)
            {
                this.statusStrip.Text = ex.Message;
                MControl.WinForms.MsgDlg.OpenMsg(ex.Message, "Statistic.CacheView");
            }

        }

        #region members

        CacheStatistic Statistic;

        //int count;
        //long freeSize;
        //long maxSize;
        //long usage;

        //private bool useChannels = false;

        private int maxUsage = 1000;
        private int interval = 1000;

        //private bool refreshQueueItems = true;
        private bool refreshQueueSummarize = true;
        private int tickInterval = 5;

        private bool doRefresh = false;
        private bool isActivated = true;
        private int intervalCount=0;
        private bool autoScale = false;
        private bool initilaized = false;
        private int fastFirstRows = 100;

        private int meterScaleInterval=100;
        private int meterScaleMax=1000;
        private int ledScaleCount=40;
        private int ledScaleMax=5000;


        #endregion

        #region properties
 
        public bool PropInitilaized
        {
            get 
            {
                //if (string.IsNullOrEmpty(sqlItems) || string.IsNullOrEmpty(sqlBySender) || string.IsNullOrEmpty(sqlValues))
                //    return false;
                return initilaized; 
            }
        }

        public bool PropAutoScale
        {
            get { return autoScale; }
            set { autoScale = value; }
        }

        //public bool PropUseChannels
        //{
        //    get { return useChannels; }
        //    set { useChannels = value; }
        //}

        //public bool PropUseDataReader
        //{
        //    get { return useDataReader; }
        //    set { useDataReader = value; }
        //}

        //public string PropConnectionString
        //{
        //    get { return cnn; }
        //    set { cnn = value; }
        //}

        //public string PropItemsSource
        //{
        //    get { return sqlItems; }
        //    set { sqlItems = value; }
        //}
        //public string PropItemsBySenderSource
        //{
        //    get { return sqlBySender; }
        //    set { sqlBySender = value; }
        //}
        //public string PropItemsSummarizeSource
        //{
        //    get { return sqlValues; }
        //    set { sqlValues = value; }
        //}
        //public string PropChannelsSource
        //{
        //    get { return sqlChannels; }
        //    set { sqlChannels = value; }
        //}

        public int PropInterval //= 1000
        {
            get { return interval; }
            set { if (value >= 1000) { interval = value; } }
        }

        public int PropMaxUsage //= 1000
        {
            get { return maxUsage; }
            set 
            {
                if ((value >= 1) && maxUsage != value)
                {
                    maxUsage = value;
                    //OnUsageChanges();
                }
            }
      }
        public int PropTickInterval //= 50
        {
            get { return tickInterval; }
            set
            {
                if ((value >= 2) && tickInterval != value)
                {
                    tickInterval = value;
                }
            }
        }
       
        public int PropFastFirstRows //= 100
        {
            get { return fastFirstRows; }
            set
            {
                if ((value >= 10) && fastFirstRows != value)
                {
                    fastFirstRows = value;
                }
            }
        }
        //public string PropQueueName1
        //{
        //    get { return QueueName1; }
        //    set { QueueName1 = value; lblMeter1.Text = value; lblUsage1.Text = value; }
        //}
        //public string PropQueueName2
        //{
        //    get { return QueueName2; }
        //    set { QueueName2 = value; lblMeter2.Text = value; lblUsage2.Text = value; }
        //}
        //public string PropQueueName3
        //{
        //    get { return QueueName3; }
        //    set { QueueName3 = value; lblMeter3.Text = value; lblUsage3.Text = value; }
        //}
        //public string PropQueueName4
        //{
        //    get { return QueueName4; }
        //    set { QueueName4 = value; lblMeter4.Text = value; lblUsage4.Text = value; }
        //}

        //public string PropChannelName1
        //{
        //    get { return ChannelName1; }
        //    set { ChannelName1 = value; lblChannel1.Text = value; }
        //}
        //public string PropChannelName2
        //{
        //    get { return ChannelName2; }
        //    set { ChannelName2 = value; lblChannel2.Text = value; }
        //}
        //public string PropChannelName3
        //{
        //    get { return ChannelName3; }
        //    set { ChannelName3 = value; lblChannel3.Text = value; }
        //}
        //public string PropChannelName4
        //{
        //    get { return ChannelName4; }
        //    set { ChannelName4 = value; lblChannel4.Text = value; }
        //}

        public int PropMeterScaleInterval
        {
            get { return meterScaleInterval; }
            set 
            {
                if (meterScaleInterval != value)
                {
                    meterScaleInterval = value;
                    //OnMeterChanges();
                }
            }
        }

        public int PropMeterScaleMax
        {
            get { return meterScaleMax; }
            set 
            {
                if (meterScaleMax != value)
                {
                    meterScaleMax = value;
                    //OnMeterChanges();
                }
            }
        }

        public int PropLedScaleCount
        {
            get { return ledScaleCount; }
            set 
            {
                if (ledScaleCount != value)
                {
                    ledScaleCount = value;
                    OnLedChanges();
                }
            }
       }

        public int PropLedScaleMax
        {
            get { return ledScaleMax; }
            set 
            {
                if (ledScaleMax != value)
                {
                    ledScaleMax = value;
                    OnLedChanges();
                }
            }
      }

        

        #endregion

        #region override

        protected virtual void SetGridItems()
        {
            //itemsSource = dt;
            if (this.gridItems.DataSource == null)
            {
                this.gridItems.Init(Statistic.CacheView, "", "RemoteCacheItems");
            }
            else
            {
                this.gridItems.ReBinding(Statistic.CacheView);
            }
            refreshQueueItems = false;
        }

        protected virtual void SetGridSummarise()
        {
            //summarizeSource = dt;
            //if (this.gridSummarize.DataSource == null)
            //{
            //    //this.gridSummarize.Init(summarizeSource, "", "QueueItemsBySender");
            //}
            //else
            //{
            //    //this.gridSummarize.ReBinding(summarizeSource);
            //}
            this.gridSummarize.SetDataBinding(Statistic.ItemsCall, "");
            refreshQueueSummarize = false;
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            isActivated = false;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            isActivated = true;
        }

        protected override void OnAsyncExecutingWorker(MControl.Threading.AsyncCallEventArgs e)
        {
            base.OnAsyncExecutingWorker(e);
            try
            {
                Statistic = RemoteCacheClient.RemoteClient.GetStatistic();

            }
            catch { }
            finally
            {
                //doRefresh = true;
            }

        }

        protected override void OnAsyncCompleted(MControl.Threading.AsyncCallEventArgs e)
        {
            base.OnAsyncCompleted(e);

            try
            {
                SetGridItems();
                if (refreshQueueSummarize)
                {
                    SetGridSummarise();
                }
                //FillQueueValues();
                //FillChannelValues();
                FillControls();
            }
            catch(Exception ex) 
            {
                string s = ex.Message;
            }
            finally
            {
                base.AsyncDispose();
            }


        }

        #endregion

        #region private methods

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                intervalCount++;
                doRefresh = intervalCount >= tickInterval;
                //if (refreshQueueItems)
                //{
                //    AsyncDalStart();
                //    refreshQueueItems = false;
                //}
                if (isActivated)
                {
                    if (doRefresh)
                    {
                        this.statusStrip.Text = "";
                        base.AsyncBeginInvoke(null);
                        intervalCount = 0;
                    }
                    else
                    {
                        FillUsageControls();
                    }
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        private void FillControls()
        {

            if (autoScale)
            {
                //int valInterval = meterScaleInterval;
                //if (QueueItem1Count > 0)
                //{
                //    valInterval = QueueItem1Count / 10;
                //    ctlMeter1.SetAutoScale(QueueItem1Count, Math.Max(meterScaleMax, QueueItem1Count), Math.Max(meterScaleInterval, valInterval));
                //}
                //if (QueueItem2Count > 0)
                //{
                //    valInterval = QueueItem2Count / 10;
                //    ctlMeter2.SetAutoScale(QueueItem2Count, Math.Max(meterScaleMax, QueueItem2Count), Math.Max(meterScaleInterval, valInterval));
                //}
                //if (QueueItem3Count > 0)
                //{
                //    valInterval = QueueItem3Count / 10;
                //    ctlMeter3.SetAutoScale(QueueItem3Count, Math.Max(meterScaleMax, QueueItem3Count), Math.Max(meterScaleInterval, valInterval));
                //}
                //if (QueueItem4Count > 0)
                //{
                //    valInterval = QueueItem4Count / 10;
                //    ctlMeter4.SetAutoScale(QueueItem4Count, Math.Max(meterScaleMax, QueueItem4Count), Math.Max(meterScaleInterval, valInterval));
                //}

                //ctlLedAll.ScaleValue = QueueItemTotalCount;
            }
            else
            {
                this.ctlMeter1.ScaleValue = Statistic.UsageDefault;
                this.ctlMeter2.ScaleValue = Statistic.UsageData;
                this.ctlMeter3.ScaleValue = Statistic.UsageImages;
                this.ctlMeter4.ScaleValue = Statistic.UsageFiles;

            }

            //if (QueueItemTotalCount > maxUsage)
            //    OnUsageChanges();
            this.ctlUsage1.Value1 = (int)Statistic.FreeSize;
            this.ctlUsage1.Value2 = (int)Statistic.Usage;
            this.ctlUsage2.Value1 = (int)Statistic.CallCount;
            this.ctlUsage2.Value2 = 0;

            this.ctlUsageHistory1.AddValues((int)Statistic.FreeSize, (int)Statistic.Usage);
            this.ctlUsageHistory2.AddValues(Statistic.CallCount, 0);


            //this.ctlLedAll.ScaleValue = QueueItemTotalCount;

            this.ctlLedChannel1.ScaleValue = Statistic.CountDefault;
            this.ctlLedChannel2.ScaleValue = Statistic.CountData;
            this.ctlLedChannel3.ScaleValue = Statistic.CountImages;
            this.ctlLedChannel4.ScaleValue = Statistic.CountFiles;


            ctlPieChart1.Items[0].Weight = (double)Statistic.Usage;
            ctlPieChart1.Items[1].Weight = (double)Statistic.FreeSize;
            //ctlPieChart1.Items[2].Weight = (double)QueueItem3Count;
            //ctlPieChart1.Items[3].Weight = (double)QueueItem4Count;

            ctlPieChart1.Items[0].ToolTipText = "Usage:" + Statistic.Usage.ToString();
            ctlPieChart1.Items[1].ToolTipText = "Free Size:" + Statistic.FreeSize.ToString();
            //ctlPieChart1.Items[2].ToolTipText = QueueName3 + ":" + QueueItem3Count.ToString();
            //ctlPieChart1.Items[3].ToolTipText = QueueName4 + ":" + QueueItem4Count.ToString();

            ctlPieChart1.Items[0].PanelText = "Usage:" + Statistic.Usage.ToString();
            ctlPieChart1.Items[1].PanelText = "Free Size:" + Statistic.FreeSize.ToString();
            //ctlPieChart1.Items[2].PanelText = QueueName3 + ":" + QueueItem3Count.ToString();
            //ctlPieChart1.Items[3].PanelText = QueueName4 + ":" + QueueItem4Count.ToString();

             ctlPieChart1.AddChartDescription();


             this.lblUsageValue1.Text = Statistic.Usage.ToString();
             this.lblUsageValue2.Text = Statistic.CallCount.ToString();
             
             this.lblMeterValue1.Text = Statistic.UsageDefault.ToString();
             this.lblMeterValue2.Text = Statistic.UsageData.ToString();
             this.lblMeterValue3.Text = Statistic.UsageImages.ToString();
             this.lblMeterValue4.Text = Statistic.UsageFiles.ToString();
            
             this.lblLedValue1.Text = Statistic.CountDefault.ToString();
             this.lblLedValue2.Text = Statistic.CountData.ToString();
             this.lblLedValue3.Text = Statistic.CountImages.ToString();
             this.lblLedValue4.Text = Statistic.CountFiles.ToString();
            //this.lblTotalQueue.Text = QueueItemTotalCount.ToString();
        }

        private void FillMeterControls()
        {

            if (autoScale)
            {
                //int valInterval = meterScaleInterval;
                //if (QueueItem1Count > 0)
                //{
                //    valInterval = QueueItem1Count / 10;
                //    ctlMeter1.SetAutoScale(QueueItem1Count, Math.Max(meterScaleMax, QueueItem1Count), Math.Max(meterScaleInterval, valInterval));
                //}
                //if (QueueItem2Count > 0)
                //{
                //    valInterval = QueueItem2Count / 10;
                //    ctlMeter2.SetAutoScale(QueueItem2Count, Math.Max(meterScaleMax, QueueItem2Count), Math.Max(meterScaleInterval, valInterval));
                //}
                //if (QueueItem3Count > 0)
                //{
                //    valInterval = QueueItem3Count / 10;
                //    ctlMeter3.SetAutoScale(QueueItem3Count, Math.Max(meterScaleMax, QueueItem3Count), Math.Max(meterScaleInterval, valInterval));
                //}
                //if (QueueItem4Count > 0)
                //{
                //    valInterval = QueueItem4Count / 10;
                //    ctlMeter4.SetAutoScale(QueueItem4Count, Math.Max(meterScaleMax, QueueItem4Count), Math.Max(meterScaleInterval, valInterval));
                //}

                //ctlLedAll.ScaleValue = QueueItemTotalCount;
            }
            else
            {
                this.ctlMeter1.ScaleValue = Statistic.UsageDefault;
                this.ctlMeter2.ScaleValue = Statistic.UsageData;
                this.ctlMeter3.ScaleValue = Statistic.UsageImages;
                this.ctlMeter4.ScaleValue = Statistic.UsageFiles;

            }

            this.lblMeterValue1.Text = Statistic.UsageDefault.ToString();
            this.lblMeterValue2.Text = Statistic.UsageData.ToString();
            this.lblMeterValue3.Text = Statistic.UsageImages.ToString();
            this.lblMeterValue4.Text = Statistic.UsageFiles.ToString();
            //this.lblTotalQueue.Text = QueueItemTotalCount.ToString();
        }

        private void FillUsageControls()
        {

            //if (QueueItemTotalCount > maxUsage)
            //    OnUsageChanges();

            this.ctlUsage1.Value1 = (int)Statistic.FreeSize;
            this.ctlUsage1.Value2 = (int)Statistic.Usage;
            this.ctlUsage2.Value1 = (int)Statistic.CallCount;
            this.ctlUsage2.Value2 = 0;

            this.ctlUsageHistory1.AddValues((int)Statistic.FreeSize, (int)Statistic.Usage);
            this.ctlUsageHistory2.AddValues(Statistic.CallCount, 0);


            this.lblUsageValue1.Text = Statistic.Usage.ToString();
            this.lblUsageValue2.Text = Statistic.CallCount.ToString();
      
        }

        private void InitChart()
        {
            //if (useChannels)
            //{
            //    this.useChannels = !string.IsNullOrEmpty(sqlChannels);
            //}

            //OnUsageChanges();
            OnLedChanges();
            OnMeterChanges();
            ctlPieChart1.Items.Clear();
            if (Statistic == null)
                return;
            ctlPieChart1.Items.Add(new PieChartItem(0, Color.Blue, "Usage", "Usage: " + Statistic.Usage.ToString(), 0));
            ctlPieChart1.Items.Add(new PieChartItem(0, Color.Gold, "FreeSize", "Free: " + Statistic.FreeSize.ToString(), 0));
            //ctlPieChart1.Items.Add(new PieChartItem(0, Color.Green, QueueName3, "Queue: " + QueueName3, 0));
            //ctlPieChart1.Items.Add(new PieChartItem(0, Color.Red, QueueName4, "Queue: " + QueueName4, 0));

            this.ctlPieChart1.Padding = new System.Windows.Forms.Padding(60, 0, 0, 0);
            this.ctlPieChart1.ItemStyle.SurfaceTransparency = 0.75F;
            this.ctlPieChart1.FocusedItemStyle.SurfaceTransparency = 0.75F;
            this.ctlPieChart1.FocusedItemStyle.SurfaceBrightness = 0.3F;
            this.ctlPieChart1.AddChartDescription();
            this.ctlPieChart1.Leaning = (float)(40 * Math.PI / 180);
            this.ctlPieChart1.Depth = 50;
            this.ctlPieChart1.Radius = 240F;

            initilaized = true;
        }

        private void OnLedChanges()
        {
            this.SuspendLayout();

            this.ctlLedChannel1.ScaleMax = ledScaleMax;
            this.ctlLedChannel2.ScaleMax = ledScaleMax;
            this.ctlLedChannel3.ScaleMax = ledScaleMax;
            this.ctlLedChannel4.ScaleMax = ledScaleMax;
            
            this.ctlLedChannel1.ScaleLedCount = ledScaleCount;
            this.ctlLedChannel2.ScaleLedCount = ledScaleCount;
            this.ctlLedChannel3.ScaleLedCount = ledScaleCount;
            this.ctlLedChannel4.ScaleLedCount = ledScaleCount;

            int ledRed = Types.ToInt(ledScaleMax * 0.9, ledScaleMax);

            this.ctlLedChannel1.ScaleLedRed = ledRed;
            this.ctlLedChannel2.ScaleLedRed = ledRed;
            this.ctlLedChannel3.ScaleLedRed = ledRed;
            this.ctlLedChannel4.ScaleLedRed = ledRed;

            int ledYellow = Types.ToInt(ledScaleMax * 0.7, ledScaleMax);

            this.ctlLedChannel1.ScaleLedYellow = ledYellow;
            this.ctlLedChannel2.ScaleLedYellow = ledYellow;
            this.ctlLedChannel3.ScaleLedYellow = ledYellow;
            this.ctlLedChannel4.ScaleLedYellow = ledYellow;
            this.ResumeLayout(false);

        }
        private void OnMeterChanges()
        {

            //int max = Math.Max(meterScaleMax, (int)((float)QueueItem1Count * 1.2F));

            this.SuspendLayout();

            this.ctlMeter1.ScaleMax = Math.Max(meterScaleMax, (int)((float)Statistic.UsageDefault * 1.2F));
            this.ctlMeter2.ScaleMax = Math.Max(meterScaleMax, (int)((float)Statistic.UsageData * 1.2F));
            this.ctlMeter3.ScaleMax = Math.Max(meterScaleMax, (int)((float)Statistic.UsageImages * 1.2F));
            this.ctlMeter4.ScaleMax = Math.Max(meterScaleMax, (int)((float)Statistic.UsageFiles * 1.2F));

            this.ctlMeter1.ScaleInterval = meterScaleInterval;
            this.ctlMeter2.ScaleInterval = meterScaleInterval;
            this.ctlMeter3.ScaleInterval = meterScaleInterval;
            this.ctlMeter4.ScaleInterval = meterScaleInterval;

            int meterRed = Types.ToInt(meterScaleMax * 0.9, meterScaleMax);

            this.ctlMeter1.ScaleLedRed = meterRed;
            this.ctlMeter2.ScaleLedRed = meterRed;
            this.ctlMeter3.ScaleLedRed = meterRed;
            this.ctlMeter4.ScaleLedRed = meterRed;

            int meterYellow = Types.ToInt(meterScaleMax * 0.7, meterScaleMax);

            this.ctlMeter1.ScaleLedYellow = meterYellow;
            this.ctlMeter2.ScaleLedYellow = meterYellow;
            this.ctlMeter3.ScaleLedYellow = meterYellow;
            this.ctlMeter4.ScaleLedYellow = meterYellow;

            //int total = meterScaleMax * 4;
            //int ledRed = Types.ToInt(total * 0.9, total);
            //int ledYellow = Types.ToInt(total * 0.7, total);

            //this.ctlLedAll.ScaleMax = total;
            //this.ctlLedAll.ScaleLedRed = ledRed;
            //this.ctlLedAll.ScaleLedYellow = ledYellow;

            this.ResumeLayout(false);

        }
        private void OnUsageChanges()
        {
            if (Statistic == null)
                return;
            int max = Math.Max(maxUsage, (int)((float)Statistic.CallCount * 1.5F));
            //this.SuspendLayout();
            this.ctlUsage1.Maximum = (int)Statistic.MaxSize;
            this.ctlUsage2.Maximum = max;
            //this.ctlUsage3.Maximum = max;
            //this.ctlUsage4.Maximum = max;

            this.ctlUsageHistory1.Maximum = (int)Statistic.MaxSize;
            this.ctlUsageHistory2.Maximum = max;
            //this.ctlUsageHistory3.Maximum = max;
            //this.ctlUsageHistory4.Maximum = max;
            //this.ResumeLayout(false);
        }

        private void FillQueueValues()
        {
            //if (valuesSource == null || valuesSource.Rows.Count == 0)
            //{
            //    QueueItem1Count = 0;
            //    QueueItem2Count = 0;
            //    QueueItem3Count = 0;
            //    QueueItem4Count = 0;
            //}
            //else
            //{
            //    foreach (DataRow dr in valuesSource.Rows)
            //    {

            //        if (dr["QueueName"].ToString().Equals(QueueName1))
            //            QueueItem1Count = Types.ToInt(dr["Total"], 0);
            //        else if (dr["QueueName"].ToString().Equals(QueueName2))
            //            QueueItem2Count = Types.ToInt(dr["Total"], 0);
            //        else if (dr["QueueName"].ToString().Equals(QueueName3))
            //            QueueItem3Count = Types.ToInt(dr["Total"], 0);
            //        else if (dr["QueueName"].ToString().Equals(QueueName4))
            //            QueueItem4Count = Types.ToInt(dr["Total"], 0);
            //    }
            //}
            //test
            //QueueItem1Count = 1200;
            //QueueItem2Count = 200;
            //QueueItem3Count = 700;
            //QueueItem4Count = 500;

            //QueueItemTotalCount = QueueItem1Count + QueueItem2Count + QueueItem3Count + QueueItem4Count;
        }

        private void FillChannelValues()
        {
            //if (channelsSource == null || channelsSource.Rows.Count == 0)
            //{
            //        ChannelItems1Count = 0;
            //        ChannelItems2Count = 0;
            //        ChannelItems3Count = 0;
            //        ChannelItems4Count = 0;
            //}
            //else
            //{
            //    foreach (DataRow dr in channelsSource.Rows)
            //    {

            //        if (dr["ChannelName"].ToString().Equals(ChannelName1))
            //            ChannelItems1Count = Types.ToInt(dr["Total"], 0);
            //        else if (dr["ChannelName"].ToString().Equals(ChannelName2))
            //            ChannelItems2Count = Types.ToInt(dr["Total"], 0);
            //        else if (dr["ChannelName"].ToString().Equals(ChannelName3))
            //            ChannelItems3Count = Types.ToInt(dr["Total"], 0);
            //        else if (dr["ChannelName"].ToString().Equals(ChannelName4))
            //            ChannelItems4Count = Types.ToInt(dr["Total"], 0);
            //    }
            //}
            //test
            //ChannelItems1Count = 3200;
            //ChannelItems2Count = 5200;
            //ChannelItems3Count = 200;
            //ChannelItems4Count = 1200;

            //ChannelItemsTotalCount = ChannelItems1Count + ChannelItems2Count + ChannelItems3Count + ChannelItems4Count;

        }

         #endregion

        #region Config


        private void Config()
        {
            GridField[] fields = new GridField[8];
            fields[0] = new GridField("MaxSize", maxUsage);
            fields[1] = new GridField("Interval", interval);
            fields[2] = new GridField("MeterScaleInterval", meterScaleInterval);
            fields[3] = new GridField("MeterScaleMax", meterScaleMax);
            fields[4] = new GridField("LedScaleCount", ledScaleCount);
            fields[5] = new GridField("LedScaleMax", ledScaleMax);
            fields[6] = new GridField("AutoScale", autoScale);
            fields[7] = new GridField("TickInterval", tickInterval);

 
            fields[0].Description = "Max Memory of cache";
            fields[1].Description = "Interval in millisecondes for Refresh";
            fields[2].Description = "Meter Scale Interval ";
            fields[3].Description = "Meter Scale Max Value";
            fields[4].Description = "Led Scale items Count";
            fields[5].Description = "Led Scale Max value";
            fields[6].Description = "Auto Scale properies";
            fields[7].Description = "Tick Interval Refreshing";

            VGridDlg dlg = new VGridDlg();
            dlg.VGrid.SetDataBinding(fields, "Monitor");
            dlg.Width = 400;
            DialogResult dr = dlg.ShowDialog();

            maxUsage = Types.ToInt(fields[0].Value, maxUsage);
            interval = Types.ToInt(fields[1].Value, interval);
            meterScaleInterval = Types.ToInt(fields[2].Value, meterScaleInterval);
            meterScaleMax = Types.ToInt(fields[3].Value, meterScaleMax);
            ledScaleCount = Types.ToInt(fields[4].Value, ledScaleCount);
            ledScaleMax = Types.ToInt(fields[5].Value, ledScaleMax);
            autoScale = Types.ToBool(fields[6].Text, true);
            tickInterval = Types.ToInt(fields[7].Value, meterScaleMax);

        }

        #endregion

        private void ctlToolBar_ButtonClick(object sender, MControl.WinForms.ToolButtonClickEventArgs e)
        {
            switch(e.Button.Name)
            {

                case "tbRefresh":
                    if (tabControl.SelectedIndex == 0)
                    {
                        refreshQueueItems = true;
                    }
                    if (tabControl.SelectedIndex == 1)
                    {
                        refreshQueueSummarize = true;
                    }

                    break;
                case "tbClose":
                    this.Close();
                    break;
                 case "tbConfig":
                    Config();
                    LoadCache();
                    //AsyncDalReStart();
                    InitChart();
                    break;
           }
        }

        private void tbOffset_SelectedItemClick(object sender, MControl.WinForms.SelectedPopUpItemEvent e)
        {
            float offset = Types.ToFloat(e.Item.Text, 0F);
            for (int i = 0; i < ctlPieChart1.Items.Count; i++)
            {
                ctlPieChart1.Items[i].Offset = offset;
            }

        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.tbOffset.Enabled = this.tabControl.SelectedTab == pgChart;
            this.tbRefresh.Enabled = this.tabControl.SelectedIndex <= 1;
        }
    }
}