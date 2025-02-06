using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcornUnObfuscate
{
    public class VariableContext
    {
        public List<string> UsageLines { get; set; } = new List<string>();
        public string ProcedureContext { get; set; }
        public string PrimaryContext { get; set; }
        public string SuggestedName { get; set; }
        public HashSet<string> RelatedKeywords { get; set; } = new HashSet<string>();

        // Type flags
        public bool IsCounter { get; set; }
        public bool IsFlag { get; set; }
        public bool IsFileName { get; set; }
        public bool IsErrorHandler { get; set; }
        public bool IsArray { get; set; }
        public bool IsStringManipulation { get; set; }
        public bool IsMathOperation { get; set; }
        public bool IsParameter { get; set; }
        public bool IsTemporary { get; set; }
    }
}
