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
		public TabularResult (string title, string unformattedCaption, ACalculator calc)
		{
			this.Title = title;
			this.UnformattedCaption = unformattedCaption;
			this.OriginatingCalculator = calc;
		}
		
		public TabularResult (string title, string unformattedCaption, ACalculator calc, XElement data)
		{
			this.Title = title;
			this.UnformattedCaption = unformattedCaption;
			this.OriginatingCalculator = calc;
			this.Data = data;
		}
		
		public string Title { get; private set; }
		
		public string UnformattedCaption { get; private set; }
		
		public ACalculator OriginatingCalculator { get; private set; }
		
		public ConsequenceResult MainResult { get; internal set; }
		
		private XElement Data { get; set; }
		
		public XElement GetData ()
		{
			if (Data != null)
				return Data;
			
			return OriginatingCalculator.GetTableData (MainResult);
		}
	}
}

