using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentationGenerator
{
    class Page
    {
        public string Name { get; internal set; }
        public StringBuilder Text = new StringBuilder();

        internal void Write( DirectoryInfo dir, string extension )
        {
            System.IO.File.WriteAllText( dir.FullName + "/" + Name + extension, Text.ToString() );
        }

        internal void WriteLine( string v = "" )
        {
            Text.AppendLine( v );
        }
    }
}
