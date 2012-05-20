using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace SpendingConsequences.Calculators
{
	public class UnitsPerPeriod : ACalculator
	{
		public UnitsPerPeriod (XElement definition) : base(definition)
		{
		}
		
		/// <summary>
		/// Cost per unit
		/// </summary>
		public decimal Cost {
			get {
				if (ConfigurableValues.ContainsKey ("Cost"))
					return ((decimal)ConfigurableValues ["Cost"].Value);
				else
					return 0;
			}
		}
		
		/// <summary>
		/// Period of time the units will be consumed within. Eg: day for coffees-per-day
		/// </summary>
		public TimeUnit Period {
			get {
				if (!_periodWasSet) {
					if (Definition.Attribute ("Period") != null)
						Enum.TryParse (Definition.Attribute ("Period").Value, true, out _period);
					else
						_period = TimeUnit.Day;
					
					_periodWasSet = true;
				}			
				
				return _period;
			}
		}
		private TimeUnit _period = TimeUnit.Day;
		private bool _periodWasSet = false;

		#region implemented abstract members of SpendingConsequences.Calculators.ACalculator
		public override ConsequenceResult Calculate (ConsequenceRequest request)
		{
			if (Cost == 0m)
				return null;
			
			if (request.TriggerMode == TriggerType.OneTime)
				return null;
			
			double perDay = ((double)request.InitialAmount) / ConsequenceRequest.DayCounts [request.TriggerMode];
			double unitsPerDay = perDay / (double)Cost;
			double unitsPerPeriod = unitsPerDay * ConsequenceRequest.DaysPerUnit[Period];
			
			if (unitsPerPeriod >= (double)LowerResultLimit && unitsPerPeriod <= (double)UpperResultLimit)
				return new ConsequenceResult (this, 
				                              request, 
				                              new Units (unitsPerPeriod), 
				                              FormatCaption (this.Caption, new Dictionary<string,string> {
					{"Cost", this.Cost.ToString ()}
				}
				),this.ImageName);
			else
				return null;
		}
		#endregion
	}
}

