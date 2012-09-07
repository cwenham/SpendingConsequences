// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace SpendingConsequences
{
	[Register ("MoneyControl")]
	partial class MoneyControl
	{
		[Outlet]
		MonoTouch.UIKit.UILabel caption { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField editCaption { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField configuredValue { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel currencyLabel { get; set; }

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

			if (configuredValue != null) {
				configuredValue.Dispose ();
				configuredValue = null;
			}

			if (currencyLabel != null) {
				currencyLabel.Dispose ();
				currencyLabel = null;
			}

			if (currencyButton != null) {
				currencyButton.Dispose ();
				currencyButton = null;
			}
		}
	}
}
