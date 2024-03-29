using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace SpendingConsequences.Calculators
{
	/// <summary>
	/// Returns the number of units that can be purchased for One-Time amounts
	/// </summary>
	public class UnitsForPrice : ACalculator
	{
		public UnitsForPrice (XElement definition) : base(definition)
		{
		}
		
		public Money Cost {
			get {
				if (ConfigurableValues.ContainsKey ("Cost"))
					return ((Money)ConfigurableValues ["Cost"].Value);
				else
					return 0;
			}
		}

		#region implemented abstract members of SpendingConsequences.Calculators.ACalculator
		public override ConsequenceResult Calculate (ConsequenceRequest request)
		{
			if (request.TriggerMode != TriggerType.OneTime || request.InitialAmount < LowerThreshold || request.InitialAmount > UpperThreshold)
				return null;

			try {
				Money localizedCost = ExchangeRates.CurrentRates.ConvertToGiven(this.Cost, request.InitialAmount.CurrencyCode);

				decimal units = (request.InitialAmount / localizedCost).Value;
				
				return new ConsequenceResult (this,
				                             request,
				                             new Units (units),
				                             FormatCaption (Caption, new Dictionary<string,string> {
					{"Cost", Cost.ToString()}
				}
				), this.Image,
				   (units >= LowerResultLimit && units <= UpperResultLimit)
				);				
			} catch (Exception ex) {
				Console.WriteLine ("{0} thrown when calculating units for price: {1}", ex.GetType().Name, ex.Message);
				return new ConsequenceResult (this,
				                              request,
				                              null,
				                              "Oops, something went wrong in this calculator",
				                              this.Image,
				                              false);
			}

		}
		#endregion
	}
}

