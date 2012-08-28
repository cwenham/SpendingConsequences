using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	/// <summary>
	/// Represent an XML document that configures the application's behavior
	/// </summary>
	public class SubProfile
	{
		public static String LibraryFolder
		{
			get {
				if (String.IsNullOrWhiteSpace(_libraryFolder))
					_libraryFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library");

				return _libraryFolder;
			}
		}
		private static String _libraryFolder;

		public static String InboxFolder
		{
			get {
				if (String.IsNullOrWhiteSpace(_inboxFolder))
					_inboxFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Inbox");

				return _inboxFolder;
			}
		}
		private static String _inboxFolder;

		/// <summary>
		/// Create a new empty profile and save it to the LibraryFolder
		/// </summary>
		/// <param name='name'>
		/// Name of profile. Will be used to name the file
		/// </param>
		public static SubProfile Create (String name)
		{
			XDocument definition = new XDocument(new XElement(NS.Profile + "Calculators"));
			definition.Root.Add(new XElement(NS.Profile + "Consequences"));

			String filePath = Path.Combine(LibraryFolder,String.Format("{0}.sbprofile", name));
			definition.Save(filePath);

			return SubProfile.Load(filePath);
		}

		public static SubProfile Load (String uri)
		{
			XDocument definition = XDocument.Load (uri);
			return new SubProfile (definition);
		}
		
		private SubProfile (XDocument profileDoc)
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
		
		public List<ACalculator> Calculators { get; private set; }
		
		public Dictionary<String, XElement> ResultTemplates { get; private set; }
		
		public Dictionary<String, XElement> ConsequenceTemplates { get; private set; }
	}
}

