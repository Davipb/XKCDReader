using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace XKCDReader
{
	public class XKCDComic : IEquatable<XKCDComic>
	{
		public const string CacheFolder = "Cache/";
		const string DownloadAddress = "http://xkcd.com/{0}/info.0.json";
		const string CurrentComicAddress = "http://xkcd.com/info.0.json";

		public int Number { get; }

		public string Title { get; protected set; }
		public string HoverText { get; protected set; }
		public string Transcript { get; protected set; }

		public BitmapImage Image { get; set; }

		public string Year { get; protected set; }
		public string Month { get; protected set; }
		public string Day { get; protected set; }

		public XKCDComic(int number)
		{
			Number = number;
		}

		public static bool operator ==(XKCDComic a, XKCDComic b)
		{
			if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
				return Object.Equals(a, b);

			return a.Equals(b);
		}

		public static bool operator !=(XKCDComic a, XKCDComic b)
			=> !(a == b);

		public override int GetHashCode()
			=> Number.GetHashCode();

		public override bool Equals(object obj)
			=> Equals(obj as XKCDComic);

		public bool Equals(XKCDComic other)
		{
			if (ReferenceEquals(other, null))
				return false;

			return other.Number == Number;
		}

		/// <summary>
		/// Deletes this comic's cache, erasing its downloaded files
		/// </summary>
		public void DeleteCache()
		{
			var localCache = Path.GetFullPath(Path.Combine(CacheFolder, $"{Number}.json"));
			var localImage = Path.GetFullPath(Path.Combine(CacheFolder, $"{Number}.png"));

			if (File.Exists(localCache))
				File.Delete(localCache);

			if (File.Exists(localImage))
				File.Delete(localImage);
		}

		/// <summary>
		/// Deletes the cache, erasing all downloaded files (for all comics)
		/// </summary>
		public static void ClearCache()
		{
			var cache = new DirectoryInfo(Path.GetFullPath(CacheFolder));

			foreach (var file in cache.GetFiles())
				file.Delete();

			cache.Delete();
		}

		/// <summary>
		/// Gets all cached comics
		/// </summary>
		/// <returns>All cached comics</returns>
		public static async Task<IEnumerable<XKCDComic>> GetCached()
		{
			var cache = Path.GetFullPath(CacheFolder);
			Directory.CreateDirectory(cache);
			var result = new List<XKCDComic>();

			foreach (var comic in Directory.EnumerateFiles(cache, "*.json", SearchOption.TopDirectoryOnly))
			{
				result.Add(await FromComicNumber(int.Parse(Path.GetFileNameWithoutExtension(comic))));
			}

			return result;
		}

		/// <summary>
		/// Gets xkcd's current comic
		/// </summary>
		/// <returns>The current xkcd comic</returns>
		public static async Task<XKCDComic> FromCurrent()
		{
			var obj = JObject.Parse(
				await new WebClient().DownloadStringTaskAsync(CurrentComicAddress));

			return await FromComicNumber((int)obj["num"]);
		}

		/// <summary>
		/// Gets the xkcd comic with number <paramref name="number"/>
		/// </summary>
		/// <param name="number">The number of the comic to get</param>
		/// <returns>A Task, which when awaited returns the comic with number <paramref name="number"/></returns>
		public static async Task<XKCDComic> FromComicNumber(int number)
		{
			var localCache = Path.GetFullPath(Path.Combine(CacheFolder, $"{number}.json"));

			JObject comic = null;
			bool download = true;

			if (File.Exists(localCache))
			{
				try
				{
					comic = JObject.Parse(File.ReadAllText(localCache));
					download = false;
				}
				catch (Exception e) when (e is JsonException || e is IOException)
				{
					// Cache corrupt, redownload
					File.Delete(localCache);
					download = true;
				}
			}

			if (download)
			{
				try
				{
					var info = new FileInfo(localCache);
					info.Directory.Create();
					info.Create().Close();
					var client = new WebClient { Encoding = Encoding.UTF8 };
					await client.DownloadFileTaskAsync(string.Format(DownloadAddress, number), localCache);
					comic = JObject.Parse(File.ReadAllText(localCache));
				}
				catch (Exception e) when (e is IOException || e is WebException || e is JsonException)
				{
					File.Delete(localCache);
					throw;
				}
			}

			return await FromJson(comic);
		}

		/// <summary>
		/// Loads an xkcd comic from JSON
		/// </summary>
		/// <param name="obj">The JSON object to load the comic from</param>
		/// <returns>A Task, which when awaited returns the xkcd represented by the JSON Object <paramref name="obj"/></returns>
		public static async Task<XKCDComic> FromJson(JObject obj)
		{
			var result = new XKCDComic((int)obj["num"])
			{
				Title = (string)obj["title"],
				HoverText = (string)obj["alt"],
				Transcript = (string)obj["transcript"],
				Year = (string)obj["year"],
				Month = (string)obj["month"],
				Day = (string)obj["day"],
				Image = await LoadImage((int)obj["num"], (string)obj["img"])
			};

			return result;
		}

		/// <summary>
		/// Loads the image of the comic of number <paramref name="number"/>. If the image is not cached, downloads it from the URI <paramref name="uri"/>
		/// </summary>
		/// <param name="number">The number of the comic to get the image of</param>
		/// <param name="uri">The URI to download the image in case it is not cached</param>
		/// <returns>A Task, which when awaited returns the loaded Image</returns>
		static async Task<BitmapImage> LoadImage(int number, string uri)
		{
			var localCache = Path.GetFullPath(Path.Combine(CacheFolder, $"{number}.png"));
			if (!File.Exists(localCache))
			{
				var info = new FileInfo(localCache);
				info.Directory.Create();
				info.Create().Close();
				await new WebClient().DownloadFileTaskAsync(uri, localCache);
			}

			var image = new BitmapImage();
			image.BeginInit();
			// Setting the cache to OnLoad ensures we can delete the local downloaded image without exceptions
			// (Leaving it to the default makes WPF lock the file)
			image.CacheOption = BitmapCacheOption.OnLoad;
			image.UriSource = new Uri(localCache);
			image.EndInit();
			// Async method may run in a different thread, so we need to freeze the image to make sure it works
			// in all threads (if not frozen, it can only be used in the thread that created it)
			image.Freeze();
			return image;
		}
	}
}
