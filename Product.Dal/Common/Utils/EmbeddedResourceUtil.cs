namespace Product.Dal.Common.Utils;

public class EmbeddedResourceUtil
{
    /// <summary>
    /// Reads the contents of an embedded resouce as a string.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly containing the resource.</param>
    /// <param name="embeddedResourceName">The relative resource namespace.</param>
    /// <returns></returns>
    public static string ReadAsString(string assemblyName, string embeddedResourceName)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var resourceAssembly = assemblies.Where(x => x.FullName.StartsWith(assemblyName)).SingleOrDefault();

        using (Stream manifestResourceStream = resourceAssembly.GetManifestResourceStream($"{assemblyName}.{embeddedResourceName}"))
        {
            using (StreamReader streamReader = new StreamReader(manifestResourceStream))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}
