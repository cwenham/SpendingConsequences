using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace SpendingConsequences.Calculators
{
	public class TimeUntil : ACalculator
	{
		public TimeUntil (XElement definition) : base(definition)
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
			
			if (this.Cost < request.InitialAmount)
				return null;
			
			double givenUnitsUntil = (double)Cost / ((double)request.InitialAmount);
			
			TimeSpan timeUntil = new TimeSpan (((int)Math.Ceiling (givenUnitsUntil * ConsequenceRequest.DayCounts [request.TriggerMode])), 0, 0, 0);
			
			if (timeUntil.TotalDays >= (double)LowerResultLimit && timeUntil.TotalDays <= (double)UpperResultLimit) {
				string unit = null;
				double val = 0;
				
				// Return a value that's easier for a human to judge quickly. Ie: "48 months" doesn't always translate
				// to "4 years" quickly and meaningfully when browsing a list of many results.
				if (timeUntil.TotalDays < 7) {
					unit = "days";
					val = timeUntil.TotalDays;
				} else if (timeUntil.TotalDays <= 49) {
					unit = "weeks";
					val = timeUntil.TotalDays / 7;
				} else if (timeUntil.TotalDays <= 540) {
					unit = "months";
					val = timeUntil.TotalDays / 30;
				} else {
					unit = "years";
					val = timeUntil.TotalDays / 365.25;
				}
				
				return new ConsequenceResult (this, 
				                              request,
				                              (decimal)val, 
				                              this.FormatCaption (this.Caption, new Dictionary<string,string> {
					{"Unit", unit},
					{"Cost", this.Cost.ToString ()}
				}), this.ImageName);
			}
			
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

