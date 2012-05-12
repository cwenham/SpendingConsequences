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
		
		public int CompoundingsPerYear {
			get {
				switch (Compounding) {
				case "Monthly":
					return 12;
				case "Weekly":
					return 52;
				case "Annually":
					return 1;
				case "Quarterly":
					return 4;
				default:
					return 12;
				}
			}
		}
		
		private double InvestmentsPerYear (TriggerType trigger)
		{
			switch (trigger) {
			case TriggerType.PerDay:
				return 365.25;
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
		
		private string FormatMyCaption ()
		{
			return FormatCaption (this.Caption, new Dictionary<string,string> {
					{"Rate", Rate.ToString ()},
					{"Years", Years.ToString ()},
					{"Compounding", Compounding}
				});
		}
		
		#region implemented abstract members of SpendingConsequences.Calculators.ACalculator
		public override ConsequenceResult Calculate (ConsequenceRequest request)
		{
			// For some of the scenarios below we have to sacrifice a bit of accuracy in order to produce results
			// within a few milliseconds. EG: A savings account may compound monthly but calculate interest daily, but
			// that would require more CPU time than we can afford.
			
			if (request.InitialAmount == 0m)
				return null;
			
			double compoundingPeriods = Years * CompoundingsPerYear;
			double ratePerPeriod = (double)DecimalRate / CompoundingsPerYear;
			double investmentsPerYear = InvestmentsPerYear (request.TriggerMode);
			
			decimal result = 0;
			
			try {
				if (request.TriggerMode == TriggerType.OneTime) {
					// A one-time investment means we can use the compound interest formula
				
					result = request.InitialAmount * ((decimal)Math.Pow (1 + ratePerPeriod, compoundingPeriods));

				} else if (CompoundingsPerYear == investmentsPerYear) {
					// Compounding frequency is the same as the investment frequency, so we can
					// use a mathematical series.
					// Described here: http://mathdude.quickanddirtytips.com/math-dude-web-bonus.aspx
				
					double seriesSum = 0;
					for (int i = 1; i <= compoundingPeriods; i++)
						seriesSum += Math.Pow (1 + ratePerPeriod, i);
				
					result = (decimal)(((double)request.InitialAmount) * seriesSum);
				
				} else if (CompoundingsPerYear > investmentsPerYear) {
					// We need to take the sum of sub-periods and apply the standard compound interest formula to each
				
					double investmentPeriods = Years * investmentsPerYear;
					int compoundingsPerInvestment = ((int)Math.Floor (((double)CompoundingsPerYear / investmentsPerYear)));
				
					double sum = ((double)request.InitialAmount) * Math.Pow (1 + ratePerPeriod, compoundingsPerInvestment);
					for (int i = 1; i <= investmentPeriods - 1; i++)
						sum = (sum + ((double)request.InitialAmount)) * Math.Pow (1 + ratePerPeriod, compoundingsPerInvestment);
				
					result = (decimal)sum;
					
				} else if (investmentsPerYear > CompoundingsPerYear) {
					// Apply the interest to the midpoint of the previous + next balance for each compounding period
			
					double investmentsPerPeriod = (double)investmentsPerYear / CompoundingsPerYear;
					double amountPerPeriod = (investmentsPerPeriod * ((double)request.InitialAmount));
				
					double sum = 0;
					for (int i = 1; i <= compoundingPeriods; i++) {
						sum = sum + amountPerPeriod + ((sum + amountPerPeriod / 2) * ratePerPeriod);
					}
				
					result = (decimal)sum;
				}				
			} catch (OverflowException oex) {
				// ToDo: Find a way to convey via UI that we can't crunch numbers this big
				Console.WriteLine (String.Format ("Number overflow in investment calculator: {0}", oex.Message));
				return null;
			} catch (Exception ex) {
				Console.WriteLine (String.Format ("{0} thrown in investment calculator: {1}", ex.GetType ().Name, ex.Message));
				return null;
			}
			
			if (result >= this.LowerResultLimit && result <= this.UpperResultLimit)
				return new ConsequenceResult (this, 
				                              request,
				                              (decimal)result,
				                              FormatMyCaption (),
				                              this.ImageName);
			else
				return null;
		}
		#endregion
	}
}

