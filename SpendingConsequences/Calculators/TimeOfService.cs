using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace SpendingConsequences.Calculators
{
	public class TimeOfService : ACalculator
	{
		public TimeOfService (XElement definition) : base(definition)
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
		
		public TimeUnit UnitForCost {
			get {
				if (!_unitWasSet) {
					if (Definition.Attribute ("UnitForCost") != null)
						Enum.TryParse (Definition.Attribute ("UnitForCost").Value, true, out _unitForCost);
					else
						_unitForCost = TimeUnit.Day;
					
					_unitWasSet = true;
				}			
				
				return _unitForCost;
			}
		}
		private TimeUnit _unitForCost = TimeUnit.Day;
		private bool _unitWasSet = false;
		
		public static Dictionary<TimeUnit, double> DaysPerUnit = new Dictionary<TimeUnit, double> {
			{TimeUnit.Millisecond, 0.0000000116},
			{TimeUnit.Second, 0.0000115741},
			{TimeUnit.Minute, 0.0006944444},
			{TimeUnit.Hour, 0.0416666667},
			{TimeUnit.Day, 1},
			{TimeUnit.Week, 7},
			{TimeUnit.Month, 30.4375},
			{TimeUnit.Quarter, 4.0583333333},
			{TimeUnit.Year, 365.25},
			{TimeUnit.Century, 36525},
			{TimeUnit.Millenium, 365250}
		};
		
		private static double MaxDays = TimeSpan.MaxValue.TotalDays;
		
		#region implemented abstract members of SpendingConsequences.Calculators.ACalculator
		public override ConsequenceResult Calculate (ConsequenceRequest request)
		{
			if (Cost == 0m)
				return null;
			
			if (request.TriggerMode != TriggerType.OneTime)
				return null;
			
			double serviceUnits = (double)(request.InitialAmount / this.Cost);
			double serviceDays = DaysPerUnit[UnitForCost] * serviceUnits;
			
			if (serviceDays <= MaxDays && serviceUnits >= (double)LowerResultLimit && serviceUnits <= (double)UpperResultLimit)
				return new ConsequenceResult (this, 
				                              request, 
				                              new Time(new TimeSpan((int)(Math.Floor(serviceDays)), 0, 0, 0)),
				                              this.FormatCaption (this.Caption, new Dictionary<string,string> {
					{"Unit", this.UnitForCost.ToString ()},
					{"Cost", this.Cost.ToString ()}
				}),
				                              this.ImageName);
			else
				return null;			
		}
		#endregion
	}
	
	public enum TimeUnit {
		Millisecond,
		Second,
		Minute,
		Hour,
		Day,
		Week,
		Month,
		Quarter,
		Year,
		Decade,
		Century,
		Millenium
	}
}