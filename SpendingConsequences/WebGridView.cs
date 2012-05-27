using System;
using System.Drawing;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public partial class WebGridView : UIViewController
	{
		public WebGridView (Profile profile) : base ("WebGridView", null)
		{
			this.Profile = profile;
			if (TransformCache == null)
				TransformCache = new Dictionary<string, XslCompiledTransform>();
		}
		
		private Profile Profile { get; set; }
		
		public ConsequenceResult CurrentResult { get; private set; }
		
		private BackgroundWorker resultWorker;
		
		public void SetResult (ConsequenceResult result)
		{
			if (CurrentResult == result)
				return;
			
			CurrentResult = result;	
			
			if (resultWorker != null && resultWorker.IsBusy)
				resultWorker.CancelAsync ();
			
			resultWorker = new BackgroundWorker ();
			resultWorker.WorkerSupportsCancellation = true;
			resultWorker.RunWorkerCompleted += HandleRunWorkerCompleted;
			resultWorker.DoWork += HandleDoWork;
			resultWorker.RunWorkerAsync (result);
		}

		void HandleDoWork (object sender, DoWorkEventArgs e)
		{
			BackgroundWorker myWorker = sender as BackgroundWorker;
			ConsequenceResult result = e.Argument as ConsequenceResult;
			
			string tableTemplateName = result.Calculator.TableTemplate;
			if (tableTemplateName == null)
				return; // ToDo: Maybe throw exception so parent view can abort the transition
			
			if (result.Table == null)
				return;
			
			XElement data = result.Table.GetData ();
			
			if (data == null)
				return; // ToDo: Throw exception?
			
			XslCompiledTransform transformer;
			
			if (!TransformCache.ContainsKey (tableTemplateName)) {
				XElement tableTemplate = Profile.GetResultTemplate (tableTemplateName);
			
				if (tableTemplate == null)
					return; // ToDo: Maybe throw exception so parent view can abort the transition
			
				transformer = new XslCompiledTransform ();
				XmlReader tmplReader = tableTemplate.CreateReader ();
				transformer.Load (tmplReader);		
				
				TransformCache.Add (tableTemplateName, transformer);
			} else
				transformer = TransformCache [tableTemplateName];
			
			StringBuilder htmlBuilder = new StringBuilder ();
			if (!myWorker.CancellationPending) {
				XmlReader datReader = data.CreateReader ();
				XmlWriter writer = XmlWriter.Create (htmlBuilder);
				transformer.Transform (datReader, writer);
				writer.Flush ();
			}
			
			if (!myWorker.CancellationPending)
				e.Result = htmlBuilder.ToString ();
		}

		void HandleRunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e)
		{
			DisplayedHTML = e.Result as String;
			if (this.webView != null && DisplayedHTML != null)
				this.webView.LoadData (DisplayedHTML, "application/xhtml+xml", "utf-8", BaseURL);
		}
		
		private string DisplayedHTML { get; set; }
		
		private static NSUrl BaseURL = NSUrl.FromString("http://spentbetter.com/");
		
		private static Dictionary<string, XslCompiledTransform> TransformCache { get; set; }
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
			TransformCache.Clear ();
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

