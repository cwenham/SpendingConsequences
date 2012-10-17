using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using ETFLib;
using ETFLib.Composition;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	/// <summary>
	/// Represent an XML document that configures the application's behavior
	/// </summary>
	public class SubProfile : ACompositionTree
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
			XDocument definition = new XDocument(new XElement(NS.Composition + "Calculators"));
			definition.Root.Add(new XElement(NS.Composition + "Consequences"));

			String filePath = Path.Combine(LibraryFolder,String.Format("{0}.sbprofile", name));
			definition.Save(filePath);

			return ACompositionTree.Load<SubProfile>(filePath);
		}
		
		public SubProfile (XDocument profileDoc) : base(profileDoc)
		{
			var consequencesElement = Definition.Root.Element (NS.Composition + "Consequences");
			if (consequencesElement != null) {
				var calculators = from e in consequencesElement.Elements ()
					where e.Name.Namespace == NS.Composition
					&& AComposable.ComposableType (e) != null
						select ACalculator.GetInstance (e);
				if (calculators != null)
					this.Calculators = calculators.Cast<ACalculator>().ToList();
			}
				
			var resultTemplatesElement = Definition.Root.Element (NS.Composition + "ResultTemplates");

			if (resultTemplatesElement != null)
				ResultTemplates = resultTemplatesElement.Elements ()
						.Where (x => x.Attribute ("Name") != null)
						.ToDictionary (x => x.Attribute ("Name").Value, y => y.Element (NS.XSLT + "stylesheet"));
		}

		public ACalculator AddConsequenceFromDefinition(XElement definition)
		{
			XElement calcContainer = this.Definition.Root.Element(NS.Composition + "Consequences");
			if (calcContainer == null)
			{
				calcContainer = new XElement(NS.Composition + "Consequences");
				this.Definition.Root.Add(calcContainer);
			}

			calcContainer.Add(definition);
			ACalculator calc = AComposable.GetInstance<ACalculator>(definition);

			return calc;
		}

		public Boolean IsUserEditable {
			get {
				if (!_isUserEditableSet)
				{
					var editAttribute = this.Definition.Root.Attribute("UserEditable");
					if (editAttribute != null)
						Boolean.TryParse(editAttribute.Value, out _isUserEditable);

					_isUserEditableSet = true;
				}

				return _isUserEditable;
			}
		}
		private Boolean _isUserEditable = true;
		private Boolean _isUserEditableSet = false;
		
		public List<ACalculator> Calculators { get; private set; }
		
		public Dictionary<String, XElement> ResultTemplates { get; private set; }
		
		public IEnumerable<Template> ConsequenceTemplates { 
			get {
				if (_consequenceTemplateContainer == null && this.Root != null)
					_consequenceTemplateContainer = this.Root.GetChild<AComposable>("ConsequenceTemplates");
				if (_consequenceTemplateContainer != null)
					return _consequenceTemplateContainer.Children.OfType<Template>();
				else
					return null;
			}
		}
		private AComposable _consequenceTemplateContainer;
	}
}

