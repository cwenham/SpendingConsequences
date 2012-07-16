using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace SpendingConsequences.Calculators
{
	public static class Financials
	{
		public static decimal Pow(decimal val1, decimal val2)
		{
			return (decimal)Math.Pow((double)val1, (double)val2);
		}

		public static IEnumerable<AmortizedInstallment> Amortization (Money amount, int annualCompoundings, decimal rate, decimal minPayRate, decimal paymentFloor, PayoffMode payMode)
		{
			decimal ratePerPeriod = rate / annualCompoundings;
			
			Money remaining = amount;
			
			var baseAmortization = from p in Enumerable.Range (1, int.MaxValue)
				let interest = remaining * ratePerPeriod
				let subBalance = remaining + interest
				let basePay = remaining * minPayRate
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

		public static IEnumerable<InvestmentInstallment> InvestmentSchedule (ConsequenceRequest request,
		                                                                     Investment calculator)
		{
			decimal annualRate = ACalculator.PercentAsDecimal (calculator.Rate);
			decimal compoundingsPerYear = ACalculator.CompoundingsPerYear (calculator.Compounding);
			decimal periodRate = annualRate / compoundingsPerYear;

			Money balance = 0m;

			if (request.TriggerMode != TriggerType.OneTime) {
				Money periodicInvestment = request.InitialAmount;
				decimal totalInvestments = request.InvestmentsPerYear * calculator.Years;
				decimal compoundingsPerInvestment = compoundingsPerYear / request.InvestmentsPerYear;
				var baseSchedule = from p in Enumerable.Range (1, (int)Math.Floor (totalInvestments))
									let newBalance = balance += periodicInvestment
									let earnings = ((balance * periodRate) * compoundingsPerInvestment)
									select new InvestmentInstallment {
									    Installment = p,
									    Investment = periodicInvestment,
									    Earnings = earnings,
										Balance = balance += earnings
								    };

				return baseSchedule;
			} else {
				decimal totalCompoundings = compoundingsPerYear * calculator.Years;
				balance = request.InitialAmount;
				var baseSchedule = from p in Enumerable.Range (1, (int)Math.Floor (totalCompoundings))
					let earnings = balance * periodRate
					select new InvestmentInstallment {
						Installment = p,
						Investment = p == 1 ? request.InitialAmount : 0,
						Earnings = earnings,
						Balance = balance += earnings
					};

				return baseSchedule;
			}
		}
	}
	
	public class AmortizedInstallment {
		public int Installment { get; set; }
		
		public Money Interest { get; set; }
		
		public Money Payment { get; set; }
		
		public Money BeforePayment { get; set; }
		
		public Money Balance { get; set; }
	}
	
	public enum PayoffMode {
		FlatPercent,
		FlatAmount,
		PercentPlusInterest
	}
	
	/// <summary>
	/// Individual installment in an ongoing investment
	/// </summary>
	/// <remarks>This is almost identical to AmortizedInstallment, but we might want to take them in different directions in the future. The strong typing
	/// may also help with coding.</remarks>
	public class InvestmentInstallment {
		public int Installment { get; set; }
		
		public Money Investment { get; set; }
		
		public Money Earnings { get; set; }
		
		public Money Balance { get; set; }
	}
}

