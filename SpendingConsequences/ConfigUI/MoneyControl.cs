using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public partial class MoneyControl : UIViewController
	{
		public MoneyControl (ConfigurableValue val) : base ("MoneyControl", null)
		{
			this.ConfigValue = val;
		}
		
		public ConfigurableValue ConfigValue { get; private set; }
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			this.configuredValue.KeyboardType = UIKeyboardType.DecimalPad;
			this.configuredValue.InputAccessoryView = SpendingConsequencesViewController.CreateDecimalPadAccessoryView ((sender, e) => {
				this.configuredValue.ResignFirstResponder ();
				this.ConfigValue.Value = decimal.Parse(this.configuredValue.Text);
			});
			
			this.caption.Text = ConfigValue.Label;
			this.configuredValue.Text = ConfigValue.Value.ToString ();
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

