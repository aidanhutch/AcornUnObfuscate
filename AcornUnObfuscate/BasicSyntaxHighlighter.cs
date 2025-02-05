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

        // BBC BASIC Keywords
        private readonly HashSet<string> Keywords = new HashSet<string>
        {
            "AND", "DIV", "EOR", "MOD", "OR", "ERROR", "LINE", "OFF", "STEP", "SPC", "TAB",
            "ELSE", "THEN", "OPENIN", "PTR", "PAGE", "TIME", "LOMEM", "HIMEM", "TRUE", "FALSE",
            "DEF", "ENDPROC", "LOCAL", "RETURN", "REPEAT", "UNTIL", "FOR", "NEXT", "GOTO",
            "GOSUB", "IF", "CASE", "WHEN", "OF", "ENDCASE", "WHILE", "ENDIF", "ENDWHILE",
            "DIM", "PRINT", "INPUT", "REM", "DATA", "READ", "RESTORE", "CLS", "CLG", "MODE",
            "ENVELOPE", "SOUND", "MOVE", "DRAW", "PLOT", "GCOL", "COLOUR", "VDU", "PROC", "FN"
        };

        public BasicSyntaxHighlighter(RichTextBox richTextBox)
        {
            _richTextBox = richTextBox;
        }

        public void HighlightSyntax()
        {
            string text = _richTextBox.Text;
            // Store current selection
            int selectionStart = _richTextBox.SelectionStart;
            int selectionLength = _richTextBox.SelectionLength;

            //_richTextBox.BeginUpdate();
            _richTextBox.SuspendLayout();

            // Default color for all text
            _richTextBox.SelectAll();
            _richTextBox.SelectionColor = DefaultColor;

            // Process each line separately
            string[] lines = text.Split('\n');
            int currentPosition = 0;

            foreach (string line in lines)
            {
                // Handle line numbers
                var lineNumberMatch = Regex.Match(line, @"^\s*(\d+)\s");
                if (lineNumberMatch.Success)
                {
                    ColorSegment(currentPosition, lineNumberMatch.Length, NumberColor);
                    currentPosition += lineNumberMatch.Length;
                }

                // Handle REM comments
                var remMatch = Regex.Match(line, @"\bREM\b.*$");
                if (remMatch.Success)
                {
                    ColorSegment(currentPosition + remMatch.Index, remMatch.Length, CommentColor);
                }
                else
                {
                    // Handle strings
                    foreach (Match match in Regex.Matches(line, "\"[^\"]*\""))
                    {
                        ColorSegment(currentPosition + match.Index, match.Length, StringColor);
                    }

                    // Handle keywords
                    foreach (string keyword in Keywords)
                    {
                        foreach (Match match in Regex.Matches(line, $@"\b{keyword}\b"))
                        {
                            ColorSegment(currentPosition + match.Index, match.Length, KeywordColor);
                        }
                    }

                    // Handle PROC calls and definitions
                    foreach (Match match in Regex.Matches(line, @"(PROC|FN)[A-Za-z0-9_]+"))
                    {
                        ColorSegment(currentPosition + match.Index, match.Length, ProcColor);
                    }

                    // Handle SYS calls
                    foreach (Match match in Regex.Matches(line, @"SYS\s+""[^""]*"""))
                    {
                        ColorSegment(currentPosition + match.Index, match.Length, SysColor);
                    }

                    // Handle numbers
                    foreach (Match match in Regex.Matches(line, @"\b\d+\b|&[0-9A-Fa-f]+\b"))
                    {
                        ColorSegment(currentPosition + match.Index, match.Length, NumberColor);
                    }
                }

                currentPosition += line.Length + 1; // +1 for newline
            }

            // Restore selection
            _richTextBox.SelectionStart = selectionStart;
            _richTextBox.SelectionLength = selectionLength;
            _richTextBox.SelectionColor = DefaultColor;

            _richTextBox.ResumeLayout();
            //_richTextBox.EndUpdate();
        }

        private void ColorSegment(int start, int length, Color color)
        {
            if (start < 0 || length <= 0 || start + length > _richTextBox.TextLength)
                return;

            _richTextBox.SelectionStart = start;
            _richTextBox.SelectionLength = length;
            _richTextBox.SelectionColor = color;
        }
    }
}