export PATH := /usr/local/bin:$(PATH)
XBUILD=xbuild
CONFIG=Release

KSPDIR=/Users/peter/Library/Application\ Support/Steam/steamapps/common/Kerbal\ Space\ Program
INSTALLDIR=$(KSPDIR)/GameData/KerbalSimPit
CONFIGDIR=$(INSTALLDIR)/PluginData/KerbalSimPit

PLUGINVERSION=$(shell egrep "^[.*AssemblyVersion" Properties/AssemblyInfo.cs|cut -d\" -f2)
PACKAGEDIR=package/KerbalSimPit
PACKAGECONFIGDIR=$(PACKAGEDIR/PluginData/KerbalSimPit

all:KerbalSimPit.dll

KerbalSimPit.dll:
	$(XBUILD) /p:Configuration=$(CONFIG)

install: # TODO

clean:
	$(XBUILD) /p:Configuration=$(CONFIG) /t:Clean

package: all # TODO