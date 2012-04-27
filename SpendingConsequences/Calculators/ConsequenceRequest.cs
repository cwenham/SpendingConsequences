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
		
		public decimal AmountAfter (TimeSpan period)
		{
			// Either one-time, or insufficient for further calculation
			if (TriggerMode == TriggerType.OneTime || TriggerMode == TriggerType.All || TriggerMode == TriggerType.Undefined || TriggerMode == TriggerType.Repeating)
				return InitialAmount;
			
			return (decimal)(period.TotalDays / DayCounts [TriggerMode]) * InitialAmount;
		}
		
		// These are "good enough" values, sufficient for the app's scope. The
		// calculations we'll base on these woudln't be sufficient for apps that
		// require higher precision. EG: A month or quarter can't be summarized
		// to 30 and 90 days, respectively. If we adapt this for ETF, we must use
		// calendar-sensitive math.
		public static Dictionary<TriggerType, double> DayCounts = new Dictionary<TriggerType, double>() {
			{TriggerType.PerDay, 1},
			{TriggerType.PerWeek, 7},
			{TriggerType.PerMonth, 30},
			{TriggerType.PerQuarter, 90},
			{TriggerType.PerYear, 365.25}
		};
	}
}

