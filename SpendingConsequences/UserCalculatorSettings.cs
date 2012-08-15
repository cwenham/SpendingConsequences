using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using System.Xml.Linq;

using Mono.Data.Sqlite;

using MonoTouch.Foundation;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	/// <summary>
	/// Store and retrieve user-custom settings for Configurable Values in calculators
	/// </summary>
	/// <remarks>Will only create a database when given a custom setting to store. Will return null for queries if
	/// there's either no database, or the user hasn't customized that value.</remarks>
	public class UserCalculatorSettings
	{
		public const string CURRENT_DB_VERSION = "2.0";
		
		public UserCalculatorSettings ()
		{
			UpgradeV1toV2();
		}
		
		public string CurrentDBFilename {
			get {
				return DBFilenameByVersion (CURRENT_DB_VERSION);
			}
		}

		private string DBFilenameByVersion (string version)
		{
			return string.Format ("calcSettings_{0}.db3", version);
		}

		private string DBPath (string dbFilename)
		{
			string lib = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			return Path.Combine (lib, dbFilename);
		}
		
		public string CurrentDBPath {
			get {
				return DBPath (CurrentDBFilename);
			}
		}
		
		public bool DatabaseExists {
			get {
				return File.Exists (CurrentDBPath);
			}
		}
		
		private void CreateDatabase ()
		{
			SqliteConnection.CreateFile (CurrentDBPath);
			SqliteConnection conn = new SqliteConnection (string.Format(connectionTemplate, CurrentDBPath));
			using (var c = conn.CreateCommand()) {
				c.CommandText = createTableV2;
				c.CommandType = CommandType.Text;
				conn.Open ();
				c.ExecuteNonQuery ();
				conn.Close ();
			}
		}

		private void UpgradeV1toV2 ()
		{
			string V1DBPath = DBPath(DBFilenameByVersion("1.0"));
			if (File.Exists(V1DBPath))
			try {
				if (!DatabaseExists)
					CreateDatabase ();

				SqliteConnection V1DB = new SqliteConnection (string.Format (connectionTemplate, V1DBPath));
				V1DB.Open();

				SqliteCommand listCmd = V1DB.CreateCommand();
				listCmd.CommandText = v1ListQuery;
				listCmd.CommandType = CommandType.Text;

				SqliteDataReader reader = listCmd.ExecuteReader(CommandBehavior.CloseConnection);
				if (reader.HasRows)
				{
					OpenConnection();

					while (reader.Read())
					{
						SqliteCommand cmd = PreparedInsert;
						cmd.Parameters.Clear();

						SqliteParameter pValueID = new SqliteParameter ("@valueID");
						pValueID.Value = reader["valueID"];
						cmd.Parameters.Add (pValueID);
						SqliteParameter pValue = new SqliteParameter ("@value");
						pValue.Value = reader["value"];
						cmd.Parameters.Add (pValue);
						SqliteParameter pCurrency = new SqliteParameter ("@currencyCode");
						pCurrency.Value = NSLocale.CurrentLocale.CurrencyCode;
						cmd.Parameters.Add (pCurrency);
						SqliteParameter pDate = new SqliteParameter ("@added");
						pDate.Value = reader["added"];
						cmd.Parameters.Add (pDate);

						cmd.ExecuteNonQuery();
					}

					CloseConnection();
				}

				V1DB.Close();
				File.Move(V1DBPath, V1DBPath + ".converted");
			} catch (Exception ex) {
				Console.WriteLine ("{0} thrown when trying to upgrade DB version 1.0 to 2.0: {1}", ex.GetType().Name, ex.Message);
			}
		}
		
		private const string connectionTemplate = "Data Source={0}";
		
		private const string createTableV1 = "CREATE TABLE ConfigurableValues (valueID TEXT PRIMARY KEY," +
			"added INTEGER," +
			"updated INTEGER," +
			"value TEXT" +
			")";

		private const string createTableV2 = "CREATE TABLE ConfigurableValues (valueID TEXT PRIMARY KEY," +
			"added INTEGER," +
			"updated INTEGER," +
			"value TEXT," +
			"currencyCode TEXT" +
			")";

		private const string v1ListQuery = "SELECT * FROM ConfigurableValues";
		
		/// <summary>
		/// Return database connection if it exists, otherwise null
		/// </summary>
		private SqliteConnection Connection {
			get {
				if (_connection != null)
					return _connection;
				
				if (DatabaseExists)
					_connection = new SqliteConnection (string.Format (connectionTemplate, CurrentDBPath));
				
				return _connection;
			}
		}
		private SqliteConnection _connection = null;
		
		private bool OpenConnection ()
		{
			if (Connection.State != ConnectionState.Open) {
				try {
					Connection.Open ();					
				} catch (Exception ex) {
					Console.WriteLine (string.Format (
						"{0} thrown when opening DB: {1}",
						ex.GetType ().Name,
						ex.Message
					)
					);
					return false;
				}

				Connection.StateChange += delegate(object sender, StateChangeEventArgs e) {
					_preparedInsert = null;
					_preparedUpdate = null;
					_preparedQuery = null;
				};
			}
			
			return true;
		}
		
		public void CloseConnection ()
		{
			if (Connection.State == ConnectionState.Open)
				Connection.Close ();
		}
		
		private string ValueKeyName (ConfigurableValue val)
		{
			// Use .ID if it exist, else compute a unique path
			if (val.ID != null)
				return val.ID;
			
			// Note: If the parent element has no caption, we'll use its calculator type. This means that a setting
			// for, say, an Investment calculator's Rate, will affect all un-captioned Investment calculators.
			XElement parent = val.Definition.Parent;
			XAttribute captionAttrib = parent.Attribute ("Caption");
			string caption = captionAttrib != null ? captionAttrib.Value : parent.Name.LocalName;
			return string.Format ("{0}.{1}", caption, val.Name);
		}
		
		public object GetCustomValue (ConfigurableValue val)
		{
			if (!DatabaseExists || Connection == null)
				return null;
			
			string key = ValueKeyName (val);
			
			if (Connection.State != ConnectionState.Open)
			if (!OpenConnection ())
				return null;

			SqliteCommand cmd = PreparedQuery;
			cmd.Parameters.Clear ();
			
			SqliteParameter param = new SqliteParameter ("@key");
			param.Value = key;
			cmd.Parameters.Add (param);

			SqliteDataReader reader = null;
			try {
				reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
				if (reader.Read())
				{
					if (val.ValueType == ConfigurableValueType.Money)
					{
						string value = reader["value"] as string;
						string currency = reader["currencyCode"] as string;
						return new Money(decimal.Parse(value), currency);
					}
					else
						return reader["value"];
				}
				//object result = cmd.ExecuteScalar ();
				//if (!(result is DBNull))
				//	return (string)result;
			} catch (Exception ex) {
				Console.WriteLine (string.Format ("{0} thrown when reading DB: {1}", ex.GetType ().Name, ex.Message));
			} finally
			{
				if (reader != null)
					reader.Close();
			}
			
			return null;
		}
		
		private SqliteCommand PreparedQuery {
			get {
				if (_preparedQuery == null) {
					if (Connection.State != ConnectionState.Open)
						try {
							Connection.Open ();
						} catch (Exception ex) {
							Console.WriteLine (string.Format (
								"{0} thrown when opening DB for PreparedUpdate: {1}",
								ex.GetType ().Name,
								ex.Message
							)
							);
							return null;
						}
					
					_preparedQuery = Connection.CreateCommand ();
					_preparedQuery.CommandType = CommandType.Text;
					_preparedQuery.CommandText = queryTemplate;
					_preparedQuery.Prepare ();
				}
				
				return _preparedQuery;
			}
		}
		private SqliteCommand _preparedQuery = null;

		private const string queryTemplate = "SELECT [value], [currencyCode] FROM ConfigurableValues WHERE valueID = @key LIMIT 1";

		public void StoreCustomValue (ConfigurableValue val, Money newValue)
		{
			StoreCustomValue (val, newValue.Value.ToString(), newValue.CurrencyCode);
		}

		public void StoreCustomValue (ConfigurableValue val, string newValue)
		{
			StoreCustomValue (val, newValue, DBNull.Value);
		}

		public void StoreCustomValue (ConfigurableValue val, string newValue, object currencyCode)
		{
			if (!DatabaseExists)
				CreateDatabase ();
			
			string key = ValueKeyName (val);
			bool exists = GetCustomValue (val) != null;
			
			if (Connection.State != ConnectionState.Open)
				if (!OpenConnection ())
					return;
			
			SqliteCommand cmd = exists ? PreparedUpdate : PreparedInsert;
			cmd.Parameters.Clear ();
			
			SqliteParameter pValueID = new SqliteParameter ("@valueID");
			pValueID.Value = key;
			cmd.Parameters.Add (pValueID);
			SqliteParameter pValue = new SqliteParameter ("@value");
			pValue.Value = newValue;
			cmd.Parameters.Add (pValue);
			SqliteParameter pCurrency = new SqliteParameter ("@currencyCode");
			pCurrency.Value = currencyCode;
			cmd.Parameters.Add (pCurrency);
			SqliteParameter pDate = exists ? new SqliteParameter ("@updated") : new SqliteParameter ("@added");
			pDate.Value = DateTime.UtcNow;
			cmd.Parameters.Add (pDate);
			
			try {
				cmd.ExecuteNonQuery ();				
			} catch (Exception ex) {
				Console.WriteLine (string.Format ("{0} thrown when writing to DB: {1}", ex.GetType ().Name, ex.Message));
			}
		}

		private SqliteCommand PreparedInsert {
			get {
				if (_preparedInsert == null) {
					if (Connection.State != ConnectionState.Open)
						try {
							Connection.Open ();
						} catch (Exception ex) {
							Console.WriteLine (string.Format (
								"{0} thrown when opening DB for PreparedInsert: {1}",
								ex.GetType ().Name,
								ex.Message
							)
							);
							return null;
						}
					
					_preparedInsert = Connection.CreateCommand ();
					_preparedInsert.CommandType = CommandType.Text;
					_preparedInsert.CommandText = insertTemplate;
					_preparedInsert.Prepare ();
				}

				return _preparedInsert;
			}
		}
		private SqliteCommand _preparedInsert = null;

		private const string insertTemplate = "INSERT INTO ConfigurableValues (valueID, added, value, currencyCode) VALUES (@valueID, @added, @value, @currencyCode)";
		
		private SqliteCommand PreparedUpdate {
			get {
				if (_preparedUpdate == null) {
					if (Connection.State != ConnectionState.Open)
						try {
							Connection.Open ();
						} catch (Exception ex) {
							Console.WriteLine (string.Format (
								"{0} thrown when opening DB for PreparedUpdate: {1}",
								ex.GetType ().Name,
								ex.Message
							)
							);
							return null;
						}
					
					_preparedUpdate = Connection.CreateCommand ();
					_preparedUpdate.CommandType = CommandType.Text;
					_preparedUpdate.CommandText = updateTemplate;
					_preparedUpdate.Prepare ();
				}
				
				return _preparedUpdate;
			}
		}
		private SqliteCommand _preparedUpdate = null;

		private const string updateTemplate = "UPDATE ConfigurableValues SET updated = @updated, value = @value, currencyCode = @currencyCode WHERE valueID = @valueID";
	}
}

