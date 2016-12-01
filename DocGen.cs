using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DocumentationGenerator
{
    public class DocInfo
    {
        public string Summary;
        public string Example;
    }

    class DocGen
    {
        public DirectoryInfo OutputFolder { get; internal set; }
        public List<Type> Types = new List<Type>();

        public Dictionary<string, DocInfo> Documentation = new Dictionary<string, DocInfo>();

        internal void Process()
        {
            
        }

        internal DocInfo GetDocumentation( string name )
        {
            //if ( !Documentation.ContainsKey( name ) ) return new DocInfo() { Summary = name };
            if ( !Documentation.ContainsKey( name ) ) return null;
            

            return Documentation[name];
        }

        string ParamPrint( Type t )
        {
            if ( t.IsGenericType )
            {
                var par = t.GetGenericArguments();
                var args = "";
                if ( par.Length > 0 )
                {
                    args = string.Join( ",", par.Select( x => ParamPrint( x ) ) ).Replace( "+", "." ).Replace( "&", "@" );
                    args = $"{{{args}}}";
                }

                return $"{t.GetGenericTypeDefinition().FullName}{args}".Replace( "`1", "" ).Replace( "`2", "" ).Replace( "`3", "" ).Replace( "`4", "" );
                Console.WriteLine( t );
            }

            return t.FullName.Replace( "+", "." );
        }


        internal DocInfo GetDocumentation( MethodInfo o )
        {
            if ( o.Name == "UpdateWhile" )
                Console.Write( "" );

            var args = "";
            var par = o.GetParameters();
            if ( par.Length > 0 )
            {
                args = string.Join( ",", par.Select( x => ParamPrint( x.ParameterType )) ).Replace( "+", "." ).Replace( "&", "@" );
                args = $"({args})";
            }

            var val = GetDocumentation( $"M:{o.DeclaringType.FullName.Replace( "+", "." ).Replace( "&", "@" )}.{o.Name}{args}" );

            return val;
        }

        internal DocInfo GetDocumentation( ConstructorInfo o )
        {
            var args = "";
            var par = o.GetParameters();
            if ( par.Length > 0 )
            {
                args = string.Join( ",", par.Select( x => x.ParameterType.FullName ) ).Replace( "+", "." ).Replace( "&", "@" );
                args = $"({args})";
            }

            var val = GetDocumentation( $"M:{o.DeclaringType.FullName.Replace( "+", "." ).Replace( "&", "@" )}.#ctor{args}" );

            return val;
        }

        internal DocInfo GetDocumentation( PropertyInfo o )
        {
            return GetDocumentation( $"P:{o.DeclaringType.FullName.Replace( "+", "." ).Replace( "&", "@" )}.{o.Name}" );
        }

        internal DocInfo GetDocumentation( FieldInfo o )
        {
            return GetDocumentation( $"F:{o.DeclaringType.FullName.Replace( "+", "." ).Replace( "&", "@" )}.{o.Name}" );
        }

        internal DocInfo GetDocumentation( Type o )
        {
            if ( o.FullName.Contains( "Recipe" ) )
            {
                System.Diagnostics.Debug.WriteLine( o.FullName );
            }

            return GetDocumentation( $"T:{o.FullName.Replace( "+", "." ).Replace( "&", "@" )}" );
        }

        internal void AddLibrary( string library )
        {
            var assembly = Assembly.LoadFile( library );
            if ( assembly == null )
                throw new System.Exception( "Library missing" );

            Types.AddRange( assembly.ExportedTypes );

            var xmlName = library.Replace( ".dll", ".xml" );
            if ( System.IO.File.Exists( xmlName ) )
            {
                AddDocumentation( xmlName );
            }
        }

        private void AddDocumentation( string xmlName )
        {
            var document = new XmlDocument();
            document.Load( xmlName );

            foreach ( XmlNode m in document.SelectSingleNode( ".//members" ) )
            {
                if ( m.Attributes == null ) continue;

                var d = new DocInfo();

                var n = m.SelectSingleNode( "summary" );
                if ( n  != null )
                    d.Summary = string.Join( "\n", n.InnerText.Split( new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries ).Select( x => CleanLine( x ) ).Skip( 1 ) );

                n = m.SelectSingleNode( "example" );
                if ( n != null )
                    d.Example = string.Join( "\n", n.InnerText.Split( new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries ).Select( x => CleanLine( x ) ).Skip( 1 ) );

                Documentation.Add( m.Attributes["name"].Value, d );
            }
        }

        public string CleanLine( string s )
        {
            if ( s.StartsWith( "            " ) )
                s = s.Substring( 12 );

            return s;
        }
    }
}
