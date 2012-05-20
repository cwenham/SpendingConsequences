using System;

using MonoTouch.Foundation;

namespace SpendingConsequences.Calculators
{
	/// <summary>
	/// A wrapper type for representing currency
	/// </summary>
	/// <remarks>We use this in ConsequenceResults rather than just a plain decimal in order to make it clear that it's money rather than a quantity or
	/// some other value. It also overrides the ToString method to return a localized version.
	/// <para>NOTE: this is not meant to handle automatic currency conversions or representations. The ToString() method will always output in the current
	/// locale's currency.</para></remarks>
	public class Money
	{		
		public Money (decimal amount)
		{
			this.Value = amount;
		}
		
		public static implicit operator Money(decimal amount)
        {
            return new Money(amount);
        }
		
		public decimal Value { get; private set; }
		
		public override string ToString ()
		{
			NSNumberFormatter formatter = new NSNumberFormatter ();
			formatter.NumberStyle = NSNumberFormatterStyle.Currency;
			formatter.Locale = NSLocale.CurrentLocale;
			return formatter.StringFromNumber (new NSNumber((double)(this.Value)));
		}
		
		#region Static helper methods
		/// <summary>
		/// Returns the currency symbol for the Current Locale
		/// </summary>
		public static string LocalCurrencySymbol ()
		{
			NSNumberFormatter formatter = new NSNumberFormatter ();
			formatter.NumberStyle = NSNumberFormatterStyle.Currency;
			formatter.Locale = NSLocale.CurrentLocale;
			return formatter.CurrencySymbol;
		}
		#endregion
	}
}

