using System;
namespace SpendingConsequences
{
	/// <summary>
	/// Represent a message to the user, displayable in UI, that the results of a calculation are bigger than Double.MaxValue
	/// </summary>
	public class OverflowMessage
	{
		public OverflowMessage ()
		{
		}
		
		public override string ToString ()
		{
			return "Eeep! Too big to calculate!";
		}
	}
}

