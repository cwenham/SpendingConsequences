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
			this.editCaption.Text = ConfigValue.Label;
			this.editCaption.ShouldReturn += (textField) => {
				textField.ResignFirstResponder();
				this.caption.Text = textField.Text;
				this.ConfigValue.Label = textField.Text;
				return true;
			};

			this.configuredValue.Text = ConfigValue.Value.ToString ();
			this.stepper.Value = Convert.ToDouble (ConfigValue.Value);
			this.stepper.MinimumValue = Convert.ToDouble(ConfigValue.MinValue);
			this.stepper.MaximumValue = Convert.ToDouble(ConfigValue.MaxValue);
			this.stepper.StepValue = 1;
			this.stepper.ValueChanged += delegate {
				this.configuredValue.Text = stepper.Value.ToString ();
			};
			this.stepper.TouchUpInside += delegate {
				this.ConfigValue.Value = Convert.ToInt32 (stepper.Value);
				ValueChanged (this, new ConfigurableValueChanged (this.ConfigValue));
			};
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
			this.editCaption.ResignFirstResponder();
			this.ConfigValue.Label = this.editCaption.Text;
			ConfigurationChanged(this, new ConfigurationChangeEventArgs(this.ConfigValue));
		}
		#endregion
	}
}

