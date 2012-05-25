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
		
		public ConsequenceDetailController (Profile profile)
		{
			NSBundle.MainBundle.LoadNib ("ConsequenceDetailController", this, null);
			this.Profile = profile;
			this.ViewDidLoad();
			LastLabelYOffset = this.caption.Frame.Y + this.caption.Frame.Height;
		}
		
		private Profile Profile { get; set; }
		
		// Y + Height of the last label in our XIB, below which we can add dynamic controls
		private float LastLabelYOffset { get; set; }
		
		public ConsequenceResult CurrentResult { get; private set; }
		
		private UIViewController[] CurrentConfiggers { get; set; }
		
		/// <summary>
		/// A view used to display tabular results, such as amortization tables, when the device is rotated to landscape
		/// </summary>
		private WebGridView GridView { get; set; }
		
		public void SetCurrentResult (ConsequenceResult result)
		{
			if (result == null)
				return;
			
			this.NavigationItem.Title = result.Request.Summary;
			
			UpdateCurrentResult (result);
			
			UIImage image = Profile.GetImage (result.ImageName);

			if (image != null)
				iconView.Image = image;
			
			ClearDynamicViews ();
			
			float offset = LastLabelYOffset + 5;

			CurrentConfiggers = result.Calculator.ConfigurableValues.Values.Select (x => GetConfigurator (x)).ToArray ();
			foreach (UIViewController configger in CurrentConfiggers.Where(x => x != null)) {
				configger.View.Frame = new RectangleF (0, offset, configger.View.Frame.Width, configger.View.Frame.Height);
				offset += configger.View.Frame.Height + 5;
				this.scrollView.AddSubview (configger.View);
			}
			
			if (result.Calculator.SupportElements != null && result.Calculator.SupportElements.ContainsKey ("Commentary")) {
				Commentary commentaryElement = result.Calculator.SupportElements ["Commentary"] as Commentary;
				NSAttributedString commentaryFormatted = commentaryElement.ToAttributedString (UIFont.FromName ("Baskerville", 17.0f));
				
				CoreTextView commentView = new CoreTextView ();
				commentView.Tag = DYNAMIC_VIEW_TAG;
				commentView.Text = commentaryFormatted;
				SizeF suggestedSize = commentView.SuggestedFrameSize (new SizeF (this.scrollView.Frame.Width - 12, 10000.0f));
				commentView.Frame = new RectangleF (5, offset, this.scrollView.Frame.Width - 12, suggestedSize.Height);
				offset += suggestedSize.Height + 5;
				this.scrollView.AddSubview (commentView);
			}
			
			this.scrollView.ContentSize = new SizeF (this.scrollView.Frame.Width, offset);
			this.scrollView.BringSubviewToFront (this.resultSubview);
		}
		
		private void UpdateCurrentResult (ConsequenceResult result)
		{
			CurrentResult = result;
			
			this.calculatedAmount.Text = result.ComputedValue.ToString();
			this.caption.Text = result.FormattedCaption;
		}
		
		private UIViewController GetConfigurator (ConfigurableValue val)
		{
			// ToDo: Implement this with flyweight pattern
			
			IConfigControl control = null;
			
			switch (val.ValueType) {
			case ConfigurableValueType.Money:
				control = new MoneyControl (val);
				break;
			case ConfigurableValueType.Months:
				control = new MonthsControl (val);
				break;
			case ConfigurableValueType.Percentage:
				control = new PercentageControl (val);
				break;
			case ConfigurableValueType.Year:
				control = new YearControl (val);
				break;
			}
			
			if (control != null)
				control.ValueChanged += HandleControlValueChanged;
			
			return control as UIViewController;
		}

		void HandleControlValueChanged (object sender, ConfigurableValueChanged e)
		{
			ConsequenceResult oldResult = this.CurrentResult;
			ConsequenceResult newResult = this.CurrentResult.Calculator.Calculate (this.CurrentResult.Request);
			if (newResult != null)
				this.UpdateCurrentResult (newResult);
			
			// Bump the notificiation up so the main view controller can refresh the result, too
			ResultChanged (this, new ResultChangedArgs (oldResult, newResult));
		}
		
		public event EventHandler<ResultChangedArgs> ResultChanged = delegate{};
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
			if (UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.Portrait 
				|| UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.PortraitUpsideDown
				&& this.GridView != null)
				this.GridView = null;
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			UIImage detailBackground = UIImage.FromBundle (@"UIArt/detail_background.png");
			this.View.BackgroundColor = UIColor.FromPatternImage (detailBackground);
			
			UIImage resultBackground = UIImage.FromBundle (@"UIArt/detail_result_panel.png");
			this.resultSubview.BackgroundColor = UIColor.FromPatternImage (resultBackground);
		
			this.scrollView.Scrolled += delegate(object sender, EventArgs e) {
				// Make the subview with the results stay fixed while content scrolls underneath it
				this.resultSubview.Frame = new RectangleF (0, this.scrollView.ContentOffset.Y, 
				                                          this.resultSubview.Frame.Width, 
				                                          this.resultSubview.Frame.Height);
			};
			
			// We want to show an extra detail view, usually WebGridView for amortization tables, etc.
			NSNotificationCenter.DefaultCenter.AddObserver ("UIDeviceOrientationDidChangeNotification", DeviceRotated);
		}
		
		private void DeviceRotated (NSNotification notification)
		{
			if (UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.LandscapeLeft 
				|| UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.LandscapeRight
				&& CurrentResult.Table != null) {
				if (GridView == null)
					GridView = new WebGridView (Profile);
				GridView.SetResult (this.CurrentResult);
				this.PresentViewController (GridView, false, null);
			}
			
			if (UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.Portrait 
				|| UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.PortraitUpsideDown
				&& this.PresentedViewController == GridView) {
				this.DismissViewController (false, () => {});
			}
		}
		
		public override void ViewDidAppear (bool animated)
		{
			UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications ();
		}
		
		public override void ViewWillAppear (bool animated)
		{
			this.NavigationController.SetNavigationBarHidden (false, true);
			base.ViewWillAppear (animated);
		}
		
		public override void ViewWillDisappear (bool animated)
		{
			NavigationController.SetNavigationBarHidden (true, true);
			UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications ();
			base.ViewWillDisappear (animated);
		}
		
		private void ClearDynamicViews() {
			foreach (UIView v in scrollView.Subviews.ToArray())
				if (v.Tag == DYNAMIC_VIEW_TAG)
					v.RemoveFromSuperview ();
			
			if (CurrentConfiggers != null)
				foreach (UIViewController configger in CurrentConfiggers.Where(x => x != null))
					configger.View.RemoveFromSuperview ();
		}
		
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;
			ClearDynamicViews();
			
			ReleaseDesignerOutlets ();
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			if (toInterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown || toInterfaceOrientation == UIInterfaceOrientation.Portrait)
				return true;
			
			if ((toInterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || toInterfaceOrientation == UIInterfaceOrientation.LandscapeRight) && CurrentResult.Table != null)
				return true;
			
			return false;
		}
	}
	
	public class ResultChangedArgs : EventArgs
	{
		public ConsequenceResult OldResult { get; private set; }
		
		public ConsequenceResult NewResult { get; private set; }
		
		public ResultChangedArgs (ConsequenceResult oldResult, ConsequenceResult newResult)
		{
			this.OldResult = oldResult;
			this.NewResult = newResult;
		}
	}
}

