---
{ 
	"title" : "StageAssetBundles",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_Pipeline_2X_Icon" ]
}

---

[StageAssetBundles](assetlink://GUID/924ee63e6c016f14d8a1560b288f15a3) is an AssetBundle build system

## Fields

* **Asset Bundle Build Options**
  - These options control various aspects of how asset bundles build
  - Refer to the [Unity Scripting API](https://docs.unity3d.com/ScriptReference/BuildAssetBundleOptions.html) for details
* **Build Target**
  - The platform that you're building content for
* **Recurse Directories**
  - Recurse directories added to AssetBundleDefinitions asset lists
* **Simulate**
  - Prepares a bundle build and outputs what asset will go into what bundles to the Console window
  - A visual interface will be being added to provide this information as well
* **Bundle Artifact Path**
  - This is the path where AssetBundles will be cached to prevent unnecessary rebuilding of bundles

## Required ManifestDatums

* [AssetBundleDefinitions](documentation://GUID/b3d3f798ec15f8240ad5105c46ce59f5)

## Remarks

StageAssetBundles is a Manifest aware AssetBundle building job. 

This job executes once on the pipeline, but conducts dependency resolution across all manifests in the manifest hierarchy
Each Manifest with AssetBundleDefinitions defined in the hierarchy will have all its defined AssetBundles built

This can establish a dependency relationship between bundles for different manifests, verify that you're getting the results you expect

If you need to rebuild all your AssetBundles, select the Force Rebuild AssetBundle option in the Asset Bundle Build Options field.

The fastest but largest AssetBundles to build are ones built with the [BuildAssetBundleOptions.UncompressedAssetBundle](https://docs.unity3d.com/ScriptReference/BuildAssetBundleOptions.UncompressedAssetBundle.html) option

If you want to get information about your AssetBundle dependency hiercharies quickly, use the [BuildAssetBundleOptions.DryRunBuild](https://docs.unity3d.com/ScriptReference/BuildAssetBundleOptions.DryRunBuild.html)