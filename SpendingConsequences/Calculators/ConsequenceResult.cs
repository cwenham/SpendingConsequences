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
		                          Image image,
		                          bool recommended)
		{
			this.Calculator = calculator;
			this.Request = request;
			this.ComputedValue = computedValue;
			this.FormattedCaption = formattedCaption;
			this.Image = image;
			this.Recommended = recommended;
		}
		
		public ConsequenceResult (ACalculator calculator,
		                          ConsequenceRequest request,
		                          object computedValue, 
		                          TabularResult table,
		                          string formattedCaption,
		                          Image image,
		                          bool recommended)
		{
			this.Calculator = calculator;
			this.Request = request;
			this.ComputedValue = computedValue;
			this.FormattedCaption = formattedCaption;
			this.Image = image;
			this.Recommended = recommended;
			this.Table = table;
			this.Table.MainResult = this;
		}
		
		public ACalculator Calculator { get; private set; }
		
		public ConsequenceRequest Request { get; private set; }
		
		public object ComputedValue {
			get;
			private set;
		}
		
		public String FormattedCaption { get; private set; }
		
		public Image Image { get; private set; }
		
		/// <summary>
		/// True if the result is worth including in the result set, false if it should be excluded
		/// </summary>
		/// <remarks>A problem was noticed when the user changes the configurable values of a consequence module until the result falls outside of a
		/// threshold. Therefore, rather than returning null, all calculators will do the best they can and set this property to False if they think the
		/// result is unworthwhile for display in the result set.</remarks>
		public bool Recommended { get; private set; }
		
		/// <summary>
		/// An amortization table or other tabular result, usually displayed in landscape view
		/// </summary>
		public TabularResult Table { get; private set; }
	}
}

