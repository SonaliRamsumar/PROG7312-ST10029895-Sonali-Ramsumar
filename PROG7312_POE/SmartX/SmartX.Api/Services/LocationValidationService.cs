
namespace SmartX.Api.Services
{
    public class LocationValidationService
    {
        // Each location can contain child locations.
        // This creates a hierarchy instead of a flat array.
        private sealed class LocationNode
        {
            public string Name { get; }

            public List<LocationNode> Children { get; }

            public LocationNode(string name, params LocationNode[] children)
            {
                Name = name;
                Children = children.ToList();
            }
        }

        // The facility is the root of the location hierarchy.
        // The Server Room is inside the Main Office.
        private static readonly LocationNode Facility = new LocationNode(
            "Smart-X Facility",
            new LocationNode("Greenhouse 1"),
            new LocationNode("Greenhouse 2"),
            new LocationNode("Warehouse"),
            new LocationNode(
                "Main Office",
                new LocationNode("Server Room")
            )
        );

        public bool IsValidLocation(string location)
        {
            // Empty location names cannot be valid.
            if (string.IsNullOrWhiteSpace(location))
            {
                return false;
            }

            // Start recursive searching from the facility root.
            return CheckLocationRecursive(Facility, location.Trim());
        }

        private bool CheckLocationRecursive(
            LocationNode currentNode,
            string target)
        {
            // Check whether the current node matches the target.
            if (currentNode.Name.Equals(
                target,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Recursively search every child and its descendants.
            foreach (LocationNode child in currentNode.Children)
            {
                if (CheckLocationRecursive(child, target))
                {
                    return true;
                }
            }

            // The target was not found in this branch or its children.
            return false;
        }
    }
}
