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
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.splitContainer2 = new System.Windows.Forms.SplitContainer();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.disconnectionIndicator = new System.Windows.Forms.Button();
      this.connectionIndicator = new System.Windows.Forms.Button();
      this.buttonConnect = new System.Windows.Forms.Button();
      this.dgvScan = new System.Windows.Forms.DataGridView();
      this.rtbLog = new System.Windows.Forms.RichTextBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dgvScan)).BeginInit();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
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
      this.splitContainer1.Panel2.Controls.Add(this.rtbLog);
      this.splitContainer1.Size = new System.Drawing.Size(800, 561);
      this.splitContainer1.SplitterDistance = 331;
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
      this.splitContainer2.Panel1.Controls.Add(this.groupBox1);
      // 
      // splitContainer2.Panel2
      // 
      this.splitContainer2.Panel2.Controls.Add(this.groupBox2);
      this.splitContainer2.Size = new System.Drawing.Size(800, 331);
      this.splitContainer2.SplitterDistance = 220;
      this.splitContainer2.TabIndex = 0;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.disconnectionIndicator);
      this.groupBox1.Controls.Add(this.connectionIndicator);
      this.groupBox1.Controls.Add(this.buttonConnect);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
      this.label2.Size = new System.Drawing.Size(79, 15);
      this.label2.TabIndex = 4;
      this.label2.Text = "Disconnected";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(128, 27);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(65, 15);
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
      // dgvScan
      // 
      this.dgvScan.AllowUserToAddRows = false;
      this.dgvScan.AllowUserToDeleteRows = false;
      this.dgvScan.AllowUserToOrderColumns = true;
      this.dgvScan.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
      this.dgvScan.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dgvScan.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dgvScan.Location = new System.Drawing.Point(3, 16);
      this.dgvScan.Name = "dgvScan";
      this.dgvScan.Size = new System.Drawing.Size(570, 312);
      this.dgvScan.TabIndex = 0;
      // 
      // rtbLog
      // 
      this.rtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
      this.rtbLog.Location = new System.Drawing.Point(0, 0);
      this.rtbLog.Name = "rtbLog";
      this.rtbLog.Size = new System.Drawing.Size(800, 226);
      this.rtbLog.TabIndex = 0;
      this.rtbLog.Text = "";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.dgvScan);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox2.Location = new System.Drawing.Point(0, 0);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(576, 331);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "groupBox2";
      // 
      // ScanInjector
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(800, 561);
      this.Controls.Add(this.splitContainer1);
      this.Name = "ScanInjector";
      this.Text = "ScanInjector";
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      this.splitContainer2.Panel1.ResumeLayout(false);
      this.splitContainer2.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
      this.splitContainer2.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dgvScan)).EndInit();
      this.groupBox2.ResumeLayout(false);
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
    private System.Windows.Forms.DataGridView dgvScan;
    private System.Windows.Forms.RichTextBox rtbLog;
    private System.Windows.Forms.GroupBox groupBox2;
  }
}

