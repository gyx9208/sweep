namespace ManualClassify
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.isUserButton = new System.Windows.Forms.Button();
            this.isWasteButton = new System.Windows.Forms.Button();
            this.passButton = new System.Windows.Forms.Button();
            this.countLabel = new System.Windows.Forms.Label();
            this.userNameLabel = new System.Windows.Forms.Label();
            this.urlLabel = new System.Windows.Forms.Label();
            this.postLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // isUserButton
            // 
            this.isUserButton.Location = new System.Drawing.Point(358, 317);
            this.isUserButton.Name = "isUserButton";
            this.isUserButton.Size = new System.Drawing.Size(75, 23);
            this.isUserButton.TabIndex = 0;
            this.isUserButton.Text = "用户";
            this.isUserButton.UseVisualStyleBackColor = true;
            this.isUserButton.Click += new System.EventHandler(this.isUserButton_Click);
            // 
            // isWasteButton
            // 
            this.isWasteButton.Location = new System.Drawing.Point(459, 317);
            this.isWasteButton.Name = "isWasteButton";
            this.isWasteButton.Size = new System.Drawing.Size(75, 23);
            this.isWasteButton.TabIndex = 1;
            this.isWasteButton.Text = "营销";
            this.isWasteButton.UseVisualStyleBackColor = true;
            this.isWasteButton.Click += new System.EventHandler(this.isWasteButton_Click);
            // 
            // passButton
            // 
            this.passButton.Location = new System.Drawing.Point(556, 317);
            this.passButton.Name = "passButton";
            this.passButton.Size = new System.Drawing.Size(75, 23);
            this.passButton.TabIndex = 2;
            this.passButton.Text = "待定";
            this.passButton.UseVisualStyleBackColor = true;
            this.passButton.Click += new System.EventHandler(this.passButton_Click);
            // 
            // countLabel
            // 
            this.countLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.countLabel.AutoSize = true;
            this.countLabel.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.countLabel.Location = new System.Drawing.Point(599, 9);
            this.countLabel.Name = "countLabel";
            this.countLabel.Size = new System.Drawing.Size(32, 16);
            this.countLabel.TabIndex = 3;
            this.countLabel.Text = "0/N";
            // 
            // userNameLabel
            // 
            this.userNameLabel.AutoSize = true;
            this.userNameLabel.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.userNameLabel.Location = new System.Drawing.Point(12, 30);
            this.userNameLabel.Name = "userNameLabel";
            this.userNameLabel.Size = new System.Drawing.Size(56, 16);
            this.userNameLabel.TabIndex = 5;
            this.userNameLabel.Text = "用户名";
            // 
            // urlLabel
            // 
            this.urlLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.urlLabel.AutoSize = true;
            this.urlLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.urlLabel.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.urlLabel.ForeColor = System.Drawing.Color.Blue;
            this.urlLabel.Location = new System.Drawing.Point(224, 34);
            this.urlLabel.Margin = new System.Windows.Forms.Padding(10, 0, 3, 0);
            this.urlLabel.Name = "urlLabel";
            this.urlLabel.Size = new System.Drawing.Size(59, 12);
            this.urlLabel.TabIndex = 6;
            this.urlLabel.Text = "url label";
            this.urlLabel.Click += new System.EventHandler(this.urlLabel_Click);
            // 
            // postLayoutPanel
            // 
            this.postLayoutPanel.AutoScroll = true;
            this.postLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.postLayoutPanel.Location = new System.Drawing.Point(27, 62);
            this.postLayoutPanel.Name = "postLayoutPanel";
            this.postLayoutPanel.Size = new System.Drawing.Size(520, 222);
            this.postLayoutPanel.TabIndex = 7;
            this.postLayoutPanel.WrapContents = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(647, 352);
            this.Controls.Add(this.postLayoutPanel);
            this.Controls.Add(this.urlLabel);
            this.Controls.Add(this.userNameLabel);
            this.Controls.Add(this.countLabel);
            this.Controls.Add(this.passButton);
            this.Controls.Add(this.isWasteButton);
            this.Controls.Add(this.isUserButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "手动分类";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button isUserButton;
        private System.Windows.Forms.Button isWasteButton;
        private System.Windows.Forms.Button passButton;
        private System.Windows.Forms.Label countLabel;
        private System.Windows.Forms.Label userNameLabel;
        private System.Windows.Forms.Label urlLabel;
        private System.Windows.Forms.FlowLayoutPanel postLayoutPanel;
    }
}

