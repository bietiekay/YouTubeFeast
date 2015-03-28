using System;
using VimeoDownloadLibrary;
using System.Net;
using System.Collections.Generic;

namespace vimeodownloadersharp
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Vimeo Downloader Sharp");
			Console.WriteLine ("(C) 2015 Daniel Kirstenpfad");
			Console.WriteLine ("Based on a Bash Script by John Slade");
			Console.WriteLine ("\tCopyright (C) 2008, 2010  Denver Gingerich");
			Console.WriteLine ("\tCopyright (C) 2009  Jori Hamalainen");
			Console.WriteLine ("\tCopyright (C) 2012  John Slade (http://jtes.net)");
			Console.WriteLine ();



			if (args.Length <= 0) {

				Console.WriteLine ("Usage: ");
				Console.WriteLine ();
				Console.WriteLine ("vimeo-downloader <Vimeo Video ID or Vimeo Channel RSS URL>");
				Console.WriteLine ();
				Console.WriteLine ("You find the VideoID in the URL of the Vimeo URL. it's the number - like https://vimeo.com/123456789 - it is the 123456789.");
			} else {

				if (args [0].StartsWith ("http")) {
					// this is supposed to be a vimeo video channel url
					List<VimeoVideoChannel> Videos = VimeoDownloadLibrary.VimeoDownloadLibrary.ParseVimeoChannel(args[0]);

					foreach (VimeoVideoChannel video in Videos) {
						Console.WriteLine (video.VideoName + "  -  " + video.VideoID);
					}

				} else {
					// this is supposed to be a vimeo video id
					VideoDownloadURL DownloadURL = VimeoDownloadLibrary.VimeoDownloadLibrary.GetVimeoVideoDownloadURL (args [0]);

					Console.WriteLine ("Downloading: " + DownloadURL.VideoName);

					using (WebClient Client = new WebClient ()) {
						Client.Headers.Add ("user-agent", "Mozilla/5.0");
						Client.DownloadFile (DownloadURL.VideoURL, DownloadURL.VideoName + ".mp4");
					}
				}
			}

		}
	}
}
