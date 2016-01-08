using System;
using System.Linq;
using System.Windows.Forms;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventLog;

namespace UsingKenticoLibrariesInForms
{
	public partial class PageTypeStatisticsForm : Form
	{
		public PageTypeStatisticsForm()
		{
			InitializeComponent();
		}

		private void loadButton_Click(object sender, EventArgs e)
		{
			try
			{
				// Initialize Kentico API
				CMSApplication.Init();
			}
			catch (Exception ex)
			{
				// Show the exception
				MessageBox.Show(ex.Message);
			}

			try
			{
				// Load all docs from Corporate Site and group them by class name
				var docs = DocumentHelper.GetDocuments().OnSite(new SiteInfoIdentifier("CorporateSite")).Columns("ClassName")
					.TypedResult.Items.GroupBy(d => d.ClassName).Select(group => new { Group = group.Key, Count = group.Count() });

				// Add the data to the chart
				foreach (var doc in docs)
				{
					docStatsChart.Series["PageTypes"].Points.AddXY(doc.Group, doc.Count);
				}

			}
			catch (Exception ex)
			{
				// Log exception to the Kentico event log
				EventLogProvider.LogException("NuGetWinFormsExample", "LOADDOC", ex);
			}
		}
	}
}
