using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace SpendingConsequences.Calculators
{
	public class ConsequenceResult
	{
		public ConsequenceResult (ACalculator calculator,
		                          ConsequenceRequest request,
		                          object computedValue, 
		                          string formattedCaption,
		                          string imageName)
		{
			this.Calculator = calculator;
			this.Request = request;
			this.ComputedValue = computedValue;
			this.FormattedCaption = formattedCaption;
			this.ImageName = imageName;
		}
		
		public ACalculator Calculator { get; private set; }
		
		public ConsequenceRequest Request { get; private set; }
		
		public object ComputedValue {
			get;
			private set;
		}
		
		public String FormattedCaption { get; private set; }
		
		public string ImageName { get; private set; }
	}
}

