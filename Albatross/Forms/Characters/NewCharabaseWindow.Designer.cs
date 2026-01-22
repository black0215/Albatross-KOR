namespace Albatross.Forms.Characters
{
    partial class NewCharabaseWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewCharabaseWindow));
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.modelFlatComboBox = new Albatross.UI.FlatComboBox();
            this.isYokaiFlatCheckBox = new Albatross.UI.FlatCheckBox();
            this.addButton = new System.Windows.Forms.Button();
            this.modelTypeVSTabControl = new Albatross.UI.VSTabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label4 = new System.Windows.Forms.Label();
            this.newVarianceFlatNumericUpDown1 = new Albatross.UI.FlatNumericUpDown();
            this.newNumberFlatNumericUpDown = new Albatross.UI.FlatNumericUpDown();
            this.newNameFlatComboBox = new Albatross.UI.FlatComboBox();
            this.newIconTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.newModelTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.modelTypeVSTabControl.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.newVarianceFlatNumericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.newNumberFlatNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // nameTextBox
            // 
            this.nameTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.nameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.nameTextBox.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.nameTextBox.Location = new System.Drawing.Point(75, 19);
            this.nameTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.ReadOnly = true;
            this.nameTextBox.Size = new System.Drawing.Size(132, 14);
            this.nameTextBox.TabIndex = 63;
            this.nameTextBox.Click += new System.EventHandler(this.NameTextBox_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(26, 19);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(39, 12);
            this.label5.TabIndex = 62;
            this.label5.Text = "Name";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(19, 29);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 76;
            this.label1.Text = "Model";
            // 
            // modelFlatComboBox
            // 
            this.modelFlatComboBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.modelFlatComboBox.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(54)))), ((int)(((byte)(54)))));
            this.modelFlatComboBox.ButtonColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(88)))), ((int)(((byte)(88)))));
            this.modelFlatComboBox.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.modelFlatComboBox.FormattingEnabled = true;
            this.modelFlatComboBox.Location = new System.Drawing.Point(72, 26);
            this.modelFlatComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.modelFlatComboBox.Name = "modelFlatComboBox";
            this.modelFlatComboBox.Size = new System.Drawing.Size(131, 21);
            this.modelFlatComboBox.TabIndex = 77;
            // 
            // isYokaiFlatCheckBox
            // 
            this.isYokaiFlatCheckBox.AutoSize = true;
            this.isYokaiFlatCheckBox.CheckMarkColor = System.Drawing.Color.White;
            this.isYokaiFlatCheckBox.Location = new System.Drawing.Point(229, 18);
            this.isYokaiFlatCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.isYokaiFlatCheckBox.Name = "isYokaiFlatCheckBox";
            this.isYokaiFlatCheckBox.Size = new System.Drawing.Size(90, 16);
            this.isYokaiFlatCheckBox.TabIndex = 79;
            this.isYokaiFlatCheckBox.Text = "Make Yokai";
            this.isYokaiFlatCheckBox.UseVisualStyleBackColor = true;
            // 
            // addButton
            // 
            this.addButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.addButton.ForeColor = System.Drawing.Color.White;
            this.addButton.Location = new System.Drawing.Point(29, 171);
            this.addButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(301, 21);
            this.addButton.TabIndex = 80;
            this.addButton.Text = "추가";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.AddButton_Click);
            // 
            // modelTypeVSTabControl
            // 
            this.modelTypeVSTabControl.ActiveIndicator = System.Drawing.Color.White;
            this.modelTypeVSTabControl.ActiveTab = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(54)))), ((int)(((byte)(54)))));
            this.modelTypeVSTabControl.ActiveText = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.modelTypeVSTabControl.Background = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(54)))), ((int)(((byte)(54)))));
            this.modelTypeVSTabControl.BackgroundTab = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(54)))), ((int)(((byte)(54)))));
            this.modelTypeVSTabControl.Border = System.Drawing.Color.White;
            this.modelTypeVSTabControl.Controls.Add(this.tabPage3);
            this.modelTypeVSTabControl.Controls.Add(this.tabPage2);
            this.modelTypeVSTabControl.Divider = System.Drawing.Color.White;
            this.modelTypeVSTabControl.Font = new System.Drawing.Font("Leelawadee UI", 8.25F);
            this.modelTypeVSTabControl.InActiveIndicator = System.Drawing.Color.White;
            this.modelTypeVSTabControl.InActiveTab = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(54)))), ((int)(((byte)(54)))));
            this.modelTypeVSTabControl.InActiveText = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.modelTypeVSTabControl.Location = new System.Drawing.Point(29, 48);
            this.modelTypeVSTabControl.Margin = new System.Windows.Forms.Padding(0);
            this.modelTypeVSTabControl.Name = "modelTypeVSTabControl";
            this.modelTypeVSTabControl.Padding = new System.Drawing.Point(0, 0);
            this.modelTypeVSTabControl.SelectedIndex = 0;
            this.modelTypeVSTabControl.Size = new System.Drawing.Size(301, 111);
            this.modelTypeVSTabControl.TabIndex = 81;
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(54)))), ((int)(((byte)(54)))));
            this.tabPage3.Controls.Add(this.label1);
            this.tabPage3.Controls.Add(this.modelFlatComboBox);
            this.tabPage3.Location = new System.Drawing.Point(4, 26);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage3.Size = new System.Drawing.Size(293, 81);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "기존 메달";
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(54)))), ((int)(((byte)(54)))));
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.newVarianceFlatNumericUpDown1);
            this.tabPage2.Controls.Add(this.newNumberFlatNumericUpDown);
            this.tabPage2.Controls.Add(this.newNameFlatComboBox);
            this.tabPage2.Controls.Add(this.newIconTextBox);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.newModelTextBox);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Location = new System.Drawing.Point(4, 26);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage2.Size = new System.Drawing.Size(293, 81);
            this.tabPage2.TabIndex = 2;
            this.tabPage2.Text = "새 메달";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(19, 54);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(36, 13);
            this.label4.TabIndex = 73;
            this.label4.Text = "Name";
            // 
            // newVarianceFlatNumericUpDown1
            // 
            this.newVarianceFlatNumericUpDown1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.newVarianceFlatNumericUpDown1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(54)))), ((int)(((byte)(54)))));
            this.newVarianceFlatNumericUpDown1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.newVarianceFlatNumericUpDown1.ButtonHighlightColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(88)))), ((int)(((byte)(88)))));
            this.newVarianceFlatNumericUpDown1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.newVarianceFlatNumericUpDown1.Location = new System.Drawing.Point(208, 51);
            this.newVarianceFlatNumericUpDown1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.newVarianceFlatNumericUpDown1.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.newVarianceFlatNumericUpDown1.Name = "newVarianceFlatNumericUpDown1";
            this.newVarianceFlatNumericUpDown1.Size = new System.Drawing.Size(70, 22);
            this.newVarianceFlatNumericUpDown1.TabIndex = 72;
            // 
            // newNumberFlatNumericUpDown
            // 
            this.newNumberFlatNumericUpDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.newNumberFlatNumericUpDown.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(54)))), ((int)(((byte)(54)))));
            this.newNumberFlatNumericUpDown.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.newNumberFlatNumericUpDown.ButtonHighlightColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(88)))), ((int)(((byte)(88)))));
            this.newNumberFlatNumericUpDown.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.newNumberFlatNumericUpDown.Location = new System.Drawing.Point(131, 51);
            this.newNumberFlatNumericUpDown.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.newNumberFlatNumericUpDown.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.newNumberFlatNumericUpDown.Name = "newNumberFlatNumericUpDown";
            this.newNumberFlatNumericUpDown.Size = new System.Drawing.Size(70, 22);
            this.newNumberFlatNumericUpDown.TabIndex = 71;
            // 
            // newNameFlatComboBox
            // 
            this.newNameFlatComboBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.newNameFlatComboBox.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(54)))), ((int)(((byte)(54)))));
            this.newNameFlatComboBox.ButtonColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(88)))), ((int)(((byte)(88)))));
            this.newNameFlatComboBox.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.newNameFlatComboBox.FormattingEnabled = true;
            this.newNameFlatComboBox.Location = new System.Drawing.Point(72, 51);
            this.newNameFlatComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.newNameFlatComboBox.Name = "newNameFlatComboBox";
            this.newNameFlatComboBox.Size = new System.Drawing.Size(51, 21);
            this.newNameFlatComboBox.TabIndex = 68;
            // 
            // newIconTextBox
            // 
            this.newIconTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.newIconTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.newIconTextBox.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.newIconTextBox.Location = new System.Drawing.Point(72, 31);
            this.newIconTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.newIconTextBox.Name = "newIconTextBox";
            this.newIconTextBox.ReadOnly = true;
            this.newIconTextBox.Size = new System.Drawing.Size(205, 15);
            this.newIconTextBox.TabIndex = 67;
            this.newIconTextBox.Click += new System.EventHandler(this.NewIconTextBox_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(19, 31);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 66;
            this.label3.Text = "Icon";
            // 
            // newModelTextBox
            // 
            this.newModelTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.newModelTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.newModelTextBox.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.newModelTextBox.Location = new System.Drawing.Point(72, 12);
            this.newModelTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.newModelTextBox.Name = "newModelTextBox";
            this.newModelTextBox.ReadOnly = true;
            this.newModelTextBox.Size = new System.Drawing.Size(205, 15);
            this.newModelTextBox.TabIndex = 65;
            this.newModelTextBox.Click += new System.EventHandler(this.NewModelTextBox_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(19, 12);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 64;
            this.label2.Text = "Model";
            // 
            // NewCharabaseWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(54)))), ((int)(((byte)(54)))));
            this.ClientSize = new System.Drawing.Size(345, 210);
            this.Controls.Add(this.modelTypeVSTabControl);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.isYokaiFlatCheckBox);
            this.Controls.Add(this.nameTextBox);
            this.Controls.Add(this.label5);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewCharabaseWindow";
            this.Text = "Make Charabase";
            this.Load += new System.EventHandler(this.NewCharabaseWindow_Load);
            this.modelTypeVSTabControl.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.newVarianceFlatNumericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.newNumberFlatNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private UI.FlatComboBox modelFlatComboBox;
        private UI.FlatCheckBox isYokaiFlatCheckBox;
        private System.Windows.Forms.Button addButton;
        private UI.VSTabControl modelTypeVSTabControl;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage2;
        private UI.FlatComboBox newNameFlatComboBox;
        private System.Windows.Forms.TextBox newIconTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox newModelTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private UI.FlatNumericUpDown newVarianceFlatNumericUpDown1;
        private UI.FlatNumericUpDown newNumberFlatNumericUpDown;
    }
}