using System;
using System.IO;
using System.Text.RegularExpressions;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace Paraesthesia.Tools.NAntTasks.LintRelativePaths {
	/// <summary>
	/// Ensures that no relative references are made to system folders.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Since projects/solutions can exist in any location on a developer's system, references
	/// to common system locations ("Program Files," "Windows\System32," etc.) can't
	/// be made in a relative fashion - they must be absolute.
	/// </para>
	/// <para>
	/// The "lint relative paths" task looks at all specified files and outputs a list
	/// of locations where relative paths are found to special system folders.  If any
	/// are found, the build fails.
	/// </para>
	/// <para>
	/// The following regular expressions are searched for in the specified files and, if
	/// found, fail the build:
	/// </para>
	/// <list type="bullet">
	/// <item>
	/// <term><c>\.\.\\Inetpub</c></term>
	/// </item>
	/// <item>
	/// <term><c>\.\.\\Windows</c></term>
	/// </item>
	/// <item>
	/// <term><c>\.\.\\Program Files</c></term>
	/// </item>
	/// </list>
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
	/// The path to a file to lint for relative paths.  May not be used in conjunction with
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
	/// A standard NAnt fileset describing the set of files to lint for relative paths.
	/// May not be used in conjunction with the <c>file</c> attribute.
	/// </description>
	/// </item>
	/// </list>
	/// </remarks>
	/// <example>
	/// <para>
	/// The following example lints the relative paths from a specific project file:
	/// </para>
	/// <code>
	/// &lt;lintrelativepaths file=&quot;MyFolder\MyFile.csproj&quot; /&gt;
	/// </code>
	/// <para>
	/// This example uses the <c>fileset</c> element to specify all solutions and projects
	/// in an application:
	/// </para>
	/// <code>
	/// &lt;lintrelativepaths&gt;
	///   &lt;fileset&gt;
	///     &lt;include name=&quot;**\*.sln&quot; /&gt;
	///     &lt;include name=&quot;**\*.csproj&quot; /&gt;
	///   &lt;/fileset&gt;
	/// &lt;/lintrelativepaths&gt;
	/// </code>
	/// </example>
	[TaskName("lintrelativepaths")]
	public class LintRelativePathsTask : Task {

		#region LintRelativePathsTask Variables

		/// <summary>
		/// Static reference to the regular expressions that need to be checked for in files.
		/// </summary>
		private static Regex[] PathChecks;

		/// <summary>
		/// Internal storage for the
		/// <see cref="Paraesthesia.Tools.NAntTasks.LintRelativePaths.LintRelativePathsTask.File" />
		/// property.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.LintRelativePaths.LintRelativePathsTask" />
		private FileInfo _file = null;

		/// <summary>
		/// Internal storage for the
		/// <see cref="Paraesthesia.Tools.NAntTasks.LintRelativePaths.LintRelativePathsTask.LintFileSet" />
		/// property.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.LintRelativePaths.LintRelativePathsTask" />
		private FileSet _lintFileSet = new FileSet();

		#endregion



		#region LintRelativePathsTask Properties

		/// <summary>
		/// Gets or sets an individual file to lint.
		/// </summary>
		/// <value>
		/// A <see cref="System.IO.FileInfo"/> containing the file information for a
		/// file to lint relative paths on.
		/// </value>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.LintRelativePaths.LintRelativePathsTask" />
		[TaskAttribute("file")]
		public FileInfo File{
			get {
				return _file;
			}
			set {
				_file = value;
			}
		}

		/// <summary>
		/// Gets or sets the list of files to lint.
		/// </summary>
		/// <value>
		/// A <see cref="System.IO.FileInfo"/> containing the file information for a
		/// set of files to lint relative paths on.
		/// </value>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.LintRelativePaths.LintRelativePathsTask" />
		[BuildElement("fileset")]
		public FileSet LintFileSet {
			get {
				return _lintFileSet;
			}
			set {
				_lintFileSet = value;
			}
		}

		#endregion



		#region LintRelativePathsTask Implementation

		#region Constructors

		/// <summary>
		/// Initializes <see langword="static" /> fields.
		/// </summary>
		static LintRelativePathsTask(){
			PathChecks = new Regex[3];
			PathChecks[0] = new Regex(@"\.\.\\Inetpub", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			PathChecks[1] = new Regex(@"\.\.\\Windows", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			PathChecks[2] = new Regex(@"\.\.\\Program Files", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Ensures an invalid combination of attributes is not used.
		/// </summary>
		/// <param name="taskNode">The <see cref="System.Xml.XmlNode"/> that contains the task definition.</param>
		/// <exception cref="NAnt.Core.BuildException">
		/// Thrown if both the <c>file</c> and <c>fileset</c> properties are specified.
		/// </exception>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.LintRelativePaths.LintRelativePathsTask" />
		protected override void InitializeTask(System.Xml.XmlNode taskNode) {
			if(this.File != null && this.LintFileSet.Includes.Count != 0){
				throw new BuildException("Cannot specify both 'file' and use <fileset> in the same <lintrelativepaths> task.", this.Location);
			}
			base.InitializeTask (taskNode);
		}

		/// <summary>
		/// Executes path linting for the specified set of files.
		/// </summary>
		protected override void ExecuteTask() {
			this.Log(Level.Info, "Linting relative paths...");
			if(this.File != null){
				this.LintRelativePath(this.File.FullName);
			}
			else if(this.LintFileSet.FileNames.Count > 0){
				foreach(string filename in this.LintFileSet.FileNames){
					this.LintRelativePath(filename);
				}
			}
			this.Log(Level.Info, "Specified files linted for relative paths.");
		}

		#endregion

		#region Methods

		/// <summary>
		/// Lints the relative paths from a given file.
		/// </summary>
		/// <param name="filename">
		/// The <see cref="System.String"/> containing the filename to process for relative paths.
		/// </param>
		/// <exception cref="NAnt.Core.BuildException">
		/// Thrown if <paramref name="filename" /> doesn't exist or if a relative path is
		/// found in the file.
		/// </exception>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.LintRelativePaths.LintRelativePathsTask" />
		protected void LintRelativePath(string filename){
			if(filename == null || filename == ""){
				return;
			}
			this.Log(Level.Verbose, "Linting relative paths in: {0}", filename);
			StreamReader reader = null;
			try {
				reader = System.IO.File.OpenText(filename);
				string line = reader.ReadLine();
				while(line != null){
					for(int i = 0; i < PathChecks.Length; i++){
						if(PathChecks[i].IsMatch(line)){
							throw new BuildException(String.Format("Relative path found in file '{0}' matching expression '{1}'", filename, PathChecks[i]), this.Location);
						}
					}
					line = reader.ReadLine();
				}
			}
			finally {
				if(reader != null){
					reader.Close();
					reader = null;
				}
			}
		}

		#endregion

		#endregion

	}
}
