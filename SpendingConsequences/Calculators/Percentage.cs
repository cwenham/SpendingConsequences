using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace SpendingConsequences.Calculators
{
	public class Percentage : ACalculator
	{
		public const decimal DEFAULT_AMOUNT = 15m;
		public const int DEFAULT_YEARS = 5;

		public Percentage (XElement definition) : base(definition)
		{
		}

		/// <summary>
		/// Percentage to take
		/// </summary>
		public decimal Amount {
			get {
				if (ConfigurableValues.ContainsKey ("Amount"))
					return ((decimal)ConfigurableValues ["Amount"].Value);
				else
					return DEFAULT_AMOUNT;
			}
		}

		/// <summary>
		/// Number of years to calculate for repeating investments
		/// </summary>
		/// <value>
		public int Years {
			get {
				if (ConfigurableValues.ContainsKey ("Years"))
					return ((int)ConfigurableValues ["Years"].Value);
				else
					return DEFAULT_YEARS;
			}
		}

		#region implemented abstract members of SpendingConsequences.Calculators.ACalculator
		public override ConsequenceResult Calculate (ConsequenceRequest request)
		{
			Money total = request.TriggerMode == TriggerType.OneTime ? request.InitialAmount : request.AmountAfter(TimeSpan.FromDays((Double)ConsequenceRequest.DaysPerUnit[TimeUnit.Year] * this.Years));

			return new ConsequenceResult(this,
			                             request,
			                             total * PercentAsDecimal(this.Amount),
			                             FormatCaption(this.Caption, new Dictionary<string,string> {
				{"Percentage", String.Format("{0}%", this.Amount)},
				{"Years", Years.ToString()}
			}),
			                             this.Image,
			                             true);
		}
		#endregion

	}
}

