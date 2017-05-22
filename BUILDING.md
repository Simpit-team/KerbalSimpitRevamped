# Building Kerbal Sim Pit

## Compiling:


## Building a release:

### Prepare the release

* Update `CHANGELOG.md`.
* Update version in `version-info.m4`.
* Create a version git tag.
* Push everything, including tags.

### Build and upload new zip file.

* Run the [KSPIT-KSPIT](https://home.hardy.dropbear.id.au/bamboo/browse/KSPIT-KSPIT)
  Bamboo build plan. This will generate a release artefact.
* Run the [KerbalSimPit deployment](https://home.hardy.dropbear.id.au/bamboo/deploy/viewDeploymentProjectEnvironments.action?id=950273)
  deployment project. This will take the release artefact generated above
  and upload it to the pool directory on the server.
  
### Build and deploy metadata.

* Run the [KSPIT-CKAN](https://home.hardy.dropbear.id.au/bamboo/browse/KSPIT-CKAN)
  Bamboo build plan. This takes version info exported by the KSPIT build,
  builds a `.netkan` file, and runs netkan over it to generate a CKAN file.
  It then retrieves the archive from the last build, and adds the new
  CKAN file to it.
* Run the [KerbalSimPit CKAN](https://home.hardy.dropbear.id.au/bamboo/deploy/viewDeploymentProjectEnvironments.action?id=950274)
  deployment project. This takes the refreshed archive from the last step,
  and uploads it along with the version file.
