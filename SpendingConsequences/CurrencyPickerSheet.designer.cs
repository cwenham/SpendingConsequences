// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace SpendingConsequences
{
	[Register ("CurrencyPickerSheet")]
	partial class CurrencyPickerSheet
	{
		[Outlet]
		MonoTouch.UIKit.UIPickerView currencyPicker { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton useButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton cancelButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (currencyPicker != null) {
				currencyPicker.Dispose ();
				currencyPicker = null;
			}

			if (useButton != null) {
				useButton.Dispose ();
				useButton = null;
			}

			if (cancelButton != null) {
				cancelButton.Dispose ();
				cancelButton = null;
			}
		}
	}
}
