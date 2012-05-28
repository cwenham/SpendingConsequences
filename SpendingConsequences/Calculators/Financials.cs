using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace SpendingConsequences.Calculators
{
	public static class Financials
	{
		public static IEnumerable<AmortizedInstallment> Amortization (decimal amount, int annualCompoundings, double rate, double minPayRate, decimal paymentFloor, PayoffMode payMode)
		{
			double ratePerPeriod = rate / annualCompoundings;
			
			decimal remaining = amount;
			
			var baseAmortization = from p in Enumerable.Range (1, int.MaxValue)
				let interest = remaining * (decimal)ratePerPeriod
				let subBalance = remaining + interest
				let basePay = remaining * (decimal)minPayRate
				let minPay = payMode == PayoffMode.PercentPlusInterest ? basePay + interest : basePay
				let adjustedPay = minPay < paymentFloor ? paymentFloor : minPay
				let actualPay = adjustedPay > subBalance ? subBalance : adjustedPay
				select new AmortizedInstallment {
					Installment = p,
					Interest = interest,
					Payment = actualPay,
					BeforePayment = remaining += interest,
					Balance = remaining -= actualPay
					};
			
			return baseAmortization.TakeWhile (x => x.BeforePayment > 0);
		}
	}
	
	public class AmortizedInstallment {
		public int Installment { get; set; }
		
		public decimal Interest { get; set; }
		
		public decimal Payment { get; set; }
		
		public decimal BeforePayment { get; set; }
		
		public decimal Balance { get; set; }
	}
	
	public enum PayoffMode {
		FlatPercent,
		FlatAmount,
		PercentPlusInterest
	}
}

