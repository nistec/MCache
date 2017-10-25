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
            Session,
            SessionActive,
            SessionIdle,
        }
        private enum SubActions
        {
            Default,
            Performance,
            Statistic,
            Usage
        }
        //private int curIndex;
        private Actions curAction = Actions.Services;
        private Actions lastAction = Actions.Services;
        private SubActions curSubAction = SubActions.Default;

        private bool shouldRefresh=true;
        //private System.ServiceProcess.ServiceController[] services;

        const string channelManager = "";
        const string channelServer = "";

        //TreeNode nodeRoot;
        //RemoteQueueClient client;
        //RemoteQueue remote;
        //string[] ObjTypeNodeList=new  string[]{"Default","RemotingData","TextFile","BinaryFile","ImageFile","XmlDocument","SerializeClass","HtmlFile"};

        string[] Sections = new string[] { "RemoteCache", "DataCache", "SyncDb", "Session"};

        #endregion

        #region ctor

        public CacheManagmentForm()
        {
            InitializeComponent();
            this.tbBack.Enabled = false;
            this.tbForward.Enabled = false;
            this.mcManagment.TreeView.ImageList = this.imageList1;
            
            //CreateNodeList();
            //remote = new RemoteQueueClient("");// RemoteQueue.Instance;

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CreateServicesNodeList();
            LoadUsage();
            //Config();
            //LoadDal();
            
            //InitChart();
            //this.timer1.Interval = interval;
            //this.timer1.Enabled = true;
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

        private void ShowGridDataItems(string name)
        {

            try
            {
                DataTable item = ManagerApi.DataCacheApi.GetDataTable(null, name);// RemoteDataClient.Instance.GetDataTable(name);


                if (item == null)//.IsEmpty)
                    return;

                this.mcManagment.SelectedPage = pgItems;
                this.gridItems.CaptionText = item.TableName;
                this.gridItems.DataSource = item;
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
                var reportItem = ManagerApi.SyncCacheApi.GetItemsReport(name);// RemoteDataClient.Instance.GetDataTable(name);


                if (reportItem == null)//.IsEmpty)
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

        #region Items

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
                //RemoteCacheClient rcc = new RemoteCacheClient();
                string[] list = ManagerApi.GetAllKeysIcons();// rcc.GetAllKeysIcons();
                if (list == null)
                    goto Label_Exit;

                //parents
                //TreeNode[] parents=new TreeNode[8];
                //parents[0] = new TreeNode(CacheObjType.Default.ToString(),9,10);
                //parents[1] = new TreeNode(CacheObjType.RemotingData.ToString(), 9, 10);
                //parents[2] = new TreeNode(CacheObjType.TextFile.ToString(), 9, 10);
                //parents[3] = new TreeNode(CacheObjType.BinaryFile.ToString(), 9, 10);
                //parents[4] = new TreeNode(CacheObjType.ImageFile.ToString(), 9, 10);
                //parents[5] = new TreeNode(CacheObjType.XmlDocument.ToString(), 9, 10);
                //parents[6] = new TreeNode(CacheObjType.SerializeClass.ToString(), 9, 10);
                //parents[7] = new TreeNode(CacheObjType.HtmlFile.ToString(), 9, 10);
                //this.mcManagment.Items.AddRange(parents);

                foreach (string s in list)
                {
                    int icon = Types.ToInt( s.Substring(0, 1),0);
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

        //private void RemoveTreeItem(RemoteCacheClient rcc)
        //{
        //    try
        //    {

        //        if (mcManagment.TreeView == null || mcManagment.TreeView.Nodes == null)
        //            return;

        //        TreeNode tn = mcManagment.TreeView.SelectedNode;
        //        if (tn.Parent == null)
        //        {
        //            if (tn.Nodes == null || tn.Nodes.Count==0)
        //            {
        //                return;
        //            }
        //            if (MsgBox.ShowQuestionYNC("This will removed all items from Cache section, Continue", "Remove Items") == DialogResult.Yes)
        //            {
        //                foreach (TreeNode n in tn.Nodes)
        //                {
        //                    rcc.RemoveItem(n.Text);
        //                }
        //                tn.Nodes.Clear();
        //            }
        //        }
        //        else
        //        {
        //            if (MsgBox.ShowQuestionYNC("This item will removed from Cache, Continue", "Remove Item") == DialogResult.Yes)
        //            {
        //                rcc.RemoveItem(tn.Text);
        //                tn.Remove();
        //            }
        //        }

        //        mcManagment.TreeView.Refresh();
        //        shouldRefresh = false;
        //        LoadUsage();
        //    }
        //    catch (Exception ex)
        //    {
        //        MsgBox.ShowError(ex.Message);
        //    }
        //}

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
                            ManagerApi.CacheApi.RemoveItem(n.Text);
                        }
                        tn.Nodes.Clear();
                    }
                }
                else
                {
                    if (MsgBox.ShowQuestionYNC("This item will removed from Cache, Continue", "Remove Item") == DialogResult.Yes)
                    {
                        ManagerApi.CacheApi.RemoveItem(tn.Text);
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
                //RemoteCacheClient rcc = new RemoteCacheClient();
                //CacheItem item = rcc.ViewItem(name);

                Task<CacheEntry> task = new Task<CacheEntry>(() => ManagerApi.CacheApi.ViewItem(name));
                task.Start();
                task.Wait(5000);
                if (!task.IsCompleted)
                {
                    return;
                }
                CacheEntry item = task.Result;// CacheApi.GetItemView(name);


                if (item==null)//.IsEmpty)
                    return;
                //object value = null;

                this.mcManagment.SelectedPage = pgImage;
                this.txtImageHeader.Text = item.PrintHeader();
                object value = item.GetValue();
                this.txtBody.Text =item.BodyToBase64();//.BodyToBase64();//.SerializeBodyToBase64();

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


        private void ShowItemValue(CacheEntry item)
        {
            this.mcManagment.SelectedPage = pgSource;
            this.txtHeader.Text = item.PrintHeader();
            //value = item.Load();
            //value = item.Value;
            object value = item.GetValue();
            if (value != null)
                this.txtBody.Text = value.ToString();
        }
        private void ShowItemRef(CacheEntry item)
        {
            this.vgrid.Fields.Clear();
            this.mcManagment.SelectedPage = pgClass;
            this.vgrid.CaptionText = item.PrintHeader();
            this.vgrid.SetDataBinding(item.GetValue(), item.Key);
        }

        #endregion

        #region Session

        private void CreateNodeSession(bool shouldRefresh)
        {
            this.shouldRefresh = shouldRefresh;
            CreateNodeSession();
        }

        //private void CreateNodeSession()
        //{

        //    //if (!shouldRefresh && curAction == Actions.Session)
        //    //    return;

        //    try
        //    {
        //        DoClearSelectedItem();

        //        mcManagment.TreeView.Nodes.Clear();
        //        mcManagment.ListCaption = "Cache Session Items";

        //        //SessionManager rs = new SessionManager();
        //        ICollection<string> sessionList = ManagerApi.GetAllSessionsKeys();// rs.GetAllSessionsKeys();
        //        //ICollection<SessionItem> sessionItems = rs.GetAllItems();
        //        if (sessionList == null || sessionList.Count == 0)
        //            goto Label_Exit;

        //        Dictionary<string, TreeNode> parents = new Dictionary<string, TreeNode>();
        //        foreach (string item in sessionList)
        //        {
        //            TreeNode t = new TreeNode(item, 9, 10);
        //            t.Tag = "Session";
        //            parents[item] = t;
        //        }

        //        //RemoteCacheClient rcc = new RemoteCacheClient();

        //        ICollection<CacheEntry> cloneItems = ManagerApi.CloneItems(CloneType.Session);//rcc.CloneItems(CloneType.Session);

        //        foreach (CacheEntry pair in cloneItems)
        //        {
        //            string name = (string)pair.Key;
        //            TreeNode t = new TreeNode(name, 0, 1);
        //            t.Tag = pair.GetValue();
        //            string kv = (string)pair.Id;
        //            if (parents.ContainsKey(kv))
        //            {
        //                parents[kv].Nodes.Add(t);
        //            }
        //        }
        //        TreeNode[] range = new TreeNode[parents.Count];
        //        parents.Values.CopyTo(range, 0);
        //        this.mcManagment.Items.AddRange(range);

        //        mcManagment.TreeView.Sort();
        //        shouldRefresh = false;

        //    Label_Exit:
        //        curAction = Actions.Session;
        //    }
        //    catch (Exception ex)
        //    {
        //        MsgBox.ShowError(ex.Message);
        //    }
        //}

        private void CreateNodeSession()
        {


            //if (!shouldRefresh && curAction == action)
            //    return;

            try
            {
                DoClearSelectedItem();

                mcManagment.TreeView.Nodes.Clear();
                mcManagment.ListCaption = "Cache Session Items";

                //SessionManager rs = new SessionManager();
                ICollection<string> sessionList = ManagerApi.GetAllSessionsKeys();
                //ICollection<SessionItem> sessionItems = rs.GetAllItems();

                if (sessionList == null || sessionList.Count == 0)
                    goto Label_Exit;

                Dictionary<string, TreeNode> parents = new Dictionary<string, TreeNode>();
                foreach (string item in sessionList)
                {
                    TreeNode t = new TreeNode(item, 9, 10);
                    t.Tag = "Session";
                    parents[item] = t;

                    ICollection<string> items = ManagerApi.GetSessionsItemsKeys(item);//  rs.GetSessionsItemsKeys(item);
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
                curAction = Actions.Session;
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }

        private void CreateNodeSession(SessionState state)
        {

            Actions action = state == SessionState.Active ? Actions.SessionActive : Actions.SessionIdle;

            //if (!shouldRefresh && curAction == action)
            //    return;

            try
            {
                DoClearSelectedItem();

                mcManagment.TreeView.Nodes.Clear();
                mcManagment.ListCaption = "Cache Session Items";

                //SessionManager rs = new SessionManager();
                ICollection<string> sessionList = ManagerApi.GetAllSessionsStateKeys(state);
                //ICollection<SessionItem> sessionItems = rs.GetAllItems();

                if (sessionList == null || sessionList.Count == 0)
                    goto Label_Exit;

                Dictionary<string, TreeNode> parents = new Dictionary<string, TreeNode>();
                foreach (string item in sessionList)
                {
                    TreeNode t = new TreeNode(item, 9, 10);
                    t.Tag = "Session";
                    parents[item] = t;

                    ICollection<string> items = ManagerApi.GetSessionsItemsKeys(item);//  rs.GetSessionsItemsKeys(item);
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

                //RemoteCacheClient rcc = new RemoteCacheClient();

                //ICollection<CacheItem> cloneItems = rcc.CloneItems(CloneType.Session);

                //foreach (CacheItem pair in cloneItems)
                //{
                //    string name = (string)pair.Key;
                //    TreeNode t = new TreeNode(name, 0, 1);
                //    t.Tag = pair.Value;
                //    string kv = (string)pair.SessionId;
                //    if (parents.ContainsKey(kv))
                //    {
                //        parents[kv].Nodes.Add(t);
                //    }
                //}

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

 
        /*
        private void CreateNodeSessionItems()
        {

            if (!shouldRefresh && curAction == Actions.SessionItems)
                return;

            try
            {
                mcManagment.TreeView.Nodes.Clear();
                mcManagment.ListCaption = "Cache Session Items";

                RemoteSession rs = new RemoteSession();
                ICollection<string> sessionList = rs.GetAllSessionsKeys();
                //ICollection<SessionItem> sessionItems = rs.GetAllItems();

                if (sessionList == null || sessionList.Count == 0)
                    goto Label_Exit;

                Dictionary<string, TreeNode> parents = new Dictionary<string, TreeNode>();
                foreach (string item in sessionList)
                {
                    TreeNode t = new TreeNode(item, 9, 10);
                    t.Tag = "Session";
                    parents[item] = t;

                    ICollection<string> items = rs.GetSessionsItemsKeys(item);
                    foreach (string s in items)
                    {
                        string name = s;
                        TreeNode titem = new TreeNode(name, 0, 1);
                        //t.Tag = pair.Value;
                        string kv = (string)item;
                        if (parents.ContainsKey(kv))
                        {
                            parents[kv].Nodes.Add(t);
                        }
                    }
                }

                
                TreeNode[] range = new TreeNode[parents.Count];
                parents.Values.CopyTo(range, 0);
                this.mcManagment.Items.AddRange(range);

                mcManagment.TreeView.Sort();
                shouldRefresh = false;

            Label_Exit:
                curAction = Actions.SessionItems;
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }
        */
        private void ShowGridSession()
        {
            string tag= GetSelectedTag();
            string text = GetSelectedItem();
            if (text != null)
            {
              
                if (tag == "Session")
                {
                    ShowSessionItem(text);
                }
                //else if (tag == "App")
                //{
                //    ShowAppItem(text);
                //}
                else
                {
                    ShowGridItems(text);
                }
            }
        }

        private void ShowSessionItem(string sessionId)
        {
            //RemoteSession rcc = new RemoteSession(sessionId);
            //SessionItem item =  rcc.GetSession();

            ISessionBagStream item = ManagerApi.SessionApi.GetExistingSession(sessionId);
            if (item == null)//.IsEmpty)
                return;
            this.mcManagment.SelectedPage = pgClass;
            this.vgrid.CaptionText = item.SessionId;
            this.vgrid.SetDataBinding(item, item.SessionId);
        }

        //private void ShowAppItem(string name)
        //{
        //    RemoteApp rcc = new RemoteApp();
        //    SessionItem item = rcc.GetApp(name);
        //    if (item == null)//.IsEmpty)
        //        return;
        //    this.mcManagment.SelectedPage = pgClass;
        //    this.vgrid.CaptionText = item.SessionId;
        //    this.vgrid.SetDataBinding(item, item.SessionId);
        //}
        #endregion

        #region App
/*
        private void CreateNodeApp(bool shouldRefresh)
        {
            this.shouldRefresh = shouldRefresh;
            CreateNodeApp();
        }

        private void CreateNodeApp()
        {

            if (!shouldRefresh && curAction == Actions.App)
                return;

            try
            {
                mcManagment.TreeView.Nodes.Clear();
                mcManagment.ListCaption = "Cache App Items";

                RemoteApp rs = new RemoteApp();
                string[] sessionList = rs.GetAllKeys();
                IDictionary sessionItems = rs.GetAllItems();

                if (sessionList == null || sessionList.Length == 0)
                    goto Label_Exit;

                Dictionary<string, TreeNode> parents = new Dictionary<string, TreeNode>();
                foreach (string item in sessionList)
                {
                    TreeNode t = new TreeNode(item, 9, 10);
                    t.Tag = "App";
                    parents[item] = t;
                }

                foreach (DictionaryEntry  pair in sessionItems)
                {
                    string name =(string) pair.Key;
                    TreeNode t = new TreeNode(name, 0, 1);
                    t.Tag = pair.Value;

                    string kv = (string)pair.Value;
                    if (parents.ContainsKey(kv))
                    {
                        parents[kv].Nodes.Add(t);
                    }
                }
                TreeNode[] range = new TreeNode[parents.Count];
                parents.Values.CopyTo(range, 0);
                this.mcManagment.Items.AddRange(range);

                mcManagment.TreeView.Sort();
                shouldRefresh = false;

            Label_Exit:
                curAction = Actions.App;
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }

        private void ShowGridApp()
        {
            string text = GetSelectedItem();
            if (text != null)
            {
                ShowGridItems(text);
            }
        }
*/
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

        //private void RefreshServiceList2()
        //{
        //    services = ServiceController.GetServices();
        //    mcManagment.TreeView.Nodes.Clear();
        //    mcManagment.ListCaption = "Services";
        //    this.mcManagment.SelectedPage = pageDetails;

        //    foreach (ServiceController s in services)
        //    {
        //        if (s.ServiceName.ToLower().StartsWith("mcontrol"))
        //        {
        //            TreeNode t = new TreeNode(s.DisplayName);
        //            t.Tag = s;
        //            t.ImageIndex = 8;
        //            t.SelectedImageIndex = 8;
        //            //t.StateImageIndex = 8;
        //            this.mcManagment.Items.Add(t);
        //        }
        //    }
        //    mcManagment.TreeView.Sort();
        //    //listBoxServices.DisplayMember = "DisplayName";
        //    //listBoxServices.DataSource = services;

        //}

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
            if (IsPerformanceSubActions)
            {
                DoRefreshSubAction(true);
                return;
            }
            curSubAction = SubActions.Default;

            if (curAction == Actions.RemoteCache)
            {
                CreateNodeItems(true);
                ShowGridItems();
                //return;
            }
            else if (curAction == Actions.Session)
            {
                CreateNodeSession(true);
            }
            else if (curAction == Actions.SessionActive)
            {
                CreateNodeSession(SessionState.Active);
            }
            else if (curAction == Actions.SessionIdle)
            {
                CreateNodeSession(SessionState.Idle);
            }
            //else if (curAction == Actions.SessionItems)
            //{
            //    CreateNodeSessionItems();
            //}
            else if (curAction == Actions.DataCache)
            {
                CreateNodeDataItems(true);
                ShowGridDataItems();
                //return;
            }
            else if (curAction == Actions.SyncDb)
            {
                CreateNodeSyncItems(true);
                ShowGridSyncItems();
                //return;
            }
            else if (curAction == Actions.Services)
            {
                RefreshServiceList();
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
            try{
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
        catch (Exception)
        {
            //MsgBox.ShowError(ex.Message);
        }
        finally
        {
            WaitDlg.EndProgress();
        }
    }

        private void DoStop()
        {
            ServiceController controller =null;
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

            bool isItems = (mode == Actions.RemoteCache || mode == Actions.DataCache || mode == Actions.SyncDb);
            bool isService =  (mode == Actions.Services);
            bool isSession = (mode == Actions.Session || mode == Actions.SessionActive || mode == Actions.SessionIdle /*|| mode== Actions.SessionItems*/);


            tbUsage.Enabled = !isService && !isSession;
            tbItems.Enabled = !isService;
            tbProperty.Enabled = !isService;
            tbRefreshItem.Enabled = !isService;

            tbStatistic.Enabled = !isService && !isSession;
            tbAddItem.Enabled = !isService && !isSession;
            tbDelete.Enabled = !isService;
            tbSaveXml.Enabled = !isService && !isSession;
            tbLoadXml.Enabled = !isService && !isSession;
            tbLog.Enabled = !isService;
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
            tbActions.Enabled = true;// isServiceRunning;
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
                                case "Session":
                                    ToolBarSettings(Actions.Session);
                                    this.mcManagment.SelectedPage = pgItems;
                                    CreateNodeSession();
                                    return;
                                case "Session-Active":
                                    ToolBarSettings(Actions.SessionActive);
                                    this.mcManagment.SelectedPage = pgItems;
                                    CreateNodeSession(SessionState.Active);
                                    return;
                                case "Session-Idle":
                                    ToolBarSettings(Actions.SessionIdle);
                                    this.mcManagment.SelectedPage = pgItems;
                                    CreateNodeSession(SessionState.Idle);
                                    return;
                                //case "SessionItems":
                                //    ToolBarSettings(Actions.SessionItems);
                                //    this.mcManagment.SelectedPage = pgItems;
                                //    CreateNodeSessionItems();
                                //    return;

                                    

                                //case "App":
                                //    ToolBarSettings(Actions.App);
                                //    this.mcManagment.SelectedPage = pgItems;
                                //    CreateNodeApp();
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
                        

                        return;
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
            curSubAction = SubActions.Default;

            string log = ManagerApi.CacheLog(); //rcc.CacheLog();
            this.mcManagment.SelectedPage = pgSource;
            txtHeader.Text = "Cache Log";
            txtBody.Text = log;
        }

        private void DoAddItem()
        {

            if (curAction == Actions.RemoteCache)
            {
                int status = AddItemDlg.Open();
                if (status>0)
                {
                    CreateNodeItems(true);
                }

                //CacheItem item = AddItemDlg.Open();
                //if (!(item == null))//.IsEmpty)
                //{
                //    RemoteCacheClient rcc = new RemoteCacheClient();
                //    rcc.AddItem(item);
                //    CreateNodeItems(true);
                //}
            }
            else if (curAction == Actions.DataCache)
            {
                bool ok = AddDataItemDlg.Open();
                if (ok)
                {
                    CreateNodeDataItems(true);
                }

            }

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
                case Actions.Session:
                case Actions.SessionActive:
                case Actions.SessionIdle:
                    //case Actions.SessionItems:
                    {
                        if (MsgBox.ShowQuestionYNC("This operation will remove all items in selected session item, Continue", "Remove Item") == DialogResult.Yes)
                        {
                            string tag = GetSelectedTag();
                            if (tag == "Session" || tag == "SessionActive" || tag == "SessionIdle")
                                ManagerApi.SessionApi.RemoveSession(name, false);//RemoteSession.Instance(name).RemoveSession();
                            else
                                ManagerApi.SessionApi.RemoveSession(tag, false);//RemoteSession.Instance(tag).RemoveSession();

                            CreateNodeSession(true);
                        }
                    }
                    break;
                case Actions.RemoteCache:
                    {
                        if (name.Contains("$"))
                        {
                            MsgBox.ShowInfo("This item can not be deleted");
                            return;
                        }
                        RemoveTreeItem();// (rcc);

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
                        //RemoveTreeItem(rcc);

                        if (MsgBox.ShowQuestionYNC("This item will removed from Cache, Continue", "Remove Item") == DialogResult.Yes)
                        {
                            ManagerApi.DataCacheApi.RemoveTable(null, name);// RemoteDataClient.Instance.Remove(name);
                            CreateNodeDataItems(true);
                        }
                    }
                    break;
                case Actions.SyncDb:
                    {
                        //RemoveTreeItem(rcc);

                        if (MsgBox.ShowQuestionYNC("This item will removed from sync Cache, Continue", "Remove Item") == DialogResult.Yes)
                        {
                            ManagerApi.SyncCacheApi.RemoveItem(name);// RemoteDataClient.Instance.Remove(name);
                            CreateNodeDataItems(true);
                        }
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
                        obj = ManagerApi.CacheApi.ViewItem(name);//rcc.ViewItem(name);
                        itemName = "RemoteItem";
                    }
                    break;
                case Actions.DataCache:
                    {
                        obj = ManagerApi.DataCacheApi.GetItemProperties(null, name);// RemoteDataClient.Instance.GetItemProperties(name);
                        //obj = RemoteCacheClient.RemoteClient.ViewItem(name);
                        itemName = "RemoteDataItem";
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
                        GenericRecord ge = (GenericRecord)ManagerApi.SyncCacheApi.GetItem(CacheKeyInfo.Get(name, CacheKeyInfo.SplitKey(key)), typeof(GenericRecord));
                        if (ge == null)
                            return;
                        obj = ge.ToDataRow();

                        //obj = SyncCacheApi.GetEntityItems(name);// RemoteDataClient.Instance.GetItemProperties(name);
                        //obj = RemoteCacheClient.RemoteClient.ViewItem(name);
                        itemName = "RemoteSyncItem";
                    }
                    break;
                case Actions.Session:
                case Actions.SessionActive:
                case Actions.SessionIdle:
                    //case Actions.SessionItems:
                    {
                        string tag = GetSelectedTag();
                        if (tag == "Session" || tag == "SessionActive" || tag == "SessionIdle")
                            return;
                        obj = ManagerApi.CacheApi.ViewItem(name);//rcc.ViewItem(name);
                        itemName = "RemoteItem";
                    }
                    break;
            }
            ShowProperty(obj, itemName, "Cache Item Property");
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
            catch(Exception ex) 
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
                else if (curAction == Actions.Session || curAction == Actions.SessionActive || curAction == Actions.SessionIdle)
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
                //case Actions.App:
                case Actions.Session:
                case Actions.SessionActive:
                case Actions.SessionIdle:
                    this.txtImageHeader.Text = "";
                    this.txtBody.Text = "";
                    break;
                case Actions.RemoteCache:
                    this.txtImageHeader.Text = "";
                    this.txtBody.Text = "";
                    break;
                case Actions.DataCache:
                    this.gridItems.CaptionText = "";
                    this.gridItems.DataSource = null;
                    break;
                case Actions.SyncDb:
                    this.gridItems.CaptionText = "";
                    this.gridItems.DataSource = null;
                    break;
                case Actions.Services:

                    break;
            }
        }


        private void DoRefreshItem()
        {

            if (IsPerformanceSubActions)
            {
                DoRefreshSubAction(false);
                return;
            }
            curSubAction = SubActions.Default;

            DoClearSelectedItem();

            lastAction = curAction;

            switch (curAction)
            {
                //case Actions.App:
                case Actions.Session:
                case Actions.SessionActive:
                case Actions.SessionIdle:
                    //case Actions.SessionItems:
                    ShowGridSession();
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
                case Actions.Services:
                    ShowServiceDetails();
                    break;
            }
        }
  
        private void mcManagment_SelectionNodeChanged(object sender, TreeViewEventArgs e)
        {
            curSubAction = SubActions.Default;
            DoRefreshItem();
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
        private int tickInterval =1;// 5;
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

        //private void SetStatistic()
        //{
        //    string name = curAction.ToString();// GetSelectedItem();
        //    if (string.IsNullOrEmpty(name))
        //        return;
        //    //queueCount = RemoteManager.QueueList.Length;
        //    //if (queueCount <= 0)
        //    //    queueCount = 1;
        //    //RemoteQueue client = new RemoteQueue(name);


        //    //CachePerformanceReport report = GetPerformanceReport(name);
        //    //avgCount = report.AvgHitPerMinute;
        //    //curUsage = report.AvgHitPerMinute;
        //    //maxUsage = report.MaxHitPerMinute;
        //    //if (maxUsage <= 0)
        //    //    maxUsage = 1;// / queueCount;
        //}

        void FillPerformanceControl()
        {
            if (curAction == Actions.Services)
                return;
           
            //if (mcManagment.SelectedPage != pgChart)
            //{
            //    timer1.Stop();
            //    timer1.Enabled = false;
            //    return;
            //}
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
                    int memoSize = (int)report.MemoSize;// / 1024;
                    string sizeDesc = string.Format("{0} KB", memoSize);
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
                this.Invoke(d, new object[] { labl,text });
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
               // SetStatistic();
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
        protected virtual void OnAsyncCompleted(Extension.Nistec.Threading.AsyncCallEventArgs e)
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
                        Task.Factory.StartNew(() =>  FillPerformanceControl());

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
            switch(name)
            {
                //case "Services":
                case "RemoteCache":
                    return CacheAgentType.Cache;
                case "DataCache":
                    return CacheAgentType.DataCache;
                case "SyncDb":
                    return CacheAgentType.SyncCache;
                case "Session":
                case "SessionActive":
                case "SessionIdle":
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