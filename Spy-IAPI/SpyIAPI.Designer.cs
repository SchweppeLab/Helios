namespace Spy_IAPI
{
  partial class SpyIAPI
  {
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpyIAPI));
      splitContainer1 = new SplitContainer();
      splitContainer2 = new SplitContainer();
      groupBox2 = new GroupBox();
      label5 = new Label();
      label4 = new Label();
      label3 = new Label();
      listenIndicatorOn = new Button();
      listenIndicatorWait = new Button();
      cbOnAcquisition = new CheckBox();
      buttonListen = new Button();
      listenIndicatorOff = new Button();
      groupBox1 = new GroupBox();
      label2 = new Label();
      label1 = new Label();
      disconnectionIndicator = new Button();
      buttonConnect = new Button();
      connectionIndicator = new Button();
      splitContainer3 = new SplitContainer();
      plotSpectrum = new ScottPlot.WinForms.FormsPlot();
      lblScanFilter = new Label();
      lblScanInfo = new Label();
      rtbLog = new RichTextBox();
      toolStrip1 = new ToolStrip();
      toolStripButton1 = new ToolStripButton();
      statusStrip1 = new StatusStrip();
      ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
      splitContainer1.Panel1.SuspendLayout();
      splitContainer1.Panel2.SuspendLayout();
      splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
      splitContainer2.Panel1.SuspendLayout();
      splitContainer2.SuspendLayout();
      groupBox2.SuspendLayout();
      groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
      splitContainer3.Panel1.SuspendLayout();
      splitContainer3.Panel2.SuspendLayout();
      splitContainer3.SuspendLayout();
      toolStrip1.SuspendLayout();
      SuspendLayout();
      // 
      // splitContainer1
      // 
      splitContainer1.Dock = DockStyle.Fill;
      splitContainer1.FixedPanel = FixedPanel.Panel1;
      splitContainer1.IsSplitterFixed = true;
      splitContainer1.Location = new Point(0, 48);
      splitContainer1.Margin = new Padding(1, 2, 1, 2);
      splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      splitContainer1.Panel1.Controls.Add(splitContainer2);
      splitContainer1.Panel1MinSize = 320;
      // 
      // splitContainer1.Panel2
      // 
      splitContainer1.Panel2.Controls.Add(splitContainer3);
      splitContainer1.Size = new Size(1120, 834);
      splitContainer1.SplitterDistance = 320;
      splitContainer1.SplitterWidth = 3;
      splitContainer1.TabIndex = 2;
      // 
      // splitContainer2
      // 
      splitContainer2.Dock = DockStyle.Fill;
      splitContainer2.FixedPanel = FixedPanel.Panel1;
      splitContainer2.IsSplitterFixed = true;
      splitContainer2.Location = new Point(0, 0);
      splitContainer2.Margin = new Padding(1, 2, 1, 2);
      splitContainer2.Name = "splitContainer2";
      splitContainer2.Orientation = Orientation.Horizontal;
      // 
      // splitContainer2.Panel1
      // 
      splitContainer2.Panel1.Controls.Add(groupBox2);
      splitContainer2.Panel1.Controls.Add(groupBox1);
      splitContainer2.Panel1.Padding = new Padding(3);
      splitContainer2.Size = new Size(320, 834);
      splitContainer2.SplitterDistance = 224;
      splitContainer2.SplitterWidth = 2;
      splitContainer2.TabIndex = 0;
      // 
      // groupBox2
      // 
      groupBox2.Controls.Add(label5);
      groupBox2.Controls.Add(label4);
      groupBox2.Controls.Add(label3);
      groupBox2.Controls.Add(listenIndicatorOn);
      groupBox2.Controls.Add(listenIndicatorWait);
      groupBox2.Controls.Add(cbOnAcquisition);
      groupBox2.Controls.Add(buttonListen);
      groupBox2.Controls.Add(listenIndicatorOff);
      groupBox2.Dock = DockStyle.Top;
      groupBox2.Location = new Point(3, 99);
      groupBox2.Name = "groupBox2";
      groupBox2.Size = new Size(314, 128);
      groupBox2.TabIndex = 5;
      groupBox2.TabStop = false;
      groupBox2.Text = "Activity";
      // 
      // label5
      // 
      label5.AutoSize = true;
      label5.Location = new Point(190, 97);
      label5.Name = "label5";
      label5.Size = new Size(38, 25);
      label5.TabIndex = 9;
      label5.Text = "Off";
      // 
      // label4
      // 
      label4.AutoSize = true;
      label4.Location = new Point(190, 72);
      label4.Name = "label4";
      label4.Size = new Size(72, 25);
      label4.TabIndex = 8;
      label4.Text = "Waiting";
      // 
      // label3
      // 
      label3.AutoSize = true;
      label3.Location = new Point(190, 47);
      label3.Name = "label3";
      label3.Size = new Size(67, 25);
      label3.TabIndex = 7;
      label3.Text = "Spying";
      // 
      // listenIndicatorOn
      // 
      listenIndicatorOn.BackColor = Color.Gray;
      listenIndicatorOn.Enabled = false;
      listenIndicatorOn.Location = new Point(159, 48);
      listenIndicatorOn.Margin = new Padding(4, 5, 4, 5);
      listenIndicatorOn.Name = "listenIndicatorOn";
      listenIndicatorOn.Size = new Size(24, 24);
      listenIndicatorOn.TabIndex = 6;
      listenIndicatorOn.UseVisualStyleBackColor = false;
      // 
      // listenIndicatorWait
      // 
      listenIndicatorWait.BackColor = Color.Gray;
      listenIndicatorWait.Enabled = false;
      listenIndicatorWait.Location = new Point(159, 72);
      listenIndicatorWait.Margin = new Padding(4, 5, 4, 5);
      listenIndicatorWait.Name = "listenIndicatorWait";
      listenIndicatorWait.Size = new Size(24, 24);
      listenIndicatorWait.TabIndex = 5;
      listenIndicatorWait.UseVisualStyleBackColor = false;
      // 
      // cbOnAcquisition
      // 
      cbOnAcquisition.AutoSize = true;
      cbOnAcquisition.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
      cbOnAcquisition.Location = new Point(3, 35);
      cbOnAcquisition.Name = "cbOnAcquisition";
      cbOnAcquisition.Size = new Size(138, 25);
      cbOnAcquisition.TabIndex = 4;
      cbOnAcquisition.Text = "On Acquisition";
      cbOnAcquisition.UseVisualStyleBackColor = true;
      // 
      // buttonListen
      // 
      buttonListen.Location = new Point(4, 72);
      buttonListen.Margin = new Padding(4, 5, 4, 5);
      buttonListen.Name = "buttonListen";
      buttonListen.Size = new Size(140, 48);
      buttonListen.TabIndex = 3;
      buttonListen.Text = "Spy";
      buttonListen.UseVisualStyleBackColor = true;
      buttonListen.Click += buttonListen_Click;
      // 
      // listenIndicatorOff
      // 
      listenIndicatorOff.BackColor = Color.Red;
      listenIndicatorOff.Enabled = false;
      listenIndicatorOff.Location = new Point(159, 96);
      listenIndicatorOff.Margin = new Padding(4, 5, 4, 5);
      listenIndicatorOff.Name = "listenIndicatorOff";
      listenIndicatorOff.Size = new Size(24, 24);
      listenIndicatorOff.TabIndex = 2;
      listenIndicatorOff.UseVisualStyleBackColor = false;
      // 
      // groupBox1
      // 
      groupBox1.Controls.Add(label2);
      groupBox1.Controls.Add(label1);
      groupBox1.Controls.Add(disconnectionIndicator);
      groupBox1.Controls.Add(buttonConnect);
      groupBox1.Controls.Add(connectionIndicator);
      groupBox1.Dock = DockStyle.Top;
      groupBox1.Location = new Point(3, 3);
      groupBox1.Margin = new Padding(4, 5, 4, 5);
      groupBox1.Name = "groupBox1";
      groupBox1.Padding = new Padding(4, 5, 4, 5);
      groupBox1.Size = new Size(314, 96);
      groupBox1.TabIndex = 4;
      groupBox1.TabStop = false;
      groupBox1.Text = "Instrument Status";
      // 
      // label2
      // 
      label2.AutoSize = true;
      label2.Location = new Point(187, 60);
      label2.Margin = new Padding(4, 0, 4, 0);
      label2.Name = "label2";
      label2.Size = new Size(119, 25);
      label2.TabIndex = 4;
      label2.Text = "Disconnected";
      // 
      // label1
      // 
      label1.AutoSize = true;
      label1.Location = new Point(187, 37);
      label1.Margin = new Padding(4, 0, 4, 0);
      label1.Name = "label1";
      label1.Size = new Size(97, 25);
      label1.TabIndex = 3;
      label1.Text = "Connected";
      // 
      // disconnectionIndicator
      // 
      disconnectionIndicator.BackColor = Color.Red;
      disconnectionIndicator.Enabled = false;
      disconnectionIndicator.Location = new Point(159, 61);
      disconnectionIndicator.Margin = new Padding(4, 5, 4, 5);
      disconnectionIndicator.Name = "disconnectionIndicator";
      disconnectionIndicator.Size = new Size(24, 24);
      disconnectionIndicator.TabIndex = 2;
      disconnectionIndicator.UseVisualStyleBackColor = false;
      // 
      // buttonConnect
      // 
      buttonConnect.Location = new Point(4, 38);
      buttonConnect.Margin = new Padding(4, 5, 4, 5);
      buttonConnect.Name = "buttonConnect";
      buttonConnect.Size = new Size(140, 48);
      buttonConnect.TabIndex = 1;
      buttonConnect.Text = "Connect";
      buttonConnect.UseVisualStyleBackColor = true;
      buttonConnect.Click += buttonConnect_Click;
      // 
      // connectionIndicator
      // 
      connectionIndicator.BackColor = Color.Gray;
      connectionIndicator.Enabled = false;
      connectionIndicator.Location = new Point(159, 38);
      connectionIndicator.Margin = new Padding(4, 5, 4, 5);
      connectionIndicator.Name = "connectionIndicator";
      connectionIndicator.Size = new Size(24, 24);
      connectionIndicator.TabIndex = 0;
      connectionIndicator.UseVisualStyleBackColor = false;
      // 
      // splitContainer3
      // 
      splitContainer3.Dock = DockStyle.Fill;
      splitContainer3.Location = new Point(0, 0);
      splitContainer3.Margin = new Padding(4, 5, 4, 5);
      splitContainer3.Name = "splitContainer3";
      splitContainer3.Orientation = Orientation.Horizontal;
      // 
      // splitContainer3.Panel1
      // 
      splitContainer3.Panel1.Controls.Add(plotSpectrum);
      splitContainer3.Panel1.Controls.Add(lblScanFilter);
      splitContainer3.Panel1.Controls.Add(lblScanInfo);
      // 
      // splitContainer3.Panel2
      // 
      splitContainer3.Panel2.Controls.Add(rtbLog);
      splitContainer3.Size = new Size(797, 834);
      splitContainer3.SplitterDistance = 426;
      splitContainer3.SplitterWidth = 7;
      splitContainer3.TabIndex = 1;
      // 
      // plotSpectrum
      // 
      plotSpectrum.DisplayScale = 1F;
      plotSpectrum.Dock = DockStyle.Fill;
      plotSpectrum.Location = new Point(0, 50);
      plotSpectrum.Margin = new Padding(4, 5, 4, 5);
      plotSpectrum.Name = "plotSpectrum";
      plotSpectrum.Size = new Size(797, 376);
      plotSpectrum.TabIndex = 0;
      // 
      // lblScanFilter
      // 
      lblScanFilter.AutoSize = true;
      lblScanFilter.Dock = DockStyle.Top;
      lblScanFilter.Location = new Point(0, 25);
      lblScanFilter.Margin = new Padding(4, 0, 4, 0);
      lblScanFilter.Name = "lblScanFilter";
      lblScanFilter.Size = new Size(0, 25);
      lblScanFilter.TabIndex = 2;
      // 
      // lblScanInfo
      // 
      lblScanInfo.AutoSize = true;
      lblScanInfo.Dock = DockStyle.Top;
      lblScanInfo.Location = new Point(0, 0);
      lblScanInfo.Margin = new Padding(4, 0, 4, 0);
      lblScanInfo.Name = "lblScanInfo";
      lblScanInfo.Size = new Size(0, 25);
      lblScanInfo.TabIndex = 1;
      // 
      // rtbLog
      // 
      rtbLog.Dock = DockStyle.Fill;
      rtbLog.Location = new Point(0, 0);
      rtbLog.Margin = new Padding(4, 5, 4, 5);
      rtbLog.Name = "rtbLog";
      rtbLog.Size = new Size(797, 401);
      rtbLog.TabIndex = 0;
      rtbLog.Text = "";
      // 
      // toolStrip1
      // 
      toolStrip1.AutoSize = false;
      toolStrip1.ImageScalingSize = new Size(40, 40);
      toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton1 });
      toolStrip1.Location = new Point(0, 0);
      toolStrip1.Name = "toolStrip1";
      toolStrip1.Padding = new Padding(0);
      toolStrip1.Size = new Size(1120, 48);
      toolStrip1.TabIndex = 0;
      toolStrip1.Text = "toolStrip1";
      // 
      // toolStripButton1
      // 
      toolStripButton1.AutoSize = false;
      toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
      toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
      toolStripButton1.ImageTransparentColor = Color.Magenta;
      toolStripButton1.Name = "toolStripButton1";
      toolStripButton1.Size = new Size(44, 44);
      toolStripButton1.Text = "toolStripButton1";
      // 
      // statusStrip1
      // 
      statusStrip1.AutoSize = false;
      statusStrip1.ImageScalingSize = new Size(40, 40);
      statusStrip1.Location = new Point(0, 882);
      statusStrip1.Name = "statusStrip1";
      statusStrip1.Padding = new Padding(0, 0, 9, 0);
      statusStrip1.Size = new Size(1120, 53);
      statusStrip1.TabIndex = 1;
      statusStrip1.Text = "statusStrip1";
      // 
      // SpyIAPI
      // 
      AutoScaleDimensions = new SizeF(10F, 25F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(1120, 935);
      Controls.Add(splitContainer1);
      Controls.Add(toolStrip1);
      Controls.Add(statusStrip1);
      Margin = new Padding(1, 2, 1, 2);
      Name = "SpyIAPI";
      Text = "Spy-IAPI";
      splitContainer1.Panel1.ResumeLayout(false);
      splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
      splitContainer1.ResumeLayout(false);
      splitContainer2.Panel1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
      splitContainer2.ResumeLayout(false);
      groupBox2.ResumeLayout(false);
      groupBox2.PerformLayout();
      groupBox1.ResumeLayout(false);
      groupBox1.PerformLayout();
      splitContainer3.Panel1.ResumeLayout(false);
      splitContainer3.Panel1.PerformLayout();
      splitContainer3.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
      splitContainer3.ResumeLayout(false);
      toolStrip1.ResumeLayout(false);
      toolStrip1.PerformLayout();
      ResumeLayout(false);
    }

    #endregion

    private SplitContainer splitContainer1;
    private SplitContainer splitContainer2;
    private ToolStrip toolStrip1;
    private ToolStripButton toolStripButton1;
    private StatusStrip statusStrip1;
    private Button connectionIndicator;
    private Button buttonConnect;
    private Button buttonListen;
    private Button listenIndicatorOff;
    private RichTextBox rtbLog;
    private SplitContainer splitContainer3;
    private ScottPlot.WinForms.FormsPlot plotSpectrum;
    private Label lblScanFilter;
    private Label lblScanInfo;
    private GroupBox groupBox1;
    private Label label2;
    private Label label1;
    private Button disconnectionIndicator;
    private GroupBox groupBox2;
    private CheckBox cbOnAcquisition;
    private Label label5;
    private Label label4;
    private Label label3;
    private Button listenIndicatorOn;
    private Button listenIndicatorWait;
  }
}
