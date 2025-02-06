using System.Text.RegularExpressions;

namespace AcornUnObfuscate
{
    // ReSharper disable once IdentifierTypo
    public class BasicDeobfuscator
    {
        private Dictionary<string, string> variableMap = new();
        private Dictionary<string, string> procMap = new();
        private Dictionary<string, VariableContext> variableContexts = new();
        private int _nextVarNumber = 1;

        // ReSharper disable once IdentifierTypo
        public List<string> DeobfuscateCode(List<string> lines)
        {
            // First pass: gather context
            GatherContext(lines);

            // Second pass: determine meaningful names
            DetermineNames();

            var result = new List<string>();
            var indentLevel = 0;

            foreach (var line in lines)
            {
                // Split line number and content
                var match = Regex.Match(line, @"^(\d+)\s+(.*)$");
                if (!match.Success) continue;

                var lineNumber = match.Groups[1].Value;
                var content = match.Groups[2].Value;

                // Process the line content
                // ReSharper disable once IdentifierTypo
                var deobfuscatedLine = DeobfuscateLine(content);

                // Handle indentation based on keywords
                var statements = deobfuscatedLine.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                //need to make we dont go out of bounds
                if (statements.Length == 0)
                    continue;
                var firstStatement = statements[0].Trim();

                // Reduce indent for ending blocks
                if (firstStatement.StartsWith("END") ||
                    firstStatement.StartsWith("NEXT") ||
                    firstStatement.StartsWith("UNTIL") ||
                    firstStatement.StartsWith("ELSE"))
                {
                    indentLevel = Math.Max(0, indentLevel - 1);
                }

                // Ensure non-negative indent
                var adjustedIndent = Math.Max(0, indentLevel);

                // Build the formatted line
                var formattedLine = $"{lineNumber} {new string(' ', adjustedIndent * 2)}{deobfuscatedLine}";
                result.Add(formattedLine);

                // Adjust indent for next line based on current line content
                foreach (var statement in statements)
                {
                    var trimmedStatement = statement.Trim();

                    // Increase indent after these structures
                    if (trimmedStatement.Contains("THEN") ||
                        trimmedStatement.StartsWith("FOR") ||
                        trimmedStatement.StartsWith("REPEAT") ||
                        trimmedStatement.EndsWith("OF") ||
                        (trimmedStatement.StartsWith("CASE") && !trimmedStatement.StartsWith("ENDCASE")))
                    {
                        indentLevel++;
                    }
                    // Handle ELSE specifically - we've already decreased for it, now increase for the following block
                    else if (trimmedStatement.StartsWith("ELSE"))
                    {
                        indentLevel++;
                    }
                }
            }

            return result;
        }

        // ReSharper disable once IdentifierTypo
        private string DeobfuscateLine(string line)
        {
            // Split multiple statements on colon
            var statements = line.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            var processedStatements = new List<string>();

            foreach (var statement in statements)
            {
                var processed = ProcessStatement(statement);
                processedStatements.Add(processed);
            }

            return string.Join(" : ", processedStatements);
        }

        private string ProcessStatement(string statement)
        {
            // Add spaces around operators
            statement = Regex.Replace(statement, @"([\+\-\*\/\=\<\>\,])", " $1 ");

            // Handle variable renaming
            statement = RenameVariables(statement);

            // Add space after keywords
            var keywords = new[] { "IF", "THEN", "ELSE", "FOR", "TO", "STEP", "NEXT",
                                 "CASE", "OF", "WHEN", "ENDCASE", "REPEAT", "UNTIL" };
            foreach (var keyword in keywords)
            {
                statement = Regex.Replace(statement,
                    $@"\b{keyword}\b",
                    $"{keyword} ",
                    RegexOptions.IgnoreCase);
            }

            // Clean up multiple spaces
            statement = Regex.Replace(statement, @"\s+", " ").Trim();

            return statement;
        }

        private string RenameVariables(string statement)
        {
            // Find variables (letter followed by % or $)
            var matches = Regex.Matches(statement, @"\b([a-zA-Z]+[%$])\b");
            foreach (Match match in matches)
            {
                var varName = match.Groups[1].Value;
                if (variableMap.ContainsKey(varName))
                {
                    statement = Regex.Replace(statement,
                        $@"\b{varName}\b",
                        variableMap[varName]);
                    continue;
                }

                // If we haven't mapped this variable yet, check its context
                if (variableContexts.TryGetValue(varName, out var context))
                {
                    string newName;
                    var suffix = varName.EndsWith("%") ? "%" : "$";

                    // Use context information to create a meaningful name
                    if (!string.IsNullOrEmpty(context.SuggestedName))
                    {
                        newName = $"{context.SuggestedName}{_nextVarNumber}{suffix}";
                    }
                    else if (context.IsArray)
                    {
                        newName = $"array{_nextVarNumber}{suffix}";
                    }
                    else if (context.IsCounter)
                    {
                        newName = $"counter{_nextVarNumber}{suffix}";
                    }
                    else if (context.IsFlag)
                    {
                        newName = $"flag{_nextVarNumber}{suffix}";
                    }
                    else if (context.IsFileName)
                    {
                        newName = $"fileName{_nextVarNumber}{suffix}";
                    }
                    else if (context.IsErrorHandler)
                    {
                        newName = $"error{_nextVarNumber}{suffix}";
                    }
                    else if (context.IsParameter)
                    {
                        newName = $"param{_nextVarNumber}{suffix}";
                    }
                    else if (context.PrimaryContext != null)
                    {
                        newName = $"{context.PrimaryContext.ToLower()}{_nextVarNumber}{suffix}";
                    }
                    else if (!string.IsNullOrEmpty(context.ProcedureContext))
                    {
                        newName = $"proc{context.ProcedureContext}Var{_nextVarNumber}{suffix}";
                    }
                    else
                    {
                        newName = varName.EndsWith("%") ? $"intVar{_nextVarNumber}%" : $"strVar{_nextVarNumber}$";
                    }

                    variableMap[varName] = newName;
                    _nextVarNumber++;

                    statement = Regex.Replace(statement,
                        $@"\b{varName}\b",
                        variableMap[varName]);
                }
            }

            // Also handle procedure names
            var procMatch = Regex.Match(statement, @"(DEFPROC|PROC)([a-zA-Z][a-zA-Z0-9_]*)");
            if (procMatch.Success)
            {
                var prefix = procMatch.Groups[1].Value;
                var procName = procMatch.Groups[2].Value;
                if (procMap.ContainsKey(procName))
                {
                    statement = statement.Replace(prefix + procName, prefix + procMap[procName]);
                }
            }

            return statement;
        }

        private void GatherContext(List<string> lines)
        {
            var currentProc = "";

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var match = Regex.Match(line, @"^(\d+)\s+(.*)$");
                if (!match.Success) continue;

                var content = match.Groups[2].Value;

                // Track procedure definitions
                var procMatch = Regex.Match(content, @"DEFPROC([a-zA-Z][a-zA-Z0-9_]*)");
                if (procMatch.Success)
                {
                    currentProc = procMatch.Groups[1].Value;
                    AnalyzeProcedureContext(content, i, lines);
                }

                // Track variable usage
                var varMatches = Regex.Matches(content, @"\b([a-zA-Z]+[%$])\b");
                foreach (Match varMatch in varMatches)
                {
                    var varName = varMatch.Groups[1].Value;
                    if (!variableContexts.ContainsKey(varName))
                    {
                        variableContexts[varName] = new VariableContext();
                    }

                    var context = variableContexts[varName];
                    context.UsageLines.Add(content);
                    context.ProcedureContext = currentProc;

                    // Analyze surrounding lines for context (-2 to +2 lines)
                    AnalyzeVariableContext(varName, i, lines);
                }
            }
        }

        private void DetermineNames()
        {
            foreach (var context in variableContexts)
            {
                var varName = context.Key;
                var ctx = context.Value;
                string newName;

                if (ctx.IsCounter)
                    newName = "counter" + _nextVarNumber + (varName.EndsWith("%") ? "%" : "$");
                else if (ctx.IsFlag)
                    newName = "flag" + _nextVarNumber + (varName.EndsWith("%") ? "%" : "$");
                else if (ctx.IsFileName)
                    newName = "fileName" + _nextVarNumber + (varName.EndsWith("%") ? "%" : "$");
                else if (ctx.IsErrorHandler)
                    newName = "error" + _nextVarNumber + (varName.EndsWith("%") ? "%" : "$");
                else if (ctx.RelatedKeywords.Any())
                    newName = ctx.RelatedKeywords.First().ToLower() + _nextVarNumber + (varName.EndsWith("%") ? "%" : "$");
                else if (ctx.ProcedureContext != "")
                    newName = "proc" + ctx.ProcedureContext + "Var" + _nextVarNumber + (varName.EndsWith("%") ? "%" : "$");
                else
                    newName = "var" + _nextVarNumber + (varName.EndsWith("%") ? "%" : "$");

                variableMap[varName] = newName;
                _nextVarNumber++;
            }

            // Also determine procedure names if they haven't been mapped yet
            foreach (var proc in procMap.Keys.ToList())
            {
                if (!procMap.ContainsKey(proc))
                {
                    procMap[proc] = "Proc" + proc;
                }
            }
        }

        // Helper method to identify type-specific prefixes
        private string GetTypePrefix(string varName, List<string> usageContext)
        {
            if (usageContext.Any(l => l.Contains("FOR") && l.Contains(varName)))
                return "loop";
            if (usageContext.Any(l => l.Contains("OPENIN") || l.Contains("OPENOUT")))
                return "file";
            if (usageContext.Any(l => l.Contains("ERROR") || l.Contains("ERL")))
                return "err";
            if (usageContext.Any(l => l.Contains("MENU") || l.Contains("WINDOW")))
                return "ui";
            if (usageContext.Any(l => l.Contains("PROC") && l.Contains(varName)))
                return "proc";

            return "var";
        }

        private void AnalyzeProcedureContext(string procDef, int lineIndex, List<string> lines)
        {
            // First try WIMP-specific analysis
            AnalyzeWimpProcedure(procDef, lineIndex, lines);

            // If no WIMP-specific name was assigned, fall back to general analysis
            var procMatch = Regex.Match(procDef, @"DEFPROC([a-zA-Z][a-zA-Z0-9_]*)");
            if (!procMatch.Success) return;

            var procName = procMatch.Groups[1].Value;

            // If procedure wasn't mapped by WIMP analysis, use original analysis
            if (!procMap.ContainsKey(procName))
            {
                var contextLines = new List<string>();
                string newProcName = null;

                // Collect next few lines after procedure definition for context
                for (var i = lineIndex + 1; i < Math.Min(lines.Count, lineIndex + 5); i++)
                {
                    var match = Regex.Match(lines[i], @"^(\d+)\s+(.*)$");
                    if (match.Success)
                        contextLines.Add(match.Groups[2].Value);
                }

                // Original action patterns
                var actionPatterns = new Dictionary<string, string>
                {
                    { @"OPEN\w+\s+[a-zA-Z%$]", "File" },
                    { @"SYS\s*""Wimp", "Wimp" },
                    { @"SYS[^""]+[a-zA-Z%$]", "System" },
                    { @"CASE|OF|WHEN|END\s*CASE", "Switch" },
                    { @"ERROR|ERR|ERL", "Error" },
                    { @"DRAW|PLOT|MOVE|COLOUR", "Draw" },
                    { @"SOUND|BEATS|VOICE|TEMPO", "Sound" },
                    { @"MOUSE|POINTER", "Mouse" },
                    { @"MENU|SELECT", "Menu" },
                    { @"LOAD|SAVE", "Data" }
                };

                // First look for action patterns
                foreach (var pattern in actionPatterns)
                {
                    if (contextLines.Any(line => Regex.IsMatch(line, pattern.Key, RegexOptions.IgnoreCase)))
                    {
                        newProcName = "Handle" + pattern.Value;
                        break;
                    }
                }

                // If no pattern found, use generic name
                if (newProcName == null)
                {
                    newProcName = "HandleSystem";
                }

                // Add unique number to avoid name conflicts
                var suffix = 1;
                var baseName = newProcName;
                while (procMap.ContainsValue(newProcName))
                {
                    newProcName = baseName + suffix;
                    suffix++;
                }

                procMap[procName] = newProcName;
            }
        }

        private void AnalyzeVariableContext(string varName, int lineIndex, List<string> lines)
        {
            var context = variableContexts[varName];
            var surroundingLines = new List<string>();
            var isInteger = varName.EndsWith("%");
            var isString = varName.EndsWith("$");

            // Collect lines before and after (2 lines each direction)
            for (var i = Math.Max(0, lineIndex - 2); i <= Math.Min(lines.Count - 1, lineIndex + 2); i++)
            {
                var match = Regex.Match(lines[i], @"^(\d+)\s+(.*)$");
                if (match.Success)
                    surroundingLines.Add(match.Groups[2].Value);
            }

            // Common variable usage patterns
            var patterns = new Dictionary<string, (string category, string suggestedName)>
            {
                // File handling patterns
                { @"OPENIN.*" + varName, ("File", "inputFile") },
                { @"OPENOUT.*" + varName, ("File", "outputFile") },
                { @"BGET.*" + varName, ("File", "fileHandle") },
                { @"BPUT.*" + varName, ("File", "fileHandle") },
                
                // Window/UI patterns
                { @"WINDOW.*" + varName, ("Window", "window") },
                { @"MENU.*" + varName, ("Menu", "menu") },
                { @"ICON.*" + varName, ("UI", "icon") },
                { @"BUTTON.*" + varName, ("UI", "button") },
                
                // System patterns
                { @"SYS.*" + varName, ("System", "sysParam") },
                { @"TIME.*" + varName, ("System", "time") },
                { @"PAGE.*" + varName, ("System", "page") },
                
                // Graphics patterns
                { @"PLOT.*" + varName, ("Graphics", "plotCoord") },
                { @"DRAW.*" + varName, ("Graphics", "drawCoord") },
                { @"COLOUR.*" + varName, ("Graphics", "color") },
                { @"POINT.*" + varName, ("Graphics", "point") },

                // Counter patterns
                { @"FOR\s+" + varName + @"\s*=", ("Counter", "index") },
                { @"STEP.*" + varName, ("Counter", "step") },
                { @"COUNT.*" + varName, ("Counter", "count") },

                // Boolean/Flag patterns
                { @"IF.*" + varName + @".*THEN", ("Flag", "flag") },
                { @"UNTIL.*" + varName, ("Flag", "condition") },
                { @"WHILE.*" + varName, ("Flag", "condition") },

                // Error handling patterns
                { @"ERROR.*" + varName, ("Error", "errorCode") },
                { @"ERL.*" + varName, ("Error", "errorLine") },
                { @"ERR.*" + varName, ("Error", "error") }
            };

            // Check each line against patterns
            foreach (var line in surroundingLines)
            {
                foreach (var pattern in patterns)
                {
                    if (Regex.IsMatch(line, pattern.Key, RegexOptions.IgnoreCase))
                    {
                        context.RelatedKeywords.Add(pattern.Value.category);
                        context.SuggestedName = pattern.Value.suggestedName;
                        break;
                    }
                }

                // Special case analysis
                AnalyzeSpecialCases(line, varName, context);
            }

            // Post-analysis refinements
            RefineVariableContext(context, isInteger, isString);
        }

        private void AnalyzeSpecialCases(string line, string varName, VariableContext context)
        {
            // Detect array usage
            if (Regex.IsMatch(line, varName + @"\s*\([^\)]+\)"))
            {
                context.IsArray = true;
                if (!context.RelatedKeywords.Contains("Array"))
                    context.RelatedKeywords.Add("Array");
            }

            // Detect string manipulation
            if (Regex.IsMatch(line, @"LEFT\$|RIGHT\$|MID\$|STR\$|STRING\$") && line.Contains(varName))
            {
                context.IsStringManipulation = true;
                if (!context.RelatedKeywords.Contains("String"))
                    context.RelatedKeywords.Add("String");
            }

            // Detect mathematical operations
            if (Regex.IsMatch(line, @"[+\-*/\\]" + varName) || Regex.IsMatch(line, varName + @"[+\-*/\\]"))
            {
                context.IsMathOperation = true;
                if (!context.RelatedKeywords.Contains("Math"))
                    context.RelatedKeywords.Add("Math");
            }

            // Detect procedure parameters
            if (Regex.IsMatch(line, @"PROC\w+[^)]*" + varName + @"[^)]*\)"))
            {
                context.IsParameter = true;
                if (!context.RelatedKeywords.Contains("Param"))
                    context.RelatedKeywords.Add("Param");
            }
        }

        private void RefineVariableContext(VariableContext context, bool isInteger, bool isString)
        {
            // If we have multiple contexts, prioritize them
            if (context.RelatedKeywords.Count > 1)
            {
                // Priority order for multiple contexts
                var priorityOrder = new[]
                {
                    "Error", "File", "Window", "Menu", "Graphics",
                    "Counter", "Flag", "System", "Array", "String",
                    "Math", "Param"
                };

                foreach (var priority in priorityOrder)
                {
                    if (context.RelatedKeywords.Contains(priority))
                    {
                        context.PrimaryContext = priority;
                        break;
                    }
                }
            }
            else if (context.RelatedKeywords.Count == 1)
            {
                context.PrimaryContext = context.RelatedKeywords.First();
            }

            // Add type-specific context if not already present
            if (isInteger && !context.RelatedKeywords.Contains("Integer"))
                context.RelatedKeywords.Add("Integer");
            if (isString && !context.RelatedKeywords.Contains("String"))
                context.RelatedKeywords.Add("String");

            // Set IsTemporary if the variable is used in a very limited scope
            context.IsTemporary = context.UsageLines.Count <= 2;
        }

        private void AnalyzeWimpProcedure(string procDef, int lineIndex, List<string> lines)
        {
            var procMatch = Regex.Match(procDef, @"DEFPROC([a-zA-Z][a-zA-Z0-9_]*)");
            if (!procMatch.Success) return;

            var procName = procMatch.Groups[1].Value;
            var procedureLines = new List<string>();

            // Get the full procedure content
            var i = lineIndex + 1;
            while (i < lines.Count && !lines[i].Contains("ENDPROC"))
            {
                var match = Regex.Match(lines[i], @"^(\d+)\s+(.*)$");
                if (match.Success)
                {
                    procedureLines.Add(match.Groups[2].Value);
                }
                i++;
            }

            string newProcName = null;
            var fullProcText = string.Join(" ", procedureLines);

            // Check WIMP patterns first
            foreach (var pattern in RiscOsPatterns.WimpPatterns)
            {
                if (procedureLines.Any(l => Regex.IsMatch(l, pattern.Key)))
                {
                    newProcName = pattern.Value.name;
                    break;
                }
            }

            // If no WIMP pattern matched, check SWI patterns
            if (newProcName == null)
            {
                foreach (var swi in RiscOsPatterns.SwiPatterns)
                {
                    if (procedureLines.Any(l => l.Contains(swi.Key)))
                    {
                        newProcName = $"Handle{swi.Value.prefix}";
                        break;
                    }
                }
            }

            // If still no match, check common patterns
            if (newProcName == null)
            {
                foreach (var pattern in RiscOsPatterns.CommonPatterns)
                {
                    if (pattern.Value.All(p => procedureLines.Any(l => Regex.IsMatch(l, p, RegexOptions.IgnoreCase))))
                    {
                        newProcName = $"Handle{pattern.Key}";
                        break;
                    }
                }
            }

            // Special cases based on content analysis
            if (newProcName == null)
            {
                if (procedureLines.Any(l => l.Contains("CASE") && l.Contains("WHEN") && l.Contains("\"")))
                {
                    // Look for message handling
                    var messageMatch = Regex.Match(fullProcText, @"WHEN\s+""([^""]+)""");
                    if (messageMatch.Success)
                    {
                        newProcName = $"Process{messageMatch.Groups[1].Value}Message";
                    }
                }
                else if (procedureLines.Any(l => l.Contains("Template")))
                {
                    newProcName = "HandleTemplate";
                }
            }

            // Add unique number if name exists
            if (newProcName != null)
            {
                var suffix = 1;
                var baseName = newProcName;
                while (procMap.ContainsValue(newProcName))
                {
                    newProcName = baseName + suffix;
                    suffix++;
                }

                procMap[procName] = newProcName;
            }
        }
    }
}

