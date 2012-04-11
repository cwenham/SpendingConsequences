using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace SpendingConsequences.Calculators
{
	/// <summary>
	/// A compound interest calculator
	/// </summary>
	public class Investment : ACalculator
	{
		private const float DEFAULT_RATE = 5.0f;
		private const int DEFAULT_YEARS = 5;
		
		public Investment (XElement definition) : base(definition)
		{
		}
		
		public double Rate {
			get {
				if (ConfigurableValues.ContainsKey ("Rate"))
					return ((double)ConfigurableValues ["Rate"].Value) / 100;
				else
					return DEFAULT_RATE;
			}
		}
		
		public int Years {
			get {
				if (ConfigurableValues.ContainsKey ("Years"))
					return ((int)ConfigurableValues ["Years"].Value);
				else
					return DEFAULT_YEARS;
			}
		}
		
		public int FrequencyPerYear {
			get {
				// Hard-code monthly compounding for now
				return 12;
			}
		}
		
		#region implemented abstract members of SpendingConsequences.Calculators.ACalculator
		public override ConsequenceResult Calculate (ConsequenceRequest request)
		{
			if (request.InitialAmount == 0m)
				return null;
			
			if (request.TriggerMode == TriggerType.OneTime) {
				double periods = Years * FrequencyPerYear;
				double ratePerPeriod = (double)Rate / FrequencyPerYear;
				return new ConsequenceResult (this, 
				                             request.InitialAmount * ((decimal)Math.Pow (1 + ratePerPeriod, periods)), 
				                             FormatCaption (this.Caption, new Dictionary<string,string> {
					{"Rate", Rate.ToString ()},
					{"Years", Years.ToString ()}
				}),
				                              this.ImageName);
			}
			
			return null;
		}
		#endregion
	}
}

