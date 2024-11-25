using System;

namespace Product.Web.Models.Configuration;

public class ConfigurationRequest
{
    public string ConfigInfo { get; set; }

    public string ConfigType { get; set; }

    public string ConfigName { get; set; }

    public bool IsActive { get; set; }
}
