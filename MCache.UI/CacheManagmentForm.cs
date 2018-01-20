//#define TEST

using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Linq;
using System.Threading.Tasks;

using Nistec.Data;
using Nistec.Data.SqlClient;
using Nistec.Collections;
using Nistec.Caching.Data;
using Nistec.Data.Entities;
using Nistec.Caching.Session;
using Nistec.Caching.Install;
using Nistec.Services;
using Nistec.Generic;

using Nistec.Win;
using Nistec.Charts;
using Nistec.GridView;
using Nistec.WinForms;
using Nistec.Data.Entities.Cache;
using Nistec.Serialization;
using Nistec.Channels;

namespace Nistec.Caching.Remote.UI
{
    public partial class CacheManagmentForm : McForm
    {
        #region members

        private enum Actions
        {
            Services,
            RemoteCache,
            DataCache,
            SyncDb,
            SessionGrid,
            SessionTree,
            TimersReport
            //SessionActive,
            //SessionIdle,
        }
        private enum SubActions
        {
            Default,
            Log,
            Performance,
            Statistic,
            Usage
        }
        //private int curIndex;
        private Actions curAction = Actions.Services;
        private Actions lastAction = Actions.Services;
        private SubActions curSubAction = SubActions.Default;

        private bool shouldRefresh = true;
        //private System.ServiceProcess.ServiceController[] services;

        const string channelManager = "";
        const string channelServer = "";


        string[] Sections = new string[] { "RemoteCache", "DataCache", "SyncDb", "Session" };

        #endregion

        #region ctor

        public CacheManagmentForm()
        {
            InitializeComponent();
            this.tbBack.Enabled = false;
            this.tbForward.Enabled = false;
            this.mcManagment.TreeView.ImageList = this.imageList1;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CreateServicesNodeList();
            //LoadUsage();
        }

        #endregion

        #region Data

        private void CreateNodeDataItems(bool shouldRefresh)
        {
            this.shouldRefresh = shouldRefresh;
            CreateNodeDataItems();
        }

        private void CreateNodeDataItems()
        {
            if (!shouldRefresh && curAction == Actions.DataCache)
                return;

            try
            {
                DoClearSelectedItem();

                mcManagment.TreeView.Nodes.Clear();
                mcManagment.ListCaption = "Cache Data Items";

                string[] list = ManagerApi.GetAllDataKeys();
                if (list == null)
                    goto Label_Exit;

                //TreeNode parent = TreeNode("Data Cache", 9, 10);
                //this.mcManagment.Items.AddRange(parents);

                foreach (string s in list)
                {
                    int icon = 1;
                    string name = s;
                    TreeNode t = new TreeNode(name);
                    t.Tag = s;
                    t.ImageIndex = icon;
                    t.SelectedImageIndex = icon;
                    //parent.Nodes.Add(t);
                    this.mcManagment.Items.Add(t);
                }
                mcManagment.TreeView.Sort();
                //listBoxServices.DisplayMember = "DisplayName";
                //listBoxServices.DataSource = services;
                shouldRefresh = false;
                LoadUsage();
            Label_Exit:
                curAction = Actions.DataCache;
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }

        }

        private void ShowGridDataItems()
        {
            string text = GetSelectedItem();
            if (text != null)
            {
                ShowGridDataItems(text);
            }
        }

        private void ShowGridDataItems(string nameKey)
        {

            try
            {
                var ck = ComplexArgs.Parse(nameKey);
                var reportItem = ManagerApi.DataCacheApi.GetItemsReport(ck.Prefix,ck.Suffix);


                if (reportItem == null)
                    return;

                this.mcManagment.SelectedPage = pgItems;
                this.gridItems.CaptionText = reportItem.Caption;
                this.gridItems.DataSource = reportItem.Data;

                WiredGridDropDown(1);

                //DataTable item = ManagerApi.DataCacheApi.GetTable(null, name);
                //if (item == null)
                //    return;

                //this.mcManagment.SelectedPage = pgItems;
                //this.gridItems.CaptionText = item.TableName;
                //this.gridItems.DataSource = item;
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }


        #endregion

        #region SyncDb

        private void CreateNodeSyncItems(bool shouldRefresh)
        {
            this.shouldRefresh = shouldRefresh;
            CreateNodeSyncItems();
        }

        private void CreateNodeSyncItems()
        {
            if (!shouldRefresh && curAction == Actions.SyncDb)
                return;

            try
            {
                DoClearSelectedItem();

                mcManagment.TreeView.Nodes.Clear();
                mcManagment.ListCaption = "Cache Data Sync Items";

                string[] list = ManagerApi.GetAllSyncCacheKeys();
                if (list == null)
                    goto Label_Exit;

                //TreeNode parent = TreeNode("Data Cache", 9, 10);
                //this.mcManagment.Items.AddRange(parents);

                foreach (string s in list)
                {
                    int icon = 1;
                    string name = s;
                    TreeNode t = new TreeNode(name);
                    t.Tag = s;
                    t.ImageIndex = icon;
                    t.SelectedImageIndex = icon;
                    //parent.Nodes.Add(t);
                    this.mcManagment.Items.Add(t);
                }
                mcManagment.TreeView.Sort();
                //listBoxServices.DisplayMember = "DisplayName";
                //listBoxServices.DataSource = services;
                shouldRefresh = false;
                LoadUsage();
            Label_Exit:
                curAction = Actions.SyncDb;
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }

        }

        private void ShowGridSyncItems()
        {
            string text = GetSelectedItem();
            if (text != null)
            {
                ShowGridSyncItems(text);
            }
        }

        private void ShowGridSyncItems(string name)
        {

            try
            {
                var reportItem = ManagerApi.SyncCacheApi.GetItemsReport(name);


                if (reportItem == null)
                    return;

                this.mcManagment.SelectedPage = pgItems;
                this.gridItems.CaptionText = reportItem.Caption;
                this.gridItems.DataSource = reportItem.Data;
                WiredGridDropDown(1);
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }


        #endregion

        #region Cache grid

        private void CreateNodeItems(bool shouldRefresh)
        {
            this.shouldRefresh = shouldRefresh;
            CreateNodeItems();
        }

        private void CreateNodeItems()
        {
            if (!shouldRefresh && curAction == Actions.RemoteCache)
                return;

            try
            {
                DoClearSelectedItem();

                mcManagment.TreeView.Nodes.Clear();
                mcManagment.ListCaption = "Remote Cache Items";


                int icon = 1;
                string name = "Remote Cache";
                TreeNode t = new TreeNode(name);
                t.Tag = "Remote Cache";
                t.ImageIndex = icon;
                t.SelectedImageIndex = icon;
                this.mcManagment.Items.Add(t);


                shouldRefresh = false;
                //LoadUsage();
                curAction = Actions.RemoteCache;
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }

        //private void ShowGridItems()
        //{
        //    string text = GetSelectedItem();
        //    if (text != null)
        //    {
        //        ShowGridItems(text);
        //    }
        //}
        private void ShowGridItems(string selected)
        {

            try
            {
                var reportItem = ManagerApi.Report(CacheManagerCmd.ReportCacheItems);//.GetCacheItemsReport();

                if (reportItem == null)
                    return;

                //this.mcManagment.SelectedPage = pgItems;
                this.gridItems.CaptionText = reportItem.Caption;
                this.gridItems.DataSource = reportItem.Data;
                //WiredGridDropDown(1);
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }

        private void ShowGridItems()
        {

            try
            {
                var reportItem = ManagerApi.Report(CacheManagerCmd.ReportCacheItems);//.GetCacheItemsReport();

                if (reportItem == null)
                    return;

                this.mcManagment.SelectedPage = pgItems;
                this.gridItems.CaptionText = reportItem.Caption; 
                this.gridItems.DataSource = reportItem.Data;
                WiredGridDropDown(1);
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }

#if(false)
        /*
        private void CreateNodeItems(bool shouldRefresh)
        {
            this.shouldRefresh = shouldRefresh;
            CreateNodeItems();
        }

        private void CreateNodeItems()
        {

            if (!shouldRefresh && curAction == Actions.RemoteCache)
                return;

            try
            {
                DoClearSelectedItem();

                mcManagment.TreeView.Nodes.Clear();
                mcManagment.ListCaption = "Cache Items";

                string[] list = ManagerApi.GetAllKeysIcons();
                if (list == null)
                    goto Label_Exit;

                foreach (string s in list)
                {
                    int icon = Types.ToInt(s.Substring(0, 1), 0);
                    string name = s.Substring(2);
                    TreeNode t = new TreeNode(name);
                    t.Tag = s;
                    t.ImageIndex = icon;
                    t.SelectedImageIndex = icon;
                    //parents[icon].Nodes.Add(t);
                    this.mcManagment.Items.Add(t);
                }
                mcManagment.TreeView.Sort();
                //listBoxServices.DisplayMember = "DisplayName";
                //listBoxServices.DataSource = services;
                shouldRefresh = false;
                LoadUsage();

            Label_Exit:
                curAction = Actions.RemoteCache;
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }
        */
        private void ShowGridItems()
        {
            string text = GetSelectedItem();

            if (text != null)
            {
                //if (ObjTypeNodeList.Contains(text))
                //    return; 
                ShowGridItems(text);
            }
        }

        private void ShowGridItems(string name)
        {

            try
            {

                Task<CacheEntry> task = new Task<CacheEntry>(() => ManagerApi.CacheApi.GetItem(name));
                task.Start();
                task.Wait(5000);
                if (!task.IsCompleted)
                {
                    return;
                }
                CacheEntry item = task.Result;

                if (item == null)
                    return;

                ShowItemJson(item);

                //this.mcManagment.SelectedPage = pgImage;
                //this.txtImageHeader.Text = item.PrintHeader();
                //object value = item.GetValue();
                //this.txtBody.Text = item.GetValueJson();//.BodyToBase64();

                //if (Serialization.SerializeTools.IsPrimitiveOrString(item.BodyType))
                //    ShowItemValue(item);
                //else
                //    ShowItemRef(item);

                /*
                switch (item.CacheObjType)
                {
                    case CacheObjType.ImageFile:
                        this.mcManagment.SelectedPage = pgImage;
                        this.txtImageHeader.Text = item.PrintHeader();
                        value = item.GetValue();
                        if (value != null)
                            this.imgSource.Image = (Image)CacheUtil.DeserializeImage(value.ToString());
                        break;
                    case CacheObjType.BinaryFile:
                        this.mcManagment.SelectedPage = pgSource;
                        this.txtHeader.Text = item.PrintHeader();
                        //value = item.Load();
                        value = item.GetValue();
                        if (value != null)
                            this.txtBody.Text = value.ToString();
                        break;
                    case CacheObjType.TextFile:
                        this.mcManagment.SelectedPage = pgSource;
                        this.txtHeader.Text = item.PrintHeader();
                        //value = item.Load();
                        value = item.GetValue();
                        if (value != null)
                            this.txtBody.Text = value.ToString();
                        break;
                    case CacheObjType.XmlDocument:
                        this.mcManagment.SelectedPage = pgSource;
                        this.txtHeader.Text = item.PrintHeader();

                        value = item.GetValue();
                        if (value != null)
                            this.txtBody.Text = value.ToString();
                        break;
                    case CacheObjType.HtmlFile:
                        this.mcManagment.SelectedPage = pgBrowser;
                        this.txtHeaderBrowser.Text = item.PrintHeader();

                        value = item.GetValue();
                        if (value != null)
                            this.ctlBrowser.DocumentText = value.ToString();
                        break;
                    case CacheObjType.Default:
                        value = item.GetValue();
                        Type type=value.GetType();
                        FieldType fld = Nistec.Types.DataTypeOf(type);//Type.GetType(item.ItemType));
                        switch(fld)
                        {
                            case FieldType.Bool:
                            case FieldType.Date:
                            case FieldType.Number:
                                ShowItemValue(item);
                                break;
                            default://case FieldType.Text:
                                if(type.IsValueType)
                                    ShowItemValue(item);
                                else if (type == typeof(string))
                                    ShowItemValue(item);
                                else 
                                    ShowItemRef(item);
                                break;
                        }
                        break;
                    case CacheObjType.RemotingData:
                        this.mcManagment.SelectedPage = pgItems;
                        this.gridItems.CaptionText = item.PrintHeader();
                        DataTable dt = (DataTable) item.GetValue();//.DeserializeValue();//.Value;
                        this.gridItems.DataSource = dt;
                        break;
                    case CacheObjType.SerializeClass:
                        this.mcManagment.SelectedPage = pgClass;
                        this.vgrid.CaptionText = item.PrintHeader();
                        this.vgrid.SetDataBinding(item.Clone(), item.Key);
                        break;
                }
                 */
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }
#endif
        private void RemoveTreeItem(string key)
        {
            try
            {
                if (mcManagment.TreeView == null || mcManagment.TreeView.Nodes == null)
                    return;

                TreeNode tn = mcManagment.TreeView.SelectedNode;
                if (tn.Parent == null)
                {
                    return;
                }
                tn.Remove();

                mcManagment.TreeView.Refresh();
                shouldRefresh = false;
                LoadUsage();
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }

        private void RemoveTreeItem()
        {
            try
            {

                if (mcManagment.TreeView == null || mcManagment.TreeView.Nodes == null)
                    return;

                TreeNode tn = mcManagment.TreeView.SelectedNode;
                if (tn.Parent == null)
                {
                    if (tn.Nodes == null || tn.Nodes.Count == 0)
                    {
                        return;
                    }
                    if (MsgBox.ShowQuestionYNC("This will removed all items from Cache section, Continue", "Remove Items") == DialogResult.Yes)
                    {
                        foreach (TreeNode n in tn.Nodes)
                        {
                            ManagerApi.CacheApi.Remove(n.Text);
                        }
                        tn.Nodes.Clear();
                    }
                }
                else
                {
                    if (MsgBox.ShowQuestionYNC("This item will removed from Cache, Continue", "Remove Item") == DialogResult.Yes)
                    {
                        ManagerApi.CacheApi.Remove(tn.Text);
                        tn.Remove();
                    }
                }

                mcManagment.TreeView.Refresh();
                shouldRefresh = false;
                LoadUsage();
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }

        private void ShowItemJson(CacheEntry item)
        {
            this.mcManagment.SelectedPage = pgSource;
            this.txtHeader.Text = item.PrintHeader();
            this.txtBody.Text = item.ToJson(true);
        }

        private void ShowItemValue(CacheEntry item)
        {
            this.mcManagment.SelectedPage = pgSource;
            this.txtHeader.Text = item.PrintHeader();
            this.txtBody.Text = item.GetValueJson();// value.ToString();
        }
        private void ShowItemRef(CacheEntry item)
        {
            this.vgrid.Fields.Clear();
            this.mcManagment.SelectedPage = pgClass;
            this.vgrid.CaptionText = item.PrintHeader();
            this.vgrid.SetDataBinding(item.GetValue(), item.Id);
        }

        #endregion

        #region Session

        private void CreateNodeSession(bool shouldRefresh)
        {
            this.shouldRefresh = shouldRefresh;
            CreateNodeSession();
        }
        private void CreateNodeSessionGrid()
        {
            try
            {
                DoClearSelectedItem();

                mcManagment.TreeView.Nodes.Clear();
                mcManagment.ListCaption = "Session Grid";

                int icon = 1;
                string name = "Session Grid";
                TreeNode t = new TreeNode(name);
                t.Tag = "Session Grid";
                t.ImageIndex = icon;
                t.SelectedImageIndex = icon;

                this.mcManagment.Items.Add(t);
                
                shouldRefresh = false;
                curAction = Actions.SessionGrid;
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }

        private void CreateNodeSession()
        {
            try
            {
                DoClearSelectedItem();

                mcManagment.TreeView.Nodes.Clear();
                mcManagment.ListCaption = "Cache Session Items";

                ICollection<string> sessionList = ManagerApi.GetAllSessionsKeys();

                if (sessionList == null || sessionList.Count == 0)
                    goto Label_Exit;

                Dictionary<string, TreeNode> parents = new Dictionary<string, TreeNode>();
                foreach (string item in sessionList)
                {
                    TreeNode t = new TreeNode(item, 9, 10);
                    t.Tag = "Session";
                    parents[item] = t;

                    ICollection<string> items = ManagerApi.GetSessionsItemsKeys(item);
                    foreach (string s in items)
                    {
                        string name = s;
                        TreeNode titem = new TreeNode(name, 0, 1);
                        titem.Tag = s;// pair.Value;
                        string kv = (string)item;
                        if (parents.ContainsKey(kv))
                        {
                            parents[kv].Nodes.Add(titem);
                        }
                    }
                }


                TreeNode[] range = new TreeNode[parents.Count];
                parents.Values.CopyTo(range, 0);
                this.mcManagment.Items.AddRange(range);

                mcManagment.TreeView.Sort();
                shouldRefresh = false;

            Label_Exit:
                curAction = Actions.SessionTree;// Actions.Session;
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }

        private void CreateNodeSession(SessionState state)
        {

            //Actions action = state == SessionState.Active ? Actions.SessionActive : Actions.SessionIdle;
            Actions action = Actions.SessionTree;
            try
            {
                DoClearSelectedItem();

                mcManagment.TreeView.Nodes.Clear();
                mcManagment.ListCaption = "Cache Session Items";

                ICollection<string> sessionList = ManagerApi.GetAllSessionsKeysByState(state);

                if (sessionList == null || sessionList.Count == 0)
                    goto Label_Exit;

                Dictionary<string, TreeNode> parents = new Dictionary<string, TreeNode>();
                foreach (string item in sessionList)
                {
                    TreeNode t = new TreeNode(item, 9, 10);
                    t.Tag = "Session";
                    parents[item] = t;

                    ICollection<string> items = ManagerApi.GetSessionsItemsKeys(item);
                    foreach (string s in items)
                    {
                        string name = s;
                        TreeNode titem = new TreeNode(name, 0, 1);
                        titem.Tag = s;// pair.Value;
                        string kv = (string)item;
                        if (parents.ContainsKey(kv))
                        {
                            parents[kv].Nodes.Add(titem);
                        }
                    }
                }

                TreeNode[] range = new TreeNode[parents.Count];
                parents.Values.CopyTo(range, 0);
                this.mcManagment.Items.AddRange(range);

                mcManagment.TreeView.Sort();
                shouldRefresh = false;

            Label_Exit:
                curAction = action;
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }

        private void ShowGridTreeSession()
        {
            string selected = GetSelectedItem();
            ShowGridTreeSession(selected);
        }

        private void ShowGridTreeSession(string selected)
        {
            //string selected = GetSelectedItem();
            if (selected != null)
            {
                string tag = GetSelectedTag();

                if (tag == "Session")
                {
                    ShowSessionItem(selected);
                }
                else
                {
                    ShowSessionEntry(tag, selected);
                }
            }
        }

        private void ShowGridSessionItems(string selected)
        {

            try
            {
                var reportItem = ManagerApi.Report(CacheManagerCmd.ReportSessionItems);

                if (reportItem == null)
                    return;

                //this.mcManagment.SelectedPage = pgItems;
                this.gridItems.CaptionText = reportItem.Caption;
                this.gridItems.DataSource = reportItem.Data;
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }
        private void ShowGridSessionItems()
        {

            try
            {
                var reportItem = ManagerApi.Report(CacheManagerCmd.ReportSessionItems);

                if (reportItem == null)
                    return;

                this.mcManagment.SelectedPage = pgItems;
                this.gridItems.CaptionText = reportItem.Caption;
                this.gridItems.DataSource = reportItem.Data;
                WiredGridDropDown(1);
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }
        private void ShowSessionEntry(string tag, string key)
        {
            if (tag == "SessionTree")//(tag == "Session" || tag == "SessionActive" || tag == "SessionIdle")
                return;
            var node = GetSelectedNode();
            var sessionId = node.Parent.Text;
            var entry = ManagerApi.SessionApi.ViewSessionItem(sessionId, key);
            ShowPropertyJson(entry, "Session Item", sessionId + ", " + key);
        }
            
    
        private void ShowSessionItem(string sessionId)
        {
            ISessionBagStream item = ManagerApi.SessionApi.ViewExistingSession(sessionId);
            if (item == null)//.IsEmpty)
                return;
            this.mcManagment.SelectedPage = pgClass;
            this.vgrid.CaptionText = string.Format("Id: {0}, Creation: {1}, LastUsed:{2}", item.SessionId, item.Creation, item.LastUsed);
            this.vgrid.SetDataBinding(item, item.SessionId);
        }


        #endregion

        #region  Timer Report

        private void CreateNodeTimerItems(bool shouldRefresh)
        {
            this.shouldRefresh = shouldRefresh;
            CreateNodeTimerItems();
        }

        private void CreateNodeTimerItems()
        {
            if (!shouldRefresh && curAction == Actions.TimersReport)
                return;

            try
            {
                DoClearSelectedItem();

                mcManagment.TreeView.Nodes.Clear();
                mcManagment.ListCaption = "Cache Timer Items";


                //case CacheManagerCmd.ReportCacheTimer:
                //case CacheManagerCmd.ReportSessionTimer:
                //case CacheManagerCmd.ReportSyncBoxItems:
                //case CacheManagerCmd.ReportSyncBoxQueue:
                //case CacheManagerCmd.ReportTimerSyncDispatcher:


                string[] list = new string[] { CacheManagerCmd.ReportCacheTimer, CacheManagerCmd.ReportSessionTimer, CacheManagerCmd.ReportDataTimer, CacheManagerCmd.ReportSyncBoxItems, CacheManagerCmd.ReportSyncBoxQueue, CacheManagerCmd.ReportTimerSyncDispatcher };

                foreach (string s in list)
                {
                    int icon = 1;
                    string name = s;
                    TreeNode t = new TreeNode(name);
                    t.Tag = s;
                    t.ImageIndex = icon;
                    t.SelectedImageIndex = icon;
                    //parent.Nodes.Add(t);
                    this.mcManagment.Items.Add(t);
                }
                mcManagment.TreeView.Sort();
                shouldRefresh = false;
                LoadUsage();
                curAction = Actions.TimersReport;
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }

        private void ShowGridTimerItems()
        {
            string text = GetSelectedItem();
            if (text != null)
            {
                ShowGridTimerItems(text);
            }
        }

        private void ShowGridTimerItems(string command)
        {

            try
            {
                var reportItem = ManagerApi.Report(command);


                if (reportItem == null)
                    return;

                this.mcManagment.SelectedPage = pgItems;
                this.gridItems.CaptionText = reportItem.Caption;
                this.gridItems.DataSource = reportItem.Data;
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }


        #endregion

        #region Grid box

        bool EditBoxFlag = false;
        private void WiredGridDropDown(int column)
        {
            if (EditBoxFlag)
                UnWiredGridDropDown(column);
            if (gridItems.Columns.Count > column && gridItems.Columns[column].ColumnType == GridColumnType.MemoColumn)
            {
                ((GridMemoColumn)gridItems.Columns[column]).EditBox.DropDown += EditBox_DropDown;
                ((GridMemoColumn)gridItems.Columns[column]).EditBox.DropUp += EditBox_DropUp;
                EditBoxFlag = true;
            }
        }

        private void UnWiredGridDropDown(int column)
        {
            if (EditBoxFlag)
            {
                if (gridItems.Columns.Count > column && gridItems.Columns[column].ColumnType== GridColumnType.MemoColumn)
                {
                    ((GridMemoColumn)gridItems.Columns[column]).EditBox.DropDown -= EditBox_DropDown;
                    ((GridMemoColumn)gridItems.Columns[column]).EditBox.DropUp -= EditBox_DropUp;
                }
                EditBoxFlag = false;
            }
        }

        private void EditBox_DropUp(object sender, EventArgs e)
        {

        }
        private void EditBox_DropDown(object sender, EventArgs e)
        {
            DropDownColumnBody((GridMemoBox)sender);
        }
        private void DropDownColumnUp(GridMemoBox box)
        {

        }
        private void DropDownColumnBody(GridMemoBox box)
        {
            try
            {

                DataRowView record = GetCurrentGridRow();
                if (record == null)
                {
                    return;
                }
                string key = Types.NZ(record.Row[0], null);
                if (key == null)
                {
                    return;
                }

                switch (curAction)
                {
                    case Actions.RemoteCache:
                        {
                            var item = ManagerApi.CacheApi.ViewEntry(key);
                            if (item == null)
                            {
                                return;
                            }
                            //var body = item.DecodeBody();
                            //if (body == null)
                            //{
                            //    return;
                            //}
                            box.Text = item.GetValueJson(true);
                        }
                        break;
                    case Actions.SyncDb:
                        {
                            string name = GetSelectedItem();
                            //var keyInfo = ComplexKey.Get(name, ComplexKey.SplitKey(key));
                            //var keyInfo = ComplexKey.Get(name, key);
                            var item = (GenericRecord)ManagerApi.SyncCacheApi.GetAs(name, key);// typeof(GenericRecord));
                            if (item == null)
                            {
                                return;
                            }
                            box.Text = item.ToJson(true);
                        }
                        break;
                    case Actions.SessionGrid:
                        {
                            string id = Types.NZ(record.Row[4], null);
                            key = key.Replace(id + KeySet.Separator, "");
                            //key.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
                            var item = ManagerApi.SessionApi.ViewSessionItem(id, key);
                            if (item == null)
                            {
                                return;
                            }
                            box.Text = item.BodyToJson(true);
                        }
                        break;
                    case Actions.DataCache:
                        {
                            string nameInfo = GetSelectedItem();
                            var keyInfo = ComplexKey.Parse(nameInfo);
                            //var keyInfo = ComplexKey.Get(name, key);
                            var item = (GenericRecord)ManagerApi.DataCacheApi.GetRecord(keyInfo.Prefix, keyInfo.Suffix, key);
                            if (item == null)
                            {
                                return;
                            }
                            box.Text = item.ToJson(true);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message,"Cache Management");
            }
        }

        void RefreshGrid()
        {
            if (gridItems == null || gridItems.Rows == null || gridItems.Rows.Count == 0)
            {
                return;
            }
            gridItems.Refresh();
            
        }
        DataRowView GetCurrentGridRow()
        {
            if (gridItems == null || gridItems.Rows == null || gridItems.Rows.Count == 0)
            {
                return null;
            }
            if (gridItems.CurrentRowIndex < 0)
            {
                return null;
            }
            return gridItems.GetCurrentDataRow();
        }

        string GetSelectedCacheKey()
        {
            DataRowView record = GetCurrentGridRow();
            if (record == null)
            {
                return null;
            }
            string key = Types.NZ(record.Row[0], null);
            return key;

        }
        string GetSelectedSessionId()
        {
            DataRowView record = GetCurrentGridRow();
            if (record == null)
            {
                return null;
            }
            string id = Types.NZ(record.Row[4], null);
            return id;

        }
        string GetSelectedSessionKey()
        {

            DataRowView record = GetCurrentGridRow();
            if (record == null)
            {
                return null;
            }
            string key = Types.NZ(record.Row[0], null);
            if (key == null)
            {
                return null;
            }

            string id = Types.NZ(record.Row[4], null);
            key = key.Replace(id + KeySet.Separator, "");
            return key;
        }

        #endregion

        #region service

        private void CreateServicesNodeList()
        {
            if (!shouldRefresh && curAction == Actions.Services)
                return;

            RefreshServiceList();
            ToolBarSettings(Actions.Services);
            shouldRefresh = false;
            curAction = Actions.Services;

        }

        private void RefreshServiceList()
        {
            mcManagment.TreeView.Nodes.Clear();
            mcManagment.ListCaption = "Services";
            this.mcManagment.SelectedPage = pageDetails;

            var service = Settings.GetServiceInstalled();
            if (service == null)
            {
                return;
            }
            TreeNode t = new TreeNode(service.DisplayName);
            t.Tag = service.ServiceController;
            t.ImageIndex = 8;
            t.SelectedImageIndex = 8;
            //t.StateImageIndex = 8;
            this.mcManagment.Items.Add(t);

        }

       
        protected string GetServiceTypeName(ServiceType type)
        {
            string serviceType = "";
            if ((type & ServiceType.InteractiveProcess) != 0)
            {
                serviceType = "Interactive ";
                type -= ServiceType.InteractiveProcess;
            }
            switch (type)
            {
                case ServiceType.Adapter:
                    serviceType += "Adapter";
                    break;
                case ServiceType.FileSystemDriver:
                case ServiceType.KernelDriver:
                case ServiceType.RecognizerDriver:
                    serviceType += "Driver";
                    break;
                case ServiceType.Win32OwnProcess:
                    serviceType += "Win32 Service Process";
                    break;
                case ServiceType.Win32ShareProcess:
                    serviceType += "Win32 Shared Process";
                    break;
                default:
                    serviceType += "unknown type " + type.ToString();
                    break;
            }
            return serviceType;
        }

        protected void SetServiceStatus(ServiceController controller)
        {
            bool isEnabled = controller != null;
            tbStart.Enabled = isEnabled;
            tbStop.Enabled = isEnabled;
            tbPause.Enabled = isEnabled;
            tbRestart.Enabled = isEnabled;
            //tbInstall.Enabled = !isEnabled;// !Settings.IsServiceInstalled();
            if (controller != null)
            {
                if (!controller.CanPauseAndContinue)
                {
                    tbPause.Enabled = false;
                    //tbRestart.Enabled = false;
                }
                if (!controller.CanStop)
                {
                    tbStop.Enabled = false;
                }
                SetServiceStatus(controller.Status);
            }

        }
        protected void SetServiceStatus(ServiceControllerStatus status)
        {

            switch (status)
            {
                case ServiceControllerStatus.ContinuePending:
                    //textServiceStatus.Text = "Continue Pending";
                    tbPause.Enabled = false;
                    //tbInstall.Enabled = false;
                    break;
                case ServiceControllerStatus.Paused:
                    //textServiceStatus.Text = "Paused";
                    tbPause.Enabled = false;
                    tbStart.Enabled = false;
                    //tbInstall.Enabled = false;
                    break;
                case ServiceControllerStatus.PausePending:
                    //textServiceStatus.Text = "Pause Pending";
                    tbPause.Enabled = false;
                    tbStart.Enabled = false;
                    //tbInstall.Enabled = false;
                    break;
                case ServiceControllerStatus.StartPending:
                    //textServiceStatus.Text = "Start Pending";
                    tbStart.Enabled = false;
                    //tbInstall.Enabled = false;
                    break;
                case ServiceControllerStatus.Running:
                    //textServiceStatus.Text = "Running";
                    tbStart.Enabled = false;
                    //tbInstall.Enabled = false;

                    //ToolBarSettings(Actions.RemoteCache);
                    //CreateServicesNodeList();
                    //LoadUsage();

                    break;
                case ServiceControllerStatus.Stopped:
                    //textServiceStatus.Text = "Stopped";
                    tbStop.Enabled = false;
                    tbRestart.Enabled = false;
                    //tbInstall.Enabled = true;
                    break;
                case ServiceControllerStatus.StopPending:
                    //textServiceStatus.Text = "Stop Pending";
                    tbStop.Enabled = false;
                    tbRestart.Enabled = false;
                    tbInstall.Enabled = false;
                    break;
                default:
                    //textServiceStatus.Text = "Unknown status";
                    //tbInstall.Enabled = true;

                    break;
            }

        }
        #endregion

        #region Service controller

        //private bool IsServiceControllerInstalled()
        //{

        //    TreeNode node = mcManagment.TreeView.SelectedNode;
        //    if (node != null)
        //    {
        //        var controller = (ServiceController)node.Tag;
        //        return controller != null;
        //    }
        //    return false;
        //}

        ServiceController m_controller;
        private void LoadServiceController()
        {
            if (m_controller == null)
            {
                var agent = Settings.GetServiceInstalled();
                if (agent != null)
                {
                    m_controller = agent.ServiceController;
                }
            }
        }

        private ServiceController GetServiceController()
        {
            LoadServiceController();
            return m_controller;
        }

        private bool IsServiceControllerInstalled()
        {
            LoadServiceController();
            return (m_controller != null);
        }

        private bool IsServiceControllerRunning()
        {
            LoadServiceController();

            if (m_controller != null)
            {
                return m_controller.Status == ServiceControllerStatus.Running;
            }

            return false;

            //TreeNode node = mcManagment.TreeView.SelectedNode;
            //if (node != null)
            //{
            //    var controller = (ServiceController)node.Tag;
            //    if (controller != null)
            //    {
            //        return controller.Status == ServiceControllerStatus.Running;
            //    }
            //}

            //return false;
        }

        //private ServiceController GetServiceController()
        //{
        //    ServiceController controller = null;
        //    if (curAction == Actions.Services)
        //    {
        //        TreeNode node = mcManagment.TreeView.SelectedNode;
        //        if (node != null)
        //        {
        //            controller = (ServiceController)node.Tag;
        //            curIndex = node.Index;
        //        }
        //    }
        //    return controller;
        //}

        private void ShowServiceDetails()
        {
            ServiceController controller = GetServiceController();
            if (controller == null)
                return;

            this.listView.Items.Clear();
            ListViewItem item = this.listView.Items.Add(controller.DisplayName);
            item.SubItems.Add(controller.ServiceName);
            item.SubItems.Add(controller.ServiceType.ToString());
            item.SubItems.Add(controller.Status.ToString());
            this.listView.Items.Add("You need administration permission for service controller!");
            SetServiceStatus(controller);
        }

        private void DoRefreshSubAction(bool reset)
        {
            switch (curSubAction)
            {
                case SubActions.Performance:
                case SubActions.Statistic:
                case SubActions.Usage:
                    if (reset)
                    {
                        switch (MsgBox.ShowQuestionYNC("This action will reset performance counter, Continue? ", ""))
                        {
                            case System.Windows.Forms.DialogResult.Cancel:
                                break;
                            case System.Windows.Forms.DialogResult.OK:
                            case System.Windows.Forms.DialogResult.Yes:
                                DoResetPerformanceCounter();
                                DoRefreshPerformance(curSubAction);
                                break;
                            case System.Windows.Forms.DialogResult.No:
                                DoRefreshPerformance(curSubAction);
                                break;
                        }
                    }
                    else
                    {
                        DoRefreshPerformance(curSubAction);
                    }
                    break;
                default:
                    break;
            }
        }

        private void DoRefreshPerformance(SubActions action)
        {
            switch (action)
            {
                case SubActions.Performance:
                    DoPerformanceReport(); break;
                case SubActions.Statistic:
                    DoStatistic(); break;
                case SubActions.Usage:
                    DoUsage(); break;
            }
        }

        private bool IsPerformanceSubActions
        {
            get { return (curSubAction == SubActions.Performance || curSubAction == SubActions.Statistic || curSubAction == SubActions.Usage); }
        }

        private void DoRefresh()
        {
            if (curSubAction != SubActions.Default)
            {
                if (IsPerformanceSubActions)
                {
                    DoRefreshSubAction(false);
                    return;
                }
                else if (curSubAction == SubActions.Log)
                {
                    DoLog();
                    return;
                }
            }
            curSubAction = SubActions.Default;

            switch (curAction)
            {

                case Actions.RemoteCache:
                    {
                        var selected = GetSelectedNode();
                        if (selected != null)
                        {
                            ShowGridItems(selected.Text);
                            SetSelectTreeView();
                        }
                        else
                        {
                            CreateNodeItems(true);
                            ShowGridItems();
                        }
                    }
                    break;
                case Actions.SessionTree:
                    {
                        var selected = GetSelectedNode();
                        if (selected != null)
                        {
                            ShowGridTreeSession(selected.Text);
                            SetSelectTreeView();
                        }
                        else
                        {
                            CreateNodeSession(true);
                        }
                    }
                    break;
                case Actions.SessionGrid:
                    {
                        var selected = GetSelectedNode();
                        if (selected != null)
                        {
                            ShowGridSessionItems(selected.Text);
                            SetSelectTreeView();
                        }
                        else
                        {
                            CreateNodeSessionGrid();
                        }
                    }
                    break;
                case Actions.TimersReport:
                    {
                        var selected = GetSelectedNode();
                        if (selected != null)
                        {
                            ShowGridTimerItems(selected.Text);
                            SetSelectTreeView();
                        }
                        else
                        {
                            CreateNodeTimerItems(true);
                            ShowGridTimerItems();
                        }
                    }
                    break;
                case Actions.DataCache:
                    {
                        CreateNodeDataItems(true);
                        ShowGridDataItems();
                    }
                    break;
                case Actions.SyncDb:
                    {
                        var selected = GetSelectedItem();
                        if (selected != null)
                        {
                            ShowGridSyncItems(selected);
                            SetSelectTreeView();
                        }
                        else
                        {
                            CreateNodeSyncItems(true);
                            ShowGridSyncItems();
                        }
                    }
                    break;
                case Actions.Services:
                    {
                        RefreshServiceList();
                    }
                    break;
            }
        }

        private void DoInstall()
        {
            curSubAction = SubActions.Default;

            //if (!Settings.IsServiceInstalled())
            //{

            wfrm_Install frm = new wfrm_Install();
            DialogResult dr = frm.ShowDialog();
            if (dr == DialogResult.OK)
            {
                frm.Close();
            }

            RefreshServiceList();
            //this.mcManagment.SelectedIndex = 0;
            //this.mcManagment.SelectNode();
            ToolBarSettings(Actions.Services);
            ShowServiceDetails();

            // }
        }

        private void DoPause()
        {
            try
            {
                curSubAction = SubActions.Default;

                ServiceController controller = GetServiceController();
                if (controller == null)
                    return;
                WaitDlg.RunProgress("Pause...");
                if (controller.Status == ServiceControllerStatus.Paused || controller.Status == ServiceControllerStatus.PausePending)
                {
                    controller.Continue();
                    controller.WaitForStatus(ServiceControllerStatus.Running);
                }
                else
                {
                    controller.Pause();
                    controller.WaitForStatus(ServiceControllerStatus.Paused);
                }
                System.Threading.Thread.Sleep(1000);
                SetServiceStatus(controller);
                ShowServiceDetails();
            }
            finally
            {
                WaitDlg.EndProgress();
            }
        }
        private void DoRestart()
        {
            try
            {
                curSubAction = SubActions.Default;

                ServiceController controller = GetServiceController();
                if (controller == null)
                    return;
                WaitDlg.RunProgress("Stop...");
                controller.Stop();
                controller.WaitForStatus(ServiceControllerStatus.Stopped);
                System.Threading.Thread.Sleep(1000);
                WaitDlg.RunProgress("Start...");
                controller.Start();
                controller.WaitForStatus(ServiceControllerStatus.Running);
                System.Threading.Thread.Sleep(1000);
                SetServiceStatus(controller);
                ShowServiceDetails();
            }
            finally
            {
                WaitDlg.EndProgress();
            }

        }

        private void DoStart()
        {
            try
            {
                curSubAction = SubActions.Default;

                ServiceController controller = GetServiceController();
                if (controller == null)
                    return;
                WaitDlg.RunProgress("Start...");
                controller.Start();
                controller.WaitForStatus(ServiceControllerStatus.Running);
                System.Threading.Thread.Sleep(1000);
                SetServiceStatus(controller);
                ShowServiceDetails();
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
            finally
            {
                WaitDlg.EndProgress();
            }
        }

        private void DoStop()
        {
            ServiceController controller = null;
            try
            {
                curSubAction = SubActions.Default;

                controller = GetServiceController();
                if (controller == null)
                    return;
                WaitDlg.RunProgress("Stop...");
                controller.Stop();
                controller.WaitForStatus(ServiceControllerStatus.Stopped);
                System.Threading.Thread.Sleep(1000);
                SetServiceStatus(controller);
                ShowServiceDetails();
            }
            catch (Exception)
            {
                //MsgBox.ShowError(ex.Message);
                SetServiceStatus(ServiceControllerStatus.StopPending);
            }
            finally
            {
                WaitDlg.EndProgress();
            }
        }
        #endregion

        #region tool bar

        private void SetSelectTreeView()
        {
            mcManagment.TreeView.Select();
        }

        private void SetSelectedNode(TreeNode selected)
        {
            mcManagment.TreeView.SelectedNode = selected;
        }

        private TreeNode GetSelectedNode()
        {
            TreeNode node = mcManagment.TreeView.SelectedNode;
            return node;
        }
        private string GetSelectedItem()
        {
            TreeNode node = mcManagment.TreeView.SelectedNode;
            if (node == null)
                return null;
            return node.Text;
        }
        private string GetSelectedTag()
        {
            TreeNode node = mcManagment.TreeView.SelectedNode;
            if (node == null)
                return null;
            return (string)node.Tag;
        }


        private void ToolBarSettings(Actions mode)
        {

            bool isTimer = (mode == Actions.TimersReport);
            bool isItems = (mode == Actions.RemoteCache || mode == Actions.DataCache || mode == Actions.SyncDb);
            bool isService = (mode == Actions.Services);
            //bool isSession = (mode == Actions.Session || mode == Actions.SessionActive || mode == Actions.SessionIdle );
            bool isSession = (mode == Actions.SessionTree || mode == Actions.SessionGrid);

            tbUsage.Enabled = !isService;// && !isSession;
            tbItems.Enabled = !isService;
            tbProperty.Enabled = !isService;
            tbRefreshItem.Enabled = !isService;

            tbStatistic.Enabled = !isService;// && !isSession;
            tbAddItem.Enabled = false;// !isService && !isSession;
            tbDelete.Enabled = isItems || isSession;// !isService && !isTimer;
            tbSaveXml.Enabled = false;// !isService && !isSession;
            tbLoadXml.Enabled = false;// !isService && !isSession;
            tbLog.Enabled = !isService;
            tbHelp.Enabled= !isService;
            //-tbActions.Enabled =  !isService;

            tbRefresh.Enabled = true;

            tbBack.Enabled = false;
            tbForward.Enabled = false;

            bool isServiceInstalled = false;
            bool isServiceRunning = IsServiceControllerRunning();
            if (isService)
            {
                if (isServiceRunning)
                    isServiceInstalled = true;
                else
                    isServiceInstalled = IsServiceControllerRunning();

                //var sc=Settings.GetServiceInstalled();
                //if (sc != null)
                //{
                //    isServiceInstalled = sc.Installed;// IsServiceControllerInstalled();
                //    if (sc.ServiceController != null)
                //    {
                //        isServiceRunning = sc.ServiceController.Status == ServiceControllerStatus.Running;//. IsServiceControllerRunning();
                //    }
                //}
            }
#if(TEST)
            tbActions.Enabled = true;
#else
            tbActions.Enabled = isServiceRunning;
#endif
            tbPause.Enabled = isServiceInstalled;
            tbRestart.Enabled = isServiceInstalled;
            tbStart.Enabled = isServiceInstalled;
            tbStop.Enabled = isServiceInstalled;
            tbInstall.Enabled = isService;// !isServiceInstalled;
        }

        private void mcManagment_ToolButtonClick(object sender, Nistec.WinForms.ToolButtonClickEventArgs e)
        {

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                switch (e.Button.Name)
                {
                    case "tbActions":
                        {
                            curSubAction = SubActions.Default;
                            this.txtHeader.ReadOnly = true;

                            switch (e.Button.SelectedPopUpItem.Text)
                            {
                                case "Services":
                                    ToolBarSettings(Actions.Services);
                                    this.mcManagment.SelectedPage = pageDetails;
                                    CreateServicesNodeList();
                                    RefreshServiceList();
                                    break;
                                case "RemoteCache":
                                    ToolBarSettings(Actions.RemoteCache);
                                    this.mcManagment.SelectedPage = pgItems;
                                    CreateNodeItems();
                                    return;
                                case "DataCache":
                                    ToolBarSettings(Actions.DataCache);
                                    this.mcManagment.SelectedPage = pgItems;
                                    CreateNodeDataItems();
                                    return;
                                case "SyncDb":
                                    ToolBarSettings(Actions.SyncDb);
                                    this.mcManagment.SelectedPage = pgItems;
                                    CreateNodeSyncItems();
                                    return;
                                case "SessionTree":
                                    ToolBarSettings(Actions.SessionTree);
                                    this.mcManagment.SelectedPage = pgItems;
                                    CreateNodeSession();
                                    return;
                                case "SessionGrid":
                                    ToolBarSettings(Actions.SessionGrid);
                                    this.mcManagment.SelectedPage = pgItems;
                                    CreateNodeSessionGrid();
                                    return;
                                case "TimerReport":
                                    ToolBarSettings(Actions.TimersReport);
                                    this.mcManagment.SelectedPage = pgItems;
                                    CreateNodeTimerItems();
                                    return;
                                    //case "Session":
                                    //    ToolBarSettings(Actions.Session);
                                    //    this.mcManagment.SelectedPage = pgItems;
                                    //    CreateNodeSession();
                                    //    return;
                                    //case "Session-Active":
                                    //    ToolBarSettings(Actions.SessionActive);
                                    //    this.mcManagment.SelectedPage = pgItems;
                                    //    CreateNodeSession(SessionState.Active);
                                    //    return;
                                    //case "Session-Idle":
                                    //    ToolBarSettings(Actions.SessionIdle);
                                    //    this.mcManagment.SelectedPage = pgItems;
                                    //    CreateNodeSession(SessionState.Idle);
                                    //    return;
                            }
                        }
                        break;
                    case "tbStatistic":
                        DoStatistic();
                        return;
                    case "tbItems":
                        //this.mcManagment.SelectedPage = pgItems;
                        DoPerformanceReport();
                        //DoRefresh();
                        return;
                    case "tbUsage":
                        //this.mcManagment.SelectedPage = pgChart;
                        DoUsage();
                        return;
                    case "tbBack":
                        //
                        return;
                    case "tbForward":
                        //
                        return;
                    case "tbRefresh":
                        DoRefresh();
                        return;
                    case "tbStart":
                        DoStart();
                        break;
                    case "tbStop":
                        DoStop();
                        break;
                    case "tbPause":
                        DoPause();
                        break;
                    case "tbRestart":
                        DoRestart();
                        break;
                    case "tbInstall":
                        DoInstall();
                        break;
                    case "tbHelp":
                        if (curSubAction == SubActions.Log)
                        {
                            if (txtHeader.Text == "" || txtHeader.Text == "Cache Log")
                                DoLog();
                            else
                                DoLogFilter(txtHeader.Text);
                        }
                        break;
                    case "tbProperty":
                        DoProperty();
                        break;
                    case "tbRefreshItem":
                        DoRefreshItem();
                        return;
                    case "tbSaveXml":
                        DoSaveXml();
                        return;
                    case "tbLoadXml":
                        DoLoadXml();
                        return;
                    case "tbDelete":
                        DoRemoveItem();
                        break;
                    case "tbAddItem":
                        DoAddItem();
                        break;
                    case "tbLog":
                        DoLog();
                        break;

                }
                //mcManagment.TreeView.SelectedNode = mcManagment.TreeView.Nodes[index];
                //SetServiceStatus(controller);
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message, "Cache Management");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                //switch (e.Button.Name)
                //{
                //    case "tbItems":
                //        curSubAction = SubActions.Performance; break;
                //    case "tbUsage":
                //        curSubAction = SubActions.Usage; break;
                //    case "tbStatistic":
                //        curSubAction = SubActions.Statistic;break;
                //    default:
                //        curSubAction = SubActions.Default;break;
                //}
            }
        }

        private void DoLog()
        {
            //RemoteCacheClient rcc = new RemoteCacheClient();
            curSubAction = SubActions.Log;

            string log = ManagerApi.CacheLog(); //rcc.CacheLog();
            this.mcManagment.SelectedPage = pgSource;
            txtHeader.Text = "";// "Cache Log";
            this.txtHeader.ReadOnly = false;
            txtBody.Text = log;
            mcManagment.TreeView.SelectedNode = null;
        }
        private void DoLogFilter(string text)
        {
            string[] lines = txtBody.Lines;

            //string[] finders = text.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
            ////string log = ManagerApi.CacheLog();
            ////txtBody.Text = log;
            //string[] results = lines;
            //Array.Copy(lines, results,lines.Length);
            //int i = 0;
            //int length = finders.Length;
            //IEnumerable<string> list = results;
            //for (i = 0; i < length; i++)
            //{
            //    string txt = finders[i].Trim();
            //    list = list.Where(s => s.IndexOf(txt, StringComparison.CurrentCultureIgnoreCase) >= 0);
            //}

            var list = lines.Where(s => s.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0);

            StringBuilder sb = new StringBuilder();
            foreach (string s in list)
            {
                sb.AppendLine(s);
            }
            txtBody.Text = sb.ToString();
            mcManagment.TreeView.SelectedNode = null;
        }

        private void DoAddItem()
        {

            //if (curAction == Actions.RemoteCache)
            //{
            //    int status = AddItemDlg.Open();
            //    if (status>0)
            //    {
            //        CreateNodeItems(true);
            //    }

            //    //CacheItem item = AddItemDlg.Open();
            //    //if (!(item == null))//.IsEmpty)
            //    //{
            //    //    RemoteCacheClient rcc = new RemoteCacheClient();
            //    //    rcc.AddItem(item);
            //    //    CreateNodeItems(true);
            //    //}
            //}
            //else if (curAction == Actions.DataCache)
            //{
            //    bool ok = AddDataItemDlg.Open();
            //    if (ok)
            //    {
            //        CreateNodeDataItems(true);
            //    }

            //}

        }
        private void DoRemoveItem()
        {

            if (IsPerformanceSubActions)
            {
                DoRefreshSubAction(true);
                return;
            }

            curSubAction = SubActions.Default;

            string name = GetSelectedItem();
            //RemoteCacheClient rcc = new RemoteCacheClient();

            switch (curAction)
            {
                case Actions.SessionGrid:
                    string sessionId = GetSelectedSessionId();
                    if (sessionId == null)
                        return;
                    if (MsgBox.ShowQuestionYNC("This operation will remove session [" + sessionId + "] include all items in selected session item, Continue", "Remove Item") == DialogResult.Yes)
                    {
                        ManagerApi.SessionApi.RemoveSession(sessionId);
                        RefreshGrid();
                    }
                    break;
                case Actions.SessionTree:
                    //case Actions.Session:
                    //case Actions.SessionActive:
                    //case Actions.SessionIdle:
                    {

                        string tag = GetSelectedTag();
                        if (tag == "SessionTree")
                        {
                            if (name == null)
                                return;
                            if (MsgBox.ShowQuestionYNC("This operation will remove session [" + name + "] include all items in selected session item, Continue", "Remove Item") == DialogResult.Yes)
                            {
                                ManagerApi.SessionApi.RemoveSession(name);
                                CreateNodeSession(true);
                            }
                        }
                        else if (tag != null)
                        {
                            if (MsgBox.ShowQuestionYNC("This operation will remove session [" + tag + "] include all items in selected session item, Continue", "Remove Item") == DialogResult.Yes)
                            {
                                ManagerApi.SessionApi.RemoveSession(tag);
                                CreateNodeSession(true);
                            }
                        }
                    }
                    break;
                case Actions.RemoteCache:
                    {
                        if (name == null)
                            return;
                        if (name.Contains("$"))
                        {
                            MsgBox.ShowInfo("This item can not be deleted");
                            return;
                        }
                        string cacheKey = GetSelectedCacheKey();
                        if (string.IsNullOrEmpty(cacheKey))
                            return;
                        if (MsgBox.ShowQuestionYNC("This will remove item [" + cacheKey + "] from Cache, Continue", "Remove Items") == DialogResult.Yes)
                        {
                            ManagerApi.CacheApi.Remove(cacheKey);
                            RefreshGrid();
                        }

                        //RemoveTreeItem();// (rcc);

                        //if (MsgBox.ShowQuestionYNC("This item will removed from Cache, Continue", "Remove Item") == DialogResult.Yes)
                        //{
                        //    rcc.RemoveItem(name);
                        //    RemoveTreeItem(name);
                        //    //CreateNodeItems(true);
                        //}
                    }
                    break;
                case Actions.DataCache:
                    {
                        if (name == null)
                            return;
                        if (MsgBox.ShowQuestionYNC("This item will remove table [" + name + "] from Cache, Continue", "Remove Item") == DialogResult.Yes)
                        {
                            var keyInfo = ComplexKey.Parse(name);
                            ManagerApi.DataCacheApi.RemoveTable(keyInfo.Prefix, keyInfo.Suffix);// RemoteDataClient.Instance.Remove(name);
                            CreateNodeDataItems(true);
                        }
                    }
                    break;
                case Actions.SyncDb:
                    {
                        if (name == null)
                            return;
                        if (MsgBox.ShowQuestionYNC("This item will remove sync item [" + name + "] from sync Cache, Continue", "Remove Item") == DialogResult.Yes)
                        {
                            ManagerApi.SyncCacheApi.Remove(name);
                            CreateNodeDataItems(true);
                        }
                    }
                    break;
                case Actions.TimersReport:
                    {
                        //do nothing
                    }
                    break;
            }
        }
        private void DoProperty()
        {
            curSubAction = SubActions.Default;

            string name = GetSelectedItem();

            object obj = null;
            string itemName = "";
            //RemoteCacheClient rcc = new RemoteCacheClient();
            if (string.IsNullOrEmpty(name))
            {
                Hashtable h = (Hashtable)ManagerApi.CacheProperties();// rcc.CacheProperties();
                if (h != null)
                {
                    ShowProperty(h, itemName, "Cache Item Property");
                }
                return;
            }


            switch (curAction)
            {
                case Actions.RemoteCache:
                    {
                        //obj = ManagerApi.CacheApi.GetItem(name);//rcc.ViewItem(name);
                        //itemName = "RemoteItem";

                        if (gridItems == null || gridItems.Rows == null || gridItems.Rows.Count == 0)
                        {
                            return;
                        }
                        if (gridItems.CurrentRowIndex < 0)
                        {
                            return;
                        }
                        DataRowView record = gridItems.GetCurrentDataRow();
                        if (record == null)
                        {
                            return;
                        }
                        string key = Types.NZ(record.Row[0], null);
                        if (key == null)
                        {
                            return;
                        }
                        var item = ManagerApi.CacheApi.ViewEntry(key);
                        if (item == null)
                        {
                            return;
                        }
                        var body = item.DecodeBody();
                        if (body == null)
                        {
                            return;
                        }
                        if (SerializeTools.IsSimple(body.GetType()))
                        {
                            var gr = new KeyValueItem()
                            {
                                Key = key,
                                Value = body
                            };// new GenericRecord();

                            obj = gr;
                        }
                        else
                        {
                            obj = body;
                        }
                        itemName = "CacheItem: "+ key;

                    }
                    break;
                case Actions.DataCache:
                    {
                        //if (gridItems == null || gridItems.Rows == null || gridItems.Rows.Count == 0)
                        //{
                        //    return;
                        //}
                        //if (gridItems.CurrentRowIndex < 0)
                        //{
                        //    return;
                        //}
                        //DataRowView record = (DataRowView)gridItems.Rows[gridItems.CurrentRowIndex];
                        //if (record == null)
                        //{
                        //    return;
                        //}
                        //string key = Types.NZ(record.Row[0], null);
                        //if (key == null)
                        //{
                        //    return;
                        //}

                        //var keyInfo = ComplexKey.Get(name, key);
                        //GenericRecord ge = (GenericRecord)ManagerApi.DataCacheApi.GetRecord(keyInfo.Prefix, keyInfo.Suffix,key);
                        //if (ge == null)
                        //    return;
                        //obj = ge.ToDataRow();

                        var keyInfo = ComplexKey.Parse(name);
                        obj = ManagerApi.DataCacheApi.GetItemProperties(keyInfo.Prefix, keyInfo.Suffix);
                        itemName = "RemoteDataItem";
                    }
                    break;
                case Actions.SyncDb:
                    {
                        //if (gridItems == null || gridItems.Rows == null || gridItems.Rows.Count == 0)
                        //{
                        //    return;
                        //}
                        //if (gridItems.CurrentRowIndex < 0)
                        //{
                        //    return;
                        //}
                        //DataRowView record = (DataRowView)gridItems.Rows[gridItems.CurrentRowIndex];
                        //if (record == null)
                        //{
                        //    return;
                        //}
                        //string key = Types.NZ(record.Row[0], null);
                        //if (key == null)
                        //{
                        //    return;
                        //}

                        //var keyInfo = ComplexKey.Get(name, key);
                        ////var keyInfo = ComplexKey.Get(name, ComplexKey.SplitKey(key));
                        //GenericRecord ge = (GenericRecord)ManagerApi.SyncCacheApi.Get(keyInfo, typeof(GenericRecord));
                        //if (ge == null)
                        //    return;
                        //obj = ge.ToDataRow();

                        //var keyInfo = ComplexKey.Parse(name);
                        obj = ManagerApi.SyncCacheApi.GetItemProperties(name);// keyInfo.Prefix, keyInfo.Suffix);
                        itemName = "RemoteSyncItem";
                    }
                    break;
                case Actions.SessionGrid:
                    {
                        var sessionId = GetSelectedSessionId();
                        var key = GetSelectedSessionKey();
                        obj = ManagerApi.SessionApi.ViewSessionItem(sessionId, key);
                        itemName = "SessionItem";
                    }
                    break;
                case Actions.SessionTree:
                    //case Actions.Session:
                    //case Actions.SessionActive:
                    //case Actions.SessionIdle:
                    {
                        string tag = GetSelectedTag();
                        if (tag == "SessionTree")//(tag == "Session" || tag == "SessionActive" || tag == "SessionIdle")
                            return;
                        var node = GetSelectedNode();
                        var sessionId = node.Parent.Text;
                        obj = ManagerApi.SessionApi.ViewSessionItem(sessionId, name);//rcc.ViewItem(name);
                        itemName = "SessionItem";
                    }
                    break;
                case Actions.TimersReport:
                    obj = null;
                    break;
            }
            ShowProperty(obj, itemName, "Cache Item Property");
        }

        private void DoRecordInfo()
        {
            curSubAction = SubActions.Default;

            string name = GetSelectedItem();

            object obj = null;
            string itemName = "";
            //RemoteCacheClient rcc = new RemoteCacheClient();
            if (string.IsNullOrEmpty(name))
            {
                Hashtable h = (Hashtable)ManagerApi.CacheProperties();// rcc.CacheProperties();
                if (h != null)
                {
                    ShowProperty(h, itemName, "Cache Item Property");
                }
                return;
            }


            switch (curAction)
            {
                case Actions.RemoteCache:
                    {
                        //obj = ManagerApi.CacheApi.GetItem(name);//rcc.ViewItem(name);
                        //itemName = "RemoteItem";

                        if (gridItems == null || gridItems.Rows == null || gridItems.Rows.Count == 0)
                        {
                            return;
                        }
                        if (gridItems.CurrentRowIndex < 0)
                        {
                            return;
                        }
                        DataRowView record = gridItems.GetCurrentDataRow();
                        if (record == null)
                        {
                            return;
                        }
                        string key = Types.NZ(record.Row[0], null);
                        if (key == null)
                        {
                            return;
                        }
                        var item = ManagerApi.CacheApi.ViewEntry(key);
                        if (item == null)
                        {
                            return;
                        }
                        var body = item.DecodeBody();
                        if (body == null)
                        {
                            return;
                        }
                        if (SerializeTools.IsSimple(body.GetType()))
                        {
                            var gr = new KeyValueItem()
                            {
                                Key = key,
                                Value = body
                            };// new GenericRecord();

                            obj = gr;
                        }
                        else
                        {
                            obj = body;
                        }
                        itemName = "CacheItem: " + key;

                    }
                    break;
                case Actions.DataCache:
                    {
                        if (gridItems == null || gridItems.Rows == null || gridItems.Rows.Count == 0)
                        {
                            return;
                        }
                        if (gridItems.CurrentRowIndex < 0)
                        {
                            return;
                        }
                        DataRowView record = (DataRowView)gridItems.Rows[gridItems.CurrentRowIndex];
                        if (record == null)
                        {
                            return;
                        }
                        string key = Types.NZ(record.Row[0], null);
                        if (key == null)
                        {
                            return;
                        }

                        var keyInfo = ComplexKey.Parse(name);
                        GenericRecord ge = (GenericRecord)ManagerApi.DataCacheApi.GetRecord(keyInfo.Prefix, keyInfo.Suffix, key);
                        if (ge == null)
                            return;
                        obj = ge.ToDataRow();

                        //var keyInfo = ComplexKey.Parse(name);
                        //obj = ManagerApi.DataCacheApi.GetItemProperties(keyInfo.Prefix, keyInfo.Suffix);
                        //itemName = "RemoteDataItem";
                    }
                    break;
                case Actions.SyncDb:
                    {
                        if (gridItems == null || gridItems.Rows == null || gridItems.Rows.Count == 0)
                        {
                            return;
                        }
                        if (gridItems.CurrentRowIndex < 0)
                        {
                            return;
                        }
                        DataRowView record = (DataRowView)gridItems.Rows[gridItems.CurrentRowIndex];
                        if (record == null)
                        {
                            return;
                        }
                        string key = Types.NZ(record.Row[0], null);
                        if (key == null)
                        {
                            return;
                        }

                        var keyInfo = ComplexKey.Get(name, key);
                        //var keyInfo = ComplexKey.Get(name, ComplexKey.SplitKey(key));
                        GenericRecord ge = (GenericRecord)ManagerApi.SyncCacheApi.GetAs(name, key);//, typeof(GenericRecord));
                        if (ge == null)
                            return;
                        obj = ge.ToDataRow();

                        //obj = ManagerApi.SyncCacheApi.GetItemProperties(name);// keyInfo.Prefix, keyInfo.Suffix);
                        //itemName = "RemoteSyncItem";
                    }
                    break;
                case Actions.SessionGrid:
                    {
                        var sessionId = GetSelectedSessionId();
                        var key = GetSelectedSessionKey();
                        obj = ManagerApi.SessionApi.ViewSessionItem(sessionId, key);
                        itemName = "SessionItem";
                    }
                    break;
                case Actions.SessionTree:
                    //case Actions.Session:
                    //case Actions.SessionActive:
                    //case Actions.SessionIdle:
                    {
                        string tag = GetSelectedTag();
                        if (tag == "SessionTree")//(tag == "Session" || tag == "SessionActive" || tag == "SessionIdle")
                            return;
                        var node = GetSelectedNode();
                        if (node.Parent == null)
                            return;
                        var sessionId = node.Parent.Text;
                        obj = ManagerApi.SessionApi.ViewSessionItem(sessionId, name);//rcc.ViewItem(name);
                        itemName = "SessionItem";
                    }
                    break;
                case Actions.TimersReport:
                    obj = null;
                    break;
            }
            ShowProperty(obj, itemName, "Cache Item Property");
        }
        private void ShowPropertyJson(object obj, string itemName, string text)
        {
            try
            {
                if (obj != null)
                {
                    this.mcManagment.SelectedPage = pgSource;
                    this.txtHeader.Text = itemName + ", " + text;
                    this.txtBody.Text = JsonSerializer.Serialize(obj,true);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
            }
        }
        private void ShowProperty(object obj, string itemName, string text)
        {
            try
            {
                if (obj != null)
                {
                    Nistec.GridView.VGridDlg dlg = new VGridDlg();
                    dlg.Text = text;
                    dlg.ControlLayout = ControlLayout.Visual;
                    dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                    dlg.VGrid.SetDataBinding(obj, itemName);
                    dlg.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
            }

        }
        private void ShowProperty(Hashtable obj, string itemName, string text)
        {
            try
            {
                if (obj != null)
                {
                    Nistec.GridView.VGridDlg dlg = new VGridDlg();
                    dlg.Text = text;
                    dlg.ControlLayout = ControlLayout.Visual;
                    dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                    dlg.VGrid.SetDataBinding(obj, itemName);
                    dlg.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
            }
        }

        private void DoSaveXml()
        {
            try
            {
                string filename = CommonDlg.SaveAs("(*.xml)|*.xml", Environment.CurrentDirectory);
                if (string.IsNullOrEmpty(filename))
                    return;

                //if (curAction == Actions.RemoteCache)
                //{
                //    RemoteCacheClient rcc = new RemoteCacheClient();
                //    rcc.CacheToXml(filename);
                //}
                //else if (curAction == Actions.DataCache)
                //{
                //    RemoteDataClient.Instance.CacheToXmlConfig(filename);
                //}
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }

        private void DoLoadXml()
        {
            try
            {
                string filename = CommonDlg.FileDialog("(*.xml)|*.xml", Environment.CurrentDirectory);
                if (string.IsNullOrEmpty(filename))
                    return;

                //if (curAction == Actions.RemoteCache)
                //{
                //    RemoteCacheClient rcc = new RemoteCacheClient();
                //    rcc.XmlToCache(filename);

                //    CreateNodeItems(true);
                //}
                //else if (curAction == Actions.DataCache)
                //{
                //    RemoteDataClient.Instance.LoadXmlConfigFile(filename);
                //    CreateNodeDataItems(true);
                //}
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }

        private void DoStatistic()
        {
            try
            {
                curSubAction = SubActions.Statistic;

                if (curAction == Actions.RemoteCache)
                {
                    DataTable statistic = ManagerApi.GetStateCounterReport(CacheAgentType.Cache); if (statistic == null)
                        if (statistic == null)
                            return;
                    this.mcManagment.SelectedPage = pgItems;
                    this.gridItems.CaptionText = "Cache Statistic";
                    this.gridItems.DataSource = statistic;
                }
                else if (curAction == Actions.DataCache)
                {
                    DataTable statistic = ManagerApi.GetStateCounterReport(CacheAgentType.DataCache);
                    if (statistic == null)
                        return;
                    this.mcManagment.SelectedPage = pgItems;
                    this.gridItems.CaptionText = "Data Cache Statistic";
                    this.gridItems.DataSource = statistic;

                }
                else if (curAction == Actions.SyncDb)
                {
                    DataTable statistic = ManagerApi.GetStateCounterReport(CacheAgentType.SyncCache);
                    if (statistic == null)
                        return;
                    this.mcManagment.SelectedPage = pgItems;
                    this.gridItems.CaptionText = "sync Cache Statistic";
                    this.gridItems.DataSource = statistic;//.CacheView;

                }
                else if (curAction == Actions.SessionTree || curAction == Actions.SessionGrid)
                //else if (curAction == Actions.Session || curAction == Actions.SessionActive || curAction == Actions.SessionIdle)
                {
                    DataTable statistic = ManagerApi.GetStateCounterReport(CacheAgentType.SessionCache);
                    if (statistic == null)
                        return;
                    this.mcManagment.SelectedPage = pgItems;
                    this.gridItems.CaptionText = "sync Cache Statistic";
                    this.gridItems.DataSource = statistic;//.CacheView;

                }
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }

        private void DoPerformanceReport()
        {
            try
            {
                curSubAction = SubActions.Performance;
                this.mcManagment.SelectedPage = pgItems;
                var report = ManagerApi.GetPerformanceReport();
                if (report != null)
                {
                    this.mcManagment.SelectedPage = pgItems;
                    this.gridItems.CaptionText = "Cache Performance Report";
                    this.gridItems.DataSource = report.PerformanceReport;
                }
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }


        private void DoPerformanceStateReport()
        {
            try
            {

                var report = ManagerApi.GetStateCounterReport();
                if (report != null)
                {
                    this.mcManagment.SelectedPage = pgItems;
                    this.gridItems.CaptionText = "Cache State Counter Report";
                    this.gridItems.DataSource = report;
                }
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }

        private void DoResetPerformanceCounter()
        {
            try
            {

                ManagerApi.ResetPerformanceCounter();
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }

        private void DoClearSelectedItem()
        {
            this.mcManagment.SelectedPage = null;

            switch (lastAction)
            {
                case Actions.SessionGrid:
                    this.txtImageHeader.Text = "";
                    this.txtBody.Text = "";
                    UnWiredGridDropDown(1);
                    this.gridItems.CaptionText = "";
                    this.gridItems.DataSource = null;
                    break;
                case Actions.SessionTree:
                    //case Actions.Session:
                    //case Actions.SessionActive:
                    //case Actions.SessionIdle:
                    this.txtImageHeader.Text = "";
                    this.txtBody.Text = "";
                    break;
                case Actions.RemoteCache:
                    this.txtImageHeader.Text = "";
                    this.txtBody.Text = "";
                    UnWiredGridDropDown(1);
                    this.gridItems.CaptionText = "";
                    this.gridItems.DataSource = null;
                    break;
                case Actions.DataCache:
                    UnWiredGridDropDown(1);
                    this.gridItems.CaptionText = "";
                    this.gridItems.DataSource = null;
                    break;
                case Actions.SyncDb:
                    UnWiredGridDropDown(1);
                    this.gridItems.CaptionText = "";
                    this.gridItems.DataSource = null;
                    break;
                case Actions.TimersReport:
                    this.gridItems.CaptionText = "";
                    this.gridItems.DataSource = null;
                    break;
                case Actions.Services:

                    break;
            }
        }


        private void DoRefreshItem()
        {
            if (curSubAction != SubActions.Default)
            {
                if (IsPerformanceSubActions)
                {
                    DoRefreshSubAction(true);
                    return;
                }
                else if (curSubAction == SubActions.Log)
                {
                    DoLog();
                    return;
                }
            }

            //if (IsPerformanceSubActions)
            //{
            //    DoRefreshSubAction(false);
            //    return;
            //}
            curSubAction = SubActions.Default;

            DoClearSelectedItem();

            lastAction = curAction;

            switch (curAction)
            {
                case Actions.SessionGrid:
                    ShowGridSessionItems();
                    break;
                case Actions.SessionTree:
                    ShowGridTreeSession();
                    break;
                case Actions.RemoteCache:
                    ShowGridItems();
                    break;
                case Actions.DataCache:
                    ShowGridDataItems();
                    break;
                case Actions.SyncDb:
                    ShowGridSyncItems();
                    break;
                case Actions.TimersReport:
                    ShowGridTimerItems();
                    break;
                case Actions.Services:
                    ShowServiceDetails();
                    break;
            }
        }

        private void mcManagment_SelectionNodeChanged(object sender, TreeViewEventArgs e)
        {
            curSubAction = SubActions.Default;
            try
            {
                DoRefreshItem();
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message, "Cache Management");
            }
        }

        private void tbOffset_SelectedItemClick(object sender, Nistec.WinForms.SelectedPopUpItemEvent e)
        {
            //float offset = Types.ToFloat(e.Item.Text, 0F);
            //for (int i = 0; i < ctlPieChart1.Items.Count; i++)
            //{
            //    ctlPieChart1.Items[i].Offset = offset;
            //}

        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            //this.tbOffset.Enabled = this.tabControl.SelectedTab == pgChart;
            //this.tbRefresh.Enabled = this.tabControl.SelectedIndex <= 1;
        }

        #endregion

        #region usage

        //const int maxCapcity = 100000;
        private int maxUsage = 0;
        private int interval = 1000;
        private int tickInterval = 1;// 5;
        private bool refreshStatistic = false;
        private bool isActivated = true;
        private int intervalCount = 0;
        private int intervalSecond = 2;//5

        long curUsage = 0;
        int avgCount = 1;
        long lastUsage = 0;

        DateTime lastUsageTime;
        DateTime lastCounterTime;
        bool chartIntialized = false;

        //CachePerformanceReport performanceReport;

        //CachePerformanceReport GetPerformanceReport()
        //{

        //    if (performanceReport == null || DateTime.Now.Subtract(lastUsageTime).TotalSeconds > 60)
        //    {
        //        performanceReport = ManagerApi.GetPerformanceReport();
        //        avgCount = performanceReport.AvgHitPerMinute;
        //        maxUsage = performanceReport.MaxHitPerMinute;
        //        lastUsageTime = DateTime.Now;
        //    }

        //    return performanceReport;

        //}

        ICachePerformanceReport GetPerformanceReport(string name)
        {
            var agentType = ActionToCacheAgent(name);
            return ManagerApi.GetAgentPerformanceReport(agentType);
        }

        private void SetStatistic()
        {
            string name = curAction.ToString();// GetSelectedItem();
            if (string.IsNullOrEmpty(name))
                return;
            //queueCount = RemoteManager.QueueList.Length;
            //if (queueCount <= 0)
            //    queueCount = 1;
            //RemoteQueue client = new RemoteQueue(name);


            ICachePerformanceReport report = GetPerformanceReport(name);
            if (report != null)
            {
                avgCount = report.AvgHitPerMinute;
                curUsage = report.AvgHitPerMinute;
                maxUsage = report.MaxHitPerMinute;
            }
            if (maxUsage <= 0)
                maxUsage = 1;// / queueCount;
        }

        void FillPerformanceControl()
        {
            if (curAction == Actions.Services)
                return;

            if (mcManagment.SelectedPage != pgChart)
            {
                timer1.Stop();
                timer1.Enabled = false;
                return;
            }

            if (DateTime.Now.Subtract(lastCounterTime).TotalSeconds < intervalSecond)
                return;

            float avgResponseTime = 0;
            float avgSyncTime = 0;
            var performanceReport = ManagerApi.GetPerformanceReport();
            if (performanceReport != null)
            {
                avgCount = performanceReport.AvgHitPerMinute;
                maxUsage = performanceReport.MaxHitPerMinute;
                avgResponseTime = performanceReport.AvgResponseTime;
                avgSyncTime = performanceReport.AvgSyncTime;
            }
            else
            {
                return;
            }
            lastUsage = curUsage;
            curUsage = avgCount;

            if (maxUsage <= 0)
                maxUsage = 1;
            if (curUsage <= 0)
                curUsage = 1;


            this.mcUsage1.Maximum = maxUsage;
            this.mcUsage1.Value1 = (int)curUsage;
            this.mcUsage1.Value2 = (int)lastUsage;

            this.mcUsageHistory1.Maximum = maxUsage;
            this.mcUsageHistory1.AddValues((int)curUsage, (int)lastUsage);
            SetSafeText(this.lblUsage, curUsage.ToString());
            SetSafeText(this.lblUsageHistory, string.Format("Avg Hit Per Minute: {0}, Max Hit Per Minute:{1}, Avg Response Time:{2}", curUsage, maxUsage, Math.Round(avgResponseTime, 5)));//, Math.Round(avgSyncTime, 3)));

            lastCounterTime = DateTime.Now;

            if (DateTime.Now.Subtract(lastUsageTime).TotalSeconds > 30)
            {

                int i = 0;
                List<ICachePerformanceReport> reports = new List<ICachePerformanceReport>();
                foreach (string name in Sections)
                {
                    var agentType = ActionToCacheAgent(name);
                    ICachePerformanceReport report = ManagerApi.GetAgentPerformanceReport(agentType);
                    if (report != null)
                    {
                        reports.Add(report);
                    }
                }


                ctlPieChart1.Items.Clear();

                foreach (var report in reports)
                {
                    string name = report.CounterName;
                    int memoSize = (int)report.MemoSize / 1024;
                    string sizeDesc = string.Format("{0} Kb", memoSize);
                    var item = new PieChartItem(memoSize, GetColor(i), name, name + ": " + memoSize.ToString(), 0);
                    item.Weight = (double)memoSize;
                    item.ToolTipText = name + ":" + sizeDesc;
                    item.PanelText = name + ":" + sizeDesc;

                    ctlPieChart1.Items.Add(item);

                    i++;
                }
                ctlPieChart1.AddChartDescription();

                if (!chartIntialized)
                {
                    InitChart();
                }
            }

            lastUsageTime = DateTime.Now;
        }

        delegate void SetLabelTextCallback(McLabel labl, string text);

        private void SetSafeText(McLabel labl, string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (labl.InvokeRequired)
            {
                SetLabelTextCallback d = new SetLabelTextCallback(SetSafeText);
                this.Invoke(d, new object[] { labl, text });
            }
            else
            {
                labl.Text = text;
            }
        }

        private void DoUsage()
        {
            curSubAction = SubActions.Usage;
            this.mcManagment.SelectedPage = pgChart;
            LoadUsage();
        }
        protected void LoadUsage()
        {
            try
            {
                if (this.timer1.Enabled)
                    return;
                SetStatistic();
            }
            catch (Exception ex)
            {
                //this.statusStrip.Text = ex.Message;
                Nistec.WinForms.MsgDlg.ShowMsg(ex.Message, "Statistic.QueueView");
            }
            //InitChart();
            this.timer1.Interval = interval;
            this.timer1.Enabled = true;
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

        private void RefreshStatistic()
        {
            intervalCount = 0;
            base.AsyncBeginInvoke(null);
        }

        //protected virtual void OnAsyncCancelExecuting(EventArgs e);
        protected override void OnAsyncCompleted(Extension.Nistec.Threading.AsyncCallEventArgs e)
        {
            base.OnAsyncCompleted(e);

            try
            {
                Task.Factory.StartNew(() => FillPerformanceControl());
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
            finally
            {
                base.AsyncDispose();
            }

        }
        //protected virtual void OnAsyncExecutingProgress(AsyncProgressEventArgs e);
        protected override void OnAsyncExecutingWorker(Extension.Nistec.Threading.AsyncCallEventArgs e)
        {
            base.OnAsyncExecutingWorker(e);
            try
            {
                //SetStatistic();
                //--FillPerformanceControl();
            }
            catch { }

        }

        //protected override void OnAsyncExecutingWorker(Nistec.Threading.AsyncCallEventArgs e)
        //{
        //    base.OnAsyncExecutingWorker(e);
        //    try
        //    {
        //        //SetStatistic();
        //        //--FillPerformanceControl();
        //    }
        //    catch { }
        //}

        //protected override void OnAsyncCompleted(Nistec.Threading.AsyncCallEventArgs e)//Nistec.Threading.AsyncCallEventArgs e)
        //{
        //    base.OnAsyncCompleted(e);

        //    try
        //    {
        //        Task.Factory.StartNew(() => FillPerformanceControl());
        //    }
        //    catch (Exception ex)
        //    {
        //        string s = ex.Message;
        //    }
        //    finally
        //    {
        //        base.AsyncDispose();
        //    }
        //}

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                intervalCount++;
                refreshStatistic = intervalCount >= tickInterval;
                if (isActivated)
                {
                    if (refreshStatistic)
                    {
                        //this.statusStrip.Text = "";
                        base.AsyncBeginInvoke(null);
                        intervalCount = 0;
                    }
                    else
                    {
                        Task.Factory.StartNew(() => FillPerformanceControl());

                    }
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        //private void FillUsageControls()
        //{

        //if (maxUsage <= 0)
        //{
        //    CachePerformanceReport report = ManagerApi.GetPerformanceReport();
        //    avgCount = report.AvgHitPerMinute;
        //    maxUsage = report.MaxHitPerMinute;

        //    if (avgCount <= 0)
        //        avgCount = 1;
        //    if (maxUsage <= 0)
        //        maxUsage = avgCount;

        //}

        //avgCount = PerformanceReport.AvgHitPerMinute;
        //maxUsage = PerformanceReport.MaxHitPerMinute;
        //lastUsage = curUsage;
        //curUsage = avgCount;

        //if (maxUsage <= 0)
        //    maxUsage = 1;
        //if (curUsage <= 0)
        //    curUsage = 1;

        //this.mcUsage1.Maximum = maxUsage;
        //this.mcUsage1.Value1 = (int)curUsage;
        //this.mcUsage1.Value2 = (int)lastUsage;

        //this.mcUsageHistory1.Maximum = maxUsage;
        //this.mcUsageHistory1.AddValues((int)curUsage, (int)lastUsage);
        //this.lblUsage.Text = curUsage.ToString();

        //}

        //private void FillControls()
        //{

        //if (Statistic == null)
        //    return;

        //this.ctlLedAll.ScaleValue = QueueItemTotalCount;
        //if (curAction == Actions.Services)
        //    return;

        //FillPerformanceControl();

        //if (!chartIntialized)
        //{
        //    InitChart();
        //}
        //else
        //{
        //    FillChartControl();
        //}

        //int count = this.mcManagment.Items.Count;
        //if (count <= 0)
        //    return;
        //if (ctlPieChart1.Items.Count != count)
        //{
        //    InitChart();
        //}

        //for (int i = 0; i < count; i++)
        //{
        //    string name = this.mcManagment.Items[i].Text;
        //    var agentType = ActionToCacheAgent(name);
        //    CachePerformanceReport report = ManagerApi.GetAgentPerformanceReport(agentType);
        //    if (report == null)
        //        continue;
        //    int requestCount = report.AvgHitPerSecond;
        //    //ctlPieChart1.Items.Add(new PieChartItem((double)queueCount, GetColor(i), name, name + ": " + queueCount.ToString(), 0));
        //    ctlPieChart1.Items[i].Weight = (double)requestCount;
        //    ctlPieChart1.Items[i].ToolTipText = name + ":" + requestCount.ToString();
        //    ctlPieChart1.Items[i].PanelText = name + ":" + requestCount.ToString();

        //}

        //ctlPieChart1.AddChartDescription();
        //}

        CacheAgentType ActionToCacheAgent(string name)
        {
            switch (name)
            {
                //case "Services":
                case "RemoteCache":
                    return CacheAgentType.Cache;
                case "DataCache":
                    return CacheAgentType.DataCache;
                case "SyncDb":
                    return CacheAgentType.SyncCache;
                case "Session":
                case "SessionTree":
                case "SessionGrid":
                    return CacheAgentType.SessionCache;
                default:
                    return CacheAgentType.Cache;
            }
        }


        //void FillChartControl()
        //{
        //    if (DateTime.Now.Subtract(lastUsageTime).TotalSeconds < 30)
        //        return;

        //    lastUsageTime = DateTime.Now;

        //    ctlPieChart1.Items.Clear();

        //    int i = 0;



        //    foreach (string name in Sections)
        //    {
        //        var agentType = ActionToCacheAgent(name);
        //        CachePerformanceReport report = ManagerApi.GetAgentPerformanceReport(agentType);
        //        if (report != null)
        //        {
        //            int memoSize =(int) report.MemoSize/1024;
        //            string sizeDesc = string.Format("{0} KB",memoSize);
        //            var item = new PieChartItem(memoSize, GetColor(i), name, name + ": " + memoSize.ToString(), 0);
        //            item.Weight = (double)memoSize;
        //            item.ToolTipText = name + ":" + sizeDesc;
        //            item.PanelText = name + ":" + sizeDesc;

        //            ctlPieChart1.Items.Add(item);
        //        }
        //        i++;
        //    }
        //    ctlPieChart1.AddChartDescription();
        //}

        private void InitChart()
        {
            //if (curAction == Actions.Services)
            //    return;

            //ctlPieChart1.Items.Clear();

            //int count = this.mcManagment.Items.Count;
            //if (count <= 0)
            //    return;
            //for (int i = 0; i < count; i++)
            //{
            //    string name = this.mcManagment.Items[i].Text;
            //    var agentType = ActionToCacheAgent(name);
            //    CachePerformanceReport report = ManagerApi.GetAgentPerformanceReport(agentType);
            //    int requestCount = report.AvgHitPerSecond;
            //    ctlPieChart1.Items.Add(new PieChartItem(requestCount, GetColor(i), name, name + ": " + requestCount.ToString(), 0));
            //}

            //FillPerformanceControl();

            //ctlPieChart1.Items.Add(new PieChartItem(0, Color.Blue, "Usage", "Usage: " + curItemsUsage.ToString(), 0));

            this.ctlPieChart1.Padding = new System.Windows.Forms.Padding(60, 0, 0, 0);
            this.ctlPieChart1.ItemStyle.SurfaceTransparency = 0.75F;
            this.ctlPieChart1.FocusedItemStyle.SurfaceTransparency = 0.75F;
            this.ctlPieChart1.FocusedItemStyle.SurfaceBrightness = 0.3F;
            this.ctlPieChart1.AddChartDescription();
            this.ctlPieChart1.Leaning = (float)(40 * Math.PI / 180);
            this.ctlPieChart1.Depth = 50;
            this.ctlPieChart1.Radius = 90F;

            chartIntialized = true;
        }

        /*
        
        private bool chartinits = false;

        int curDataUsage = 0;
        int curItemsUsage = 0;
        //long curUsage = 0;
        long curFreeSize = 0;
        private void InitChart()
        {
            //if (useChannels)
            //{
            //    this.useChannels = !string.IsNullOrEmpty(sqlChannels);
            //}

            if (chartinits)
                return;
            ctlPieChart1.Items.Clear();
            //if (Statistic == null)
            //    return;

            ctlPieChart1.Items.Add(new PieChartItem(0, Color.Blue, "Usage", "Usage: " + curItemsUsage.ToString(), 0));
            ctlPieChart1.Items.Add(new PieChartItem(0, Color.Gold, "DataUsage", "DataUsage: " + curDataUsage.ToString(), 0));
            ctlPieChart1.Items.Add(new PieChartItem(0, Color.Green, "FreeSize", "Free: " + curFreeSize.ToString(), 0));
            //ctlPieChart1.Items.Add(new PieChartItem(0, Color.Green, QueueName3, "Queue: " + QueueName3, 0));
            //ctlPieChart1.Items.Add(new PieChartItem(0, Color.Red, QueueName4, "Queue: " + QueueName4, 0));

            this.ctlPieChart1.Padding = new System.Windows.Forms.Padding(60, 0, 0, 0);
            this.ctlPieChart1.ItemStyle.SurfaceTransparency = 0.75F;
            this.ctlPieChart1.FocusedItemStyle.SurfaceTransparency = 0.75F;
            this.ctlPieChart1.FocusedItemStyle.SurfaceBrightness = 0.3F;
            this.ctlPieChart1.AddChartDescription();
            this.ctlPieChart1.Leaning = (float)(40 * Math.PI / 180);
            this.ctlPieChart1.Depth = 50;
            this.ctlPieChart1.Radius = 90F;

            chartinits = true;
        }
        */
        private Color GetColor(int index)
        {

            Color[] colors = new Color[] { Color.Blue, Color.Gold, Color.Green, Color.Red, Color.LightSalmon, Color.YellowGreen, Color.Turquoise, Color.Silver };
            return colors[index % 7];

        }

        #endregion

#if(false)
        #region usage

        CacheView Statistic;

        private int maxUsage = 0;
        private int interval = 1000;

        private int tickInterval = 10;

        private bool refreshStatistic = false;
        private bool isActivated = true;
        private int intervalCount = 0;
        private bool chartinits = false;
        
        int curDataUsage = 0;
        int curItemsUsage = 0;
        long curUsage = 0;
        long curFreeSize = 0;

        protected void LoadUsage()
        {
            try
            {
                if (isActivated && mcManagment.SelectedPage == pgChart)
                {
                    InitChart();

                    //Config();
                    //RemoteCacheClient rcc = new RemoteCacheClient();
                    Statistic = ManagerApi.GetStatistic();//rcc.GetStatistic();
                    DataCacheStatistic ds = DataCacheApi.GetStatistic(null);
                    curDataUsage =(int) ds.Usage;// RemoteDataClient.Instance.Size;
                    curUsage = (int)Statistic.Usage;
                    curItemsUsage = (int)curUsage-curDataUsage;
                    curFreeSize = Statistic.FreeSize;

                    //curItemsUsage = (int)Statistic.Usage;
                    //curUsage = curDataUsage + curItemsUsage;
                    //curFreeSize = Statistic.MaxSize - curUsage;

                }
            }
            catch (Exception ex)
            {
                //this.statusStrip.Text = ex.Message;
                Nistec.WinForms.MsgDlg.ShowMsg(ex.Message, "Statistic.CacheView");
            }
            if (timer1.Enabled == false)
            {
                this.timer1.Interval = interval;
                this.timer1.Enabled = true;
            }
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

        protected override void OnAsyncExecutingWorker(Nistec.Threading.AsyncCallEventArgs e)
        {
            base.OnAsyncExecutingWorker(e);
            try
            {
                //RemoteCacheClient rcc = new RemoteCacheClient();
                Statistic = ManagerApi.GetStatistic();// rcc.GetStatistic();

            }
            catch { }
            finally
            {
                //doRefresh = true;
            }

        }

        protected override void OnAsyncCompleted(Nistec.Threading.AsyncCallEventArgs e)
        {
            base.OnAsyncCompleted(e);

            try
            {
                LoadUsage();
                FillControls();
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
            finally
            {
                base.AsyncDispose();
            }


        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                intervalCount++;
                refreshStatistic = intervalCount >= tickInterval;
                //if (refreshQueueItems)
                //{
                //    AsyncDalStart();
                //    refreshQueueItems = false;
                //}
                if (isActivated && mcManagment.SelectedPage== pgChart)
                {
                    if (refreshStatistic)
                    {
                        //this.statusStrip.Text = "";
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

        private void FillUsageControls()
        {

            if (Statistic == null)
                return;
            //int max = Math.Max(maxUsage, (int)((float)Statistic.CallCount * 1.5F));
            maxUsage = (int)Statistic.MaxSize;

            this.mcUsage1.Maximum = maxUsage;// (int)Statistic.MaxSize;

            this.mcUsageHistory1.Maximum = maxUsage;// (int)Statistic.MaxSize;

            this.mcUsage1.Value1 = (int)curItemsUsage;
            this.mcUsage1.Value2 = (int)curDataUsage;

            this.mcUsageHistory1.AddValues((int)curItemsUsage, (int)curDataUsage);
            //this.ctlUsageHistory2.AddValues(Statistic.CallCount, 0);


            this.lblUsage.Text = curUsage.ToString();
            //this.lblUsageValue2.Text = Statistic.CallCount.ToString();

            //this.lblTotalQueue.Text = QueueItemTotalCount.ToString();

        }

        private void FillControls()
        {

            if (Statistic == null)
                return;
  
            //this.ctlLedAll.ScaleValue = QueueItemTotalCount;


            ctlPieChart1.Items[0].Weight = (double)curItemsUsage;
            ctlPieChart1.Items[1].Weight = (double)curDataUsage;
            ctlPieChart1.Items[2].Weight = (double)curFreeSize;

            ctlPieChart1.Items[0].ToolTipText = "Usage:" + curItemsUsage.ToString();
            ctlPieChart1.Items[1].ToolTipText = "Data Usage:" + curDataUsage.ToString();
            ctlPieChart1.Items[2].ToolTipText = "Free Size:" + curFreeSize.ToString();

            ctlPieChart1.Items[0].PanelText = "Usage:" + curItemsUsage.ToString();
            ctlPieChart1.Items[1].PanelText = "Data Usage:" + curDataUsage.ToString();
            ctlPieChart1.Items[2].PanelText = "Free Size:" + curFreeSize.ToString();

            ctlPieChart1.AddChartDescription();
        }

 
        private void InitChart()
        {
            //if (useChannels)
            //{
            //    this.useChannels = !string.IsNullOrEmpty(sqlChannels);
            //}

            if (chartinits)
                return;
            ctlPieChart1.Items.Clear();
            //if (Statistic == null)
            //    return;

            ctlPieChart1.Items.Add(new PieChartItem(0, Color.Blue, "Usage", "Usage: " + curItemsUsage.ToString(), 0));
            ctlPieChart1.Items.Add(new PieChartItem(0, Color.Gold, "DataUsage", "DataUsage: " + curDataUsage.ToString(), 0));
            ctlPieChart1.Items.Add(new PieChartItem(0, Color.Green, "FreeSize", "Free: " + curFreeSize.ToString(), 0));
            //ctlPieChart1.Items.Add(new PieChartItem(0, Color.Green, QueueName3, "Queue: " + QueueName3, 0));
            //ctlPieChart1.Items.Add(new PieChartItem(0, Color.Red, QueueName4, "Queue: " + QueueName4, 0));

            this.ctlPieChart1.Padding = new System.Windows.Forms.Padding(60, 0, 0, 0);
            this.ctlPieChart1.ItemStyle.SurfaceTransparency = 0.75F;
            this.ctlPieChart1.FocusedItemStyle.SurfaceTransparency = 0.75F;
            this.ctlPieChart1.FocusedItemStyle.SurfaceBrightness = 0.3F;
            this.ctlPieChart1.AddChartDescription();
            this.ctlPieChart1.Leaning = (float)(40 * Math.PI / 180);
            this.ctlPieChart1.Depth = 50;
            this.ctlPieChart1.Radius = 90F;

            chartinits = true;
        }

        #endregion
#endif
    }
}