using System.Reflection;
using System.Runtime.InteropServices;
using ApprovalTests.Reporters;

[assembly: AssemblyTitle("GitReleaseNotes.Tests")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("GitReleaseNotes.Tests")]
[assembly: AssemblyCopyright("Copyright ©  2013")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("4d174be3-7573-4d93-a238-97f857e5305a")]


[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: UseReporter(typeof(DiffReporter))]