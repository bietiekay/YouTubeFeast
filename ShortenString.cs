using System;

namespace YouTubeFeast
{
	public static class ShortenString
	{
		public static string LimitCharacters(string text, int length) 
		{
		    // If text in shorter or equal to length, just return it
		    if (text.Length <= length) 
			{
		        return text;
		    }
		
		    // Text is longer, so try to find out where to cut
		    char[] delimiters = new char[] { ' ', '.', ',', ':', ';' };
		    int index = text.LastIndexOfAny(delimiters, length - 3);
		    
		    if (index > (length / 2)) 
			{
		        return text.Substring(0, index) + "...";
		    }
		    else 
			{
		        return text.Substring(0, length - 3) + "...";
		    }
		}				
	}
}

