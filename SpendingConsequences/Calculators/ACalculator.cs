using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using MonoTouch.UIKit;

using ETFLib;
using ETFLib.Composition;

namespace SpendingConsequences.Calculators
{
	public abstract class ACalculator : AComposable
	{
		// Default thresholds for selection. InitialAmmounts or results that are outside this threshold will exclude this
		// calculator from the result set.
		private const decimal DEFAULT_LOWER_THRESHOLD = 0.01m;
		private const decimal DEFAULT_UPPER_THRESHOLD = decimal.MaxValue;
		private const decimal DEFAULT_LOWER_RESULT_LIMIT = 0.01m;
		private const decimal DEFAULT_UPPER_RESULT_LIMIT = decimal.MaxValue;
		
		private static TriggerType[] RepeatingModes = new TriggerType[] {
			TriggerType.PerDay,
			TriggerType.PerMonth,
			TriggerType.PerQuarter,
			TriggerType.PerWeek,
			TriggerType.PerYear
		};
		
		public ACalculator (XElement definition) : base(definition)
		{			
			ConfigurableValues = Definition.Elements (NS.Composition + "Configurable")
				.Select (x => new ConfigurableValue (x))
				.ToDictionary (x => x.Name);
			
			SupportElements = (from e in Definition.Elements()
			                   where ASupportElement.KnownElementNames().Contains(e.Name.LocalName)
			                   select ASupportElement.GetInstance(e)).ToDictionary(x => x.Name);
		}
		
		/// <summary>
		/// Configuration values, such as cost, interest rate, etc. used by calculator
		/// </summary>
		public Dictionary<String,ConfigurableValue> ConfigurableValues { get; private set; }
		
		/// <summary>
		/// Support elements, such as commentary, etc.
		/// </summary>
		public Dictionary<String,ASupportElement> SupportElements { get; private set; }

		/// <summary>
		/// True if the user can edit labels, commentary and icons within the app
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>			
		public bool IsUserEditable { 
			get {
				if (OriginalProfile != null)
					return OriginalProfile.IsUserEditable;

				return true;
			}
		}

		public SubProfile OriginalProfile {
			get {
				return this.Definition.Document.Annotation<SubProfile>();
			}
		}

		public String Caption {
			get {
				return AttributeOrNull("Caption");
			}
			set {
				if (Definition.Attribute("Caption") != null)
					Definition.Attribute("Caption").Value = value;
				else
					Definition.Add (new XAttribute("Caption", value));
			}
		}
		
		public TriggerType[] TriggersOn {
			get {
				if (_TriggersOn == null && Definition.Attribute ("TriggersOn") != null) {
					string triggerList = Definition.Attribute ("TriggersOn").Value;
					string[] components = triggerList.Split (',');
					_TriggersOn = components.Select(x => (TriggerType)Enum.Parse (typeof(TriggerType), x, true)).ToArray();
				}
				
				return _TriggersOn;					
			}
		}
		private TriggerType[] _TriggersOn = null;
		
		/// <summary>
		/// Return the gender the consequence definition is limited to
		/// </summary>
		public Gender ForGender {
			get {
				if (!_genderWasSet) {
					if (Definition.Attribute ("ForGender") != null)
						Enum.TryParse (Definition.Attribute ("ForGender").Value, true, out _forGender);
					else
						_forGender = Gender.Unspecified;
					
					_genderWasSet = true;
				}			
				
				return _forGender;
			}
		}
		private Gender _forGender = Gender.Unspecified;
		private bool _genderWasSet = false;
		
		/// <summary>
		/// Return a country-code if the consequence definition is regionally limited
		/// </summary>
		public string Country {
			get {
				return AttributeOrNull("Country");
			}
		}

		public string Category {
			get {
				return AttributeOrNull("Category");
			}
		}
		
		public bool WillTriggerOn (TriggerType mode)
		{
			// We need to force the AOT compiler to include the Contains() method in the pre-compiled binary to avoid 
			// ExecutionEngineExceptions on the device.
			// See http://docs.xamarin.com/ios/troubleshooting#System.ExecutionEngineException.3a_Attempting_to_JIT_compile_method_(wrapper_managed-to-managed)_Foo.5b.5d.3aSystem.Collections.Generic.ICollection.601.get_Count_()
			if (((ICollection<TriggerType>)TriggersOn).Contains (mode))
				return true;
			
			if (((ICollection<TriggerType>)RepeatingModes).Contains (mode))
				return TriggersOn.Contains (TriggerType.Repeating);
			
			return false;
		}
		
		public string TableTemplate {
			get {
				return AttributeOrNull("TableTemplate");
			}
		}
		
		/// <summary>
		/// The lowest InitialAmount that this calculator will work for
		/// </summary>
		/// <value>
		/// The lower threshold.
		/// </value>
		public decimal LowerThreshold {
			get {
				if (_lowerThreshold == 0m)
					if (Definition.Attribute ("LowerThreshold") != null) {
						if (!decimal.TryParse (Definition.Attribute ("LowerThreshold").Value, out _lowerThreshold))
							_lowerThreshold = DEFAULT_LOWER_THRESHOLD;
					} else
						_lowerThreshold = DEFAULT_LOWER_THRESHOLD;
				
				return _lowerThreshold;
			}
		}
		private decimal _lowerThreshold = 0m;
		
		/// <summary>
		/// The highest InitialAmount that this calculator will work for
		/// </summary>
		/// <value>
		/// The upper threshold.
		/// </value>
		public decimal UpperThreshold {
			get {
				if (_upperThreshold == 0m)
					if (Definition.Attribute ("UpperThreshold") != null) {
						if (!decimal.TryParse (Definition.Attribute ("UpperThreshold").Value, out _upperThreshold))
							_upperThreshold = DEFAULT_UPPER_THRESHOLD;
					} else
						_upperThreshold = DEFAULT_UPPER_THRESHOLD;
				
				return _upperThreshold;				
			}
		}
		private decimal _upperThreshold = 0m;
		
		/// <summary>
		/// The lowest result value that's worth displaying
		/// </summary>
		public decimal LowerResultLimit {
			get {
				if (_lowerResultLimit == 0m)
					if (Definition.Attribute ("LowerResultLimit") != null) {
						if (!decimal.TryParse (Definition.Attribute ("LowerResultLimit").Value, out _lowerResultLimit))
							_lowerResultLimit = DEFAULT_LOWER_RESULT_LIMIT;
					} else
						_lowerResultLimit = DEFAULT_LOWER_RESULT_LIMIT;
				
				return _lowerResultLimit;	
			}
		}
		private decimal _lowerResultLimit = 0m;
		
		/// <summary>
		/// The highest result value that's worth displaying
		/// </summary>
		/// <remarks>If a calculator's result exceeds this value, it will return null. Should be used when the results
		/// would not be useful, eg: more than 12 inkjet cartridges per year for a home user.</remarks>
		public decimal UpperResultLimit {
			get {
				if (_upperResultLimit == 0m)
					if (Definition.Attribute ("UpperResultLimit") != null) {
						if (!decimal.TryParse (Definition.Attribute ("UpperResultLimit").Value, out _upperResultLimit))
							_upperResultLimit = DEFAULT_UPPER_RESULT_LIMIT;
					} else
						_upperResultLimit = DEFAULT_UPPER_RESULT_LIMIT;
				
				return _upperResultLimit;	
			}
		}
		private decimal _upperResultLimit = 0m;

		public virtual Image Image {
			get {
				if (_image == null)
				{
					var imageElement = Definition.Element (NS.Composition + "Image");
					if (imageElement != null)
						_image = new Image(imageElement);
				}

				return _image;
			}
		}
		private Image _image;
		
		/// <summary>
		/// The comments for this calculator, before any formatting
		/// </summary>
		/// <value>
		/// The unformatted commentary.
		/// </value>
		public String UnformattedCommentary {
			get {
				var commentaryElement = Definition.Element (NS.Composition + "Commentary");
				if (commentaryElement != null)
					return commentaryElement.Value;
				else return null;
			}
		}

		public int SortOrder {
			get {
				string givenOrder = AttributeOrNull("SortOrder");
				if (givenOrder != null)
				{
					int order = int.MaxValue;
					if (int.TryParse(givenOrder, out order))
						return order;
				}

				return this.Definition.ElementsBeforeSelf().Count();
			}
			set {
				if (this.Definition.Attribute("SortOrder") != null)
					this.Definition.Attribute("SortOrder").Value = value.ToString();
				else
					this.Definition.Add(new XAttribute("SortOrder", value));
			}
		}
		
		public string FormatCaption (string caption, Dictionary<string, string> values)
		{
			string formatted = caption;
			foreach (string key in values.Keys)
				formatted = formatted.Replace (string.Format ("[{0}]", key), values [key]);
			return formatted;
		}
		
		public abstract ConsequenceResult Calculate (ConsequenceRequest request);
		
		/// <summary>
		/// Dynamically generated tabular data
		/// </summary>
		/// <remarks>Any calculator that returns dynamic tabular data, such as amortization tables, should override this and pass a TabularResult to the
		/// ConsequenceResult they return.</remarks>
		public virtual XElement GetTableData(ConsequenceResult result)
		{
			return null;
		}
		
		#region Static helpers
		
		public static decimal PercentAsDecimal (decimal percent)
		{
			return percent / 100;
		}
		
		public static decimal InvestmentsPerYear (TriggerType trigger)
		{
			switch (trigger) {
			case TriggerType.PerDay:
				return 365.25m;
			case TriggerType.PerMonth:
				return 12m;
			case TriggerType.PerQuarter:
				return 4m;
			case TriggerType.PerWeek:
				return 52m;
			case TriggerType.PerYear:
				return 1m;
			default:
				return 1m;
			}
		}
		
		public static int CompoundingsPerYear (string Compounding) 
		{
				switch (Compounding) {
				case "Monthly":
					return 12;
				case "Weekly":
					return 52;
				case "Annually":
					return 1;
				case "Quarterly":
					return 4;
				default:
					return 12;
				}
		}		
		#endregion	
	}
	
	public enum TriggerType
	{
		Undefined,
		All,
		OneTime,
		Repeating,
		PerDay,
		PerWeek,
		PerMonth,
		PerQuarter,
		PerYear
	}
	
	public enum Gender
	{
		Unspecified,
		Male,
		Female
	}
}

