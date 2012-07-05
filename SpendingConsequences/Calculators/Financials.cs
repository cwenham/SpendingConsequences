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

		public static IEnumerable<AmortizedInstallment> Amortization (decimal amount, int annualCompoundings, decimal rate, decimal minPayRate, decimal paymentFloor, PayoffMode payMode)
		{
			decimal ratePerPeriod = rate / annualCompoundings;
			
			decimal remaining = amount;
			
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
		
		public static IEnumerable<InvestmentInstallment> InvestmentSchedule (decimal periodicInvestment, 
		                                                                     decimal investmentsPerYear, 
		                                                                     decimal annualCompoundings, 
		                                                                     decimal rate, 
		                                                                     int investmentPeriods,
		                                                                     decimal reportsPerYear)
		{
			int daysOfInvestment = (int)(Math.Floor ((investmentPeriods / investmentsPerYear) * (decimal)ConsequenceRequest.DaysPerUnit [TimeUnit.Year]));			
			decimal ratePerDay = (decimal)rate / (decimal)ConsequenceRequest.DaysPerUnit [TimeUnit.Year];
			decimal daysPerInvestment = (decimal)ConsequenceRequest.DaysPerUnit [TimeUnit.Year] / investmentsPerYear;
			decimal daysPerCompounding = (decimal)ConsequenceRequest.DaysPerUnit [TimeUnit.Year] / annualCompoundings;
			decimal daysPerReport = (decimal)ConsequenceRequest.DaysPerUnit [TimeUnit.Year] / reportsPerYear;
			
			decimal balance = periodicInvestment;
			decimal interestEarned = 0;
			decimal daysUntilInvestment = daysPerInvestment;
			decimal daysUntilCompounding = daysPerCompounding;
			decimal daysUntilReport = daysPerReport;
			
			int installment = 1;
			
			InvestmentInstallment nextInstallment = new InvestmentInstallment {
				Installment = 1,
				Investment = periodicInvestment, 
				Earnings = 0
			};
			
			List<InvestmentInstallment> installments = new List<InvestmentInstallment> ();
			
			for (int i = 0; i < daysOfInvestment; i++) {
				try {	
					interestEarned += balance * ratePerDay;
					nextInstallment.Earnings += balance * ratePerDay;
				
					if (daysUntilReport < 1) {
						nextInstallment.Balance = balance;
						installments.Add (nextInstallment);
						
						nextInstallment = new InvestmentInstallment {
							Installment = installment,
							Investment = 0,
							Earnings = 0
						};
						
						daysUntilReport += daysPerReport;
					}
					
					if (daysUntilCompounding < 1) {
						balance += interestEarned;
						interestEarned = 0;
						daysUntilCompounding += daysPerCompounding;
					}
					
					if (daysUntilInvestment < 1) {
						installment += 1;
						
						daysUntilInvestment += daysPerInvestment;
						
						balance += periodicInvestment;
						nextInstallment.Investment += periodicInvestment;
						nextInstallment.Installment = installment;
					}
					
					daysUntilInvestment--;
					daysUntilCompounding--;
					daysUntilReport--;
				} catch (Exception ex) {
					Console.WriteLine (string.Format ("{0} thrown calculating installment: {1}", ex.GetType ().Name, ex.Message));
				}
	
			}
			
			balance += interestEarned;
			
			if (nextInstallment != null) {
				nextInstallment.Earnings = interestEarned;
				nextInstallment.Balance = balance;
				installments.Add (nextInstallment);
			}
			
			return installments;
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
	
	/// <summary>
	/// Individual installment in an ongoing investment
	/// </summary>
	/// <remarks>This is almost identical to AmortizedInstallment, but we might want to take them in different directions in the future. The strong typing
	/// may also help with coding.</remarks>
	public class InvestmentInstallment {
		public int Installment { get; set; }
		
		public decimal Investment { get; set; }
		
		public decimal Earnings { get; set; }
		
		public decimal Balance { get; set; }
	}
}

