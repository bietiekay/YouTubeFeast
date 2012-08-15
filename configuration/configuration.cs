using System;
using System.Configuration;
using System.IO;
using System.Collections.Generic;
using YoutubeExtractor;

namespace YouTubeFeast
{
    public class ChannelJob
    {
        public String ChannelURL;
		public String ChannelDownloadDirectory;
		public VideoFormat DownloadVideoFormat;
		public Int32 Interval;
		public DateTime LastDownload;
		public bool SearchBottom;
        public String SearchKeyword;
		public HashingAndCaching HashCache;
    }

    public static class YouTubeFeastConfiguration
    {
        public static List<ChannelJob> DownloadJobs = new List<ChannelJob>();

        public static void ReadConfiguration(String Configfilename)
        {
            if (File.Exists(Configfilename))
            {
                // get all lines from the config file 
                String[] ConfigFileContent = File.ReadAllLines(Configfilename);
                Int32 LineNumber = 0;

                foreach(String LineElement in ConfigFileContent)
                {
                    LineNumber++;
					
					// ignore anything that starts with # and read everything else
                    if (!LineElement.StartsWith("#"))
                    {
						String[] TokenizedLine = LineElement.Split(new char[1] { '\t' });
						ChannelJob NewJob = new ChannelJob();
						
						if (TokenizedLine.Length >= 5)
						{
							NewJob.ChannelURL = TokenizedLine[0];
							NewJob.ChannelDownloadDirectory = TokenizedLine[2];
							switch (TokenizedLine[1].ToUpper())
							{
							    case "360P": 
							        NewJob.DownloadVideoFormat = VideoFormat.Standard360;
							        break;
							    case "720P":
							        NewJob.DownloadVideoFormat = VideoFormat.HighDefinition720;
							        break;
								case "1080P":
									NewJob.DownloadVideoFormat = VideoFormat.HighDefinition1080;
									break;
							    default:
							        NewJob.DownloadVideoFormat = VideoFormat.Standard360;
							        break;
							}
							switch (TokenizedLine[4].ToUpper())
							{
								case "ADDBOTTOM":
									NewJob.SearchBottom = true;
									break;
                                case "ADDTOP":
                                    NewJob.SearchBottom = false;
                                    break;
								default:
									NewJob.SearchBottom = false;
									break;
							}
							NewJob.Interval = Convert.ToInt32(TokenizedLine[3])/**60*60*1000*/;

                            if (TokenizedLine.Length == 6)
                            {
                                NewJob.SearchKeyword = TokenizedLine[5];
                            }
                            else
                                NewJob.SearchKeyword = "";

							NewJob.HashCache = new HashingAndCaching(NewJob.ChannelDownloadDirectory);

							DownloadJobs.Add(NewJob);
							ConsoleOutputLogger.WriteLine("Added a new Job ("+NewJob.ChannelURL+")");
						}
						else
							throw (new Exception("configuration file - error in line "+LineNumber));
                    }
                }
            }
            else
            {
                throw (new Exception("Error: configuration file "+Configfilename+" not found!"));
            }
        }
    }
}