using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public partial class MonthsControl : UIViewController, IConfigControl
	{
		public MonthsControl (ConfigurableValue val) : base ("MonthsControl", null)
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
			
			this.caption.Text = ConfigValue.Label;
			this.configuredValue.Text = ConfigValue.Value.ToString ();
			this.stepper.Value = ((double)ConfigValue.Value);
			this.stepper.MinimumValue = 1d;
			this.stepper.MaximumValue = 24d;
			this.stepper.StepValue = 1d;
			this.stepper.ValueChanged += delegate {
				this.ConfigValue.Value = stepper.Value;
				this.configuredValue.Text = stepper.Value.ToString ();
				ValueChanged (this, new ConfigurableValueChanged (this.ConfigValue));
			};
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

