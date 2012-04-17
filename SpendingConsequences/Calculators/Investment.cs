using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace SpendingConsequences.Calculators
{
	/// <summary>
	/// A compound interest calculator
	/// </summary>
	public class Investment : ACalculator
	{
		private const float DEFAULT_RATE = 5.0f;
		private const int DEFAULT_YEARS = 5;
		private const string DEFAULT_COMPOUNDING_FREQUENCY = "Monthly";
		
		public Investment (XElement definition) : base(definition)
		{
		}
		
		/// <summary>
		/// The Annual Percentage of Return (APR) as a decimal
		/// </summary>
		public double DecimalRate {
			get {
				if (ConfigurableValues.ContainsKey ("Rate"))
					return ((double)ConfigurableValues ["Rate"].Value) / 100;
				else
					return DEFAULT_RATE;
			}
		}
		
		/// <summary>
		/// The Annual Percentage of Return (APR) as a percentage
		/// </summary>
		public double Rate {
			get {
				if (ConfigurableValues.ContainsKey ("Rate"))
					return ((double)ConfigurableValues ["Rate"].Value);
				else
					return DEFAULT_RATE;
			}
		}
		
		public int Years {
			get {
				if (ConfigurableValues.ContainsKey ("Years"))
					return ((int)ConfigurableValues ["Years"].Value);
				else
					return DEFAULT_YEARS;
			}
		}
		
		public string Compounding {
			get {
				if (ConfigurableValues.ContainsKey ("Compounding"))
					return ConfigurableValues ["Compounding"].Value as string;
				else
					return DEFAULT_COMPOUNDING_FREQUENCY;
			}
		}
		
		public int FrequencyPerYear {
			get {
				switch (Compounding) {
				case "Monthly":
					return 12;
				case "Weekly":
					return 52;
				case "Annually":
					return 1;
				default:
					return 12;
				}
			}
		}
		
		private int InvestmentsPerYear (TriggerType trigger)
		{
			switch (trigger) {
			case TriggerType.PerDay:
				return 365;
			case TriggerType.PerMonth:
				return 12;
			case TriggerType.PerQuarter:
				return 4;
			case TriggerType.PerWeek:
				return 52;
			case TriggerType.PerYear:
				return 1;
			default:
				return 1;
			}
		}
		
		#region implemented abstract members of SpendingConsequences.Calculators.ACalculator
		public override ConsequenceResult Calculate (ConsequenceRequest request)
		{
			if (request.InitialAmount == 0m)
				return null;
			
			double compoundingPeriods = Years * FrequencyPerYear;
			double ratePerPeriod = (double)DecimalRate / FrequencyPerYear;
			
			if (request.TriggerMode == TriggerType.OneTime) {

				return new ConsequenceResult (this, 
				                             request.InitialAmount * ((decimal)Math.Pow (1 + ratePerPeriod, compoundingPeriods)), 
				                             FormatCaption (this.Caption, new Dictionary<string,string> {
					{"Rate", DecimalRate.ToString ()},
					{"Years", Years.ToString ()}
				}),
				                              this.ImageName);
			} else {
				// Use a mathethematical series to calculate the mutliplier
				// Described here: http://mathdude.quickanddirtytips.com/math-dude-web-bonus.aspx
				double seriesSum = 0;
				for (int i = 1; i <= compoundingPeriods; i++) {
					seriesSum += Math.Pow (1 + ratePerPeriod, i);
				}
				
				// Since the contribution rate may not coincide with the compounding rate, we
				// need to calculate the investment-per-compounding period
				double recurringInvestment = (((double)request.InitialAmount) * InvestmentsPerYear (request.TriggerMode)) / FrequencyPerYear;
				
				return new ConsequenceResult (this,
				                             (decimal)(recurringInvestment * seriesSum),
				                             FormatCaption (this.Caption, new Dictionary<string,string> {
					{"Rate", Rate.ToString ()},
					{"Years", Years.ToString ()},
					{"Compounding", Compounding}
				}),
				                              this.ImageName);
			}
		}
		#endregion
	}
}

