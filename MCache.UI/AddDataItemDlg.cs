using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using Nistec.Caching.Data;

using Nistec.Win;
using Nistec.WinForms;
using Nistec.GridView;
using Nistec;

namespace Nistec.Caching.Remote.UI
{

 

	/// <summary>
	/// Summary description for SmsSettings.
	/// </summary>
	public class AddDataItemDlg : Nistec.WinForms.McForm
    {
        #region control
        private McLabel lblUsage;
        private McTextBox txtMappingName;
        private McLabel ctlLabel1;
        private McTextBox txtKey;
        private McButton btnOk;
        private McButton btnCancel;
        private McLabel ctlLabel2;
        private McComboBox cbSyncType;
        private McCheckBox isSync;
        private McGroupBox groupBox1;
        private McSpinEdit numMinute;
        private McSpinEdit numHour;
        private McLabel mcLabel1;
        private Grid grid;
        private McButton btnDetails;
        private McButton btnAdd;
        private McTextBox txtSourceName;
        private McLabel mcLabel2;
		private System.ComponentModel.IContainer components=null;

        public AddDataItemDlg()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
        }
        #endregion

        #region Windows Form Designer generated code
        /// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.lblUsage = new Nistec.WinForms.McLabel();
            this.txtMappingName = new Nistec.WinForms.McTextBox();
            this.ctlLabel1 = new Nistec.WinForms.McLabel();
            this.txtKey = new Nistec.WinForms.McTextBox();
            this.btnOk = new Nistec.WinForms.McButton();
            this.btnCancel = new Nistec.WinForms.McButton();
            this.ctlLabel2 = new Nistec.WinForms.McLabel();
            this.cbSyncType = new Nistec.WinForms.McComboBox();
            this.isSync = new Nistec.WinForms.McCheckBox();
            this.groupBox1 = new Nistec.WinForms.McGroupBox();
            this.numMinute = new Nistec.WinForms.McSpinEdit();
            this.numHour = new Nistec.WinForms.McSpinEdit();
            this.mcLabel1 = new Nistec.WinForms.McLabel();
            this.grid = new Nistec.GridView.Grid();
            this.btnDetails = new Nistec.WinForms.McButton();
            this.btnAdd = new Nistec.WinForms.McButton();
            this.txtSourceName = new Nistec.WinForms.McTextBox();
            this.mcLabel2 = new Nistec.WinForms.McLabel();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            // 
            // StyleGuideBase
            // 
            this.StyleGuideBase.AlternatingColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(225)))), ((int)(((byte)(239)))));
            this.StyleGuideBase.BackgroundColor = System.Drawing.Color.AliceBlue;
            this.StyleGuideBase.BorderColor = System.Drawing.Color.SteelBlue;
            this.StyleGuideBase.BorderHotColor = System.Drawing.Color.Blue;
            this.StyleGuideBase.CaptionColor = System.Drawing.Color.SteelBlue;
            this.StyleGuideBase.ColorBrush1 = System.Drawing.Color.LightSteelBlue;
            this.StyleGuideBase.ColorBrush2 = System.Drawing.Color.AliceBlue;
            this.StyleGuideBase.ColorBrushLower = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(174)))), ((int)(((byte)(237)))));
            this.StyleGuideBase.ColorBrushUpper = System.Drawing.Color.AliceBlue;
            this.StyleGuideBase.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.StyleGuideBase.FormColor = System.Drawing.Color.AliceBlue;
            this.StyleGuideBase.StylePlan = Nistec.WinForms.Styles.SteelBlue;
            // 
            // lblUsage
            // 
            this.lblUsage.BackColor = System.Drawing.Color.AliceBlue;
            this.lblUsage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblUsage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lblUsage.ForeColor = System.Drawing.Color.Black;
            this.lblUsage.Location = new System.Drawing.Point(6, 24);
            this.lblUsage.Name = "lblUsage";
            this.lblUsage.Size = new System.Drawing.Size(172, 13);
            this.lblUsage.Text = "Mapping Name";
            this.lblUsage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtMappingName
            // 
            this.txtMappingName.BackColor = System.Drawing.Color.White;
            this.txtMappingName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.txtMappingName.ForeColor = System.Drawing.Color.Black;
            this.txtMappingName.Location = new System.Drawing.Point(6, 39);
            this.txtMappingName.Name = "txtMappingName";
            this.txtMappingName.Size = new System.Drawing.Size(172, 20);
            this.txtMappingName.StylePainter = this.StyleGuideBase;
            this.txtMappingName.TabIndex = 116;
            // 
            // ctlLabel1
            // 
            this.ctlLabel1.BackColor = System.Drawing.Color.AliceBlue;
            this.ctlLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ctlLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.ctlLabel1.ForeColor = System.Drawing.Color.Black;
            this.ctlLabel1.Location = new System.Drawing.Point(17, 49);
            this.ctlLabel1.Name = "ctlLabel1";
            this.ctlLabel1.Size = new System.Drawing.Size(72, 13);
            this.ctlLabel1.Text = "Table Name";
            this.ctlLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtKey
            // 
            this.txtKey.BackColor = System.Drawing.Color.White;
            this.txtKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.txtKey.ForeColor = System.Drawing.Color.Black;
            this.txtKey.Location = new System.Drawing.Point(17, 64);
            this.txtKey.Name = "txtKey";
            this.txtKey.Size = new System.Drawing.Size(178, 20);
            this.txtKey.StylePainter = this.StyleGuideBase;
            this.txtKey.TabIndex = 113;
            // 
            // btnOk
            // 
            this.btnOk.ControlLayout = Nistec.WinForms.ControlLayout.VistaLayout;
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnOk.Location = new System.Drawing.Point(292, 90);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(84, 25);
            this.btnOk.StylePainter = this.StyleGuideBase;
            this.btnOk.TabIndex = 120;
            this.btnOk.Text = "Ok";
            this.btnOk.ToolTipText = "Ok";
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.ControlLayout = Nistec.WinForms.ControlLayout.VistaLayout;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnCancel.Location = new System.Drawing.Point(292, 121);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(84, 25);
            this.btnCancel.StylePainter = this.StyleGuideBase;
            this.btnCancel.TabIndex = 119;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.ToolTipText = "Check Credit";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // ctlLabel2
            // 
            this.ctlLabel2.BackColor = System.Drawing.Color.AliceBlue;
            this.ctlLabel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ctlLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.ctlLabel2.ForeColor = System.Drawing.Color.Black;
            this.ctlLabel2.Location = new System.Drawing.Point(6, 102);
            this.ctlLabel2.Name = "ctlLabel2";
            this.ctlLabel2.Size = new System.Drawing.Size(72, 13);
            this.ctlLabel2.Text = "Sync Type";
            this.ctlLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbSyncType
            // 
            this.cbSyncType.BackColor = System.Drawing.Color.White;
            this.cbSyncType.ButtonToolTip = "";
            this.cbSyncType.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.cbSyncType.ForeColor = System.Drawing.Color.Black;
            this.cbSyncType.IntegralHeight = false;
            this.cbSyncType.Location = new System.Drawing.Point(6, 118);
            this.cbSyncType.Name = "cbSyncType";
            this.cbSyncType.Size = new System.Drawing.Size(172, 20);
            this.cbSyncType.StylePainter = this.StyleGuideBase;
            this.cbSyncType.TabIndex = 115;
            this.cbSyncType.SelectedIndexChanged += new System.EventHandler(this.cbCacheType_SelectedIndexChanged);
            // 
            // isSync
            // 
            this.isSync.BackColor = System.Drawing.Color.AliceBlue;
            this.isSync.ForeColor = System.Drawing.SystemColors.ControlText;
            this.isSync.Location = new System.Drawing.Point(17, 90);
            this.isSync.Name = "isSync";
            this.isSync.Size = new System.Drawing.Size(61, 13);
            this.isSync.TabIndex = 10004;
            this.isSync.Text = "Is Sync";
            this.isSync.CheckedChanged += new System.EventHandler(this.isSync_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.AliceBlue;
            this.groupBox1.Controls.Add(this.txtSourceName);
            this.groupBox1.Controls.Add(this.mcLabel2);
            this.groupBox1.Controls.Add(this.numMinute);
            this.groupBox1.Controls.Add(this.numHour);
            this.groupBox1.Controls.Add(this.mcLabel1);
            this.groupBox1.Controls.Add(this.txtMappingName);
            this.groupBox1.Controls.Add(this.lblUsage);
            this.groupBox1.Controls.Add(this.cbSyncType);
            this.groupBox1.Controls.Add(this.ctlLabel2);
            this.groupBox1.Enabled = false;
            this.groupBox1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.groupBox1.Location = new System.Drawing.Point(17, 113);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.ReadOnly = false;
            this.groupBox1.Size = new System.Drawing.Size(200, 186);
            this.groupBox1.TabIndex = 10005;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Synchronize";
            // 
            // numMinute
            // 
            this.numMinute.BackColor = System.Drawing.Color.White;
            this.numMinute.ButtonAlign = Nistec.WinForms.ButtonAlign.Right;
            this.numMinute.DecimalPlaces = 0;
            this.numMinute.DefaultValue = "";
            this.numMinute.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.numMinute.ForeColor = System.Drawing.Color.Black;
            this.numMinute.Format = "N";
            this.numMinute.FormatType = NumberFormats.StandadNumber;
            this.numMinute.Location = new System.Drawing.Point(66, 158);
            this.numMinute.MaxValue = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numMinute.MinValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numMinute.Name = "numMinute";
            this.numMinute.Size = new System.Drawing.Size(54, 20);
            this.numMinute.StylePainter = this.StyleGuideBase;
            this.numMinute.TabIndex = 10007;
            this.numMinute.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // numHour
            // 
            this.numHour.BackColor = System.Drawing.Color.White;
            this.numHour.ButtonAlign = Nistec.WinForms.ButtonAlign.Right;
            this.numHour.DecimalPlaces = 0;
            this.numHour.DefaultValue = "";
            this.numHour.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.numHour.ForeColor = System.Drawing.Color.Black;
            this.numHour.Format = "N";
            this.numHour.FormatType = NumberFormats.StandadNumber;
            this.numHour.Location = new System.Drawing.Point(6, 158);
            this.numHour.MaxValue = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numHour.MinValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numHour.Name = "numHour";
            this.numHour.Size = new System.Drawing.Size(54, 20);
            this.numHour.StylePainter = this.StyleGuideBase;
            this.numHour.TabIndex = 10005;
            this.numHour.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // mcLabel1
            // 
            this.mcLabel1.BackColor = System.Drawing.Color.AliceBlue;
            this.mcLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.mcLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.mcLabel1.ForeColor = System.Drawing.Color.Black;
            this.mcLabel1.Location = new System.Drawing.Point(6, 143);
            this.mcLabel1.Name = "mcLabel1";
            this.mcLabel1.Size = new System.Drawing.Size(172, 13);
            this.mcLabel1.Text = "Sync Time (hour,minute)";
            this.mcLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // grid
            // 
            this.grid.BackColor = System.Drawing.Color.White;
            this.grid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.grid.CaptionVisible = false;
            this.grid.DataMember = "";
            this.grid.DisableOnLoading = false;
            this.grid.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.grid.ForeColor = System.Drawing.Color.Black;
            this.grid.Location = new System.Drawing.Point(17, 314);
            this.grid.Name = "grid";
            this.grid.Size = new System.Drawing.Size(359, 121);
            this.grid.TabIndex = 10006;
            // 
            // btnDetails
            // 
            this.btnDetails.ControlLayout = Nistec.WinForms.ControlLayout.VistaLayout;
            this.btnDetails.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnDetails.Enabled = false;
            this.btnDetails.Location = new System.Drawing.Point(292, 237);
            this.btnDetails.Name = "btnDetails";
            this.btnDetails.Size = new System.Drawing.Size(84, 25);
            this.btnDetails.StylePainter = this.StyleGuideBase;
            this.btnDetails.TabIndex = 10007;
            this.btnDetails.Text = "Details";
            this.btnDetails.ToolTipText = "Show / Hide DataSource";
            this.btnDetails.Click += new System.EventHandler(this.btnDetails_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.ControlLayout = Nistec.WinForms.ControlLayout.VistaLayout;
            this.btnAdd.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnAdd.Location = new System.Drawing.Point(292, 59);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(84, 25);
            this.btnAdd.StylePainter = this.StyleGuideBase;
            this.btnAdd.TabIndex = 10008;
            this.btnAdd.Text = "Add";
            this.btnAdd.ToolTipText = "Add DataSource";
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // txtSourceName
            // 
            this.txtSourceName.BackColor = System.Drawing.Color.White;
            this.txtSourceName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.txtSourceName.ForeColor = System.Drawing.Color.Black;
            this.txtSourceName.Location = new System.Drawing.Point(6, 78);
            this.txtSourceName.Name = "txtSourceName";
            this.txtSourceName.Size = new System.Drawing.Size(172, 20);
            this.txtSourceName.StylePainter = this.StyleGuideBase;
            this.txtSourceName.TabIndex = 10012;
            // 
            // mcLabel2
            // 
            this.mcLabel2.BackColor = System.Drawing.Color.AliceBlue;
            this.mcLabel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.mcLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.mcLabel2.ForeColor = System.Drawing.Color.Black;
            this.mcLabel2.Location = new System.Drawing.Point(6, 63);
            this.mcLabel2.Name = "mcLabel2";
            this.mcLabel2.Size = new System.Drawing.Size(172, 13);
            this.mcLabel2.Text = "Source Name";
            this.mcLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // AddDataItemDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.CaptionVisible = true;
            this.ClientSize = new System.Drawing.Size(395, 313);
            this.ControlLayout = Nistec.WinForms.ControlLayout.Visual;
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnDetails);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.isSync);
            this.Controls.Add(this.ctlLabel1);
            this.Controls.Add(this.txtKey);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "AddDataItemDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add Data Cache Item";
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.btnOk, 0);
            this.Controls.SetChildIndex(this.txtKey, 0);
            this.Controls.SetChildIndex(this.ctlLabel1, 0);
            this.Controls.SetChildIndex(this.isSync, 0);
            this.Controls.SetChildIndex(this.groupBox1, 0);
            this.Controls.SetChildIndex(this.grid, 0);
            this.Controls.SetChildIndex(this.btnDetails, 0);
            this.Controls.SetChildIndex(this.btnAdd, 0);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

 
	    protected override bool Initialize(object[] args)
		{
			return true;
		}
        

        #region IOfficeControl
        //internal MainWindow owner;

        //public void InitOwner(MainWindow owner)
        //{
        //    this.owner = owner;
        //    SetIStyle();
        //}
        //public void SetIStyle()
        //{
        //    if (owner != null)
        //    {
        //        this.StylePainter = owner.CurrentStylePainter;
        //        foreach (Control c in this.Controls)
        //        {
        //            if (c is Nistec.WinForms.ILayout)
        //            {
        //                ((Nistec.WinForms.ILayout)c).StylePainter = owner.CurrentStylePainter;
        //            }
        //        }
        //    }
        //}
        #endregion

        private bool showDetails = false;
        private DataTable source=null;
        //private string tableName;

        public static bool Open()
        {
            bool ok = false;
            AddDataItemDlg frm = new AddDataItemDlg();
            //frm.LoadSettings(AddItemDlg);
            DialogResult dr= frm.ShowDialog();
            if (dr == DialogResult.OK)
            {
                ok = true;
                frm.Close();
            }
            return ok;
        }

        //public DataTable CacheItem
        //{
        //    get { return source; }
        //}

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!DesignMode)
            {
                this.cbSyncType.Items.AddRange(Enum.GetNames(typeof(SyncType)));
               // this.StyleGuideBase.StylePlan = Cache.IStyle.StylePlan;
            }
        }

        public void StyleGuidChanged(Nistec.WinForms.Styles style)
        {
            this.StylePainter.StylePlan = style;
            foreach (Control c in this.Controls)
            {
                if (c is ILayout) ((ILayout)c).StylePainter = this.StyleGuideBase;
            }
        }

        protected override void OnStylePropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnStylePropertyChanged(e);
            this.BackColor = this.StyleGuideBase.BackgroundColor;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!ValidateItem())
                return;
            try
            {
                CreateItem();
            }
            catch (Exception ex)
            {
                Nistec.WinForms.MsgDlg.ShowDialog(ex.Message, "ERROR");
            }

        }

        private SyncType GetCurrentSyncType()
        {
            return (SyncType)Enum.Parse(typeof(SyncType), this.cbSyncType.Text, true);
        }

        private void CreateItem()
        {
            SyncType syncType = GetCurrentSyncType();
            if (isSync.Checked)
            {
                //RemoteDataClient.Instance.AddDataItem(source, this.txtKey.Text, this.txtMappingName.Text,this.txtSourceName.Text, syncType, new TimeSpan((int)this.numHour.Value, (int)numMinute.Value, 0));
                string[] list = CacheUtil.SplitStrTrim(txtSourceName.Text, this.txtMappingName.Text);
                ManagerApi.DataCacheApi.AddDataItemSync(null, source, this.txtKey.Text, this.txtMappingName.Text, list, syncType, new TimeSpan((int)this.numHour.Value, (int)numMinute.Value, 0));
            }
            else
            {
                //RemoteDataClient.Instance.AddDataItem(source, this.txtKey.Text);
                ManagerApi.DataCacheApi.AddDataItem(null, source, this.txtKey.Text);
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private bool ValidateItem()
        {
            string errorMessage = "";
            bool isValid = true;

            if (txtKey.TextLength == 0)
            {
                isValid = false;
                errorMessage += "\r\nInvalid TableName";
            }
            if (isSync.Checked)
            {
                if (cbSyncType.Text.Length == 0)
                {
                    isValid = false;
                    errorMessage += "\r\nInvalid SyncType";
                }
                if (txtMappingName.TextLength == 0)
                {
                    isValid = false;
                    errorMessage += "\r\nInvalid MappingName";
                }
                if (txtSourceName.TextLength == 0)
                {
                    isValid = false;
                    errorMessage += "\r\nInvalid SourceName";
                }

                SyncType syncType = GetCurrentSyncType();
                TimeSpan time = new TimeSpan((int)numHour.Value, (int)numMinute.Value, 0);
                switch (syncType)
                {
                    case  SyncType.Daily:
                    case  SyncType.Interval:
                        if (time.TotalMinutes==0)
                        {
                            isValid = false;
                            errorMessage += "\r\nTimeSpan should be greater then zero for that SyncType";
                        }
                        break;
                }
            }
            if (!string.IsNullOrEmpty(errorMessage))
            {
                MsgBox.ShowError(errorMessage);
            }
            return isValid;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cbCacheType_SelectedIndexChanged(object sender, EventArgs e)
        {
    
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            if (showDetails)
            {
                this.Height = 313;
            }
            else
            {
                this.Height = 447;
            }
            showDetails = !showDetails;
        }

        private void isSync_CheckedChanged(object sender, EventArgs e)
        {
            this.groupBox1.Enabled = this.isSync.Checked;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                //ADO-
                //Nistec.Ado.UI.AdoWizard wizard = new Nistec.Ado.UI.AdoWizard(Nistec.Ado.UI.ImExMode.Import);
                //if (wizard.ShowDialog() == DialogResult.Yes)
                //{
                //    source = wizard.Source;
                //    tableName = wizard.MappingName;
                //    this.txtKey.Text = tableName;
                //    this.txtMappingName.Text = tableName;
                //    this.txtSourceName.Text = tableName;
                //    this.grid.DataSource = source;
                //}
                //else
                //{
                //    return;
                //}

                //this.btnOk.Enabled = source != null;
                //this.btnDetails.Enabled = this.btnOk.Enabled;
            }
            catch (Exception ex)
            {
                Nistec.WinForms.MsgDlg.ShowDialog(ex.Message, "ERROR");
            }
        }

	}
}

