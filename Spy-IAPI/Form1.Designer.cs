namespace Spy_IAPI
{
  partial class Form1
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
      splitContainer1 = new SplitContainer();
      splitContainer2 = new SplitContainer();
      buttonListen = new Button();
      listenIndicator = new Button();
      buttonConnect = new Button();
      connectionIndicator = new Button();
      toolStrip1 = new ToolStrip();
      toolStripButton1 = new ToolStripButton();
      statusStrip1 = new StatusStrip();
      rtbLog = new RichTextBox();
      ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
      splitContainer1.Panel1.SuspendLayout();
      splitContainer1.Panel2.SuspendLayout();
      splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
      splitContainer2.Panel1.SuspendLayout();
      splitContainer2.SuspendLayout();
      toolStrip1.SuspendLayout();
      SuspendLayout();
      // 
      // splitContainer1
      // 
      splitContainer1.Dock = DockStyle.Fill;
      splitContainer1.Location = new Point(0, 48);
      splitContainer1.Margin = new Padding(1);
      splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      splitContainer1.Panel1.Controls.Add(splitContainer2);
      splitContainer1.Panel1MinSize = 240;
      // 
      // splitContainer1.Panel2
      // 
      splitContainer1.Panel2.Controls.Add(rtbLog);
      splitContainer1.Size = new Size(784, 481);
      splitContainer1.SplitterDistance = 240;
      splitContainer1.SplitterWidth = 2;
      splitContainer1.TabIndex = 2;
      // 
      // splitContainer2
      // 
      splitContainer2.Dock = DockStyle.Fill;
      splitContainer2.Location = new Point(0, 0);
      splitContainer2.Margin = new Padding(1);
      splitContainer2.Name = "splitContainer2";
      splitContainer2.Orientation = Orientation.Horizontal;
      // 
      // splitContainer2.Panel1
      // 
      splitContainer2.Panel1.Controls.Add(buttonListen);
      splitContainer2.Panel1.Controls.Add(listenIndicator);
      splitContainer2.Panel1.Controls.Add(buttonConnect);
      splitContainer2.Panel1.Controls.Add(connectionIndicator);
      splitContainer2.Panel1.Padding = new Padding(2);
      splitContainer2.Size = new Size(240, 481);
      splitContainer2.SplitterDistance = 156;
      splitContainer2.SplitterWidth = 1;
      splitContainer2.TabIndex = 0;
      // 
      // buttonListen
      // 
      buttonListen.Location = new Point(5, 88);
      buttonListen.Name = "buttonListen";
      buttonListen.Size = new Size(100, 36);
      buttonListen.TabIndex = 3;
      buttonListen.Text = "Listen";
      buttonListen.UseVisualStyleBackColor = true;
      // 
      // listenIndicator
      // 
      listenIndicator.BackColor = Color.Red;
      listenIndicator.Location = new Point(2, 66);
      listenIndicator.Name = "listenIndicator";
      listenIndicator.Size = new Size(236, 16);
      listenIndicator.TabIndex = 2;
      listenIndicator.UseVisualStyleBackColor = false;
      // 
      // buttonConnect
      // 
      buttonConnect.Location = new Point(5, 24);
      buttonConnect.Name = "buttonConnect";
      buttonConnect.Size = new Size(100, 36);
      buttonConnect.TabIndex = 1;
      buttonConnect.Text = "Connect";
      buttonConnect.UseVisualStyleBackColor = true;
      buttonConnect.Click += buttonConnect_Click;
      // 
      // connectionIndicator
      // 
      connectionIndicator.BackColor = Color.Red;
      connectionIndicator.Dock = DockStyle.Top;
      connectionIndicator.Location = new Point(2, 2);
      connectionIndicator.Name = "connectionIndicator";
      connectionIndicator.Size = new Size(236, 16);
      connectionIndicator.TabIndex = 0;
      connectionIndicator.UseVisualStyleBackColor = false;
      // 
      // toolStrip1
      // 
      toolStrip1.AutoSize = false;
      toolStrip1.ImageScalingSize = new Size(40, 40);
      toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton1 });
      toolStrip1.Location = new Point(0, 0);
      toolStrip1.Name = "toolStrip1";
      toolStrip1.Padding = new Padding(0);
      toolStrip1.Size = new Size(784, 48);
      toolStrip1.TabIndex = 0;
      toolStrip1.Text = "toolStrip1";
      // 
      // toolStripButton1
      // 
      toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
      toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
      toolStripButton1.ImageTransparentColor = Color.Magenta;
      toolStripButton1.Name = "toolStripButton1";
      toolStripButton1.Size = new Size(44, 45);
      toolStripButton1.Text = "toolStripButton1";
      // 
      // statusStrip1
      // 
      statusStrip1.AutoSize = false;
      statusStrip1.ImageScalingSize = new Size(40, 40);
      statusStrip1.Location = new Point(0, 529);
      statusStrip1.Name = "statusStrip1";
      statusStrip1.Padding = new Padding(0, 0, 6, 0);
      statusStrip1.Size = new Size(784, 32);
      statusStrip1.TabIndex = 1;
      statusStrip1.Text = "statusStrip1";
      // 
      // rtbLog
      // 
      rtbLog.Dock = DockStyle.Fill;
      rtbLog.Location = new Point(0, 0);
      rtbLog.Name = "rtbLog";
      rtbLog.Size = new Size(542, 481);
      rtbLog.TabIndex = 0;
      rtbLog.Text = "";
      // 
      // Form1
      // 
      AutoScaleDimensions = new SizeF(7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(784, 561);
      Controls.Add(splitContainer1);
      Controls.Add(toolStrip1);
      Controls.Add(statusStrip1);
      Margin = new Padding(1);
      Name = "Form1";
      Text = "Spy-IAPI";
      splitContainer1.Panel1.ResumeLayout(false);
      splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
      splitContainer1.ResumeLayout(false);
      splitContainer2.Panel1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
      splitContainer2.ResumeLayout(false);
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
    private Button listenIndicator;
    private RichTextBox rtbLog;
  }
}
