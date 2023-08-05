using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using INMServer.LicWebService;

namespace INMServer
{
    public partial class FrmRegister : Form
    {
        #region Fields and Properties
        private Point pointMouseOffset = new Point(0, 0);

        private static InmsDataSet.tblKeyListDataTable _tblKeyList;
        private static InmsDataSetTableAdapters.tblKeyListTableAdapter _tblKeyListTableAdapter;
        #endregion

        #region Constructors
        public FrmRegister()
        {
            InitializeComponent();

            // Make our bitmap region for the form

            this.MouseDown += new MouseEventHandler(frmRegister_MouseDown);
            this.MouseMove += new MouseEventHandler(frmRegister_MouseMove);
        }
        #endregion

        #region Internal Methods
        #endregion

        #region Private Methods
        


        private bool GetAllHard(string ypID)
        {
            try
            {
                //string sName = "";
                //string SqlStr = " select CPUID,UKKEY,Status from T_KEY where CPUID='" + ypID + "'";
                //DataSet ds = DbHelperSQL.Query(SqlStr);
                //if (ds != null && ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                //{
                //    DataCrypto cry = new DataCrypto();
                //    sName = cry.Encrypto(ds.Tables[0].Rows[0]["CPUID"].ToString() + ds.Tables[0].Rows[0]["UKKEY"].ToString());

                //    if (sName != ds.Tables[0].Rows[0]["Status"].ToString())
                //        return false;
                //    else
                //        return true;
                //}
                //else
                //{
                //    return false;
                //}
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool GetIpByHostName()
        {
            try
            {
                //Inms.license.LicService lic = new Inms.license.LicService();
                //return lic.HelloWorld();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Event Methods
        private void FrmRegister_Load(object sender, EventArgs e)
        {
            try
            {
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// frmLogon_MouseMove
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void frmRegister_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (!(sender is System.Windows.Forms.Control)) return;

                if (e.Button == MouseButtons.Left)
                {

                    Control objControl = (Control)sender;
                    ScrollableControl objScrollControl = (ScrollableControl)objControl.Parent;
                    Point pointMouse = Control.MousePosition;

                    if (sender.Equals(this))
                    {
                        pointMouse.Offset(pointMouseOffset.X, pointMouseOffset.Y);
                    }
                    else
                    {
                        pointMouse.Offset(pointMouseOffset.X - objScrollControl.DockPadding.Left - objControl.Left
                          , pointMouseOffset.Y - objScrollControl.DockPadding.Top - objControl.Top);
                    }

                    this.Location = pointMouse;
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }            
        }

        /// <summary>
        /// edName_KeyDown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void frmRegister_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                pointMouseOffset = new Point(-e.X, -e.Y);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }            
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            try
            {
                string serialKey = edCode1.Text + edCode2.Text + edCode3.Text + edCode4.Text + edCode5.Text;

                LicServiceClient lic = new LicServiceClient();                

                string strMsg;
                if (lic.LicRequestFirst(out strMsg) == false)
                {
                    lblText.Text = strMsg;
                    this.DialogResult = DialogResult.None;
                    return;
                }
                ActivationInfo actInfo;

                ProductInfo prodInfo = ComMisc.GetProductInfo(serialKey);
                if (lic.LicRequestActivation(out actInfo, prodInfo) == true)
                {
                    if (actInfo.IsActivated == false)
                    {
                        this.DialogResult = DialogResult.None;
                        return;
                    }
                    DataCrypto cry = new DataCrypto();
                    string status = cry.Encrypto(serialKey + actInfo.ActivationCode);

                    _tblKeyList = new InmsDataSet.tblKeyListDataTable();
                    _tblKeyListTableAdapter = new InmsDataSetTableAdapters.tblKeyListTableAdapter();
                    _tblKeyListTableAdapter.Connection = ComMisc.GetDBConnection();

                    _tblKeyListTableAdapter.Fill(_tblKeyList);
                    DataRow[] rows = _tblKeyList.Select("hwinfo='" + prodInfo.CpuId + prodInfo.HdModel + prodInfo.HdSerial + "'");
                    if (rows.Length > 0)
                    {
                        _tblKeyList.DefaultView[0].BeginEdit();                        
                        _tblKeyList.DefaultView[0]["serial"] = serialKey;
                        _tblKeyList.DefaultView[0]["activate"] = actInfo.ActivationCode;
                        _tblKeyList.DefaultView[0]["status"] = status;
                        _tblKeyList.DefaultView[0].EndEdit();

                        _tblKeyListTableAdapter.Update(_tblKeyList);
                    }
                    else
                    {
                        DataRow row = _tblKeyList.NewRow();
                        _tblKeyList.DefaultView[0]["hwinfo"] = prodInfo.CpuId + prodInfo.HdModel + prodInfo.HdSerial;
                        _tblKeyList.DefaultView[0]["serial"] = serialKey;
                        _tblKeyList.DefaultView[0]["activate"] = actInfo.ActivationCode;
                        _tblKeyList.DefaultView[0]["status"] = status;

                        _tblKeyList.Rows.Add(row);
                        _tblKeyListTableAdapter.Update(_tblKeyList);
                    }
                }
                else
                {
                    lblText.Text = actInfo.ActivateDesc;
                    this.DialogResult = DialogResult.None;
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                this.DialogResult = DialogResult.None;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        #endregion                
    }
}
