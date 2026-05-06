using Domain;

namespace Web.Models
{
    /// <summary>
    /// AlertHelper return default values if input is empty
    /// </summary>
    public static class AlertHelper
    {
        // Method to split the input string and return default values if input is empty
        public static List<string> GetStatesOrStatuses(string input, List<string> defaultValues)
        {
            return string.IsNullOrEmpty(input)
                ? defaultValues
                : input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        // Method to get default states or statuses for both Email and SMS alerts
        public static (List<string> states, List<string> statuses) GetDefaults(
            string type,
            string stateInput,
            string statusInput)
        {
            // Define defaults based on the type (Email/SMS)
            List<string> defaultStates = type == "Email"
                ? new List<string> { }
                : new List<string> { };

            List<string> defaultStatuses = type == "Email"
                ? new List<string> { }
                : new List<string> { };

            // Return selected states and statuses
            List<string> selectedStates = GetStatesOrStatuses(stateInput, defaultStates);
            List<string> selectedStatuses = GetStatesOrStatuses(statusInput, defaultStatuses);

            return (selectedStates, selectedStatuses);
        }
    }
}
