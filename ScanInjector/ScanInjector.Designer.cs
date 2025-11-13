namespace ScanInjector
{
  partial class ScanInjector
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScanInjector));
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.splitContainer2 = new System.Windows.Forms.SplitContainer();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.label5 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.listenIndicatorOff = new System.Windows.Forms.Button();
      this.listenIndicatorOn = new System.Windows.Forms.Button();
      this.buttonListen = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.disconnectionIndicator = new System.Windows.Forms.Button();
      this.connectionIndicator = new System.Windows.Forms.Button();
      this.buttonConnect = new System.Windows.Forms.Button();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.comIT = new System.Windows.Forms.NumericUpDown();
      this.comAGC = new System.Windows.Forms.NumericUpDown();
      this.comRes = new System.Windows.Forms.ComboBox();
      this.comAnalyzer = new System.Windows.Forms.ComboBox();
      this.buttonCustomScan = new System.Windows.Forms.Button();
      this.label14 = new System.Windows.Forms.Label();
      this.label13 = new System.Windows.Forms.Label();
      this.label12 = new System.Windows.Forms.Label();
      this.label11 = new System.Windows.Forms.Label();
      this.comClearTarget = new System.Windows.Forms.Button();
      this.comRemoveTarget = new System.Windows.Forms.Button();
      this.comAddTarget = new System.Windows.Forms.Button();
      this.comTargets = new System.Windows.Forms.ListBox();
      this.label10 = new System.Windows.Forms.Label();
      this.label9 = new System.Windows.Forms.Label();
      this.comMassHigh = new System.Windows.Forms.NumericUpDown();
      this.comMassLow = new System.Windows.Forms.NumericUpDown();
      this.label8 = new System.Windows.Forms.Label();
      this.comNCE = new System.Windows.Forms.NumericUpDown();
      this.label7 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.comScanType = new System.Windows.Forms.ComboBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.comScanCollection = new System.Windows.Forms.ListBox();
      this.label4 = new System.Windows.Forms.Label();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.rtbLog = new System.Windows.Forms.RichTextBox();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.plotSpectrum = new ScottPlot.WinForms.FormsPlot();
      this.labelScanFilter = new System.Windows.Forms.Label();
      this.labelScanInfo = new System.Windows.Forms.Label();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.comIT)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.comAGC)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.comMassHigh)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.comMassLow)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.comNCE)).BeginInit();
      this.panel1.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.SuspendLayout();
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      this.splitContainer1.IsSplitterFixed = true;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.Control;
      this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
      this.splitContainer1.Panel2.Controls.Add(this.statusStrip1);
      this.splitContainer1.Size = new System.Drawing.Size(784, 561);
      this.splitContainer1.SplitterDistance = 304;
      this.splitContainer1.TabIndex = 0;
      // 
      // splitContainer2
      // 
      this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      this.splitContainer2.IsSplitterFixed = true;
      this.splitContainer2.Location = new System.Drawing.Point(0, 0);
      this.splitContainer2.Name = "splitContainer2";
      // 
      // splitContainer2.Panel1
      // 
      this.splitContainer2.Panel1.Controls.Add(this.groupBox3);
      this.splitContainer2.Panel1.Controls.Add(this.groupBox1);
      // 
      // splitContainer2.Panel2
      // 
      this.splitContainer2.Panel2.Controls.Add(this.groupBox2);
      this.splitContainer2.Size = new System.Drawing.Size(784, 304);
      this.splitContainer2.SplitterDistance = 220;
      this.splitContainer2.TabIndex = 0;
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.label5);
      this.groupBox3.Controls.Add(this.label3);
      this.groupBox3.Controls.Add(this.listenIndicatorOff);
      this.groupBox3.Controls.Add(this.listenIndicatorOn);
      this.groupBox3.Controls.Add(this.buttonListen);
      this.groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBox3.Location = new System.Drawing.Point(0, 64);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(220, 64);
      this.groupBox3.TabIndex = 2;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Activity";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(128, 42);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(26, 17);
      this.label5.TabIndex = 8;
      this.label5.Text = "Off";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(128, 25);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(42, 17);
      this.label3.TabIndex = 6;
      this.label3.Text = "Active";
      // 
      // listenIndicatorOff
      // 
      this.listenIndicatorOff.BackColor = System.Drawing.Color.Red;
      this.listenIndicatorOff.Location = new System.Drawing.Point(112, 42);
      this.listenIndicatorOff.Name = "listenIndicatorOff";
      this.listenIndicatorOff.Size = new System.Drawing.Size(16, 16);
      this.listenIndicatorOff.TabIndex = 4;
      this.listenIndicatorOff.UseVisualStyleBackColor = false;
      // 
      // listenIndicatorOn
      // 
      this.listenIndicatorOn.BackColor = System.Drawing.Color.Gray;
      this.listenIndicatorOn.Location = new System.Drawing.Point(112, 26);
      this.listenIndicatorOn.Name = "listenIndicatorOn";
      this.listenIndicatorOn.Size = new System.Drawing.Size(16, 16);
      this.listenIndicatorOn.TabIndex = 2;
      this.listenIndicatorOn.UseVisualStyleBackColor = false;
      // 
      // buttonListen
      // 
      this.buttonListen.Location = new System.Drawing.Point(3, 26);
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
      this.groupBox1.Size = new System.Drawing.Size(220, 64);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Instrument Status";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(128, 43);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(86, 17);
      this.label2.TabIndex = 4;
      this.label2.Text = "Disconnected";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(128, 27);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(70, 17);
      this.label1.TabIndex = 3;
      this.label1.Text = "Connected";
      // 
      // disconnectionIndicator
      // 
      this.disconnectionIndicator.BackColor = System.Drawing.Color.Red;
      this.disconnectionIndicator.Location = new System.Drawing.Point(112, 42);
      this.disconnectionIndicator.Name = "disconnectionIndicator";
      this.disconnectionIndicator.Size = new System.Drawing.Size(16, 16);
      this.disconnectionIndicator.TabIndex = 2;
      this.disconnectionIndicator.UseVisualStyleBackColor = false;
      // 
      // connectionIndicator
      // 
      this.connectionIndicator.BackColor = System.Drawing.Color.Gray;
      this.connectionIndicator.Location = new System.Drawing.Point(112, 26);
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
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.comIT);
      this.groupBox2.Controls.Add(this.comAGC);
      this.groupBox2.Controls.Add(this.comRes);
      this.groupBox2.Controls.Add(this.comAnalyzer);
      this.groupBox2.Controls.Add(this.buttonCustomScan);
      this.groupBox2.Controls.Add(this.label14);
      this.groupBox2.Controls.Add(this.label13);
      this.groupBox2.Controls.Add(this.label12);
      this.groupBox2.Controls.Add(this.label11);
      this.groupBox2.Controls.Add(this.comClearTarget);
      this.groupBox2.Controls.Add(this.comRemoveTarget);
      this.groupBox2.Controls.Add(this.comAddTarget);
      this.groupBox2.Controls.Add(this.comTargets);
      this.groupBox2.Controls.Add(this.label10);
      this.groupBox2.Controls.Add(this.label9);
      this.groupBox2.Controls.Add(this.comMassHigh);
      this.groupBox2.Controls.Add(this.comMassLow);
      this.groupBox2.Controls.Add(this.label8);
      this.groupBox2.Controls.Add(this.comNCE);
      this.groupBox2.Controls.Add(this.label7);
      this.groupBox2.Controls.Add(this.label6);
      this.groupBox2.Controls.Add(this.comScanType);
      this.groupBox2.Controls.Add(this.panel1);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBox2.Location = new System.Drawing.Point(0, 0);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(560, 304);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Custom Scan Settings";
      // 
      // comIT
      // 
      this.comIT.Location = new System.Drawing.Point(261, 87);
      this.comIT.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.comIT.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.comIT.Name = "comIT";
      this.comIT.Size = new System.Drawing.Size(64, 25);
      this.comIT.TabIndex = 24;
      this.comIT.Tag = "in milliseconds";
      this.comIT.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
      // 
      // comAGC
      // 
      this.comAGC.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.comAGC.Location = new System.Drawing.Point(94, 86);
      this.comAGC.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
      this.comAGC.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.comAGC.Name = "comAGC";
      this.comAGC.Size = new System.Drawing.Size(80, 25);
      this.comAGC.TabIndex = 23;
      this.comAGC.Value = new decimal(new int[] {
            100000,
            0,
            0,
            0});
      // 
      // comRes
      // 
      this.comRes.FormattingEnabled = true;
      this.comRes.Location = new System.Drawing.Point(261, 24);
      this.comRes.Name = "comRes";
      this.comRes.Size = new System.Drawing.Size(80, 25);
      this.comRes.TabIndex = 22;
      // 
      // comAnalyzer
      // 
      this.comAnalyzer.FormattingEnabled = true;
      this.comAnalyzer.Location = new System.Drawing.Point(94, 24);
      this.comAnalyzer.Name = "comAnalyzer";
      this.comAnalyzer.Size = new System.Drawing.Size(80, 25);
      this.comAnalyzer.TabIndex = 21;
      this.comAnalyzer.Text = "Orbitrap";
      this.comAnalyzer.SelectedValueChanged += new System.EventHandler(this.comAnalyzer_SelectedValueChanged);
      // 
      // buttonCustomScan
      // 
      this.buttonCustomScan.Location = new System.Drawing.Point(9, 260);
      this.buttonCustomScan.Name = "buttonCustomScan";
      this.buttonCustomScan.Size = new System.Drawing.Size(125, 34);
      this.buttonCustomScan.TabIndex = 20;
      this.buttonCustomScan.Text = "Request Scan";
      this.buttonCustomScan.UseVisualStyleBackColor = true;
      this.buttonCustomScan.Click += new System.EventHandler(this.buttonCustomScan_Click);
      // 
      // label14
      // 
      this.label14.AutoSize = true;
      this.label14.Location = new System.Drawing.Point(186, 89);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(73, 17);
      this.label14.TabIndex = 19;
      this.label14.Text = "Inject Time:";
      // 
      // label13
      // 
      this.label13.AutoSize = true;
      this.label13.Location = new System.Drawing.Point(6, 89);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(36, 17);
      this.label13.TabIndex = 18;
      this.label13.Text = "AGC:";
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(186, 27);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(72, 17);
      this.label12.TabIndex = 17;
      this.label12.Text = "Resolution:";
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(6, 27);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(60, 17);
      this.label11.TabIndex = 16;
      this.label11.Text = "Analyzer:";
      // 
      // comClearTarget
      // 
      this.comClearTarget.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("comClearTarget.BackgroundImage")));
      this.comClearTarget.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
      this.comClearTarget.Location = new System.Drawing.Point(249, 216);
      this.comClearTarget.Name = "comClearTarget";
      this.comClearTarget.Size = new System.Drawing.Size(28, 28);
      this.comClearTarget.TabIndex = 15;
      this.comClearTarget.UseVisualStyleBackColor = true;
      this.comClearTarget.Click += new System.EventHandler(this.comClearTarget_Click);
      // 
      // comRemoveTarget
      // 
      this.comRemoveTarget.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("comRemoveTarget.BackgroundImage")));
      this.comRemoveTarget.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
      this.comRemoveTarget.Location = new System.Drawing.Point(249, 182);
      this.comRemoveTarget.Name = "comRemoveTarget";
      this.comRemoveTarget.Size = new System.Drawing.Size(28, 28);
      this.comRemoveTarget.TabIndex = 14;
      this.comRemoveTarget.UseVisualStyleBackColor = true;
      this.comRemoveTarget.Click += new System.EventHandler(this.comRemoveTarget_Click);
      // 
      // comAddTarget
      // 
      this.comAddTarget.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("comAddTarget.BackgroundImage")));
      this.comAddTarget.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
      this.comAddTarget.Location = new System.Drawing.Point(249, 148);
      this.comAddTarget.Name = "comAddTarget";
      this.comAddTarget.Size = new System.Drawing.Size(28, 28);
      this.comAddTarget.TabIndex = 13;
      this.comAddTarget.UseVisualStyleBackColor = true;
      this.comAddTarget.Click += new System.EventHandler(this.comAddTarget_Click);
      // 
      // comTargets
      // 
      this.comTargets.FormattingEnabled = true;
      this.comTargets.ItemHeight = 17;
      this.comTargets.Location = new System.Drawing.Point(94, 148);
      this.comTargets.Name = "comTargets";
      this.comTargets.Size = new System.Drawing.Size(149, 106);
      this.comTargets.TabIndex = 12;
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(5, 148);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(90, 34);
      this.label10.TabIndex = 11;
      this.label10.Text = "Target m/z:\r\n(MSx allowed)";
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(164, 121);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(13, 17);
      this.label9.TabIndex = 10;
      this.label9.Text = "-";
      // 
      // comMassHigh
      // 
      this.comMassHigh.Location = new System.Drawing.Point(179, 117);
      this.comMassHigh.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
      this.comMassHigh.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
      this.comMassHigh.Name = "comMassHigh";
      this.comMassHigh.Size = new System.Drawing.Size(64, 25);
      this.comMassHigh.TabIndex = 9;
      this.comMassHigh.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
      // 
      // comMassLow
      // 
      this.comMassLow.Location = new System.Drawing.Point(94, 117);
      this.comMassLow.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
      this.comMassLow.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
      this.comMassLow.Name = "comMassLow";
      this.comMassLow.Size = new System.Drawing.Size(64, 25);
      this.comMassLow.TabIndex = 8;
      this.comMassLow.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(6, 121);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(83, 17);
      this.label8.TabIndex = 7;
      this.label8.Text = "Mass Range:";
      // 
      // comNCE
      // 
      this.comNCE.Location = new System.Drawing.Point(261, 55);
      this.comNCE.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
      this.comNCE.Name = "comNCE";
      this.comNCE.Size = new System.Drawing.Size(64, 25);
      this.comNCE.TabIndex = 6;
      this.comNCE.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(186, 58);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(36, 17);
      this.label7.TabIndex = 5;
      this.label7.Text = "NCE:";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(6, 58);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(69, 17);
      this.label6.TabIndex = 4;
      this.label6.Text = "Scan Type:";
      // 
      // comScanType
      // 
      this.comScanType.FormattingEnabled = true;
      this.comScanType.Items.AddRange(new object[] {
            "MS1",
            "MS2"});
      this.comScanType.Location = new System.Drawing.Point(94, 55);
      this.comScanType.Name = "comScanType";
      this.comScanType.Size = new System.Drawing.Size(80, 25);
      this.comScanType.TabIndex = 3;
      this.comScanType.Tag = "Limited to MS1/MS2 for the purposes of this tutorial software.";
      this.comScanType.Text = "MS2";
      this.comScanType.SelectedValueChanged += new System.EventHandler(this.comScanType_SelectedValueChanged);
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.comScanCollection);
      this.panel1.Controls.Add(this.label4);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
      this.panel1.Location = new System.Drawing.Point(357, 21);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(200, 280);
      this.panel1.TabIndex = 2;
      // 
      // comScanCollection
      // 
      this.comScanCollection.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comScanCollection.Font = new System.Drawing.Font("Cascadia Code", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.comScanCollection.FormattingEnabled = true;
      this.comScanCollection.ItemHeight = 17;
      this.comScanCollection.Location = new System.Drawing.Point(0, 17);
      this.comScanCollection.Name = "comScanCollection";
      this.comScanCollection.Size = new System.Drawing.Size(200, 263);
      this.comScanCollection.TabIndex = 0;
      this.comScanCollection.SelectedIndexChanged += new System.EventHandler(this.comScanCollection_SelectedIndexChanged);
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Dock = System.Windows.Forms.DockStyle.Top;
      this.label4.Location = new System.Drawing.Point(0, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(102, 17);
      this.label4.TabIndex = 1;
      this.label4.Text = "Collected Scans:";
      // 
      // tabControl1
      // 
      this.tabControl1.Alignment = System.Windows.Forms.TabAlignment.Bottom;
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(784, 231);
      this.tabControl1.TabIndex = 1;
      // 
      // tabPage2
      // 
      this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
      this.tabPage2.Controls.Add(this.rtbLog);
      this.tabPage2.Location = new System.Drawing.Point(4, 4);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(776, 205);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Message Log";
      // 
      // rtbLog
      // 
      this.rtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
      this.rtbLog.Location = new System.Drawing.Point(3, 3);
      this.rtbLog.Name = "rtbLog";
      this.rtbLog.Size = new System.Drawing.Size(770, 199);
      this.rtbLog.TabIndex = 0;
      this.rtbLog.Text = "";
      // 
      // tabPage1
      // 
      this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
      this.tabPage1.Controls.Add(this.plotSpectrum);
      this.tabPage1.Controls.Add(this.labelScanFilter);
      this.tabPage1.Controls.Add(this.labelScanInfo);
      this.tabPage1.Location = new System.Drawing.Point(4, 4);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(776, 205);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Spectrum";
      // 
      // plotSpectrum
      // 
      this.plotSpectrum.DisplayScale = 0F;
      this.plotSpectrum.Dock = System.Windows.Forms.DockStyle.Fill;
      this.plotSpectrum.Location = new System.Drawing.Point(3, 29);
      this.plotSpectrum.Name = "plotSpectrum";
      this.plotSpectrum.Size = new System.Drawing.Size(770, 173);
      this.plotSpectrum.TabIndex = 0;
      // 
      // labelScanFilter
      // 
      this.labelScanFilter.AutoSize = true;
      this.labelScanFilter.Dock = System.Windows.Forms.DockStyle.Top;
      this.labelScanFilter.Location = new System.Drawing.Point(3, 16);
      this.labelScanFilter.Name = "labelScanFilter";
      this.labelScanFilter.Size = new System.Drawing.Size(41, 13);
      this.labelScanFilter.TabIndex = 2;
      this.labelScanFilter.Text = "label15";
      // 
      // labelScanInfo
      // 
      this.labelScanInfo.AutoSize = true;
      this.labelScanInfo.Dock = System.Windows.Forms.DockStyle.Top;
      this.labelScanInfo.Location = new System.Drawing.Point(3, 3);
      this.labelScanInfo.Name = "labelScanInfo";
      this.labelScanInfo.Size = new System.Drawing.Size(41, 13);
      this.labelScanInfo.TabIndex = 1;
      this.labelScanInfo.Text = "label15";
      // 
      // statusStrip1
      // 
      this.statusStrip1.Location = new System.Drawing.Point(0, 231);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new System.Drawing.Size(784, 22);
      this.statusStrip1.TabIndex = 2;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // ScanInjector
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(784, 561);
      this.Controls.Add(this.splitContainer1);
      this.MinimumSize = new System.Drawing.Size(800, 600);
      this.Name = "ScanInjector";
      this.Text = "ScanInjector";
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.Panel2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      this.splitContainer2.Panel1.ResumeLayout(false);
      this.splitContainer2.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
      this.splitContainer2.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.comIT)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.comAGC)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.comMassHigh)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.comMassLow)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.comNCE)).EndInit();
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.tabControl1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button disconnectionIndicator;
    private System.Windows.Forms.Button connectionIndicator;
    private System.Windows.Forms.Button buttonConnect;
    private System.Windows.Forms.RichTextBox rtbLog;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Button listenIndicatorOff;
    private System.Windows.Forms.Button listenIndicatorOn;
    private System.Windows.Forms.Button buttonListen;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.ListBox comScanCollection;
    private System.Windows.Forms.NumericUpDown comNCE;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.ComboBox comScanType;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.NumericUpDown comMassHigh;
    private System.Windows.Forms.NumericUpDown comMassLow;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label label14;
    private System.Windows.Forms.Label label13;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.Button comClearTarget;
    private System.Windows.Forms.Button comRemoveTarget;
    private System.Windows.Forms.Button comAddTarget;
    private System.Windows.Forms.ListBox comTargets;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.Button buttonCustomScan;
    private System.Windows.Forms.ComboBox comRes;
    private System.Windows.Forms.ComboBox comAnalyzer;
    private System.Windows.Forms.NumericUpDown comIT;
    private System.Windows.Forms.NumericUpDown comAGC;
    private System.Windows.Forms.Label labelScanInfo;
    private ScottPlot.WinForms.FormsPlot plotSpectrum;
    private System.Windows.Forms.Label labelScanFilter;
  }
}

