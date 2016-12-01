using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace DocumentationGenerator
{
    class Program
    {
        class Options
        {
            [Option( 'l', "library", Required = true, HelpText = "Libraries to be parsed" )]
            public string Library { get; set; }

            [Option( 'o', "output", Required = true, HelpText = "Output folder" )]
            public string TargetFolder { get; set; }

            [Option( 'd', "delete", Required = false, HelpText = "Clear everything" )]
            public bool Erase { get; set; }
        }

        static void Main( string[] args )
        {
            var options = CommandLine.Parser.Default.ParseArguments<Options>( args );

            if ( options.Errors.Any() )
                return;

            if ( !System.IO.Directory.Exists( options.Value.TargetFolder ) )
                System.IO.Directory.CreateDirectory( options.Value.TargetFolder );

            if ( options.Value.Erase )
            {
                foreach ( var f in System.IO.Directory.EnumerateFiles( options.Value.TargetFolder ) )
                {
                    System.IO.File.Delete( f );
                }
            }

            var dg = new DocGen();

            dg.AddLibrary( options.Value.Library );
            dg.Process();

            var output = new GithubOutput();
            output.DocGen = dg;
            output.ToFolder( new System.IO.DirectoryInfo( options.Value.TargetFolder ) );
        }
    }
}
