using System;
using System.Threading.Tasks;
using XKCDReader.Services;

namespace XKCDReader.Tests
{
	public class MockComicService : IComicService
	{
		public XKCDComic ReturnResult { get; }
		public int LastNumberParameter { get; private set; }

		public event Action ClearCacheCalled;

		public MockComicService(XKCDComic result)
		{
			ReturnResult = result;
		}

		public void ClearCache()
		{
			ClearCacheCalled?.Invoke();
		}

		public async Task<XKCDComic> GetCurrent()
			=> await Task.FromResult(ReturnResult);

		public async Task<XKCDComic> GetWithNumber(int number)
		{
			LastNumberParameter = number;
			return await Task.FromResult(new XKCDComic(number));
		}

		public XKCDComic GetFromUser(AddComicViewModel context, System.Windows.Window owner)
			=> ReturnResult;
	}
}
