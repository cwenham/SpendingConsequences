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
			
			
			decimal totalPayment = paymentPerInstallment * Installments;
			decimal maxLoanAmount = 0;
			if (Rate > 0)
				maxLoanAmount = (decimal)(((double)paymentPerInstallment / ratePerPeriod) * (1 - (1 / Math.Pow ((1 + ratePerPeriod), (double)Installments))));
			else
				maxLoanAmount = totalPayment;
			
			return new ConsequenceResult (this,
		                             request,
		                             new Money (maxLoanAmount),
		                             FormatCaption (this.Caption, new Dictionary<string,string> {
			{"Installments", Installments.ToString ()},
			{"TotalPayment", totalPayment.ToString ()},
			{"TotalInterest", (totalPayment - maxLoanAmount).ToString()}
		}
			), this.ImageName,
			   (maxLoanAmount >= LowerResultLimit && maxLoanAmount <= UpperResultLimit));
		}
		#endregion
	}
}

