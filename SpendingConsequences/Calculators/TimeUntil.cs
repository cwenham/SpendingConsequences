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
			
			double givenUnitsUntil = (double)Cost / ((double)request.InitialAmount.Value);
			
			TimeSpan timeUntil = new TimeSpan (((int)Math.Ceiling (givenUnitsUntil * request.DaysPerPeriod)), 0, 0, 0);
							
			return new ConsequenceResult (this, 
			                              request,
			                              new Time (timeUntil), 
			                              this.FormatCaption (this.Caption, new Dictionary<string,string> {
				{"Cost", this.Cost.ToString ()}
			}
			), this.ImageName,
			   (timeUntil.TotalDays >= (double)LowerResultLimit && timeUntil.TotalDays <= (double)UpperResultLimit));
		}
		#endregion
	}
}

