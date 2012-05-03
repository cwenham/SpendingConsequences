using System;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace SpendingConsequences
{
	/// <summary>
	/// Represent a string formatted with 'Markish' - a subset of Markdown
	/// </summary>
	/// <remarks>We want to use Markdown syntax for some basic formatting, but we don't need support for the entire
	/// Markdown spec. 'Markish' will only support bold, italic, and maybe some other formatting syntax. This class's
	/// main purpose is to expose a ToNSAttributedString() method to convert Markish to something we can display with
	/// CoreText.</remarks>
	public class MarkishString
	{
		public MarkishString (string markish)
		{
			this.MarkishSource = markish;
		}
		
		public string MarkishSource { get; private set; }
	}
}

