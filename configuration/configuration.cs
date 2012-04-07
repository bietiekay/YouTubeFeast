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
		public Int64 Interval;
		public DateTime LastDownload;
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
						
						if (TokenizedLine.Length == 4)
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
							NewJob.Interval = Convert.ToInt32(TokenizedLine[3])*60*60*1000;
							
							DownloadJobs.Add(NewJob);
							Console.WriteLine("Added a new Job ("+NewJob.ChannelURL+")");
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