using System;
using System.Linq;
using System.Xml.Linq;

namespace SpendingConsequences.Calculators
{
	public class ConfigurableValue
	{
		public ConfigurableValue (XElement definition)
		{
			this.Definition = definition;
		}
		
		public XElement Definition { get; private set; }
		
		public String Name {
			get {
				return Definition.Attribute ("Name") != null ? Definition.Attribute ("Name").Value : "Value";
			}
		}
		
		public String Label {
			get {
				return Definition.Attribute ("Label") != null ? Definition.Attribute ("Label").Value : "Label";
			}
		}
		
		public String ID {
			get {
				return Definition.Attribute("ID") != null ? Definition.Attribute("ID").Value : null;
			}
		}
		
		public decimal MinValue {
			get {
				if (Definition.Attribute ("MinValue") != null) {
					decimal minVal = 0;
					if (!decimal.TryParse (Definition.Attribute ("MinValue").Value, out minVal))
						return 0;
					else
						return minVal;
				} else
					return 0;
			}
		}
		
		public decimal MaxValue {
			get {
				if (Definition.Attribute ("MaxValue") != null) {
					decimal maxVal = 0;
					if (!decimal.TryParse (Definition.Attribute ("MaxValue").Value, out maxVal))
						return decimal.MaxValue;
					else
						return maxVal;
				} else
					return decimal.MaxValue;
			}
		}
		
		public decimal StepValue {
			get {
				if (Definition.Attribute ("StepValue") != null) {
					decimal stepVal = 0;
					if (!decimal.TryParse (Definition.Attribute ("StepValue").Value, out stepVal))
						return 0.01m;
					else
						return stepVal;
				} else
					return 0.01m;
			}
		}

		public string CurrencyCode {
			get {
				return Definition.Attribute("CurrencyCode") != null ? Definition.Attribute("CurrencyCode").Value : null;
			}
		}
		
		/// <summary>
		/// Returns true if a UI should not be presented for changing the value
		/// </summary>
		/// <remarks>Used for values that may be computed from other sources, fetched from web services, etc.</remarks>
		public bool SuppressUI {
			get {
				bool _suppress = false;
				if (Definition.Attribute ("SuppressUI") != null)
					bool.TryParse (Definition.Attribute ("SuppressUI").Value, out _suppress);
				return _suppress;
			}
		}
		
		public ConfigurableValueType ValueType {
			get {
				if (_ValueType == ConfigurableValueType.Undefined && Definition.Attribute ("Type") != null)
					_ValueType = ((ConfigurableValueType)Enum.Parse (typeof(ConfigurableValueType), Definition.Attribute ("Type").Value, true));
				
				return _ValueType;
			}
		}
		private ConfigurableValueType _ValueType = ConfigurableValueType.Undefined;
		
		private static UserCalculatorSettings UserSettings = new UserCalculatorSettings();
		
		public static void CloseResources ()
		{
			if (UserSettings != null)
				UserSettings.CloseConnection ();
		}
		
		public object Value {
			get {
				if (_value == null) {
					if (ValueType == ConfigurableValueType.Money)
						_value = UserSettings.GetCustomValue (this) as Money;
					else
						_value = UserSettings.GetCustomValue (this);
					if (_value == null)
						_value = Definition.Attribute ("Value").Value;
				}

				if (_value is string && ValueType != ConfigurableValueType.String)
				{
					switch (ValueType) {
					case ConfigurableValueType.Decimal:
						_value = decimal.Parse (_value as String);
						break;
					case ConfigurableValueType.Integer:
						_value = int.Parse (_value as string);
						break;
					case ConfigurableValueType.Money:
						_value = new Money(decimal.Parse (_value as string), CurrencyCode);
						break;
					case ConfigurableValueType.Percentage:
						_value = decimal.Parse (_value as string);
						break;
					case ConfigurableValueType.Year:
						_value = int.Parse (_value as string);
						break;
					case ConfigurableValueType.Months:
						_value = int.Parse (_value as string);
						break;
					case ConfigurableValueType.Currency:
						_value = _value as String ?? "USD";
						break;
					case ConfigurableValueType.PayoffMode:
						PayoffMode mode;
						if (!Enum.TryParse (_value as string, out mode))
							mode = PayoffMode.PercentPlusInterest;
						_value = mode;
						break;
					}
				}

				return _value;
			}
			set {
				_value = value;
				if (value != null)
					if (value is Money)
						UserSettings.StoreCustomValue(this, value as Money);
					else
					    UserSettings.StoreCustomValue (this, value.ToString ());

				ValueChanged(this, new EventArgs());
			}
		}
		private object _value = null;

		public event EventHandler<EventArgs> ValueChanged = delegate {};
	}
	
	public enum ConfigurableValueType
	{
		Undefined,
		Integer,
		Decimal,
		Money,
		Year,
		Percentage,
		Months,
		PayoffMode,
		String,
		Currency
	}
}

