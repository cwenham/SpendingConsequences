using System;
using System.Drawing;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Collections.Generic;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public partial class WebGridView : UIViewController
	{
		public WebGridView () : base ("WebGridView", null)
		{
		}
		
		public ConsequenceResult CurrentResult { get; private set; }
		
		public void SetResult (ConsequenceResult result)
		{
			if (CurrentResult == result)
				return;
			
			CurrentResult = result;			
			
			string tableTemplateName = result.Calculator.TableTemplate;
			if (tableTemplateName == null)
				return; // ToDo: Maybe throw exception so parent view can abort the transition
			
			XElement tableTemplate = SpendingConsequencesViewController.ResultTemplates.ContainsKey (tableTemplateName) ?
				SpendingConsequencesViewController.ResultTemplates [tableTemplateName] : null;
			
			if (tableTemplate == null)
				return; // ToDo: Maybe throw exception so parent view can abort the transition
			
			XElement data = result.Table.GetData ();
			
			if (data == null)
				return; // ToDo: Throw exception?
			
			Transform = new XslCompiledTransform ();
			XmlReader tmplReader = tableTemplate.CreateReader ();
			Transform.Load (tmplReader);
			
			XmlReader datReader = data.CreateReader ();
			StringBuilder htmlBuilder = new StringBuilder ();
			XmlWriter writer = XmlWriter.Create (htmlBuilder);
			Transform.Transform (datReader, writer);
			writer.Flush ();
			
			DisplayedHTML = htmlBuilder.ToString ();
			if (this.webView != null)
				this.webView.LoadData (DisplayedHTML, "application/xhtml+xml", "utf-8", BaseURL);
		}
		
		private string DisplayedHTML { get; set; }
		
		private static NSUrl BaseURL = NSUrl.FromString("http://spentbetter.com/");
		
		private XslCompiledTransform Transform { get; set; }
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.
			if (DisplayedHTML != null)
				this.webView.LoadData (DisplayedHTML, "application/xhtml+xml", "utf-8", BaseURL);
		}
		
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;
			
			ReleaseDesignerOutlets ();
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return (toInterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || toInterfaceOrientation == UIInterfaceOrientation.LandscapeRight);
		}
	}
}

