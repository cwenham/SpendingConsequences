using System;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public partial class SpendingConsequencesViewController : UIViewController
	{	
		// Maximum dollar value that will fit in input field without being truncated
		private const decimal MAX_AMOUNT = 99999999999999.99m;
		
		// The space we live in, used to restore after scrolling up to accomodate an on-screen keyboard
		RectangleF _contentViewSize = RectangleF.Empty;
		
		public SpendingConsequencesViewController (Dictionary<string,Profile> profiles)
		{
			NSBundle.MainBundle.LoadNib ("SpendingConsequencesViewController", this, null);
			this.Profiles = profiles;
			this.ViewDidLoad ();
		}
		
		public Dictionary<string,Profile> Profiles { get; private set; }
		
		public ConsequenceTableSource TableSource { get; private set; }
		
		public ConsequenceDetailController DetailController { get; private set; }
		
		private UIView DecimalAccessoryView { get; set; }
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
			ConfigurableValue.CloseResources ();
			
			if (DetailController != null)
				if (this.NavigationController.PresentedViewController == this) {
					DetailController.ResultChanged -= HandleResultChanged;
					DetailController = null;	
				}
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			SetUICosmetics ();
			
			this.mul2.TouchUpInside += Handle_Mul2_TouchUpInside;
			this.div2.TouchUpInside += Handle_Div2_TouchUpInside;
			
			// Add handlers to move the view whenever the keyboard appears or disappears
			NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillShowNotification, delegate(NSNotification n) {
				this.KeyboardOpenedOrClosed (n, "Open");
			}
			);
			NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillHideNotification, delegate(NSNotification n) {
				this.KeyboardOpenedOrClosed (n, "Close");
			}
			);
			
			// 
			this.InitialAmount.KeyboardType = UIKeyboardType.DecimalPad;
			DecimalAccessoryView = CreateDecimalPadAccessoryView (delegate(object sender, EventArgs e) {
				FinishedEditingInitialAmount ();
			}
			);
			this.InitialAmount.InputAccessoryView = DecimalAccessoryView;
			
			if (Profiles != null) {
				TableSource = new ConsequenceTableSource (Profiles, this);
				ConsequenceView.Source = TableSource;
				TableSource.ResultsReady += delegate {
					this.ConsequenceView.ReloadData ();	
				};
			}
			
			this.InitialAmount.ShouldReturn = (textField) => {
				FinishedEditingInitialAmount ();
				return true;
			};
			
			this.SpendingMode.ValueChanged += (sender, e) => {
				if (!IsEditingAmount)
					RefreshCalculators ();
			};

			this.NavigationItem.Title = "Back";
			this.currencySymbol.Text = Money.LocalCurrencySymbol ();
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			// Set focus to entry box so the keyboard comes up immediately, making it faster and more obvious
			// where to begin.
			if (!viewShownOnce) {
				this.InitialAmount.BecomeFirstResponder ();
				viewShownOnce = true;
			}
		}

		private bool viewShownOnce = false;
		
		/// <summary>
		/// Fits views and controls with custom backgrounds and bitmaps
		/// </summary>
		private void SetUICosmetics ()
		{
			UIImage backgroundImage = UIImage.FromBundle (@"UIArt/input_panel.png");
			this.panelView.BackgroundColor = UIColor.FromPatternImage (backgroundImage);
			
			UIImage segSelected = UIImage.FromBundle (@"UIArt/mode_sel.png").CreateResizableImage (new UIEdgeInsets (0, 9, 0, 9));
			UIImage segUnselected = UIImage.FromBundle (@"UIArt/mode_unsel.png").CreateResizableImage (new UIEdgeInsets (0,	9, 0, 9));
			UIImage segSelUnsel = UIImage.FromBundle (@"UIArt/mode_sel_unsel.png");
			UIImage segUnselSel = UIImage.FromBundle (@"UIArt/mode_unsel_sel.png");
			UIImage segUnselUnsel = UIImage.FromBundle (@"UIArt/mode_unsel_unsel.png");
			
			this.SpendingMode.SetBackgroundImage (segUnselected, UIControlState.Normal, UIBarMetrics.Default);
			this.SpendingMode.SetBackgroundImage (segSelected, UIControlState.Selected, UIBarMetrics.Default);
			this.SpendingMode.SetDividerImage (segUnselUnsel, UIControlState.Normal, UIControlState.Normal,	UIBarMetrics.Default);
			this.SpendingMode.SetDividerImage (segSelUnsel,
				UIControlState.Selected,
				UIControlState.Normal,
				UIBarMetrics.Default
			);
			this.SpendingMode.SetDividerImage (
				segUnselSel,
				UIControlState.Normal,
				UIControlState.Selected,
				UIBarMetrics.Default
			);
			
			UIImage mul2unsel = UIImage.FromBundle (@"UIArt/mul2_unsel.png");
			UIImage div2unsel = UIImage.FromBundle (@"UIArt/div2_unsel.png");
			
			this.mul2.SetBackgroundImage (mul2unsel, UIControlState.Normal);
			this.div2.SetBackgroundImage (div2unsel, UIControlState.Normal);
			
			UIImage resultBackground = UIImage.FromBundle (@"UIArt/result_panel.png");
			this.View.BackgroundColor = UIColor.FromPatternImage (resultBackground);
		}

		void Handle_Div2_TouchUpInside (object sender, EventArgs e)
		{
			decimal current = CurrentAmount;
			if (current > 0.01m)
				CurrentAmount = current / 2m;
			else if (current == 0m)
				CurrentAmount = 1.00m; // Default to a simple value so the user can enter small amounts quickly
			else
				return;
			
			TableSource.ComputeConsequences (new ConsequenceRequest (CurrentAmount, CurrentMode));
		}

		void Handle_Mul2_TouchUpInside (object sender, EventArgs e)
		{
			decimal current = CurrentAmount;
			if (current > 0m)
			if (current < MAX_AMOUNT / 2)
				CurrentAmount = current * 2m;
			else
				return;
			else
				CurrentAmount = 1.00m;
			
			TableSource.ComputeConsequences (new ConsequenceRequest (CurrentAmount, CurrentMode));		
		}
		
		private decimal CurrentAmount {
			get {
				decimal amount;
				if (decimal.TryParse (this.InitialAmount.Text, out amount))
					return amount;
				else 
					return 0;
			}
			set {
				this.InitialAmount.Text = string.Format("{0:0.00}", value);
			}
		}
		
		private TriggerType CurrentMode {
			get {
				switch (this.SpendingMode.SelectedSegment) {
				case 0:
					return TriggerType.OneTime;
				case 1:
					return TriggerType.PerDay;
				case 2:
					return TriggerType.PerWeek;
				case 3:
					return TriggerType.PerMonth;
				case 4:
					return TriggerType.PerYear;
				default:
					return TriggerType.OneTime;
				}
			}
		}
		
		/// <summary>
		/// Create the Input Accessory View for a decimal pad that contains a "Done" button
		/// </summary>
		/// <returns>
		/// The pad accessory view.
		/// </returns>
		/// <remarks>This is static internal so it can be used on some ConfigUI elements.
		/// It should be moved to a more neutral class later, though.</remarks>
		internal static UIView CreateDecimalPadAccessoryView (EventHandler doneButtonHandler)
		{
			//ToDo: Move me to a neutral class so it's easier to reuse me
			
			UIView accView = new UIView (new RectangleF (10.0f, 0.0f, 310.0f, 30.0f));
			accView.BackgroundColor = UIColor.FromRGB (197, 202, 188);
			
			UIButton btnDone = UIButton.FromType (UIButtonType.Custom);
			btnDone.Frame = new RectangleF (240.0f, 0.0f, 80.0f, 30.0f);
			btnDone.SetTitleColor (UIColor.Black, UIControlState.Normal);
			btnDone.SetTitle ("Done", UIControlState.Normal);

			UIImage segSelected = UIImage.FromBundle (@"UIArt/mode_sel.png").CreateResizableImage (new UIEdgeInsets (0, 9, 0, 9));
			UIImage segUnselected = UIImage.FromBundle (@"UIArt/mode_unsel.png").CreateResizableImage (new UIEdgeInsets (0, 9, 0, 9));
			
			btnDone.SetBackgroundImage (segUnselected, UIControlState.Normal);
			btnDone.SetBackgroundImage (segSelected, UIControlState.Highlighted);
			
			btnDone.TouchUpInside += doneButtonHandler;
			
			accView.AddSubview (btnDone);
			return accView;
		}
		
		private void FinishedEditingInitialAmount ()
		{
			UITextField textField = this.InitialAmount;
			textField.ResignFirstResponder ();

			RefreshCalculators();
		}
		
		private void RefreshCalculators ()
		{
			if (TableSource != null && CurrentAmount > 0m)
				TableSource.ComputeConsequences (new ConsequenceRequest (CurrentAmount, CurrentMode));
		}
		
		public void DisplayConsequenceDetails (ConsequenceResult result)
		{
			if (this.DetailController == null) {
				DetailController = new ConsequenceDetailController (Profiles);
				DetailController.ResultChanged += HandleResultChanged;
			}
			
			DetailController.SetCurrentResult (result);
			
			if (this.NavigationController != null)
				this.NavigationController.PushViewController (DetailController, true);
		}

		void HandleResultChanged (object sender, ResultChangedArgs e)
		{
			int index = this.TableSource.ReplaceResult (e.OldResult, e.NewResult);
			if (index > -1) {
				NSIndexPath path = NSIndexPath.FromRowSection (index, 0);
				this.ConsequenceView.ReloadRows (new NSIndexPath[] {path}, UITableViewRowAnimation.None);
			}			
		}
		
		private bool IsEditingAmount = false;
		
		protected void KeyboardOpenedOrClosed (NSNotification n, string openOrClose)
		{
			if (openOrClose == "Open") {
				IsEditingAmount = true;
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
				IsEditingAmount = false;
				double animationDuration = UIKeyboard.AnimationDurationFromNotification (n);

				UIView.BeginAnimations ("ResizeForKeyboard");
				UIView.SetAnimationDuration (animationDuration);
				this.View.Frame = this._contentViewSize;
				UIView.CommitAnimations ();
			}
		}
		
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;
			
			ConfigurableValue.CloseResources ();
			ReleaseDesignerOutlets ();
		}

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return (toInterfaceOrientation == UIInterfaceOrientation.Portrait || toInterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown);
		}
	}
}

