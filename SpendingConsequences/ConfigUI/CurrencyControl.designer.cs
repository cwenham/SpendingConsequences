// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace SpendingConsequences
{
	[Register ("CurrencyControl")]
	partial class CurrencyControl
	{
		[Outlet]
		MonoTouch.UIKit.UILabel caption { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField editCaption { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel currencyName { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton currencyButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (caption != null) {
				caption.Dispose ();
				caption = null;
			}

			if (editCaption != null) {
				editCaption.Dispose ();
				editCaption = null;
			}

			if (currencyName != null) {
				currencyName.Dispose ();
				currencyName = null;
			}

			if (currencyButton != null) {
				currencyButton.Dispose ();
				currencyButton = null;
			}
		}
	}
}
