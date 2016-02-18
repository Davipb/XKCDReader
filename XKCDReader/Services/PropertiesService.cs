using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace XKCDReader.Services
{
	public interface IPropertiesService
	{
		bool LoadCurrent { get; set; }
		bool SaveCache { get; set; }
		bool PurgeComic { get; set; }
		IEnumerable<int> ReadConfigurationFile(string file);
		Task SaveConfigurationFile(string file, IEnumerable<int> config);
		void Save();
		void Reload();
		void Reset();
	}

	public class PropertiesService : IPropertiesService
	{
		public bool LoadCurrent
		{
			get { return Properties.Settings.Default.LoadCurrent; }
			set { Properties.Settings.Default.LoadCurrent = value; }
		}

		public bool PurgeComic
		{
			get { return Properties.Settings.Default.PurgeComic; }
			set { Properties.Settings.Default.PurgeComic = value; }
		}

		public bool SaveCache
		{
			get { return Properties.Settings.Default.SaveCache; }
			set { Properties.Settings.Default.SaveCache = value; }
		}

		public void Reset()
			=> Properties.Settings.Default.Reset();

		public void Reload()
			=> Properties.Settings.Default.Reload();

		public void Save()
			=> Properties.Settings.Default.Save();

		public IEnumerable<int> ReadConfigurationFile(string file)
			=> File.ReadAllLines(file).Select(x => int.Parse(x));

		public async Task SaveConfigurationFile(string file, IEnumerable<int> config)
		{
			using (var writer = new StreamWriter(File.Open(file, FileMode.Create)))
			{
				foreach (var line in config)
					await writer.WriteLineAsync(line.ToString());
			}
		}
	}
}
