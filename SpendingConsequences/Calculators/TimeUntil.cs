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
			
			TimeSpan timeUntil;
			
			switch (request.TriggerMode) {
			case TriggerType.PerDay:
				timeUntil = new TimeSpan (((int)Math.Ceiling (givenUnitsUntil)), 0, 0, 0);
				break;
			case TriggerType.PerWeek:
				timeUntil = new TimeSpan (((int)Math.Ceiling (givenUnitsUntil * 7)), 0, 0, 0);
				break;
			case TriggerType.PerMonth:
				timeUntil = new TimeSpan (((int)Math.Ceiling (givenUnitsUntil * 30)), 0, 0, 0);
				break;
			case TriggerType.PerQuarter:
				timeUntil = new TimeSpan (((int)Math.Ceiling (givenUnitsUntil * 90)), 0, 0, 0);
				break;
			case TriggerType.PerYear:
				timeUntil = new TimeSpan (((int)Math.Ceiling (givenUnitsUntil * 365.25)), 0, 0, 1);
				break;
			default:
				break;
			}
			
			if (timeUntil.TotalDays > 1 && timeUntil.TotalDays < (365 * 30)) {
				string unit = null;
				double val = 0;
				
				// Return a value that's easier for a human to judge quickly
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
				
				return new ConsequenceResult (this, (decimal)val, this.FormatCaption (this.Caption, new Dictionary<string,string> {
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

