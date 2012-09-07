using System;
using System.Drawing;
using System.Diagnostics;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public partial class PercentageControl : UIViewController, IConfigControl
	{
		public PercentageControl (ConfigurableValue val) : base ("PercentageControl", null)
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
			this.editCaption.Text = ConfigValue.Label;
			this.editCaption.ShouldReturn += (textField) => {
				textField.ResignFirstResponder();
				this.caption.Text = textField.Text;
				this.ConfigValue.Label = textField.Text;
				return true;
			};

			this.configuredValue.Text = String.Format ("{0:0.00}", ConfigValue.Value);
			this.stepper.Value = Convert.ToDouble(ConfigValue.Value);
			this.stepper.MinimumValue = Convert.ToDouble(ConfigValue.MinValue);
			this.stepper.MaximumValue = Convert.ToDouble(ConfigValue.MaxValue);
			this.stepper.StepValue = Convert.ToDouble(ConfigValue.StepValue);
			valueChangeTimer = new Stopwatch ();
			valueChangeTimer.Start ();
			this.stepper.ValueChanged += delegate {
				valueChangeTimer.Stop ();
				this.configuredValue.Text = String.Format ("{0:0.00}", stepper.Value);
				
				if (valueChangeTimer.ElapsedMilliseconds < 1000)
					averageValueChange = ((long)Math.Floor ((double)(averageValueChange + valueChangeTimer.ElapsedMilliseconds) / 2));

				// Go faster if the user has held their finger down for a while
				if (averageValueChange <= 100)
					this.stepper.StepValue = (double)ConfigValue.StepValue * 10;
				
				valueChangeTimer.Reset ();
				valueChangeTimer.Start ();
			};
			this.stepper.TouchUpInside += delegate {
				this.ConfigValue.Value = Convert.ToDecimal(stepper.Value);
				ValueChanged (this, new ConfigurableValueChanged (this.ConfigValue));	
				this.stepper.StepValue = Convert.ToDouble(ConfigValue.StepValue);
			};
		}
		
		private long averageValueChange = 1000;
		private Stopwatch valueChangeTimer { get; set; }
		
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

		public event EventHandler<CurrencyChangeEventArgs> CurrencyButtonClicked = delegate {};

		public event EventHandler<ConfigurationChangeEventArgs> ConfigurationChanged = delegate{};

		void IConfigControl.BeginEditing ()
		{
			this.caption.Hidden = true;
			this.editCaption.Hidden = false;
		}

		void IConfigControl.EndEditing ()
		{
			this.caption.Hidden = false;
			this.editCaption.Hidden = true;
			this.ConfigValue.Label = this.editCaption.Text;
			ConfigurationChanged(this, new ConfigurationChangeEventArgs(this.ConfigValue));
		}
		#endregion
	}
}

