using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Product.Dal;
using Product.Dal.Entities;
using Product.Dal.Interfaces;

namespace Product.Bal;

public class TemplateService : TemplateServiceBase
{
    private const string _DefaultTemplateName = "Default";

    private readonly DBContext _db;

    public TemplateService(DBContext db, IOptions<Settings> settings) : base(settings, db)
    {
        _db = db;
    }

    public override ITemplateDefinition Get(string name, DateTime? asAtdate = null, bool getBaseMetadata = false)
    {
        // Get the current version of the template
        var template = _db.Templates.FirstOrDefault(x => x.TemplateName == name);



        if (getBaseMetadata && template != null)
        {
            // Detach the entity before removing obsolete properties so that these changes don't get committed to the DB if another method calls SaveChanges
            _db.Entry(template).State = EntityState.Detached;

            // Apply the base metadata to the template (type, min/max length, regex, plus remove obsolete and hidden properties)
            ApplyBaseMetadata(template);
        }

        return template;
    }

    public override ITemplateDefinition Get(int? templateId, DateTime? asAtdate = null, bool getDefault = true, bool getBaseMetadata = false)
    {
        // Get the default template if no Id is given
        if (templateId == null)
        {
            return Get(_DefaultTemplateName, asAtdate);
        }

        // Get the current version of the template
        var template = _db.Templates.Find(templateId);

        // Get the default template if not found
        if (getDefault && template == null)
        {
            return Get(_DefaultTemplateName, asAtdate);
        }

        if (getBaseMetadata && template != null)
        {
            // Detach the entity before removing obsolete properties so that these changes don't get committed to the DB if another method calls SaveChanges
            _db.Entry(template).State = EntityState.Detached;

            // Apply the base metadata to the template (type, min/max length, regex, plus remove obsolete and hidden properties)
            ApplyBaseMetadata(template);
        }

        return template;
    }

    public override ITemplateDefinition Get(Client client, int? templateId = null)
    {
        // TODO: Could implement a local cache since this will be accessed frequently but will not change frequently

        if (client == null)
        {
            return null;
        }

        // Get the required template
        var template = Get(templateId ?? client.TemplateId, client.ModifiedDate);

        if (template != null)
        {
            // Detach the entity before removing obsolete properties so that these changes don't get committed to the DB if another method calls SaveChanges
            // _db.Entry(template).State = EntityState.Detached;

            // Apply the base metadata to the template.
            ApplyBaseMetadata(template, client);
        }

        return template;
    }
}