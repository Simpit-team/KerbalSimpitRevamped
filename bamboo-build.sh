#!/bin/bash
set +eu
set -o pipefail

# Intended only to be run by the main build server,
# it's very full of environment-specific things.
export KSPDIR=/srv/home/peter/KerbalSpaceProgram
export KSPLIBDIR=${KSPDIR}/KSP_Data/Managed

# I've been using the Bamboo build number as a version for
# assorted things. But Bamboo does not have a good way to
# manually set this, so it's been reset across a couple of
# rebuilds. Use an epoch to increment it manually:
if [ -v bamboo_buildNumber ]; then
    BAMBOOEPOCH=40
    let PLUGINVERSION=$BAMBOOEPOCH+${bamboo_buildNumber}
    export PLUGINVERSION
fi

make package
