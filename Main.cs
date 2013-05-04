using System;
using System.Threading;
using System.Collections.Generic;
using YoutubeExtractor;
using System.IO;
using System.Linq;

namespace YouTubeFeast
{
	class MainClass
	{
		public static void Main (string[] args2)
		{
			ConsoleOutputLogger.verbose = true;
			ConsoleOutputLogger.writeLogfile = false;


			ConsoleOutputLogger.WriteLine("YouTubeFeast version "+System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
			ConsoleOutputLogger.WriteLine("YouTubeExtractor library created by flagbug (https://github.com/flagbug/YoutubeExtractor/)");
			ConsoleOutputLogger.WriteLine("(C) Daniel Kirstenpfad 2012 - http://www.technology-ninja.com");
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
						Console.WriteLine("Updating: "+job.ChannelDownloadDirectory);
						// we should download something... or at least look for new stuff
                        List<String> DownloadURLs = new List<string>();
                        try
                        {
						    DownloadURLs = YoutubeDownload.GenerateDownloadURLsFromChannel(job.ChannelURL);
						    job.LastDownload = DateTime.Now;
                        }
                        catch(Exception e)
                        {
                            ConsoleOutputLogger.WriteLine("Error in Channel " + job.ChannelURL);
                            ConsoleOutputLogger.WriteLine(e.Message);
                        }
						
						// it seems that we got a nice list here, now let's
						if (DownloadURLs.Count > 0)
						{
							// start the downloadings...
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
                                        Console.WriteLine("Maximum Downloads ("+job.MaximumChecks+") reached for this run for " + job.ChannelDownloadDirectory);
                                        break;
                                    }
                                }
								VideoInfo video = null;
                                IEnumerable<VideoInfo> videoInfos = null;
                                try
                                {
                                    // get all the available video formats for this one...
                                    videoInfos = DownloadUrlResolver.GetDownloadUrls(url,Properties.Settings.Default.Proxy);
								    video = videoInfos.First(info => ((info.Resolution == job.DownloadVideoFormat) && (info.VideoType == VideoType.Mp4)));
                                }
                                catch(Exception e)
                                {
									//Console.WriteLine(e.Message);
									//Console.WriteLine(e.StackTrace);
                                    //videoInfos = DownloadUrlResolver.GetDownloadUrls(url);
                                    //video = videoInfos.First(info => info.VideoFormat == VideoFormat.Standard360);
                                    //Console.WriteLine("Error: Video with the desired resolution is not available ("+job.ChannelDownloadDirectory+")");
                                    //video = videoInfos.First(info => info.VideoFormat == VideoFormat.Standard360);
                                    continue;
                                }
								
								if (video != null)
								{
                                    String tmp_filename = Path.Combine(job.ChannelDownloadDirectory,"youtubefeast.tmp");
									String filename = Path.Combine(job.ChannelDownloadDirectory, video.Title + video.VideoExtension);
                                    
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

										Console.WriteLine("File: "+filename+" already exists.");
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

                                        ConsoleOutputLogger.WriteLine("Downloading: " + ShortenString.LimitCharacters(video.Title, 40) + " to "+job.ChannelDownloadDirectory);

                                        var videoDownloader = new VideoDownloader(video, tmp_filename);

                                        if (Properties.Settings.Default.UseProxy)
										    videoDownloader = new VideoDownloader(video, tmp_filename,Properties.Settings.Default.Proxy);

                                        Int32 left = Console.CursorLeft;
                                        Int32 top = Console.CursorTop;

										videoDownloader.ProgressChanged += (sender, args) => DisplayProgress(left,top,args.ProgressPercentage);

										try
                                        {
										    videoDownloader.Execute();
                                            // if successfull, rename...
                                            FileInfo f2 = new FileInfo(tmp_filename);
                                            long s2 = f2.Length;
                                            if (s2 == 0)
                                            {
                                                File.Delete(filename);
                                                Console.WriteLine("zeroed...");
                                            }
                                            else
                                            {
                                                //Console.WriteLine("Moving: " + tmp_filename + " --> " + filename);
                                                File.Move(tmp_filename, filename);
                                            }

                                        }
                                        catch(Exception e)
                                        {
                                            ConsoleOutputLogger.WriteLine("Error: "+ShortenString.LimitCharacters(e.Message,40));
                                            //video = videoInfos.First(info => info.VideoFormat == VideoFormat.Standard360);
                                        }

										// now checking if we already downloaded a file with the same content but different name earlier
										if (File.Exists(filename))
										{
											String justdownloadedMD5 = job.HashCache.CalculateMD5Sum(filename);

											if (job.HashCache.HashCodeExists(justdownloadedMD5))
											{
												// we've seen this file earlier...
												String OldName = job.HashCache.ReplaceFilenameInCache(justdownloadedMD5,video.Title+video.VideoExtension);

												String OldFilePath = Path.Combine(job.ChannelDownloadDirectory, OldName);

												if (File.Exists(OldFilePath))
												{
													try
													{
														ConsoleOutputLogger.WriteLine("Found a duplicate: "+OldName+" -> "+video.Title+video.VideoExtension);
														File.Delete(OldFilePath);
													}
													catch(Exception)
													{

													}
												}
											}
											else
											{
												// we've never seen this file
												job.HashCache.AddToCache(justdownloadedMD5,video.Title + video.VideoExtension);
											}
										}
                                        Console.WriteLine("done    ");
									}
								}
							}
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
		}
	}
}
