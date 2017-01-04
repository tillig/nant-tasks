using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Resources;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;

namespace Paraesthesia.Tools.NAntTasks.AlphaResx {
	/// <summary>
	/// Writes resources in an XML resource (.resx) file or an output stream in alphabetical order by name.
	/// </summary>
	public class AlphaResXResourceWriter : System.Resources.IResourceWriter {
		#region AlphaResXResourceWriter Variables

		#region Constants

		/// <summary>
		/// The version of RESX file that this writer outputs.
		/// </summary>
		public const string Version = "1.3";

		/// <summary>
		/// The schema for this version of RESX file.
		/// </summary>
		public const string ResourceSchema = "<xsd:schema id=\"root\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\"><xsd:element name=\"root\" msdata:IsDataSet=\"true\"><xsd:complexType><xsd:choice maxOccurs=\"unbounded\"><xsd:element name=\"data\"><xsd:complexType><xsd:sequence><xsd:element name=\"value\" type=\"xsd:string\" minOccurs=\"0\" msdata:Ordinal=\"1\" /><xsd:element name=\"comment\" type=\"xsd:string\" minOccurs=\"0\" msdata:Ordinal=\"2\" /></xsd:sequence><xsd:attribute name=\"name\" type=\"xsd:string\" msdata:Ordinal=\"1\" /><xsd:attribute name=\"type\" type=\"xsd:string\" msdata:Ordinal=\"3\" /><xsd:attribute name=\"mimetype\" type=\"xsd:string\" msdata:Ordinal=\"4\" /></xsd:complexType></xsd:element><xsd:element name=\"resheader\"><xsd:complexType><xsd:sequence><xsd:element name=\"value\" type=\"xsd:string\" minOccurs=\"0\" msdata:Ordinal=\"1\" /></xsd:sequence><xsd:attribute name=\"name\" type=\"xsd:string\" use=\"required\" /></xsd:complexType></xsd:element></xsd:choice></xsd:complexType></xsd:element></xsd:schema>";

		/// <summary>
		/// Length of a line used in line-wrapping functions.
		/// </summary>
		protected const int LINE_LENGTH = 80;

		#endregion

		#region Instance

		private bool _hasBeenSaved = false;
		private string _fileName = "";
		private Stream _stream = null;
		private TextWriter _textWriter = null;
		private IFormatter _binaryFormatter = null;
		private ArrayList _allResources = null;

		#endregion

		#endregion



		#region AlphaResXResourceWriter Implementation

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResXResourceWriter" /> class.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResXResourceWriter" />
		protected AlphaResXResourceWriter(){
			this._binaryFormatter = new BinaryFormatter();
			this._allResources = new ArrayList();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResXResourceWriter" /> class.
		/// </summary>
		/// <param name="fileName">The output file name.</param>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResXResourceWriter" />
		public AlphaResXResourceWriter(string fileName) : this(){
			this._fileName = fileName;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResXResourceWriter" /> class.
		/// </summary>
		/// <param name="stream">The output stream.</param>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResXResourceWriter" />
		public AlphaResXResourceWriter(Stream stream) : this(){
			this._stream = stream;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResXResourceWriter" /> class.
		/// </summary>
		/// <param name="textWriter">The <see cref="System.IO.TextWriter"/> object to send output to.</param>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResXResourceWriter" />
		public AlphaResXResourceWriter(TextWriter textWriter) : this(){
			this._textWriter = textWriter;
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResXResourceWriter" /> class.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResXResourceWriter" />
		~AlphaResXResourceWriter(){
			this.Dispose(false);
		}

		#endregion

		#region Methods

		#region Instance

		/// <summary>
		/// Coverts a byte array to a base-64 encoded line-wrapped string.
		/// </summary>
		/// <param name="data">The data to convert.</param>
		/// <returns>
		/// A <see cref="System.String"/> with a base-64 encoded version of the
		/// <paramref name="data" /> wrapped to 80-char line lengths.
		/// </returns>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResXResourceWriter" />
		protected virtual string ConvertDataToBase64WrappedString(byte[] data){
			string b64 = Convert.ToBase64String(data);

			// If it's only one line long, return it.
			if (b64.Length <= LINE_LENGTH) {
				return b64;
			}

			// Perform line wrapping
			StringBuilder builder = new StringBuilder(b64.Length + ((b64.Length / LINE_LENGTH) * 3));
			int charCount = 0;
			while (charCount < (b64.Length - LINE_LENGTH)) {
				builder.Append(Environment.NewLine);
				builder.Append("        ");
				builder.Append(b64, charCount, LINE_LENGTH);
				charCount += LINE_LENGTH;
			}
			builder.Append(Environment.NewLine);
			builder.Append("        ");
			builder.Append(b64, charCount, b64.Length - charCount);
			builder.Append(Environment.NewLine);
			return builder.ToString();
		}

		#endregion

		#region IResourceWriter Members

		/// <summary>
		/// Closes the underlying resource file or stream, ensuring all the data has been
		/// written to the file.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResXResourceWriter" />
		/// <seealso cref="System.Resources.IResourceWriter" />
		public virtual void Close() {
			this.Dispose();
		}

		/// <summary>
		/// Adds an 8-bit unsigned integer array as a named resource to the list of
		/// resources to be written.
		/// </summary>
		/// <param name="name">Name of a resource.</param>
		/// <param name="value">Value of a resource as an 8-bit unsigned integer array.</param>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResXResourceWriter" />
		/// <seealso cref="System.Resources.IResourceWriter" />
		public virtual void AddResource(string name, byte[] value) {
			Resource res = new Resource();
			res.Name = name;
			res.Value = this.ConvertDataToBase64WrappedString(value);
			res.Type = typeof(byte[]).AssemblyQualifiedName;
			this._allResources.Add(res);
		}

		/// <summary>
		/// Adds a named resource of type <see cref="System.Object"/> to the list of
		/// resources to be written.
		/// </summary>
		/// <param name="name">Name of a resource.</param>
		/// <param name="value">The value of the resource.</param>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResXResourceWriter" />
		/// <seealso cref="System.Resources.IResourceWriter" />
		public virtual void AddResource(string name, object value) {
			if(value is String){
				this.AddResource(name, (string)value);
			}
			else if(value is byte[]){
				this.AddResource(name, (byte[])value);
			}
			else{
				Type type = (value == null ? typeof(object) : value.GetType());
				if(value != null && !type.IsSerializable){
					throw new InvalidOperationException(String.Format("Unable to serialize resource {0} of type {1}.", name, type.FullName));
				}

				// Try converting the object to a string
				TypeConverter converter = TypeDescriptor.GetConverter(type);
				bool canConvertToString = converter.CanConvertTo(typeof(string));
				bool canConvertFromString = converter.CanConvertFrom(typeof(string));
				if (canConvertToString && canConvertFromString) {
					Resource res = new Resource();
					res.Name = name;
					res.Value = converter.ConvertToInvariantString(value);
					res.Type = type.FullName;
					this._allResources.Add(res);
					return;
				}

				// Try converting the object to a byte array
				bool canConvertToByte = converter.CanConvertTo(typeof(byte[]));
				bool canConvertFromByte = converter.CanConvertFrom(typeof(byte[]));
				if (canConvertToByte && canConvertFromByte) {
					// Can convert to byte array
					byte[] objByteArray = (byte[]) converter.ConvertTo(value, typeof(byte[]));
					string encoded = this.ConvertDataToBase64WrappedString(objByteArray);
					Resource res = new Resource();
					res.Name = name;
					res.Value = encoded;
					res.Type = type.FullName;
					res.MimeType = "application/x-microsoft.net.object.bytearray.base64";
					this._allResources.Add(res);
				}
				else if (value == null) {
					// Value is null
					Resource res = new Resource();
					res.Name = name;
					res.Value = "";
					res.Type = "System.Resources.ResXNullRef, System.Windows.Forms, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
					this._allResources.Add(res);
				}
				else {
					// Serialize the object to binary
					MemoryStream stm = new MemoryStream();
					this._binaryFormatter.Serialize(stm, value);
					string serValue = this.ConvertDataToBase64WrappedString(stm.ToArray());
					Resource res = new Resource();
					res.Name = name;
					res.Value = serValue;
					res.MimeType = "application/x-microsoft.net.object.binary.base64";
					this._allResources.Add(res);
				}
			}
		}

		/// <summary>
		/// Adds a named resource of type <see cref="System.String"/> to the list of
		/// resources to be written.
		/// </summary>
		/// <param name="name">Name of a resource.</param>
		/// <param name="value">The value of the resource.</param>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResXResourceWriter" />
		/// <seealso cref="System.Resources.IResourceWriter" />
		public virtual void AddResource(string name, string value) {
			Resource res = new Resource();
			res.Name = name;
			res.Value = value;
			this._allResources.Add(res);
		}

		/// <summary>
		/// Writes all the resources added by the <see cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResXResourceWriter.AddResource"/>
		/// method to the output file or stream.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResXResourceWriter" />
		/// <seealso cref="System.Resources.IResourceWriter" />
		public virtual void Generate() {
			if(this._hasBeenSaved){
				throw new InvalidOperationException("Resources have already been generated.");
			}

			// Sort the resources
			this._allResources.Sort();

			// Initialize the XML writer/document
			XmlTextWriter writer = null;
			bool docStartWritten = false;
			if(this._textWriter != null){
				this._textWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
				docStartWritten = true;
				writer = new XmlTextWriter(this._textWriter);
			}
			else if (this._stream != null) {
				writer = new XmlTextWriter(this._stream, Encoding.UTF8);
			}
			else {
				writer = new XmlTextWriter(this._fileName, Encoding.UTF8);
			}
			writer.Formatting = Formatting.Indented;
			writer.Indentation = 1;
			writer.IndentChar = '\t';
			if(!docStartWritten){
				writer.WriteStartDocument();
			}

			// Write doc root
			writer.WriteStartElement("root");

			// Write the RESX schema
			XmlTextReader reader = new XmlTextReader(new StringReader(ResourceSchema));
			reader.WhitespaceHandling = WhitespaceHandling.None;
			writer.WriteNode(reader, true);

			// Write standard header elements
			writer.WriteStartElement("resheader");
			writer.WriteAttributeString("name", "ResMimeType");
			writer.WriteStartElement("value");
			writer.WriteString("text/microsoft-resx");
			writer.WriteEndElement();
			writer.WriteEndElement();

			writer.WriteStartElement("resheader");
			writer.WriteAttributeString("name", "Version");
			writer.WriteStartElement("value");
			writer.WriteString(Version);
			writer.WriteEndElement();
			writer.WriteEndElement();

			writer.WriteStartElement("resheader");
			writer.WriteAttributeString("name", "Reader");
			writer.WriteStartElement("value");
			writer.WriteString(typeof(ResXResourceReader).AssemblyQualifiedName);
			writer.WriteEndElement();
			writer.WriteEndElement();

			writer.WriteStartElement("resheader");
			writer.WriteAttributeString("name", "Writer");
			writer.WriteStartElement("value");
			writer.WriteString(typeof(ResXResourceWriter).AssemblyQualifiedName);
			writer.WriteEndElement();
			writer.WriteEndElement();

			// Write resources
			foreach(Resource res in this._allResources){
				writer.WriteStartElement("data");
				writer.WriteAttributeString("name", res.Name);
				if (res.Type != null) {
					writer.WriteAttributeString("type", res.Type);
				}
				if (res.MimeType != null) {
					writer.WriteAttributeString("mimetype", res.MimeType);
				}
				writer.WriteStartElement("value");
				writer.WriteString(res.Value);
				writer.WriteEndElement();
				writer.WriteEndElement();
			}

			// Close doc root
			writer.WriteEndElement();
			writer.Flush();
			writer.Close();

			// Mark the resources as being generated
			this._hasBeenSaved = true;
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or
		/// resetting unmanaged resources.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResXResourceWriter" />
		/// <seealso cref="System.IDisposable" />
		public virtual void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or
		/// resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing">
		/// <see langword="true" /> to dispose of managed resources; <see langword="false" />
		/// otherwise.
		/// </param>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResXResourceWriter" />
		/// <seealso cref="System.IDisposable" />
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (!this._hasBeenSaved) {
					this.Generate();
				}
				if (this._stream != null) {
					this._stream.Close();
					this._stream = null;
				}
				if (this._textWriter != null) {
					this._textWriter.Close();
					this._textWriter = null;
				}
			}
		}

		#endregion

		#endregion

		#endregion


	}
}
