using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreText;
using MonoTouch.CoreGraphics;

namespace SpendingConsequences.Calculators
{
	/// <summary>
	/// Represent a Commentary element on a consequence calculator
	/// </summary>
	/// <remarks>This class's primary job is to expose a ToAttributedString() method so that formatted text can
	/// be displayed with CoreText.</remarks>
	public class Commentary : ASupportElement
	{
		public Commentary (XElement definition) : base (definition)
		{
		}
		
		public NSAttributedString ToAttributedString (UIFont baseFont)
		{
			if (_asAttributedString == null) {
				CTStringAttributes attrs = new CTStringAttributes ();
				attrs.Font = new CTFont(baseFont.Name, baseFont.PointSize);
				attrs.ForegroundColor = UIColor.Black.CGColor;
				
				_asAttributedString = IterateChildNodes (Definition, attrs);
			}
			
			return _asAttributedString;
		}
		
		private NSAttributedString IterateChildNodes (XElement element, CTStringAttributes baseAttribs)
		{
			NSMutableAttributedString mutString = new NSMutableAttributedString ("");
			foreach (var child in element.Nodes()) {
				NSAttributedString formatted = NodeToAttributedString (child, baseAttribs);
				if (formatted != null)
					mutString.Append (formatted);
			}
			return mutString;
		}
		
		private NSAttributedString NodeToAttributedString (XNode node, CTStringAttributes baseAttribs)
		{			
			CTStringAttributes newAttribs;
			switch (node.NodeType) {
			case XmlNodeType.Text:
				string text = ((XText)node).Value;
				text = r_findExcessWhitespace.Replace (text, " ");
				return new NSAttributedString (text, baseAttribs);
			case XmlNodeType.Element:
				switch (((XElement)node).Name.LocalName) {
				case "b":
					newAttribs = new CTStringAttributes ();
					newAttribs.Font = Bolded (baseAttribs.Font);
					return IterateChildNodes ((XElement)node, newAttribs);
				case "i":
					newAttribs = new CTStringAttributes ();
					newAttribs.Font = Italicized (baseAttribs.Font);
					return IterateChildNodes ((XElement)node, newAttribs);
				case "br":
					return new NSAttributedString("\n", baseAttribs);
				default:
					break;
				}
				break;
			}		
			return null;
		}
		
		private Regex r_findExcessWhitespace = new Regex(@"(\s{2,}|\n|\t)", RegexOptions.Compiled);
		
		private CTFont Bolded (CTFont reference)
		{
			return FindMatchingFont (reference, CTFontSymbolicTraits.Bold);
		}
		
		private CTFont Italicized (CTFont reference)
		{
			return FindMatchingFont (reference, CTFontSymbolicTraits.Italic);
		}
		
		private CTFont FindMatchingFont (CTFont reference, CTFontSymbolicTraits trait)
		{
			CTFontSymbolicTraits symbolic;
			CTFontTraits traits = reference.GetTraits ();
			if (traits.SymbolicTraits.HasValue)
				symbolic = traits.SymbolicTraits.Value | trait;
			else
				symbolic = trait;
			
			CTFont newFont = reference.WithSymbolicTraits (reference.Size, symbolic, symbolic);
			if (newFont != null)
				return newFont;
			else
				return reference;
		}
		
		
		private NSAttributedString _asAttributedString = null;
	}
}

