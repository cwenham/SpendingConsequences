using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	/// <summary>
	/// Represent an XML document that configures the application's behavior
	/// </summary>
	public class Profile
	{
		public static Profile Load (string uri)
		{
			XDocument definition = XDocument.Load (uri);
			return new Profile (definition);
		}
		
		private Profile (XDocument profileDoc)
		{
			this.Definition = profileDoc;
			
			var calculators = from e in Definition.Root.Element (NS.Profile + "Consequences").Elements ()
					where e.Name.Namespace == NS.Profile
				&& ACalculator.CalcType (e) != null
						select ACalculator.GetInstance (e);
			if (calculators != null)
				this.Calculators = calculators.ToList ();
				
			ResultTemplates = Definition.Root.Element (NS.Profile + "ResultTemplates").Elements ()
					.Where (x => x.Attribute ("Name") != null)
					.ToDictionary (x => x.Attribute ("Name").Value, y => y.Element (NS.XSLT + "stylesheet"));
				
			ConsequenceTemplates = Definition.Root.Element (NS.Profile + "ConsequenceTemplates").Elements ()
					.Where (x => x.Attribute ("Name") != null)
					.ToDictionary (x => x.Attribute ("Name").Value);
		}
		
		public XDocument Definition { get; private set; }
		
		public UIImage GetImage (string imageName)
		{
			if (ImageCache == null)
				ImageCache = new NSCache ();
			
			NSObject key = NSObject.FromObject (imageName);
			UIImage image = ImageCache.ObjectForKey (key) as UIImage;
			if (image == null) {
				string filename = string.Format ("Artwork/{0}.png", imageName);
				if (NSFileManager.DefaultManager.FileExists (filename)) {
					image = UIImage.FromBundle (filename);
					ImageCache.SetObjectforKey (image, key);	
				}
			}
			return image;
		}
		
		/// <summary>
		/// Cache of artwork
		/// </summary>
		private NSCache ImageCache { get; set; }
		
		public List<ACalculator> Calculators { get; private set; }
		
		public XElement GetResultTemplate (string name)
		{
			if (ResultTemplates.ContainsKey (name))
				return ResultTemplates [name];
			else
				return null;
		}
		
		private Dictionary<string, XElement> ResultTemplates { get; set; }
		
		public Dictionary<string, XElement> ConsequenceTemplates { get; private set; }
	}
}

