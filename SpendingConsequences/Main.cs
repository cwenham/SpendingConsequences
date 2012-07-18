using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

#if DEBUG
using TestFlightSdk;
#endif

namespace SpendingConsequences
{
	public class Application
	{		
		// This is the main entry point of the application.
		static void Main (string[] args)
		{
			try {
#if DEBUG
				// Start the TestFlight API
				TestFlight.TakeOff (@"e75d3cd1ee2cf1cd64beeacc25289f34_OTY2OTkyMDEyLTA2LTA0IDE5OjA3OjQwLjY5Mzk2MQ");
#endif
				
				UIApplication.Main (args, null, "AppDelegate");				
			} catch (Exception ex) {
				Console.WriteLine (string.Format ("{0} thrown: {1}", ex.GetType ().Name, ex.Message));
			}

		}
	}
}
