using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AcornUnObfuscate
{
    public class BasicDetokenizer
    {
        // Token lists based on BBC BASIC V specification
        private static readonly string[] MainTokens = new string[]
        {
            "OTHERWISE", "AND", "DIV", "EOR", "MOD", "OR", "ERROR", "LINE", "OFF",
            "STEP", "SPC", "TAB(", "ELSE", "THEN", "<line>", "OPENIN", "PTR",
            "PAGE", "TIME", "LOMEM", "HIMEM", "ABS", "ACS", "ADVAL", "ASC",
            "ASN", "ATN", "BGET", "COS", "COUNT", "DEG", "ERL", "ERR",
            "EVAL", "EXP", "EXT", "FALSE", "FN", "GET", "INKEY", "INSTR(",
            "INT", "LEN", "LN", "LOG", "NOT", "OPENUP", "OPENOUT", "PI",
            "POINT(", "POS", "RAD", "RND", "SGN", "SIN", "SQR", "TAN",
            "TO", "TRUE", "USR", "VAL", "VPOS", "CHR$", "GET$", "INKEY$",
            "LEFT$(", "MID$(", "RIGHT$(", "STR$", "STRING$(", "EOF",
            "<ESCFN>", "<ESCCOM>", "<ESCSTMT>",
            "WHEN", "OF", "ENDCASE", "ELSE", "ENDIF", "ENDWHILE", "PTR",
            "PAGE", "TIME", "LOMEM", "HIMEM", "SOUND", "BPUT", "CALL", "CHAIN",
            "CLEAR", "CLOSE", "CLG", "CLS", "DATA", "DEF", "DIM", "DRAW",
            "END", "ENDPROC", "ENVELOPE", "FOR", "GOSUB", "GOTO", "GCOL", "IF",
            "INPUT", "LET", "LOCAL", "MODE", "MOVE", "NEXT", "ON", "VDU",
            "PLOT", "PRINT", "PROC", "READ", "REM", "REPEAT", "REPORT", "RESTORE",
            "RETURN", "RUN", "STOP", "COLOUR", "TRACE", "UNTIL", "WIDTH", "OSCLI"
        };

        private static readonly string[] ExtendedFunctionTokens = new string[]
        {
            "SUM", "BEAT"
        };

        private static readonly string[] ExtendedCommandTokens = new string[]
        {
            "APPEND", "AUTO", "CRUNCH", "DELET", "EDIT", "HELP", "LIST", "LOAD",
            "LVAR", "NEW", "OLD", "RENUMBER", "SAVE", "TEXTLOAD", "TEXTSAVE", "TWIN",
            "TWINO", "INSTALL"
        };

        private static readonly string[] ExtendedStatementTokens = new string[]
        {
            "CASE", "CIRCLE", "FILL", "ORIGIN", "PSET", "RECT", "SWAP", "WHILE",
            "WAIT", "MOUSE", "QUIT", "SYS", "INSTALL", "LIBRARY", "TINT", "ELLIPSE",
            "BEATS", "TEMPO", "VOICES", "VOICE", "STEREO", "OVERLAY"
        };

        public class BasicLine
        {
            public int LineNumber { get; set; }
            public string Content { get; set; }
        }

        public List<BasicLine> DetokenizeFile(string filePath)
        {
            var lines = new List<BasicLine>();
            byte[] fileData = File.ReadAllBytes(filePath);
            int position = 0;

            while (position < fileData.Length)
            {
                // Check for CR marker
                if (fileData[position] != 0x0D)
                    throw new Exception("Invalid file format: Expected CR marker");

                position++;

                // Check for end of program marker
                if (position < fileData.Length && fileData[position] == 0xFF)
                    break;

                // Read line number (2 bytes)
                int lineNumber = (fileData[position] << 8) | fileData[position + 1];
                position += 2;

                // Read line length
                int lineLength = fileData[position];
                position++;

                // Read line content
                byte[] lineData = new byte[lineLength - 4]; // -4 for CR, line number, and length bytes
                Array.Copy(fileData, position, lineData, 0, lineData.Length);

                string detokenizedLine = DetokenizeLine(lineData);
                lines.Add(new BasicLine { LineNumber = lineNumber, Content = detokenizedLine });

                position += lineData.Length;
            }

            return lines;
        }

        private string DetokenizeLine(byte[] lineData)
        {
            StringBuilder result = new StringBuilder();
            int position = 0;

            while (position < lineData.Length)
            {
                byte currentByte = lineData[position];

                if (currentByte >= 0x7F)
                {
                    // Handle extended tokens
                    if (currentByte == 0xC6 || currentByte == 0xC7 || currentByte == 0xC8)
                    {
                        if (position + 1 >= lineData.Length)
                            throw new Exception("Invalid token data");

                        position++;
                        byte tokenByte = lineData[position];
                        int tokenIndex = tokenByte - 0x8E;

                        switch (currentByte)
                        {
                            case 0xC6:
                                if (tokenIndex < ExtendedFunctionTokens.Length)
                                    result.Append(ExtendedFunctionTokens[tokenIndex]);
                                break;
                            case 0xC7:
                                if (tokenIndex < ExtendedCommandTokens.Length)
                                    result.Append(ExtendedCommandTokens[tokenIndex]);
                                break;
                            case 0xC8:
                                if (tokenIndex < ExtendedStatementTokens.Length)
                                    result.Append(ExtendedStatementTokens[tokenIndex]);
                                break;
                        }
                    }
                    // Handle line number references (0x8D)
                    else if (currentByte == 0x8D)
                    {
                        if (position + 3 >= lineData.Length)
                            throw new Exception("Invalid line number reference");

                        // Process 3-byte line number reference
                        position++;
                        // Implementation of line number reference handling can be added here
                        result.Append("<LINE_REF>");
                        position += 2;
                    }
                    // Handle main tokens
                    else
                    {
                        int tokenIndex = currentByte - 0x7F;
                        if (tokenIndex < MainTokens.Length)
                            result.Append(MainTokens[tokenIndex]);
                    }
                }
                else
                {
                    // Regular ASCII character
                    result.Append((char)currentByte);
                }

                position++;
            }

            return result.ToString();
        }
    }
}