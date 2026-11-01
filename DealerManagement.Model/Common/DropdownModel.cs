namespace DealerManagement.Model.Common
{
    public class DropdownModel
    {
        // Name is the option value (e.g. ID), Value is the option text (e.g. Display Name)
        // This matches ShampanERP CommonComboLoader.js: new Option(item.value, item.name)
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}

