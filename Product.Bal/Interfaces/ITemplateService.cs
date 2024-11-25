using Product.Dal.Entities;
using Product.Dal.Interfaces;

namespace Product.Bal.Interfaces;

public interface ITemplateService
{
    /// <summary>
    /// Get the template by template name.
    /// </summary>
    /// <param name="name">Template name.</param>
    /// <param name="asAtdate">Optional as-at date.</param>
    /// <param name="getBaseMetadata">Optionally overlay types from the Profile class.</param>
    /// <returns></returns>
    ITemplateDefinition Get(string name, DateTime? asAtdate = null, bool getBaseMetadata = false);

    /// <summary>
    /// Get the template by template Id.
    /// </summary>
    /// <param name="templateId">Template Id.</param>
    /// <param name="asAtdate">Optional as-at date.</param>
    /// <param name="getDefault">Fallback to the default template if not found.</param>
    /// <param name="getBaseMetadata">Optionally overlay types from the Profile class.</param>
    /// <returns></returns>
    ITemplateDefinition Get(int? templateId, DateTime? asAtdate = null, bool getDefault = true, bool getBaseMetadata = false);

    /// <summary>
    /// Get the template for a Client.
    /// </summary>
    /// <param name="client">Client</param>
    /// <returns></returns>
    ITemplateDefinition Get(Client client, int? templateId = null);
}
