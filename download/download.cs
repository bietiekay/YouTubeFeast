using System;
using System.Net;
using System.Collections.Generic;
using System.Text;

namespace YouTubeFeast
{
	public static class YoutubeDownload
	{
		#region Generate Download URLs from a Channel URL
		public static List<String> GenerateDownloadURLsFromChannel(String ChannelURL)
		{
			WebClient myWebClient = new WebClient();
			//Console.WriteLine("Downloading: " + ChannelURL);                        
			// Download the Web resource and save it into a data buffer.
			byte[] myDataBuffer;
			try
			{
				myDataBuffer = myWebClient.DownloadData (ChannelURL);
			}
			catch ( Exception e)
			{
				Console.WriteLine("\t\tError: "+ShortenString.LimitCharacters(e.Message,40));
				return new List<String>();
			}
			
			List<String> Output = new List<String>();
			String PrependURL = "http://www.youtube.com"; // this is what we prepend to each resulting string
			// convert to a string so we can easier work through it...
			string download = Encoding.ASCII.GetString(myDataBuffer);
			// now we need to find any "/watch?" passages in there - because that translates to actual movie urls
 			
			bool done = false;
			
			while(!done)
			{
				Int32 occurrence = download.IndexOf("/watch?");
				
				if (occurrence == -1)
					done = true;
				else
				{
					// we found something... now lets read from there until we find a "					
					download = download.Remove(0,occurrence);	// remove the garbage until the found position
					occurrence = download.IndexOf("\""); // find the ending of this ...
					
					String Postpend = download.Remove(occurrence);
					download = download.Remove(0,Postpend.Length); // remove the found one from the lot
					
                    bool takeit = true;
                    String NewURL = PrependURL+Postpend;
                    foreach(String _URL in Output)
                    {
                        String Start1 = _URL.Substring(0, 42);
                        String Start2 = NewURL.Substring(0, 42);

                        if (Start1 == Start2)
                        {
                            takeit = false;
                            break;
                        }
                    }


                    if (takeit)
                    {
					    Output.Add(NewURL);
                        //Console.WriteLine(NewURL);
                    }
				}
			}
			//Console.WriteLine("I have found a total of "+Output.Count+" movies in that channel");
			return Output;
		}
		#endregion
	}
}

