using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;

namespace AcornUnOfuscate
{
    public class BasicSyntaxHighlighter
    {
        private readonly RichTextBox _richTextBox;

        // Visual Studio-like colors
        private readonly Color KeywordColor = Color.FromArgb(86, 156, 214);    // Blue for keywords
        private readonly Color StringColor = Color.FromArgb(214, 157, 133);    // Orange-brown for strings
        private readonly Color CommentColor = Color.FromArgb(87, 166, 74);     // Green for comments
        private readonly Color NumberColor = Color.FromArgb(181, 206, 168);    // Light green for numbers
        private readonly Color DefaultColor = Color.FromArgb(220, 220, 220);   // Light grey for default text
        private readonly Color ProcColor = Color.FromArgb(220, 220, 170);      // Light yellow for PROC/FN
        private readonly Color SysColor = Color.FromArgb(197, 134, 192);       // Purple for SYS calls
        private readonly Color OperatorColor = Color.FromArgb(180, 180, 180);  // Grey for operators

        // BBC BASIC Keywords - expanded list
        private readonly HashSet<string> Keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "AND", "DIV", "EOR", "MOD", "OR", "ERROR", "LINE", "OFF", "STEP", "SPC", "TAB",
            "ELSE", "THEN", "OPENIN", "PTR", "PAGE", "TIME", "LOMEM", "HIMEM", "TRUE", "FALSE",
            "DEF", "ENDPROC", "LOCAL", "RETURN", "REPEAT", "UNTIL", "FOR", "NEXT", "GOTO",
            "GOSUB", "IF", "CASE", "WHEN", "OF", "ENDCASE", "WHILE", "ENDIF", "ENDWHILE",
            "DIM", "PRINT", "INPUT", "REM", "DATA", "READ", "RESTORE", "CLS", "CLG", "MODE",
            "ENVELOPE", "SOUND", "MOVE", "DRAW", "PLOT", "GCOL", "COLOUR", "VDU", "PROC", "FN",
            "ENDCASE", "OTHERWISE", "CHAIN", "OSCLI", "END", "CLOSE", "OPENOUT", "BPUT", "BGET"
        };

        private readonly HashSet<string> Operators = new HashSet<string>
        {
            "+", "-", "*", "/", "=", "<", ">", ">=", "<=", "<>", "<<", ">>", "!", "?", "$", "%"
        };

        public BasicSyntaxHighlighter(RichTextBox richTextBox)
        {
            _richTextBox = richTextBox;
        }

        public void HighlightSyntax()
        {
            //_richTextBox.BeginUpdate();
            _richTextBox.SuspendLayout();

            // Store current selection
            int selectionStart = _richTextBox.SelectionStart;
            int selectionLength = _richTextBox.SelectionLength;

            // Default color for all text
            _richTextBox.SelectionStart = 0;
            _richTextBox.SelectionLength = _richTextBox.TextLength;
            _richTextBox.SelectionColor = DefaultColor;

            // Process each line
            int position = 0;
            string[] lines = _richTextBox.Lines;

            foreach (string originalLine in lines)
            {
                if (string.IsNullOrWhiteSpace(originalLine))
                {
                    position += originalLine.Length + 1;
                    continue;
                }

                string line = originalLine;
                int lineStart = position;

                // Handle line numbers first
                Match lineNumMatch = Regex.Match(line, @"^\s*(\d+)\s");
                if (lineNumMatch.Success)
                {
                    ColorSegment(lineStart + lineNumMatch.Groups[1].Index,
                               lineNumMatch.Groups[1].Length,
                               NumberColor);
                    line = line.Substring(lineNumMatch.Length);
                    lineStart += lineNumMatch.Length;
                }

                // Handle REM comments - these take precedence
                int remIndex = line.IndexOf("REM", StringComparison.OrdinalIgnoreCase);
                if (remIndex >= 0)
                {
                    ColorSegment(lineStart + remIndex, line.Length - remIndex, CommentColor);
                }
                else
                {
                    // Handle string literals
                    int startQuote = -1;
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (line[i] == '"')
                        {
                            if (startQuote == -1)
                                startQuote = i;
                            else
                            {
                                ColorSegment(lineStart + startQuote, i - startQuote + 1, StringColor);
                                startQuote = -1;
                            }
                        }
                    }

                    // Handle keywords
                    foreach (string keyword in Keywords)
                    {
                        foreach (Match match in Regex.Matches(line, $@"\b{keyword}\b", RegexOptions.IgnoreCase))
                        {
                            ColorSegment(lineStart + match.Index, match.Length, KeywordColor);
                        }
                    }

                    // Handle PROC calls and definitions separately
                    foreach (Match match in Regex.Matches(line, @"\b(PROC|FN)[A-Za-z0-9_]+", RegexOptions.IgnoreCase))
                    {
                        ColorSegment(lineStart + match.Index, match.Length, ProcColor);
                    }

                    // Handle SYS commands
                    foreach (Match match in Regex.Matches(line, @"SYS\s*""[^""]*""", RegexOptions.IgnoreCase))
                    {
                        ColorSegment(lineStart + match.Index, match.Length, SysColor);
                    }

                    // Handle numbers (including hex)
                    foreach (Match match in Regex.Matches(line, @"\b(&[0-9A-Fa-f]+|\d+)\b"))
                    {
                        ColorSegment(lineStart + match.Index, match.Length, NumberColor);
                    }

                    // Handle operators
                    foreach (string op in Operators)
                    {
                        int opIndex = 0;
                        while ((opIndex = line.IndexOf(op, opIndex)) != -1)
                        {
                            ColorSegment(lineStart + opIndex, op.Length, OperatorColor);
                            opIndex += op.Length;
                        }
                    }
                }

                position += originalLine.Length + 1; // +1 for newline
            }

            // Restore selection
            _richTextBox.SelectionStart = selectionStart;
            _richTextBox.SelectionLength = selectionLength;

            _richTextBox.ResumeLayout();
            //_richTextBox.EndUpdate();
        }

        private void ColorSegment(int start, int length, Color color)
        {
            try
            {
                if (start < 0 || length <= 0 || start >= _richTextBox.TextLength)
                    return;

                // Ensure we don't go past the end of the text
                if (start + length > _richTextBox.TextLength)
                    length = _richTextBox.TextLength - start;

                _richTextBox.SelectionStart = start;
                _richTextBox.SelectionLength = length;
                _richTextBox.SelectionColor = color;
            }
            catch
            {
                // Ignore any out of range errors
            }
        }
    }
}