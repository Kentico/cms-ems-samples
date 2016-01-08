namespace UsingKenticoLibrariesInForms
{
	partial class PageTypeStatisticsForm
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
			System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
			System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
			System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
			this.docStatsChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
			this.loadButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.docStatsChart)).BeginInit();
			this.SuspendLayout();
			// 
			// docStatsChart
			// 
			chartArea2.Name = "ChartArea1";
			this.docStatsChart.ChartAreas.Add(chartArea2);
			this.docStatsChart.Dock = System.Windows.Forms.DockStyle.Fill;
			legend2.Name = "Legend1";
			this.docStatsChart.Legends.Add(legend2);
			this.docStatsChart.Location = new System.Drawing.Point(0, 0);
			this.docStatsChart.Name = "docStatsChart";
			this.docStatsChart.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			series2.ChartArea = "ChartArea1";
			series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Doughnut;
			series2.CustomProperties = "PieLabelStyle=Disabled";
			series2.Legend = "Legend1";
			series2.Name = "PageTypes";
			this.docStatsChart.Series.Add(series2);
			this.docStatsChart.Size = new System.Drawing.Size(678, 547);
			this.docStatsChart.TabIndex = 0;
			this.docStatsChart.Text = "Page Statistics";
			// 
			// loadButton
			// 
			this.loadButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.loadButton.Location = new System.Drawing.Point(0, 0);
			this.loadButton.Name = "loadButton";
			this.loadButton.Size = new System.Drawing.Size(678, 32);
			this.loadButton.TabIndex = 1;
			this.loadButton.Text = "Show page type usage on Corporate Site";
			this.loadButton.UseVisualStyleBackColor = true;
			this.loadButton.Click += new System.EventHandler(this.loadButton_Click);
			// 
			// PageTypeStatisticsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(678, 547);
			this.Controls.Add(this.loadButton);
			this.Controls.Add(this.docStatsChart);
			this.Name = "PageTypeStatisticsForm";
			this.Text = "Page Type Statistics";
			((System.ComponentModel.ISupportInitialize)(this.docStatsChart)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataVisualization.Charting.Chart docStatsChart;
		private System.Windows.Forms.Button loadButton;
	}
}

