using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System;
using System.Linq;

namespace FruitPantry
{
    public static class WikiHelpers
    {
        private class SearchResult
        {
            public string Title { get; set; }
        }

        private class Thumbnail
        {
            public string Source { get; set; }
        }

        private struct ItemData
        {
            public int Id;
            public string Timestamp;
            public int Price;
            public int? Volume;
        }

        private struct ItemResult
        {
            public string ItemName;
            public ItemData Data;
        }

        static public async Task<int> GetItemPrice(string wikiItemName)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                using (var response = await client.GetAsync($"https://api.weirdgloop.org/exchange/history/rs/latest?name={wikiItemName}"))
                {
                    if (response == null || !response.IsSuccessStatusCode)
                    {
                        return -1;
                    }

                    string content = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(content);
                    JToken? successToken;
                    if (json.TryGetValue("success", out successToken))
                    {
                        Console.WriteLine($"Query failed: {json["error"]}");
                        return -1;
                    }

                    var child = json.Children().FirstOrDefault();
                    if (child != null)
                    {
                        child = child.Children().First();
                        if (child != null)
                        {
                            var data = child.ToObject<ItemData>();
                            return data.Price;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex}");
                return -1;
            }
            return -1;
        }

        public static async Task<KeyValuePair<string, string>?> GetWikiUrl(string itemName)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://runescape.wiki");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync($"/api.php?action=query&list=search&srsearch={itemName}&format=json");
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();

            IList<SearchResult> searchResults = new List<SearchResult>();

            try
            {
                JObject search = JObject.Parse(content);
                IList<JToken> tokens = search["query"]["search"].Children().ToList();
                foreach (JToken result in tokens)
                {
                    // JToken.ToObject is a helper method that uses JsonSerializer internally
                    SearchResult searchResult = result.ToObject<SearchResult>();
                    searchResults.Add(searchResult);
                }
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine($"Invalid json response: " + ex);
                return null;
            }

            if (!searchResults.Any())
            {
                Console.WriteLine("No result");
                return null;
            }

            // find closest result.
            int bestDistance = int.MaxValue;
            int bestIndex = 0;
            int currentIndex = 0;
            foreach (SearchResult result in searchResults)
            {
                int distance = ComputeStringDistance(result.Title, itemName);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = currentIndex;
                }
                ++currentIndex;
            }

            SearchResult first = searchResults.ElementAt(bestIndex);
            string escapedTitle = first.Title.Replace(' ', '_').Replace("'", "%27");

            string wikiUrl = $"https://runescape.wiki/w/{escapedTitle}";
            return new(escapedTitle, wikiUrl);
        }

        public static async Task<string?> GetWikiImage(string wikiPageTitle)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://runescape.wiki");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // pithumbsize - max width of image to search for. We make this huge to get the highest res image).
            string imageUrl = $"/api.php?action=query&prop=pageimages&titles=File:{wikiPageTitle}_detail.png&pithumbsize=10000&format=json";
            HttpResponseMessage response = await client.GetAsync(imageUrl);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();

            try
            {
                JObject search = JObject.Parse(content);
                var pages = search["query"]["pages"].First.Children().First();
                Thumbnail thumbnail = pages["thumbnail"].ToObject<Thumbnail>();

                return thumbnail.Source;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public static async Task<string> GetWikiItemName(string itemName)
        {
            var wikiInfo = await GetWikiUrl(itemName);
            if (wikiInfo != null)
            {
                return wikiInfo.Value.Key;
            }
            return null;
        }

        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        private static int ComputeStringDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
    }
}
