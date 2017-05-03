export PATH := /usr/local/bin:$(PATH)
XBUILD=xbuild
CONFIG=Release

KSPDIR=/Users/peter/Library/Application\ Support/Steam/steamapps/common/Kerbal\ Space\ Program
INSTALLDIR=$(KSPDIR)/GameData/KerbalSimPit
CONFIGDIR=$(INSTALLDIR)/PluginData/KerbalSimPit

PACKAGEDIR=package/KerbalSimPit
PACKAGECONFIGDIR=$(PACKAGEDIR/PluginData/KerbalSimPit

ifdef PLUGINVERSION
	BUILDVERSION=$(PLUGINVERSION)
	ZIPNAME=KerbalSimPit-$(PLUGINVERSION).zip
else
	BUILDVERSION=0
	ZIPNAME=KerbalSimPit.zip
endif

all:KerbalSimPitSerial.dll KerbalSimPit.dll

KerbalSimPit.dll:Properties/AssemblyInfo.cs
	$(XBUILD) /p:Configuration=$(CONFIG) Main.csproj

KerbalSimPitSerial.dll:
	$(XBUILD) /p:Configuration=$(CONFIG) Serial.csproj

install:all
	cp Bin/KerbalSimPit.dll $(INSTALLDIR)
	cp Bin/KerbalSimPitSerial.dll $(INSTALLDIR)
	cp Bin/Mono.Posix.dll $(INSTALLDIR)

clean:
	$(XBUILD) /p:Configuration=$(CONFIG) /t:Clean Main.csproj
	$(XBUILD) /p:Configuration=$(CONFIG) /t:Clean Serial.csproj
	rm -f KerbalSimPit.version
	rm -f Properties/AssemblyInfo.cs
	rm -f *.zip

KerbalSimPit.version:KerbalSimPit.version.m4 version-info.m4
	m4 -DBUILDVER=$(BUILDVERSION) version-info.m4 KerbalSimPit.version.m4 > KerbalSimPit.version

Properties/AssemblyInfo.cs:Properties/AssemblyInfo.cs.m4 version-info.m4
	m4 -DBUILDVER=$(BUILDVERSION) version-info.m4 Properties/AssemblyInfo.cs.m4 > Properties/AssemblyInfo.cs

package: all KerbalSimPit.version
	mkdir -p $(PACKAGEDIR)
	cp Bin/*.dll $(PACKAGEDIR)
	cp KerbalSimPit.version $(PACKAGEDIR)
	cd package; zip -r -9 ../$(ZIPNAME) KerbalSimPit
	rm -r package

