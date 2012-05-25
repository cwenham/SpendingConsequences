using System;
using System.Linq;
using System.Collections.Generic;

namespace SpendingConsequences.Calculators
{
	/// <summary>
	/// Wrapper for TimeStamp, overriding ToString to produce human-readable results
	/// </summary>
	public class Time
	{
		public Time (TimeSpan span)
		{
			this.Value = span;
		}
		
		public static implicit operator Time(TimeSpan span)
        {
            return new Time(span);
        }
		
		public TimeSpan Value { get; private set; }
		
		public override string ToString ()
		{
			return ShortHumanizedTimespan (this);
		}
		
		public string ToShortString ()
		{
			return ShortHumanizedTimespan (this);
		}
		
		public string ToLongString ()
		{
			return LongHumanizedTimespan (this);
		}
		
		#region Static helper methods
		/// <summary>
		/// Return a string for a timespan in a meaningful form, eg "4.1 years" rather than "50 months"
		/// </summary>
		public static string ShortHumanizedTimespan (Time span)
		{			
			TimeSpan period = span.Value;
			
			if (period.TotalMinutes <= 59)
				return string.Format ("{0:%m} minutes", period);
			
			if (period.TotalMinutes <= 2879)
				return string.Format ("{0:%h} hours {0:%m} minutes", period);
		
			string unit = null;
			double val = 0;
			
			if (period.TotalDays < 7) {
				unit = "days";
				val = period.TotalDays;
			} else if (period.TotalDays <= 49) {
				unit = "weeks";
				val = period.TotalDays / 7;
			} else if (period.TotalDays <= 540) {
				unit = "months";
				val = period.TotalDays / 30;
			} else {
				unit = "years";
				val = period.TotalDays / 365.25;
			}
			
			if (val % 1 == 0)
				return string.Format ("{0:0} {1}", val, unit);
			else
				return string.Format ("{0:0.0} {1}", val, unit);
		}
		
		/// <summary>
		/// Return a string for a timespan in a meaningful form, eg "4 years, 2 months" rather than "50 months"
		/// </summary>
		public static string LongHumanizedTimespan (Time span)
		{
			TimeSpan period = span.Value;
			
			if (period.TotalMinutes <= 2879)
				return string.Format ("{0:%h} hours {0:%m} minutes", period);
			
			List<string> pieces = new List<string> ();
			
			if (period.TotalDays > 365) {
				int years = (int)(Math.Floor (period.TotalDays / 365));
				period = period.Subtract (new TimeSpan (365 * years, 0, 0, 0));
				pieces.Add (string.Format ("{0} years", years));
			}
			
			if (period.TotalDays > 30) {
				int months = (int)(Math.Floor (period.TotalDays / 30));
				period = period.Subtract (new TimeSpan (30 * months, 0, 0, 0));
				pieces.Add (string.Format ("{0} months", months));
			}
			
			if (period.TotalDays > 7) {
				int weeks = (int)(Math.Floor (period.TotalDays / 7));
				period = period.Subtract (new TimeSpan (7 * weeks, 0, 0, 0));
				pieces.Add (string.Format ("{0} weeks", weeks));
			}
			
			if (period.TotalDays > 0)
				pieces.Add (string.Format ("{0} days", period.TotalDays));
			
			if (pieces.Count == 1)
				return pieces.First();
			
			if (pieces.Count > 1)
				string.Format ("{0} and {1}",
				              string.Join (", ", pieces.Take (pieces.Count - 1).ToArray ()),
				              pieces.Last ()
				);
			
			return null;
		}
		#endregion
	}
}

