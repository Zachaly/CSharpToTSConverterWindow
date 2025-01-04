using CSharpToTSConverterWindow.Model;
using System.Text;

namespace CSharpToTSConverterWindow
{
    public static class Converter
    {
        public static (string Name, List<PropertyDescription> Properties, string[] parentTypes) GetNameAndProperties(string[] lines)
        {
            int currentLine;

            for (currentLine = 0; currentLine < lines.Length; currentLine++)
            {
                if (lines[currentLine].Contains("class") || lines[currentLine].Contains("record"))
                {
                    break;
                }
            }

            var nameLine = lines[currentLine];
            var nameLineWords = nameLine.Split(' ');

            string name = "";
            for (int i = 1; i < nameLineWords.Length; i++)
            {
                if (nameLineWords[i - 1] == "class" || nameLineWords[i - 1] == "record")
                {
                    name = nameLineWords[i];
                }
            }

            string[] parentTypes = { };

            if(nameLineWords.Contains(":"))
            {
                parentTypes = nameLine[nameLine.IndexOf(":")..]
                    .Remove(0)
                    .Trim()
                    .Split(' ')
                    .Where(x => !string.IsNullOrEmpty(x)).ToArray();
            }

            var properties = new List<PropertyDescription>();
            currentLine++;
            foreach (var line in lines[currentLine..])
            {
                if (line.Contains("get"))
                {
                    var words = line.Trim().Split(" ");
                    var typeName = words[1];
                    var propertyName = words[2];
                    var isNullable = typeName.Contains('?');

                    if (isNullable)
                    {
                        typeName = typeName.Remove(typeName.Length - 1);
                    }

                    properties.Add(new PropertyDescription(propertyName, typeName, isNullable));
                }
            }

            return (name, properties, parentTypes);
        }
        public static List<PropertyDescription> ConvertCSharpPropertiesToTypeScript(List<PropertyDescription> cSharpProperties)
        {
            var tsProps = new List<PropertyDescription>();

            foreach (var prop in cSharpProperties)
            {
                var name = $"{prop.Name.Substring(0, 1).ToLower()}{prop.Name.Substring(1)}";
                var type = ConvertType(prop.Type);

                tsProps.Add(new PropertyDescription(name, type, prop.IsNullable));
            }

            return tsProps;
        }
        public static string GenerateTypeScript(IEnumerable<PropertyDescription> properties, string name, string[] parentTypes)
        {
            var builder = new StringBuilder("");

            builder.Append($"export default interface {name}");

            if(parentTypes.Any())
            {
                builder.Append(" extends ");
                builder.Append(string.Join(',', parentTypes));
                builder.Append(" {");
            }
            else
            {
                builder.Append(" {");
            }
            builder.AppendLine();

            int index = 0;
            var maxIndex = properties.Count();

            foreach (var prop in properties)
            {
                var nullable = prop.IsNullable ? "?" : string.Empty;
                builder.Append($"\t{prop.Name}{nullable}: {prop.Type}");
                index++;
                if(index != maxIndex)
                {
                    builder.Append(',');
                }
                builder.AppendLine();
            }

            builder.AppendLine("}");

            return builder.ToString();
        }

        private static string ConvertType(string type)
        {
            string res = type;

            if(Constants.CollectionTypes.Any(x => type.Contains(x)))
            {
                var startIndex = type.IndexOf('<') + 1;
                var innerType = type[startIndex..type.IndexOf('>')];
                innerType = ConvertType(innerType);

                return $"{innerType}[]";
            }

            if(Constants.NumberTypes.Contains(type))
            {
                res = "number";
            }

            if(Constants.StringTypes.Contains(type))
            {
                res = "string";
            }

            return res;
        }
    }
}
