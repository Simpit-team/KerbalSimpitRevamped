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

all:KerbalSimPit.dll

KerbalSimPit.dll:
	$(XBUILD) /p:Configuration=$(CONFIG)

install:all
	cp Bin/KerbalSimPit.dll $(INSTALLDIR)
	cp Bin/SerialPortLib2.dll $(INSTALLDIR)

clean:
	$(XBUILD) /p:Configuration=$(CONFIG) /t:Clean
	rm -f KerbalSimPit.version
	rm -f *.zip

KerbalSimPit.version:
	m4 -DBUILDVER=$(BUILDVERSION) KerbalSimPit.version.m4 > KerbalSimPit.version

package: all KerbalSimPit.version
	mkdir -p $(PACKAGEDIR)
	cp Bin/*.dll $(PACKAGEDIR)
	cp KerbalSimPit.version $(PACKAGEDIR)
	cd package; zip -r -9 ../$(ZIPNAME) KerbalSimPit
	rm -r package

