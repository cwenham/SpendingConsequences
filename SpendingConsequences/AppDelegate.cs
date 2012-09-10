using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.ComponentModel;
using System.Json;
using System.Net;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		private const string FallbackExchangeRates = "fallbackExchangeRates.json";
		private const string LatestExchangeRatesURL = "http://openexchangerates.org/api/latest.json?app_id=9c9c379497a649a9b807b339c5579b7e";
		private const string ExchangeRateCacheFilename = "latest.json";

		// class-level declarations
		UIWindow window;
		SpendingConsequencesViewController viewController;
		UINavigationController navController;
		
		public AppProfile Profile { get; private set; }

		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			Profile = new AppProfile("ConsequenceCalculators.xml");

			LoadUserProfiles();
			UpdateExchangeRates();

			window = new UIWindow (UIScreen.MainScreen.Bounds);
			
			viewController = new SpendingConsequencesViewController (Profile);
			navController = new UINavigationController (viewController);
			window.RootViewController = navController;
			window.MakeKeyAndVisible ();
			
			return true;
		}

		public override void WillEnterForeground (UIApplication application)
		{
			if (ExchangeRates.CurrentRates.OldestQuote < DateTime.Now.AddHours(-24))
				UpdateExchangeRates();
		}

		private void LoadUserProfiles () {
			if (Directory.Exists(SubProfile.InboxFolder))
				foreach (var file in Directory.GetFiles("Documents/Inbox", "*.sbprofile"))
					try {
					File.Move(file, Path.Combine(SubProfile.InboxFolder, Path.GetFileName(file)));					
					} catch (Exception ex) {
					Console.WriteLine("{0} thrown when moving file: {1}", ex.GetType().Name, ex.Message);
					}

			// Load any user profiles
			if (Directory.Exists(SubProfile.LibraryFolder))
				foreach (var file in Directory.GetFiles (SubProfile.LibraryFolder, "*.sbprofile"))
					try {
						SubProfile userProfile = SubProfile.Load (file);
						if (userProfile != null)
							Profile.AddSubProfile (Path.GetFileNameWithoutExtension (file), userProfile);
					} catch (Exception ex) {
					Console.WriteLine("{0} thrown when loading profile at {1}: {2}", ex.GetType().Name, file, ex.Message);
					}
		}

		private BackgroundWorker ForexWorker;

		public void UpdateExchangeRates ()
		{
			ExchangeRates cachedRates = null;
			string cachePath = Path.Combine (SubProfile.LibraryFolder, ExchangeRateCacheFilename);
			if (File.Exists (cachePath)) {
				cachedRates = ExchangeRates.FromOXLatestJson (cachePath);
				ExchangeRates.CurrentRates = cachedRates;
				if (cachedRates.OldestQuote > DateTime.Now.AddHours(-24))
				{
					// Cached rates are less than a day old, keep using them
					return;
				}
			}

			if (ForexWorker != null && ForexWorker.IsBusy)
				ForexWorker.CancelAsync();

			ForexWorker = new BackgroundWorker();
			ForexWorker.WorkerSupportsCancellation = true;
			ForexWorker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e) {
				if (!e.Cancelled)
				{
					// Try to load new rates, if available, and replace the cache if reading was successful
					if (e.Result != null && e.Result is String)
					{
						try {
							ExchangeRates latestRates = ExchangeRates.FromOXLatestJson(e.Result as string);	
							if (latestRates != null)
							{
								ExchangeRates.CurrentRates = latestRates;
								File.Move (e.Result as string, cachePath);
								return;
							}
						} catch (Exception) {
							File.Delete (e.Result as String);
						}
					};

					// If we managed to load cached rates from earlier, use those in case above fails
					if (cachedRates != null)
					{
						ExchangeRates.CurrentRates = cachedRates;
						return;
					}

					// Resort to our Fallback rates, which are only updated whenever the app itself is
					try {
						ExchangeRates latestRates = ExchangeRates.FromOXLatestJson(FallbackExchangeRates);	
						if (latestRates != null)
						{
							ExchangeRates.CurrentRates = latestRates;
							return;
						}
					} catch (Exception ex) {
						Console.WriteLine("{0} thrown when trying to read Fallback exchange rates: {1}", ex.GetType().Name, ex.Message);
					}
				}
			};
			ForexWorker.DoWork += HandleForexDoWork;
			ForexWorker.RunWorkerAsync();
		}

		void HandleForexDoWork (object sender, DoWorkEventArgs e)
		{
			e.Result = null;
			BackgroundWorker myWorker = sender as BackgroundWorker;

			string tempFile = Path.GetTempFileName ();
			try {
				using (WebClient wc = new WebClient()) {
					wc.DownloadFile (LatestExchangeRatesURL, tempFile);
				}

				e.Cancel = myWorker.CancellationPending;
				if (!myWorker.CancellationPending)
					e.Result = tempFile;
				else
					File.Delete(tempFile);
			} catch (Exception ex) {
				Console.WriteLine ("{0} thrown when fetching latest exchange rates: {1}", ex.GetType ().Name, ex.Message);
				if (!File.Exists (tempFile))
					File.Delete (tempFile);
			}
		}
	}
}

