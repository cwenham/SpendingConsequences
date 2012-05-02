using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

// ToDo: Consolidate this with MonthsOfService and a configuration option to chose the unit of time in the result

namespace SpendingConsequences.Calculators
{
	public class YearsOfService : ACalculator
	{
		public YearsOfService (XElement definition) : base(definition)
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
			
			if (request.TriggerMode != TriggerType.OneTime)
				return null;
			
			decimal yearsService = request.InitialAmount / this.Cost;
			if (yearsService >= LowerResultLimit && yearsService <= UpperResultLimit)
				return new ConsequenceResult (this, 
				                              request, 
				                              yearsService, 
				                              this.FormatCaption (this.Caption, new Dictionary<string,string> {
					{"Years", yearsService.ToString ()},
					{"Cost", this.Cost.ToString ()}
				}),
				                              this.ImageName);
			else
				return null;			
		}
		
		public override string ResultFormat {
			get {
				return "{0:0.0}";
			}
		}
		#endregion
	}
}