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

		public Money Cost {
			get {
				if (ConfigurableValues.ContainsKey ("Cost"))
					return ((Money)ConfigurableValues ["Cost"].Value);
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

		public decimal UnitsPerDay {
			get {
				if (ConfigurableValues.ContainsKey ("UnitsPerDay"))
					return ((decimal)ConfigurableValues ["UnitsPerDay"].Value);
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

			try {
				decimal serviceUnits = (request.InitialAmount / this.Cost).Value;
				decimal serviceSeconds = ConsequenceRequest.SecondsPerUnit [UnitForCost] * serviceUnits;

				if (UnitsPerDay > 0)
				{
					// If the service is not provided 24 hours/day, then spread the time over whole days.
					// This is mainly used to calculate how long it would take to earn $n, given an 8-hour day
					decimal maxSecondsPerDay = UnitsPerDay * ConsequenceRequest.SecondsPerUnit[UnitForCost];
					decimal wholeDays = Math.Floor(serviceSeconds / maxSecondsPerDay);
					decimal wholeDaysInSeconds = wholeDays * ConsequenceRequest.SecondsPerUnit[TimeUnit.Day];
					serviceSeconds = wholeDaysInSeconds + (serviceSeconds - (wholeDays * maxSecondsPerDay));
				}

				if (serviceSeconds > (decimal)TimeSpan.MaxValue.TotalSeconds)
					return new ConsequenceResult (this,
					                             request,
					                             new OverflowMessage (),
					                             "Try reducing the spending amount",
					                             this.ImageName,
					                             false);
				else
					return new ConsequenceResult (this, 
					                              request, 
					                              new Time (TimeSpan.FromSeconds((double)serviceSeconds)),
					                              this.FormatCaption (this.Caption, new Dictionary<string,string> {
														{"Unit", this.UnitForCost.ToString ()},
														{"Cost", this.Cost.ToString ()}
													}),
					                              this.ImageName,
					                              (serviceSeconds <= (decimal)TimeSpan.MaxValue.TotalSeconds && serviceUnits >= LowerResultLimit && serviceUnits <= UpperResultLimit));				
			} catch (Exception ex) {
				Console.WriteLine ("{0} thrown when calculating Time Of Service: {1}", ex.GetType().Name, ex.Message);
				return new ConsequenceResult (this,
				                              request,
				                              null,
				                              "Oops, something went wrong in this calculator",
				                              this.ImageName,
				                              false);
			}

		
		}
		#endregion
	}
}