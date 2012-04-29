using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace SpendingConsequences.Calculators
{
	public class UnitsPerYear : ACalculator
	{
		public UnitsPerYear (XElement definition) : base(definition)
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
		
		private static TimeSpan oneYear = new TimeSpan(365,0,0,0);

		#region implemented abstract members of SpendingConsequences.Calculators.ACalculator
		public override ConsequenceResult Calculate (ConsequenceRequest request)
		{
			if (Cost == 0m)
				return null;
			
			if (request.TriggerMode == TriggerType.OneTime)
				return null;
			
			decimal perYear = request.AmountAfter (oneYear);
			decimal units = perYear / Cost;			
			
			if (units >= LowerResultLimit && units <= UpperResultLimit)
				return new ConsequenceResult (this, units, FormatCaption (this.Caption, new Dictionary<string,string> {
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

