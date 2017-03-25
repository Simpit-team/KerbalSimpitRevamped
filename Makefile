export PATH := /usr/local/bin:$(PATH)
XBUILD=xbuild
CONFIG=Release

KSPDIR=/Users/peter/Library/Application\ Support/Steam/steamapps/common/Kerbal\ Space\ Program
INSTALLDIR=$(KSPDIR)/GameData/KerbalSimPit
CONFIGDIR=$(INSTALLDIR)/PluginData/KerbalSimPit

PACKAGEDIR=package/KerbalSimPit
PACKAGECONFIGDIR=$(PACKAGEDIR/PluginData/KerbalSimPit

ifdef PLUGINVERSION
	ZIPNAME=KerbalSimPit-$(PLUGINVERSION).zip
else
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

package: all
	mkdir -p $(PACKAGEDIR)
	cp Bin/*.dll $(PACKAGEDIR)
	cd package; zip -r -9 ../$(ZIPNAME) KerbalSimPit
	rm -r package

