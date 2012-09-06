using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

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
			this.CurrencyCode = NSLocale.CurrentLocale.CurrencyCode;
		}

		public Money (decimal amount, string currencyCode)
		{
			this.Value = amount;
			this.CurrencyCode = currencyCode;
		}

		public Money Clone()
		{
			return new Money(this.Value, this.CurrencyCode);
		}
		
		public static implicit operator Money(decimal amount)
        {
            return new Money(amount);
        }

		public static Money NewMoney(decimal amount, string currencyCode)
		{
			return new Money(amount, currencyCode);
		}

		#region Operator overloads
		public static Money operator *(decimal multiplier, Money amount)
		{
			return new Money(multiplier * amount.Value, amount.CurrencyCode);
		}

		public static Money operator *(Money amount, decimal multiplier)
		{
			return new Money(amount.Value * multiplier, amount.CurrencyCode);
		}

		public static Money operator /(Money amount, int value)
		{
			return new Money(amount.Value / value, amount.CurrencyCode);
		}

		public static Money operator /(Money amount, decimal divisor)
		{
			return new Money(amount.Value / divisor, amount.CurrencyCode);
		}

		public static Money operator /(Money amount1, Money amount2)
		{
			if (amount1.CurrencyCode != amount2.CurrencyCode)
				throw new InvalidOperationException("Cannot divide amounts of different currency");

			return new Money(amount1.Value / amount2.Value, amount1.CurrencyCode);
		}

		public static Money operator +(Money amount1, Money amount2)
		{
			if (amount1.CurrencyCode != amount2.CurrencyCode)
				throw new InvalidOperationException("Cannot add amounts of different currency");

			return new Money(amount1.Value + amount2.Value, amount1.CurrencyCode);
		}

		public static Money operator -(Money amount1, Money amount2)
		{
			if (amount1.CurrencyCode != amount2.CurrencyCode)
				throw new InvalidOperationException("Cannot subtract amounts of different currency");

			return new Money(amount1.Value - amount2.Value, amount1.CurrencyCode);
		}

		public static bool operator <=(decimal value, Money amount)
		{
			if ((object)amount != null)
				return value <= amount.Value;
			else
				return false;
		}

		public static bool operator <=(Money amount, decimal value)
		{
			if ((object)amount != null)
				return amount.Value <= value;
			else
				return false;
		}

		public static bool operator <=(Money amount, int value)
		{
			if ((object)amount != null)
				return amount.Value <= value;
			else
				return false;
		}

		public static bool operator >=(decimal value, Money amount)
		{
			if ((object)amount != null)
				return value >= amount.Value;
			else
				return false;
		}

		public static bool operator >=(Money amount, int value)
		{
			if ((object)amount != null)
				return amount.Value >= value;
			else
				return false;
		}

		public static bool operator >=(Money amount, decimal value)
		{
			if ((object)amount != null)
				return amount.Value >= value;
			else
				return false;
		}

		public static bool operator >(decimal value, Money amount)
		{
			if ((object)amount != null)
				return value > amount.Value;
			else
				return false;
		}

		public static bool operator >(Money amount, decimal value)
		{
			if ((object)amount != null)
				return amount.Value > value;
			else return false;
		}

		public static bool operator >(int value, Money amount)
		{
			if ((object)amount != null)
				return value > amount.Value;
			else
				return false;
		}

		public static bool operator >(Money amount1, Money amount2)
		{
			if ((object)amount1 == null || (object)amount2 == null)
				return false;

			if (amount1.CurrencyCode != amount2.CurrencyCode)
				throw new InvalidOperationException("Cannot compare amounts of different currencies");

			return amount1.Value > amount2.Value;
		}

		public static bool operator <(decimal value, Money amount)
		{
			if ((object)amount != null)
				return value < amount.Value;
			else
				return false;
		}

		public static bool operator <(Money amount, decimal value)
		{
			if ((object)amount != null)
				return amount.Value < value;
			else
				return false;
		}

		public static bool operator <(int value, Money amount)
		{
			if ((object)amount != null)
				return value < amount.Value;
			else
				return false;
		}

		public static bool operator <(Money amount1, Money amount2)
		{
			if ((object)amount1 == null || (object)amount2 == null)
				return false;

			if (amount1.CurrencyCode != amount2.CurrencyCode)
				throw new InvalidOperationException("Cannot compare amounts of different currencies");

			return amount1.Value < amount2.Value;
		}

		public static bool operator ==(decimal value, Money amount)
		{
			if ((object)amount != null)
				return value == amount.Value;
			else
				return false;
		}

		public static bool operator ==(int value, Money amount)
		{
			if ((object)amount != null)
				return value == amount.Value;
			else
				return false;
		}

		public static bool operator ==(Money amount, int value)
		{
			if ((object)amount != null)
				return amount.Value == value;
			else
				return false;
		}

		public static bool operator ==(Money amount1, Money amount2)
		{
			if ((object)amount1 != null)
				return amount1.Equals(amount2);
			else
				return (object)amount2 == null;
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;

			if (obj is Money)
				return ((Money)obj).Value == this.Value && ((Money)obj).CurrencyCode == this.CurrencyCode;

			return base.Equals (obj);
		}

		public static bool operator !=(int value, Money amount)
		{
			if ((object)amount != null)
				return value != amount.Value;
			else
				return true;
		}

		public static bool operator !=(decimal value, Money amount)
		{
			if ((object)amount != null)
				return value != amount.Value;
			else
				return true;
		}

		public static bool operator !=(Money amount, int value)
		{
			if ((object)amount != null)
				return amount.Value != value;
			else return true;
		}

		public static bool operator !=(Money amount1, Money amount2)
		{
			if ((object)amount1 == null && (object)amount2 == null)
				return false;
			if ((object)amount1 == null || (object)amount2 == null)
				return true;

			return !amount1.Equals(amount2);
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
		#endregion

		public decimal Value { get; private set; }

		public string CurrencyCode { get; private set; }

		public override string ToString ()
		{
			NSNumberFormatter formatter = new NSNumberFormatter ();
			formatter.NumberStyle = NSNumberFormatterStyle.Decimal;
			formatter.Locale = NSLocale.CurrentLocale;
			formatter.MaximumFractionDigits = 2;

			string currencySymbol = ExchangeRates.SymbolForCurrencyCode(this.CurrencyCode);
			if (string.IsNullOrWhiteSpace(currencySymbol))
				currencySymbol = this.CurrencyCode;
			string formattedNumber = formatter.StringFromNumber (new NSNumber((double)(this.Value)));

			return String.Format("{0}{1}", currencySymbol, formattedNumber);
		}

		public string CurrencySymbol ()
		{
			return ExchangeRates.SymbolForCurrencyCode(this.CurrencyCode);
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

