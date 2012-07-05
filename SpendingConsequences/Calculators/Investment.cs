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
		private const decimal DEFAULT_RATE = 5.0m;
		private const int DEFAULT_YEARS = 5;
		private const string DEFAULT_COMPOUNDING_FREQUENCY = "Monthly";
		
		public Investment (XElement definition) : base(definition)
		{
		}
		
		/// <summary>
		/// The Annual Percentage of Return (APR) as a percentage
		/// </summary>
		public decimal Rate {
			get {
				if (ConfigurableValues.ContainsKey ("Rate"))
					return ((decimal)ConfigurableValues ["Rate"].Value);
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
			
			if (request.InitialAmount.Value == 0m)
				return null;
			
			int annualCompoundings = CompoundingsPerYear (Compounding);
			
			int compoundingPeriods = Years * annualCompoundings;
			decimal ratePerPeriod = PercentAsDecimal (Rate) / annualCompoundings;
			decimal investmentsPerYear = InvestmentsPerYear (request.TriggerMode);
			
			Money result = null;
			
			try {
				if (request.TriggerMode == TriggerType.OneTime) {
					// A one-time investment means we can use the compound interest formula

					result = request.InitialAmount * Financials.Pow (1 + ratePerPeriod, compoundingPeriods);

				} else if (annualCompoundings == investmentsPerYear) {
					// Compounding frequency is the same as the investment frequency, so we can
					// use a mathematical series.
					// Described here: http://mathdude.quickanddirtytips.com/math-dude-web-bonus.aspx
					
					decimal seriesSum = Enumerable.Range (1, compoundingPeriods).Sum (x => Financials.Pow (1 + ratePerPeriod, x));
				
					result = request.InitialAmount.Value * seriesSum;
				
				} else if (annualCompoundings > investmentsPerYear) {
					// We need to take the sum of sub-periods and apply the standard compound interest formula to each
				
					decimal investmentPeriods = Years * investmentsPerYear;
					int compoundingsPerInvestment = ((int)Math.Floor (annualCompoundings / investmentsPerYear));
				
					Money sum = request.InitialAmount * Financials.Pow (1 + ratePerPeriod, compoundingsPerInvestment);
					for (int i = 1; i <= investmentPeriods - 1; i++)
						sum = (sum + (request.InitialAmount * Financials.Pow (1 + ratePerPeriod, compoundingsPerInvestment)));
				
					result = sum;
					
				} else if (investmentsPerYear > annualCompoundings) {
					// Apply the interest to the midpoint of the previous + next balance for each compounding period
			
					decimal investmentsPerPeriod = (decimal)(investmentsPerYear / annualCompoundings);
					decimal amountPerPeriod = (investmentsPerPeriod * request.InitialAmount.Value);
				
					decimal sum = 0;
					for (int i = 1; i <= compoundingPeriods; i++) {
						sum = sum + amountPerPeriod + ((sum + amountPerPeriod / 2) * (decimal)ratePerPeriod);
					}
				
					result = sum;
				}				
			} catch (OverflowException oex) {
				Console.WriteLine (String.Format ("Number overflow in investment calculator: {0}", oex.Message));
				return new ConsequenceResult (this,
				                              request,
				                              new OverflowMessage (),
				                              "Try reducing the interest rate or years",
				                              this.ImageName,
				                              false				                             
				);
			} catch (Exception ex) {
				Console.WriteLine (String.Format ("{0} thrown in investment calculator: {1}", ex.GetType ().Name, ex.Message));
				return null;
			}

			// Investment schedule routine isn't working properly at the moment.
//			return new ConsequenceResult (this, 
//			                              request,
//			                              new Money (result),
//			                              new TabularResult(request.Summary, string.Format ("{0} invested at {1:0.00}%", request.Summary, Rate), this),
//			                              FormatMyCaption (),
//			                              this.ImageName,
//			                              (result >= this.LowerResultLimit && result <= this.UpperResultLimit));

			return new ConsequenceResult (this, 
			                              request,
			                              result,
			                              FormatMyCaption (),
			                              this.ImageName,
			                              (result >= this.LowerResultLimit && result <= this.UpperResultLimit));
		}
		
		public override XElement GetTableData (ConsequenceResult result)
		{
			int annualCompoundings = CompoundingsPerYear (Compounding);
			decimal investmentsPerYear = (decimal)(InvestmentsPerYear (result.Request.TriggerMode));

			decimal reportsPerYear = result.Request.ReportsPerYear;
			
			var schedule = Financials.InvestmentSchedule (result.Request.InitialAmount.Value,
			                                             investmentsPerYear,
			                                             annualCompoundings,
			                                             PercentAsDecimal (Rate),
			                                             (int)(Math.Floor(investmentsPerYear * Years)),
			                                             reportsPerYear);	
			
			return new XElement (new XStreamingElement ("InvestmentSchedule", 
			                    new XAttribute ("Title", string.Format ("{0} invested at {1:0.00}%", result.Request.Summary, Rate)),
			                       from i in schedule
			                       select new XElement ("Row",
			                     new XElement ("Installment", string.Format ("{0} {1}", result.Request.ModeUnit, i.Installment)),
			                    new XElement ("Investment", i.Investment.ToString ("C")),
			                    new XElement ("Earnings", i.Earnings.ToString ("C")),
			                    new XElement ("Balance", i.Balance.ToString ("C")))
			)
			);
		}
		#endregion
	}
}

