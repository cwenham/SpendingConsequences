// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace SpendingConsequences
{
	[Register ("MonthsControl")]
	partial class MonthsControl
	{
		[Outlet]
		MonoTouch.UIKit.UILabel caption { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIStepper stepper { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel configuredValue { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (caption != null) {
				caption.Dispose ();
				caption = null;
			}

			if (stepper != null) {
				stepper.Dispose ();
				stepper = null;
			}

			if (configuredValue != null) {
				configuredValue.Dispose ();
				configuredValue = null;
			}
		}
	}
}
