using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public partial class MoneyControl : UIViewController, IConfigControl
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
			
			this.currencyLabel.Text = Money.LocalCurrencySymbol ();
			
			this.configuredValue.KeyboardType = UIKeyboardType.DecimalPad;
			this.configuredValue.InputAccessoryView = SpendingConsequencesViewController.CreateDecimalPadAccessoryView ((sender, e) => {
				this.configuredValue.ResignFirstResponder ();

				decimal newVal = 0m;
				if (decimal.TryParse(this.configuredValue.Text, out newVal))
				{
					this.ConfigValue.Value = newVal;
					ValueChanged(this, new ConfigurableValueChanged(this.ConfigValue));
				}
			});
			
			this.caption.Text = ConfigValue.Label;
			this.configuredValue.Text = ((Money)ConfigValue.Value).Value.ToString();
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

		#region IConfigControl implementation
		public event EventHandler<ConfigurableValueChanged> ValueChanged = delegate {};
		#endregion
	}
}

