using System.Threading.Tasks;
using System.Windows;

namespace XKCDReader.Services
{
	public interface IComicService
	{
		Task<XKCDComic> GetCurrent();
		Task<XKCDComic> GetWithNumber(int number);
		XKCDComic GetFromUser(AddComicViewModel context, Window owner);
		void ClearCache();
	}

	public class ComicService : IComicService
	{
		public async Task<XKCDComic> GetCurrent()
			=> await XKCDComic.FromCurrent();

		public async Task<XKCDComic> GetWithNumber(int number)
			=> await XKCDComic.FromComicNumber(number);

		public XKCDComic GetFromUser(AddComicViewModel context, Window owner)
		{
			var window = new AddComicView
			{
				DataContext = context,
				Owner = owner
			};

			if (window.ShowDialog() ?? false)
				return context.Result;

			return null;
		}

		public void ClearCache()
			=> XKCDComic.ClearCache();
	}
}
