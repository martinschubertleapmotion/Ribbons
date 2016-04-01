using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace HutongGames.PlayMakerEditor
{
    /// <summary>
    /// Adds Playmaker defines to project
    /// Other tools can now use #if PLAYMAKER
    /// Package as source code so user can remove or modify
    /// </summary>
    [InitializeOnLoad]
    public class PlayMakerDefines
    {
        static PlayMakerDefines()
        {
            AddScriptingDefineSymbolToAllTargets("PLAYMAKER");
            AddScriptingDefineSymbolToAllTargets("PLAYMAKER_1_8");
        }

        public static void AddScriptingDefineSymbolToAllTargets(string defineSymbol)
        {
            foreach (BuildTargetGroup group in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (!IsValidBuildTargetGroup(group)) continue;

                var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').Select(d => d.Trim()).ToList();
                if (!defineSymbols.Contains(defineSymbol))
                {
                    defineSymbols.Add(defineSymbol);
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defineSymbols.ToArray()));
                }
            }
        }

        public static void RemoveScriptingDefineSymbolFromAllTargets(string defineSymbol)
        {
            foreach (BuildTargetGroup group in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (!IsValidBuildTargetGroup(group)) continue;

                var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').Select(d => d.Trim()).ToList();
                if (defineSymbols.Contains(defineSymbol))
                {
                    defineSymbols.Remove(defineSymbol);
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defineSymbols.ToArray()));
                }
            }
        }

        private static bool IsValidBuildTargetGroup(BuildTargetGroup group)
        {
            if (group == BuildTargetGroup.Unknown) return false;

            #if UNITY_5_3_0  // Unity 5.3.0 had tvOS in enum but throws error if used
            if ((int)(object)_group == 25) return false;
            #endif

            return true;
        }
    }
}

