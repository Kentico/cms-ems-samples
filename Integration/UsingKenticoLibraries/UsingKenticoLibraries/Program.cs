using System;
using System.Linq;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.Messaging;

namespace UsingKenticoLibraries
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				// Initialize Kentico API
				CMSApplication.Init();
			}
			catch (Exception ex)
			{
				// Print the exception
				Console.WriteLine(ex.Message);
			}

			try
			{
				// Get users containing letter 'a' via object query
				Console.WriteLine("Users containing 'a':");
				foreach (var user in UserInfoProvider.GetUsers().Where("UserName", QueryOperator.Like, "%a%"))
				{
					Console.WriteLine("\t" + user.FullName);
				}

				// Resolve some macros
				Console.WriteLine("Macros:");
				MacroResolver resolver = MacroResolver.GetInstance();
				Console.WriteLine(resolver.ResolveMacros("\tCurrent user is {%CurrentUser.UserName%}"));
				Console.WriteLine(resolver.ResolveMacros("\t5*7 is {%5*7%}"));

				// Build partial site trees
				Console.WriteLine("Site structure:");
				foreach (var doc in DocumentHelper.GetDocuments().Columns("DocumentName, NodeLevel")
					.Where("NodeLevel", QueryOperator.LessOrEquals, 3).OrderBy("NodeSiteID, NodeAliasPath"))
				{
					Console.WriteLine(String.Concat(Enumerable.Repeat("\t", doc.NodeLevel)) + doc.DocumentName);
				}
			}
			catch (Exception ex)
			{
				// Log exception to the Kentico event log
				EventLogProvider.LogException("NuGetConsoleExample", "MAIN", ex);
			}
			Console.WriteLine("Press ENTER...");
			Console.ReadLine();
		}
	}
}
