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
			set {
				if (Definition.Attribute("Label") != null)
					Definition.Attribute("Label").Value = value;
				else
					Definition.Add(new XAttribute("Label", value));
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
			set {
				if (Definition.Attribute("CurrencyCode") != null)
					Definition.Attribute("CurrencyCode").Value = value;
				else
					Definition.Add(new XAttribute("CurrencyCode", value));
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
		
		public object Value {
			get {
				if (_value == null)
				{
					if (Definition.Attribute("Value") != null)
						_value = Definition.Attribute("Value").Value;
					else
						_value = Definition.Value;

					if (ValueType == ConfigurableValueType.Money && _value != null)
					{
						decimal amount = 0;
						if (Decimal.TryParse(_value as String, out amount))
							_value = Money.NewMoney(amount, this.CurrencyCode);
					}
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
				{
					this.Definition.Value = ((Money)_value).Value.ToString();
					this.CurrencyCode = ((Money)_value).CurrencyCode;
				}
					else
					    this.Definition.Value = _value.ToString();

				if (Definition.Attribute("Value") != null)
					Definition.Attribute("Value").Remove();

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

