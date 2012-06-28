using System;
using System.Collections.Generic;
using System.Linq;

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
			var mainProfile = Profile.Load ("ConsequenceCalculators.xml");

			if (mainProfile != null)
				Profiles.Add("main", mainProfile);
			
			window = new UIWindow (UIScreen.MainScreen.Bounds);
			
			viewController = new SpendingConsequencesViewController (Profiles);
			navController = new UINavigationController (viewController);
			navController.SetNavigationBarHidden (true, false);
			window.RootViewController = navController;
			window.MakeKeyAndVisible ();
			
			return true;
		}
	}
}

