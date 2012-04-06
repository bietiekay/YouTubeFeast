using System;
using System.Threading;

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
				Thread.Sleep(1000);
			}
		}
	}
}
