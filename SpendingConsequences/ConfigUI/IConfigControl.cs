using System;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public interface IConfigControl
	{
		void BeginEditing();

		void EndEditing();

		/// <summary>
		/// Caption or other non-value parameter has changed
		/// </summary>
		/// <remarks>A ConfigurableValue is made of two things a user can change: the value, which is used as a
		/// parameter in a calculation, and a configuration, which identifies the type of value or the label used to
		/// describe it. This event signals a change in the latter, typically due to the user tapping the 'Edit' button
		/// and changing the label or caption.</remarks>
		event EventHandler<ConfigurationChangeEventArgs> ConfigurationChanged;

		/// <summary>
		/// Value has changed
		/// </summary>
		event EventHandler<ConfigurableValueChanged> ValueChanged;

		event EventHandler<CurrencyChangeEventArgs> CurrencyButtonClicked;
	}
	
	public class ConfigurableValueChanged : EventArgs
	{
		public ConfigurableValue ConfigValue { get; private set; }
		
		public ConfigurableValueChanged(ConfigurableValue val)
		{
			this.ConfigValue = val;
		}
	}

	public class CurrencyChangeEventArgs : EventArgs
	{
		public ConfigurableValue ConfigValue { get; private set; }

		public CurrencyChangeEventArgs(ConfigurableValue val)
		{
			this.ConfigValue = val;
		}
	}

	public class ConfigurationChangeEventArgs : EventArgs
	{
		public ConfigurableValue ConfigValue { get; private set; }

		public ConfigurationChangeEventArgs(ConfigurableValue val)
		{
			this.ConfigValue = val;
		}
	}
}

