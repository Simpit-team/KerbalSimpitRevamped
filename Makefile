export PATH := /usr/local/bin:$(PATH)
XBUILD=xbuild
CONFIG=Release

ifndef KSPDIR
	KSPDIR=/Users/peter/Library/Application\ Support/Steam/steamapps/common/Kerbal\ Space\ Program
endif
INSTALLDIR=$(KSPDIR)/GameData/KerbalSimpit
CONFIGDIR=$(INSTALLDIR)/PluginData/KerbalSimpit

PACKAGEDIR=package/KerbalSimpit

ifdef PLUGINVERSION
	BUILDVERSION=$(PLUGINVERSION)
	ZIPNAME=KerbalSimpit-$(PLUGINVERSION).zip
else
	BUILDVERSION=0
	ZIPNAME=KerbalSimpit.zip
endif

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

