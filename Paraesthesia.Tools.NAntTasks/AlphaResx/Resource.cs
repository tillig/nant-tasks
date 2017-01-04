using System;

namespace Paraesthesia.Tools.NAntTasks.AlphaResx {
	/// <summary>
	/// Represents an individual resource that resides in a RESX file.
	/// </summary>
	public class Resource : IComparable {

		#region Resource Variables

		/// <summary>
		/// Internal storage for the
		/// <see cref="Paraesthesia.Tools.NAntTasks.AlphaResx.Resource.MimeType" />
		/// property.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.Resource" />
		private string _mimeType = null;

		/// <summary>
		/// Internal storage for the
		/// <see cref="Paraesthesia.Tools.NAntTasks.AlphaResx.Resource.Name" />
		/// property.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.Resource" />
		private string _name = null;

		/// <summary>
		/// Internal storage for the
		/// <see cref="Paraesthesia.Tools.NAntTasks.AlphaResx.Resource.Type" />
		/// property.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.Resource" />
		private string _type = null;

		/// <summary>
		/// Internal storage for the
		/// <see cref="Paraesthesia.Tools.NAntTasks.AlphaResx.Resource.Value" />
		/// property.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.Resource" />
		private string _value = null;

		#endregion



		#region Resource Properties

		/// <summary>
		/// Gets or sets the resource MIME type.
		/// </summary>
		/// <value>
		/// A <see cref="System.String"/> with the resource MIME type.
		/// </value>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.Resource" />
		public virtual string MimeType {
			get {
				return _mimeType;
			}
			set {
				_mimeType = value;
			}
		}

		/// <summary>
		/// Gets or sets the resource name.
		/// </summary>
		/// <value>
		/// A <see cref="System.String"/> with the unique resource ID.
		/// </value>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.Resource" />
		public virtual string Name {
			get {
				return _name;
			}
			set {
				_name = value;
			}
		}

		/// <summary>
		/// Gets or sets the resource type.
		/// </summary>
		/// <value>
		/// A <see cref="System.String"/> with the resource type name.
		/// </value>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.Resource" />
		public virtual string Type {
			get {
				return _type;
			}
			set {
				_type = value;
			}
		}

		/// <summary>
		/// Gets or sets the resource value.
		/// </summary>
		/// <value>
		/// A <see cref="System.String"/> with the contents of the resource.
		/// </value>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.Resource" />
		public virtual string Value {
			get {
				return _value;
			}
			set {
				_value = value;
			}
		}

		#endregion



		#region Resource Implementation

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Paraesthesia.Tools.NAntTasks.AlphaResx.Resource" /> class.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.Resource" />
		public Resource() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Paraesthesia.Tools.NAntTasks.AlphaResx.Resource" /> class.
		/// </summary>
		/// <param name="name">The resource name (unique ID).</param>
		/// <param name="value">The resource value (contents).</param>
		/// <param name="type">The strong type of the resource.</param>
		/// <param name="mimeType">The MIME type of the resource.</param>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.Resource" />
		public Resource(string name, string value, string type, string mimeType){
			this._name = name;
			this._value = value;
			this._type = type;
			this._mimeType = mimeType;
		}

		#endregion

		#region IComparable Members

		/// <summary>
		/// Compares one resource to another by name.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> (resource) to compare to.</param>
		/// <returns>A 32-bit signed integer that indicates the relative order of the comparands.</returns>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.Resource" />
		/// <seealso cref="System.String.CompareTo" />
		/// <seealso cref="System.IComparable.CompareTo" />
		public int CompareTo(object obj) {
			if(obj == null){
				return 1;
			}
			Resource compareObj = obj as Resource;
			if(compareObj == null){
				throw new ArgumentException("Unable to compare a non-Resource to a Resource.", "obj");
			}
			return this.Name.CompareTo(compareObj.Name);
		}

		#endregion

		#endregion
	}
}
