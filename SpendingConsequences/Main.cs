using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace SpendingConsequences
{
	public class Application
	{
		// This is the main entry point of the application.
		static void Main (string[] args)
		{
			try {
				UIApplication.Main (args, null, "AppDelegate");				
			} catch (Exception ex) {
				Console.WriteLine (string.Format ("{0} thrown: {1}", ex.GetType ().Name, ex.Message));
			}

		}
	}
}
