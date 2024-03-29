using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using MonoTouch;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.ObjCRuntime;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public class CurrencyPickerModel : UIPickerViewModel
	{
		public CurrencyPickerModel ()
		{
		}

		public override int GetComponentCount (UIPickerView picker)
		{
			return 1;
		}

		public override int GetRowsInComponent (UIPickerView picker, int component)
		{
			return ExchangeRates.CurrentRates.SupportedCurrencies.Length;
		}

		public override float GetComponentWidth (UIPickerView picker, int component)
		{
			return 300f;
		}

		public override string GetTitle (UIPickerView picker, int row, int component)
		{
			string currencyCode = ExchangeRates.CurrentRates.SupportedCurrencies [row];
			string currencyName = ExchangeRates.GetCurrencyName(currencyCode);
			if (currencyName == null)
				currencyName = "";

			return string.Format("{0} - {1}",
			                     currencyCode, currencyName);
		}

		public override void Selected (UIPickerView picker, int row, int component)
		{
			CurrencyChanged(this, new CurrencyChangedEventArgs(ExchangeRates.CurrentRates.SupportedCurrencies[row]));

			SelectedCurrency = ExchangeRates.CurrentRates.SupportedCurrencies[row];
		}

		public string SelectedCurrency { get; private set; }

		public event EventHandler<CurrencyChangedEventArgs> CurrencyChanged = delegate{};
	}

	public class CurrencyChangedEventArgs : EventArgs
	{
		public string CurrencyCode { get; private set; }

		public CurrencyChangedEventArgs(string currencyCode)
		{
			this.CurrencyCode = currencyCode;
		}
	}

	public static class NSLocaleExtensions {
		/// <summary>
		/// Mapping for displayNameForKey:value
		/// </summary>
		/// <remarks>As of writing, MonoTouch doesn't yet map this function. The following code was taken from
		/// http://stackoverflow.com/questions/7577535/iphone-obtaining-a-list-of-countries-from-monotouch</remarks>
		public static string DisplayNameForKeyValue(this NSLocale locale, string key, string value)
		{
			IntPtr handle = locale.Handle;
			IntPtr selDisplayNameForKeyValue = new Selector ("displayNameForKey:value:").Handle;
			NSString nsKey = new NSString(key);
			NSString nsValue = new NSString(value);
			return NSString.FromHandle (MonoTouch.ObjCRuntime.Messaging.IntPtr_objc_msgSend_IntPtr_IntPtr (handle, selDisplayNameForKeyValue, nsKey.Handle, nsValue.Handle));
		}
	}
}

