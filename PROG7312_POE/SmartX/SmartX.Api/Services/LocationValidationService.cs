namespace SmartX.Api.Services
{
    public class LocationValidationService
    {
        public bool IsValidLocation(string location)
        {
            string[] allowedLocations =
            {
                "Greenhouse 1",
                "Greenhouse 2",
                "Warehouse",
                "Server Room",
                "Main Office"
            };

            return CheckLocationRecursive(allowedLocations, location, 0);
        }

        private bool CheckLocationRecursive(
            string[] locations,
            string target,
            int index)
        {
            // Stops when we reach the end of the array.
            if (index >= locations.Length)
            {
                return false;
            }

            // Returns true if the current location matches.
            if (locations[index].Equals(
                target,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Checks the next item in the array.
            return CheckLocationRecursive(
                locations,
                target,
                index + 1);
        }
    }
}