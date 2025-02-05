using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcornUnObfuscate
{
    public class RiscOsPatterns
    {
        // Main WIMP patterns from documentation
        public static readonly Dictionary<string, (string name, string description)> WimpPatterns = new()
        {
            // Core WIMP patterns
            { @"SYS\s*""Wimp_Poll"".*b%", ("MainPoll", "Main WIMP polling loop") },
            { @"SYS\s*""Wimp_Initialise"".*task%", ("InitializeWimp", "WIMP initialization") },
            { @"Wimp_CreateIcon", ("CreateIcon", "Icon creation handler") },
            { @"Wimp_CloseDown", ("CloseWimp", "WIMP shutdown handler") },
            
            // Window handling
            { @"Wimp_CreateWindow", ("CreateWindow", "Window creation") },
            { @"Wimp_OpenWindow", ("OpenWindow", "Window opening handler") },
            { @"Wimp_CloseWindow", ("CloseWindow", "Window closing handler") },
            { @"Wimp_GetWindowState", ("GetWindowState", "Window state handler") },
            
            // Icon handling
            { @"iconbar", ("IconBar", "Icon bar handler") },
            { @"!b%=-1", ("IconBarIcon", "Icon bar icon creation") },
            { @"CreateIcon.*iconbar", ("CreateIconBar", "Icon bar creation") },
            
            // Message handling
            { @"Message_DataLoad", ("LoadData", "Data loading handler") },
            { @"Message_DataSave", ("SaveData", "Data saving handler") },
            { @"CASE\s*r%\s*OF", ("ProcessMessage", "Message processor") },
            
            // Menu handling
            { @"Menu_.*Selection", ("MenuSelect", "Menu selection handler") },
            { @"CreateMenu", ("CreateMenu", "Menu creation") },

            // Event handling
            { @"WHILE\s*TIME", ("TimerEvent", "Timer event handler") },
            { @"Mouse_Click", ("MouseClick", "Mouse click handler") },
            { @"Key_Pressed", ("KeyPress", "Key press handler") },

            // Template handling
            { @"Template", ("Template", "Template handler") },
            { @"LoadTemplate", ("LoadTemplate", "Template loader") },

            // Error handling
            { @"ONERROR", ("ErrorTrap", "Error trap handler") },
            { @"ERROR", ("ErrorHandler", "Error handler") }
        };

        // Common RISC OS SWI calls and their meanings
        public static readonly Dictionary<string, (string prefix, string meaning)> SwiPatterns = new()
        {
            { @"OS_CLI", ("Cli", "Command line interface") },
            { @"OS_File", ("File", "File operations") },
            { @"OS_GBPB", ("Buffer", "Buffer operations") },
            { @"OS_Find", ("Find", "File finding") },
            { @"OS_ReadVarVal", ("Variable", "Variable reading") },
            { @"OS_ReadModeVariable", ("Mode", "Screen mode") },
            { @"OS_Byte", ("Byte", "System setting") },
            { @"OS_Word", ("Word", "System operation") },
        };

        // Common procedure patterns from the unobfuscated code
        public static readonly Dictionary<string, string[]> CommonPatterns = new()
        {
            { "Poll", new[] { "REPEAT", "SYS.*Poll", "UNTIL", "reason%" } },
            { "Init", new[] { "DIM", "=-1", "=FALSE", "=0" } },
            { "Menu", new[] { "CASE.*OF", "WHEN.*\"", "ENDCASE" } },
            { "Icon", new[] { "!b%", "CreateIcon", "icon" } },
            { "Window", new[] { "window%", "OpenWindow", "CloseWindow" } },
            { "File", new[] { "OPENIN", "OPENOUT", "CLOSE#", "PTR#" } },
            { "Template", new[] { "Template", "DIM.*%", "buffer" } }
        };
    }
}
