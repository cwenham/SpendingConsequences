using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace SpendingConsequences
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		SpendingConsequencesViewController viewController;
		UINavigationController navController;
		
		public Dictionary<string,Profile> Profiles { get; private set; }

		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			Profiles = new Dictionary<string, Profile>();

			// The main profile is never changed by the app and will be replaced on upgrades
			var mainProfile = Profile.Load ("ConsequenceCalculators.xml");

			if (mainProfile != null)
				Profiles.Add("main", mainProfile);

			LoadUserProfiles();

			window = new UIWindow (UIScreen.MainScreen.Bounds);
			
			viewController = new SpendingConsequencesViewController (Profiles);
			navController = new UINavigationController (viewController);
			navController.SetNavigationBarHidden (true, false);
			window.RootViewController = navController;
			window.MakeKeyAndVisible ();
			
			return true;
		}

		private void LoadUserProfiles () {
			// Check Documents/Inbox for any files being given to us, move to Library
			string inboxFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Inbox");
			string libFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library");

			if (Directory.Exists(inboxFolder))
				foreach (var file in Directory.GetFiles("Documents/Inbox", "*.sbprofile"))
					try {
					File.Move(file, Path.Combine(libFolder, Path.GetFileName(file)));					
					} catch (Exception ex) {
					Console.WriteLine("{0} thrown when moving file: {1}", ex.GetType().Name, ex.Message);
					}

			// Load any user profiles
			if (Directory.Exists(libFolder))
				foreach (var file in Directory.GetFiles (libFolder, "*.sbprofile"))
					try {
						Profile userProfile = Profile.Load (file);
						if (userProfile != null)
							Profiles.Add (Path.GetFileName (file), userProfile);
					} catch (Exception ex) {
					Console.WriteLine("{0} thrown when loading profile at {1}: {2}", ex.GetType().Name, file, ex.Message);
					}
		}
	}
}

