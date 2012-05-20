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
		
		public decimal Cost {
			get {
				if (ConfigurableValues.ContainsKey ("Cost"))
					return ((decimal)ConfigurableValues ["Cost"].Value);
				else
					return 0;
			}
		}

		#region implemented abstract members of SpendingConsequences.Calculators.ACalculator
		public override ConsequenceResult Calculate (ConsequenceRequest request)
		{
			if (request.TriggerMode != TriggerType.OneTime || request.InitialAmount < LowerThreshold || request.InitialAmount > UpperThreshold)
				return null;
			
			decimal units = request.InitialAmount / Cost;
			
			if (units >= LowerResultLimit && units <= UpperResultLimit)
				return new ConsequenceResult (this,
				                             request,
				                             new Units(units),
				                             FormatCaption (Caption, new Dictionary<string,string> {
					{"Cost", Cost.ToString()}
				}
				), this.ImageName
				);
			else
				return null;
		}
		#endregion
	}
}

