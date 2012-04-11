using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace SpendingConsequences.Calculators
{
	public class MonthsOfService : ACalculator
	{
		public MonthsOfService (XElement definition) : base(definition)
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
			
			decimal monthsService = request.InitialAmount / this.Cost;
			if (monthsService > 0m)
				return new ConsequenceResult (this, monthsService, this.FormatCaption (this.Caption, new Dictionary<string,string> {
					{"Months", monthsService.ToString ()},
					{"Cost", this.Cost.ToString ()}
				}),
				                              this.ImageName);
			else
				return null;			
		}
		
		public override string ResultFormat {
			get {
				return "{0:0}";
			}
		}
		#endregion
	}
}

