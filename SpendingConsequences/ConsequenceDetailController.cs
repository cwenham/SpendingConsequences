using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public partial class ConsequenceDetailController : UIViewController
	{	
		private const int DYNAMIC_VIEW_TAG = 1024;
		
		public ConsequenceDetailController ()
		{
			NSBundle.MainBundle.LoadNib ("ConsequenceDetailController", this, null);
			this.ViewDidLoad();
			LastLabelYOffset = this.caption.Frame.Y + this.caption.Frame.Height;
		}
		
		// Y + Height of the last label in our XIB, below which we can add dynamic controls
		private float LastLabelYOffset { get; set; }
		
		public ConsequenceResult CurrentResult { get; private set; }
		
		private UIViewController[] CurrentConfiggers { get; set; }
		
		public void SetCurrentResult (ConsequenceResult result)
		{
			CurrentResult = result;
			
			this.calculatedAmount.Text = String.Format (result.Calculator.ResultFormat, result.ComputedValue);
			this.caption.Text = result.FormattedCaption;
			
			// Remove any controls we had dynamically placed before
			foreach (UIView v in scrollView.Subviews.ToArray())
				if (v.Tag == DYNAMIC_VIEW_TAG)
					v.RemoveFromSuperview ();
			
			if (CurrentConfiggers != null)
				foreach (UIViewController configger in CurrentConfiggers.Where(x => x != null))
					configger.View.RemoveFromSuperview ();
			
			float offset = LastLabelYOffset + 10;

			CurrentConfiggers = result.Calculator.ConfigurableValues.Values.Select (x => GetConfigurator (x)).ToArray ();
			foreach (UIViewController configger in CurrentConfiggers.Where(x => x != null)) {
				configger.View.Frame = new RectangleF (0, offset, configger.View.Frame.Width, configger.View.Frame.Height);
				offset += configger.View.Frame.Height + 5;
				this.scrollView.AddSubview (configger.View);
			}
		}
		
		private UIViewController GetConfigurator (ConfigurableValue val)
		{
			// ToDo: Implement this with flyweight pattern
			switch (val.ValueType) {
			case ConfigurableValueType.Integer:
				return null; // ToDo: Complete me
			case ConfigurableValueType.Money:
				return new MoneyControl(val);
			case ConfigurableValueType.Months:
				return new MonthsControl (val);
			case ConfigurableValueType.Percentage:
				return new PercentageControl (val);
			case ConfigurableValueType.Year:
				return new YearControl(val);
			default:
				return null;
			}
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
			
		}
		
		public override void ViewWillAppear (bool animated)
		{
			this.NavigationController.SetNavigationBarHidden (false, true);
			base.ViewWillAppear (animated);
		}
		
		public override void ViewWillDisappear (bool animated)
		{
			NavigationController.SetNavigationBarHidden(true,true);
			base.ViewWillDisappear (animated);
		}
		
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;
			
			ReleaseDesignerOutlets ();
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
		}
	}
}

