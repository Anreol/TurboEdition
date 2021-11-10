## 4.1.1

### Pipelines and Logging

This update introduces a system to maintain logs of Pipeline runs. These logs saved under the Assets/ThunderKitAssets/Logs folder and grouped by Pipeline.  Pipeline Logs are rich data sets which provides listings of runtime function and reporting of build artifacts. Logs will show you what was done during a pipeline run, what files were copied, deleted, and created during that run.

The pipeline logs will additionally show any errors, and provide any potentially relevant data from the errors that could lead to resolution. These errors are enhanced by ThunderKit's markdown system, allowing you to click on source code lines to open up your code editor to the source of errors for further debugging.  This should help developers who extend ThunderKit with custom PipelineJobs and PathComponents.

The most recent log for a pipeline can be launched by inspecting the Pipeline then clicking on the Show Log button.

The execute button for pipelines have been moved from under the "Add Pipeline Button" to the top left of the Pipeline Inspector. This should reduce incidents of accidentally firing off the Pipeline.

### Markdown Level Up

Text alignment and kerning has been improved significantly.  I'm sorry for any mental anguish users have suffered.

The Markdown implementation performance and output quality has been significantly improved. Previously the UIElementsRenderer would break all string literals on whitespace separation and then render each word as an individual VisualElement of type Label. This provided an easy way to achieve flow document layouting, however resulted in large documents taking an exceptionally long time to render.

In this update the UIElementsRenderer will now scan each ParagraphBlock returned by MarkDig and if the Paragraph contains only simple string literals will opt to render the entire paragraph in a single Label.  This reduces the number of elements generated in large documents by thousands. This results in significantly improved render times in large documents as well as faster layouting.

Additionally, the Markdown system now supports adding custom Schemes for Markdown links from external libraries which has enabled new features in ThunderKit.

Finally the code design of the MarkdownElement and its utilization has been improved to prevent cases where Markdown doesn't have the necessary visual styles to render correctly.

### Documentation Improvements

The Markdown improvements has allowed the introduction of Documentation page links to be created. Now MarkdownElements can link to specific documentation pages.  This hasn't been applied to all documentation to create a highly connected document graph yet, but additional enhancements to documentation will be done over time.

Some documents have been reformatted to improve their layout flexibility

### Fixes and Improvements

* Automatically generate a PackageSource for the ThunderKit Extensiosn Thunderstore

* Remove ThunderKit.Core.Editor namespace due to code clarity issues a namespace named Editor creates in Unity

* Fix bugs with Pipeline flow related to Asynchronous migration

* Fix a number of cases where Exception context could be hidden

* Add a new toggle to Copy jobs that indicates if the Job should try to create the target directory, default value is true

* Fixed some cases where Pipelines would run to the end instead of halting when encountering what should have been a fatal exception

* StageAssetBundles and StageAssemblies logging and code flow has been improved to clarify common cases where these jobs will fail to execute correct

* Added and improved logging to Copy, Delete, ExecutePipepline, ExecuteProcess, StageAssemblies, StageAssetBundles, StageDependencies, StageManifestFiles, Zip and StageThunderstoreManifest

* Fix issue where SteamBepInExLaunch could fail to start due to formatting of command line args

* Fix issue in Zip that could cause the job to fail in a case it shouldn't


## 4.0.0

### Important

This update is breaking support for .NET 3.5 due to the difficulty in providing functional tools for certain aspects of Unity which are asynchronous.
For people who need .NET 3.5 support, install ThunderKit using the net35compatibility branch which will receive fixes able to be ported upon request

`"com.passivepicasso.thunderkit":"https://github.com/PassivePicasso/ThunderKit.git#net35compatibility",` 

This update changes how Manifest assets in the Unity AssetDatabase are managed. You will be asked to run an upgrade process that will update all your Manifests to the new configuration automatically.
Please make sure you back up your projects before updating in case of any problems.

Some games do not have their Unity game version properly identified in their executable. Due to this, ThunderKit will now read the games globalgamemanager file to identify the correct Unity version for the game.  Some users may find they need to switch unity versions because of this change, but it is a necessary step to take to avoid unforseen issues.

### Known Issues

* Unity 2021.2.0b7 does not detect package installation or uninstallation automatically requiring the user to manually refresh the Project
This is an issue which appears to be a bug with Unity's AssetDatabase.Refresh call and a bug report will be generated for Unity Technologies to investigate.
This bug may be resolved in newer versions of the Unity 2021.2 beta, however there are no games available to test against which wouldn't introduce factors that could muddle results.
If Unity doesn't appear to import packages installed from Thunderstore, or doesn't appear to fully remove an uninstalled package, refresh your project using the context menu option in the Project window, or on windows press Ctrl+R

* Unity 2021.2.0b7 locks up when importing and loading assemblies from packages or games.
  - Work-around: Kill the Unity process after it seems like the import process has stopped loading new assemblies and restart Unity

### Improvements

* Unity 2021.2 beta can now succesfully install packages, however the user must manually refresh the project (Ctrl+R) to complete the installation.

* Pipelines and PipelineJobs now execute asynchronously to support operations which require that Unity take control of processing.

* StageAssemblies previously relied on simply copying assemblies from the project's Library/ScriptAssemblies folder. While fast and convenient this prevented users from taking control of build parameters which may be necessary for their projects.  StageAssemblies now allows you to specify Build Targets and Build Target Groups in addition to allowing you to stage debug databases. Due to this change StageAssemblies now builds player assemblies, allowing the utilization of available optimization steps the compilation engine provides.

* Package Installation has been improved to no longer utilize the AssetDatabase to create and place files to avoid edge cases with some versions of Unity that prevent installation. Due to this change Manifest's now have Asset GUIDs assigned by ThunderKit.  This change ensures that Manifest's will easily and automatically reference their dependencies, and references to dependencies will continue to work after reinstallation them.  This change is not backwards compatible

* Added compiler directives and code to support Unity 2021.2

* Add a utility to assist in migrating Non-ThunderKit modding projects to ThunderKit by updating asset references from Dll references to Script file references. This is available under Tools/ThunderKit/Migration 

* Added error messaging for PathReferences and Manifests

### Fixes and Changes

* Fix cases where Progress bars may not update or close as expected
* Fix ManifestIdentities not always being saved during package installation
* Fix issue where somtimes PackageSourceSettings will empty its reference array requiring manual repopulation
* Fix PackageManager not removing Scripting Defines when removing a ThunderKit managed Package

## 3.4.1

## Fixes

* Fix an issue where scanning some assemblies would result in a ReflectionTypeLoadException preventing the settings window from loading.

## 3.4.0

## PackageSource Management

PackageSources have been updated with central management and the ability to setup multiple sources of the same types.

You can now manage your Package Sources in the [ThunderKit Settings Window](menulink://Tools/ThunderKit/Settings).
In the ThunderKit Settings window you will be able to Add, Remove, Configure, Rename and Refresh your PackageSources.

## ThunderKit Extensions Thunderstore

With the ability to add multiple PackageSources to your project, you can now add the ThunderKit Extensions Thunderstore as a source. 
Like all Thunderstores, this provides the ability for the community to add resources that help grow the platform in a modular way.

If you would like to take advantage of the ThunderKit Extensions Thunderstore, Add a new ThunderstoreSource to your PackageSources and set the url to ``` https://thunderkit.thunderstore.io ```


## 3.3.0

## Steam Launch support

Some Unity games require that Steam launches them in order to successfully start up, to support this requirement the previous update added the RegistryLookup PathComponent.
This update builds upon that by adding a Steam.exe PathReference asset which locates the Steam.exe using the Windows Registry via the RegistryLookup PathComponent.

To improve the coverage and usability of the BepInEx Template, the template now includes a SteamBepInExLaunch Pipeline and the Launch Pipeline was renamed to BepInExLaunch

In Order to use the SteamBepInExLaunch Pipeline, copy it over the BepInExLaunch pipeline in your Build and Launch Pipeline, or replace BepInExLaunch anywhere you used it with the SteamBepInExLaunch pipeline.

References to the Launch Pipeline will be automatically updated to BepInExLaunch.

## Templates

Two new Templates have been added to ThunderKit under ThunderKit/Editor/Templates
DirectLaunch is a new pre-configured Launch pipeline which directly executes the game's Executable in its working directory.
SteamLaunch is a new pre-configured Launch pipeline which executes the game's Executable in its working directory using Steam and the applaunch command line argument.

## GameAppId and Steam Launching

In order to use any of the pre-configured Steam launching pipelines you will need to provide ThunderKit with the games Steam App Id.

Follow these Steps to setup Steam Launching
1. Create a new PathReference under your Assets directory and name it GameAppId
2. Add the Constant PathComponent to the newly created PathReference
3. Find the Steam App Id for the game you're modding
  * You can find the SteamAppId easily by copying it from the game's Steam Store page url

After Completing these steups you're seting to use Steam Launching.

## Fixes and Improvements

* Recently a ManifestIdentity data loss issue was re-discovered, this has not yet been resolved but some pieces of code have been updated in response while a proper resolution is pending
* Some PipelineJobs and PathComponents referenced a Manifests's ManifestIdentity by using LINQ to query for it in the Manifest's Data elements, these have been updated to use the Manifest's cached Identity reference.
* StageThunderstoreManifest has been updated to provide a clearer error when a dependency is missing its ManifestIdentity
* Some minor code cleanup was done against commented out code and unnecessary usings
* Fixed LocalThunderstoreSource not updating its listing when it already has Packages listed
* Fixed PackageSource.Clear Failing to clear Packages successfully under some conditions
* Fixed PackageManager failing to render correctly when a Package has invalid dependency information



## 3.2.0

### New Feature: PathComponent: Registry Lookup

the Registry Lookup path component has been added to support cases where values from the registry are needed to complete Pipeline efforts.
For example, some games require steam to execute them, and you may need to do so using the Steam executable.  
The installed Steam executable can easily be located by looking up the value in the registry, and this provides a path to that.

#### Performance Improvements

* ThunderstoreAPI has been updated to utilize gzip compression in order to greatly increase the speed of acquiring package listings.
* Fixed MarkdownRenderer's LinkInlineRenderer leaking handles and memory
* Fix an issue with SettingsWindowData and ThunderstoreSettings that would cause the settings window to have poor responsiveness

#### Bugs

* Fix a pageEntry.depth to be pageEntry.Depth in Documentation.cs for Unity 2019+
* Fix a NullReferenceException that could show up on the Settings window sometimes
* Fix cases in PackageManager that could cause a null reference and fail to load the manager UI
* Fix a problem that would cause a PackageSource to have no packages and be unable to be populated

## 3.1.7

## Fixes and Improvements

* Improve the Package Manager search responsiveness

## 3.1.6

## Fixes and Improvements

* Improve package import process with better ProgressBar status messages
* Install packages as final step of import process

## 3.1.1

This update implements support for .NET 3.5 and includes a number of general improvements and fixes

## New Features

### Optimization

Special thanks to Therzok for doing an optimization pass to clean up a number of cases where the code could be more efficient and cleaner.

### Package Manager
* Now updates when package sources are refreshed.
* Now refreshes package sources when opened.
* Now has a Refresh button next to the Filters button which will refresh all available PackageSources
* PackageSources now invoke the SourcesInitialized event when a source has been updated
* PackageSources can register an event handler on the InitializeSources event to be informed when it should update
* Thunderstore API no longer automatically updates on a timer

### 3.5 Migration changes

* Added csc.rsp and mcs.rsp to AssemblyDefinition containing folders to ensure that the correct language version is used for ThunderKit regardless of Scripting Back End choice.
* Removed Async/Await as its not available in 3.5
  * Some cases were replaced with other asynchronous mechanisms.
  * More cases will be moved to asynchronous mechanisms in the future, however there are currently a few that were migrated to synchronous execution.
* Migrate to Directory.GetFiles and Directory.GetDirectories over Directory.EnumerateFiles and Directory.EnumerateDirectories due to lack of support in .NET 3.5


### Zip Changes

* Migrated to SharpCompress from System.IO.Compression
* Updated zip handling in ThunderstoreSource and LocalThunderstoreSource
* Updated Zip PipelineJob to use SharpCompress

### Markdown / Documentation changes
* Improved method of locating Documentation assets
* Significant improvements made to the UIElementRenderer allocations
* Improvements to Regex Usage

### File System changes
* Migrated many file system management facilities to use FileUtil instead of System.IO types
* Updated Copy PipelineJob to use FileUtil  This changes how Copy works, A recursive copy will Replace the designated destination directory, not fill its contents
* Stage Manifest Files now uses FileUtil to deploy files

### BepInEx Template Changes

The BepInEx template has been updated to use some new features that arose out of the .NET 3.5 changes.
First, the template is somewhat large so it has been broken up into 4 Pipelines
1. Stage:  This pipeline executes StageAssetBundles, StageAssemblies, StageDependencies, StageManifestFiles and finally StageThunderstoreManifest.
2. Deploy: Conducts the following Copy jobs
    1. Copy BepInEx to ProjectRoot/ThunderKit/BepInExPack
    2. Copy plugins to ProjectRoot/ThunderKit/BepInExPack/BepInEx/plugins
    3. Copy patchers to ProjectRoot/ThunderKit/BepInExPack/BepInEx/patchers
    4. Copy monomods to ProjectRoot/ThunderKit/BepInExPack/BepInEx/monomod
    5. Copy winhttp.dll to the Games root directory
    6. Copy a BepInEx config targeted by the PathReference named BepInExConfig if it is defined in the project to ProjectRoot/ThunderKit/BepInExPack/BepInEx/config
3. Launch:  Executes the games executable with the necessary command line parameters to load BepInEx
4. Rebuild and Launch:  Executes the 3 prior Pipelines in order.

To get started on a new mod project you only need to copy the Rebuild and Launch pipeline into your Assets folder and then populate the Manifest field.

## 3.0.0 

#### Initial Setup
A Welcome window has been added to ThunderKit to help users set up their project.
This window can be disabled by a toggle it provides.

#### ThunderKit Installer - Removed
The ThunderKit installer has been removed.  The installer caused many development issues and lost
work during the development of ThunderKit. While this issue may not have affected end users, the 
risk associated with the cost of lost work makes this feature dangerous to continue to maintain.

Unity 2018.1-2019.2 users will need to add the Thunderkit dependency to their projects Packages/manifest.json

For Unity 2019.3+ users can add ThunderKit using the Git url and use the [Install from Git](https://docs.unity3d.com/2019.3/Documentation/Manual/upm-ui-giturl.html) option in the Unity Package Manager.

#### ThunderKit Settings

ThunderKit Settings now get a dedicated window from ThunderKit and can be accessed from the main menu under [Tools/ThunderKit/Settings](menulink://Tools/ThunderKit/Settings).
These settings will no longer show up in the Project Settings window.

#### Debugging Features

ComposableObjects now support some debugging features to provide an easy access interface to implementations of Composable Object to report errors in the UI.

ComposableElements now have 2 members, IsErrored and ErrorMessage. The ComposableObjectEditor will change the header color of ComposableElements to red if IsErrored is true, signalling where a problem may be.

Implementations of ComposableObject are responsible for setting the values in IsErrored and ErrorMessage.  

For examples see [Pipeline](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Core/Pipelines/Pipeline.cs) 
and [PathReference](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Core/Paths/PathReference.cs)

If a pipeline encounters a problem it will halt its execution and highlight the step that it faulted on.
The PipelineJobs and Pipeline itself are setup to log exceptions to the Unity console like normal, with these two pieces of information you should be able to quickly identify and rectify problems.

ThunderKit Manifests do not utilize these debugging features as they are only Data Containers, however if worth while usage for debugging issues can be identified then support will be added.

#### Dependencies

Dependency Management in the 3.0.0 update has changed significantly.  Instead of Manifests installing and managing dependencies in its entirety, Manifests will now only 
be responsible for containing dependency references.  

Instead a user will now install packages via the [Package Manager](menulink://Tools/ThunderKit/Packages), and then 
add the Manifest from the Packages folder to the Manifest that requires the dependency

#### Package Manager

ThunderKit now includes a complete Package Manager, available from the main menu under [Tools/ThunderKit/Packages](menulink://Tools/ThunderKit/Packages)

The ThunderKit Package Manager is how you will add and remove all mod dependencies for your project.
If a mod in your project needs to depend on a Mod, Loader, or Library, you have the ability to install these dependencies through the Package Manager.

Currently the Package Manager comes with support for Thunderstore by default, select your Thunderstore community by setting the url from the [ThunderKit Settings](menulink://Tools/ThunderKit/Settings).

You can also create a Local Thunderstore source where you can specify a folder to examine for zip files.
Zip files in Local Thunderstore Sources must conform to Thunderstore's file naming schemes in order to be resolved correctly. 

This scheme is: `Author-ModName-Version.zip`

#### Documentation
Documentation is a major issue for new users and as such ThunderKit now comes with integrated documentation to help onboard new users.
The documentation available from the main menu under Tools/ThunderKit/Documentation

Documentation is a work in progress and improvements will be made as a better understanding is gained about users needs for information.

## 2.2.1
  * Fix issues with assembly identitification

## 2.1.3
  * New Features
    * Establish base for documentation system
    * Establish Package management as a core system
    * Add support to drag and drop Thunderstore package zip files into ThunderstoreManifest dependencies
    * Components of ComposableObjects now provide Copy, Paste, and Duplicate from their menus
    * Ensure a Scripting Define is always added for packages installed by ThunderKit, Define will be the name of the package
  * Improvements
    * Clean and organize systems for managing the loading process of ThunderKit
    * Improve the design of the ThunderKit Installer package to support more versions
    * Use built in Asset Package (unityPackage) options
    * ComposableObject now has an array of ComposableElements instead of ScrtipableObjects
  * Fixes
    * Sort add component options
    * Fix cases where directories are not created when needed
    * Fix some problems with the Thunderstore - BepInEx templates

## 2.1.0 - 2.1.2
  * Fix issues with automatic installer
  * Fix issues with package management

## 2.0.0 - First Major Version update
  * Replace Deployments with new system.
    * Manifest's will now hold all references to files that need to be included or processed for deploying a mod
    * Deployment operations will now be handled by Pipeline's and Pipeline Jobs.
    * Pipelines are containers for pipeline jobs, pipelines with special requirements can be made by creating derivatives of Pipeline.

## Early Versions
* 1.x.x - untracked iterative updates to ThunderKits feature set
* 1.0.0 - Initial Relesae of Thunderkit
