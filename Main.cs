using System;
using System.Threading;
using System.Collections.Generic;

namespace YouTubeFeast
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine("YouTubeFeast version "+System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
			Console.WriteLine("(C) Daniel Kirstenpfad 2012 - http://www.technology-ninja.com");
			Console.WriteLine();
			
			YouTubeFeastConfiguration.ReadConfiguration("YouTubeFeast.config");
			
			Console.WriteLine();
			Console.WriteLine("to quit please use control-c.");
			Console.WriteLine();
			
			while (true)
			{
				// we have to decide if there is a job we need to work on
				
				foreach(ChannelJob job in YouTubeFeastConfiguration.DownloadJobs)
				{
					List<String> DownloadURLs = YoutubeDownload.GenerateDownloadURLsFromChannel(job.ChannelURL);
				}
				
				Thread.Sleep(10000);
			}
		}
	}
}
