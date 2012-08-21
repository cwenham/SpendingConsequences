using System;
using System.Xml.Linq;

using MonoTouch;
using MonoTouch.Foundation;

namespace SpendingConsequences.Calculators
{
	public class Image : ASupportElement
	{
		public Image (XElement definition) : base (definition)
		{
		}

		public ImageType Type {
			get {
				string sType = Definition.Attribute ("Type") != null ? Definition.Attribute ("Type").Value : "Artwork";
				ImageType output;
				if (Enum.TryParse(sType,true,out output))
					return output;
				else
					return ImageType.Artwork;
			}
		}

		public string ImagePath {
			get {
				switch (Type) {
				case ImageType.Artwork:
					return string.Format ("Artwork/{0}.png", this.Name);
				case ImageType.CurrencyFlag:
					return string.Format ("CurrencyFlags/{0}.png", this.Name);
				default:
					return this.Name;
				}
			}
		}
	}

	public enum ImageType {
		Artwork,
		CurrencyFlag,
		UserPhoto
	}
}

