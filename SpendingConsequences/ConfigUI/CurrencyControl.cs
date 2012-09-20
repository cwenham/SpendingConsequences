
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public partial class CurrencyControl : UIViewController, IConfigControl
	{
		public CurrencyControl (ConfigurableValue val) : base ("CurrencyControl", null)
		{
			this.ConfigValue = val;
			this.ConfigValue.ValueChanged += HandleValueChanged;
		}

		void HandleValueChanged (object sender, EventArgs e)
		{
			this.currencyButton.SetTitle(ConfigValue.Value.ToString(), UIControlState.Normal);
			this.currencyName.Text = ExchangeRates.GetCurrencyName(ConfigValue.Value.ToString());
			ValueChanged(this, new ConfigurableValueChanged(this.ConfigValue));
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

			this.currencyName.Text = ExchangeRates.GetCurrencyName(ConfigValue.Value.ToString());

			this.currencyButton.SetTitle(ConfigValue.Value.ToString(), UIControlState.Normal);

			ArtRepository.StyleButton("mode", currencyButton);

			currencyButton.TouchUpInside += delegate {
				CurrencyButtonClicked(this, new CurrencyChangeEventArgs(this.ConfigValue));
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

