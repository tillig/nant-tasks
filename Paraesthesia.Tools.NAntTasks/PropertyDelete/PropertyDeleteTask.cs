using System;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace Paraesthesia.Tools.NAntTasks.PropertyDelete {
	/// <summary>
	/// Deletes a property from the collection of available properties.
	/// </summary>
	/// <remarks>
	/// <para>
	/// At times it is necessary for a property to be "unset" or removed from the collection
	/// of properties so the <c>property::exists</c> function can be used to correctly
	/// differentiate between a property not existing and a property existing but having
	/// an empty value.  This is especially handy in looping constructs where the property
	/// may or may not be set every time through the loop but needs to be independently
	/// controlled in each iteration through the loop.
	/// </para>
	/// <para>
	/// The task will not fail if the property you want to delete doesn't exist.
	/// </para>
	/// <para>
	/// The task will fail if you attempt to delete a property marked read-only.
	/// </para>
	/// <para>
	/// Required parameters:
	/// </para>
	/// <list type="table">
	/// <listheader>
	/// <term>Attribute</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term><c>name</c> (<see cref="System.String"/>)</term>
	/// <description>
	/// The name of the property you wish to delete.
	/// </description>
	/// </item>
	/// </list>
	/// </remarks>
	/// <example>
	/// <para>
	/// The following shows a property getting created and subsequently deleted:
	/// </para>
	/// <code>
	/// &lt;property name="myprop" value="myvalue" /&gt;
	/// &lt;echo message="myprop exists: ${property::exists('myprop')}" /&gt;
	/// &lt;propertydelete name="myprop" /&gt;
	/// &lt;echo message="myprop exists: ${property::exists('myprop')}" /&gt;
	/// </code>
	/// </example>
	[TaskName("propertydelete")]
	public class PropertyDeleteTask : NAnt.Core.Task {

		#region PropertyDeleteTask Variables

		/// <summary>
		/// Internal storage for the
		/// <see cref="Paraesthesia.Tools.NAntTasks.PropertyDelete.PropertyDeleteTask.PropertyName" />
		/// property.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.PropertyDelete.PropertyDeleteTask" />
		private string _propertyName = null;

		#endregion



		#region PropertyDeleteTask Properties

		/// <summary>
		/// Gets or sets the property name to be deleted.
		/// </summary>
		/// <value>
		/// A <see cref="System.String"/> with the name of the property to delete from
		/// the set of available properties.
		/// </value>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.PropertyDelete.PropertyDeleteTask" />
		[TaskAttribute("name", Required=true)]
		[StringValidator(AllowEmpty=false)]
		public string PropertyName {
			get {
				return this._propertyName;
			}
			set {
				this._propertyName = value;
			}
		}

		#endregion



		#region PropertyDeleteTask Implementation

		#region Overrides

		/// <summary>
		/// Deletes the named property from the collection.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.PropertyDelete.PropertyDeleteTask" />
		protected override void ExecuteTask() {
			if(!this.Properties.Contains(this.PropertyName)){
				this.Log(Level.Verbose, "Property \"{0}\" doesn't exist; unable to delete.", this.PropertyName);
				return;
			}

			if(this.Properties.IsReadOnlyProperty(this.PropertyName)){
				throw new BuildException(String.Format("Read-only properties can't be deleted.  Unable to delete property \"{0}.\"", this.PropertyName), this.Location);
			}

			this.Properties.Remove(this.PropertyName);
			this.Log(Level.Verbose, "Property \"{0}\" deleted.", this.PropertyName);
		}

		#endregion

		#endregion

	}
}
