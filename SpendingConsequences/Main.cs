using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
//using MonoTouch.TestFlight;

namespace SpendingConsequences
{
	public class Application
	{		
		// This is the main entry point of the application.
		static void Main (string[] args)
		{
			try {
				// Start the TestFlight API
				//TestFlight.TakeOff (@"e75d3cd1ee2cf1cd64beeacc25289f34_OTY2OTkyMDEyLTA2LTA0IDE5OjA3OjQwLjY5Mzk2MQ");

				UIApplication.Main (args, null, "AppDelegate");				
			} catch (Exception ex) {
				Console.WriteLine (string.Format ("{0} thrown: {1}", ex.GetType ().Name, ex.Message));
				//TestFlight.Log("{0} caught in Main: {1}", ex.GetType().Name, ex.Message);
			}

		}
	}
}
