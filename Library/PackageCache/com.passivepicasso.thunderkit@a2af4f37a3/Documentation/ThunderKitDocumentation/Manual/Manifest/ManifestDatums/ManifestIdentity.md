---
{ 
	"title" : "ManifestIdentity",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_Manifest_2X_Icon" ]
}

---

The [ManifestIdentity](assetlink://GUID/f22bb7fd1d3b56a48bc52f8e407901d6) stores unique identifying information used by ThunderKit to construct dependency information for package stores and mod loaders.

### Fields
* **Author**
  - The name of the developer or team responsible for developing and releasing this package.
* **Name**
  - The name of this package, this is the dependency name and can only contain valid path characters and except for spaces
* **Version**
  - The current in development version of this pacakge.
* **Description**
  - A short description of the package
* **Icon**
  - Image for the package, used by some Package Sources like Thunderstore
* **Dependencies**
  - A list of Manifests for packages this Manifest depends on

## Inherited Fields

* **Staging Paths**
  - A list of destinations to deploy files to
  - Supports PathReferences
  
## PipelineJobs

* [StageDependencies](documentation://GUID/af852fc5b31304e498e9def1c01db5c1)
  - Uses the ManifestIdentity.Dependencies array to deploy dependencies loaded by the ThunderKit Package Manager

* [StageThunderstoreManifest](documentation://GUID/74a0394c4eaea384e89e7a3688053c2b) 
  - Uses ManifestIdentity information to construct a manifest json file for Thunderstore

## Remarks

The ManifestIdentity contains the dependencies for each mod as well as some common identifying information for package distribution sites like Thunderstore and Mod.io

The ManifestIdentity is required on every Manifest in order to conduct most build operations.

This is because PipelineJobs will use the dependency hierarchy to discover all assets that need to be built or copied to Staging Paths.

To find external dependencies use the [ThunderKit Package Manager](menulink://Tools/ThunderKit/Packages)