using System;
using System.Collections.Generic;

namespace SpendingConsequences.Calculators
{
	public class ConsequenceRequest
	{
		public ConsequenceRequest (decimal amount, TriggerType mode)
		{
			this.InitialAmount = amount;
			this.TriggerMode = mode;
		}
		
		public decimal InitialAmount { get; private set; }
		
		public TriggerType TriggerMode { get; private set; }

		public double DaysPerPeriod { 
			get {
				return DayCounts [TriggerMode];
			}
		}

		public decimal InvestmentsPerYear {
			get {
				return PeriodsPerYear [TriggerMode];
			}
		}

		public decimal ReportsPerYear {
			get {
				if (TriggerMode == TriggerType.OneTime)
					return PeriodsPerYear[TriggerType.PerMonth];
				if (TriggerMode == TriggerType.PerDay)
					return PeriodsPerYear[TriggerType.PerWeek];
				return PeriodsPerYear[TriggerMode];
			}
		}

		public string ModeUnit {
			get {
				return ModeUnits [TriggerMode];
			}
		}

		public string ModeTerm {
			get {
				return ShortTerms [TriggerMode];
			}
		}

		public decimal AmountAfter (TimeSpan period)
		{
			// Either one-time, or insufficient for further calculation
			if (TriggerMode == TriggerType.OneTime || TriggerMode == TriggerType.All || TriggerMode == TriggerType.Undefined || TriggerMode == TriggerType.Repeating)
				return InitialAmount;
			
			return (decimal)(period.TotalDays / DayCounts [TriggerMode]) * InitialAmount;
		}
		
		/// <summary>
		/// Summary of the request, for use in titles
		/// </summary>
		public string Summary {
			get {
				return String.Format("{0:C} {1}", InitialAmount, ShortTerms[TriggerMode]);
			}
		}
		
		// These are "good enough" values, sufficient for the app's scope. The
		// calculations we'll base on these woudln't be sufficient for apps that
		// require higher precision. EG: A month or quarter can't be summarized
		// to 30 and 90 days, respectively. If we adapt this for ETF, we must use
		// calendar-sensitive math.
		private static Dictionary<TriggerType, double> DayCounts = new Dictionary<TriggerType, double> () {
			{TriggerType.OneTime, double.MaxValue},
			{TriggerType.PerDay, 1},
			{TriggerType.PerWeek, 7},
			{TriggerType.PerMonth, 30.4375},
			{TriggerType.PerQuarter, 91.3125},
			{TriggerType.PerYear, 365.25}
		};
		
		private static Dictionary<TriggerType, decimal> PeriodsPerYear = new Dictionary<TriggerType, decimal> () {
			{TriggerType.OneTime, 1},
			{TriggerType.PerDay, 365.25m},
			{TriggerType.PerWeek, 52.1785714286m},
			{TriggerType.PerMonth, 12m},
			{TriggerType.PerQuarter, 4m},
			{TriggerType.PerYear, 1m}
		};
		
		private static Dictionary<TriggerType, string> ModeUnits = new Dictionary<TriggerType, string> () {
			{TriggerType.OneTime, "Investment"},
			{TriggerType.PerDay, "Day"},
			{TriggerType.PerWeek, "Week"},
			{TriggerType.PerMonth, "Month"},
			{TriggerType.PerQuarter, "Quarter"},
			{TriggerType.PerYear, "Year"}
		};
		
		private static Dictionary<TriggerType, string> ShortTerms = new Dictionary<TriggerType, string>() {
			{TriggerType.OneTime, "Once"},
			{TriggerType.PerDay, "Daily"},
			{TriggerType.PerWeek, "Weekly"},
			{TriggerType.PerMonth, "Monthly"},
			{TriggerType.PerQuarter, "Quarterly"},
			{TriggerType.PerYear, "Yearly"}
		};
		
		public static Dictionary<TimeUnit, double> DaysPerUnit = new Dictionary<TimeUnit, double> {
			{TimeUnit.Millisecond, 0.0000000116},
			{TimeUnit.Second, 0.0000115741},
			{TimeUnit.Minute, 0.0006944444},
			{TimeUnit.Hour, 0.0416666667},
			{TimeUnit.Day, 1},
			{TimeUnit.Week, 7},
			{TimeUnit.Month, 30.4375},
			{TimeUnit.Quarter, 91.3125},
			{TimeUnit.Year, 365.25},
			{TimeUnit.Century, 36525},
			{TimeUnit.Millenium, 365250}
		};
		
		public static Dictionary<TimeUnit, double> SecondsPerUnit = new Dictionary<TimeUnit, double> {
			{TimeUnit.Millisecond, 0.001},
			{TimeUnit.Second, 1},
			{TimeUnit.Minute, 60},
			{TimeUnit.Hour, 3600},
			{TimeUnit.Day, 86400},
			{TimeUnit.Week, 604800},
			{TimeUnit.Month, 2629800},
			{TimeUnit.Quarter, 7889400},
			{TimeUnit.Year, 31557600},
			{TimeUnit.Century, 3155760000},
			{TimeUnit.Millenium, 31557600000}
		};
	}
	
	public enum TimeUnit {
		Millisecond,
		Second,
		Minute,
		Hour,
		Day,
		Week,
		Month,
		Quarter,
		Year,
		Decade,
		Century,
		Millenium
	}
}

