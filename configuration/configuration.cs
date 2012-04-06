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
                   /* 
                    String[] TokenizedLine = LineElement.Split(new char[1] { ' ' });
                    LineNumber++;

                    if (!LineElement.StartsWith("#"))
                    { 

                        ScriptingActorElement NewElement = new ScriptingActorElement();

                        if (TokenizedLine.Length == 4)
                        { 
                            NewElement.SensorToWatchName = TokenizedLine[0];
                            NewElement.SensorValue = Convert.ToDouble(TokenizedLine[1]);
                            NewElement.ActorToSwitchName = TokenizedLine[2];
                            if (TokenizedLine[3].ToUpper() == "ON")
                                NewElement.ActionToRunName = actor_status.On;
                            else
                                if (TokenizedLine[3].ToUpper() == "OFF")
                                    NewElement.ActionToRunName = actor_status.Off;
                                else
                                    if (TokenizedLine[3].ToUpper() == "ONOFF")
                                        NewElement.ActionToRunName = actor_status.OnOff;

                            ScriptingActorActions.Add(NewElement);
                        }
                        else
                            throw (new Exception("Scripting Actor Configuration File - Error in line "+LineNumber));
                    }
                    */
                }
            }
            else
            {
                throw (new Exception("Error: configuration file "+Configfilename+" not found!"));
            }
        }
    }
}