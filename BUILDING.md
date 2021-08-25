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

Set the version number
Update `CHANGELOG.md`.
Tag the commit with the version and push it
Create a folder with the DLLS, the icons, the localisation files, etc.
Zip it and upload to GitHub
Update CKAN metadata information

Make a matching Arduino lib release ?

TODO Automate it.

# Previous content of BUILDING.md. No longer used.

## Building a release:

### Prepare the release

* Update `CHANGELOG.md`.
* Update version in `version-info.m4`.
* Create a version git tag.
* Push everything, including tags.

### Build and upload new zip file.

* Run the [KSPIT-KSPIT](https://home.hardy.dropbear.id.au/bamboo/browse/KSPIT-KSPIT)
  Bamboo build plan. This will generate a release artefact.
* Run the [KerbalSimpit deployment](https://home.hardy.dropbear.id.au/bamboo/deploy/viewDeploymentProjectEnvironments.action?id=950273)
  deployment project. This will take the release artefact generated above
  and upload it to the pool directory on the server.

### Build and deploy metadata.

* Run the [KSPIT-CKAN](https://home.hardy.dropbear.id.au/bamboo/browse/KSPIT-CKAN)
  Bamboo build plan. This takes version info exported by the KSPIT build,
  builds a `.netkan` file, and runs netkan over it to generate a CKAN file.
  It then retrieves the archive from the last build, and adds the new
  CKAN file to it.
* Run the [KerbalSimpit CKAN](https://home.hardy.dropbear.id.au/bamboo/deploy/viewDeploymentProjectEnvironments.action?id=950274)
  deployment project. This takes the refreshed archive from the last step,
  and uploads it along with the version file.
