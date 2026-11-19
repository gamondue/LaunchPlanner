namespace gamon.LaunchPlanner
{
    partial class frmBackground
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmBackground));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenu1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.modificaFileToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.letturaFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.uscitaToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.modificaFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.letturaFileToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uscitaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.txtLog = new System.Windows.Forms.TextBox();
            this.contextMenu1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenu1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "LaunchPlanner";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
            // 
            // contextMenu1
            // 
            this.contextMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.modificaFileToolStripMenuItem1,
            this.letturaFileToolStripMenuItem,
            this.aboutToolStripMenuItem1,
            this.uscitaToolStripMenuItem1});
            this.contextMenu1.Name = "contextMenu1";
            this.contextMenu1.Size = new System.Drawing.Size(168, 136);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(167, 22);
            this.toolStripMenuItem2.Text = "Modifica eventi";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // modificaFileToolStripMenuItem1
            // 
            this.modificaFileToolStripMenuItem1.Name = "modificaFileToolStripMenuItem1";
            this.modificaFileToolStripMenuItem1.Size = new System.Drawing.Size(167, 22);
            this.modificaFileToolStripMenuItem1.Text = "Modifica file";
            this.modificaFileToolStripMenuItem1.Click += new System.EventHandler(this.modificaFileToolStripMenuItem1_Click);
            // 
            // letturaFileToolStripMenuItem
            // 
            this.letturaFileToolStripMenuItem.Name = "letturaFileToolStripMenuItem";
            this.letturaFileToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.letturaFileToolStripMenuItem.Text = "Esecuzione eventi";
            this.letturaFileToolStripMenuItem.Click += new System.EventHandler(this.letturaFileToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem1
            // 
            this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
            this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(167, 22);
            this.aboutToolStripMenuItem1.Text = "About";
            this.aboutToolStripMenuItem1.Click += new System.EventHandler(this.aboutToolStripMenuItem1_Click);
            // 
            // uscitaToolStripMenuItem1
            // 
            this.uscitaToolStripMenuItem1.Name = "uscitaToolStripMenuItem1";
            this.uscitaToolStripMenuItem1.Size = new System.Drawing.Size(167, 22);
            this.uscitaToolStripMenuItem1.Text = "Uscita";
            this.uscitaToolStripMenuItem1.Click += new System.EventHandler(this.uscitaToolStripMenuItem1_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(317, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.modificaFileToolStripMenuItem,
            this.letturaFileToolStripMenuItem1,
            this.aboutToolStripMenuItem,
            this.uscitaToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(189, 22);
            this.toolStripMenuItem1.Text = "Modifica eventi";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // modificaFileToolStripMenuItem
            // 
            this.modificaFileToolStripMenuItem.Name = "modificaFileToolStripMenuItem";
            this.modificaFileToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.modificaFileToolStripMenuItem.Text = "Modifica file";
            this.modificaFileToolStripMenuItem.Click += new System.EventHandler(this.modificaFileToolStripMenuItem1_Click);
            // 
            // letturaFileToolStripMenuItem1
            // 
            this.letturaFileToolStripMenuItem1.Name = "letturaFileToolStripMenuItem1";
            this.letturaFileToolStripMenuItem1.Size = new System.Drawing.Size(189, 22);
            this.letturaFileToolStripMenuItem1.Text = "Aggionamento da file";
            this.letturaFileToolStripMenuItem1.Click += new System.EventHandler(this.letturaFileToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // uscitaToolStripMenuItem
            // 
            this.uscitaToolStripMenuItem.Name = "uscitaToolStripMenuItem";
            this.uscitaToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.uscitaToolStripMenuItem.Text = "Uscita";
            this.uscitaToolStripMenuItem.Click += new System.EventHandler(this.uscitaToolStripMenuItem_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 5000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(0, 30);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(316, 484);
            this.txtLog.TabIndex = 2;
            // 
            // frmLaunchPlanner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(317, 514);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmLaunchPlanner";
            this.Text = "LaunchPlanner";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.frmLaunchPlanner_Load);
            this.contextMenu1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenu1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uscitaToolStripMenuItem;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripMenuItem uscitaToolStripMenuItem1;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.ToolStripMenuItem letturaFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem letturaFileToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem modificaFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem modificaFileToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
    }
}

