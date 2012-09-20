using System;
using System.Drawing;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public partial class CurrencyPickerSheet : UIViewController
	{
		public CurrencyPickerSheet () : base ("CurrencyPickerSheet", null)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		private CurrencyPickerModel Model { get; set; }

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			ArtRepository.StyleView("result", this.View);
			ArtRepository.StyleButton("embossed", this.useButton);
			ArtRepository.StyleButton("embossed", this.cancelButton);

			Model = new CurrencyPickerModel();
			Model.CurrencyChanged += delegate(object sender, CurrencyChangedEventArgs e) {
				this.CurrencyChanged(this, e);
			};
			this.currencyPicker.Model = Model;

			this.useButton.TouchUpInside += delegate(object sender, EventArgs e) {
				this.CurrencyChosen(this, new CurrencyChangedEventArgs(Model.SelectedCurrency));
			};

			this.cancelButton.TouchUpInside += delegate(object sender, EventArgs e) {
				this.Cancelled(this, e);
			};
		}

		public void SetCurrency(string currencyCode)
		{
			int index = ExchangeRates.CurrentRates.SupportedCurrencies
				.Select((code, ix) => new { Currency = code, Pos = ix})
				.Where(x => x.Currency.Equals(currencyCode, StringComparison.OrdinalIgnoreCase))
				.Select(x => x.Pos).FirstOrDefault();
			this.currencyPicker.Select(index, 0, true);
		}

		public event EventHandler<CurrencyChangedEventArgs> CurrencyChanged = delegate{};

		public event EventHandler<CurrencyChangedEventArgs> CurrencyChosen = delegate{};

		public event EventHandler<EventArgs> Cancelled = delegate{};
	}
}

