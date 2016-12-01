using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DocumentationGenerator
{
    class GithubOutput
    {
        public DocGen DocGen { get; internal set; }

        List<Page> Pages;

        public void ToFolder( DirectoryInfo directoryInfo )
        {
            Pages = new List<Page>();

            foreach ( var t in DocGen.Types.GroupBy( x => x.Assembly ) )
            {
                AssemblyPage( t.Key, t.ToArray() );
            }

            foreach ( var page in Pages )
            {
                page.Write( directoryInfo, ".md" );
            }
        }

        private void AssemblyPage( Assembly asm, Type[] type )
        {
            var page = new Page();
            page.Name = asm.FullName.Substring( 0, asm.FullName.IndexOf( ',' ) );

            page.WriteLine( $"# {page.Name}" );
            page.WriteLine();

            foreach ( var g in type.GroupBy( x => x.Namespace ) )
            {
                page.WriteLine();
                page.WriteLine( $"## {g.Key}" );

                foreach ( var t in g.Where( x => !x.IsEnum ) )
                {
                    if ( !t.FullName.Contains( "+" ) && t.DeclaringType == null )
                    {
                        page.WriteLine( $"* [[{t.Name}|{g.Key}.{t.Name}]]" );
                    }

                    ClassPage( asm, type, page, t );
                }
            }

            Pages.Add( page );
        }

        private void ClassPage( Assembly asm, Type[] type, Page parentPage, Type t )
        {
            var page = new Page();
            page.Name = t.FullName.Replace( "+", "." );

            page.WriteLine( "# " + t.Name );
            page.WriteLine( "" );

            var classDoc = DocGen.GetDocumentation( t );

            AddDocumentation( page, classDoc );

            var constructors = t.GetConstructors( BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public )
                                .OrderBy( x => x.Name )
                                .ToArray();

            var methods = t.GetMethods( BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public )
                                .Where ( x => !x.IsSpecialName && x.DeclaringType == t )
                                .OrderBy( x => x.Name )
                                .ToArray();

            var properties = t.GetProperties()
                                .OrderBy( x => x.Name )
                                .ToArray();

            var fields = t.GetFields()
                                .ToArray();

            if ( constructors.Length > 0 )
            {
                page.WriteLine();
                page.WriteLine( "### Constructors" );

                int i = 1;
                foreach ( var o in constructors )
                {
                    var doc = DocGen.GetDocumentation( o );
                    var pageLink = $"{page.Name}.{o.DeclaringType.Name}.{i}";

                    
                    var def = $"public **[[{o.DeclaringType.Name}|{pageLink}]]**( {string.Join( ", ", o.GetParameters().Select( x => PrintableParam( x.ParameterType, x.Name ) ) )} );";

                    page.WriteLine( $"* {def}" );

                    VariablePage( page.Name, o.DeclaringType.Name + "." + i, "Constructor", def, doc );
                    i++;
                }
            }



            if ( methods.Length > 0 )
            {
                page.WriteLine();
                page.WriteLine( "### Methods" );

                foreach ( var o in methods )
                {
                    var doc = DocGen.GetDocumentation( o );
                    var pageLink = $"{page.Name}.{o.Name}";

                    var def = $"public {PrintableType( o.ReturnType, true )} **[[{o.Name}|{pageLink}]]**( {string.Join( ", ", o.GetParameters().Select( x => PrintableParam( x.ParameterType, x.Name ) ) )} );";

                    page.WriteLine( $"* {def}" );

                    VariablePage( page.Name, o.Name, "Method", def, doc );
                }
            }

            if ( properties.Length > 0 )
            {
                page.WriteLine( "" );
                page.WriteLine( "### Properties" );

                foreach ( var o in properties )
                {
                    var pageLink = $"{page.Name}.{o.Name}";

                    var doc = DocGen.GetDocumentation( o );
                    var def = $"public {PrintableType( o.PropertyType, true )} **[[{o.Name}|{pageLink}]]** {{ get; set; }}";

                    page.WriteLine( $"* {def}" );
                    
                    VariablePage( page.Name, o.Name, "Property", def, doc );
                }
            }

            if ( fields.Length > 0 )
            {
                page.WriteLine( "" );
                page.WriteLine( "### Fields" );

                foreach ( var o in fields )
                {
                    var doc = DocGen.GetDocumentation( o );

                    var pageLink = $"{page.Name}.{o.Name}";
                    var def = $"public {PrintableType( o.FieldType, true )} **[[{o.Name}|{pageLink}]]**;";

                    page.WriteLine( $"* {def}" );

                    
                    VariablePage( page.Name, o.Name, "Property", def, doc );
                }
            }

            Pages.Add( page );
        }

        private void AddDocumentation( Page page, DocInfo classDoc )
        {
            if ( classDoc == null ) return;

            if ( !string.IsNullOrEmpty( classDoc.Summary ) )
            {
                page.WriteLine( "### Summary" );
                page.WriteLine( $"{classDoc.Summary}" );
                page.WriteLine( "" );
            }

            if ( !string.IsNullOrEmpty( classDoc.Example ) )
            {
                page.WriteLine( "### Example" );
                page.WriteLine( "```csharp" );
                page.WriteLine( $"{classDoc.Example}" );
                page.WriteLine( "```" );
                page.WriteLine( "" );
            }
        }

        private void VariablePage( string parentName, string Name, string TypeName, string Definition, DocInfo doc )
        {
            var page = new Page();
            page.Name = parentName + "." + Name;

            page.WriteLine( $"# {Name}" );

            page.WriteLine( $"## {TypeName}" );
                page.WriteLine( $"{Definition}" );
                page.WriteLine( "" );

            AddDocumentation( page, doc );

            Pages.Add( page );
        }

        private string PrintableParam( Type parameterType, string name, bool decorate = true )
        {
            if ( !decorate )
                return $"{PrintableType( parameterType, decorate )} {name}";

            return $"{PrintableType(parameterType, decorate )} {name}";
        }

        private string PrintableType( Type type, bool decorate )
        {
            var t = PrintableType( type );

            if ( decorate )
            {
                var link = t;

                if ( type.Assembly == typeof( object ).Assembly || type.IsGenericType || type.IsGenericParameter )
                {
                    link = "https://msdn.microsoft.com/en-us/library/ya5y69ds.aspx";
                }
                else if ( type.HasElementType )
                {
                    link = type.GetElementType().FullName.Replace( "+", "." );
                }
                else
                {
                    link = type.FullName.Replace( "+", "." );
                }

                return $"[[{t}|{link}]]";
            }

            return t;
        }

        private string PrintableType( Type type )
        {
            if ( type == typeof( ulong ) ) return "ulong";
            if ( type == typeof( bool ) ) return "bool";
            if ( type == typeof( string ) ) return "string";
            if ( type == typeof( void ) ) return "void";
            if ( type == typeof( int ) ) return "int";
            if ( type == typeof( uint ) ) return "uint";
            if ( type == typeof( object ) ) return "object";
            if ( type == typeof( byte ) ) return "byte";
            if ( type == typeof( float ) ) return "float";
            if ( type == typeof( double ) ) return "double";
            if ( type == typeof( byte[] ) ) return "byte[]";

            if ( type.IsGenericType )
            {
                return $"{type.Name.Trim( '`', '1', '2', '3', '4' )}&lt;{string.Join( ", ", type.GenericTypeArguments.Select( x => PrintableType( x ) ) )}&gt;";
            }

            return $"{type.Name}";
        }
    }
}
