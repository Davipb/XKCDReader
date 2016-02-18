using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace XKCDReader.Tests
{
	[TestClass]
	public class MainWindowTests
	{
		#region AddComic
		[TestCategory("AddComic")]
		[TestMethod]
		public void AddComic_Normal_AddsComic()
		{
			var result = new XKCDComic(500);

			var viewModel = new MainWindowViewModel(new MockMessageService(), new MockComicService(result), new MockPropertiesService());
			viewModel.AddComicCommand.Execute(null);

			Assert.AreEqual(viewModel.Comics.Count, 1);
			Assert.IsTrue(viewModel.Comics.Contains(result));
		}

		[TestCategory("AddComic")]
		[TestMethod]
		public void AddComic_Normal_SelectsAddedComic()
		{
			var result = new XKCDComic(500);

			var viewModel = new MainWindowViewModel(new MockMessageService(), new MockComicService(result), new MockPropertiesService());
			viewModel.AddComicCommand.Execute(null);

			Assert.AreEqual(viewModel.SelectedComic, result);
		}


		[TestCategory("AddComic")]
		[TestMethod]
		public void AddComic_UserCancels_DoesntAddComic()
		{
			var viewModel = new MainWindowViewModel(new MockMessageService(), new MockComicService(null), new MockPropertiesService());
			viewModel.AddComicCommand.Execute(null);

			Assert.IsFalse(viewModel.Comics.Count > 0);
		}
		#endregion

		[TestCategory("AddCurrentComic")]
		[TestMethod]
		public void AddCurrentComic_Normal_AddsComic()
		{
			var current = new XKCDComic(500);

			var viewModel = new MainWindowViewModel(new MockMessageService(), new MockComicService(current), new MockPropertiesService());
			viewModel.LoadedCommand.ExecuteAsync(null).Wait();
			viewModel.AddCurrentComicCommand.ExecuteAsync(null).Wait();

			Assert.AreEqual(viewModel.Comics.Count, 1);
			Assert.IsTrue(viewModel.Comics.Contains(current));
		}

		#region AddRandomComic
		[TestCategory("AddRandomComic")]
		[TestMethod]
		public void AddRandomComic_Type0WhenEmpty_AddsComic()
		{
			var current = new XKCDComic(500);

			var viewModel = new MainWindowViewModel(new MockMessageService(), new MockComicService(current), new MockPropertiesService());
			viewModel.LoadedCommand.ExecuteAsync(null).Wait();
			viewModel.AddRandomComicCommand.ExecuteAsync("0").Wait();

			Assert.AreEqual(viewModel.Comics.Count, 1);

			var comicNumber = viewModel.Comics[0].Number;

			Assert.IsTrue(comicNumber > 1);
			Assert.IsTrue(comicNumber <= current.Number);
		}

		[TestCategory("AddRandomComic")]
		[TestMethod]
		public void AddRandomComic_Type0WhenFull_SelectsComic()
		{
			int totalComics = 10;
			var allComics = new List<XKCDComic>();

			var comicService = new MockComicService(new XKCDComic(totalComics));
			var viewModel = new MainWindowViewModel(new MockMessageService(), comicService, new MockPropertiesService());
			viewModel.LoadedCommand.ExecuteAsync(null).Wait();

			for (int i = 1; i <= totalComics; i++)
			{
				var comic = new XKCDComic(i);
				allComics.Add(comic);
				viewModel.Comics.Add(comic);
			}

			viewModel.AddRandomComicCommand.ExecuteAsync("0").Wait();

			Assert.AreEqual(viewModel.Comics.Count, totalComics);
			Assert.IsTrue(allComics.Contains(viewModel.SelectedComic));
		}

		[TestCategory("AddRandomComic")]
		[TestMethod]
		public void AddRandomComic_Type1WhenEmpty_AddsComic()
		{
			var current = new XKCDComic(10);
			var comicService = new MockComicService(current);

			var viewModel = new MainWindowViewModel(new MockMessageService(), comicService, new MockPropertiesService());
			viewModel.LoadedCommand.ExecuteAsync(null).Wait();
			viewModel.AddRandomComicCommand.ExecuteAsync("1").Wait();

			Assert.AreEqual(viewModel.Comics.Count, 1);

			var comicNumber = comicService.LastNumberParameter;

			Assert.IsTrue(comicNumber > 1);
			Assert.IsTrue(comicNumber <= current.Number);
		}

		[TestCategory("AddRandomComic")]
		[TestMethod]
		public void AddRandomComic_Type1WhenPartiallyFull_AddsComic()
		{
			int totalComics = 10;
			int filledComics = 5;
			var allComics = new List<XKCDComic>();

			var comicService = new MockComicService(new XKCDComic(totalComics));
			var viewModel = new MainWindowViewModel(new MockMessageService(), comicService, new MockPropertiesService());
			viewModel.LoadedCommand.ExecuteAsync(null).Wait();

			for (int i = 1; i <= filledComics; i++)
			{
				var comic = new XKCDComic(i);
				allComics.Add(comic);
				viewModel.Comics.Add(comic);
			}

			viewModel.AddRandomComicCommand.ExecuteAsync("1").Wait();

			Assert.AreEqual(viewModel.Comics.Count, filledComics + 1);
			Assert.IsFalse(allComics.Contains(viewModel.SelectedComic));
		}

		[TestCategory("AddRandomComic")]
		[TestMethod]
		public void AddRandomComic_Type1WhenFull_DoesNothing()
		{
			int totalComics = 10;

			var comicService = new MockComicService(new XKCDComic(totalComics));
			var viewModel = new MainWindowViewModel(new MockMessageService(), comicService, new MockPropertiesService());
			viewModel.LoadedCommand.ExecuteAsync(null).Wait();

			for (int i = 1; i <= totalComics; i++)
			{
				viewModel.Comics.Add(new XKCDComic(i));
			}

			viewModel.AddRandomComicCommand.ExecuteAsync("1").Wait();

			Assert.AreEqual(viewModel.Comics.Count, totalComics);
			Assert.IsNull(viewModel.SelectedComic);
		}

		[TestCategory("AddRandomComic")]
		[TestMethod]
		public void AddRandomComic_Type2WhenEmpty_DoesNothing()
		{
			var current = new XKCDComic(10);

			var viewModel = new MainWindowViewModel(new MockMessageService(), new MockComicService(current), new MockPropertiesService());
			viewModel.LoadedCommand.ExecuteAsync(null).Wait();
			viewModel.AddRandomComicCommand.ExecuteAsync("2").Wait();

			Assert.AreEqual(viewModel.Comics.Count, 0);
			Assert.IsNull(viewModel.SelectedComic);
		}

		[TestCategory("AddRandomComic")]
		[TestMethod]
		public void AddRandomComic_Type2WhenPartiallyFull_SelectsComic()
		{
			int totalComics = 10;
			int filledComics = 5;
			var allComics = new List<XKCDComic>();

			var comicService = new MockComicService(new XKCDComic(totalComics));
			var viewModel = new MainWindowViewModel(new MockMessageService(), comicService, new MockPropertiesService());
			viewModel.LoadedCommand.ExecuteAsync(null).Wait();

			for (int i = 1; i <= filledComics; i++)
			{
				var comic = new XKCDComic(i);
				allComics.Add(comic);
				viewModel.Comics.Add(comic);
			}

			viewModel.AddRandomComicCommand.ExecuteAsync("2").Wait();

			Assert.AreEqual(viewModel.Comics.Count, filledComics);
			Assert.IsTrue(allComics.Contains(viewModel.SelectedComic));
		}

		[TestCategory("AddRandomComic")]
		[TestMethod]
		public void AddRandomComic_Type2WhenFull_SelectsComic()
		{
			int totalComics = 10;
			var allComics = new List<XKCDComic>();

			var comicService = new MockComicService(new XKCDComic(totalComics));
			var viewModel = new MainWindowViewModel(new MockMessageService(), comicService, new MockPropertiesService());
			viewModel.LoadedCommand.ExecuteAsync(null).Wait();

			for (int i = 1; i <= totalComics; i++)
			{
				var comic = new XKCDComic(i);
				allComics.Add(comic);
				viewModel.Comics.Add(comic);
			}

			viewModel.AddRandomComicCommand.ExecuteAsync("2").Wait();

			Assert.AreEqual(viewModel.Comics.Count, totalComics);
			Assert.IsTrue(allComics.Contains(viewModel.SelectedComic));
		}
		#endregion

		[TestCategory("RemoveComic")]
		[TestMethod]
		public void RemoveComic_Normal_RemovesComic()
		{
			var viewModel = new MainWindowViewModel(new MockMessageService(), new MockComicService(new XKCDComic(500)), new MockPropertiesService());
			viewModel.AddComicCommand.Execute(null);
			viewModel.RemoveComicCommand.Execute(null);

			Assert.AreEqual(viewModel.Comics.Count, 0);
		}

		#region Loaded
		[TestCategory("Loaded")]
		[TestMethod]
		public void Loaded_WhenNoConfiguration_DoesNothing()
		{
			var viewModel = new MainWindowViewModel(new MockMessageService(), new MockComicService(new XKCDComic(500)), new MockPropertiesService());
			viewModel.LoadedCommand.ExecuteAsync(null).Wait();

			Assert.AreEqual(viewModel.Comics.Count, 0);
			Assert.IsNull(viewModel.SelectedComic);
		}

		[TestCategory("Loaded")]
		[TestMethod]
		public void Loaded_WhenConfigurationPresent_LoadsConfiguration()
		{
			var config = new[] { 1, 100, 150, 200, 250, 300, 350, 400, 450, 500 };
			var properties = new MockPropertiesService
			{
				ConfigurationFile = config
			};

			var viewModel = new MainWindowViewModel(new MockMessageService(), new MockComicService(new XKCDComic(500)), properties);
			viewModel.LoadedCommand.ExecuteAsync(null).Wait();

			Assert.AreEqual(viewModel.Comics.Count, config.Length);
			Assert.IsTrue(viewModel.Comics.Select(x => x.Number).OrderBy(x => x).SequenceEqual(config));
		}

		[TestCategory("Loaded")]
		[TestMethod]
		public void Loaded_WhenLoadCurrentIsTrue_LoadsCurrent()
		{
			var current = new XKCDComic(500);
			var properties = new MockPropertiesService
			{
				LoadCurrent = true
			};

			var viewModel = new MainWindowViewModel(new MockMessageService(), new MockComicService(current), properties);
			viewModel.LoadedCommand.ExecuteAsync(null).Wait();

			Assert.AreEqual(viewModel.Comics.Count, 1);
			Assert.IsTrue(viewModel.Comics.Contains(current));
		}
		#endregion

		#region Closed
		[TestCategory("Closed")]
		[TestMethod]
		public void Closed_WhenSaveCacheIsFalse_DeletesCache()
		{
			bool called = false;
			var comicService = new MockComicService(null);
			comicService.ClearCacheCalled += () => called = true;

			var viewModel = new MainWindowViewModel(new MockMessageService(), comicService, new MockPropertiesService { SaveCache = false });
			viewModel.ClosedCommand.ExecuteAsync(null).Wait();

			Assert.IsTrue(called);
		}

		[TestCategory("Closed")]
		[TestMethod]
		public void Closed_Normal_SavesConfigurationFile()
		{
			IEnumerable<XKCDComic> allComics = new[]
			{
				new XKCDComic(50), new XKCDComic(100), new XKCDComic(150),
				new XKCDComic(200), new XKCDComic(250), new XKCDComic(300)
			};

			var properties = new MockPropertiesService();

			var viewModel = new MainWindowViewModel(new MockMessageService(), new MockComicService(null), properties);

			foreach (var comic in allComics) viewModel.Comics.Add(comic);

			viewModel.ClosedCommand.ExecuteAsync(null).Wait();

			Assert.IsTrue(properties.ConfigurationFile.SequenceEqual(allComics.Select(x => x.Number)));
		}
		#endregion

	}
}
