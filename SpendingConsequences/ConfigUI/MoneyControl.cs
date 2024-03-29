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
			this.ConfigValue.ValueChanged += HandleValueChanged;
		}
		
		public ConfigurableValue ConfigValue { get; private set; }

		private bool ExpectingChange = false;

		private void HandleValueChanged (object sender, EventArgs e)
		{
			if (ConfigValue.Value != null && ExpectingChange == false)
			{
				Money newAmount = ConfigValue.Value as Money;
				if (newAmount != null)
				{
					this.currencyLabel.Text = newAmount.CurrencySymbol();
					this.currencyButton.SetTitle(newAmount.CurrencyCode, UIControlState.Normal);
					this.configuredValue.Text = newAmount.Value.ToString("0.00");
				}
			}

			ExpectingChange = false;
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			Money editedAmount = ConfigValue.Value as Money;
			this.currencyLabel.Text = editedAmount.CurrencySymbol();
			this.currencyButton.SetTitle(editedAmount.CurrencyCode, UIControlState.Normal);

			UIImage segSelected = UIImage.FromBundle (@"UIArt/mode_sel.png").CreateResizableImage (new UIEdgeInsets (0, 9, 0, 9));
			UIImage segUnselected = UIImage.FromBundle (@"UIArt/mode_unsel.png").CreateResizableImage (new UIEdgeInsets (0, 9, 0, 9));
			
			currencyButton.SetBackgroundImage (segUnselected, UIControlState.Normal);
			currencyButton.SetBackgroundImage (segSelected, UIControlState.Highlighted);
			currencyButton.TouchUpInside += delegate {
				CurrencyButtonClicked(this, new CurrencyChangeEventArgs(this.ConfigValue));
			};

			
			this.configuredValue.KeyboardType = UIKeyboardType.DecimalPad;
			this.configuredValue.InputAccessoryView = SpendingConsequencesViewController.CreateDecimalPadAccessoryView ((sender, e) => {
				this.configuredValue.ResignFirstResponder ();

				decimal newVal = 0m;
				if (decimal.TryParse(this.configuredValue.Text, out newVal))
				{
					ExpectingChange = true;
					Money newAmount = Money.NewMoney(newVal, editedAmount.CurrencyCode);
					this.ConfigValue.Value = newAmount;
					ValueChanged(this, new ConfigurableValueChanged(this.ConfigValue));
				}
			});
			
			this.caption.Text = ConfigValue.Label;
			this.editCaption.Text = ConfigValue.Label;
			this.editCaption.ShouldReturn += (textField) => {
				textField.ResignFirstResponder();
				this.caption.Text = textField.Text;
				this.ConfigValue.Label = textField.Text;
				return true;
			};

			this.configuredValue.Text = editedAmount.Value.ToString("0.00");
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

