
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace SpendingConsequences
{
	public partial class ConsequenceFooterViewController : UIViewController
	{
		public ConsequenceFooterViewController () : base ("ConsequenceFooterViewController", null)
		{
		}
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			ArtRepository.StyleButton("embossed", this.addConsequenceButton);
			this.addConsequenceButton.TouchUpInside += delegate(object sender, EventArgs e) {
				AddConsequenceRequested(this, e);
			};
		}

		public event EventHandler<EventArgs> AddConsequenceRequested = delegate{};
	}
}

