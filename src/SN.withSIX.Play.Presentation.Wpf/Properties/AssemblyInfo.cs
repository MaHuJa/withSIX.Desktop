// <copyright company="SIX Networks GmbH" file="AssemblyInfo.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("Play withSIX")]
[assembly: AssemblyDescription("Play withSIX Presentation")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("SIX Networks")]
[assembly: AssemblyMetadata("SquirrelAwareVersion", "1")]

#if MAIN_RELEASE
[assembly: AssemblyProduct("Play withSIX")]
#else
#if BETA_RELEASE
[assembly: AssemblyProduct("Play withSIX BETA")]
#else
#if NIGHTLY_RELEASE
[assembly: AssemblyProduct("Play withSIX ALPHA")]
#else

[assembly: AssemblyProduct("Play withSIX DEV")]
#endif
#endif
#endif

[assembly: AssemblyCopyright("Copyright © SIX Networks 2009-2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

//In order to begin building localizable applications, set 
//<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page, 
    // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
    //(used if a resource is not found in the page, 
    // app, or any theme specific resource dictionaries)
    )]


// Version information for an assembly consists of the following four values:   
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.5.*")]

[assembly: AssemblyVersion("1.51.0.666")]
[assembly: AssemblyFileVersion("1.51.0.666")]
[assembly: AssemblyInformationalVersion("1.51.0-dev201501241")]
[assembly: InternalsVisibleTo("SN.withSIX.Play.Tests.Presentation")]
// Needed for Infrastructure.Api when merged with S.A.

[assembly: InternalsVisibleTo("ImpromptuInterfaceDynamicAssembly")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("Castle.Core")]
[assembly: InternalsVisibleTo("ShortBus")] // required for internal handlers like the decorators...