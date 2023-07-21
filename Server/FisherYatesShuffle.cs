namespace Server;

/// <summary>
/// Implements the Fisher-Yates shuffle algorithm
/// https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
/// </summary>
public static class FisherYatesShuffle
{
    public static void Shuffle<T>(this IList<T> list, Random random)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}