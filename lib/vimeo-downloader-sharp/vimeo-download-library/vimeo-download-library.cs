using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.ServiceModel.Syndication;
using System.Xml;
using System.IO;

namespace VimeoDownloadLibrary
{
	public class VimeoVideoChannel
	{
		public String VideoID;
		public String VideoName;

		public VimeoVideoChannel(String ID, String Name)
		{
			VideoID = ID;
			VideoName = Name;
		}
	}

	public class VideoDownloadURL
	{
		public String VideoURL;
		public String VideoName;

		public VideoDownloadURL(String URL, String Name)
		{
			VideoURL = URL;
			VideoName = Name;
		}
	}

	public static class VimeoDownloadLibrary
	{
		/// <summary>
		/// Parses the vimeo channel RSS URL and returns a list of DownloadURLs on that page
		/// </summary>
		/// <returns>The vimeo videos download information contained in this channel.</returns>
		/// <param name="ChannelURL">Channel UR.</param>
		public static List<VimeoVideoChannel> ParseVimeoChannel(String ChannelURL)
		{
			// create the empty output list...
			List<VimeoVideoChannel> Output = new List<VimeoVideoChannel> ();

			XmlReader reader = XmlReader.Create(ChannelURL);
			SyndicationFeed feed = SyndicationFeed.Load(reader);
			reader.Close();
			foreach (SyndicationItem item in feed.Items)
			{
				if (item.Links.Count >= 0) {
					Output.Add(new VimeoVideoChannel(item.Links[0].Uri.AbsolutePath.Remove(0,1),item.Title.Text));
				}
			}
			return Output;
		}

		/// <summary>
		/// Downloads a Video from Vimeo. Always defaults to the first HD one.
		/// </summary>
		/// <param name="VideoID">the Vimeo Video ID</param>
		/// <param name="DownloadPath">where to store the downloaded video</param>
		/// <returns>>DownloadURL</returns>
		public static VideoDownloadURL GetVimeoVideoDownloadURL(String VideoID)
		{
			// First we build the Vimeo URL...
			String VimeoURL = "http://vimeo.com/" + VideoID;

			// now we retrieve that page...

			var VimeoVideoPage = string.Empty;
			using (var webClient = new System.Net.WebClient())
			{
				webClient.Headers.Add ("user-agent", "Mozilla/5.0");
				VimeoVideoPage = webClient.DownloadString(VimeoURL);
			}

			if (VimeoVideoPage == string.Empty) {
				return null;
			} else {
				// we have the page and now we need to find the video configuration in there...

				// Convert the VideoPage into it's lines
				string[] VimeoVideoPageLines = VimeoVideoPage.Split ('\n');

				var data_config_url_line = string.Empty;

				// Grep through the Lines and find the one with "data-config-url"
				foreach (String _line in VimeoVideoPageLines) {

					if (_line.Contains ("data-config-url")) {
						data_config_url_line = _line;
						break;
					}
				}

				// first find the data-config-url
				String ConfigURL = data_config_url_line.Remove(0,data_config_url_line.IndexOf("data-config-url=")+17);
				ConfigURL = WebUtility.HtmlDecode (ConfigURL.Remove (ConfigURL.IndexOf ("\""), ConfigURL.Length - ConfigURL.IndexOf ("\"")));

				// Retrieve the configuration now
				var VimeoVideoConfiguration = string.Empty;
				using (var webClient = new System.Net.WebClient())
				{
					webClient.Headers.Add ("user-agent", "Mozilla/5.0");
					VimeoVideoConfiguration = webClient.DownloadString(ConfigURL);
				}

				if (VimeoVideoConfiguration == string.Empty) {
					return null;
				} else {

					VimeoVideoConfigurationRootObject VimeoVideoConfigurationRoot = JsonConvert.DeserializeObject<VimeoVideoConfigurationRootObject>(WebUtility.HtmlDecode (VimeoVideoConfiguration));

					return new VideoDownloadURL (VimeoVideoConfigurationRoot.request.files.h264.hd.url, VimeoVideoConfigurationRoot.video.title);
				}
			}
			return null;
		}
	}
}

