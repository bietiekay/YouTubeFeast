using System;
using System.Configuration;
using System.IO;
using System.Collections.Generic;
using YoutubeExtractor;

namespace YouTubeFeast
{
    public class ChannelDownloadJob
    {
        public String ChannelURL;
		public String ChannelDownloadDirectory;
		public VideoFormat DownloadVideoFormat;
    }

    public static class YouTubeFeastConfiguration
    {
        public static List<ChannelDownloadJob> DownloadJobs = new List<ChannelDownloadJob>();

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
						ChannelDownloadJob NewJob = new ChannelDownloadJob();
						
						if (TokenizedLine.Length == 3)
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