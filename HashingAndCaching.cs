using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace YouTubeFeast
{
	public class HashingAndCaching
	{
		private String HashCacheFile;

		private Dictionary<String,String> Cache;

		public HashingAndCaching(String HashCachePath)
		{
			HashCacheFile = HashCachePath+System.IO.Path.DirectorySeparatorChar+".hashcache";

			// initialize the cache
			Cache = new Dictionary<string, string>();

			// fill the cache, if the cache file exists...
			if (File.Exists(HashCacheFile))
			{
				String[] initialCache = File.ReadAllLines(HashCacheFile);

				// this is the format of this cache file:
				// 1078921e445578a5dc2249b052b84debfilename.m4v
				// where 1078921e445578a5dc2249b052b84deb is the hash and filename.m4v is the filename
				// that means: 32 bytes for the hash, the rest for the filename

				foreach(String line in initialCache)
				{
					// add this hash,filename combination to the cache dictionary
					Cache.Add(line.Remove(32),line.Remove(0,32));
				}
			}
		}

		public String GetFilenameForHash(String Hash)
		{
			return "";
		}

		public bool HashCodeExists(String Hash)
		{
			if (Cache.ContainsKey(Hash))
				return true;
			else
				return false;
		}

		public void AddToCache(String Hash, String Filename)
		{
			try
			{
				Cache.Add(Hash,Filename);

				SerializeToDisk();
			}
			catch(Exception)
			{
			}
		}

		public String ReplaceFilenameInCache(String Hash, String Filename)
		{
			String ReturnValue = "";
			try
			{
				ReturnValue = Cache[Hash];

				Cache[Hash] = Filename;

				SerializeToDisk();
			}
			catch(Exception)
			{
			}

			return ReturnValue;
		}

		public String CalculateMD5Sum(string file)
		{
			FileStream FileCheck = System.IO.File.OpenRead(file);
			FileInfo f = new FileInfo(file);

			// MD5-Hash aus dem Byte-Array berechnen
			MD5 md5 = new MD5CryptoServiceProvider();

			FileCheck.Seek((int)(f.Length/2),SeekOrigin.Begin);

			byte[] md5Hash = md5.ComputeHash(FileCheck);

			FileCheck.Close();
			            
			return BitConverter.ToString(md5Hash).Replace("-", "").ToLower();
		}

		/// <summary>
		/// Serializes the cache to disk.
		/// </summary>
		private void SerializeToDisk()
		{
			TextWriter tw = new StreamWriter(HashCacheFile,false);

			foreach(KeyValuePair<String,String> kvpair in Cache)
			{
				tw.WriteLine(kvpair.Key+kvpair.Value);
			}

			tw.Close();
		}
	}
}

