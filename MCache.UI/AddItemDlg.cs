using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using Nistec.Win;
using Nistec.WinForms;
using Nistec;


namespace Nistec.Caching.Remote.UI
{

 

	/// <summary>
	/// Summary description for SmsSettings.
	/// </summary>
	public class AddItemDlg : Nistec.WinForms.McForm
    {
        private McLabel lblUsage;
        private McTextBox txtValue;
        private McLabel ctlLabel1;
        private McTextBox txtKey;
        private McButton btnOk;
        private McButton btnCancel;
        private McLabel ctlLabel2;
        private McComboBox cbCacheType;
        private McLabel mcLabel2;
        private McMultiBox txtUrl;
        private McLabel mcLabel1;
        private McTextBox txtExpiration;
		private System.ComponentModel.IContainer components=null;

        public AddItemDlg()
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.lblUsage = new Nistec.WinForms.McLabel();
            this.txtValue = new Nistec.WinForms.McTextBox();
            this.ctlLabel1 = new Nistec.WinForms.McLabel();
            this.txtKey = new Nistec.WinForms.McTextBox();
            this.btnOk = new Nistec.WinForms.McButton();
            this.btnCancel = new Nistec.WinForms.McButton();
            this.ctlLabel2 = new Nistec.WinForms.McLabel();
            this.cbCacheType = new Nistec.WinForms.McComboBox();
            this.mcLabel2 = new Nistec.WinForms.McLabel();
            this.txtUrl = new Nistec.WinForms.McMultiBox();
            this.mcLabel1 = new Nistec.WinForms.McLabel();
            this.txtExpiration = new Nistec.WinForms.McTextBox();
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
            this.StyleGuideBase.StylePlan = Nistec.WinForms.Styles.Desktop;
            // 
            // lblUsage
            // 
            this.lblUsage.BackColor = System.Drawing.Color.AliceBlue;
            this.lblUsage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblUsage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lblUsage.ForeColor = System.Drawing.Color.Black;
            this.lblUsage.Location = new System.Drawing.Point(17, 122);
            this.lblUsage.Name = "lblUsage";
            this.lblUsage.Size = new System.Drawing.Size(72, 13);
            this.lblUsage.Text = "Value";
            this.lblUsage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtValue
            // 
            this.txtValue.BackColor = System.Drawing.Color.White;
            this.txtValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.txtValue.ForeColor = System.Drawing.Color.Black;
            this.txtValue.Location = new System.Drawing.Point(17, 137);
            this.txtValue.Name = "txtValue";
            this.txtValue.Size = new System.Drawing.Size(104, 20);
            this.txtValue.StylePainter = this.StyleGuideBase;
            this.txtValue.TabIndex = 116;
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
            this.ctlLabel1.Text = "Key";
            this.ctlLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtKey
            // 
            this.txtKey.BackColor = System.Drawing.Color.White;
            this.txtKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.txtKey.ForeColor = System.Drawing.Color.Black;
            this.txtKey.Location = new System.Drawing.Point(17, 64);
            this.txtKey.Name = "txtKey";
            this.txtKey.Size = new System.Drawing.Size(104, 20);
            this.txtKey.StylePainter = this.StyleGuideBase;
            this.txtKey.TabIndex = 113;
            // 
            // btnOk
            // 
            this.btnOk.ControlLayout = Nistec.WinForms.ControlLayout.VistaLayout;
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnOk.Location = new System.Drawing.Point(178, 64);
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
            this.btnCancel.Location = new System.Drawing.Point(178, 96);
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
            this.ctlLabel2.Location = new System.Drawing.Point(17, 85);
            this.ctlLabel2.Name = "ctlLabel2";
            this.ctlLabel2.Size = new System.Drawing.Size(72, 13);
            this.ctlLabel2.Text = "Cache Type";
            this.ctlLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbCacheType
            // 
            this.cbCacheType.BackColor = System.Drawing.Color.White;
            this.cbCacheType.ButtonToolTip = "";
            this.cbCacheType.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.cbCacheType.ForeColor = System.Drawing.Color.Black;
            this.cbCacheType.IntegralHeight = false;
            this.cbCacheType.Location = new System.Drawing.Point(17, 101);
            this.cbCacheType.Name = "cbCacheType";
            this.cbCacheType.Size = new System.Drawing.Size(104, 20);
            this.cbCacheType.StylePainter = this.StyleGuideBase;
            this.cbCacheType.TabIndex = 115;
            this.cbCacheType.SelectedIndexChanged += new System.EventHandler(this.cbCacheType_SelectedIndexChanged);
            // 
            // mcLabel2
            // 
            this.mcLabel2.BackColor = System.Drawing.Color.AliceBlue;
            this.mcLabel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.mcLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.mcLabel2.ForeColor = System.Drawing.Color.Black;
            this.mcLabel2.Location = new System.Drawing.Point(17, 201);
            this.mcLabel2.Name = "mcLabel2";
            this.mcLabel2.Size = new System.Drawing.Size(72, 13);
            this.mcLabel2.Text = "Source";
            this.mcLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtUrl
            // 
            this.txtUrl.BackColor = System.Drawing.Color.White;
            this.txtUrl.ButtonToolTip = "";
            this.txtUrl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.txtUrl.ForeColor = System.Drawing.Color.Black;
            this.txtUrl.Location = new System.Drawing.Point(17, 216);
            this.txtUrl.MultiType = Nistec.WinForms.MultiType.Brows;
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Size = new System.Drawing.Size(245, 20);
            this.txtUrl.StylePainter = this.StyleGuideBase;
            this.txtUrl.TabIndex = 129;
            // 
            // mcLabel1
            // 
            this.mcLabel1.BackColor = System.Drawing.Color.AliceBlue;
            this.mcLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.mcLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.mcLabel1.ForeColor = System.Drawing.Color.Black;
            this.mcLabel1.Location = new System.Drawing.Point(17, 162);
            this.mcLabel1.Name = "mcLabel1";
            this.mcLabel1.Size = new System.Drawing.Size(104, 13);
            this.mcLabel1.Text = "Expiration (minute)";
            this.mcLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtExpiration
            // 
            this.txtExpiration.BackColor = System.Drawing.Color.White;
            this.txtExpiration.DecimalPlaces = 0;
            this.txtExpiration.DefaultValue = "0";
            this.txtExpiration.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.txtExpiration.ForeColor = System.Drawing.Color.Black;
            this.txtExpiration.Format = "F";
            this.txtExpiration.FormatType = Formats.FixNumber;
            this.txtExpiration.Location = new System.Drawing.Point(17, 177);
            this.txtExpiration.Name = "txtExpiration";
            this.txtExpiration.Size = new System.Drawing.Size(104, 20);
            this.txtExpiration.StylePainter = this.StyleGuideBase;
            this.txtExpiration.TabIndex = 10005;
            this.txtExpiration.Text = "0";
            // 
            // AddItemDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.CaptionVisible = true;
            this.ClientSize = new System.Drawing.Size(281, 263);
            this.ControlLayout = Nistec.WinForms.ControlLayout.Visual;
            this.Controls.Add(this.mcLabel1);
            this.Controls.Add(this.txtExpiration);
            this.Controls.Add(this.mcLabel2);
            this.Controls.Add(this.txtUrl);
            this.Controls.Add(this.lblUsage);
            this.Controls.Add(this.txtValue);
            this.Controls.Add(this.ctlLabel1);
            this.Controls.Add(this.txtKey);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.ctlLabel2);
            this.Controls.Add(this.cbCacheType);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "AddItemDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add Cache Item";
            this.Controls.SetChildIndex(this.cbCacheType, 0);
            this.Controls.SetChildIndex(this.ctlLabel2, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.btnOk, 0);
            this.Controls.SetChildIndex(this.txtKey, 0);
            this.Controls.SetChildIndex(this.ctlLabel1, 0);
            this.Controls.SetChildIndex(this.txtValue, 0);
            this.Controls.SetChildIndex(this.lblUsage, 0);
            this.Controls.SetChildIndex(this.txtUrl, 0);
            this.Controls.SetChildIndex(this.mcLabel2, 0);
            this.Controls.SetChildIndex(this.txtExpiration, 0);
            this.Controls.SetChildIndex(this.mcLabel1, 0);
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

        //private CacheItem m_CacheItem;
        private int Status = 0;

        public static int Open()
        {
            AddItemDlg frm = new AddItemDlg();
            //frm.LoadSettings(AddItemDlg);
            DialogResult dr= frm.ShowDialog();
            if (dr == DialogResult.OK)
            {
                frm.Close();
            }
            return frm.Status;//.CacheItem;
        }

        //public CacheItem CacheItem
        //{
        //    get { return m_CacheItem; }
        //}

        private CacheObjType CacheType
        {
            get
            {
                try
                {
                    return (CacheObjType)Enum.Parse(typeof(CacheObjType), this.cbCacheType.Text, true);
                }
                catch
                {
                    return CacheObjType.Default;
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!DesignMode)
            {
                this.cbCacheType.Items.AddRange(Enum.GetNames(typeof(CacheObjType)));
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
                if (Status>0)// !(m_CacheItem==null))//.IsEmpty)
                {
                    DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Nistec.WinForms.MsgDlg.ShowDialog(ex.Message, "ERROR");
            }

        }

        private int GetTimeout()
        {
            return Types.ToInt(txtExpiration.Text, 0);
        }

        private void CreateItem()
        {
            //m_CacheItem = null;// Nistec.Caching.CacheItem.Empty;
            //RemoteCacheClient rcc = new RemoteCacheClient();
           switch (CacheType)
            {
                case CacheObjType.Default:
                    ManagerApi.CacheApi.AddItem(txtKey.Text, txtValue.Text, GetTimeout());
                    Status = 0;
                    //Status = rcc.AddItem(txtKey.Text, txtValue.Text, CacheType, SysObjType.Default);
                    //m_CacheItem = Nistec.Caching.CacheItem.Create(txtKey.Text, txtValue.Text, CacheType, true);
                    break;
                case CacheObjType.BinaryFile:
                case CacheObjType.TextFile:
                case CacheObjType.ImageFile:
                case CacheObjType.XmlDocument:
                case CacheObjType.HtmlFile:
                     ManagerApi.CacheApi.AddItem(txtKey.Text, txtUrl.Text, GetTimeout());// CacheType, SysObjType.Default);
                     Status = 0;
                    //m_CacheItem = Nistec.Caching.CacheItem.Create(txtKey.Text, txtUrl.Text, CacheType, true);
                    break;
                case CacheObjType.RemotingData:
                    //ADO-
                    //Nistec.Ado.UI.AdoWizard wizard = new Nistec.Ado.UI.AdoWizard(Nistec.Ado.UI.ImExMode.Import);
                    //if (wizard.ShowDialog() == DialogResult.Yes)
                    //{
                    //    Status = rcc.AddItem(txtKey.Text, wizard.Source, GetTimeout());//CacheType, SysObjType.Default);
                    //    //m_CacheItem = Nistec.Caching.CacheItem.Create(txtKey.Text, wizard.Source, CacheType, true);
                    //}
                    //else
                    //{
                    //    return;
                    //}

                    break;
            }
            Close();
        }

        private bool ValidateItem()
        {
            string errorMessage = "";
            bool isValid = true;
            CacheObjType type = CacheType;

            if (txtKey.TextLength == 0)
            {
                isValid = false;
                errorMessage += "\r\nInvalid Key";
            }
            if (cbCacheType.Text.Length == 0)
            {
                isValid = false;
                errorMessage += "\r\nInvalid CacheType";
            }
            switch (type)
            {
                case CacheObjType.Default:
                    if (txtValue.TextLength == 0)
                    {
                        isValid = false;
                        errorMessage += "\r\nInvalid Value";
                    }
                    break;
                case CacheObjType.RemotingData:
                    break;
                default:
                    if (type != CacheObjType.RemotingData && txtUrl.Text.Length == 0)
                    {
                        isValid = false;
                        errorMessage += "\r\nInvalid Source";
                    }
                    break;
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
            switch (CacheType)
            {
                case CacheObjType.Default:
                    this.txtValue.Enabled = true;
                    this.txtUrl.Enabled = false;
                    break;
                case CacheObjType.BinaryFile:
                    this.txtValue.Enabled = false;
                    this.txtUrl.Enabled = true;
                    this.txtUrl.Filter = "(*.bin)|*.bin|(*.*)|*.*";
                    break;
                case CacheObjType.TextFile:
                    this.txtValue.Enabled = false;
                    this.txtUrl.Enabled = true;
                    this.txtUrl.Filter = "(*.txt)|*.txt|(*.*)|*.*";
                    break;
                case CacheObjType.ImageFile:
                    this.txtValue.Enabled = false;
                    this.txtUrl.Enabled = true;
                    this.txtUrl.Filter = "(*.Gif)|*.Gif|(*.Jpg)|*.Jpg|(*.jpeg)|*.jpeg|(*.Png)|*.Png|(*.Bmp)|*.Bmp|(*.emf)|*.Emf|(*.Wmf)|*.Wmf|(*.Tiff)|*.Tiff|(*.Ico)|*.Ico|(*.*)|*.*";
                    break;
                case CacheObjType.XmlDocument:
                    this.txtValue.Enabled = false;
                    this.txtUrl.Enabled = true;
                    this.txtUrl.Filter = "(*.xml)|*.xml|(*.*)|*.*";
                    break;
                case CacheObjType.HtmlFile:
                    this.txtValue.Enabled = false;
                    this.txtUrl.Enabled = true;
                    this.txtUrl.Filter = "(*.htm)|*.htm|(*.html)|*.html|(*.mht)|*.mht|(*.*)|*.*";
                    break;
                case CacheObjType.RemotingData:
                    this.txtValue.Enabled = false;
                    this.txtUrl.Enabled = false;
                    break;
            }
        }

	}
}

