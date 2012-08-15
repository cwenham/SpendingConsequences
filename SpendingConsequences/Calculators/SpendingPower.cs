using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using SpendingConsequences.Calculators;

namespace SpendingConsequences.Calculators
{
	/// <summary>
	/// Determines the maximum size of a loan, given the number of payments and APR
	/// </summary>
	public class SpendingPower : ACalculator
	{
		private const decimal DEFAULT_RATE = 4.5m;
		private const int DEFAULT_INSTALLMENTS = 60;
		private const string DEFAULT_PAYMENT_FREQUENCY = "Monthly";
		
		public SpendingPower (XElement definition) : base(definition)
		{
		}
		
		/// <summary>
		/// The Annual interest rate as a percentage
		/// </summary>
		public decimal Rate {
			get {
				if (ConfigurableValues.ContainsKey ("Rate"))
					return ((decimal)ConfigurableValues ["Rate"].Value);
				else
					return DEFAULT_RATE;
			}
		}

		public int Installments {
			get {
				if (ConfigurableValues.ContainsKey ("Installments"))
					return ((int)ConfigurableValues ["Installments"].Value);
				else
					return DEFAULT_INSTALLMENTS;
			}
		}
		
		public string PaymentFrequency {
			get {
				if (ConfigurableValues.ContainsKey ("PaymentFrequency"))
					return ConfigurableValues ["PaymentFrequency"].Value as string;
				else
					return DEFAULT_PAYMENT_FREQUENCY;
			}
		}
		
		public override XElement GetTableData (ConsequenceResult result)
		{
			int annualPayments = CompoundingsPerYear (PaymentFrequency);
			Money paymentPerInstallment = (result.Request.InitialAmount * (decimal)(InvestmentsPerYear (result.Request.TriggerMode))) / annualPayments;
			
			var amortization = Financials.Amortization (ExchangeRates.CurrentRates.ConvertToGiven(result.ComputedValue as Money, paymentPerInstallment.CurrencyCode), 
			                                 CompoundingsPerYear (PaymentFrequency), 
			                                 PercentAsDecimal (Rate), 
			                                 0.0m,
			                                 paymentPerInstallment,
			                                 PayoffMode.FlatAmount);

			return new XElement (new XStreamingElement ("Amortization", 
			                    new XAttribute ("Title", string.Format ("{0} financed at {1}%", result.ComputedValue as Money, Rate)),
			                       from i in amortization
			                       select new XElement ("Row",
			                    new XElement ("Installment", string.Format ("Payment {0}", i.Installment)),
			                    new XElement ("Payment", i.Payment.ToString ()),
			                    new XElement ("Interest", i.Interest.ToString ()),
			                    new XElement ("Principal", (i.Payment - i.Interest).ToString ()),
			                    new XElement ("Balance", i.Balance.ToString ()))
			)
			);
		}
		
		#region implemented abstract members of SpendingConsequences.Calculators.ACalculator
		public override ConsequenceResult Calculate (ConsequenceRequest request)
		{
			if (request.InitialAmount <= 0 || request.TriggerMode == TriggerType.OneTime)
				return null;

			try {
				int annualPayments = CompoundingsPerYear (PaymentFrequency);
				decimal ratePerPeriod = PercentAsDecimal (Rate) / annualPayments;
				Money paymentPerInstallment = (request.InitialAmount * (decimal)(InvestmentsPerYear (request.TriggerMode))) / annualPayments;
				
				
				Money totalPayment = paymentPerInstallment * Installments;
				Money maxLoanAmount = null;
				if (Rate > 0)
					maxLoanAmount = ((paymentPerInstallment / ratePerPeriod) * (1 - (1 / Financials.Pow ((1 + ratePerPeriod), Installments))));
				else
					maxLoanAmount = totalPayment;
				
				return new ConsequenceResult (this,
			                             request,
			                             maxLoanAmount,
				                         new TabularResult (request.Summary, string.Format ("Amortization of a {0} loan at {1}%", maxLoanAmount, Rate), this),
			                             FormatCaption (this.Caption, new Dictionary<string,string> {
				{"Installments", Installments.ToString ()},
				{"TotalPayment", totalPayment.ToString ()},
				{"TotalInterest", (totalPayment - maxLoanAmount).ToString()}
			}
				), this.ImageName,
				   (maxLoanAmount >= LowerResultLimit && maxLoanAmount <= UpperResultLimit));				
			} catch (Exception ex) {
				Console.WriteLine("{0} thrown when computing spending power: {1}", ex.GetType().Name, ex.Message);
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

