namespace ScanInjector
{
  partial class TargetMZ
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
      this.label1 = new System.Windows.Forms.Label();
      this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
      this.butCancel = new System.Windows.Forms.Button();
      this.butOK = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(29, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(108, 17);
      this.label1.TabIndex = 0;
      this.label1.Text = "Enter Target m/z:";
      // 
      // numericUpDown1
      // 
      this.numericUpDown1.DecimalPlaces = 4;
      this.numericUpDown1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.numericUpDown1.Location = new System.Drawing.Point(32, 36);
      this.numericUpDown1.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
      this.numericUpDown1.Name = "numericUpDown1";
      this.numericUpDown1.Size = new System.Drawing.Size(140, 25);
      this.numericUpDown1.TabIndex = 1;
      // 
      // butCancel
      // 
      this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.butCancel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.butCancel.Location = new System.Drawing.Point(12, 82);
      this.butCancel.Name = "butCancel";
      this.butCancel.Size = new System.Drawing.Size(96, 32);
      this.butCancel.TabIndex = 2;
      this.butCancel.Text = "Cancel";
      this.butCancel.UseVisualStyleBackColor = true;
      // 
      // butOK
      // 
      this.butOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.butOK.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.butOK.Location = new System.Drawing.Point(114, 82);
      this.butOK.Name = "butOK";
      this.butOK.Size = new System.Drawing.Size(96, 32);
      this.butOK.TabIndex = 3;
      this.butOK.Text = "OK";
      this.butOK.UseVisualStyleBackColor = true;
      this.butOK.Click += new System.EventHandler(this.butOK_Click);
      // 
      // TargetMZ
      // 
      this.AcceptButton = this.butOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.butCancel;
      this.ClientSize = new System.Drawing.Size(224, 126);
      this.ControlBox = false;
      this.Controls.Add(this.butOK);
      this.Controls.Add(this.butCancel);
      this.Controls.Add(this.numericUpDown1);
      this.Controls.Add(this.label1);
      this.Name = "TargetMZ";
      this.Text = "Target m/z";
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.NumericUpDown numericUpDown1;
    private System.Windows.Forms.Button butCancel;
    private System.Windows.Forms.Button butOK;
  }
}