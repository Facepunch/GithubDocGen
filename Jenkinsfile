node( 'vs2017' )
{
	stage 'Checkout'
		checkout scm

	stage 'Build'
		bat 'nuget restore DocumentationGenerator.sln'
		bat "\"${tool 'MSBuild'}\" DocumentationGenerator.csproj /p:Configuration=Release /p:ProductVersion=1.0.0.${env.BUILD_NUMBER}"
		bat "\"${tool 'MSBuild'}\" DocumentationGenerator.csproj /p:Configuration=Debug /p:ProductVersion=1.0.0.${env.BUILD_NUMBER}"
		
	stage 'ilmerge'
		bat "ilmerge.exe /targetplatform:v4,C:/Windows/Microsoft.NET/Framework/v4.0.30319 /out:bin/DocumentationGenerator.exe bin/Release/DocumentationGenerator.exe bin/Release/Newtonsoft.Json.dll bin/Release/CommandLine.dll"

	stage 'Archive'
		archiveArtifacts artifacts: 'bin/DocumentationGenerator.*'
}