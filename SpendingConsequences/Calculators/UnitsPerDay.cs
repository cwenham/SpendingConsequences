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
			
			decimal perDay = request.InitialAmount;
			
			switch (request.TriggerMode) {
			case TriggerType.PerWeek:
				perDay = request.InitialAmount / 7;
				break;
			case TriggerType.PerMonth:
				perDay = request.InitialAmount / 30;
				break;
			case TriggerType.PerQuarter:
				perDay = request.InitialAmount / 90;
				break;
			case TriggerType.PerYear:
				perDay = request.InitialAmount / 365;
				break;
			default:
				break;
			}
			
			if (perDay / Cost > 1.0m)
				return new ConsequenceResult (this, perDay / Cost, FormatCaption (this.Caption, new Dictionary<string,string> {
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

