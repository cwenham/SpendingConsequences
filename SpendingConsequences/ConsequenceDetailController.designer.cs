// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace SpendingConsequences
{
	[Register ("ConsequenceDetailController")]
	partial class ConsequenceDetailController
	{
		[Outlet]
		MonoTouch.UIKit.UIScrollView scrollView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel calculatedAmount { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel caption { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField editCaption { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView resultSubview { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView iconView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (scrollView != null) {
				scrollView.Dispose ();
				scrollView = null;
			}

			if (calculatedAmount != null) {
				calculatedAmount.Dispose ();
				calculatedAmount = null;
			}

			if (caption != null) {
				caption.Dispose ();
				caption = null;
			}

			if (editCaption != null) {
				editCaption.Dispose ();
				editCaption = null;
			}

			if (resultSubview != null) {
				resultSubview.Dispose ();
				resultSubview = null;
			}

			if (iconView != null) {
				iconView.Dispose ();
				iconView = null;
			}
		}
	}
}
