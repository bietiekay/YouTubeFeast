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
		public Int32 DownloadVideoFormat;
		public Int32 Interval;
		public DateTime LastDownload;
		public bool SearchBottom;
        public String SearchKeyword;
		public HashingAndCaching HashCache;
        public bool Continue;
        public Int32 MaximumChecks;
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
						
						if (TokenizedLine.Length >= 6)
						{
							NewJob.ChannelURL = TokenizedLine[0];
							NewJob.ChannelDownloadDirectory = TokenizedLine[2];
							switch (TokenizedLine[1].ToUpper())
							{
							    case "360P": 
							        NewJob.DownloadVideoFormat = 360;
							        break;
							    case "720P":
							        NewJob.DownloadVideoFormat = 720;
							        break;
								case "1080P":
									NewJob.DownloadVideoFormat = 1080;
									break;
							    default:
							        NewJob.DownloadVideoFormat = 360;
							        break;
							}
                            switch (TokenizedLine[4].ToUpper())
                            {
                                case "CONTINUE":
                                    NewJob.Continue = true;
                                    break;
                                case "BREAK":
                                    NewJob.Continue = false;
                                    break;
                                default:
                                    NewJob.Continue = false;
                                    break;
                            }
                            NewJob.MaximumChecks = Convert.ToInt32(TokenizedLine[5])/**60*60*1000*/;

							switch (TokenizedLine[6].ToUpper())
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

                            if (TokenizedLine.Length == 8)
                            {
                                NewJob.SearchKeyword = TokenizedLine[7];
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