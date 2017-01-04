using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Util;
using NAnt.NUnit.Types;
using NAnt.NUnit2.Types;

namespace Paraesthesia.Tools.NAntTasks.NUnitExec
{
	/// <summary>
	/// Runs NUnit tests by executing a command-line NUnit process.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This task is a replacement for the <c>nunit2</c> task to run NUnit tests
	/// using the console application rather than via direct interface with the
	/// NUnit libraries.  This is actually the recommended method of testing -
	/// using the console app rather than the <c>nunit2</c> task - to avoid
	/// assembly load and bind problems as versions of NUnit and NAnt change.
	/// </para>
	/// <para>
	/// The syntax of the task is written to be identical to that of the <c>nunit2</c>
	/// task (<see cref="NAnt.NUnit2.Tasks.NUnit2Task"/>) but also allows you to
	/// specify the location of the NUnit console application.
	/// </para>
	/// <para>
	/// The code proper is based on a combination of the <c>nunit2</c> task
	/// (<see cref="NAnt.NUnit2.Tasks.NUnit2Task"/>) and the <c>csc</c> task
	/// (<see cref="NAnt.DotNet.Tasks.CscTask"/>).
	/// </para>
	/// <para>
	/// Required child elements:
	/// </para>
	/// <list type="table">
	/// <listheader>
	/// <term>Element</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term><c>formatter</c> (<see cref="NAnt.NUnit.Types.FormatterElementCollection"/>)</term>
	/// <description>
	/// A <see cref="NAnt.NUnit.Types.FormatterElementCollection"/> with the set of
	/// formatters defining how to handle the test output.
	/// </description>
	/// </item>
	/// <item>
	/// <term><c>test</c> (<see cref="NAnt.NUnit2.Types.NUnit2TestCollection"/>)</term>
	/// <description>
	/// A <see cref="NAnt.NUnit2.Types.NUnit2TestCollection"/> with the set of
	/// tests to run.
	/// </description>
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
	/// <term><c>basedir</c> (<see cref="System.String"/>)</term>
	/// <description>
	/// The directory the program is in. Defaults to the project's base directory.
	/// If this is relative, it will be calculated relative to the project's base directory.
	/// </description>
	/// </item>
	/// <item>
	/// <term><c>program</c> (<see cref="System.String"/>)</term>
	/// <description>
	/// The path to the NUnit console application (without arguments).  Defaults to <c>nunit-console.exe</c>.
	/// </description>
	/// </item>
	/// <item>
	/// <term><c>haltonfailure</c> (<see cref="System.Boolean"/>)</term>
	/// <description>
	/// Stop the test run if a test fails.  Defaults to <see langword="false" />.
	/// </description>
	/// </item>
	/// <item>
	/// <term><c>workingdir</c> (<see cref="System.String"/>)</term>
	/// <description>
	/// The directory in which the command will be executed.  Defaults to the project's base directory.
	/// </description>
	/// </item>
	/// </list>
	/// <para>
	/// Again, the syntax is meant to be identical to that of the standard
	/// <c>nunit2</c> task with the exception of the name of the task (<c>nunitexec</c>)
	/// and the specification of the location of the NUnit console application.
	/// For more syntax information, consult the NAnt documentation.
	/// </para>
	/// </remarks>
	/// <example>
	/// <para>
	/// The following snippet is a sample of how the <c>nunit2</c> task might be
	/// seen in a NAnt build file:
	/// </para>
	/// <code>
	/// &lt;nunit2&gt;
	///   &lt;formatter type="Plain" /&gt;
	///   &lt;formatter type="Xml" usefile="true" extension=".xml" outputdir="..\build\log"/&gt;
	///   &lt;test assemblyname="MyTestAssembly.dll"&gt;
	///     &lt;categories&gt;
	///       &lt;include name="MyTestCategory"/&gt;
	///     &lt;/categories&gt;
	///   &lt;/test&gt;
	/// &lt;/nunit2&gt;
	/// </code>
	/// <para>
	/// To convert this to use the NUnit console to execute the tests, the simplest
	/// way is just to replace the task name with <c>nunitexec</c>, like this:
	/// </para>
	/// <code>
	/// &lt;nunitexec&gt;
	///   &lt;formatter type="Plain" /&gt;
	///   &lt;formatter type="Xml" usefile="true" extension=".xml" outputdir="..\build\log"/&gt;
	///   &lt;test assemblyname="MyTestAssembly.dll"&gt;
	///     &lt;categories&gt;
	///       &lt;include name="MyTestCategory"/&gt;
	///     &lt;/categories&gt;
	///   &lt;/test&gt;
	/// &lt;/nunitexec&gt;
	/// </code>
	/// <para>
	/// Note the only change is the name of the task executing.  This version
	/// assumes you have <c>nunit-console.exe</c> somewhere in the path, ready
	/// to go.  If you don't have it in your path, you'll need to add the <c>program</c>
	/// parameter to specify where it is:
	/// </para>
	/// <code>
	/// &lt;nunitexec program="path\to\nunit-console.exe"&gt;
	///   &lt;formatter type="Plain" /&gt;
	///   &lt;formatter type="Xml" usefile="true" extension=".xml" outputdir="..\build\log"/&gt;
	///   &lt;test assemblyname="MyTestAssembly.dll"&gt;
	///     &lt;categories&gt;
	///       &lt;include name="MyTestCategory"/&gt;
	///     &lt;/categories&gt;
	///   &lt;/test&gt;
	/// &lt;/nunitexec&gt;
	/// </code>
	/// <para>
	/// That's it - the output from the test execution should be the same format
	/// and in the same locations as you'd expect it to be if running the standard
	/// <c>nunit2</c> task.
	/// </para>
	/// </example>
	/// <seealso cref="NAnt.NUnit2.Tasks.NUnit2Task"/>
	/// <seealso cref="NAnt.DotNet.Tasks.CscTask"/>
	/// <seealso cref="NAnt.Core.Tasks.ExecTask" />
	/// <seealso cref="NAnt.Core.Tasks.ExternalProgramBase" />
	[TaskName("nunitexec")]
	public class NUnitExecTask : ExternalProgramBase
	{

		#region NUnitExecTask Variables

		#region Constants

		/// <summary>
		/// Default value for the NUnit console executable filename.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		public const string DefaultExeName = "nunit-console.exe";

		#endregion

		#region Instance

		/// <summary>
		/// Internal storage for the
		/// <see cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask.BaseDirectory" />
		/// property.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		private DirectoryInfo _baseDirectory;

		/// <summary>
		/// Internal storage for the
		/// <see cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask.FormatterElements" />
		/// property.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		private FormatterElementCollection _formatterElements = new FormatterElementCollection();

		/// <summary>
		/// Command-line options for the NUnit executable.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		private string _options;

		/// <summary>
		/// Internal storage for the
		/// <see cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask.FileName" />
		/// property.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		private string _program = DefaultExeName;

		/// <summary>
		/// Internal storage for the
		/// <see cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask.Tests" />
		/// property.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		private NUnit2TestCollection _tests = new NUnit2TestCollection();

		/// <summary>
		/// Internal storage for the
		/// <see cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask.WorkingDirectory" />
		/// property.
		/// </summary>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		private DirectoryInfo _workingDirectory;

		#endregion

		#endregion



		#region NUnitExecTask Properties

		/// <summary>
		/// Gets or sets the base directory to execute the tests in.
		/// </summary>
		/// <value>
		/// A <see cref="System.IO.DirectoryInfo"/> with the base directory
		/// to execute from.
		/// </value>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		[TaskAttribute("basedir")]
		public override DirectoryInfo BaseDirectory
		{
			get
			{
				if (this._baseDirectory == null)
				{
					return base.BaseDirectory;
				}
				return this._baseDirectory;
			}
			set
			{
				this._baseDirectory = value;
			}
		}

		/// <summary>
		/// Gets or sets the NUnit command to execute.
		/// </summary>
		/// <value>
		/// A <see cref="System.String"/> with the path to the NUnit command-line
		/// executable to run.  Defaults to <see cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask.DefaultExeName"/>.
		/// </value>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		[TaskAttribute("program", Required = false)]
		[StringValidator(AllowEmpty = false)]
		public string FileName
		{
			get
			{
				return _program;
			}
			set
			{
				this._program = StringUtils.ConvertEmptyToNull(value);
			}
		}

		/// <summary>
		/// Gets the set of formatters used for test output.
		/// </summary>
		/// <value>
		/// A <see cref="NAnt.NUnit.Types.FormatterElementCollection" /> with the
		/// set of formatters used for test output.
		/// </value>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		[BuildElementArray("formatter")]
		public FormatterElementCollection FormatterElements
		{
			get
			{
				return this._formatterElements;
			}
		}

		/// <summary>
		/// Gets or sets a flag indicating if the task should halt when it receives
		/// a test failure.
		/// </summary>
		/// <value>
		/// <see langword="true" /> to stop execution of additional tests when
		/// a failure is encountered; <see langword="false" /> to finish all
		/// test runs.  Default is <see langword="false" />.
		/// </value>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		[TaskAttribute("haltonfailure")]
		[BooleanValidator]
		public bool HaltOnFailure { get; set; }

		/// <summary>
		/// Gets the set of command-line arguments.
		/// </summary>
		/// <value>
		/// A <see cref="System.String"/> with the set of command-line arguments
		/// to be sent to the NUnit executable.
		/// </value>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		public override string ProgramArguments
		{
			get
			{
				return this._options;
			}
		}

		/// <summary>
		/// Gets the full path of the program to execute.
		/// </summary>
		/// <value>
		/// A <see cref="System.String"/> with the path to the NUnit executable
		/// that will be used to run the tests.
		/// </value>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		public override string ProgramFileName
		{
			get
			{
				if (Path.IsPathRooted(this.FileName))
				{
					return this.FileName;
				}
				if (this._baseDirectory == null)
				{
					string fullPathToExecutable = this.Project.GetFullPath(this.FileName);
					if (File.Exists(fullPathToExecutable))
					{
						return fullPathToExecutable;
					}
					return this.FileName;
				}
				return Path.GetFullPath(Path.Combine(this.BaseDirectory.FullName, this.FileName));
			}
		}

		/// <summary>
		/// Gets the set of tests to run.
		/// </summary>
		/// <value>
		/// A <see cref="NAnt.NUnit2.Types.NUnit2TestCollection"/> with the set
		/// of tests that should be executed by NUnit.
		/// </value>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		[BuildElementArray("test")]
		public NUnit2TestCollection Tests
		{
			get
			{
				return this._tests;
			}
		}

		/// <summary>
		/// Gets or sets the working directory for test execution.
		/// </summary>
		/// <value>
		/// A <see cref="System.IO.DirectoryInfo"/> with the working directory.
		/// </value>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		[TaskAttribute("workingdir")]
		public DirectoryInfo WorkingDirectory
		{
			get
			{
				if (this._workingDirectory == null)
				{
					return base.BaseDirectory;
				}
				return this._workingDirectory;
			}
			set
			{
				this._workingDirectory = value;
			}
		}

		#endregion



		#region NUnitExecTask Implementation

		#region Overrides

		/// <summary>
		/// Sets up and executes the NUnit executable.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If there are no formatters specified, a default/plain formatter is
		/// added to the collection and a warning is logged.
		/// </para>
		/// <para>
		/// For each test suite to run, for each assembly to test, the options
		/// for the NUnit console application are compiled using
		/// <see cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask.WriteOptions"/>
		/// and the NUnit console application is executed with those options.
		/// </para>
		/// <para>
		/// After the tests complete, the test output is written to the locations
		/// specified by any formatters.
		/// </para>
		/// </remarks>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		protected override void ExecuteTask()
		{
			if (this.FormatterElements.Count == 0)
			{
				FormatterElement defaultElement = new FormatterElement();
				defaultElement.Project = this.Project;
				defaultElement.NamespaceManager = base.NamespaceManager;
				defaultElement.Type = FormatterType.Plain;
				defaultElement.UseFile = false;
				this.FormatterElements.Add(defaultElement);
				this.Log(Level.Warning, "No <formatter .../> element was specified. A plain formatter was added to prevent losing output of the test results.");
				this.Log(Level.Warning, "Add a <formatter .../> element to the <nunitexec> task to prevent this warning from being output and to ensure forward compatibility with future revisions of NAnt.");
			}

			// Run the tests
			foreach (NUnit2Test test in this.Tests)
			{
				try
				{
					foreach (string testAssembly in test.TestAssemblies)
					{
						// Create temporary files to get the test output
						string xmlTempFile = Path.GetTempFileName();
						string plainTempFile = Path.GetTempFileName();

						try
						{
							// Get the options for the console app and run the tests,
							// collecting output to the temporary file for use in
							// plain formatters.
							this._options = this.WriteOptions(test, testAssembly, xmlTempFile);
							using (StreamWriter errWriter = File.AppendText(plainTempFile))
							{
								this.OutputWriter = errWriter;
								base.ExecuteTask();
							}
						}
						finally
						{
							// Process each specified formatter and write the output to the appropriate destination(s)
							foreach (FormatterElement formatter in this.FormatterElements)
							{
								string tempFilePath = null;
								switch (formatter.Type)
								{
									case FormatterType.Plain:
										tempFilePath = plainTempFile;
										break;
									case FormatterType.Xml:
										tempFilePath = xmlTempFile;
										break;
									default:
										this.Log(Level.Warning, "Unknown formatter type '{0}' - unable to process.", formatter.Type);
										break;
								}
								this.ProcessFormatter(formatter, tempFilePath, testAssembly);
							}

							// Clean up any temporary files
							if (!StringUtils.IsNullOrEmpty(xmlTempFile) && File.Exists(xmlTempFile))
							{
								File.Delete(xmlTempFile);
							}
							if (!StringUtils.IsNullOrEmpty(plainTempFile) && File.Exists(plainTempFile))
							{
								File.Delete(plainTempFile);
							}
						}
					}
				}
				catch (BuildException err)
				{
					// If the build needs to halt as soon as the test run fails, do that
					if (this.HaltOnFailure || test.HaltOnFailure)
					{
						throw new BuildException("Tests Failed.", this.Location, err);
					}
				}
			}
		}

		/// <summary>
		/// Initializes the task.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This initialization process validates that the specified NUnit path
		/// does not contain invalid path characters.
		/// </para>
		/// </remarks>
		/// <exception cref="NAnt.Core.BuildException">
		/// Thrown if <see cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask.FileName"/>
		/// contains invalid path characters.
		/// </exception>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		protected override void Initialize()
		{
			try
			{
				if (!Path.IsPathRooted(this.FileName))
				{
					// Do nothing - this simply checks for invalid path characters.
				}
			}
			catch (Exception err)
			{
				throw new BuildException(String.Format(CultureInfo.InvariantCulture, this.GetExecTaskString("NA1117"), new object[] { this.FileName, this.Name }), this.Location, err);
			}
			base.Initialize();
		}

		/// <summary>
		/// Prepares the executable process to run.
		/// </summary>
		/// <param name="process">The <see cref="System.Diagnostics.Process"/> that will prepare and execute.</param>
		/// <remarks>
		/// <para>
		/// This method sets the working directory on the process based on
		/// task settings.
		/// </para>
		/// </remarks>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		protected override void PrepareProcess(Process process)
		{
			base.PrepareProcess(process);
			process.StartInfo.WorkingDirectory = this.WorkingDirectory.FullName;
		}

		#endregion

		#region Methods

		#region Instance

		/// <summary>
		/// Retrieves a resource string from the NAnt assembly.
		/// </summary>
		/// <param name="name">The ID of the resource to retrieve.</param>
		/// <returns>A <see cref="System.String"/> with the value of the resource.</returns>
		/// <remarks>
		/// This mechanism is helpful in emulating certain functionality and
		/// standardizing messages used by the <see cref="NAnt.Core.Tasks.ExecTask"/>.
		/// </remarks>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		protected virtual string GetExecTaskString(string name)
		{
			Assembly nantCoreAsm = typeof(NAnt.Core.Tasks.ExecTask).Assembly;
			return ResourceUtils.GetString(name, null, nantCoreAsm);
		}

		/// <summary>
		/// Gets the filename to output with test results.
		/// </summary>
		/// <param name="formatter">The formatter specifying the output file information.</param>
		/// <param name="assemblyName">The test assembly executing to produce the output file.</param>
		/// <returns>
		/// A <see cref="System.String"/> with the appropriate full path to the
		/// test result output file, or <see langword="null" /> if no output file
		/// is required.
		/// </returns>
		/// <remarks>
		/// <para>
		/// Test result filenames are in the format <paramref name="assemblyName" />-results[extension].
		/// For example, if <paramref name="assemblyName" /> is <c>MyAssembly.dll</c>
		/// and <paramref name="formatter" /> specifies an <see cref="NAnt.NUnit.Types.FormatterElement.Extension"/> of <c>.xml</c>,
		/// the output filename will be <c>MyAssembly.dll-results.xml</c>.  If
		/// <paramref name="formatter" /> also specifies an <see cref="NAnt.NUnit.Types.FormatterElement.OutputDirectory"/>,
		/// the output filename will include that path information.
		/// </para>
		/// </remarks>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		protected virtual string GetOutputFilename(FormatterElement formatter, string assemblyName)
		{
			if (formatter == null || !formatter.UseFile || StringUtils.IsNullOrEmpty(assemblyName))
			{
				return null;
			}

			// Output filename format is [assembly]-results[extension]
			string resultFileName = assemblyName + "-results" + formatter.Extension;
			if (formatter.OutputDirectory != null)
			{
				resultFileName = Path.Combine(formatter.OutputDirectory.FullName, Path.GetFileName(resultFileName));
			}
			return resultFileName;
		}

		/// <summary>
		/// Writes the output to the designated formatter.
		/// </summary>
		/// <param name="formatter">The formatter specifying where the output noted should go.</param>
		/// <param name="outputTempFilePath">The temporary file containing the output to be written to the formatter.</param>
		/// <param name="assemblyName">The test assembly executing to produce the output file.</param>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		protected virtual void ProcessFormatter(FormatterElement formatter, string outputTempFilePath, string assemblyName)
		{
			if (formatter == null)
			{
				throw new ArgumentNullException("formatter", "The formatter to write output to may not be null.");
			}
			if (StringUtils.IsNullOrEmpty(outputTempFilePath) || !File.Exists(outputTempFilePath))
			{
				throw new ArgumentOutOfRangeException("outputTempFilePath", outputTempFilePath, "The output temporary file to write to the formatter does not exist.");
			}
			if (StringUtils.IsNullOrEmpty(assemblyName))
			{
				throw new ArgumentOutOfRangeException("assemblyName", assemblyName, "Unable to write output for an assembly with no name.");
			}

			// Handle file output
			if (formatter.UseFile)
			{
				string outputFilename = this.GetOutputFilename(formatter, assemblyName);
				if (outputFilename != null)
				{
					string outPath = Path.GetDirectoryName(outputFilename);
					if (!Directory.Exists(outPath))
					{
						this.Log(Level.Verbose, "Creating output directory [{0}].", outPath);
						Directory.CreateDirectory(outPath);
					}
					File.Copy(outputTempFilePath, outputFilename, true);
				}
				return;
			}

			// Handle non-file output (write to console)
			using (StreamReader sr = new StreamReader(outputTempFilePath))
			{
				sr.ReadLine();
				StringBuilder fileContents = new StringBuilder();
				while (sr.Peek() > -1)
				{
					fileContents.Append(sr.ReadLine().Trim()).Append(Environment.NewLine);
				}
				this.Log(Level.Info, fileContents.ToString());
			}
		}

		/// <summary>
		/// Writes the command-line options for a single assembly's worth of
		/// tests for the NUnit console executable.
		/// </summary>
		/// <param name="testToExecute">Test being executed by NUnit.</param>
		/// <param name="assemblyName">The specific assembly to write the options for.</param>
		/// <param name="xmlFilePath">The location to output XML formatted output.</param>
		/// <exception cref="System.ArgumentNullException">
		/// Thrown if <paramref name="testToExecute" /> is <see langword="null" />
		/// or if <paramref name="assemblyName" /> is <see langword="null" /> or
		/// <see cref="System.String.Empty"/>.
		/// </exception>
		/// <returns>
		/// A <see cref="System.String"/> with the command-line options to use
		/// with NUnit's console application to execute the test indicated by
		/// <paramref name="testToExecute" />.
		/// </returns>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		protected virtual string WriteOptions(NUnit2Test testToExecute, string assemblyName, string xmlFilePath)
		{
			if (testToExecute == null)
			{
				throw new ArgumentNullException("testToExecute", "Unable to create command-line option set for null test.");
			}
			if (StringUtils.IsNullOrEmpty(assemblyName))
			{
				throw new ArgumentNullException("assemblyName", "The name of the assembly to execute the test against may not be null.");
			}

			// No app.config can be specified - log message
			if (testToExecute.AppConfigFile != null)
			{
				this.Log(Level.Warning, "The <nunitexec> task does not support specifying an app.config for tests. Ignoring the attribute.");
			}

			StringWriter optionWriter = new StringWriter();
			try
			{
				// NOTE: Don't write the "err" or "output" values - it gets caught for the plain text
				// formatter as part of the task execution using this.OutputWriter.

				// Write the assemblies to the command line
				optionWriter.Write("\"{0}\" ", assemblyName);

				// Disable writing the logo - no need
				this.WriteOption(optionWriter, "nologo");

				// Target test execution to the framework being targeted for the build
				// (Had some reports with problems using this in later versions of NAnt so commented out for now.)
				// this.WriteOption(optionWriter, "framework", String.Format(CultureInfo.InvariantCulture, "v{0}", this.Project.TargetFramework.ClrVersion));

				// Only one XML output file is allowed
				if (!StringUtils.IsNullOrEmpty(xmlFilePath))
				{
					this.WriteOption(optionWriter, "xml", xmlFilePath);
				}

				// Specific test fixture to execute
				if (testToExecute.TestName != null)
				{
					this.WriteOption(optionWriter, "fixture", testToExecute.TestName);
				}


				// Transform file for plain output
				if (testToExecute.XsltFile != null)
				{
					this.WriteOption(optionWriter, "transform", testToExecute.XsltFile.FullName);
				}

				// Categories to include/exclude (CategoryCollection automatically comma-delimits)
				string includes = testToExecute.Categories.Includes.ToString();
				if (!StringUtils.IsNullOrEmpty(includes))
				{
					this.WriteOption(optionWriter, "include", includes);
				}
				string excludes = testToExecute.Categories.Excludes.ToString();
				if (!StringUtils.IsNullOrEmpty(excludes))
				{
					this.WriteOption(optionWriter, "exclude", excludes);
				}

				return optionWriter.ToString();
			}
			finally
			{
				optionWriter.Close();
			}
		}

		/// <summary>
		/// Writes a name-only option for the NUnit console executable.
		/// </summary>
		/// <param name="writer">The <see cref="System.IO.StringWriter"/> to write the option to.</param>
		/// <param name="name">The option to write.</param>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		protected virtual void WriteOption(StringWriter writer, string name)
		{
			writer.Write("/{0} ", name);
		}

		/// <summary>
		/// Writes a name/value option for the NUnit console executable.
		/// </summary>
		/// <param name="writer">The <see cref="System.IO.StringWriter"/> to write the option to.</param>
		/// <param name="name">The option to write.</param>
		/// <param name="arg">The value for the option.</param>
		/// <seealso cref="Paraesthesia.Tools.NAntTasks.NUnitExec.NUnitExecTask" />
		protected virtual void WriteOption(StringWriter writer, string name, string arg)
		{
			writer.Write("/{0}=\"{1}\" ", name, arg);
		}

		#endregion

		#endregion

		#endregion
	}
}
