using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace SpendingConsequences.Calculators
{
	public class Within10Percent : ACalculator
	{
		public Within10Percent (XElement definition) : base(definition)
		{
		}
		
		public decimal Threshold {
			get {
				if (ConfigurableValues.ContainsKey ("Threshold"))
					return ((decimal)ConfigurableValues ["Threshold"].Value);
				else
					return 0;
			}
		}
		
		public int Limit {
			get {
				if (ConfigurableValues.ContainsKey ("Limit"))
					return ((int)ConfigurableValues ["Limit"].Value);
				else
					return int.MaxValue;
			}
		}

		#region implemented abstract members of SpendingConsequences.Calculators.ACalculator
		public override ConsequenceResult Calculate (ConsequenceRequest request)
		{
			if (Threshold == 0m)
				return null;
			
			decimal upper = Threshold + (Threshold * 0.10m);
			decimal lower = Threshold - (Threshold * 0.10m);
			
			if (request.InitialAmount >= lower && request.InitialAmount <= upper)
				return new ConsequenceResult (this, Threshold, this.Caption, this.ImageName);
			
			for (int i = 1; i <= Limit; i++) {
				decimal accumilated = request.AmountAfter (TimeSpan.FromDays (30 * i));
				if (accumilated >= lower && accumilated <= upper)
					return new ConsequenceResult (this, Threshold, this.FormatCaption (this.Caption, new Dictionary<string,string> {
						{"Months", i.ToString ()},
						{"Threshold", this.Threshold.ToString ()}
					}),
					                              this.ImageName);
			}
			
			return null;
		}
		#endregion
	}
}

