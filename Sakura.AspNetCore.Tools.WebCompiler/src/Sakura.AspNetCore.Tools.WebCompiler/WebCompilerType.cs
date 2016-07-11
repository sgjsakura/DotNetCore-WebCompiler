namespace Sakura.AspNetCore.Tools.WebCompiler
{
    /// <summary>
    /// Define the compiler type.
    /// </summary>
    public enum WebCompilerType
    {
        /// <summary>
        /// Infer the comipler type according to the file extension.
        /// </summary>
        Auto = 0,
        /// <summary>
        /// SASS/SCSS compiler.
        /// </summary>
        Scss,
    }
}