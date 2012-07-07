using System;


namespace SpendingConsequences.Calculators
{
	/// <summary>
	/// Wrapper for physical units
	/// </summary>
	public class Units
	{
		public Units (int units)
		{
			this.Value = units;
		}
		
		public Units (double units)
		{
			this.Value = (decimal)units;
		}
		
		public Units (decimal units)
		{
			this.Value = units;
		}
		
		public decimal Value { get; private set; }
		
		public override string ToString ()
		{
			if (Value % 1 <= 0.1m)
				return string.Format ("{0:0}", Value);
			else 
				return string.Format ("{0:0.0}", Value);
		}
	}
}

