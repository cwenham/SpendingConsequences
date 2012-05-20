using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace SpendingConsequences.Calculators
{
	public class UnitsPerDay : ACalculator
	{
		public UnitsPerDay (XElement definition) : base(definition)
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
			if (Cost == 0m)
				return null;
			
			if (request.TriggerMode == TriggerType.OneTime)
				return null;
			
			double perDay = ((double)request.InitialAmount) / ConsequenceRequest.DayCounts [request.TriggerMode];
			
			decimal units = ((decimal)perDay / Cost);
			
			if (units >= LowerResultLimit && units <= UpperResultLimit)
				return new ConsequenceResult (this, 
				                              request, 
				                              new Units(units), 
				                              FormatCaption (this.Caption, new Dictionary<string,string> {
					{"Cost", this.Cost.ToString ()},
					{"Units", units.ToString ()}
					}),
				                              this.ImageName);
			else
				return null;
		}
		#endregion
	}
}

