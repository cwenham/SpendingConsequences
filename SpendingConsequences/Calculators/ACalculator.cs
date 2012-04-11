using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using MonoTouch.UIKit;

namespace SpendingConsequences.Calculators
{
	public abstract class ACalculator
	{
		public ACalculator (XElement definition)
		{
			this.Definition = definition;
			
			ConfigurableValues = Definition.Elements ("Configurable")
				.Select (x => new ConfigurableValue (x))
				.ToDictionary(x => x.Name);
		}
		
		public XElement Definition { get; private set; }
		
		public Dictionary<String,ConfigurableValue> ConfigurableValues { get; private set; }
		
		public String Caption {
			get {
				if (Definition.Attribute ("Caption") != null)
					return Definition.Attribute ("Caption").Value;
				else
					return null;
			}
		}
		
		public TriggerType TriggersOn {
			get {
				if (_TriggersOn == TriggerType.Undefined && Definition.Attribute ("TriggersOn") != null)
					_TriggersOn = ((TriggerType)Enum.Parse (typeof(TriggerType), Definition.Attribute ("TriggersOn").Value, true));
				
				return _TriggersOn;					
			}
		}
		private TriggerType _TriggersOn = TriggerType.Undefined;
		
		public String ImageName {
			get {
				var imageElement = Definition.Element ("Image");
				if (imageElement != null && imageElement.Attribute ("Name") != null)
					return imageElement.Attribute ("Name").Value;
				else
					return null;
			}
		}
		
		public String UnformattedCommentary {
			get {
				var commentaryElement = Definition.Element ("Commentary");
				if (commentaryElement != null)
					return commentaryElement.Value;
				else return null;
			}
		}
		
		public string FormatCaption (string caption, Dictionary<string, string> values)
		{
			string formatted = caption;
			foreach (string key in values.Keys)
				formatted = formatted.Replace (string.Format ("[{0}]", key), values [key]);
			return formatted;
		}
		
		public virtual string ResultFormat {
			get {
				return "{0:C}";
			}
		}
		
		public abstract ConsequenceResult Calculate (ConsequenceRequest request);
		
		#region Static helpers
		
		private static Dictionary<string, Type> _calcTypes { get; set; }
		
		public static Type CalcType (XElement definition)
		{
			if (_calcTypes == null)
				_calcTypes = (from t in Assembly.GetCallingAssembly ().GetTypes ()
				              where t.IsSubclassOf (typeof(ACalculator))
				              select new {
					name = t.Name,
					typ = t
				}).ToDictionary (x => x.name, y => y.typ);
			
			if (_calcTypes.ContainsKey (definition.Name.LocalName))
				return _calcTypes [definition.Name.LocalName];
			else
				return null;
		}
		
		public static ACalculator GetInstance (XElement definition)
		{
			Type calcType = CalcType (definition);
			if (calcType != null) {
				if (definition.Annotation (calcType) != null)
					return definition.Annotation (calcType) as ACalculator;
				else {
					var instance = Activator.CreateInstance (calcType, new object[] { definition });
					definition.AddAnnotation (instance);
					return instance as ACalculator;
				}
			} else
				return null;
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
}

