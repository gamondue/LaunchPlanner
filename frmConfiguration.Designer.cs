namespace gamon.LaunchPlanner
{
    partial class frmConfiguration
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmConfiguration));
            this.dgrdEvents = new System.Windows.Forms.DataGridView();
            this.istanteInizio = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IstanteFine = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Durata = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Periodo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Descrizione = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.URI = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Abilitato = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnErase = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.cmbPeriods = new System.Windows.Forms.ComboBox();
            this.dtpkStart = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtURI = new System.Windows.Forms.TextBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.chkEnabled = new System.Windows.Forms.CheckBox();
            this.dtpkEnd = new System.Windows.Forms.DateTimePicker();
            this.txtDaysDuration = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtHoursDuration = new System.Windows.Forms.TextBox();
            this.txtMinutesDuration = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.txtSecondsDuration = new System.Windows.Forms.TextBox();
            this.btnTest = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.txtSecondsPeriod = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.txtMinutesPeriod = new System.Windows.Forms.TextBox();
            this.txtHoursPeriod = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtDaysPeriod = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgrdEvents)).BeginInit();
            this.SuspendLayout();
            // 
            // dgrdEvents
            // 
            this.dgrdEvents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgrdEvents.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgrdEvents.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgrdEvents.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.istanteInizio,
            this.IstanteFine,
            this.Durata,
            this.Periodo,
            this.Descrizione,
            this.URI,
            this.Abilitato});
            this.dgrdEvents.Location = new System.Drawing.Point(0, 148);
            this.dgrdEvents.Name = "dgrdEvents";
            this.dgrdEvents.ReadOnly = true;
            this.dgrdEvents.Size = new System.Drawing.Size(888, 248);
            this.dgrdEvents.TabIndex = 0;
            this.dgrdEvents.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgrdEvents_CellClick);
            this.dgrdEvents.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgrdEvents_CellContentClick);
            // 
            // istanteInizio
            // 
            this.istanteInizio.HeaderText = "Istante Inizio";
            this.istanteInizio.Name = "istanteInizio";
            this.istanteInizio.ReadOnly = true;
            this.istanteInizio.Width = 91;
            // 
            // IstanteFine
            // 
            this.IstanteFine.HeaderText = "Istante Fine";
            this.IstanteFine.Name = "IstanteFine";
            this.IstanteFine.ReadOnly = true;
            this.IstanteFine.Width = 87;
            // 
            // Durata
            // 
            this.Durata.HeaderText = "Durata";
            this.Durata.Name = "Durata";
            this.Durata.ReadOnly = true;
            this.Durata.Width = 64;
            // 
            // Periodo
            // 
            this.Periodo.HeaderText = "Periodo";
            this.Periodo.Name = "Periodo";
            this.Periodo.ReadOnly = true;
            this.Periodo.Width = 68;
            // 
            // Descrizione
            // 
            this.Descrizione.HeaderText = "Descrizione";
            this.Descrizione.Name = "Descrizione";
            this.Descrizione.ReadOnly = true;
            this.Descrizione.Width = 87;
            // 
            // URI
            // 
            this.URI.HeaderText = "URI";
            this.URI.Name = "URI";
            this.URI.ReadOnly = true;
            this.URI.Width = 51;
            // 
            // Abilitato
            // 
            this.Abilitato.HeaderText = "Abilitato";
            this.Abilitato.Name = "Abilitato";
            this.Abilitato.ReadOnly = true;
            this.Abilitato.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Abilitato.Width = 69;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(12, 105);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(65, 37);
            this.btnAdd.TabIndex = 7;
            this.btnAdd.Text = "Aggiungi";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnErase
            // 
            this.btnErase.Location = new System.Drawing.Point(154, 105);
            this.btnErase.Name = "btnErase";
            this.btnErase.Size = new System.Drawing.Size(65, 37);
            this.btnErase.TabIndex = 8;
            this.btnErase.Text = "Elimina";
            this.btnErase.UseVisualStyleBackColor = true;
            this.btnErase.Click += new System.EventHandler(this.btnErase_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(809, 105);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(65, 37);
            this.btnClose.TabIndex = 9;
            this.btnClose.Text = "Chiudi";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // cmbPeriods
            // 
            this.cmbPeriods.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPeriods.FormattingEnabled = true;
            this.cmbPeriods.Location = new System.Drawing.Point(583, 22);
            this.cmbPeriods.Name = "cmbPeriods";
            this.cmbPeriods.Size = new System.Drawing.Size(131, 21);
            this.cmbPeriods.TabIndex = 4;
            this.cmbPeriods.SelectedIndexChanged += new System.EventHandler(this.cmbPeriodi_SelectedIndexChanged);
            // 
            // dtpkStart
            // 
            this.dtpkStart.CustomFormat = "dd MMM yyyy HH:mm:ss";
            this.dtpkStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpkStart.Location = new System.Drawing.Point(12, 24);
            this.dtpkStart.Name = "dtpkStart";
            this.dtpkStart.Size = new System.Drawing.Size(148, 20);
            this.dtpkStart.TabIndex = 1;
            this.dtpkStart.ValueChanged += new System.EventHandler(this.dtpkInizio_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(580, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Periodo";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Inizio";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(163, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(27, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Fine";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(383, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Durata";
            // 
            // txtURI
            // 
            this.txtURI.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtURI.Location = new System.Drawing.Point(80, 79);
            this.txtURI.Name = "txtURI";
            this.txtURI.Size = new System.Drawing.Size(796, 20);
            this.txtURI.TabIndex = 6;
            this.txtURI.TextChanged += new System.EventHandler(this.txtURI_TextChanged);
            // 
            // txtDescription
            // 
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.Location = new System.Drawing.Point(80, 52);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(796, 20);
            this.txtDescription.TabIndex = 5;
            this.txtDescription.TextChanged += new System.EventHandler(this.txtDescription_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 55);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Descrizione";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 82);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(26, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "URI";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(738, 105);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(65, 37);
            this.btnSave.TabIndex = 16;
            this.btnSave.Text = "Salva modifiche";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // chkEnabled
            // 
            this.chkEnabled.AutoSize = true;
            this.chkEnabled.Location = new System.Drawing.Point(317, 26);
            this.chkEnabled.Name = "chkEnabled";
            this.chkEnabled.Size = new System.Drawing.Size(63, 17);
            this.chkEnabled.TabIndex = 17;
            this.chkEnabled.Text = "Abilitato";
            this.chkEnabled.UseVisualStyleBackColor = true;
            this.chkEnabled.CheckedChanged += new System.EventHandler(this.chkEnabled_CheckedChanged);
            // 
            // dtpkEnd
            // 
            this.dtpkEnd.CustomFormat = "dd MMM yyyy HH:mm:ss";
            this.dtpkEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpkEnd.Location = new System.Drawing.Point(166, 24);
            this.dtpkEnd.Name = "dtpkEnd";
            this.dtpkEnd.Size = new System.Drawing.Size(145, 20);
            this.dtpkEnd.TabIndex = 18;
            this.dtpkEnd.ValueChanged += new System.EventHandler(this.dtpkFine_ValueChanged);
            // 
            // txtDaysDuration
            // 
            this.txtDaysDuration.Location = new System.Drawing.Point(386, 22);
            this.txtDaysDuration.Name = "txtDaysDuration";
            this.txtDaysDuration.Size = new System.Drawing.Size(26, 20);
            this.txtDaysDuration.TabIndex = 19;
            this.txtDaysDuration.Text = "0";
            this.txtDaysDuration.TextChanged += new System.EventHandler(this.txtDurata_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(413, 25);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(19, 13);
            this.label7.TabIndex = 20;
            this.label7.Text = "gg";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(464, 25);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(13, 13);
            this.label8.TabIndex = 22;
            this.label8.Text = "h";
            // 
            // txtHoursDuration
            // 
            this.txtHoursDuration.Location = new System.Drawing.Point(436, 22);
            this.txtHoursDuration.Name = "txtHoursDuration";
            this.txtHoursDuration.Size = new System.Drawing.Size(26, 20);
            this.txtHoursDuration.TabIndex = 21;
            this.txtHoursDuration.Text = "0";
            this.txtHoursDuration.TextChanged += new System.EventHandler(this.txtDurata_TextChanged);
            // 
            // txtMinutesDuration
            // 
            this.txtMinutesDuration.Location = new System.Drawing.Point(486, 22);
            this.txtMinutesDuration.Name = "txtMinutesDuration";
            this.txtMinutesDuration.Size = new System.Drawing.Size(26, 20);
            this.txtMinutesDuration.TabIndex = 21;
            this.txtMinutesDuration.Text = "0";
            this.txtMinutesDuration.TextChanged += new System.EventHandler(this.txtDurata_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(513, 25);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(23, 13);
            this.label9.TabIndex = 22;
            this.label9.Text = "min";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(565, 25);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(12, 13);
            this.label10.TabIndex = 24;
            this.label10.Text = "s";
            // 
            // txtSecondsDuration
            // 
            this.txtSecondsDuration.Location = new System.Drawing.Point(536, 22);
            this.txtSecondsDuration.Name = "txtSecondsDuration";
            this.txtSecondsDuration.Size = new System.Drawing.Size(26, 20);
            this.txtSecondsDuration.TabIndex = 23;
            this.txtSecondsDuration.Text = "0";
            this.txtSecondsDuration.TextChanged += new System.EventHandler(this.txtDurata_TextChanged);
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(83, 105);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(65, 37);
            this.btnTest.TabIndex = 25;
            this.btnTest.Text = "Prova";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(596, 125);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(12, 13);
            this.label11.TabIndex = 34;
            this.label11.Text = "s";
            this.label11.Visible = false;
            // 
            // txtSecondsPeriod
            // 
            this.txtSecondsPeriod.Location = new System.Drawing.Point(567, 122);
            this.txtSecondsPeriod.Name = "txtSecondsPeriod";
            this.txtSecondsPeriod.Size = new System.Drawing.Size(26, 20);
            this.txtSecondsPeriod.TabIndex = 33;
            this.txtSecondsPeriod.Text = "0";
            this.txtSecondsPeriod.Visible = false;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(544, 125);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(23, 13);
            this.label12.TabIndex = 31;
            this.label12.Text = "min";
            this.label12.Visible = false;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(495, 125);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(13, 13);
            this.label13.TabIndex = 32;
            this.label13.Text = "h";
            this.label13.Visible = false;
            // 
            // txtMinutesPeriod
            // 
            this.txtMinutesPeriod.Location = new System.Drawing.Point(517, 122);
            this.txtMinutesPeriod.Name = "txtMinutesPeriod";
            this.txtMinutesPeriod.Size = new System.Drawing.Size(26, 20);
            this.txtMinutesPeriod.TabIndex = 29;
            this.txtMinutesPeriod.Text = "0";
            this.txtMinutesPeriod.Visible = false;
            // 
            // txtHoursPeriod
            // 
            this.txtHoursPeriod.Location = new System.Drawing.Point(467, 122);
            this.txtHoursPeriod.Name = "txtHoursPeriod";
            this.txtHoursPeriod.Size = new System.Drawing.Size(26, 20);
            this.txtHoursPeriod.TabIndex = 30;
            this.txtHoursPeriod.Text = "0";
            this.txtHoursPeriod.Visible = false;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(444, 125);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(19, 13);
            this.label14.TabIndex = 28;
            this.label14.Text = "gg";
            this.label14.Visible = false;
            // 
            // txtDaysPeriod
            // 
            this.txtDaysPeriod.Location = new System.Drawing.Point(417, 122);
            this.txtDaysPeriod.Name = "txtDaysPeriod";
            this.txtDaysPeriod.Size = new System.Drawing.Size(26, 20);
            this.txtDaysPeriod.TabIndex = 27;
            this.txtDaysPeriod.Text = "0";
            this.txtDaysPeriod.Visible = false;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(414, 105);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(43, 13);
            this.label15.TabIndex = 26;
            this.label15.Text = "Periodo";
            this.label15.Visible = false;
            // 
            // frmConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.ClientSize = new System.Drawing.Size(888, 396);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtSecondsPeriod);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.txtMinutesPeriod);
            this.Controls.Add(this.txtHoursPeriod);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.txtDaysPeriod);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txtSecondsDuration);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtMinutesDuration);
            this.Controls.Add(this.txtHoursDuration);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtDaysDuration);
            this.Controls.Add(this.dtpkEnd);
            this.Controls.Add(this.chkEnabled);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.txtURI);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dtpkStart);
            this.Controls.Add(this.cmbPeriods);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnErase);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.dgrdEvents);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmConfiguration";
            this.Text = "Configurazione eventi";
            this.Load += new System.EventHandler(this.frmConfiguration_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgrdEvents)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgrdEvents;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnErase;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ComboBox cmbPeriods;
        private System.Windows.Forms.DateTimePicker dtpkStart;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtURI;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.CheckBox chkEnabled;
        private System.Windows.Forms.DateTimePicker dtpkEnd;
        private System.Windows.Forms.TextBox txtDaysDuration;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtHoursDuration;
        private System.Windows.Forms.TextBox txtMinutesDuration;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtSecondsDuration;
        private System.Windows.Forms.DataGridViewTextBoxColumn istanteInizio;
        private System.Windows.Forms.DataGridViewTextBoxColumn IstanteFine;
        private System.Windows.Forms.DataGridViewTextBoxColumn Durata;
        private System.Windows.Forms.DataGridViewTextBoxColumn Periodo;
        private System.Windows.Forms.DataGridViewTextBoxColumn Descrizione;
        private System.Windows.Forms.DataGridViewTextBoxColumn URI;
        private System.Windows.Forms.DataGridViewTextBoxColumn Abilitato;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtSecondsPeriod;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtMinutesPeriod;
        private System.Windows.Forms.TextBox txtHoursPeriod;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtDaysPeriod;
        private System.Windows.Forms.Label label15;
    }
}