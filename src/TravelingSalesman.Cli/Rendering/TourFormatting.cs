using TravelingSalesman.Core;

namespace TravelingSalesman.Cli.Rendering;

internal static class TourFormatting
{
    public const string Arrow = " -> ";

    extension(Tour tour)
    {
        public string FormatRoute(int? maximumStops = null)
        {
            if (tour.Cities.Count == 0)
            {
                return "(empty)";
            }

            var limit = Math.Clamp(maximumStops ?? tour.Cities.Count, 1, tour.Cities.Count);
            var names = tour.Cities.Take(limit).Select(static city => city.Name);
            var hidden = tour.Cities.Count - limit;
            var suffix = hidden > 0 ? $"{Arrow}... ({hidden} more){Arrow}{tour.Cities[0].Name}" : $"{Arrow}{tour.Cities[0].Name}";
            return string.Join(Arrow, names) + suffix;
        }
    }
}
