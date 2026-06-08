using Inventory_Management_System.Models;
using Inventory_Management_System.Models.DTOs;

namespace Inventory_Management_System.Services
{
    public class CustomFieldMappingService
    {
        public static string GetItemPropertyForFieldName(string? fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                return string.Empty;

            return fieldName switch
            {
                "custom_string1_value" => nameof(Item.CustomString1Value),
                "custom_string2_value" => nameof(Item.CustomString2Value),
                "custom_string3_value" => nameof(Item.CustomString3Value),
                "custom_text1_value" => nameof(Item.CustomText1Value),
                "custom_text2_value" => nameof(Item.CustomText2Value),
                "custom_text3_value" => nameof(Item.CustomText3Value),
                "custom_number1_value" => nameof(Item.CustomNumber1Value),
                "custom_number2_value" => nameof(Item.CustomNumber2Value),
                "custom_number3_value" => nameof(Item.CustomNumber3Value),
                "custom_bool1_value" => nameof(Item.CustomBool1Value),
                "custom_bool2_value" => nameof(Item.CustomBool2Value),
                "custom_bool3_value" => nameof(Item.CustomBool3Value),
                "custom_link1_value" => nameof(Item.CustomLink1Value),
                "custom_link2_value" => nameof(Item.CustomLink2Value),
                "custom_link3_value" => nameof(Item.CustomLink3Value),
                _ => string.Empty
            };
        }

        public static string GenerateFieldName(CustomFieldType type, int slotNumber)
        {
            if (slotNumber < 1 || slotNumber > 3)
                throw new ArgumentException("Slot number must be between 1 and 3", nameof(slotNumber));

            return type switch
            {
                CustomFieldType.SingleLineText => $"custom_string{slotNumber}_value",
                CustomFieldType.MultiLineText => $"custom_text{slotNumber}_value",
                CustomFieldType.Numeric => $"custom_number{slotNumber}_value",
                CustomFieldType.Boolean => $"custom_bool{slotNumber}_value",
                CustomFieldType.Link => $"custom_link{slotNumber}_value",
                _ => throw new ArgumentException($"Unknown field type: {type}", nameof(type))
            };
        }

        public static (CustomFieldType Type, int SlotNumber) ParseFieldName(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentException("Field name cannot be empty", nameof(fieldName));

            var parts = fieldName.Split('_');
            if (parts.Length != 3 || parts[0] != "custom" || parts[2] != "value")
                throw new ArgumentException($"Invalid field name format: {fieldName}", nameof(fieldName));

            var typeAndNumber = parts[1];

            if (!int.TryParse(typeAndNumber[^1..], out var slotNumber) || slotNumber < 1 || slotNumber > 3)
                throw new ArgumentException($"Invalid slot number in field name: {fieldName}", nameof(fieldName));

            var typeStr = typeAndNumber[..^1];
            var type = typeStr switch
            {
                "string" => CustomFieldType.SingleLineText,
                "text" => CustomFieldType.MultiLineText,
                "number" => CustomFieldType.Numeric,
                "bool" => CustomFieldType.Boolean,
                "link" => CustomFieldType.Link,
                _ => throw new ArgumentException($"Unknown field type in name: {fieldName}", nameof(fieldName))
            };

            return (type, slotNumber);
        }

        public static List<string> GetAllPossibleFieldNames()
        {
            var names = new List<string>();
            var types = Enum.GetValues(typeof(CustomFieldType)).Cast<CustomFieldType>();

            foreach (var type in types)
            {
                for (int i = 1; i <= 3; i++)
                {
                    names.Add(GenerateFieldName(type, i));
                }
            }

            return names;
        }

        public static string GetFieldDisplayName(string fieldName)
        {
            try
            {
                var (type, slot) = ParseFieldName(fieldName);
                var typeStr = type switch
                {
                    CustomFieldType.SingleLineText => "String",
                    CustomFieldType.MultiLineText => "Text",
                    CustomFieldType.Numeric => "Number",
                    CustomFieldType.Boolean => "Boolean",
                    CustomFieldType.Link => "Link",
                    _ => "Unknown"
                };
                return $"{typeStr} Field {slot}";
            }
            catch
            {
                return fieldName;
            }
        }

        public static List<CustomPropertyInfo> GetCustomItemProperties()
        {
            var properties = new List<CustomPropertyInfo>
            {
                new(nameof(Item.CustomString1Value), "String 1", CustomFieldType.SingleLineText),
                new(nameof(Item.CustomString2Value), "String 2", CustomFieldType.SingleLineText),
                new(nameof(Item.CustomString3Value), "String 3", CustomFieldType.SingleLineText),
                new(nameof(Item.CustomText1Value), "Text 1", CustomFieldType.MultiLineText),
                new(nameof(Item.CustomText2Value), "Text 2", CustomFieldType.MultiLineText),
                new(nameof(Item.CustomText3Value), "Text 3", CustomFieldType.MultiLineText),
                new(nameof(Item.CustomNumber1Value), "Number 1", CustomFieldType.Numeric),
                new(nameof(Item.CustomNumber2Value), "Number 2", CustomFieldType.Numeric),
                new(nameof(Item.CustomNumber3Value), "Number 3", CustomFieldType.Numeric),
                new(nameof(Item.CustomBool1Value), "Boolean 1", CustomFieldType.Boolean),
                new(nameof(Item.CustomBool2Value), "Boolean 2", CustomFieldType.Boolean),
                new(nameof(Item.CustomBool3Value), "Boolean 3", CustomFieldType.Boolean),
                new(nameof(Item.CustomLink1Value), "Link 1", CustomFieldType.Link),
                new(nameof(Item.CustomLink2Value), "Link 2", CustomFieldType.Link),
                new(nameof(Item.CustomLink3Value), "Link 3", CustomFieldType.Link),
            };

            return properties;
        }
    }

    public record CustomPropertyInfo(string PropertyName, string DisplayName, CustomFieldType FieldType);
}

