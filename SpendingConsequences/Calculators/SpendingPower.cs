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
		private const double DEFAULT_RATE = 4.5;
		private const int DEFAULT_INSTALLMENTS = 60;
		private const string DEFAULT_PAYMENT_FREQUENCY = "Monthly";
		
		public SpendingPower (XElement definition) : base(definition)
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
		
		#region implemented abstract members of SpendingConsequences.Calculators.ACalculator
		public override ConsequenceResult Calculate (ConsequenceRequest request)
		{
			if (request.InitialAmount <= 0 || request.TriggerMode == TriggerType.OneTime)
				return null;
			
			int annualPayments = CompoundingsPerYear (PaymentFrequency);
			double ratePerPeriod = PercentAsDouble (Rate) / annualPayments;
			decimal paymentPerInstallment = (request.InitialAmount * (decimal)(InvestmentsPerYear (request.TriggerMode))) / annualPayments;
			
			// The lender will assume payments will be made at the PaymentFrequency, so even if the borrower makes more frequent payments the loan will still be 
			// calculated on the former assumption. That means we can treat this like a periodic investment and subtract the interest from the sum to get the 
			// maximum loan amount.
			double seriesSum = 0;
			for (int i = 1; i <= Installments; i++)
				seriesSum += Math.Pow (1 + ratePerPeriod, i);
				
			decimal totalWithInterest = (decimal)(((double)paymentPerInstallment) * seriesSum);
			decimal totalPayment = paymentPerInstallment * Installments;
			decimal maxLoanAmount = totalPayment - (totalWithInterest - totalPayment);
			
			if (maxLoanAmount >= LowerResultLimit && maxLoanAmount <= UpperResultLimit)
				return new ConsequenceResult (this,
			                             request,
			                             new Money (maxLoanAmount),
			                             FormatCaption (this.Caption, new Dictionary<string,string> {
				{"Installments", Installments.ToString ()},
				{"TotalPayment", totalPayment.ToString ()},
				{"TotalInterest", (totalWithInterest - totalPayment).ToString()}
			}
				), this.ImageName);
			else
				return null;
		}
		#endregion
	}
}

