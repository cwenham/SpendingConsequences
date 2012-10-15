using System;
using System.Xml.Linq;
using System.Collections.Generic;

using ETFLib;

using SpendingConsequences;

namespace SpendingConsequences.Calculators
{
	public class CurrencyConversion : ACalculator
	{
		public CurrencyConversion (XElement definition) : base(definition)
		{
		}

		public string CurrencyCode {
			get {
				if (ConfigurableValues.ContainsKey ("CurrencyCode"))
					return (ConfigurableValues ["CurrencyCode"].Value as String);
				else
					return "USD";
			}
		}

		/// <summary>
		/// Return a flag appropriate for the CurrencyCode
		/// </summary>
		public override SpendingConsequences.Calculators.Image Image {
			get {
				if (_image == null || _image.Name != this.CurrencyCode)
				{
					XElement imgElement = new XElement(NS.Composition + "Image",
				                                   new XAttribute("Type", "CurrencyFlag"),
				                                   new XAttribute("Name", this.CurrencyCode));
					_image = new Image(imgElement);
				}

				return _image;
			}
		}
		private Image _image;

		#region implemented abstract members of SpendingConsequences.Calculators.ACalculator
		public override ConsequenceResult Calculate (ConsequenceRequest request)
		{
			if (request.TriggerMode != TriggerType.OneTime)
				return null;

			Money convertedAmount = ExchangeRates.CurrentRates.ConvertToGiven(request.InitialAmount, this.CurrencyCode);

			return new ConsequenceResult(this,
			                             request,
			                             convertedAmount,
			                             this.FormatCaption (this.Caption, new Dictionary<string,string> {
														{"CurrencyCode", this.CurrencyCode},
				                                        {"CurrencyName", ExchangeRates.GetCurrencyName(this.CurrencyCode)}
													}),
			                             this.Image,
			                             true);
		}
		#endregion

	}
}

