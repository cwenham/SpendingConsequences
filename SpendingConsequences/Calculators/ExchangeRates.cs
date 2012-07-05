using System;
using System.Collections.Generic;

using MonoTouch;
using MonoTouch.Foundation;

namespace SpendingConsequences.Calculators
{
	/// <summary>
	/// Store and provide conversions between world currencies
	/// </summary>
	/// <remarks>Has the concept of a base currency that's used for any conversions. That is: rather than store all
	/// exchange rates between arbitrary currencies, we only store the exchange rate to the base currency and perform
	/// conversions by first converting from the origin currency to the base, then the base currency to the destination.
	/// Unless otherwise specified the base currency--as per Bretton Woods convention--is assumed to be the U.S. Dollar.</remarks>
	public class ExchangeRates
	{
		private const string BASE_CURRENCY_CODE = "USD";

		public ExchangeRates ()
		{
			Rates = new Dictionary<string, decimal>();
			BaseCurrency = BASE_CURRENCY_CODE;

			// If the local currency is the same as the base, then add the identity rate
			if (NSLocale.CurrentLocale.CurrencyCode == BaseCurrency)
				Rates.Add(BaseCurrency, 1.0m);
		}

		public ExchangeRates (string baseCurrencyCode)
		{
			Rates = new Dictionary<string, decimal>();
			BaseCurrency = baseCurrencyCode;
		}

		private Dictionary<string, decimal> Rates { get; set; }

		public string BaseCurrency { get; private set; }

		public static ExchangeRates CurrentRates { 
			get {
				if (_currentRates == null)
				{
					_currentRates = new ExchangeRates();
				}

				return _currentRates;
			}
		}
		private static ExchangeRates _currentRates;

		public Money ConvertToLocal (Money amount)
		{
			if (amount.CurrencyCode == NSLocale.CurrentLocale.CurrencyCode)
				return amount;

			if (Rates.ContainsKey(NSLocale.CurrentLocale.CurrencyCode))
				return Money.NewMoney(amount.Value * Rates[NSLocale.CurrentLocale.CurrencyCode],
				                      NSLocale.CurrentLocale.CurrencyCode);

			// In this app, we're going to fail-back to the current amount rather than raise an exception
			return amount;
		}
	}
}

