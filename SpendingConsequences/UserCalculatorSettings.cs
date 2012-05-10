using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using System.Xml.Linq;

using Mono.Data.Sqlite;

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
		public const string CURRENT_DB_VERSION = "1.0";
		
		public UserCalculatorSettings ()
		{
		}
		
		public string CurrentDBFilename {
			get {
				return string.Format ("calcSettings_{0}.db3", CURRENT_DB_VERSION);
			}
		}
		
		public string CurrentDBPath {
			get {
				string lib = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
				return Path.Combine (lib, CurrentDBFilename);
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
				c.CommandText = createTableV1;
				c.CommandType = CommandType.Text;
				conn.Open ();
				c.ExecuteNonQuery ();
				conn.Close ();
			}
		}
		
		private const string connectionTemplate = "Data Source={0}";
		
		private const string createTableV1 = "CREATE TABLE ConfigurableValues (valueID TEXT PRIMARY KEY," +
			"added INTEGER," +
			"updated INTEGER," +
			"value TEXT" +
			")";
		
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
		
		public string GetCustomValue (ConfigurableValue val)
		{
			if (!DatabaseExists || Connection == null)
				return null;
			
			string key = ValueKeyName (val);
			Connection.Open ();
			SqliteCommand cmd = Connection.CreateCommand ();
			cmd.CommandText = queryTemplate;
			cmd.CommandType = CommandType.Text;
			SqliteParameter param = new SqliteParameter ("@key");
			param.Value = key;
			cmd.Parameters.Add (param);
			
			try {
				object result = cmd.ExecuteScalar ();
				if (!(result is DBNull))
					return (string)result;
			} catch (Exception ex) {
				Console.WriteLine(string.Format("{0} thrown when reading DB: {1}", ex.GetType().Name, ex.Message));
			} finally {
				Connection.Close ();
			}
			
			return null;
		}
		
		private const string queryTemplate = "SELECT [value] FROM ConfigurableValues WHERE valueID = @key LIMIT 1";
		
		public void StoreCustomValue (ConfigurableValue val, string newValue)
		{
			if (!DatabaseExists)
				CreateDatabase ();
			
			string key = ValueKeyName (val);
			bool exists = GetCustomValue (val) != null;
			
			Connection.Open ();
			
			SqliteCommand cmd = Connection.CreateCommand ();
			cmd.CommandText = exists ? updateTemplate : insertTemplate;
			cmd.CommandType = CommandType.Text;
			
			SqliteParameter pValueID = new SqliteParameter ("@valueID");
			pValueID.Value = key;
			cmd.Parameters.Add (pValueID);
			SqliteParameter pValue = new SqliteParameter ("@value");
			pValue.Value = newValue;
			cmd.Parameters.Add (pValue);
			SqliteParameter pDate = exists ? new SqliteParameter ("@updated") : new SqliteParameter ("@added");
			pDate.Value = DateTime.UtcNow;
			cmd.Parameters.Add (pDate);
			
			try {
				cmd.ExecuteNonQuery ();				
			} catch (Exception ex) {
				Console.WriteLine (string.Format ("{0} thrown when writing to DB: {1}", ex.GetType ().Name, ex.Message));
			} finally {
				Connection.Close ();
			}
		}
		
		private const string insertTemplate = "INSERT INTO ConfigurableValues (valueID, added, value) VALUES (@valueID, @added, @value)";
		
		private const string updateTemplate = "UPDATE ConfigurableValues SET updated = @updated, value = @value WHERE valueID = @valueID";
	}
}

