using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Resources;
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace Paraesthesia.Tools.NAntTasks.AlphaResx
{
	/// <summary>
	/// Alphabetizes RESX files.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This task will sort XML resource files by resource name.  This is helpful in a
	/// situation where resource files are modified by several developers and a diff/merge
	/// process is required to allow updates - having the files sorted by resource name
	/// makes the diff/merge cleaner.
	/// </para>
	/// <para>
	/// Note that whether a change was made or not, the source RESX file will be overwritten
	/// by the alphabetized/generated file.  Source control systems that do not directly
	/// compare contents of files for changes may have issue with this.  In CVS, for example,
	/// you will need to do a CVS update after a build to clear the "changed" status on the
	/// file if no changes were made.  In Subversion this isn't an issue, as it inherently
	/// checks file contents, not dates, to determine if a change was made.
	/// </para>
	/// <para>
	/// Optional parameters:
	/// </para>
	/// <list type="table">
	/// <listheader>
	/// <term>Attribute</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term><c>file</c> (<see cref="System.String"/>)</term>
	/// <description>
	/// The path to a RESX file to alphabetize by resource name.  May not be used in conjunction with
	/// the <c>fileset</c> element.
	/// </description>
	/// </item>
	/// </list>
	/// <para>
	/// Optional elements:
	/// </para>
	/// <list type="table">
	/// <listheader>
	/// <term>Element</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term><c>fileset</c> (<see cref="NAnt.Core.Types.FileSet"/>)</term>
	/// <description>
	/// A standard NAnt fileset describing the set of RESX files to alphabetize by resource name.
	/// May not be used in conjunction with the <c>file</c> attribute.
	/// </description>
	/// </item>
	/// </list>
	/// </remarks>
	/// <example>
	/// <para>
	/// The following example alphabetizes a specific RESX file:
	/// </para>
	/// <code>
	/// &lt;alpharesx file=&quot;MyFolder\MyFile.resx&quot; /&gt;
	/// </code>
	/// <para>
	/// This example uses the <c>fileset</c> element to specify all RESX files
	/// in an application:
	/// </para>
	/// <code>
	/// &lt;alpharesx&gt;
	///   &lt;fileset&gt;
	///     &lt;include name=&quot;**\*.resx&quot; /&gt;
	///   &lt;/fileset&gt;
	/// &lt;/alpharesx&gt;
	/// </code>
	/// </example>
	[TaskName("alpharesx")]
	public class AlphaResxTask : Task
	{
		/// <summary>
		/// Gets the complete set of files to alphabetize.
		/// </summary>
		/// <value>
		/// A <see cref="System.Collections.Specialized.StringCollection"/> that will be
		/// used as the list of RESX files that actually gets alphabetized.
		/// </value>
		/// <remarks>
		/// <para>
		/// The <see cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResxTask.ResxFile"/>
		/// and <see cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResxTask.ResxFileSet"/>
		/// properties contribute to create this collection during the <see cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResxTask.ExecuteTask"/>
		/// method.
		/// </para>
		/// </remarks>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResxTask" />
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResxTask.ResxFile"/>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResxTask.ResxFileSet"/>
		protected virtual StringCollection AllFiles { get; set; }

		/// <summary>
		/// Gets or sets the files to alphabetize as specified in the task.
		/// </summary>
		/// <value>
		/// A <see cref="NAnt.Core.Types.FileSet"/> with the set of RESX files to alphabetize.
		/// </value>
		/// <remarks>
		/// <para>
		/// This property and <see cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResxTask.ResxFile"/>
		/// are mutually exclusive.
		/// </para>
		/// </remarks>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResxTask" />
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResxTask.ResxFile" />
		[BuildElement("fileset")]
		public virtual FileSet ResxFileSet { get; set; }

		/// <summary>
		/// Gets or sets the individual file to alphabetize as specified in the task.
		/// </summary>
		/// <value>
		/// A <see cref="System.String"/> with an individual RESX file to alphabetize.
		/// </value>
		/// <remarks>
		/// <para>
		/// This property and <see cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResxTask.ResxFileSet"/>
		/// are mutually exclusive.
		/// </para>
		/// </remarks>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResxTask" />
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResxTask.ResxFileSet" />
		[TaskAttribute("file")]
		public string ResxFile { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResxTask" /> class.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResxTask" />
		public AlphaResxTask()
		{
			this.AllFiles = new StringCollection();
			this.ResxFile = null;
			this.ResxFileSet = new FileSet();
		}

		/// <summary>
		/// Executes the task.  Sets up settings and runs the primary task.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResxTask" />
		protected override void ExecuteTask()
		{
			if (this.ResxFile != null)
			{
				// An individual file was specified
				FileInfo info = new FileInfo(this.ResxFile);
				if (!info.Exists)
				{
					throw new BuildException(String.Format(CultureInfo.InvariantCulture, "Could not find RESX file {0} to alphabetize.", info.FullName), this.Location);
				}
				this.AllFiles.Add(info.FullName);
			}
			else
			{
				// A set of files was specified
				foreach (string filename in this.ResxFileSet.FileNames)
				{
					FileInfo info = new FileInfo(filename);
					if (info.Exists)
					{
						this.AllFiles.Add(info.FullName);
					}
				}
			}

			foreach (string filename in this.AllFiles)
			{
				this.AlphabetizeResxFile(filename);
			}
		}

		/// <summary>
		/// Initializes the task.  Validates parameter settings.
		/// </summary>
		/// <exception cref="NAnt.Core.BuildException">
		/// Thrown if both an individual RESX file and a RESX file set are specified.
		/// </exception>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResxTask" />
		protected override void Initialize()
		{
			if (((this.ResxFile != null) && (this.ResxFileSet != null)) && (this.ResxFileSet.Includes.Count > 0))
			{
				throw new BuildException("The 'file' attribute and the <fileset> element cannot be combined.", this.Location);
			}
		}

		/// <summary>
		/// Executes the alphabetize operation on the set of RESX files.
		/// </summary>
		/// <param name="filename">
		/// The name of the RESX file to alphabetize.
		/// </param>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.AlphaResx.AlphaResxTask" />
		protected virtual void AlphabetizeResxFile(string filename)
		{
			TempFileCollection tempFiles = null;
			try
			{
				tempFiles = new TempFileCollection();
				tempFiles.KeepFiles = false;
				if (!Directory.Exists(tempFiles.BasePath))
				{
					Directory.CreateDirectory(tempFiles.BasePath);
				}
				string tfName = Path.Combine(tempFiles.BasePath, Path.GetFileName(filename));

				using (ResXResourceReader reader = new ResXResourceReader(filename))
				{
					// Add the resources to a dictionary that will be sorted later.
					Dictionary<string, object> resources = new Dictionary<string, object>();

					IDictionaryEnumerator id = reader.GetEnumerator();
					foreach (DictionaryEntry d in reader)
					{
						resources.Add(d.Key.ToString(), d.Value);
					}

					// Write the resources to a temporary file in alpha order by key.
					using (ResXResourceWriter writer = new ResXResourceWriter(tfName))
					{
						List<string> resourceKeys = new List<string>();
						resourceKeys.AddRange(resources.Keys);
						resourceKeys.Sort();
						foreach (string key in resourceKeys)
						{
							writer.AddResource(key, resources[key]);
						}
						writer.Generate();
						writer.Close();
					}
				}

				// Copy temp file over original file
				if (File.Exists(tfName))
				{
					File.Copy(tfName, filename, true);
					this.Log(Level.Info, "Alphabetized RESX file {0}", filename);
				}
				else
				{
					if (this.FailOnError)
					{
						throw new BuildException(String.Format(CultureInfo.InvariantCulture, "Unable to alphabetize RESX file {0}", filename), this.Location);
					}
					else
					{
						this.Log(Level.Error, "Unable to alphabetize RESX file {0}", filename);
					}
				}
			}
			finally
			{
				if (tempFiles != null)
				{
					tempFiles.Delete();
					if (Directory.Exists(tempFiles.BasePath))
					{
						Directory.Delete(tempFiles.BasePath, true);
					}
					tempFiles = null;
				}
			}
		}
	}
}
