# DotNetCore-WebCompiler
.NET Core tools used to compile client web files (e.g. SCSS, TS, etc) from .NET Core projects. This tool uses the [SharpScss](https://www.nuget.org/packages/SharpScss/) as its internal implementation and wrappered it to be executable in .NET Shared Framework hosting service.

## Release Note

I'll update this part when new release is published.

- 0.1.0: Basic SCSS compilation support.

## Usage

### Add Tools Package
In order to use this tools, first use need to add this tool as a tool package into `project.json` file. Please find the `tools` section in that file and add a new item named `Sakura.AspNetCore.Tools.WebCompiler`.

### Command-Line Usage
The most common way to use this tool is running it from .NET Shared Framework Hosting Service. You may use the following command line to run this tool:
```CMD
dotnet webcompile [args] [configFiles]
```
The `configFiles` are paths of one or more configuration files used to specify all input files, output file and options for web compilation. If no configuration files are specified, this tools will try to use a file named `compileconfig.json` in the current working directory as the configuration file. The detailed format for configuration files will be shown in the next section.  

The `args` are extra arguments used to control the work mode of this tools. Currently the following args are supported:
Value|Description
-----|-----------
`-?` or `-h` or `--help`|Show the command line help inforamtion
`-b` or `--build`|Compile all supported file according to the settings in configuration files
`-c` or `--clean`|Clean all previous built files according to the settings in configuration files

If no argument is set, this tool will use `--build` as its default work mode.

### Configuration File Format
Each configuration file is a json file that contains an array of work item definitions. A sample for this file is like following:
```JS
[
  {
    "inputFiles": [ "wwwroot/site.scss" ], // The input file to be compiled, you can use multiple files as input, and they will be combined before compilation. You may also use globbing pattern in input files, e.g. use "wwwroot/**/*.scss" to get all SCSS files in wwwroot and its sub directory. 
    "outputFileName": "wwwroot/site.css", // The path of the final output file.
    "type": "SCSS", //The compiler type, currently only SCSS and SASS are supported. You can omit this settings, and this tool will try to infer the comipler accroding to the first input file's name.
    "options": { //Addtional options, currently as the same as ScssOptions class in SharpScss package.
    }
  },
  {
    // Another work items. One file can contains arbitray numbers of work items, and each will be handled invididually.
  }
]
```

### Binding with Building Events
In .NET Core applications, you can use project scripts to automatically. A common usage for this tool is compiling files after you build your project, to do so, you may edit `project.json` file and add a new line in `scripts` section like:
```JS
{
  "scripts": {
    "postcompile": ["dotnet webcompile"]
  }
}
```
You may also put this command on any other buliding events. Note: If you are using bundling tools (e.g. `BundlerMinifier.Core`), please make sure the compiling command is running before bundling command.

## Feedback and Contributing
If you have any idea or advise, pleas feel free to open issue :-)
