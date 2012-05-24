using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	/// <summary>
	/// Represent amortization tables and other tabular results, pre-rendered or dynamically rendered
	/// </summary>
	public class TabularResult
	{
		public TabularResult (string title, string unformattedCaption, ACalculator calc, List<string> headers)
		{
			this.Title = title;
			this.UnformattedCaption = unformattedCaption;
			this.OriginatingCalculator = calc;
			this.Headers = headers;
		}
		
		public TabularResult (string title, string unformattedCaption, ACalculator calc, List<string> headers, List<List<string>> data)
		{
			this.Title = title;
			this.UnformattedCaption = unformattedCaption;
			this.OriginatingCalculator = calc;
			this.Headers = headers;
			this.TableData = data;
		}
		
		public string Title { get; private set; }
		
		public string UnformattedCaption { get; private set; }
		
		public ACalculator OriginatingCalculator { get; private set; }
		
		public ConsequenceResult MainResult { get; internal set; }
		
		public List<string> Headers { get; private set; }
		
		private List<List<string>> TableData { get; set; }
		
		public List<List<string>> GetData ()
		{
			if (TableData != null)
				return TableData;
			
			return OriginatingCalculator.GetTableData (MainResult);
		}
	}
}

