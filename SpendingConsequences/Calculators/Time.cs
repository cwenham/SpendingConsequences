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
				return string.Format ("{0:%m} minute{1}", period, period.TotalMinutes > 1 ? "s" : "");
			
			if (period.TotalMinutes <= 2879)
				return string.Format ("{0:%h} hour{1} {0:%m} minute{2}", period, period.Hours > 1 ? "s" : "", period.Minutes > 1 ? "s" : "");
		
			string unit = null;
			double val = 0;
			
			if (period.TotalDays < 7) {
				val = period.TotalDays;
				unit = val > 1 ? "days" : "day";
			} else if (period.TotalDays <= 49) {
				val = period.TotalDays / 7;
				unit = val > 1 ? "weeks" : "week";
			} else if (period.TotalDays <= 540) {
				val = period.TotalDays / 30;
				unit = val > 1 ? "months" : "month";
			} else {
				val = period.TotalDays / 365.25;
				unit = val > 1 ? "years" : "year";
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
			
			if (period.TotalMinutes <= 59)
				return string.Format ("{0:%m} minute{1}", period, period.TotalMinutes > 1 ? "s" : "");
			
			if (period.TotalMinutes <= 2879)
				return string.Format ("{0:%h} hour{1} {0:%m} minute{2}", period, period.Hours > 1 ? "s" : "", period.Minutes > 1 ? "s" : "");
			
			List<string> pieces = new List<string> ();
			
			if (period.TotalDays > 365) {
				int years = (int)(Math.Floor (period.TotalDays / 365));
				period = period.Subtract (new TimeSpan (365 * years, 0, 0, 0));
				pieces.Add (string.Format ("{0} year{1}", years, years > 1 ? "s" : ""));
			}
			
			if (period.TotalDays > 30) {
				int months = (int)(Math.Floor (period.TotalDays / 30));
				period = period.Subtract (new TimeSpan (30 * months, 0, 0, 0));
				pieces.Add (string.Format ("{0} month{1}", months, months > 1 ? "s" : ""));
			}
			
			if (period.TotalDays > 7) {
				int weeks = (int)(Math.Floor (period.TotalDays / 7));
				period = period.Subtract (new TimeSpan (7 * weeks, 0, 0, 0));
				pieces.Add (string.Format ("{0} week{1}", weeks, weeks > 1 ? "s" : ""));
			}
			
			if (period.TotalDays > 0)
				pieces.Add (string.Format ("{0} day{1}", period.TotalDays, period.TotalDays > 1 ? "s" : ""));
			
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

