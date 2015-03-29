using System;
using System.Threading;
using System.Collections.Generic;
using YoutubeExtractor;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VimeoDownloadLibrary;
using System.Net;

namespace YouTubeFeast
{
	class MainClass
	{
        static double dl_percentage;

        private static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, @"[^a-zA-Z0-9_\-äöüÄÜÖ#.!€$`'""& ]+", "_", RegexOptions.Compiled);
            //return str.Replace(':', '.').Replace('|', '_').Replace('/', '_').Replace('\\', '_').Replace('>', '_').Replace('<', '_');
        }

		public static void Main (string[] args2)
		{
			ConsoleOutputLogger.verbose = true;
			ConsoleOutputLogger.writeLogfile = false;


			ConsoleOutputLogger.WriteLine("YouTubeFeast version "+System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
			ConsoleOutputLogger.WriteLine("YouTubeExtractor library created by flagbug (https://github.com/flagbug/YoutubeExtractor/)");
			ConsoleOutputLogger.WriteLine("(C) Daniel Kirstenpfad 2012-2015 - http://www.technology-ninja.com");
			ConsoleOutputLogger.WriteLine("");
			
			YouTubeFeastConfiguration.ReadConfiguration("YouTubeFeast.configuration");
			
			ConsoleOutputLogger.WriteLine("");
			ConsoleOutputLogger.WriteLine("to quit please use control-c.");
			ConsoleOutputLogger.WriteLine("");
			ConsoleOutputLogger.writeLogfile = true;

			while (true)
			{
				// we have to decide if there is a job we need to work on
				
				foreach(ChannelJob job in YouTubeFeastConfiguration.DownloadJobs)
				{
                    Int32 Counter = 0;

					TimeSpan SinceLastRun = DateTime.Now - job.LastDownload;
					TimeSpan theInterval = new TimeSpan(job.Interval,0,0);
					if ( SinceLastRun  >= theInterval )
					{
                        if (job.VideoService == VideoServices.YouTube)
                        {
                            #region YouTube
                            ConsoleOutputLogger.WriteLine("Updating: " + job.ChannelDownloadDirectory);
                            // we should download something... or at least look for new stuff
                            List<String> DownloadURLs = new List<string>();
                            try
                            {
                                DownloadURLs = YoutubeDownload.GenerateDownloadURLsFromChannel(job.ChannelURL);
                                job.LastDownload = DateTime.Now;
                            }
                            catch (Exception e)
                            {
                                ConsoleOutputLogger.WriteLine("Error in Channel " + job.ChannelURL);
                                ConsoleOutputLogger.WriteLine(e.Message);
                            }

                            // it seems that we got a nice list here, now let's
                            if (DownloadURLs.Count > 0)
                            {
                                // oh there is a policy: the first file that already exists leads to the abortion of this particular channel download
                                // that's because this tool expects the new files to appear first on the channel page and the old ones to be listed later
                                // on the page

                                if (job.SearchBottom)
                                {
                                    //Console.WriteLine("reversed: " + job.ChannelURL);
                                    DownloadURLs.Reverse();
                                }

                                foreach (String url in DownloadURLs)
                                {
                                    if (job.Continue)
                                    {
                                        if (Counter >= job.MaximumChecks)
                                        {
                                            ConsoleOutputLogger.WriteLine("Maximum Downloads (" + job.MaximumChecks + ") reached for this run for " + job.ChannelDownloadDirectory);
                                            break;
                                        }
                                    }
                                    VideoInfo video = null;
                                    IEnumerable<VideoInfo> videoInfos = null;
                                    try
                                    {
                                        // get all the available video formats for this one...
                                        videoInfos = DownloadUrlResolver.GetDownloadUrls(url, true);
                                        video = videoInfos.First(info => ((info.Resolution == job.DownloadVideoFormat) && (info.VideoType == VideoType.Mp4)));
                                    }
                                    catch (Exception e)
                                    {
                                        //Console.WriteLine(e.Message);
                                        //Console.WriteLine(e.StackTrace);
                                        //videoInfos = DownloadUrlResolver.GetDownloadUrls(url);
                                        //video = videoInfos.First(info => info.VideoFormat == VideoFormat.Standard360);
                                        //Console.WriteLine("Error: Video with the desired resolution is not available ("+job.ChannelDownloadDirectory+")");
                                        //video = videoInfos.First(info => info.VideoFormat == VideoFormat.Standard360);
                                        continue;
                                    }

                                    #region Download it
                                    if (video != null)
                                    {
                                        String tmp_filename = Path.Combine(job.ChannelDownloadDirectory, "youtubefeast.tmp");
                                        String filename = Path.Combine(job.ChannelDownloadDirectory, RemoveSpecialCharacters(video.Title) + video.VideoExtension);

                                        // check if there is a keyword present we should look for when choosing to-be-downloaded files
                                        if (job.SearchKeyword != "")
                                        {
                                            // if we do not find it in the name, skip
                                            //Console.WriteLine("checking: " + video.Title);
                                            if (!video.Title.Contains(job.SearchKeyword))
                                                continue;
                                        }

                                        if (File.Exists(filename))
                                        {
                                            Counter++;

                                            ConsoleOutputLogger.WriteLine("File: " + filename + " already exists.");
                                            //Console.WriteLine("\t\tNotice: We are finished with this channel.");
                                            if (job.Continue)
                                                continue;
                                            else
                                                break;
                                            //break;
                                            //continue;
                                        }
                                        else
                                        {
                                            Counter++;

                                            ConsoleOutputLogger.WriteLine("Downloading: " + ShortenString.LimitCharacters(video.Title, 40) + " to " + job.ChannelDownloadDirectory);
                                            #region Downloading
                                            #region Proxy
                                            if (Properties.Settings.Default.UseProxy)
                                            {
                                                var videoDownloaderProxy = new VideoDownloaderProxy(video, tmp_filename, Properties.Settings.Default.Proxy);
                                                Int32 left = Console.CursorLeft;
                                                Int32 top = Console.CursorTop;
                                                videoDownloaderProxy.DownloadProgressChanged += (sender, args) => DisplayProgress(left, top, args.ProgressPercentage);

                                                try
                                                {
                                                    dl_percentage = 0;
                                                    videoDownloaderProxy.Execute();

                                                    if (dl_percentage <= 99)
                                                    {
                                                        // file download did not complete...
                                                        ConsoleOutputLogger.WriteLine("File download not complete... aborting");
                                                    }
                                                    else
                                                    {
                                                        // if successfull, rename...
                                                        FileInfo f2 = new FileInfo(tmp_filename);
                                                        long s2 = f2.Length;
                                                        if (s2 == 0)
                                                        {
                                                            File.Delete(filename);
                                                            ConsoleOutputLogger.WriteLine("zeroed...");
                                                        }
                                                        else
                                                        {
                                                            //Console.WriteLine("Moving: " + tmp_filename + " --> " + filename);
                                                            File.Move(tmp_filename, filename);
                                                        }
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    ConsoleOutputLogger.WriteLine("Error: " + ShortenString.LimitCharacters(e.Message, 40));
                                                    //video = videoInfos.First(info => info.VideoFormat == VideoFormat.Standard360);
                                                }

                                            #endregion
                                            }
                                            else
                                            {
                                            #region NoProxy
                                                var videoDownloader = new VideoDownloader(video, tmp_filename);
                                                Int32 left = Console.CursorLeft;
                                                Int32 top = Console.CursorTop;

                                                videoDownloader.DownloadProgressChanged += (sender, args) => DisplayProgress(left, top, args.ProgressPercentage);

                                                try
                                                {
                                                    dl_percentage = 0;
                                                    videoDownloader.Execute();

                                                    if (dl_percentage <= 99)
                                                    {
                                                        // file download did not complete...
                                                        ConsoleOutputLogger.WriteLine("File download not complete... aborting");
                                                    }
                                                    else
                                                    {
                                                        // if successfull, rename...
                                                        FileInfo f2 = new FileInfo(tmp_filename);
                                                        long s2 = f2.Length;
                                                        if (s2 == 0)
                                                        {
                                                            File.Delete(filename);
                                                            ConsoleOutputLogger.WriteLine("zeroed...");
                                                        }
                                                        else
                                                        {
                                                            //Console.WriteLine("Moving: " + tmp_filename + " --> " + filename);
                                                            File.Move(tmp_filename, filename);
                                                        }
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    ConsoleOutputLogger.WriteLine("Error: " + ShortenString.LimitCharacters(e.Message, 40));
                                                    //video = videoInfos.First(info => info.VideoFormat == VideoFormat.Standard360);
                                                }
                                            #endregion
                                            }
                                            #endregion 

                                            #region now checking if we already downloaded a file with the same content but different name earlier
                                            if (File.Exists(filename))
                                            {
                                                String justdownloadedMD5 = job.HashCache.CalculateMD5Sum(filename);

                                                if (job.HashCache.HashCodeExists(justdownloadedMD5))
                                                {
                                                    // we've seen this file earlier...
                                                    String OldName = job.HashCache.ReplaceFilenameInCache(justdownloadedMD5, RemoveSpecialCharacters(video.Title) + video.VideoExtension);

                                                    String OldFilePath = Path.Combine(job.ChannelDownloadDirectory, OldName);

                                                    if (File.Exists(OldFilePath))
                                                    {
                                                        try
                                                        {

                                                            if (OldName != video.Title + video.VideoExtension)
                                                            {
                                                                ConsoleOutputLogger.WriteLine("Found a duplicate: " + OldName + " -> " + RemoveSpecialCharacters(video.Title) + video.VideoExtension);
                                                                File.Delete(OldFilePath);
                                                            }
                                                        }
                                                        catch (Exception)
                                                        {

                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    // we've never seen this file
                                                    job.HashCache.AddToCache(justdownloadedMD5, RemoveSpecialCharacters(video.Title) + video.VideoExtension);
                                                }
                                            }
                                            #endregion
                                            Console.WriteLine("done    ");
                                        }
                                    }
                                    #endregion
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region Vimeo
                            // first get the list of downloadable Videos...
                            try
                            {
                                #region Get Channel Information
                                List<VimeoVideoChannel> Videos = VimeoDownloadLibrary.VimeoDownloadLibrary.ParseVimeoChannel(job.ChannelURL);
                                job.LastDownload = DateTime.Now;
                                #endregion

                                // oh there is a policy: the first file that already exists leads to the abortion of this particular channel download
                                // that's because this tool expects the new files to appear first on the channel page and the old ones to be listed later
                                // on the page

                                #region Policies
                                if (job.SearchBottom)
                                {
                                    //Console.WriteLine("reversed: " + job.ChannelURL);
                                    Videos.Reverse();
                                }
                                #endregion

                                // since we got it - lets 
                                foreach (VimeoVideoChannel video in Videos)
                                {
                                    #region Policies
                                    // since this is vimeo we do not need to check, we can skip if file exists...
                                   // if (job.Continue)
                                   // {
                                   //     if (Counter >= job.MaximumChecks)
                                   //     {
                                   //         ConsoleOutputLogger.WriteLine("Maximum Downloads (" + job.MaximumChecks + ") reached for this run for " + job.ChannelDownloadDirectory);
                                   //         break;
                                   //     }
                                   // }
                                    // check if there is a keyword present we should look for when choosing to-be-downloaded files
                                    if (job.SearchKeyword != "")
                                    {
                                        // if we do not find it in the name, skip
                                        //Console.WriteLine("checking: " + video.Title);
                                        if (!video.VideoName.Contains(job.SearchKeyword))
                                            continue;
                                    }

                                    #endregion
                                    try
                                    {
                                        // get all the available video formats for this one...
                                        VideoDownloadURL DownloadURL = VimeoDownloadLibrary.VimeoDownloadLibrary.GetVimeoVideoDownloadURL(video.VideoID);
                                        
                                        String tmp_filename = Path.Combine(job.ChannelDownloadDirectory, "vimeofeast.tmp");
                                        String filename = Path.Combine(job.ChannelDownloadDirectory, RemoveSpecialCharacters(video.VideoName) + ".mp4");
                                        if (File.Exists(filename))
                                        {
                                            Counter++;

                                            ConsoleOutputLogger.WriteLine("File: " + filename + " already exists.");
                                            //Console.WriteLine("\t\tNotice: We are finished with this channel.");
                                            if (job.Continue)
                                                continue;
                                            else
                                                break;
                                            //break;
                                            //continue;
                                        }
                                        else
                                        {
                                            Counter++;

                                            ConsoleOutputLogger.WriteLine("Downloading: " + ShortenString.LimitCharacters(video.VideoName, 40) + " to " + job.ChannelDownloadDirectory);

                                            try
                                            {                                                
                                                using (WebClient Client = new WebClient ()) 
                                                {
						                            Client.Headers.Add ("user-agent", "Mozilla/5.0");
                                                    
                                                    if (Properties.Settings.Default.UseProxy)
                                                        Client.Proxy = new WebProxy(Properties.Settings.Default.Proxy);

                                                    Client.DownloadFile(DownloadURL.VideoURL, tmp_filename);
					                            }

                                                // download successful...
                                                if (File.Exists(tmp_filename))
                                                    File.Move(tmp_filename, filename);

                                                #region now checking if we already downloaded a file with the same content but different name earlier
                                                if (File.Exists(filename))
                                                {
                                                    
                                                    String justdownloadedMD5 = job.HashCache.CalculateMD5Sum(filename);

                                                    if (job.HashCache.HashCodeExists(justdownloadedMD5))
                                                    {
                                                        // we've seen this file earlier...
                                                        String OldName = job.HashCache.ReplaceFilenameInCache(justdownloadedMD5, RemoveSpecialCharacters(video.VideoName) + ".mp4");

                                                        String OldFilePath = Path.Combine(job.ChannelDownloadDirectory, OldName);

                                                        if (File.Exists(OldFilePath))
                                                        {
                                                            try
                                                            {

                                                                if (OldName != RemoveSpecialCharacters(video.VideoName) + ".mp4")
                                                                {
                                                                    ConsoleOutputLogger.WriteLine("Found a duplicate: " + OldName + " -> " + RemoveSpecialCharacters(video.VideoName) + ".mp4");
                                                                    File.Delete(OldFilePath);
                                                                }
                                                            }
                                                            catch (Exception)
                                                            {

                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // we've never seen this file
                                                        job.HashCache.AddToCache(justdownloadedMD5, RemoveSpecialCharacters(video.VideoName) + ".mp4");
                                                    }
                                                }
                                                #endregion


                                            }
                                            catch (Exception e)
                                            {
                                                // download failed...
                                                ConsoleOutputLogger.WriteLine("File download not complete... aborting");
                                            }

                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        continue;
                                    }
                                }
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine("Error in Channel: " + job.ChannelURL);
                                Console.WriteLine("Vimeo Download Error: " + e.Message);
                            }
                            #endregion
                        }
					}                    
				}               
				Thread.Sleep(60000);
			}
		}
		
		public static void DisplayProgress(Int32 left, Int32 top, double percentage)
		{
            Console.SetCursorPosition(left, top);
			Console.Write (Convert.ToInt32(percentage)+"%");
            dl_percentage = percentage;
		}
	}
}
