using Product.Dal.Common.Utils;
using Product.Dal.Entities;

namespace Product.Dal.Common.Data.Seed;

public static class TemplateSeedData
    {
        public static List<Template> Get()
        {
            return new List<Template>
            {
                
                new Template
                {
                    TemplateName = "Default",
                    Description = "Default template",
                    TemplateJson = ReadJsonAsString("template-default.json")
                }
            };
        }

        private static string ReadJsonAsString(string fileName)
        {
            return EmbeddedResourceUtil.ReadAsString("Product.Dal", $"Common.Data.Json.{fileName}");
        }
    }
