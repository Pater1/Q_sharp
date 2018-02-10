using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Q_sharp.Transpiler {
    public static class TranspileQS {
        #region Solution Directories Walk
        private static void DeleteDirectory(string source) {
            foreach(string dir in Directory.EnumerateDirectories(source)) {
                DeleteDirectory(dir);
            }

            foreach(string dir in Directory.EnumerateFiles(source)) {
                File.Delete(dir);
            }

            Directory.Delete(source);
        }

        public static void StartSolutionTranspile(string rootDir) {
            StartSolutionTranspile(rootDir + "\\QS_Source", rootDir + "\\CS_Transpile");
        }
        public static void StartSolutionTranspile(string source, string destination){
            if(File.Exists(source)){
                TranspileFile(source, destination);
            }else if(Directory.Exists(source)){
                TranspileDirectory(source, destination);
            }
        }

        private static void TranspileDirectory(string source, string destination) {
            if(Directory.Exists(destination)){
                DeleteDirectory(destination);
            }
            Directory.CreateDirectory(destination);

            foreach(string dir in Directory.EnumerateDirectories(source)) {
                string dirDiff = dir.Replace(source, string.Empty);
                string dirDest = destination + dirDiff;
                TranspileDirectory(dir, dirDest);
            }

            foreach(string dir in Directory.EnumerateFiles(source)) {
                string dirDiff = dir.Replace(source, string.Empty).Replace(".qs", ".cs");
                string dirDest = destination + dirDiff;
                TranspileFile(dir, dirDest);
            }
        }
        private static void TranspileFile(string source, string destination) {
            string[] sourceCode = File.ReadAllLines(source);

            File.WriteAllLines(destination, TranspileSource(sourceCode));
        }
        #endregion

        #region Source Code Manipulation
        private static readonly string[] requiredHeaders = new string[]{
            "using Q_sharp.Core;",
            "using System.Collections.Generic",
        };
        private static string[] TranspileSource(string[] c) {
            IEnumerable<string> code = c;

            code = AddRequiredHeaders(code);

            Tree<string> structureTree = TreeOutStructure(code);
            SinglyLine(structureTree);
            //foreach(var line in structureTree) {
            //    Console.WriteLine(line);
            //}

            List<Tree<string>> functionsAndProperties = PrepocessTree(structureTree);
            //foreach(var chunk in functionsAndProperties) {
            //    foreach(var line in chunk) {
            //        Console.WriteLine(line);
            //    }
            //}

            List<string> functionNames = ConvertFunctions(functionsAndProperties);
            foreach(var line in functionNames) {
                Console.WriteLine(line);
            }
            foreach(var chunk in functionsAndProperties) {
                foreach(var line in chunk) {
                    Console.WriteLine(line);
                }
            }

            //SinglyLine(functionsAndProperties);
            //foreach(var chunk in functionsAndProperties) {
            //    foreach(var line in chunk) {
            //        Console.WriteLine(line);
            //    }
            //}

            return structureTree.Where(x => x!= null).ToArray();
        }

        private enum LineType {
            FunctionDeclaration,
            FunctionCall,
            Assignment
        }
        private static readonly char[] lineDelimiters = new char[] {';', '{', '}'};
        private static void SinglyLine(IEnumerable<Tree<string>> functions) {
            foreach(Tree<string> function in functions) {
                SinglyLine(function);
            }
        }
        private static void SinglyLine(Tree<string> function) {
            Tree<string> ret = new Tree<string>();
            string workingLine = "";
            foreach(string l in function) {
                if(!string.IsNullOrWhiteSpace(l)) {
                    string line = l.Trim();
                    char end = line[line.Length - 1];

                    workingLine += " "+line;
                    if(lineDelimiters.Contains(end)) {
                        ret.Add(workingLine.Trim());
                        workingLine = "";
                    } else if(line.Where(x => lineDelimiters.Contains(x)).Any()) {
                        Tree<string> tmp = new Tree<string>();
                        tmp.Add(Regex.Split(workingLine, $@"(?<=[{new string(lineDelimiters.SelectMany(x => x.ToString()).ToArray())}])"));
                        SinglyLine(tmp);
                        ret.Add(tmp);
                        workingLine = "";
                    }
                }
            }

            function.children = ret.children;
            function.data = ret.data;
        }
        //private static List<Tree<string>> Predestack(IEnumerable<Tree<string>> functions, IEnumerable<string> watchlist) {
        //    List<Tree<string>> toEditLater = new List<Tree<string>>();
        //    foreach(Tree<string> function in functions) {
        //        List<string> returnParameters = new List<string>();
        //        while(true) {
        //            try {
        //                foreach(string line in function) {
        //                    Tree<string> n = new Tree<string>();
        //                    Action<Tree<string>, string> toLater = (Tree<string> tr, string toAdd) => {
        //                        Tree<string> e = new Tree<string>();
        //                        if(toAdd.ProperContains("=")) {
        //                            string[] sr = toAdd.Split('=');
        //                            e.Add(sr[0] + " = ");
        //                            if(sr.Length > 1) {
        //                                e.Add(toAdd.Split('=')[1]);
        //                                if(!toEditLater.Contains(tr)) {
        //                                    toEditLater.Add(tr);
        //                                }
        //                            }
        //                        } else {
        //                            e.Add(toAdd);
        //                        }
        //                        tr.Add(e);
        //                    };
        //                    if(returnParameters.Where(x => line.ProperContains(x)).Any()) {
        //                        toLater(n, line);
        //                    }
        //                    if(line.ProperContains("return ") && !line.ProperContains("yield return break;")) {
        //                        n.Add($"returnValuesDictionary[\"returnValue\"] = ");
        //                        n.Add($"    {line.Replace("return ", "").Trim()}");
        //                        n.Add($"SubStack.Push(returnValueKey, returnValuesDictionary);");

        //                        n.Add($"yield return break;");
        //                        function.Replace(line, n);
        //                    }
        //                    if(watchlist.Where(x => line.ProperContains(x)).Any()) {
        //                        if((line.ProperContains("out") || line.ProperContains("ref"))) {
        //                            returnParameters = line.Split(',', '(', ')')
        //                                        .Where(x => x.ProperContains("out") || x.ProperContains("ref"))
        //                                        .Select(x => x.Trim().Split(' ')[1].Trim())
        //                                        .ToList();

        //                            string e = line;
        //                            while(e.ProperContains("out") || e.ProperContains("ref")) {
        //                                string[] trm = e.Split('(', ')');
        //                                e = trm[0] + "(" +
        //                                    new string(trm[1].Replace("ref", "").Split(',').Where(x => !x.Contains("out ")).SelectMany(x => x + ",").Reverse().Skip(1).Reverse().ToArray())
        //                                    + ")" + trm[2];
        //                            }
        //                            n.Add(e);

        //                            n.Add($"long returnValueKey = 0;");
        //                            n.Add($"Dictionary<string, object> returnValuesDictionary = SubStack.Allocate(out returnValueKey);");
        //                            foreach(string val in returnParameters) {
        //                                n.Add($"returnValuesDictionary.Add(\"{val}\", null);");
        //                            }
        //                            function.Replace(line, n);
        //                        } else if(line.ProperContains("=")) {
        //                            toLater(n, line);
        //                        }
        //                    }
        //                }
        //                break;
        //            } catch { }
        //        }
        //    }
        //    return toEditLater;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="functions"></param>
        /// <returns>list of names of funtions</returns>
        private static List<string> ConvertFunctions(List<Tree<string>> functions) {
            List<string> retFunctions = new List<string>();

            foreach(Tree<string> tree in functions){
                string first = tree.Where(x => x != null).First();
                if(ProperContains(first, "(")) {
                    IEnumerable<int> indexes = tree.IndexOf(first);

                    string[] words = first.Replace("(", " (long returnValueKey, ").Replace(", )", ")").Split(' ');

                    string parenWord = words.Where(x => ProperContains(x, "(")).First();
                    int parenIndex = Array.IndexOf(words, parenWord);
                        
                    int funcIndex = parenIndex-1;
                    if(words[funcIndex] != "Main") {
                        retFunctions.Add(words[funcIndex]);

                        int typeIndex = funcIndex - 1;
                        string returnType = "";
                        returnType = words[typeIndex];
                        words[typeIndex] = $"IEnumerator<object>";

                        if(returnType != "void") {
                            words[parenIndex] = words[parenIndex].Replace("(", $" (out {returnType} returnValue, ");
                        }
                    }

                    string converted = new string(words.SelectMany(x => x.Trim() + " ").ToArray());
                    //Console.WriteLine(converted);

                    tree.Replace(first, converted);
                }
            }

            return retFunctions;
        }
        
        private static bool ProperContains (this string x, string y) {
            Regex catchStrings = new Regex("\".+\"");
            return x != null && catchStrings.Replace(x, string.Empty).Contains(y);
        }

        private enum TreeState{
            TopLevel,
            Namespace,
            Type,
            Function,
            Property,
            Logic
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="furtherProcessing"></param>
        /// <param name="lastState"></param>
        /// <returns>list of functions and properties</returns>
        private static List<Tree<string>> PrepocessTree(Tree<string> tree, List<Tree<string>> furtherProcessing = null, TreeState lastState = TreeState.TopLevel) {
            if(furtherProcessing == null) furtherProcessing = new List<Tree<string>>();

            TreeState state = TreeState.TopLevel;
            string line0 = tree[0]?.data ?? tree.First(x => x != null);
            if(line0 != null) {
                switch(lastState) {
                    case TreeState.TopLevel:
                        if(ProperContains(line0, "namespace")) {
                            state = TreeState.Namespace;
                            break;
                        } else {
                            goto case TreeState.Namespace;
                        }
                    case TreeState.Namespace:
                        if(ProperContains(line0, "class") || ProperContains(line0, "struct")) {
                            state = TreeState.Type;
                        }
                        break;
                    case TreeState.Type:
                        if(ProperContains(line0, "(")) {
                            state = TreeState.Function;
                        }else{
                            state = TreeState.Property;
                        }
                        break;
                    case TreeState.Function:
                    case TreeState.Property:
                        state = TreeState.Logic;
                        break;
                    case TreeState.Logic:
                    default:
                        break;
                }
            }
            
            //preprocess
            switch(state) {
                case TreeState.Function:
                case TreeState.Property:
                    //earmark for later de-stacking
                    furtherProcessing.Add(tree);
                    goto case TreeState.Logic;
                case TreeState.Logic:
                    //tree.Collapse();
                    return furtherProcessing;

                case TreeState.TopLevel:
                case TreeState.Namespace:
                case TreeState.Type:
                default:
                    foreach(Tree<string> child in tree.children) {
                        if(child[0]?.First() != null) { //don't process single lines
                            PrepocessTree(child, furtherProcessing, state);
                        }
                    }
                    break;
            }
            return furtherProcessing;
        }
        
        private static Tree<string> TreeOutStructure(IEnumerable<string> code) {
            return TreeOutStructure(code.GetEnumerator());
        }
        private static Tree<string> TreeOutStructure(IEnumerator<string> code) {
            Tree<string> tree = new Tree<string>();
            
            Func<string> curGetAndProcess = () => {
                string ret = code.Current;
                if(ret == null) return null;
                return ret;//.Trim();
            };

            List<string> sections = new List<string>();
            if(ProperContains(curGetAndProcess(), "{")){
                tree.Add(curGetAndProcess());
            }

            while(code.MoveNext()){
                if(ProperContains(curGetAndProcess(), "{")) {
                    tree.Add(TreeOutStructure(code));
                } else if(ProperContains(curGetAndProcess(),"}")) {
                    tree.Add(curGetAndProcess());
                    break;
                }else{
                    tree.Add(curGetAndProcess());
                }
            }

            return tree;
        }

        private static IEnumerable<string> AddRequiredHeaders(IEnumerable<string> code) {
            List<string> add = new List<string>();
            foreach(string header in requiredHeaders) {
                if(!code.Where(x => x.Contains(header)).Any()) {
                    add.Add(header);
                }
            }
            if(add.Count > 0) add.Add(Environment.NewLine);
            add.AddRange(code);

            return add;
        }
        #endregion
    }
}
