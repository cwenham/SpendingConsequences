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
		
		private static double MaxDays = TimeSpan.MaxValue.TotalDays;
		
		#region implemented abstract members of SpendingConsequences.Calculators.ACalculator
		public override ConsequenceResult Calculate (ConsequenceRequest request)
		{
			if (Cost == 0m)
				return null;
			
			if (request.TriggerMode != TriggerType.OneTime)
				return null;
			
			double serviceUnits = (double)(request.InitialAmount / this.Cost);
			double serviceDays = ConsequenceRequest.DaysPerUnit [UnitForCost] * serviceUnits;
			
			return new ConsequenceResult (this, 
			                              request, 
			                              new Time (new TimeSpan ((int)(Math.Floor (serviceDays)), 0, 0, 0)),
			                              this.FormatCaption (this.Caption, new Dictionary<string,string> {
				{"Unit", this.UnitForCost.ToString ()},
				{"Cost", this.Cost.ToString ()}
			}
			),
			                              this.ImageName,
			                              (serviceDays <= MaxDays && serviceUnits >= (double)LowerResultLimit && serviceUnits <= (double)UpperResultLimit));
		
		}
		#endregion
	}
}