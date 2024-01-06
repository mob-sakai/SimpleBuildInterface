using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

internal class SimpleBuildInterfaceTest
{
    [Test]
    public void ParseArguments()
    {
        var actual = SimpleBuildInterface.ParseArguments(new[]
        {
            "INVALID_VALUE",
            "-withoutValue",
            "-withValue1",
            "Value1",
            "-withValue2=Value2",
            "INVALID_VALUE",
        });
        var expected = new Dictionary<string, string>
        {
            {"withoutValue", ""},
            {"withValue1", "Value1"},
            {"withValue2", "Value2"},
        };

        CollectionAssert.AreEquivalent(expected, actual);
    }

    [Test]
    public void GetBuildScenes()
    {
        var actual = SimpleBuildInterface.GetBuildScenes(new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Level1.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Level2.unity", false),
                new EditorBuildSettingsScene("Assets/Scenes/Level3.unity", false),
            },
            null);
        var expected = new[]
        {
            "Assets/Scenes/Main.unity",
            "Assets/Scenes/Level1.unity",
        };
        CollectionAssert.AreEqual(expected, actual);
    }

    [Test]
    public void GetBuildScenes_Modifier()
    {
        var actual = SimpleBuildInterface.GetBuildScenes(new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Level1.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Level2.unity", false),
                new EditorBuildSettingsScene("Assets/Scenes/Level3.unity", false),
                new EditorBuildSettingsScene("Assets/Editor/EditorLevel1.unity", true),
                new EditorBuildSettingsScene("Assets/Editor/EditorLevel2.unity", true),
            },
            "Level1,Level2;Level3;!EditorLevel1,!EditorLevel2");
        var expected = new[]
        {
            "Assets/Scenes/Main.unity",
            "Assets/Scenes/Level1.unity",
            "Assets/Scenes/Level2.unity",
            "Assets/Scenes/Level3.unity",
        };
        CollectionAssert.AreEqual(expected, actual);
    }


    [Test]
    public void ParseBuildOptions()
    {
        var actual = SimpleBuildInterface.ParseBuildOptions("development,CompressWithLz4,INVALID");
        var expected = BuildOptions.Development | BuildOptions.CompressWithLz4;
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void ParseBuildOptions_None()
    {
        var actual = SimpleBuildInterface.ParseBuildOptions("");
        var expected = BuildOptions.None;
        Assert.AreEqual(expected, actual);
    }


    [Test]
    public void ValidBuildPlayerOptions_EmptyPath()
    {
        var option = new BuildPlayerOptions()
        {
            locationPathName = "",
        };
        var actual = SimpleBuildInterface.ValidBuildPlayerOptions(true, ref option);
        var expected = false;
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void ValidBuildPlayerOptions_NotExistPath()
    {
        var option = new BuildPlayerOptions()
        {
            target = BuildTarget.WebGL,
            locationPathName = "Not/Exist/Path",
        };
        var actual = SimpleBuildInterface.ValidBuildPlayerOptions(true, ref option);
        var expected = false;
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void ValidBuildPlayerOptions_ExistPath_BatchMode()
    {
        var option = new BuildPlayerOptions()
        {
            target = BuildTarget.WebGL,
            locationPathName = "Builds",
        };
        var actual = SimpleBuildInterface.ValidBuildPlayerOptions(true, ref option);
        var expected = true;
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void ValidBuildPlayerOptions_ExistPath()
    {
        var option = new BuildPlayerOptions()
        {
            target = BuildTarget.WebGL,
            locationPathName = "Builds",
        };
        var actual = SimpleBuildInterface.ValidBuildPlayerOptions(false, ref option);
        var expected = true;
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void ValidBuildPlayerOptions_Exception()
    {
        LogAssert.Expect(LogType.Error, new Regex( "System.Exception: ValidBuildPlayerOptions_Exception"));
        Func<BuildPlayerOptions, BuildPlayerOptions> callback = opt =>
        {
            throw new Exception("ValidBuildPlayerOptions_Exception");
        };

        var fiGetBuildPlayerOptionsHandler = typeof(BuildPlayerWindow)
            .GetField("getBuildPlayerOptionsHandler", BindingFlags.Static | BindingFlags.NonPublic);
        var getBuildPlayerOptionsHandler = fiGetBuildPlayerOptionsHandler.GetValue(null) as Func<BuildPlayerOptions, BuildPlayerOptions>;
        getBuildPlayerOptionsHandler += callback;
        fiGetBuildPlayerOptionsHandler.SetValue(null, getBuildPlayerOptionsHandler);

        var option = new BuildPlayerOptions()
        {
            target = BuildTarget.WebGL,
            locationPathName = "Builds",
        };
        try
        {
            var actual = SimpleBuildInterface.ValidBuildPlayerOptions(false, ref option);
            var expected = false;
            Assert.AreEqual(expected, actual);
        }
        finally
        {
            getBuildPlayerOptionsHandler -= callback;
            fiGetBuildPlayerOptionsHandler.SetValue(null, getBuildPlayerOptionsHandler);
        }
    }

    [Test]
    public void GetCurrentBuildPlayerOptions()
    {
        var target = BuildTarget.WebGL;
        var actual = SimpleBuildInterface.GetCurrentBuildPlayerOptions(target, new[]
        {
            "-out=Builds",
            "-buildOptions=development,SymlinkLibraries",
            "-scenes=!SampleScene",
            "-assetBundleManifestPath=AssetBundles/Manifest",
            // "-extraScriptingDefines=TEST;MODIFIED",
        });
        var expected = new BuildPlayerOptions()
        {
            locationPathName = "Builds",
            target = target,
            targetGroup = BuildPipeline.GetBuildTargetGroup(target),
            options = BuildOptions.Development | BuildOptions.SymlinkLibraries,
            scenes = SimpleBuildInterface.GetBuildScenes(EditorBuildSettings.scenes, "!SampleScene"),
            assetBundleManifestPath = "AssetBundles/Manifest",
            // extraScriptingDefines = new[] {"TEST", "MODIFIED"},
        };

        // CollectionAssert.AreEqual(expected.extraScriptingDefines, actual.extraScriptingDefines);
        CollectionAssert.AreEqual(expected.scenes, actual.scenes);
        // expected.extraScriptingDefines = actual.extraScriptingDefines = null;
        // expected.scenes = actual.scenes = null;
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void GetCurrentBuildPlayerOptions_WithoutLocation()
    {
        var target = BuildTarget.WebGL;
        var actual = SimpleBuildInterface.GetCurrentBuildPlayerOptions(target, new[]
        {
            "-buildOptions=development,SymlinkLibraries",
            "-scenes=!SampleScene",
            "-assetBundleManifestPath=AssetBundles/Manifest",
            // "-extraScriptingDefines=TEST;MODIFIED",
        });
        var expected = new BuildPlayerOptions()
        {
            locationPathName = "WebGL_Build",
            target = target,
            targetGroup = BuildPipeline.GetBuildTargetGroup(target),
            options = BuildOptions.Development | BuildOptions.SymlinkLibraries,
            scenes = SimpleBuildInterface.GetBuildScenes(EditorBuildSettings.scenes, "!SampleScene"),
            assetBundleManifestPath = "AssetBundles/Manifest",
            // extraScriptingDefines = new[] {"TEST", "MODIFIED"},
        };

        // CollectionAssert.AreEqual(expected.extraScriptingDefines, actual.extraScriptingDefines);
        CollectionAssert.AreEqual(expected.scenes, actual.scenes);
        // expected.extraScriptingDefines = actual.extraScriptingDefines = null;
        expected.scenes = actual.scenes = null;
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void UpdateBuildPlayerOptions()
    {
        var actual = SimpleBuildInterface.UpdateBuildPlayerOptions(new BuildPlayerOptions(), new[]
        {
            "-out=Builds",
            "-buildOptions=development,SymlinkLibraries",
            "-scenes=!SampleScene",
            "-assetBundleManifestPath=AssetBundles/Manifest",
            // "-extraScriptingDefines=TEST;MODIFIED",
        }, false);
        var expected = new BuildPlayerOptions()
        {
            locationPathName = "Builds",
            options = BuildOptions.Development | BuildOptions.SymlinkLibraries,
            scenes = SimpleBuildInterface.GetBuildScenes(EditorBuildSettings.scenes, "!SampleScene"),
            assetBundleManifestPath = "AssetBundles/Manifest",
            // extraScriptingDefines = new[] {"TEST", "MODIFIED"},
        };

        // CollectionAssert.AreEqual(expected.extraScriptingDefines, actual.extraScriptingDefines);
        CollectionAssert.AreEqual(expected.scenes, actual.scenes);
        // expected.extraScriptingDefines = actual.extraScriptingDefines = null;
        expected.scenes = actual.scenes = null;
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Build()
    {
        if (Application.isBatchMode) return;

        EditorUserBuildSettings.SetBuildLocation(EditorUserBuildSettings.activeBuildTarget, "");
        var actual = SimpleBuildInterface.Build(EditorUserBuildSettings.activeBuildTarget, new[]
        {
            "-out=Library/TestBuilds_" + GUID.Generate(),
            "-buildOptions=development",
        }, true);
        var expected = true;
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Build_Failure()
    {
        if (Application.isBatchMode) return;

        EditorUserBuildSettings.SetBuildLocation(EditorUserBuildSettings.activeBuildTarget, "");
        var actual = SimpleBuildInterface.Build(EditorUserBuildSettings.activeBuildTarget, new[]
        {
            "-out=",
        }, true);
        var expected = false;
        Assert.AreEqual(expected, actual);
    }
}
