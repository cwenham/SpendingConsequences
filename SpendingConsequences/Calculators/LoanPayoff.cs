using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpendingConsequences.Calculators
{
	public class LoanPayoff : ACalculator
	{
		private const double DEFAULT_RATE = 14.9;
		private const double DEFAULT_PAYMENT_PERCENT = 1;
		private const decimal DEFAULT_MINIMUM_PAYMENT = 5.0m;
		private const string DEFAULT_COMPOUNDING_FREQUENCY = "Monthly";
		
		private const PayoffMode DEFAULT_PAYOFF_MODE = PayoffMode.PercentPlusInterest;
		
		public LoanPayoff (XElement definition) : base(definition)
		{
		}
		
		/// <summary>
		/// The Annual interest rate as a percentage
		/// </summary>
		public double Rate {
			get {
				if (ConfigurableValues.ContainsKey ("Rate"))
					return ((double)ConfigurableValues ["Rate"].Value);
				else
					return DEFAULT_RATE;
			}
		}
		
		public PayoffMode PayoffMode {
			get {
				if (ConfigurableValues.ContainsKey ("PayoffMode"))
					return ((PayoffMode)ConfigurableValues ["PayoffMode"].Value);
				else
					return DEFAULT_PAYOFF_MODE;
			}
		}
		
		public double MinPayPercent {
			get {
				if (ConfigurableValues.ContainsKey ("MinPayPercent"))
					return ((double)ConfigurableValues ["MinPayPercent"].Value);
				else
					return DEFAULT_PAYMENT_PERCENT;
			}
		}
		
		public decimal MinimumPayment {
			get {
				if (ConfigurableValues.ContainsKey ("MinimumPayment"))
					return ((decimal)ConfigurableValues ["MinimumPayment"].Value);
				else
					return DEFAULT_MINIMUM_PAYMENT;
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
		
		private static List<string> AmortizationTableHeader = new List<string> {
			"Installment",
			"Payment",
			"Interest",
			"Principal",
			"Balance"
		};

		#region implemented abstract members of SpendingConsequences.Calculators.ACalculator
		public override ConsequenceResult Calculate (ConsequenceRequest request)
		{
			if (request.InitialAmount == 0 || request.InitialAmount < LowerThreshold || request.InitialAmount > UpperThreshold)
				return null;
			
			decimal accruedInterest = 0;
			int periods = 0;
			decimal remaining = request.InitialAmount;
			int annualCompoundings = CompoundingsPerYear (Compounding);
			double ratePerPeriod = PercentAsDouble (Rate) / annualCompoundings;
			double minPayRate = PercentAsDouble (MinPayPercent);
			
			List<List<string>> amortTable = new List<List<string>> ();
			
			while (remaining > 0m) {
				periods++;
				decimal interest = (decimal)((double)remaining * ratePerPeriod);
				accruedInterest += interest;
				decimal minPayment = (decimal)((double)remaining * minPayRate);
				remaining += interest;
				
				if (PayoffMode == SpendingConsequences.Calculators.PayoffMode.PercentPlusInterest)
					minPayment += interest;
				if (minPayment < MinimumPayment)
					minPayment = MinimumPayment;
				if (minPayment > remaining)
					minPayment = remaining;
				
				remaining -= minPayment;
				
				amortTable.Add (new List<string> {
					string.Format("Payment {0}", periods),  // Date
					minPayment.ToString("C"),               // Payment
					interest.ToString("C"),                 // Interest
					(minPayment - interest).ToString(),     // Principal paid
					remaining.ToString()                    // Balance
				}
				);
			}
			
			decimal payoff = accruedInterest + request.InitialAmount;
			
			return new ConsequenceResult (this,
			                             request,
			                             new Money(payoff),
			                             new TabularResult(request.Summary, String.Format("Amortization of a {0:C} loan at {1}%", request.InitialAmount, Rate), this, AmortizationTableHeader, amortTable),
			                             FormatCaption (this.Caption, new Dictionary<string,string> {
				{"Periods", periods.ToString()},
				{"Interest", accruedInterest.ToString ()}
			}
			), this.ImageName,
			   (payoff >= LowerResultLimit && payoff <= UpperResultLimit));
		}
		#endregion
	}
	
	public enum PayoffMode {
		FlatPercent,
		PercentPlusInterest
	}
}

