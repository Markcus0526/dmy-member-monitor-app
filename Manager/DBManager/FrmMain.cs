using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using MySql.Data.MySqlClient;

namespace DBManager
{
    public partial class FrmMain : Form
    {
        #region Fields and Properties

        private bool Construct_SQL_In_One_Line_From_Same_Table = true;
        private bool DropAndRecreateTable = false;
        private bool DropAndRecreateDatabase = false;
        private bool AddDateToFilename = false;
        private bool DeleteHistoryTable = true;
        private string fileExt = "";
        private bool EnableEncryption = false;
        private string strDBName = "inms2db";

        #endregion

        #region Constructors

        public FrmMain()
        {
            InitializeComponent();
        }

        #endregion

        #region Internal Methods
        #endregion

        #region Private Methods

        private void RefreshData()
        {
            if (!edPort.Focused && edPort.Text.Trim().Length == 0)
                edPort.Text = "3306";

            fileExt = edFileExt.Text.Trim();

            DropAndRecreateDatabase = chkDropDatabase.Checked;
            DropAndRecreateTable = chkDeleteTable.Checked;
            Construct_SQL_In_One_Line_From_Same_Table = chkConstructSQLIn1Line.Checked;
            AddDateToFilename = chkAddDate.Checked;
            DeleteHistoryTable = chkDeleteHistory.Checked;

            if (chkSave.Checked) SaveParameters();

            EnableEncryption = chkEncryption.Checked;
        }

        private void Backup()
        {
            #region Check Required Connection Parameters

            bool okConnect = true;
            string errorMsg = "";
            if (edServer.Text.Length == 0)
            { errorMsg += "数据库服务器没有定义。\r\n"; okConnect = false; }
            if (edUsername.Text.Length == 0)
            { errorMsg += "用户名必须不为空。\r\n"; okConnect = false; }
            if (edPassword.Text.Length == 0)
            { errorMsg += "没有输入口令。\r\n"; okConnect = false; }
            if (!okConnect)
            {
                ToolTip tt = new ToolTip();
                tt.ToolTipTitle = "有些条目是空白。";
                tt.Show(errorMsg, this, edServer.Location.X + edServer.Width, edServer.Location.Y + edServer.Height, 7000);
                ToolTip tt2 = new ToolTip();
                tt2.ToolTipTitle = "不能备份";
                tt2.Show("某些参数丢失。", this, btnBackup.Location, 2000);
                return;
            }
            #endregion

            string backupFile = "";
            string filter = "";
            try
            {
                #region Get the Backup Filename and Path

                if (fileExt != "") filter = "*." + fileExt + "|*." + fileExt;

                if (edFilename.Text.Trim().Length == 0)
                {
                    SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                    saveFileDialog1.Title = "保存数据库备份文件";
                    saveFileDialog1.Filter = filter;
                    if (Directory.Exists(edFolder.Text.Trim()))
                        saveFileDialog1.InitialDirectory = edFolder.Text.Trim();
                    if (DialogResult.OK != saveFileDialog1.ShowDialog())
                        return;
                    backupFile = saveFileDialog1.FileName;
                    if (AddDateToFilename)
                    {
                        if (fileExt != "")
                            backupFile = backupFile.Replace("." + fileExt, DateTime.Now.ToString(" yyyy-MM-dd-HH-mm-ss") + "." + fileExt);
                        else
                            backupFile += DateTime.Now.ToString(" yyyy-MM-dd-HH-mm-ss");
                    }
                }
                else
                {
                    backupFile = edFilename.Text.Trim();
                    if (chkAddDate.Checked) backupFile += DateTime.Now.ToString(" yyyy-MM-dd-HH-mm-ss");
                    if (fileExt.Length != 0) backupFile = backupFile + "." + fileExt;
                    string folderpath = "";
                    if (Directory.Exists(edFolder.Text.Trim()))
                        folderpath = edFolder.Text.Trim();
                    else
                    {
                        FolderBrowserDialog fbd = new FolderBrowserDialog();
                        fbd.Description = "选择保存备份文件的位置。\r\n" + backupFile;
                        if (DialogResult.OK != fbd.ShowDialog())
                            return;
                        folderpath = fbd.SelectedPath;
                    }
                    backupFile = (folderpath + "\\").Replace("\\\\", "\\") + backupFile;
                }
                #endregion

                // Start Backup Process

                MySqlBackupRestore mb = new MySqlBackupRestore(edServer.Text, edUsername.Text, edPassword.Text, strDBName, edPort.Text, "");
                mb.DropAndRecreateDatabase = DropAndRecreateDatabase;
                mb.DropAndRecreateTable = DropAndRecreateTable;
                mb.Construct_SQL_In_One_Line_From_Same_Table = Construct_SQL_In_One_Line_From_Same_Table;

                if (EnableEncryption)
                {
                    mb.EncryptBackupFile = EnableEncryption;
                    mb.EncryptionKey = edEncryptKey.Text;
                }

                mb.Backup(backupFile);

                // End of Backup Process

                MessageBox.Show("备份成功。\n\n你的备份文件创建于:\r\n" + backupFile, "备份", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) // Log any error that occur during the backup process
            {
                string errorMessage = "备份失败。\r\n\r\n" + ex.ToString();
                MessageBox.Show(errorMessage, "Error");
            }
        }

        private void Restore()
        {
            #region Check Required Connection Parameters

            bool okConnect = true;
            string errorMsg = "";
            if (edServer.Text.Length == 0)
            { errorMsg += "数据库服务器没有定义。\r\n"; okConnect = false; }
            if (edUsername.Text.Length == 0)
            { errorMsg += "用户名必须不为空。\r\n"; okConnect = false; }
            if (edPassword.Text.Length == 0)
            { errorMsg += "没有输入口令。\r\n"; okConnect = false; }
            if (!okConnect)
            {
                ToolTip tt = new ToolTip();
                tt.ToolTipTitle = "有些条目是空白。";
                tt.Show(errorMsg, this, edServer.Location.X + edServer.Width, edServer.Location.Y + edServer.Height, 7000);
                ToolTip tt2 = new ToolTip();
                tt2.ToolTipTitle = "不能恢复";
                tt2.Show("某些参数丢失。", this, btnRestore.Location, 2000);
                return;
            }
            #endregion
            // Locate backup file -----------------------------

            string filename = "";
            OpenFileDialog f2 = new OpenFileDialog();
            f2.Title = "开放式数据库备份文件";
            f2.Filter = "*." + fileExt + "|*." + fileExt + "|All Files|*.*";
            if (Directory.Exists(edFolder.Text.Trim()))
                f2.InitialDirectory = edFolder.Text.Trim();
            if (DialogResult.OK != f2.ShowDialog())
                return;

            filename = f2.FileName;

            // End of Locate backup file ----------------------

            try
            {
                MySqlBackupRestore mb = new MySqlBackupRestore(edServer.Text, edUsername.Text, edPassword.Text, "", edPort.Text, "");

                if (EnableEncryption)
                {
                    mb.Restore(filename, true, edEncryptKey.Text);
                }
                else
                {
                    mb.Restore(filename);
                }

                MessageBox.Show("恢复全成。", "恢复");
            }
            catch (Exception ex) // Log any error that occur during the backup process
            {
                string errorMessage = "恢复失败。\r\n\r\n" + ex.ToString();
                MessageBox.Show(errorMessage, "Error");
            }
        }

        void SaveParameters()
        {
            if (!chkSave.Checked) return;

            string a = edServer.Text + ";"
                + edUsername.Text + ";"
                + edPassword.Text + ";"
                + edPort.Text + ";"
                + edFileExt.Text + ";"
                + edFilename.Text + ";"
                + chkDontDisplay.Checked.ToString() + ";"
                + chkConstructSQLIn1Line.Checked.ToString() + ";"
                + chkDeleteTable.Checked.ToString() + ";"
                + chkAddDate.Checked.ToString() + ";"
                + chkDropDatabase.Checked.ToString() + ";"
                + edFolder.Text.Trim();
            string b = Convert.ToBase64String(Encoding.UTF8.GetBytes(a));
            XCryptEngine xe = new XCryptEngine(); xe.InitializeEngine(XCryptEngine.AlgorithmType.Rijndael);
            string c = xe.Encrypt(b);
            File.WriteAllText((Application.StartupPath + "\\" + "parameters").Replace("\\\\", "\\"), c, Encoding.UTF8);
        }

        void LoadParameters()
        {
            try
            {
                chkSave.Checked = false;
                string c = File.ReadAllText((Application.StartupPath + "\\" + "parameters").Replace("\\\\", "\\"), Encoding.UTF8);
                XCryptEngine xe = new XCryptEngine(); xe.InitializeEngine(XCryptEngine.AlgorithmType.Rijndael);
                string b = xe.Decrypt(c);
                string a = Encoding.UTF8.GetString(Convert.FromBase64String(b));
                string[] sa = a.Split(';');
                edServer.Text = sa[0];
                edUsername.Text = sa[1];
                edPassword.Text = sa[2];
                edPort.Text = sa[4];
                edFileExt.Text = sa[5];
                edFilename.Text = sa[6];
                if (sa[7] == "True") chkDontDisplay.Checked = true; else chkDontDisplay.Checked = false;
                if (sa[8] == "True") chkConstructSQLIn1Line.Checked = true; else chkConstructSQLIn1Line.Checked = false;
                if (sa[9] == "True") chkDeleteTable.Checked = true; else chkDeleteTable.Checked = false;
                if (sa[10] == "True") chkAddDate.Checked = true; else chkAddDate.Checked = false;
                if (sa[11] == "True") chkDropDatabase.Checked = true; else chkDropDatabase.Checked = false;
                edFolder.Text = sa[12];
                chkSave.Checked = true;
            }
            catch
            {
                chkSave.Checked = false;
                edServer.Text = "localhost";
                edUsername.Text = "inms2admin";
                edPassword.Text = "";
                edPort.Text = "3306";
                edFileExt.Text = "sql";
                edFilename.Text = "inms2db";
                chkDontDisplay.Checked = false;
                chkConstructSQLIn1Line.Checked = false;
                chkDeleteTable.Checked = false;
                chkAddDate.Checked = true;
                chkDropDatabase.Checked = false;
            }
        }

        #endregion

        #region Event Methods

        private void FrmMain_Load(object sender, EventArgs e)
        {
            LoadParameters();
            if (edFileExt.Text.Length == 0)
                edFileExt.Text = "sql";
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            DialogResult ret = MessageBox.Show("所有数据将被删除。\n\n你想创建新的数据库？\r\n", "创建", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (ret == DialogResult.OK)
            {
                Restore();
            }
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            RefreshData();
            Backup();
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            RefreshData();
            Restore();
        }

        private void btnFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "选择备份文件的位置。";
            if (DialogResult.OK != fbd.ShowDialog())
                return;
            edFolder.Text = fbd.SelectedPath;
        }

        private void edServer_TextChanged(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void edUser_TextChanged(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void edPort_TextChanged(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void edFilename_TextChanged(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void edFileExt_TextChanged(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void chkConstructSQLIn1Line_CheckedChanged(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void chkDeleteTable_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkDropDatabase_CheckedChanged(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void chkEncryption_CheckedChanged(object sender, EventArgs e)
        {
            edEncryptKey.ReadOnly = !chkEncryption.Checked;
            EnableEncryption = chkEncryption.Checked;
        }

        private void chkAddDate_CheckedChanged(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void edEncryptKey_TextChanged(object sender, EventArgs e)
        {

        }

        private void chkDontDisplay_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDontDisplay.Checked)
            {
                edServer.PasswordChar = '●';
                edUsername.PasswordChar = '●';
                edPassword.PasswordChar = '●';
                edPort.PasswordChar = '●';
            }
            else
            {
                edServer.PasswordChar = '\0';
                edUsername.PasswordChar = '\0';
                edPassword.PasswordChar = '●';
                edPort.PasswordChar = '\0';
            }
            if (chkSave.Checked) SaveParameters();
        }

        private void chkSave_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSave.Checked) SaveParameters();
            else
            {
                if (File.Exists((Application.StartupPath + "\\" + "parameters").Replace("\\\\", "\\")))
                    File.Delete((Application.StartupPath + "\\" + "parameters").Replace("\\\\", "\\"));
                LoadParameters();
            }
        }

        private void chkDeleteHistory_CheckedChanged(object sender, EventArgs e)
        {
            RefreshData();
        }
        #endregion        
    }
}
