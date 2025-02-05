using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AcornUnObfuscate
{
        public class BasicDeobfuscator
        {
            private Dictionary<string, string> variableMap;
            private int nextVarNumber;

            public BasicDeobfuscator()
            {
                variableMap = new Dictionary<string, string>();
                nextVarNumber = 1;
            }

            public List<string> DeobfuscateCode(List<string> lines)
            {
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
                    var deobfuscatedLine = DeobfuscateLine(content);

                    // Handle indentation based on keywords
                    var statements = deobfuscatedLine.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
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
                    if (!variableMap.ContainsKey(varName))
                    {
                        // Generate a more meaningful name based on type
                        string newName = varName.EndsWith("%") ?
                            $"intVar{nextVarNumber}%" :
                            $"strVar{nextVarNumber}$";
                        variableMap[varName] = newName;
                        nextVarNumber++;
                    }
                    statement = Regex.Replace(statement,
                        $@"\b{varName}\b",
                        variableMap[varName]);
                }

                return statement;
            }
        }
    }
