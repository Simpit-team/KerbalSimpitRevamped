# Building Kerbal Simpit

There is currently two ways of building Simpit.

## Building using Visual Studio

1. Set the KSPDIR environment variable to the version of KSP you want to build against. KSPDIR should be set to the path containing the KSP executable
2. Open the KerbalSimpit.sln and compile it.
3. The DLL is built in bin\Debug by default. If KSPDIR is set and Simpit is installed, the Simpit DLL will be overwritten to update Simpit.

## Building using Linux or Mmono and WSL on Windows

This method is not currently up to date with the addition of the keyboard emulator

1. Have WSL enabled, running Debian 10
2. Run: sudo apt install build-essential
3. Install mono complete as instructed here: https://www.mono-project.com/download/stable/#download-lin-debian
4. Create the required symlinks from your KSP install directory to the directory of this project
    * For Windows/WSL (In an Admin CMD Prompt): 
        - mklink /D "C:\Path\To\Local\Repo\KerbalSpaceProgram" "C:\Path\To\KSP\Root\Install"
        - mklink /D "C:\Path\To\Local\Repo\install" "C:\Path\To\KSP\Root\Install\GameData"
    * For Linux:
        - ln -s /path/to/steamapps/common/Kerbal Space Program    /path/to/this/project/KerbalSpaceProgram
        - ln -s /path/to/steamapps/common/Kerbal Space Program/GameData    /path/to/this/project/install
5. Run: make install, to build and install the DLL

6. Try the above steps, and if they do not work, also try installing dotnet, following the install instructions here: https://docs.microsoft.com/en-gb/dotnet/core/install/linux

# Making a release

Set the version number and the KSP compatibility in `VERSION.txt`. Set the same version number in the Arduino lib library.properties.
Update `CHANGELOG.md`.
Check that the Arduino repo is up to date (it is assumed to be in the same parent folder than the C# repo) and that there is no non-commited file.

Compile the project in Release mode. This will automatically call the `make_release.bat` script that will create a folder and a zip in the "NextReleases" folder. 
Test that the created folder, once copied into GameData, is working
Commit and push the last commit
Create a release on GitHub referecing this commit, tag it as "vX.Y.Z" and upload the created zip.
Tag the current Arduino commit with the same version tag and push it.
