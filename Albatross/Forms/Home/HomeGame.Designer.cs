
namespace Albatross
{
    partial class HomeGame
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HomeGame));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.charascaleButton = new System.Windows.Forms.Button();
            this.charaparamButton = new System.Windows.Forms.Button();
            this.charabaseButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.featureListBox = new System.Windows.Forms.ListBox();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.charascaleButton);
            this.groupBox1.Controls.Add(this.charaparamButton);
            this.groupBox1.Controls.Add(this.charabaseButton);
            this.groupBox1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.groupBox1.Location = new System.Drawing.Point(273, 25);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(252, 376);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "캐릭터";
            // 
            // charascaleButton
            // 
            this.charascaleButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.charascaleButton.Location = new System.Drawing.Point(7, 44);
            this.charascaleButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.charascaleButton.Name = "charascaleButton";
            this.charascaleButton.Size = new System.Drawing.Size(233, 21);
            this.charascaleButton.TabIndex = 2;
            this.charascaleButton.Text = "캐릭터 비율";
            this.charascaleButton.UseVisualStyleBackColor = true;
            this.charascaleButton.Click += new System.EventHandler(this.CharascaleButton_Click);
            // 
            // charaparamButton
            // 
            this.charaparamButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.charaparamButton.Location = new System.Drawing.Point(7, 71);
            this.charaparamButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.charaparamButton.Name = "charaparamButton";
            this.charaparamButton.Size = new System.Drawing.Size(233, 21);
            this.charaparamButton.TabIndex = 1;
            this.charaparamButton.Text = "요괴 능력치";
            this.charaparamButton.UseVisualStyleBackColor = true;
            this.charaparamButton.Click += new System.EventHandler(this.CharaparamButton_Click);
            // 
            // charabaseButton
            // 
            this.charabaseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.charabaseButton.Location = new System.Drawing.Point(7, 18);
            this.charabaseButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.charabaseButton.Name = "charabaseButton";
            this.charabaseButton.Size = new System.Drawing.Size(233, 21);
            this.charabaseButton.TabIndex = 0;
            this.charabaseButton.Text = "캐릭터 기본정보";
            this.charabaseButton.UseVisualStyleBackColor = true;
            this.charabaseButton.Click += new System.EventHandler(this.CharabaseButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.featureListBox);
            this.groupBox2.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.groupBox2.Location = new System.Drawing.Point(14, 25);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Size = new System.Drawing.Size(252, 376);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "현재 기능";
            // 
            // featureListBox
            // 
            this.featureListBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.featureListBox.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.featureListBox.FormattingEnabled = true;
            this.featureListBox.ItemHeight = 12;
            this.featureListBox.Items.AddRange(new object[] {
            "캐릭터_기본정보",
            "캐릭터_비율",
            "요괴_능력치"});
            this.featureListBox.Location = new System.Drawing.Point(7, 18);
            this.featureListBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.featureListBox.Name = "featureListBox";
            this.featureListBox.Size = new System.Drawing.Size(237, 352);
            this.featureListBox.TabIndex = 4;
            this.featureListBox.SelectedIndexChanged += new System.EventHandler(this.FeatureListBox_SelectedIndexChanged);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Enabled = false;
            this.optionsToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.optionsToolStripMenuItem.Text = "옵션";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Enabled = false;
            this.helpToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
            this.helpToolStripMenuItem.Text = "도움말";
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(54)))), ((int)(((byte)(54)))));
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(539, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.saveToolStripMenuItem.Text = "저장";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.SaveToolStripMenuItem_Click_1);
            // 
            // HomeGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(54)))), ((int)(((byte)(54)))));
            this.ClientSize = new System.Drawing.Size(539, 408);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximumSize = new System.Drawing.Size(555, 447);
            this.MinimumSize = new System.Drawing.Size(555, 447);
            this.Name = "HomeGame";
            this.Text = "Albatross";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HomeGame_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button charabaseButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox featureListBox;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.Button charaparamButton;
        private System.Windows.Forms.Button charascaleButton;
    }
}

