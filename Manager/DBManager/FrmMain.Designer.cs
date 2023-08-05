namespace DBManager
{
    partial class FrmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.edPort = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.edPassword = new System.Windows.Forms.TextBox();
            this.edUsername = new System.Windows.Forms.TextBox();
            this.edServer = new System.Windows.Forms.TextBox();
            this.btnBackup = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.edEncryptKey = new System.Windows.Forms.TextBox();
            this.chkEncryption = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkDropDatabase = new System.Windows.Forms.CheckBox();
            this.chkConstructSQLIn1Line = new System.Windows.Forms.CheckBox();
            this.chkDeleteTable = new System.Windows.Forms.CheckBox();
            this.btnRestore = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.btnFolder = new System.Windows.Forms.Button();
            this.edFolder = new System.Windows.Forms.TextBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.chkAddDate = new System.Windows.Forms.CheckBox();
            this.edFilename = new System.Windows.Forms.TextBox();
            this.edFileExt = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.chkSave = new System.Windows.Forms.CheckBox();
            this.chkDontDisplay = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.chkDeleteHistory = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.edPort);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.edPassword);
            this.groupBox1.Controls.Add(this.edUsername);
            this.groupBox1.Controls.Add(this.edServer);
            this.groupBox1.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.DarkOliveGreen;
            this.groupBox1.Location = new System.Drawing.Point(25, 45);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(541, 89);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "数据库服务器的设置";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(327, 58);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "密码:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // edPort
            // 
            this.edPort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.edPort.Location = new System.Drawing.Point(373, 22);
            this.edPort.Name = "edPort";
            this.edPort.Size = new System.Drawing.Size(147, 22);
            this.edPort.TabIndex = 1;
            this.edPort.Text = "3306";
            this.edPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.edPort.TextChanged += new System.EventHandler(this.edPort_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(40, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "User Name:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(288, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Server Port:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Server Address:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // edPassword
            // 
            this.edPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.edPassword.Location = new System.Drawing.Point(373, 55);
            this.edPassword.MaxLength = 40;
            this.edPassword.Name = "edPassword";
            this.edPassword.PasswordChar = '●';
            this.edPassword.Size = new System.Drawing.Size(147, 22);
            this.edPassword.TabIndex = 3;
            this.edPassword.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // edUsername
            // 
            this.edUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.edUsername.Location = new System.Drawing.Point(101, 55);
            this.edUsername.Name = "edUsername";
            this.edUsername.ReadOnly = true;
            this.edUsername.Size = new System.Drawing.Size(147, 22);
            this.edUsername.TabIndex = 2;
            this.edUsername.Text = "inmsadmin";
            this.edUsername.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.edUsername.TextChanged += new System.EventHandler(this.edUser_TextChanged);
            // 
            // edServer
            // 
            this.edServer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.edServer.Location = new System.Drawing.Point(101, 22);
            this.edServer.Name = "edServer";
            this.edServer.Size = new System.Drawing.Size(147, 22);
            this.edServer.TabIndex = 0;
            this.edServer.Text = "localhost";
            this.edServer.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.edServer.TextChanged += new System.EventHandler(this.edServer_TextChanged);
            // 
            // btnBackup
            // 
            this.btnBackup.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBackup.ForeColor = System.Drawing.Color.DarkOliveGreen;
            this.btnBackup.Location = new System.Drawing.Point(317, 182);
            this.btnBackup.Name = "btnBackup";
            this.btnBackup.Size = new System.Drawing.Size(75, 24);
            this.btnBackup.TabIndex = 4;
            this.btnBackup.Text = "备份";
            this.btnBackup.UseVisualStyleBackColor = true;
            this.btnBackup.Click += new System.EventHandler(this.btnBackup_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.BackColor = System.Drawing.Color.Transparent;
            this.groupBox4.Controls.Add(this.label14);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.edEncryptKey);
            this.groupBox4.Controls.Add(this.chkEncryption);
            this.groupBox4.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox4.ForeColor = System.Drawing.Color.DarkOliveGreen;
            this.groupBox4.Location = new System.Drawing.Point(398, 320);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(168, 140);
            this.groupBox4.TabIndex = 48;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "备份文件进行加密";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(54, 92);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(59, 13);
            this.label14.TabIndex = 4;
            this.label14.Text = "（可选）";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 116);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(150, 13);
            this.label7.TabIndex = 3;
            this.label7.Text = "注：此键将不会被保存。";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(40, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "密码:";
            // 
            // edEncryptKey
            // 
            this.edEncryptKey.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.edEncryptKey.Location = new System.Drawing.Point(16, 67);
            this.edEncryptKey.Name = "edEncryptKey";
            this.edEncryptKey.ReadOnly = true;
            this.edEncryptKey.Size = new System.Drawing.Size(140, 22);
            this.edEncryptKey.TabIndex = 1;
            this.edEncryptKey.TextChanged += new System.EventHandler(this.edEncryptKey_TextChanged);
            // 
            // chkEncryption
            // 
            this.chkEncryption.AutoSize = true;
            this.chkEncryption.Location = new System.Drawing.Point(16, 25);
            this.chkEncryption.Name = "chkEncryption";
            this.chkEncryption.Size = new System.Drawing.Size(78, 17);
            this.chkEncryption.TabIndex = 0;
            this.chkEncryption.Text = "启用加密";
            this.chkEncryption.UseVisualStyleBackColor = true;
            this.chkEncryption.CheckedChanged += new System.EventHandler(this.chkEncryption_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.Transparent;
            this.groupBox2.Controls.Add(this.chkDeleteHistory);
            this.groupBox2.Controls.Add(this.chkDropDatabase);
            this.groupBox2.Controls.Add(this.chkConstructSQLIn1Line);
            this.groupBox2.Controls.Add(this.chkDeleteTable);
            this.groupBox2.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.ForeColor = System.Drawing.Color.DarkOliveGreen;
            this.groupBox2.Location = new System.Drawing.Point(25, 320);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(367, 140);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "添加到备份文件的以下信息";
            // 
            // chkDropDatabase
            // 
            this.chkDropDatabase.AutoSize = true;
            this.chkDropDatabase.Location = new System.Drawing.Point(17, 81);
            this.chkDropDatabase.Name = "chkDropDatabase";
            this.chkDropDatabase.Size = new System.Drawing.Size(293, 17);
            this.chkDropDatabase.TabIndex = 2;
            this.chkDropDatabase.Text = "删除和重新创建数据库。 （DROP DATABASE）";
            this.chkDropDatabase.UseVisualStyleBackColor = true;
            this.chkDropDatabase.CheckedChanged += new System.EventHandler(this.chkDropDatabase_CheckedChanged);
            // 
            // chkConstructSQLIn1Line
            // 
            this.chkConstructSQLIn1Line.AutoSize = true;
            this.chkConstructSQLIn1Line.Location = new System.Drawing.Point(17, 26);
            this.chkConstructSQLIn1Line.Name = "chkConstructSQLIn1Line";
            this.chkConstructSQLIn1Line.Size = new System.Drawing.Size(270, 17);
            this.chkConstructSQLIn1Line.TabIndex = 0;
            this.chkConstructSQLIn1Line.Text = "建成1号线的所有相同的表的INSERT命令。";
            this.chkConstructSQLIn1Line.UseVisualStyleBackColor = true;
            this.chkConstructSQLIn1Line.CheckedChanged += new System.EventHandler(this.chkConstructSQLIn1Line_CheckedChanged);
            // 
            // chkDeleteTable
            // 
            this.chkDeleteTable.AutoSize = true;
            this.chkDeleteTable.Location = new System.Drawing.Point(17, 54);
            this.chkDeleteTable.Name = "chkDeleteTable";
            this.chkDeleteTable.Size = new System.Drawing.Size(324, 17);
            this.chkDeleteTable.TabIndex = 1;
            this.chkDeleteTable.Text = "删除并重新恢复过程中的所有表。 （DROP TABLE）";
            this.chkDeleteTable.UseVisualStyleBackColor = true;
            this.chkDeleteTable.CheckedChanged += new System.EventHandler(this.chkDeleteTable_CheckedChanged);
            // 
            // btnRestore
            // 
            this.btnRestore.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRestore.ForeColor = System.Drawing.Color.DarkOliveGreen;
            this.btnRestore.Location = new System.Drawing.Point(436, 182);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(75, 24);
            this.btnRestore.TabIndex = 5;
            this.btnRestore.Text = "恢复";
            this.btnRestore.UseVisualStyleBackColor = true;
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.BackColor = System.Drawing.Color.Transparent;
            this.label12.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.ForeColor = System.Drawing.Color.DarkOliveGreen;
            this.label12.Location = new System.Drawing.Point(22, 234);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(85, 13);
            this.label12.TabIndex = 53;
            this.label12.Text = "选择文件夹：";
            // 
            // btnFolder
            // 
            this.btnFolder.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnFolder.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFolder.ForeColor = System.Drawing.Color.DarkOliveGreen;
            this.btnFolder.Image = global::DBManager.Properties.Resources.folder;
            this.btnFolder.Location = new System.Drawing.Point(529, 228);
            this.btnFolder.Name = "btnFolder";
            this.btnFolder.Size = new System.Drawing.Size(28, 28);
            this.btnFolder.TabIndex = 7;
            this.btnFolder.UseVisualStyleBackColor = true;
            this.btnFolder.Click += new System.EventHandler(this.btnFolder_Click);
            // 
            // edFolder
            // 
            this.edFolder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.edFolder.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.edFolder.ForeColor = System.Drawing.Color.DarkOliveGreen;
            this.edFolder.Location = new System.Drawing.Point(113, 231);
            this.edFolder.Name = "edFolder";
            this.edFolder.Size = new System.Drawing.Size(410, 22);
            this.edFolder.TabIndex = 6;
            // 
            // btnCreate
            // 
            this.btnCreate.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCreate.ForeColor = System.Drawing.Color.DarkOliveGreen;
            this.btnCreate.Location = new System.Drawing.Point(88, 182);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(75, 24);
            this.btnCreate.TabIndex = 3;
            this.btnCreate.Text = "建立";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // chkAddDate
            // 
            this.chkAddDate.AutoSize = true;
            this.chkAddDate.BackColor = System.Drawing.Color.Transparent;
            this.chkAddDate.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkAddDate.ForeColor = System.Drawing.Color.DarkOliveGreen;
            this.chkAddDate.Location = new System.Drawing.Point(114, 261);
            this.chkAddDate.Name = "chkAddDate";
            this.chkAddDate.Size = new System.Drawing.Size(426, 17);
            this.chkAddDate.TabIndex = 8;
            this.chkAddDate.Text = "添加后的文件名，日期和时间（现在）。 （yyyy-MM-dd_HH-mm-ss）";
            this.chkAddDate.UseVisualStyleBackColor = false;
            this.chkAddDate.CheckedChanged += new System.EventHandler(this.chkAddDate_CheckedChanged);
            // 
            // edFilename
            // 
            this.edFilename.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.edFilename.Location = new System.Drawing.Point(229, 284);
            this.edFilename.Name = "edFilename";
            this.edFilename.Size = new System.Drawing.Size(151, 20);
            this.edFilename.TabIndex = 9;
            this.edFilename.Text = "inmsdb";
            this.edFilename.TextChanged += new System.EventHandler(this.edFilename_TextChanged);
            // 
            // edFileExt
            // 
            this.edFileExt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.edFileExt.Location = new System.Drawing.Point(502, 283);
            this.edFileExt.Name = "edFileExt";
            this.edFileExt.Size = new System.Drawing.Size(64, 20);
            this.edFileExt.TabIndex = 10;
            this.edFileExt.Text = "sql";
            this.edFileExt.TextChanged += new System.EventHandler(this.edFileExt_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.DarkOliveGreen;
            this.label6.Location = new System.Drawing.Point(110, 287);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(111, 13);
            this.label6.TabIndex = 59;
            this.label6.Text = "修补程序文件名：";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.Color.Transparent;
            this.label8.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.DarkOliveGreen;
            this.label8.Location = new System.Drawing.Point(414, 286);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(85, 13);
            this.label8.TabIndex = 60;
            this.label8.Text = "文件扩展名：";
            // 
            // chkSave
            // 
            this.chkSave.AutoSize = true;
            this.chkSave.BackColor = System.Drawing.Color.Transparent;
            this.chkSave.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkSave.ForeColor = System.Drawing.Color.DarkOliveGreen;
            this.chkSave.Location = new System.Drawing.Point(338, 143);
            this.chkSave.Name = "chkSave";
            this.chkSave.Size = new System.Drawing.Size(104, 17);
            this.chkSave.TabIndex = 2;
            this.chkSave.Text = "自动保存参数";
            this.chkSave.UseVisualStyleBackColor = false;
            this.chkSave.CheckedChanged += new System.EventHandler(this.chkSave_CheckedChanged);
            // 
            // chkDontDisplay
            // 
            this.chkDontDisplay.AutoSize = true;
            this.chkDontDisplay.BackColor = System.Drawing.Color.Transparent;
            this.chkDontDisplay.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkDontDisplay.ForeColor = System.Drawing.Color.DarkOliveGreen;
            this.chkDontDisplay.Location = new System.Drawing.Point(126, 143);
            this.chkDontDisplay.Name = "chkDontDisplay";
            this.chkDontDisplay.Size = new System.Drawing.Size(130, 17);
            this.chkDontDisplay.TabIndex = 1;
            this.chkDontDisplay.Text = "不显示所有文本。";
            this.chkDontDisplay.UseVisualStyleBackColor = false;
            this.chkDontDisplay.CheckedChanged += new System.EventHandler(this.chkDontDisplay_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.BackColor = System.Drawing.Color.Transparent;
            this.groupBox3.Location = new System.Drawing.Point(262, 168);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(304, 46);
            this.groupBox3.TabIndex = 63;
            this.groupBox3.TabStop = false;
            // 
            // chkDeleteHistory
            // 
            this.chkDeleteHistory.AutoSize = true;
            this.chkDeleteHistory.Checked = true;
            this.chkDeleteHistory.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDeleteHistory.Location = new System.Drawing.Point(17, 109);
            this.chkDeleteHistory.Name = "chkDeleteHistory";
            this.chkDeleteHistory.Size = new System.Drawing.Size(208, 17);
            this.chkDeleteHistory.TabIndex = 3;
            this.chkDeleteHistory.Text = "在备份过程中删除历史记录表。";
            this.chkDeleteHistory.UseVisualStyleBackColor = true;
            this.chkDeleteHistory.CheckedChanged += new System.EventHandler(this.chkDeleteHistory_CheckedChanged);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::DBManager.Properties.Resources.bkDbManager;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(594, 472);
            this.Controls.Add(this.chkDontDisplay);
            this.Controls.Add(this.chkSave);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.edFileExt);
            this.Controls.Add(this.edFilename);
            this.Controls.Add(this.chkAddDate);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.btnFolder);
            this.Controls.Add(this.edFolder);
            this.Controls.Add(this.btnRestore);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.btnBackup);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "数据库管理";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox edPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox edPassword;
        private System.Windows.Forms.TextBox edUsername;
        private System.Windows.Forms.TextBox edServer;
        private System.Windows.Forms.Button btnBackup;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox edEncryptKey;
        private System.Windows.Forms.CheckBox chkEncryption;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chkDropDatabase;
        private System.Windows.Forms.CheckBox chkConstructSQLIn1Line;
        private System.Windows.Forms.CheckBox chkDeleteTable;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button btnFolder;
        private System.Windows.Forms.TextBox edFolder;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.CheckBox chkAddDate;
        private System.Windows.Forms.TextBox edFilename;
        private System.Windows.Forms.TextBox edFileExt;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox chkSave;
        private System.Windows.Forms.CheckBox chkDontDisplay;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox chkDeleteHistory;
    }
}

