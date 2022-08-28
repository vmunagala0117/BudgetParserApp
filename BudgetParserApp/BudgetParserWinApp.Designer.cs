using System;

namespace BudgetParserApp
{
    partial class BudgetParserWinApp
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
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnProcess = new System.Windows.Forms.Button();
            this.startDate = new System.Windows.Forms.DateTimePicker();
            this.endDate = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog";
            this.openFileDialog.Filter = "CSV files (*.csv)|*.csv";
            // 
            // txtFilePath
            // 
            this.txtFilePath.Location = new System.Drawing.Point(62, 95);
            this.txtFilePath.Margin = new System.Windows.Forms.Padding(6);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(750, 38);
            this.txtFilePath.TabIndex = 0;
            this.txtFilePath.Text = "C:\\Users\\vmunagal\\Downloads\\transactions.csv";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(846, 95);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(6);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(94, 45);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // btnProcess
            // 
            this.btnProcess.Location = new System.Drawing.Point(228, 343);
            this.btnProcess.Margin = new System.Windows.Forms.Padding(6);
            this.btnProcess.Name = "btnProcess";
            this.btnProcess.Size = new System.Drawing.Size(322, 54);
            this.btnProcess.TabIndex = 2;
            this.btnProcess.Text = "Process";
            this.btnProcess.UseVisualStyleBackColor = true;
            this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
            // 
            // startDate
            // 
            this.startDate.CalendarMonthBackground = System.Drawing.Color.Wheat;
            this.startDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.startDate.Location = new System.Drawing.Point(102, 217);
            this.startDate.Margin = new System.Windows.Forms.Padding(6);
            this.startDate.Name = "startDate";
            this.startDate.Size = new System.Drawing.Size(260, 38);
            this.startDate.TabIndex = 3;
            this.startDate.Value = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);
            // 
            // endDate
            // 
            this.endDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.endDate.Location = new System.Drawing.Point(446, 219);
            this.endDate.Margin = new System.Windows.Forms.Padding(6);
            this.endDate.Name = "endDate";
            this.endDate.Size = new System.Drawing.Size(260, 38);
            this.endDate.TabIndex = 4;
            this.endDate.Value = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, DateTime.DaysInMonth(System.DateTime.Now.Year, System.DateTime.Now.Month));
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(378, 227);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 32);
            this.label1.TabIndex = 5;
            this.label1.Text = "TO";
            // 
            // BudgetParserWinApp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(954, 451);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.endDate);
            this.Controls.Add(this.startDate);
            this.Controls.Add(this.btnProcess);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtFilePath);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "BudgetParserWinApp";
            this.Text = "Budget Parser App";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnProcess;
        private System.Windows.Forms.DateTimePicker startDate;
        private System.Windows.Forms.DateTimePicker endDate;
        private System.Windows.Forms.Label label1;
    }
}

