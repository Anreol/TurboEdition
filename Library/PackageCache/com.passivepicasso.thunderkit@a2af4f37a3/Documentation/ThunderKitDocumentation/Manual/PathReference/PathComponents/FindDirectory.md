---
{ 
	"title" : "FindDirectory",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_PathReference_2X_Icon" ]
}

---

[Find Directory](assetlink://GUID/d4fa9e4f9ff58a344b7da26cb499b387) will find and return the name of a Directory

## Fields

* **Search Option**
  - Option to search only top directory or all sub directories as well
* **Search Pattern**
  - pattern to search for in file and folder names
* **Path**
  - Root of path to search

## Remarks

The Path field will process PathReferences which will be resolved before being passed to Directory.EnumerateFiles.

This will return the name of the specified directory, not its full path.
 
Find Directory is a light wrapper over [Directory.EnumerateFiles](https://docs.microsoft.com/en-us/dotnet/api/system.io.directory.enumeratefiles?view=netframework-4.6)
