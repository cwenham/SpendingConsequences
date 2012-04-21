// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace SpendingConsequences
{
	[Register ("SpendingConsequencesViewController")]
	partial class SpendingConsequencesViewController
	{
		[Outlet]
		MonoTouch.UIKit.UITableView ConsequenceView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UISegmentedControl SpendingMode { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField InitialAmount { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ConsequenceView != null) {
				ConsequenceView.Dispose ();
				ConsequenceView = null;
			}

			if (SpendingMode != null) {
				SpendingMode.Dispose ();
				SpendingMode = null;
			}

			if (InitialAmount != null) {
				InitialAmount.Dispose ();
				InitialAmount = null;
			}
		}
	}
}
