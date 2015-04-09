using System.Linq;

namespace GitReleaseNotes
{
    public sealed class Categories
    {
        private static readonly string[] DefaultCategories = { "bug", "enhancement", "feature" };

        public bool AllLabels { get; private set; }

        public string[] AvailableCategories { get; private set; }

        public Categories(string categories, bool allLabels)
        {
            AvailableCategories = categories == null ? DefaultCategories : DefaultCategories.Concat(categories.Split(',')).ToArray();
            AllLabels = allLabels;
        }

        public Categories(string[] categories, bool allLabels)
        {
            AvailableCategories = categories;
            AllLabels = allLabels;
        }

        public Categories()
        {
            AvailableCategories = new string[0];
        }
    }
}
