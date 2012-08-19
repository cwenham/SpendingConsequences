using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.IO;

using MonoTouch;
using MonoTouch.Foundation;

namespace SpendingConsequences.Calculators
{
	/// <summary>
	/// Store and provide conversions between world currencies
	/// </summary>
	/// <remarks>Because we expect to deal with amounts less than millions of dollars, we'll convert between currencies
	/// by recording all rates relative to the US Dollar by default.</remarks>
	public class ExchangeRates
	{
		private const string BASE_CURRENCY_CODE = "USD";

		public ExchangeRates ()
		{
			Rates = new Dictionary<string, decimal>();
			UpdateTimes = new Dictionary<string, DateTime>();
			BaseCurrency = BASE_CURRENCY_CODE;

			// If the local currency is the same as the base, then add the identity rate
			if (NSLocale.CurrentLocale.CurrencyCode == BaseCurrency)
				SetRate(BaseCurrency, 1.0m, DateTime.Now);
		}

		public ExchangeRates (string baseCurrencyCode)
		{
			Rates = new Dictionary<string, decimal>();
			UpdateTimes = new Dictionary<string, DateTime>();
			BaseCurrency = baseCurrencyCode;
		}

		/// <summary>
		/// Instantiate based on the data in openexchangerates.org's latest.json
		/// </summary>
		public static ExchangeRates FromOXLatestJson (JsonValue latest)
		{
			string baseCurrency = latest["base"];
			ExchangeRates newRates = new ExchangeRates(baseCurrency);

			double timeStamp = latest["timestamp"];
			DateTime updateDate = Financials.UnixTimeStampToDateTime(timeStamp);
			JsonObject rates = latest["rates"] as JsonObject;

			if (rates != null)
				foreach (string key in rates.Keys)
					newRates.SetRate(key, rates[key], updateDate);

			return newRates;
		}

		public static ExchangeRates FromOXLatestJson (string latestFile)
		{
			Stream cachedRates = File.OpenRead (latestFile);
			JsonValue latest = JsonObject.Load (cachedRates);
			cachedRates.Close (); 
			return ExchangeRates.FromOXLatestJson(latest);	
		}

		private Dictionary<string, decimal> Rates { get; set; }

		private Dictionary<string, DateTime> UpdateTimes { get; set; }

		public string BaseCurrency { get; private set; }

		public string[] SupportedCurrencies { get {
				if (_supportedCurrencies == null)
					_supportedCurrencies = Rates.Keys.ToArray<string>();

				return _supportedCurrencies;
			}
		}
		private string[] _supportedCurrencies;

		public static ExchangeRates CurrentRates { 
			get {
				if (_currentRates == null)
				{
					_currentRates = new ExchangeRates();
				}

				return _currentRates;
			}
			set {
				_currentRates = value;
			}
		}
		private static ExchangeRates _currentRates;

		public DateTime OldestQuote {
			get {
				if (UpdateTimes != null)
					return UpdateTimes.Values.OrderBy(x => x).First();
				else
					return DateTime.MinValue;
			}
		}

		/// <summary>
		/// Sets an individual exchange rate
		/// </summary>
		/// <param name='currency'>The "to" currency</para>
		/// <param name='quote'>
		/// How much of the "to" currency can be bought with 1.00 of the base currency.
		/// </param>
		public void SetRate (string currencyCode, decimal quote, DateTime asOf)
		{
			if (Rates.ContainsKey (currencyCode))
				Rates [currencyCode] = quote;
			else {
				Rates.Add (currencyCode, quote);
				_supportedCurrencies = null;
			}

			if (UpdateTimes.ContainsKey(currencyCode))
				UpdateTimes[currencyCode] = asOf;
			else
				UpdateTimes.Add(currencyCode, asOf);
		}

		public Money ConvertToBase (Money amount)
		{
			if (amount.CurrencyCode == BaseCurrency)
				return amount;

			if (Rates.ContainsKey(amount.CurrencyCode))
				return Money.NewMoney(amount.Value * (1 / Rates[amount.CurrencyCode]), BaseCurrency);

			// Can't convert to base? We'll return the original amount, the upstream code will need to handle exception
			return amount;
		}

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

		public Money ConvertToGiven (Money amount, string currencyCode)
		{
			if (amount.CurrencyCode.Equals(currencyCode, StringComparison.OrdinalIgnoreCase))
				return amount;

			if (!Rates.ContainsKey (currencyCode))
				return amount;

			Money asBase = ConvertToBase(amount);
			return new Money (asBase.Value * Rates[currencyCode], currencyCode);
		}
	}
}

