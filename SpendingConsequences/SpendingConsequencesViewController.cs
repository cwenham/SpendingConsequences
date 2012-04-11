using System;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public partial class SpendingConsequencesViewController : UIViewController
	{	
		// The space we live in, used to restore after scrolling up to accomodate an on-screen keyboard
		RectangleF _contentViewSize = RectangleF.Empty;
		
//		public SpendingConsequencesViewController () : base ("SpendingConsequencesViewController", null)
//		{
//		}
		
		public SpendingConsequencesViewController ()
		{
			NSBundle.MainBundle.LoadNib ("SpendingConsequencesViewController", this, null);
			this.ViewDidLoad();
		}
		
		public XDocument CalculatorDocument { get; private set; }
		
		public List<ACalculator> Calculators { get; private set; }
		
		public ConsequenceTableSource TableSource { get; private set; }
		
		public ConsequenceDetailController DetailController { get; private set; }
		
		public override UINavigationController NavigationController {
			get {
				return this.navMain;
			}
		}
		
		private UIView DecimalAccessoryView { get; set; }
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Add handlers to move the view whenever the keyboard appears or disappears
			NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillShowNotification, delegate(NSNotification n) {
				this.KeyboardOpenedOrClosed (n, "Open");
			});
			NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillHideNotification, delegate(NSNotification n) {
				this.KeyboardOpenedOrClosed (n, "Close");
			});
			
			// 
			this.InitialAmount.KeyboardType = UIKeyboardType.DecimalPad;
			DecimalAccessoryView = CreateDecimalPadAccessoryView (delegate(object sender, EventArgs e) {
				FinishedEditingInitialAmount ();
			});
			this.InitialAmount.InputAccessoryView = DecimalAccessoryView;
			
			// Load the XML file with the rules our app acts on
			if (CalculatorDocument == null) {
				CalculatorDocument = XDocument.Load ("ConsequenceCalculators.xml");
				var calculators = from e in CalculatorDocument.Root.Elements ()
					where ACalculator.CalcType (e) != null
						select ACalculator.GetInstance (e);
				if (calculators != null)
					this.Calculators = calculators.ToList ();
			}
			
			if (Calculators != null) {
				TableSource = new ConsequenceTableSource (Calculators, this);
				ConsequenceView.Source = TableSource;
			}
			
			this.InitialAmount.ShouldReturn = (textField) => {
				FinishedEditingInitialAmount ();
				return true;
			};
		}
		
		/// <summary>
		/// Create the Input Accessory View for a decimal pad that contains a "Done" button
		/// </summary>
		/// <returns>
		/// The pad accessory view.
		/// </returns>
		internal static UIView CreateDecimalPadAccessoryView (EventHandler doneButtonHandler)
		{
			UIView accView = new UIView (new RectangleF (10.0f, 0.0f, 310.0f, 30.0f));
			accView.BackgroundColor = UIColor.LightGray;
			
			UIButton btnDone = UIButton.FromType (UIButtonType.Custom);
			btnDone.Frame = new RectangleF (240.0f, 0.0f, 80.0f, 30.0f);
			btnDone.SetTitle ("Done", UIControlState.Normal);
			btnDone.BackgroundColor = UIColor.Blue;
			btnDone.TouchUpInside += doneButtonHandler;
			
			accView.AddSubview (btnDone);
			return accView;
		}
		
		private void FinishedEditingInitialAmount ()
		{
			UITextField textField = this.InitialAmount;
			textField.ResignFirstResponder ();

			if (TableSource != null && !String.IsNullOrWhiteSpace (textField.Text)) {
				TriggerType mode = TriggerType.OneTime;
				switch (this.SpendingMode.SelectedSegment) {
				case 0:
					mode = TriggerType.OneTime;
					break;
				case 1:
					mode = TriggerType.PerDay;
					break;
				case 2:
					mode = TriggerType.PerMonth;
					break;
				case 3:
					mode = TriggerType.PerYear;
					break;
				default:
					break;
				}
				TableSource.ComputeConsequences (new ConsequenceRequest (decimal.Parse (textField.Text), mode));
				this.ConsequenceView.ReloadData ();
			}
		}
		
		public void DisplayConsequenceDetails (ConsequenceResult result)
		{
			if (this.DetailController == null)
				DetailController = new ConsequenceDetailController ();
			
			DetailController.SetCurrentResult (result);
			
			if (this.NavigationController != null) {
				this.NavigationController.PushViewController (DetailController, true);
				
				// this.NavigationItem isn't working, so for now I'm going to force the back
				// button to what I want by changing it in the NavController's NavBar directly
				var BackButton = new UIBarButtonItem ("Back", UIBarButtonItemStyle.Bordered, null);
				this.NavigationController.NavigationBar.BackItem.BackBarButtonItem = BackButton;
			}
		}
		
		protected void KeyboardOpenedOrClosed (NSNotification n, string openOrClose)
		{
			if (openOrClose == "Open") {
				this._contentViewSize = this.View.Frame;
				RectangleF kbdFrame = UIKeyboard.BoundsFromNotification (n);
				double animationDuration = UIKeyboard.AnimationDurationFromNotification (n);

				RectangleF newFrame = this.View.Frame;

				newFrame.Y -= kbdFrame.Height;

				UIView.BeginAnimations ("ResizeForKeyboard");
				UIView.SetAnimationDuration (animationDuration);
				this.View.Frame = newFrame;
				UIView.CommitAnimations ();
			} else { 
				double animationDuration = UIKeyboard.AnimationDurationFromNotification (n);

				UIView.BeginAnimations ("ResizeForKeyboard");
				UIView.SetAnimationDuration (animationDuration);
				this.View.Frame = this._contentViewSize;
				UIView.CommitAnimations ();
			}
		}
		
		public List<ACalculator> TriggeredCalculators (decimal ammount, TriggerType mode)
		{
			return null;
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

