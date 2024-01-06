Simple Build Interface for Unity CLI
===

[![](https://img.shields.io/npm/v/com.coffee.simple-build-interface?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.coffee.simple-build-interface/)
[![](https://img.shields.io/github/v/release/mob-sakai/SimpleBuildInterface?include_prereleases)](https://github.com/mob-sakai/SimpleBuildInterface/releases)
[![](https://img.shields.io/github/release-date/mob-sakai/SimpleBuildInterface.svg)](https://github.com/mob-sakai/SimpleBuildInterface/releases)  [![](https://img.shields.io/github/license/mob-sakai/SimpleBuildInterface.svg)](https://github.com/mob-sakai/SimpleBuildInterface/blob/master/LICENSE.txt)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-orange.svg)](http://makeapullrequest.com)  
![](https://img.shields.io/badge/Unity%202018.3+-supported-blue.svg)  
![GitHub Workflow Status](https://img.shields.io/github/workflow/status/mob-sakai/CSharpCompilerSettingsForUnity/unity-test)
[![Test](https://mob-sakai.testspace.com/spaces/130862/badge?token=43a50d2fc998aa362d36934597de0c84527e5690)](https://mob-sakai.testspace.com/spaces/130862)
[![CodeCoverage](https://mob-sakai.testspace.com/spaces/130862/metrics/99758/badge)](https://mob-sakai.testspace.com/spaces/130862/current/Code%20Coverage/Code%20Coverage")


<< [Description](#Description) | [Installation](#installation) | [Usage](#usage) | [Development Note](#development-note) | [Change log](https://github.com/mob-sakai/SimpleBuildInterface/blob/master/CHANGELOG.md) >>



<br><br><br><br>

## Description

Unity supports to build for standalone platforms (Windows/macOS/Linux) from the command line.

```sh
# Lunch Unity to ...
/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -projectPath .

# build for specific standalone platform.
... -buildLinux64Player <pathname>
... -buildOSXUniversalPlayer <pathname>
... -buildWindowsPlayer <pathname>
... -buildWindows64Player <pathname>
```

This plugin provides a simple build interface to build all platforms **without `executeMethod` option**.

```sh
# Lunch Unity to ...
/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -projectPath .

# build for specific platform.
... -build -buildTarget WebGL
```

This command is equivalent to run `Build Settings > Build` on WebGL platform.

![](https://user-images.githubusercontent.com/12690315/98614365-a13b6900-233b-11eb-8529-05a49fc7000e.png)

See [Usage](#usage) for details.

<br><br><br><br>

## Installation

### Requirement

![](https://img.shields.io/badge/Unity%202018.3+-supported-blue.svg)

### Using OpenUPM

This package is available on [OpenUPM](https://openupm.com).  
You can install it via [openupm-cli](https://github.com/openupm/openupm-cli).
```
openupm add com.coffee.simple-build-interface
```

### Using Git

Find `Packages/manifest.json` in your project and add a line to `dependencies` field.

```
"com.coffee.simple-build-interface": "https://github.com/mob-sakai/SimpleBuildInterface.git"
```

To update the package, change suffix `#{version}` to the target version.

* e.g. `"com.coffee.simple-build-interface": "https://github.com/mob-sakai/SimpleBuildInterface.git#1.0.0",`

Or, use [UpmGitExtension](https://github.com/mob-sakai/UpmGitExtension) to install and update the package.



<br><br><br><br>

## Usage

```sh
# Lunch Unity to ...
/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -projectPath .

# build for current platform.
... -build

# build for specific platform.
... -build -buildTarget WebGL

# build with full-option.
... -build -buildTarget WebGL \
  -out "production_build" \
  -buildOptions "Development;!ConnectWithProfiler" \
  -scenes "Level1;!EditorLevel1" \
  -assetBundleManifestPath "AssetBundles/manifest" \
  -extraScriptingDefines "EXTRA_SYMBOL;EXTRA_SYMBOL2"
```

| Option                                 | Description                                                                        |
| -------------------------------------- | ---------------------------------------------------------------------------------- |
| `-build`                               | **(Required)**<br>Build the project for current. platform                          |
| `-out <path>`                          | Output path<br>Default: `{BuildTarget}_Build`.                                     |
| `-buildOptions <options,...>`          | Add/remove [BuildOptions][opt] to build. <sup>[1](#fn1)</sup> <sup>[2](#fn2)</sup> |
| `-scenes <names,...>`                  | Add/remove scene names to build.  <sup>[1](#fn1)</sup> <sup>[2](#fn2)</sup>        |
| `-assetBundleManifestPath <path>`      | Path to AssetBundleManifest.                                                       |
| `-extraScriptingDefines <symbols,...>` | Extra scripting defines for building player.  <sup>[1](#fn1)</sup>                 |

<br>

<a name="fn1">1</a>: Multiple values must be separated by a semi-colon (`;`) or a comma (`,`).  
<a name="fn2">2</a>: Prefix 'not' (`!`) to exclude the specified value.

[opt]: https://docs.unity3d.com/ScriptReference/BuildOptions.html

<br><br><br><br>

## Development Note

### Execution order

1. `InitializeOnLoad`
2. `InitializeOnLoadMethod`
3. `DidReloadScripts`
4. The method specified by the `-executeMethod` option
5. **Build by this plugin**

You can customize the build parameters using any method you like. :)

<br><br><br><br>

## Contributing

### Issues

Issues are very valuable to this project.

- Ideas are a valuable source of contributions others can make
- Problems show where this project is lacking
- With a question you show where contributors can improve the user experience

### Pull Requests

Pull requests are, a great way to get your ideas into this repository.  
See [sandbox/README.md](https://github.com/mob-sakai/SimpleBuildInterface/blob/sandbox/README.md).

### Support

This is an open source project that I am developing in my spare time.  
If you like it, please support me.  
With your support, I can spend more time on development. :)

[![](https://user-images.githubusercontent.com/12690315/50731629-3b18b480-11ad-11e9-8fad-4b13f27969c1.png)](https://www.patreon.com/join/mob_sakai?)  
[![](https://user-images.githubusercontent.com/12690315/66942881-03686280-f085-11e9-9586-fc0b6011029f.png)](https://github.com/users/mob-sakai/sponsorship)



<br><br><br><br>

## License

* MIT



## Author

* ![](https://user-images.githubusercontent.com/12690315/96986908-434a0b80-155d-11eb-8275-85138ab90afa.png) [mob-sakai](https://github.com/mob-sakai) [![](https://img.shields.io/twitter/follow/mob_sakai.svg?label=Follow&style=social)](https://twitter.com/intent/follow?screen_name=mob_sakai) ![GitHub followers](https://img.shields.io/github/followers/mob-sakai?style=social)



## See Also

* GitHub page : https://github.com/mob-sakai/SimpleBuildInterface
* Releases : https://github.com/mob-sakai/SimpleBuildInterface/releases
* Issue tracker : https://github.com/mob-sakai/SimpleBuildInterface/issues
* Change log : https://github.com/mob-sakai/SimpleBuildInterface/blob/master/CHANGELOG.md
