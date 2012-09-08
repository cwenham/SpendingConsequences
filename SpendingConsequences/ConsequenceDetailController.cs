using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
//using MonoTouch.TestFlight;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public partial class ConsequenceDetailController : UIViewController
	{	
		private const int DYNAMIC_VIEW_TAG = 1024;
		
		public ConsequenceDetailController (AppProfile profile)
		{
			NSBundle.MainBundle.LoadNib ("ConsequenceDetailController", this, null);
			this.Profile = profile;
			this.ViewDidLoad();
			LastLabelYOffset = this.caption.Frame.Y + this.caption.Frame.Height;
		}
		
		private AppProfile Profile { get; set; }
		
		// Y + Height of the last label in our XIB, below which we can add dynamic controls
		private float LastLabelYOffset { get; set; }
		
		public ConsequenceResult CurrentResult { get; private set; }
		
		private UIViewController[] CurrentConfiggers { get; set; }
		
		/// <summary>
		/// A view used to display tabular results, such as amortization tables, when the device is rotated to landscape
		/// </summary>
		private XsltWebView GridView { get; set; }
		
		public void SetCurrentResult (ConsequenceResult result)
		{
			if (result == null)
				return;
			
#if DEBUG
			//TestFlight.PassCheckpoint ("VIEW_DETAILS");
#endif

			UpdateCurrentResult (result);
			
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

				CommentView = new CoreTextView ();
				CommentView.Tag = DYNAMIC_VIEW_TAG;
				CommentView.Text = commentaryFormatted;
				SizeF suggestedSize = CommentView.SuggestedFrameSize (new SizeF (this.scrollView.Frame.Width - 12, 10000.0f));
				CommentView.Frame = new RectangleF (5, offset, this.scrollView.Frame.Width - 12, suggestedSize.Height);
				offset += suggestedSize.Height + 5;
				this.scrollView.AddSubview (CommentView);
			}
			
			this.scrollView.ContentSize = new SizeF (this.scrollView.Frame.Width, offset);
			this.scrollView.BringSubviewToFront (this.resultSubview);
		}

		private CoreTextView CommentView { get; set; }
		
		private void UpdateCurrentResult (ConsequenceResult result)
		{
			try {
				CurrentResult = result;
			
				this.NavigationItem.Title = result.Request.Summary;

				this.calculatedAmount.Text = result.ComputedValue.ToString ();

				this.caption.Text = result.FormattedCaption;
				this.editCaption.Text = result.Calculator.Caption;

				UIImage image = ArtRepository.GetImage (result.Image);

				if (image != null && image != iconView.Image)
					iconView.Image = image;

				if (result.Calculator.IsUserEditable)
					this.NavigationItem.SetRightBarButtonItem(EditButton,false);
				else
					if (this.NavigationItem.RightBarButtonItem != null)
						this.NavigationItem.SetRightBarButtonItem(null,false);
			} catch (Exception ex) {
				Console.WriteLine (string.Format ("{0} thrown when updating current result: {1}", ex.GetType ().Name, ex.Message));
			}
		}
		
		private UIViewController GetConfigurator (ConfigurableValue val)
		{			
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
			case ConfigurableValueType.Currency:
				control = new CurrencyControl (val);
				break;
			}
			
			if (control != null) {
				control.ValueChanged += HandleControlValueChanged;
				control.CurrencyButtonClicked += HandleCurrencyButtonClicked;
			}
			
			return control as UIViewController;
		}

		void HandleCurrencyButtonClicked (object sender, CurrencyChangeEventArgs e)
		{
			CurrencySheet = CreateCurrencyActionSheet("Choose Currency");
			CurrencyPickerController.CurrencyChosen += delegate(object sndr, CurrencyChangedEventArgs ce) {
				if (e.ConfigValue.Value is Money)
					e.ConfigValue.Value = ExchangeRates.CurrentRates.ConvertToGiven(e.ConfigValue.Value as Money, ce.CurrencyCode);
				else e.ConfigValue.Value = ce.CurrencyCode;
				CurrencySheet.DismissWithClickedButtonIndex(0,true);
				CurrencySheet = null;
				CurrencyPickerController = null;
			};
			CurrencyPickerController.Cancelled += delegate {
				CurrencySheet.DismissWithClickedButtonIndex(0,true);
				CurrencySheet = null;
				CurrencyPickerController = null;
			};

			CurrencySheet.ShowInView(this.View);
			CurrencyPickerController.View.Frame = new RectangleF(0,0,320,327);
			CurrencySheet.Bounds = new RectangleF(0,0,320,634);
			if (e.ConfigValue.ValueType == ConfigurableValueType.Money)
				CurrencyPickerController.SetCurrency(((Money)e.ConfigValue.Value).CurrencyCode);
			else if (e.ConfigValue.ValueType == ConfigurableValueType.Currency)
				CurrencyPickerController.SetCurrency(e.ConfigValue.Value as string);
		}

		private UIActionSheet CurrencySheet { get; set; }

		private CurrencyPickerSheet CurrencyPickerController { get; set; }

		public UIActionSheet CreateCurrencyActionSheet(string title) {
			UIActionSheet sheet = new UIActionSheet(title);
			sheet.Style = UIActionSheetStyle.BlackTranslucent;

			CurrencyPickerController = new CurrencyPickerSheet();
			sheet.AddSubview(CurrencyPickerController.View);

			return sheet;
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
			if (this.GridView != null && this.PresentedViewController == this)
				this.GridView = null;	
		}

		private Boolean IsEditing { 
			get {
				return _isEditing;
			}
			set {
				if (value == _isEditing)
					return;

				if (value == true)
				{
					BeginEditing();
				}
				else
				{
					EndEditing();
				}
				_isEditing = value;
			}
		}
		private Boolean _isEditing = false;

		private void BeginEditing()
		{
			EditButton.Title = "Done";

			caption.Hidden = true;
			editCaption.Hidden = false;

			foreach (IConfigControl configger in CurrentConfiggers.Where(x => x != null))
				configger.BeginEditing();
		}

		private void EndEditing()
		{
			EditButton.Title = "Edit";

			caption.Hidden = false;
			editCaption.Hidden = true;

			foreach (IConfigControl configger in CurrentConfiggers.Where(x => x != null))
				configger.EndEditing();

			if (editCaption != null)
				editCaption.ResignFirstResponder();
		}

		private UIBarButtonItem EditButton { get; set; }
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			ArtRepository.StyleView("detail", this.View);
			ArtRepository.StyleView("detail_result", this.resultSubview);

			EditButton = new UIBarButtonItem("Edit", UIBarButtonItemStyle.Bordered, delegate (object sender, EventArgs e) {
				if (IsEditing)
					IsEditing = false;
				else
					IsEditing = true;
			});

			this.editCaption.ShouldReturn += (textField) => {
				textField.ResignFirstResponder();
				this.caption.Text = textField.Text;
				if (CurrentResult != null)
					CurrentResult.Calculator.Caption = textField.Text;
				return true;
			};
		
			this.scrollView.Scrolled += delegate(object sender, EventArgs e) {
				// Make the subview with the results stay fixed while content scrolls underneath it
				this.resultSubview.Frame = new RectangleF (0, this.scrollView.ContentOffset.Y, 
				                                          this.resultSubview.Frame.Width, 
				                                          this.resultSubview.Frame.Height);
			};


			// We want to show an extra detail view, usually XsltWebView for amortization tables, etc.
			NSNotificationCenter.DefaultCenter.AddObserver (UIDevice.OrientationDidChangeNotification, DeviceRotated);
		}
		
		private void DeviceRotated (NSNotification notification)
		{
			if ((UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.LandscapeLeft 
				|| UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.LandscapeRight)
			    && !IsEditing
				&& CurrentResult.Table != null
				&& (this.PresentedViewController == null || this.PresentedViewController == this)) {
					if (GridView == null)
						GridView = new XsltWebView (Profile);
					GridView.SetResult (this.CurrentResult);
					this.PresentViewController (GridView, false, null);
			}
			
			if ((UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.Portrait 
				|| UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.PortraitUpsideDown)
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
			IsEditing = false;

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
			CommentView = null;
			
			ReleaseDesignerOutlets ();
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			if (toInterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown || toInterfaceOrientation == UIInterfaceOrientation.Portrait)
				return true;
			
			if ((toInterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || toInterfaceOrientation == UIInterfaceOrientation.LandscapeRight) && CurrentResult.Table != null && !IsEditing)
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

