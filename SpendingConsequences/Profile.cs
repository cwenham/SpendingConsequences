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
			this.Definition.Changed += delegate(object sender, XObjectChangeEventArgs e) {
				Console.WriteLine("Received XDocument changed event: {0}", e.ObjectChange.ToString());
			};
			
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
		
		public static UIImage GetImage (Image image)
		{
			if (ImageCache == null)
				ImageCache = new NSCache ();
			
			NSObject key = NSObject.FromObject (image.Name);
			UIImage img = ImageCache.ObjectForKey (key) as UIImage;
			if (img == null) {
				if (NSFileManager.DefaultManager.FileExists (image.ImagePath)) {
					img = UIImage.FromBundle (image.ImagePath);
					ImageCache.SetObjectforKey (img, key);	
				}
				else
					Console.WriteLine("Couldn't find image for {0}", image.ImagePath);
			}
			return img;
		}
		
		/// <summary>
		/// Cache of artwork
		/// </summary>
		private static NSCache ImageCache { get; set; }
		
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

