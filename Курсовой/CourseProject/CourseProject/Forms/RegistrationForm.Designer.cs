using System.Drawing;
using System.Windows.Forms;
using System;

namespace CourseProject
{
    partial class RegistrationForm
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
            this.label1 = new Label();
            this.textBoxLogin = new TextBox();
            this.label2 = new Label();
            this.textBoxPassword = new TextBox();
            this.label3 = new Label();
            this.textBoxConfirmPassword = new TextBox();
            this.label4 = new Label();
            this.textBoxSurname = new TextBox();
            this.label5 = new Label();
            this.textBoxName = new TextBox();
            this.label6 = new Label();
            this.textBoxPatronymic = new TextBox();
            this.label7 = new Label();
            this.textBoxPhone = new TextBox();
            this.btnRegister = new Button();
            this.btnCancel = new Button();
            this.groupBox1 = new GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();

            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new Point(16, 30);
            this.label1.Name = "label1";
            this.label1.Size = new Size(44, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Логин:*";
            // 
            // textBoxLogin
            // 
            this.textBoxLogin.Location = new Point(120, 27);
            this.textBoxLogin.Name = "textBoxLogin";
            this.textBoxLogin.Size = new Size(200, 20);
            this.textBoxLogin.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new Point(16, 60);
            this.label2.Name = "label2";
            this.label2.Size = new Size(51, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Пароль:*";
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new Point(120, 57);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new Size(200, 20);
            this.textBoxPassword.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new Point(16, 90);
            this.label3.Name = "label3";
            this.label3.Size = new Size(98, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Подтверждение:*";
            // 
            // textBoxConfirmPassword
            // 
            this.textBoxConfirmPassword.Location = new Point(120, 87);
            this.textBoxConfirmPassword.Name = "textBoxConfirmPassword";
            this.textBoxConfirmPassword.PasswordChar = '*';
            this.textBoxConfirmPassword.Size = new Size(200, 20);
            this.textBoxConfirmPassword.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new Point(16, 120);
            this.label4.Name = "label4";
            this.label4.Size = new Size(62, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Фамилия:*";
            // 
            // textBoxSurname
            // 
            this.textBoxSurname.Location = new Point(120, 117);
            this.textBoxSurname.Name = "textBoxSurname";
            this.textBoxSurname.Size = new Size(200, 20);
            this.textBoxSurname.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new Point(16, 150);
            this.label5.Name = "label5";
            this.label5.Size = new Size(35, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Имя:*";
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new Point(120, 147);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new Size(200, 20);
            this.textBoxName.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new Point(16, 180);
            this.label6.Name = "label6";
            this.label6.Size = new Size(57, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Отчество:";
            // 
            // textBoxPatronymic
            // 
            this.textBoxPatronymic.Location = new Point(120, 177);
            this.textBoxPatronymic.Name = "textBoxPatronymic";
            this.textBoxPatronymic.Size = new Size(200, 20);
            this.textBoxPatronymic.TabIndex = 11;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new Point(16, 210);
            this.label7.Name = "label7";
            this.label7.Size = new Size(96, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "Номер телефона:*";
            // 
            // textBoxPhone
            // 
            this.textBoxPhone.Location = new Point(120, 207);
            this.textBoxPhone.Name = "textBoxPhone";
            this.textBoxPhone.Size = new Size(200, 20);
            this.textBoxPhone.TabIndex = 13;
            this.textBoxPhone.KeyPress += new KeyPressEventHandler(this.textBoxPhone_KeyPress);
            // 

            this.btnRegister.Location = new Point(120, 280);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new Size(95, 30);
            this.btnRegister.TabIndex = 16;
            this.btnRegister.Text = "Регистрация";
            this.btnRegister.UseVisualStyleBackColor = true;
            this.btnRegister.Click += new EventHandler(this.btnRegister_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new Point(225, 280);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(95, 30);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btnCancel);
            this.groupBox1.Controls.Add(this.textBoxLogin);
            this.groupBox1.Controls.Add(this.btnRegister);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBoxPassword);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBoxPhone);
            this.groupBox1.Controls.Add(this.textBoxConfirmPassword);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textBoxPatronymic);
            this.groupBox1.Controls.Add(this.textBoxSurname);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.textBoxName);
            this.groupBox1.Location = new Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(340, 330);
            this.groupBox1.TabIndex = 18;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Регистрация нового пользователя";
            // 
            // RegistrationForm
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(364, 354);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RegistrationForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Регистрация";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
        }

        private Label label1;
        private TextBox textBoxLogin;
        private Label label2;
        private TextBox textBoxPassword;
        private Label label3;
        private TextBox textBoxConfirmPassword;
        private Label label4;
        private TextBox textBoxSurname;
        private Label label5;
        private TextBox textBoxName;
        private Label label6;
        private TextBox textBoxPatronymic;
        private Label label7;
        private TextBox textBoxPhone;
        private Button btnRegister;
        private Button btnCancel;
        private GroupBox groupBox1;
    }

        #endregion
    
}
