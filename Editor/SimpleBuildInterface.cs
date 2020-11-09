using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

[assembly: InternalsVisibleTo("SimpleBuildInterfaceTest")]

internal class SimpleBuildInterface
{
    static string s_OldSymbols = null;

    [InitializeOnLoadMethod]
    private static void OnInitializeOnLoadMethod()
    {
        if (!Application.isBatchMode) return;

        var args = ParseArguments(Environment.GetCommandLineArgs());
        if (!args.ContainsKey("build")) return;

        EditorApplication.delayCall += () =>
        {
            var success = Build(EditorUserBuildSettings.activeBuildTarget, Environment.GetCommandLineArgs(), Application.isBatchMode);
            EditorApplication.Exit(success ? 0 : 1);
        };
    }

    public static bool Build(BuildTarget target, string[] arguments, bool isBatchMode)
    {
        var success = false;
        var options = GetCurrentBuildPlayerOptions(target, arguments);

        if (ValidBuildPlayerOptions(isBatchMode, ref options))
        {
            options = UpdateBuildPlayerOptions(options, arguments);
            options.options |= BuildOptions.ShowBuiltPlayer;
            var report = BuildPipeline.BuildPlayer(options);
            success = report.summary.result == BuildResult.Succeeded;
        }

        return success;
    }

    public static BuildOptions ParseBuildOptions(string options)
    {
        BuildOptions retOpt = 0;
        BuildOptions newOpt;
        foreach (var opt in options.Split(',', ';'))
        {
            if (string.IsNullOrEmpty(opt)) continue;
            if (!Enum.TryParse(opt.TrimStart('!'), true, out newOpt)) continue;

            retOpt = opt[0] == '!'
                ? retOpt & ~newOpt
                : retOpt | newOpt;
        }

        return retOpt;
    }

    public static Dictionary<string, string> ParseArguments(string[] arguments)
    {
        var argSign = new[] {'-', '/'};
        var args = new Dictionary<string, string>();
        var key = "";
        for (var i = 0; i < arguments.Length; i++)
        {
            var arg = arguments[i];
            if (string.IsNullOrEmpty(arg)) continue;
            if (argSign.Contains(arg[0]))
            {
                var valueIndex = arg.IndexOf('=');
                if (valueIndex == -1)
                {
                    key = arg.Substring(1);
                    args[key] = "";
                }
                else
                {
                    key = arg.Substring(1, valueIndex - 1);
                    args[key] = arg.Substring(valueIndex + 1);
                    key = "";
                }
            }
            else if (0 < key.Length)
            {
                args[key] = arg;
                key = "";
            }
        }

        return args;
    }

    public static BuildPlayerOptions GetCurrentBuildPlayerOptions(BuildTarget target, string[] arguments)
    {
        var targetGroup = BuildPipeline.GetBuildTargetGroup(target);
        var options = new BuildPlayerOptions
        {
            target = target,
            targetGroup = targetGroup,
            locationPathName = EditorUserBuildSettings.GetBuildLocation(target),
        };

        return UpdateBuildPlayerOptions(options, arguments);
    }

    public static BuildPlayerOptions UpdateBuildPlayerOptions(BuildPlayerOptions options, string[] arguments)
    {
        var args = ParseArguments(arguments);
        string value;
        if (args.TryGetValue("out", out value))
            options.locationPathName = value;
        else
            options.locationPathName = options.target.ToString() + "_Build";

        if (args.TryGetValue("buildOptions", out value))
            options.options |= ParseBuildOptions(value);

        var modifier = args.TryGetValue("scenes", out value) ? value : "";
        options.scenes = GetBuildScenes(EditorBuildSettings.scenes, modifier);

        if (args.TryGetValue("assetBundleManifestPath", out value))
            options.assetBundleManifestPath = value;

        return options;
    }

    public static string[] GetBuildScenes(EditorBuildSettingsScene[] scenes, string modifier)
    {
        if (string.IsNullOrEmpty(modifier))
            return scenes
                .Where(x => x.enabled)
                .Select(x => x.path)
                .ToArray();

        var mods = modifier.Split(',', ';');
        var add = mods.Where(x => 0 < x.Length && x[0] != '!').ToArray();
        var remove = mods.Where(x => 1 < x.Length && x[0] == '!').Select(x => x.Substring(1)).ToArray();
        return scenes
            .Select(x => new {x.path, x.enabled, name = Path.GetFileNameWithoutExtension(x.path)})
            .Where(x => !remove.Contains(x.name) && (x.enabled || add.Contains(x.name)))
            .Select(x => x.path)
            .ToArray();
    }

    public static bool ValidBuildPlayerOptions(bool isBatchMode, ref BuildPlayerOptions options)
    {
        options.targetGroup = BuildPipeline.GetBuildTargetGroup(options.target);
        EditorUserBuildSettings.selectedBuildTargetGroup = options.targetGroup;

        try
        {
            // Path check.
            var askForBuildLocation = true;
            var path = (options.locationPathName ?? "").TrimEnd('/', '\\');
            if (0 < path.Length)
            {
                options.locationPathName = Path.GetFullPath(path);
                askForBuildLocation = !Directory.Exists(Path.GetDirectoryName(options.locationPathName));
            }


            if (isBatchMode && askForBuildLocation) return false;

            EditorUserBuildSettings.SetBuildLocation(options.target, options.locationPathName);

            var getBuildPlayerOptionsHandler = typeof(BuildPlayerWindow)
                .GetField("getBuildPlayerOptionsHandler", BindingFlags.Static | BindingFlags.NonPublic)
                .GetValue(null) as Func<BuildPlayerOptions, BuildPlayerOptions>;
            if (getBuildPlayerOptionsHandler != null)
            {
                options = getBuildPlayerOptionsHandler.Invoke(options);
            }
            else
            {
                options = (BuildPlayerOptions) typeof(BuildPlayerWindow.DefaultBuildMethods)
                    .GetMethod("GetBuildPlayerOptionsInternal", BindingFlags.Static | BindingFlags.NonPublic)
                    .Invoke(null, new object[] {askForBuildLocation, options});
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return false;
        }
    }
}
