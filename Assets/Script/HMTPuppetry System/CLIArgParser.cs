using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.TerrainTools;
using Unity.Burst.Intrinsics;
using System.CodeDom;
using UnityEngine.UIElements;

namespace HMT {
    [CreateAssetMenu(fileName = "CLIArgs", menuName = "CLI/CommandLineArgConfig")]
    public class CLIArgParser : ScriptableObject {
        //public static CLIArgParser Instance { get; private set; } = null;

        public enum ArgType {
            Flag,
            One,
            List
        }

        public string FlagMarker = "-";

        public bool enableHelpFlag = true;

        [HideInInspector]
        public string HelpFlag = "help";

        [HideInInspector]
        public string helpMessage;


        [HideInInspector]
        [SerializeField]
        public List<ArgConfig> argSpecs = new List<ArgConfig>();

        private Dictionary<string, ParsedArg> argsParsed = null;


        public ParsedArg Get(string key) {
            if (argsParsed == null) {
                ParseArgs();
            }
            if (argsParsed.ContainsKey(key)) {
                return argsParsed[key];
            }
            else {
                return new ParsedArg(key);
            }
        }

        public void ParseArgs() {
            string[] arguments = System.Environment.GetCommandLineArgs();
            string currentFlag = string.Empty;
            List<string> paramCollection = new List<string>();

            argsParsed = new Dictionary<string, ParsedArg>();
            List<ArgConfig> specedArgs = new List<ArgConfig>(argSpecs);

            for (int i = 0; i < arguments.Length; i++) {
                if (arguments[i].StartsWith(FlagMarker)) {
                    if(arguments[i] == FlagMarker + HelpFlag && enableHelpFlag) {
                        PrintHelp();
                    }

                    //If we have a current flag then add it to the parsed args
                    if (currentFlag != string.Empty) {
                        bool found = false;
                        for (int j = 0; j < specedArgs.Count; j++) {
                            if (specedArgs[j].flag == currentFlag) {
                                argsParsed[currentFlag] = new ParsedArg(specedArgs[j], paramCollection.ToArray(), true, true);
                                specedArgs.RemoveAt(j);
                                found = true;
                                break;
                            }
                        }
                        if (!found) {
                            ArgConfig argSpec;
                            if (paramCollection.Count == 0) {
                                argSpec = new ArgConfig(currentFlag, ArgType.Flag, string.Empty);
                            }
                            else if (paramCollection.Count == 1) {
                                argSpec = new ArgConfig(currentFlag, ArgType.One, string.Empty);
                            }
                            else {
                                argSpec = new ArgConfig(currentFlag, ArgType.List, string.Empty);
                            }
                            argsParsed[currentFlag] = new ParsedArg(argSpec, paramCollection.ToArray(), true, false);
                        }
                    }
                    //Set the current flag to the new one
                    currentFlag = arguments[i].Substring(FlagMarker.Length);
                    paramCollection = new List<string>();
                }
                else {
                    //If we have a current flag then add the argument to the collection
                    if (currentFlag != string.Empty) {
                        paramCollection.Add(arguments[i]);
                    }
                }
            }

            foreach(var arg in specedArgs) {
                argsParsed[arg.flag] = new ParsedArg(arg, new string[0], false, true);
            }
        }

        private void PrintHelp() {
            Debug.Log(helpMessage);
            Debug.Log("Defined Arguments:");
            foreach (var arg in argSpecs) {
                Debug.Log("\t" + arg.flag + ": " + arg.helpMessage);
            }
            Application.Quit();
        }




        [System.Serializable]
        public class ArgConfig {
            [SerializeField]
            public string flag;
            [SerializeField]
            public ArgType type;
            [SerializeField]
            public string helpMessage;

            public ArgConfig(string flag, ArgType type, string helpMessage) {
                this.flag = flag;
                this.type = type;
                this.helpMessage = helpMessage;
            }

        }

        public struct ParsedArg {
            public ArgConfig config;
            private string[] _values;

            public string[] Values {
                get {
                    if (!IsSet) {
                        return new string[0];
                    }
                    else {
                        return _values;
                    }
                }
            }

            public bool IsSpecified { get; private set; }
            public bool IsSet { get; private set; }

            public ParsedArg(ArgConfig config, string[] values, bool IsSet, bool IsSpecified) {
                this.config = config;
                this._values = values;
                this.IsSpecified = IsSpecified;
                this.IsSet = IsSet;
            }

            public ParsedArg(string flag) {
                this.config = new ArgConfig(flag, ArgType.Flag, string.Empty);
                this._values = new string[0];
                this.IsSpecified = false;
                this.IsSet = false;
            }

            public string GetValue(string defaultValue) {
                if(!IsSet) {
                    return defaultValue;
                }
                else {
                    if (_values.Length > 0) {
                        return _values[0];
                    }
                    else {
                        return defaultValue;
                    }
                }
            }

            public int GetValue(int defaultValue) {
                if (!IsSet) {
                    return defaultValue;
                }
                else {
                    if (_values.Length > 0) {
                        int result;
                        if (int.TryParse(_values[0], out result)) {
                            return result;
                        }
                        else {
                            return defaultValue;
                        }
                    }
                    else {
                        return defaultValue;
                    }
                }
            }

            public float GetValue(float defaultValue) {
                if (!IsSet) {
                    return defaultValue;
                }
                else {
                    if (_values.Length > 0) {
                        float result;
                        if (float.TryParse(_values[0], out result)) {
                            return result;
                        }
                        else {
                            return defaultValue;
                        }
                    }
                    else {
                        return defaultValue;
                    }
                }
            }
        }
    }

    [CustomEditor(typeof(CLIArgParser))]
    public class ArgParserEditor : Editor {

        private List<bool> foldedHelp = new List<bool>();

        public override void OnInspectorGUI() {
            CLIArgParser argParser = (CLIArgParser)target;

            while(foldedHelp.Count < argParser.argSpecs.Count) {
                foldedHelp.Add(false);
            }
            while (foldedHelp.Count > argParser.argSpecs.Count) {
                foldedHelp.RemoveAt(foldedHelp.Count - 1);
            }

            EditorGUI.BeginChangeCheck();
            EditorStyles.textField.wordWrap = true;
            EditorGUILayout.BeginVertical();

            argParser.FlagMarker = EditorGUILayout.TextField(new GUIContent("Flag Marker", "The marker that indicates a flag. This is usually a - or --"), argParser.FlagMarker);
            argParser.enableHelpFlag = EditorGUILayout.Toggle(new GUIContent("Enable Help Flag", "If the help flag is enabled and provided the messag below will be printed along with the help message for each argument. The game will then force end."), argParser.enableHelpFlag);


            if (argParser.enableHelpFlag) {
                argParser.HelpFlag = EditorGUILayout.TextField("Help Flag", argParser.HelpFlag);
                argParser.helpMessage = EditorGUILayout.TextArea(argParser.helpMessage, GUILayout.ExpandHeight(true), GUILayout.MinHeight(40));
            }


            for(int i =0; i < argParser.argSpecs.Count; i++) {
                EditorGUILayout.BeginHorizontal();
                CLIArgParser.ArgConfig arg = argParser.argSpecs[i];
                argParser.argSpecs[i].flag = EditorGUILayout.TextField(arg.flag);
                arg.type = (CLIArgParser.ArgType)EditorGUILayout.EnumPopup(arg.type);
                if(GUILayout.Button(foldedHelp[i] ? "v" : "^")) {
                    foldedHelp[i] = !foldedHelp[i];
                }
                if(foldedHelp[i]) {
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal();

                    arg.helpMessage = EditorGUILayout.TextArea(arg.helpMessage, GUILayout.ExpandHeight(true), GUILayout.MinHeight(40));
                    EditorGUI.indentLevel--;
                }

                //arg.helpMessage = EditorGUILayout.TextField(arg.helpMessage);
                EditorGUILayout.EndHorizontal();
            }


            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("+")) {
                argParser.argSpecs.Add(new CLIArgParser.ArgConfig(string.Empty, CLIArgParser.ArgType.Flag, string.Empty));
                foldedHelp.Add(false);
            }
            if(GUILayout.Button("-")) {
                if (argParser.argSpecs.Count > 0) {
                    argParser.argSpecs.RemoveAt(argParser.argSpecs.Count - 1);
                    foldedHelp.RemoveAt(foldedHelp.Count - 1);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            if(EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(argParser);
            }
        }

    }

}