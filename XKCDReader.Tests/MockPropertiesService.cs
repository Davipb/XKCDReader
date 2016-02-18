using System.Collections.Generic;
using System.Threading.Tasks;
using XKCDReader.Services;

namespace XKCDReader.Tests
{
	public class MockPropertiesService : IPropertiesService
	{
		public IEnumerable<int> ConfigurationFile { get; set; } = new int[] { };

		public bool LoadCurrent { get; set; } = false;
		public bool PurgeComic { get; set; } = false;
		public bool SaveCache { get; set; } = true;

		public void Reload() { }
		public void Reset() { }
		public void Save() { }

		public IEnumerable<int> ReadConfigurationFile(string file)
			=> ConfigurationFile;

		public async Task SaveConfigurationFile(string file, IEnumerable<int> config)
		{
			ConfigurationFile = config;
			await Task.FromResult(0);
		}
	}
}
