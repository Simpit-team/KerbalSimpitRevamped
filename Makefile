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
	KSPDIR=/Users/peter/KerbalSpaceProgram
endif

# KSPLIBDIR
# Path to the KSP managed libraries. Again, setting KSPLIBDIR in
# the environment will overwrite this.
ifndef KSPLIBDIR
	# This default works for macOS
	KSPLIBDIR=$(KSPDIR)/KSP.app/Contents/Resources/Data/Managed
	# On Linux, comment out the above line and try this
	# KSPLIBDIR=$(KSPDIR)/KSP_Data/Managed
endif

# Things less likely to need configuring:

# INSTALLDIR
# Where to install the plugin
INSTALLDIR=$(KSPDIR)/GameData/KerbalSimpit
# CONFIGDIR
# Where the plugin's configuration files are stored
CONFIGDIR=$(INSTALLDIR)/PluginData/KerbalSimpit

# Shouldn't need to change variables below here
XBUILD=xbuild
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

all:KerbalSimpitSerial.dll KerbalSimpit.dll

KerbalSimpit.dll:Properties/AssemblyInfo.cs
	$(XBUILD) /p:Configuration=$(CONFIG) Main.csproj

KerbalSimpitSerial.dll:Properties/SerialAssemblyInfo.cs
	$(XBUILD) /p:Configuration=$(CONFIG) Serial.csproj

install:all
	cp Bin/KerbalSimpit.dll $(INSTALLDIR)
	cp Bin/KerbalSimpitSerial.dll $(INSTALLDIR)
	cp Bin/Mono.Posix.dll $(INSTALLDIR)

clean:
	$(XBUILD) /p:Configuration=$(CONFIG) /t:Clean Main.csproj
	$(XBUILD) /p:Configuration=$(CONFIG) /t:Clean Serial.csproj
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

Properties/SerialAssemblyInfo.cs:Properties/SerialAssemblyInfo.cs.m4 version-info.m4
	m4 -DBUILDVER=$(BUILDVERSION) version-info.m4 Properties/SerialAssemblyInfo.cs.m4 > Properties/SerialAssemblyInfo.cs

package: all KerbalSimpit.version
	mkdir -p $(PACKAGEDIR)
	cp Bin/*.dll $(PACKAGEDIR)
	cp KerbalSimpit.version $(PACKAGEDIR)
	cp -r distrib/* $(PACKAGEDIR)
	cd package; zip -r -9 ../$(ZIPNAME) KerbalSimpit
	rm -r package

