@ECHO OFF
REM Used to create the AssemblyInfo.cs file as a pre build event for compiling Simpit
REM Arguments : ProjectDir

set ProjectDir=%1

REM Read the version number
for /f "delims== tokens=1,2" %%G in (%ProjectDir%VERSION.txt) do set %%G=%%H

echo Version read %MAJOR%.%MINOR%.%PATCH%.%BUILD%

REM use git to get a build ID, based on current commit. Currently not used as C# mandate the build id to be an int
REM FOR /F %%i in ('git describe --long --dirty --always') do set BUILD=%%i

REM Create the assemble info file
REM the [ seemed to cause a mess when creating this file in batch, so we create it line by line
REM it would be better to use the AssemblyInfo.cs.m4 template and update it instead
echo using System.Reflection;>"%ProjectDir%Properties\AssemblyInfo.cs"
echo using System.Runtime.CompilerServices;>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo using System.Runtime.InteropServices;>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo:>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo // General Information about an assembly is controlled through the following>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo // set of attributes. Change these attribute values to modify the information>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo // associated with an assembly.>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo [assembly: AssemblyTitle("KerbalSimpit")]>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo [assembly: AssemblyDescription("")]>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo [assembly: AssemblyConfiguration("")]>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo [assembly: AssemblyCompany("")]>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo [assembly: AssemblyProduct("KerbalSimpit")]>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo [assembly: AssemblyCopyright("")]>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo [assembly: AssemblyTrademark("")]>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo [assembly: AssemblyCulture("")]>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo:>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo // Setting ComVisible to false makes the types in this assembly not visible>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo // to COM components. If you need to access a type in this assembly from>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo // COM, set the ComVisible to true on that type.>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo [assembly: ComVisible(false)]>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo:>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo // The following GUID is for the ID of the typelib if this project is>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo // exposed to COM.>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo [assembly: Guid("8cab9269-eb8f-466d-b24e-638a66672650")]>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo:>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo // Version information for an assembly consists of the following four values:>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo //      Major Version>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo //      Minor Version>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo //      Build Number>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo //      Revision>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo //>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo // You can specify all the values or you can default the Build and Revision>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo // numbers by using the '*' as shown below:>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo // [assembly: AssemblyVersion("1.0.*")]>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo [assembly: AssemblyVersion("%MAJOR%.%MINOR%.%PATCH%.%BUILD%")]>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo [assembly: AssemblyFileVersion("%MAJOR%.%MINOR%.%PATCH%.%BUILD%")]>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo:>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo // KSPAssembly identifies the assembly title and version to KSP>>"%ProjectDir%Properties\AssemblyInfo.cs"
echo [assembly: KSPAssembly ("KerbalSimpit", %MAJOR%, %MINOR%)]>>"%ProjectDir%Properties\AssemblyInfo.cs"
