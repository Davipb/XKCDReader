using System.Threading.Tasks;

namespace XKCDReader
{
	public interface IComicService
	{
		Task<XKCDComic> GetCurrent();
		Task<XKCDComic> GetWithNumber(int number);
		void ClearCache();
	}

	public class ComicService : IComicService
	{
		public async Task<XKCDComic> GetCurrent()
			=> await XKCDComic.FromCurrent();

		public async Task<XKCDComic> GetWithNumber(int number)
			=> await XKCDComic.FromComicNumber(number);

		public void ClearCache()
			=> XKCDComic.ClearCache();
	}
}
