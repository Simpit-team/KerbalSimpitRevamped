######################################################################
#
# Kerbal SimPit Makefile
#
######################################################################

# Configurable paths

# KSPDIR
# This should be the path to a valid Kerbal Space Program installation.
# Defining KSPDIR in the environment will override this.
ifndef KSPDIR
	KSPDIR=KerbalSpaceProgram
endif

# KSPLIBDIR
# Path to the KSP managed libraries. Again, setting KSPLIBDIR in
# the environment will overwrite this.
ifndef KSPLIBDIR
	# This default works for macOS
	#KSPLIBDIR=$(KSPDIR)/KSP.app/Contents/Resources/Data/Managed
	# On Linux, comment out the above line and try this
	 KSPLIBDIR=$(KSPDIR)/KSP_x64_Data/Managed
endif

# Things less likely to need configuring:

# INSTALLDIR
# Where to install the plugin
INSTALLDIR= install
# CONFIGDIR
# Where the plugin's configuration files are stored
CONFIGDIR=$(KSPDIR)/PluginData/KerbalSimpit

# Shouldn't need to change variables below here
MSBUILD=msbuild
CONFIG=Release

PACKAGEDIR=package/KerbalSimpit

ifdef PLUGINVERSION
	BUILDVERSION=$(PLUGINVERSION)
	ZIPNAME=KerbalSimpit-$(PLUGINVERSION).zip
else
	BUILDVERSION=0
	ZIPNAME=KerbalSimpit.zip
endif

export KSPDIR
export KSPLIBDIR

all:KerbalSimpit.dll

KerbalSimpit.dll:Properties/AssemblyInfo.cs
	$(MSBUILD) /p:Configuration=$(CONFIG) Main.csproj

install:all
	cp Bin/KerbalSimpit.dll $(INSTALLDIR)

clean:
	$(MSBUILD) /p:Configuration=$(CONFIG) /t:Clean Main.csproj
	rm -f KerbalSimpit.version
	rm -f Properties/AssemblyInfo.cs
	rm -f Properties/SerialAssemblyInfo.cs
	rm -f *.zip

KerbalSimpit.version:KerbalSimpit.version.m4 version-info.m4
	m4 -DBUILDVER=$(BUILDVERSION) version-info.m4 KerbalSimpit.version.m4 > KerbalSimpit.version

validate:KerbalSimpit.version
	jq '.' KerbalSimpit.version > /dev/null

Properties/AssemblyInfo.cs:Properties/AssemblyInfo.cs.m4 version-info.m4
	m4 -DBUILDVER=$(BUILDVERSION) version-info.m4 Properties/AssemblyInfo.cs.m4 > Properties/AssemblyInfo.cs

package: all KerbalSimpit.version
	mkdir -p $(PACKAGEDIR)
	cp Bin/*.dll $(PACKAGEDIR)
	cp KerbalSimpit.version $(PACKAGEDIR)
	cp -r distrib/* $(PACKAGEDIR)
	cd package; zip -r -9 ../$(ZIPNAME) KerbalSimpit
	rm -r package
