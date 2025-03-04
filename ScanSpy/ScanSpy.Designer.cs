namespace ScanSpy
{
  partial class ScanSpy
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
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.splitContainer2 = new System.Windows.Forms.SplitContainer();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.label5 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.cbOnAcquisition = new System.Windows.Forms.CheckBox();
      this.listenIndicatorOff = new System.Windows.Forms.Button();
      this.listenIndicatorWait = new System.Windows.Forms.Button();
      this.listenIndicatorOn = new System.Windows.Forms.Button();
      this.buttonListen = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.disconnectionIndicator = new System.Windows.Forms.Button();
      this.connectionIndicator = new System.Windows.Forms.Button();
      this.buttonConnect = new System.Windows.Forms.Button();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.labelScanSpeed = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.labelStats = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.splitContainer3 = new System.Windows.Forms.SplitContainer();
      this.plotSpectrum = new ScottPlot.WinForms.FormsPlot();
      this.lblScanFilter = new System.Windows.Forms.Label();
      this.lblScanInfo = new System.Windows.Forms.Label();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.rtbLog = new System.Windows.Forms.RichTextBox();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.rtbHeader = new System.Windows.Forms.RichTextBox();
      this.tabPage3 = new System.Windows.Forms.TabPage();
      this.dgvTrailers = new System.Windows.Forms.DataGridView();
      this.TrailerA = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.TrailerB = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.TrailerC = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.TrailerD = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.TrailerE = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.statusStrip1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.groupBox3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
      this.splitContainer3.Panel1.SuspendLayout();
      this.splitContainer3.Panel2.SuspendLayout();
      this.splitContainer3.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.tabPage3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dgvTrailers)).BeginInit();
      this.SuspendLayout();
      // 
      // statusStrip1
      // 
      this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
      this.statusStrip1.Location = new System.Drawing.Point(0, 529);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new System.Drawing.Size(784, 32);
      this.statusStrip1.TabIndex = 0;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // toolStripStatusLabel1
      // 
      this.toolStripStatusLabel1.AutoSize = false;
      this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
      this.toolStripStatusLabel1.Size = new System.Drawing.Size(180, 27);
      this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
      this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // toolStripStatusLabel2
      // 
      this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
      this.toolStripStatusLabel2.Size = new System.Drawing.Size(118, 27);
      this.toolStripStatusLabel2.Text = "toolStripStatusLabel2";
      this.toolStripStatusLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // toolStrip1
      // 
      this.toolStrip1.AutoSize = false;
      this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
      this.toolStrip1.Location = new System.Drawing.Point(0, 0);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
      this.toolStrip1.Size = new System.Drawing.Size(784, 31);
      this.toolStrip1.TabIndex = 1;
      this.toolStrip1.Text = "toolStrip1";
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      this.splitContainer1.IsSplitterFixed = true;
      this.splitContainer1.Location = new System.Drawing.Point(0, 31);
      this.splitContainer1.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
      this.splitContainer1.Panel1MinSize = 220;
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
      this.splitContainer1.Size = new System.Drawing.Size(784, 498);
      this.splitContainer1.SplitterDistance = 230;
      this.splitContainer1.TabIndex = 2;
      // 
      // splitContainer2
      // 
      this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      this.splitContainer2.IsSplitterFixed = true;
      this.splitContainer2.Location = new System.Drawing.Point(0, 0);
      this.splitContainer2.Name = "splitContainer2";
      this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer2.Panel1
      // 
      this.splitContainer2.Panel1.Controls.Add(this.groupBox2);
      this.splitContainer2.Panel1.Controls.Add(this.groupBox1);
      // 
      // splitContainer2.Panel2
      // 
      this.splitContainer2.Panel2.Controls.Add(this.groupBox3);
      this.splitContainer2.Size = new System.Drawing.Size(230, 498);
      this.splitContainer2.SplitterDistance = 148;
      this.splitContainer2.TabIndex = 0;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.label5);
      this.groupBox2.Controls.Add(this.label4);
      this.groupBox2.Controls.Add(this.label3);
      this.groupBox2.Controls.Add(this.cbOnAcquisition);
      this.groupBox2.Controls.Add(this.listenIndicatorOff);
      this.groupBox2.Controls.Add(this.listenIndicatorWait);
      this.groupBox2.Controls.Add(this.listenIndicatorOn);
      this.groupBox2.Controls.Add(this.buttonListen);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBox2.Location = new System.Drawing.Point(0, 64);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(230, 80);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Activity";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(138, 57);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(26, 17);
      this.label5.TabIndex = 8;
      this.label5.Text = "Off";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(138, 41);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(51, 17);
      this.label4.TabIndex = 7;
      this.label4.Text = "Waiting";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(138, 25);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(47, 17);
      this.label3.TabIndex = 6;
      this.label3.Text = "Spying";
      // 
      // cbOnAcquisition
      // 
      this.cbOnAcquisition.AutoSize = true;
      this.cbOnAcquisition.Location = new System.Drawing.Point(6, 20);
      this.cbOnAcquisition.Name = "cbOnAcquisition";
      this.cbOnAcquisition.Size = new System.Drawing.Size(111, 21);
      this.cbOnAcquisition.TabIndex = 5;
      this.cbOnAcquisition.Text = "On Acquisition";
      this.cbOnAcquisition.UseVisualStyleBackColor = true;
      // 
      // listenIndicatorOff
      // 
      this.listenIndicatorOff.BackColor = System.Drawing.Color.Gray;
      this.listenIndicatorOff.Location = new System.Drawing.Point(122, 58);
      this.listenIndicatorOff.Name = "listenIndicatorOff";
      this.listenIndicatorOff.Size = new System.Drawing.Size(16, 16);
      this.listenIndicatorOff.TabIndex = 4;
      this.listenIndicatorOff.UseVisualStyleBackColor = false;
      // 
      // listenIndicatorWait
      // 
      this.listenIndicatorWait.BackColor = System.Drawing.Color.Gray;
      this.listenIndicatorWait.Location = new System.Drawing.Point(122, 42);
      this.listenIndicatorWait.Name = "listenIndicatorWait";
      this.listenIndicatorWait.Size = new System.Drawing.Size(16, 16);
      this.listenIndicatorWait.TabIndex = 3;
      this.listenIndicatorWait.UseVisualStyleBackColor = false;
      // 
      // listenIndicatorOn
      // 
      this.listenIndicatorOn.BackColor = System.Drawing.Color.Gray;
      this.listenIndicatorOn.Location = new System.Drawing.Point(122, 26);
      this.listenIndicatorOn.Name = "listenIndicatorOn";
      this.listenIndicatorOn.Size = new System.Drawing.Size(16, 16);
      this.listenIndicatorOn.TabIndex = 2;
      this.listenIndicatorOn.UseVisualStyleBackColor = false;
      // 
      // buttonListen
      // 
      this.buttonListen.Location = new System.Drawing.Point(3, 42);
      this.buttonListen.Name = "buttonListen";
      this.buttonListen.Size = new System.Drawing.Size(96, 32);
      this.buttonListen.TabIndex = 0;
      this.buttonListen.Text = "Activate";
      this.buttonListen.UseVisualStyleBackColor = true;
      this.buttonListen.Click += new System.EventHandler(this.buttonListen_Click);
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.disconnectionIndicator);
      this.groupBox1.Controls.Add(this.connectionIndicator);
      this.groupBox1.Controls.Add(this.buttonConnect);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(230, 64);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Instrument Status";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(138, 42);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(86, 17);
      this.label2.TabIndex = 4;
      this.label2.Text = "Disconnected";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(138, 26);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(70, 17);
      this.label1.TabIndex = 3;
      this.label1.Text = "Connected";
      // 
      // disconnectionIndicator
      // 
      this.disconnectionIndicator.BackColor = System.Drawing.Color.Red;
      this.disconnectionIndicator.Location = new System.Drawing.Point(122, 43);
      this.disconnectionIndicator.Name = "disconnectionIndicator";
      this.disconnectionIndicator.Size = new System.Drawing.Size(16, 16);
      this.disconnectionIndicator.TabIndex = 2;
      this.disconnectionIndicator.UseVisualStyleBackColor = false;
      // 
      // connectionIndicator
      // 
      this.connectionIndicator.BackColor = System.Drawing.Color.Gray;
      this.connectionIndicator.Location = new System.Drawing.Point(122, 27);
      this.connectionIndicator.Name = "connectionIndicator";
      this.connectionIndicator.Size = new System.Drawing.Size(16, 16);
      this.connectionIndicator.TabIndex = 1;
      this.connectionIndicator.UseVisualStyleBackColor = false;
      // 
      // buttonConnect
      // 
      this.buttonConnect.Location = new System.Drawing.Point(3, 26);
      this.buttonConnect.Name = "buttonConnect";
      this.buttonConnect.Size = new System.Drawing.Size(96, 32);
      this.buttonConnect.TabIndex = 0;
      this.buttonConnect.Text = "Connect";
      this.buttonConnect.UseVisualStyleBackColor = true;
      this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.labelScanSpeed);
      this.groupBox3.Controls.Add(this.label8);
      this.groupBox3.Controls.Add(this.labelStats);
      this.groupBox3.Controls.Add(this.label7);
      this.groupBox3.Controls.Add(this.label6);
      this.groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBox3.Location = new System.Drawing.Point(0, 0);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(230, 152);
      this.groupBox3.TabIndex = 0;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Summary";
      // 
      // labelScanSpeed
      // 
      this.labelScanSpeed.AutoSize = true;
      this.labelScanSpeed.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelScanSpeed.Location = new System.Drawing.Point(81, 118);
      this.labelScanSpeed.Name = "labelScanSpeed";
      this.labelScanSpeed.Size = new System.Drawing.Size(30, 15);
      this.labelScanSpeed.TabIndex = 4;
      this.labelScanSpeed.Text = "0 Hz";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label8.Location = new System.Drawing.Point(6, 118);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(73, 15);
      this.label8.TabIndex = 3;
      this.label8.Text = "Scan Speed: ";
      // 
      // labelStats
      // 
      this.labelStats.AutoSize = true;
      this.labelStats.Font = new System.Drawing.Font("Cascadia Code", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelStats.Location = new System.Drawing.Point(81, 39);
      this.labelStats.Name = "labelStats";
      this.labelStats.Size = new System.Drawing.Size(84, 64);
      this.labelStats.TabIndex = 2;
      this.labelStats.Text = "0         0\r\n0         0\r\n0         0\r\n0         0";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label7.Location = new System.Drawing.Point(81, 21);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(136, 15);
      this.label7.TabIndex = 1;
      this.label7.Text = "Current          All               ";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label6.Location = new System.Drawing.Point(6, 39);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(69, 60);
      this.label6.TabIndex = 0;
      this.label6.Text = "Total Scans:\r\nMS1 Scans:\r\nMS2 Scans:\r\nMS3 Scans:";
      // 
      // splitContainer3
      // 
      this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer3.Location = new System.Drawing.Point(0, 0);
      this.splitContainer3.Name = "splitContainer3";
      this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer3.Panel1
      // 
      this.splitContainer3.Panel1.Controls.Add(this.plotSpectrum);
      this.splitContainer3.Panel1.Controls.Add(this.lblScanFilter);
      this.splitContainer3.Panel1.Controls.Add(this.lblScanInfo);
      // 
      // splitContainer3.Panel2
      // 
      this.splitContainer3.Panel2.Controls.Add(this.tabControl1);
      this.splitContainer3.Size = new System.Drawing.Size(550, 498);
      this.splitContainer3.SplitterDistance = 279;
      this.splitContainer3.TabIndex = 0;
      // 
      // plotSpectrum
      // 
      this.plotSpectrum.DisplayScale = 0F;
      this.plotSpectrum.Dock = System.Windows.Forms.DockStyle.Fill;
      this.plotSpectrum.Location = new System.Drawing.Point(0, 34);
      this.plotSpectrum.Name = "plotSpectrum";
      this.plotSpectrum.Size = new System.Drawing.Size(550, 245);
      this.plotSpectrum.TabIndex = 0;
      // 
      // lblScanFilter
      // 
      this.lblScanFilter.AutoSize = true;
      this.lblScanFilter.Dock = System.Windows.Forms.DockStyle.Top;
      this.lblScanFilter.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblScanFilter.Location = new System.Drawing.Point(0, 17);
      this.lblScanFilter.Name = "lblScanFilter";
      this.lblScanFilter.Size = new System.Drawing.Size(0, 17);
      this.lblScanFilter.TabIndex = 2;
      // 
      // lblScanInfo
      // 
      this.lblScanInfo.AutoSize = true;
      this.lblScanInfo.Dock = System.Windows.Forms.DockStyle.Top;
      this.lblScanInfo.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblScanInfo.Location = new System.Drawing.Point(0, 0);
      this.lblScanInfo.Name = "lblScanInfo";
      this.lblScanInfo.Size = new System.Drawing.Size(0, 17);
      this.lblScanInfo.TabIndex = 1;
      // 
      // tabControl1
      // 
      this.tabControl1.Alignment = System.Windows.Forms.TabAlignment.Bottom;
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Controls.Add(this.tabPage3);
      this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(550, 215);
      this.tabControl1.TabIndex = 0;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.rtbLog);
      this.tabPage1.Location = new System.Drawing.Point(4, 4);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
      this.tabPage1.Size = new System.Drawing.Size(542, 189);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Message Log";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // rtbLog
      // 
      this.rtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
      this.rtbLog.Location = new System.Drawing.Point(3, 3);
      this.rtbLog.Name = "rtbLog";
      this.rtbLog.Size = new System.Drawing.Size(536, 183);
      this.rtbLog.TabIndex = 0;
      this.rtbLog.Text = "";
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.rtbHeader);
      this.tabPage2.Location = new System.Drawing.Point(4, 4);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
      this.tabPage2.Size = new System.Drawing.Size(619, 194);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Header";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // rtbHeader
      // 
      this.rtbHeader.BackColor = System.Drawing.SystemColors.Window;
      this.rtbHeader.Dock = System.Windows.Forms.DockStyle.Fill;
      this.rtbHeader.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rtbHeader.Location = new System.Drawing.Point(3, 3);
      this.rtbHeader.Name = "rtbHeader";
      this.rtbHeader.ReadOnly = true;
      this.rtbHeader.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
      this.rtbHeader.Size = new System.Drawing.Size(613, 188);
      this.rtbHeader.TabIndex = 0;
      this.rtbHeader.Text = "";
      // 
      // tabPage3
      // 
      this.tabPage3.Controls.Add(this.dgvTrailers);
      this.tabPage3.Location = new System.Drawing.Point(4, 4);
      this.tabPage3.Name = "tabPage3";
      this.tabPage3.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
      this.tabPage3.Size = new System.Drawing.Size(619, 194);
      this.tabPage3.TabIndex = 2;
      this.tabPage3.Text = "Trailers";
      this.tabPage3.UseVisualStyleBackColor = true;
      // 
      // dgvTrailers
      // 
      this.dgvTrailers.AllowUserToAddRows = false;
      this.dgvTrailers.AllowUserToDeleteRows = false;
      this.dgvTrailers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dgvTrailers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.TrailerA,
            this.TrailerB,
            this.TrailerC,
            this.TrailerD,
            this.TrailerE});
      this.dgvTrailers.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dgvTrailers.Location = new System.Drawing.Point(3, 3);
      this.dgvTrailers.Name = "dgvTrailers";
      this.dgvTrailers.ReadOnly = true;
      this.dgvTrailers.RowHeadersVisible = false;
      this.dgvTrailers.RowHeadersWidth = 62;
      this.dgvTrailers.Size = new System.Drawing.Size(613, 188);
      this.dgvTrailers.TabIndex = 0;
      // 
      // TrailerA
      // 
      this.TrailerA.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.TrailerA.HeaderText = "Trailer";
      this.TrailerA.MinimumWidth = 8;
      this.TrailerA.Name = "TrailerA";
      this.TrailerA.ReadOnly = true;
      this.TrailerA.Width = 61;
      // 
      // TrailerB
      // 
      this.TrailerB.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.TrailerB.HeaderText = "MS Level";
      this.TrailerB.MinimumWidth = 8;
      this.TrailerB.Name = "TrailerB";
      this.TrailerB.ReadOnly = true;
      this.TrailerB.Width = 77;
      // 
      // TrailerC
      // 
      this.TrailerC.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.TrailerC.HeaderText = "ExampleMS1";
      this.TrailerC.MinimumWidth = 8;
      this.TrailerC.Name = "TrailerC";
      this.TrailerC.ReadOnly = true;
      this.TrailerC.Width = 94;
      // 
      // TrailerD
      // 
      this.TrailerD.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.TrailerD.HeaderText = "ExampleMS2";
      this.TrailerD.MinimumWidth = 8;
      this.TrailerD.Name = "TrailerD";
      this.TrailerD.ReadOnly = true;
      this.TrailerD.Width = 94;
      // 
      // TrailerE
      // 
      this.TrailerE.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.TrailerE.HeaderText = "ExampleMS3";
      this.TrailerE.MinimumWidth = 8;
      this.TrailerE.Name = "TrailerE";
      this.TrailerE.ReadOnly = true;
      // 
      // ScanSpy
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(784, 561);
      this.Controls.Add(this.splitContainer1);
      this.Controls.Add(this.toolStrip1);
      this.Controls.Add(this.statusStrip1);
      this.Name = "ScanSpy";
      this.Text = "ScanSpy";
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      this.splitContainer2.Panel1.ResumeLayout(false);
      this.splitContainer2.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
      this.splitContainer2.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.splitContainer3.Panel1.ResumeLayout(false);
      this.splitContainer3.Panel1.PerformLayout();
      this.splitContainer3.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
      this.splitContainer3.ResumeLayout(false);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.tabPage3.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.dgvTrailers)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStrip toolStrip1;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.SplitContainer splitContainer3;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.GroupBox groupBox1;
    private ScottPlot.WinForms.FormsPlot plotSpectrum;
    private System.Windows.Forms.Button connectionIndicator;
    private System.Windows.Forms.Button buttonConnect;
    private System.Windows.Forms.Button disconnectionIndicator;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button buttonListen;
    private System.Windows.Forms.Button listenIndicatorOff;
    private System.Windows.Forms.Button listenIndicatorWait;
    private System.Windows.Forms.Button listenIndicatorOn;
    private System.Windows.Forms.CheckBox cbOnAcquisition;
    private System.Windows.Forms.RichTextBox rtbLog;
    private System.Windows.Forms.Label lblScanFilter;
    private System.Windows.Forms.Label lblScanInfo;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TabPage tabPage3;
    private System.Windows.Forms.DataGridView dgvTrailers;
    private System.Windows.Forms.DataGridViewTextBoxColumn TrailerA;
    private System.Windows.Forms.DataGridViewTextBoxColumn TrailerB;
    private System.Windows.Forms.DataGridViewTextBoxColumn TrailerC;
    private System.Windows.Forms.DataGridViewTextBoxColumn TrailerD;
    private System.Windows.Forms.DataGridViewTextBoxColumn TrailerE;
    private System.Windows.Forms.RichTextBox rtbHeader;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label labelStats;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label labelScanSpeed;
    private System.Windows.Forms.Label label8;
  }
}

