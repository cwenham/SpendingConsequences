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

		#region implemented abstract members of SpendingConsequences.Calculators.ACalculator
		public override ConsequenceResult Calculate (ConsequenceRequest request)
		{
			if (request.InitialAmount == 0 || request.InitialAmount < LowerThreshold || request.InitialAmount > UpperThreshold)
				return null;
			
			var amortization = Financials.Amortization (request.InitialAmount, 
			                                 CompoundingsPerYear (Compounding), 
			                                 PercentAsDouble (Rate), 
			                                 PercentAsDouble (MinPayPercent),
			                                 MinimumPayment,
			                                 PayoffMode);
			
			int installments = 0;
			decimal totalInterest = 0;
			foreach (var i in amortization) {
				installments++;
				totalInterest += i.Interest;
			}
			
			decimal payoff = request.InitialAmount + totalInterest;
			
			return new ConsequenceResult (this,
				                             request,
				                             new Money (payoff),
				                             new TabularResult (
					request.Summary,
					String.Format ("Amortization of a {0:C} loan at {1}%", request.InitialAmount, Rate),
					this),
				                             FormatCaption (this.Caption, new Dictionary<string,string> {
					{"Periods", installments.ToString()},
					{"Interest", totalInterest.ToString("C")}
				}
			), this.ImageName,
				   (payoff >= LowerResultLimit && payoff <= UpperResultLimit));		
		}
		
		
		public override XElement GetTableData (ConsequenceResult result)
		{
			var amortization = Financials.Amortization (result.Request.InitialAmount, 
			                                 CompoundingsPerYear (Compounding), 
			                                 PercentAsDouble (Rate), 
			                                 PercentAsDouble (MinPayPercent),
			                                 MinimumPayment,
			                                 PayoffMode);

			return new XElement (new XStreamingElement ("Amortization", 
			                    new XAttribute ("Title", string.Format ("{0:C} financed at {1}%", result.Request.InitialAmount, Rate)),
			                       from i in amortization
			                       select new XElement ("Row",
			                    new XElement ("Installment", string.Format ("Payment {0}", i.Installment)),
			                    new XElement ("Payment", i.Payment.ToString ("C")),
			                    new XElement ("Interest", i.Interest.ToString ("C")),
			                    new XElement ("Principal", (i.Payment - i.Interest).ToString ("C")),
			                    new XElement ("Balance", i.Balance.ToString ("C")))
			)
			);
		}
		#endregion
	}
}

