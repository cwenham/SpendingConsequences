using System;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using ETFLib;
using ETFLib.Composition;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public partial class SpendingConsequencesViewController : UIViewController
	{	
		// Maximum dollar value that will fit in input field without being truncated
		private const decimal MAX_AMOUNT = 99999999999999.99m;
		
		// The space we live in, used to restore after scrolling up to accomodate an on-screen keyboard
		RectangleF _contentViewSize = RectangleF.Empty;
		
		public SpendingConsequencesViewController (AppProfile profile)
		{
			NSBundle.MainBundle.LoadNib ("SpendingConsequencesViewController", this, null);
			this.Profile = profile;
			this.ViewDidLoad ();
		}
		
		public AppProfile Profile { get; private set; }
		
		public ConsequenceTableSource TableSource { get; private set; }
		
		public ConsequenceDetailController DetailController { get; private set; }

		public TemplatePickerSheet TemplateSheetController { get; private set; }
		
		private UIView DecimalAccessoryView { get; set; }

		private UIBarButtonItem EditButton { get; set; }
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
			
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

			UIKeyboard.Notifications.ObserveWillShow ((sender, args) => {
				IsEditingAmount = true;
				EditButton.Enabled = false;
				this._contentViewSize = this.View.Frame;
				
				RectangleF newFrame = this.View.Frame;
				
				newFrame.Y -= args.FrameBegin.Height;
				
				UIView.BeginAnimations ("ResizeForKeyboard");
				UIView.SetAnimationDuration (args.AnimationDuration);
				this.View.Frame = newFrame;
				UIView.CommitAnimations ();
			});

			UIKeyboard.Notifications.ObserveWillHide ((sender, args) => {
				IsEditingAmount = false;
				EditButton.Enabled = true;
				
				UIView.BeginAnimations ("ResizeForKeyboard");
				UIView.SetAnimationDuration (args.AnimationDuration);
				this.View.Frame = this._contentViewSize;
				UIView.CommitAnimations ();
			});

			this.InitialAmount.KeyboardType = UIKeyboardType.DecimalPad;
			DecimalAccessoryView = CreateDecimalPadAccessoryView (delegate(object sender, EventArgs e) {
				FinishedEditingInitialAmount ();
			}
			);
			this.InitialAmount.InputAccessoryView = DecimalAccessoryView;
			
			if (Profile != null) {
				TableSource = new ConsequenceTableSource (Profile, this);
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

			this.NavigationItem.Title = "Results";

			EditButton = new UIBarButtonItem("Edit", UIBarButtonItemStyle.Bordered, (sender, e) => {
				UIBarButtonItem button = sender as UIBarButtonItem;
				if (this.ConsequenceView.Editing)
				{
					this.ConsequenceView.Editing = false;
					button.Title = "Edit";
				}
				else
				{
					this.ConsequenceView.Editing = true;
					button.Title = "Done";
				}
			});
			this.NavigationItem.SetRightBarButtonItem(EditButton, false);

			this.currencySymbol.Text = Money.LocalCurrencySymbol ();
		}

		private UIActionSheet TemplateActionSheet { get; set; }

		private ConsequenceFooterViewController FooterController { get; set; }

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			// Set focus to entry box so the keyboard comes up immediately, making it faster and more obvious
			// where to begin.
			if (!viewShownOnce) {
				this.InitialAmount.BecomeFirstResponder ();
				viewShownOnce = true;
			}

			if (FooterController == null)
			{
				FooterController = new ConsequenceFooterViewController();
				FooterController.AddConsequenceRequested += delegate(object sender, EventArgs e) {
					if (TemplateSheetController == null)
					{
						TemplateSheetController = new TemplatePickerSheet(this.Profile);
						TemplateSheetController.Cancelled += delegate {
							TemplateActionSheet.DismissWithClickedButtonIndex(0,true);
						};
						TemplateSheetController.TemplateChosen += delegate(object _sender, TemplateChosenEventArgs _e) {
							TemplateActionSheet.DismissWithClickedButtonIndex(0,true);
							CreateNewConsequence(this.CurrentMode, _e.Template);
						};
					}
					if (TemplateActionSheet == null)
					{
						TemplateActionSheet = new UIActionSheet("Add A Calculator");
						TemplateActionSheet.AddSubview(TemplateSheetController.View);
					}
					TemplateActionSheet.ShowInView(this.View);
					TemplateSheetController.View.Frame = new RectangleF(0,0,320,600);
					TemplateActionSheet.Bounds = new RectangleF(0,0,320,600);
				};
			}
			this.ConsequenceView.TableFooterView = FooterController.View;
		}

		private bool viewShownOnce = false;
		
		/// <summary>
		/// Fits views and controls with custom backgrounds and bitmaps
		/// </summary>
		private void SetUICosmetics ()
		{
			ArtRepository.StyleView("input", this.panelView);
			ArtRepository.StyleView("result", this.View);

			ArtRepository.StyleSegmentedControl("mode", this.SpendingMode);
		}

		private void CreateNewConsequence(TriggerType mode, Template template)
		{
			if (template == null)
				return; 

			SubProfile userProfile = Profile.GetSubProfile("user");
			if (userProfile == null)
			{
				userProfile = SubProfile.Create("user");
				Profile.AddSubProfile("user", userProfile);
			}

			XElement unwrappedTemplate = template.GetUsableTemplateDefinition();
			if (unwrappedTemplate != null)
			{
				ACalculator calculator = userProfile.AddConsequenceFromDefinition(unwrappedTemplate);
				calculator.SortOrder = -1;

				// Switch to a compatible mode
				if (!calculator.TriggersOn.Contains(this.CurrentMode))
					if (calculator.TriggersOn.Contains(TriggerType.Repeating))
						this.CurrentMode = TriggerType.PerDay;
				else
					if (calculator.TriggersOn.Contains(TriggerType.OneTime))
						this.CurrentMode = TriggerType.OneTime;

				// Create a prototype request in order to load the Details View in edit mode
				Money prototypeAmount = this.CurrentAmount;
				if (prototypeAmount == null)
					prototypeAmount = 100;
				ConsequenceRequest prototypeRequest = new ConsequenceRequest(prototypeAmount, this.CurrentMode);
				ConsequenceResult prototypeResult = calculator.Calculate(prototypeRequest);
				DisplayConsequenceDetails(prototypeResult);
			}
		}
		
		private Money CurrentAmount {
			get {
				decimal amount;
				if (decimal.TryParse (this.InitialAmount.Text, out amount))
					return Money.NewMoney(amount,NSLocale.CurrentLocale.CurrencyCode);
				else 
					return null;
			}
			set {
				this.InitialAmount.Text = string.Format("{0:0.00}", value.Value);
			}
		}

		private Money CurrentAmountInBaseCurrency {
			get {
				return ExchangeRates.CurrentRates.ConvertToBase(CurrentAmount);
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
			set
			{
				switch (value) {
				case TriggerType.OneTime:
					this.SpendingMode.SelectedSegment = 0;
					break;
				case TriggerType.PerDay:
					this.SpendingMode.SelectedSegment = 1;
					break;
				case TriggerType.PerWeek:
					this.SpendingMode.SelectedSegment = 2;
					break;
				case TriggerType.PerMonth:
					this.SpendingMode.SelectedSegment = 3;
					break;
				case TriggerType.PerYear:
					this.SpendingMode.SelectedSegment = 4;
					break;
				case TriggerType.Repeating:
					this.SpendingMode.SelectedSegment = 1;
					break;
				default:
				break;
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

			ArtRepository.StyleButton("mode", btnDone);
			
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
			if (TableSource != null && CurrentAmount != null && CurrentAmount > 0)
				TableSource.ComputeConsequences (new ConsequenceRequest (CurrentAmount, CurrentMode));
		}
		
		public void DisplayConsequenceDetails (ConsequenceResult result)
		{
			if (this.DetailController == null) {
				DetailController = new ConsequenceDetailController (Profile);
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

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations ()
		{
			return UIInterfaceOrientationMask.Portrait | UIInterfaceOrientationMask.PortraitUpsideDown;
		}
	}
}

