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
		
		public Money Cost {
			get {
				if (ConfigurableValues.ContainsKey ("Cost"))
					return ((Money)ConfigurableValues ["Cost"].Value);
				else
					return 0;
			}
		}

		private decimal MaxDays = (decimal)TimeSpan.MaxValue.TotalDays;

		#region implemented abstract members of SpendingConsequences.Calculators.ACalculator
		public override ConsequenceResult Calculate (ConsequenceRequest request)
		{
			if (Cost == null || Cost == 0m)
				return null;
			
			if (request.TriggerMode == TriggerType.OneTime)
				return null;
			
			if (this.Cost < request.InitialAmount)
				return null;

			try {
				Money localizedCost = ExchangeRates.CurrentRates.ConvertToGiven(this.Cost, request.InitialAmount.CurrencyCode);

				decimal givenUnitsUntil = (localizedCost / request.InitialAmount).Value;

				if (givenUnitsUntil * request.DaysPerPeriod < MaxDays)
				{
					TimeSpan timeUntil = TimeSpan.FromDays((double)(givenUnitsUntil * request.DaysPerPeriod));
									
					return new ConsequenceResult (this, 
					                              request,
					                              new Time (timeUntil), 
					                              this.FormatCaption (this.Caption, new Dictionary<string,string> {
						{"Cost", this.Cost.ToString ()}
					}
					), this.ImageName,
					   (timeUntil.TotalDays >= (double)LowerResultLimit && timeUntil.TotalDays <= (double)UpperResultLimit));
				} else {
					return new ConsequenceResult(this,
					                             request,
					                             new OverflowMessage(),
					                             "Try reducing the target price", 
					                             this.ImageName, false);
				}				
			} catch (Exception ex) {
				Console.WriteLine("{0} thrown when computing Time Until: {1}", ex.GetType().Name, ex.Message);
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

