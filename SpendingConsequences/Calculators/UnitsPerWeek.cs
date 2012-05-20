using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace SpendingConsequences.Calculators
{
	public class UnitsPerWeek : ACalculator
	{
		public UnitsPerWeek (XElement definition) : base(definition)
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
			
			decimal perWeek = request.InitialAmount;
			
			switch (request.TriggerMode) {
			case TriggerType.PerDay:
				perWeek = request.InitialAmount * 7;
				break;
			case TriggerType.PerWeek:
				perWeek = request.InitialAmount;
				break;
			case TriggerType.PerMonth:
				perWeek = request.InitialAmount / 4;
				break;
			case TriggerType.PerQuarter:
				perWeek = request.InitialAmount / 13;
				break;
			case TriggerType.PerYear:
				perWeek = request.InitialAmount / 52;
				break;
			default:
				break;
			}
			
			if (perWeek / Cost >= LowerResultLimit && perWeek / Cost <= UpperResultLimit)
				return new ConsequenceResult (this, 
				                              request,
				                              new Units(perWeek / Cost), 
				                              FormatCaption (this.Caption, new Dictionary<string,string> {
						{"Cost", this.Cost.ToString ()}
					}),
				                              this.ImageName);
			else
				return null;
		}
		#endregion
	}
}

