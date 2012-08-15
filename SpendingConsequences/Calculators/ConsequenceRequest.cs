using System;
using System.Collections.Generic;

namespace SpendingConsequences.Calculators
{
	public class ConsequenceRequest
	{
		public ConsequenceRequest (Money amount, TriggerType mode)
		{
			this.InitialAmount = amount;
			this.TriggerMode = mode;
		}
		
		public Money InitialAmount { get; private set; }
		
		public TriggerType TriggerMode { get; private set; }

		public decimal DaysPerPeriod { 
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

		public Money AmountAfter (TimeSpan period)
		{
			// Either one-time, or insufficient for further calculation
			if (TriggerMode == TriggerType.OneTime || TriggerMode == TriggerType.All || TriggerMode == TriggerType.Undefined || TriggerMode == TriggerType.Repeating)
				return InitialAmount;
			
			return ((decimal)period.TotalDays / DayCounts [TriggerMode]) * InitialAmount;
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
		private static Dictionary<TriggerType, decimal> DayCounts = new Dictionary<TriggerType, decimal> () {
			{TriggerType.OneTime, decimal.MaxValue},
			{TriggerType.PerDay, 1m},
			{TriggerType.PerWeek, 7m},
			{TriggerType.PerMonth, 30.4375m},
			{TriggerType.PerQuarter, 91.3125m},
			{TriggerType.PerYear, 365.25m}
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
		
		public static Dictionary<TimeUnit, decimal> DaysPerUnit = new Dictionary<TimeUnit, decimal> {
			{TimeUnit.Millisecond, 0.0000000116m},
			{TimeUnit.Second, 0.0000115741m},
			{TimeUnit.Minute, 0.0006944444m},
			{TimeUnit.Hour, 0.0416666667m},
			{TimeUnit.Day, 1m},
			{TimeUnit.Week, 7m},
			{TimeUnit.Month, 30.4375m},
			{TimeUnit.Quarter, 91.3125m},
			{TimeUnit.Year, 365.25m},
			{TimeUnit.Century, 36525m},
			{TimeUnit.Millenium, 365250m}
		};
		
		public static Dictionary<TimeUnit, decimal> SecondsPerUnit = new Dictionary<TimeUnit, decimal> {
			{TimeUnit.Millisecond, 0.001m},
			{TimeUnit.Second, 1m},
			{TimeUnit.Minute, 60m},
			{TimeUnit.Hour, 3600m},
			{TimeUnit.Day, 86400m},
			{TimeUnit.Week, 604800m},
			{TimeUnit.Month, 2629800m},
			{TimeUnit.Quarter, 7889400m},
			{TimeUnit.Year, 31557600m},
			{TimeUnit.Century, 3155760000m},
			{TimeUnit.Millenium, 31557600000m}
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

