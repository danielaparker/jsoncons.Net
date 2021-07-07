﻿using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.Json;
using JsonCons.JsonPathLib;

namespace JsonCons.Examples
{
    public static class JsonPathExamples
    {
        public static void StoreExample()
        {
            string jsonString = @"
{ ""store"": {
    ""book"": [ 
      { ""category"": ""reference"",
        ""author"": ""Nigel Rees"",
        ""title"": ""Sayings of the Century"",
        ""price"": 8.95
      },
      { ""category"": ""fiction"",
        ""author"": ""Evelyn Waugh"",
        ""title"": ""Sword of Honour"",
        ""price"": 12.99
      },
      { ""category"": ""fiction"",
        ""author"": ""Herman Melville"",
        ""title"": ""Moby Dick"",
        ""isbn"": ""0-553-21311-3"",
        ""price"": 8.99
      },
      { ""category"": ""fiction"",
        ""author"": ""J. R. R. Tolkien"",
        ""title"": ""The Lord of the Rings"",
        ""isbn"": ""0-395-19395-8"",
        ""price"": 22.99
      }
    ],
    ""bicycle"": {
      ""color"": ""red"",
      ""price"": 19.95
    }
  }
}
            ";

            JsonDocument doc = null;

            try
            {
                doc = JsonDocument.Parse(jsonString);

                Console.WriteLine("(1) The authors of all books in the store");
                IReadOnlyList<JsonElement> values1 = JsonPath.Select(doc.RootElement, "$.store.book[*].author");
                foreach (var value in values1)
                {
                    Console.WriteLine(value);
                }
                Console.WriteLine();

                Console.WriteLine("(2) All authors");
                IReadOnlyList<JsonElement> values2 = JsonPath.Select(doc.RootElement, "$..author");
                foreach (var value in values2)
                {
                    Console.WriteLine(value);
                }
                Console.WriteLine();

                Console.WriteLine("(3) All things in store, which are some books and a red bicycle");
                IReadOnlyList<JsonElement> values3 = JsonPath.Select(doc.RootElement, "$.store.*");
                foreach (var value in values3)
                {
                    Console.WriteLine(value);
                }
                Console.WriteLine();

                Console.WriteLine("(4) The price of everything in the store.");
                IReadOnlyList<JsonElement> values4 = JsonPath.Select(doc.RootElement, "$.store..price");
                foreach (var value in values4)
                {
                    Console.WriteLine(value);
                }
                Console.WriteLine();

                Console.WriteLine("(5) The third book");
                IReadOnlyList<JsonElement> values5 = JsonPath.Select(doc.RootElement, "$..book[2]");
                foreach (var value in values5)
                {
                    Console.WriteLine(value);
                }
                Console.WriteLine();

                Console.WriteLine("(6) The last book");
                IReadOnlyList<JsonElement> values6 = JsonPath.Select(doc.RootElement, "$..book[-1]");
                foreach (var value in values6)
                {
                    Console.WriteLine(value);
                }
                Console.WriteLine();

                Console.WriteLine("(7) The first two books");
                IReadOnlyList<JsonElement> values7 = JsonPath.Select(doc.RootElement, "$..book[:2]");
                foreach (var value in values7)
                {
                    Console.WriteLine(value);
                }
                Console.WriteLine();

                Console.WriteLine("(8) Filter all books with isbn number");
                IReadOnlyList<JsonElement> values8 = JsonPath.Select(doc.RootElement, "$..book[?(@.isbn)]");
                foreach (var value in values8)
                {
                    Console.WriteLine(value);
                }
                Console.WriteLine();

                Console.WriteLine("(9) Filter all books cheaper than 10");
                IReadOnlyList<JsonElement> values9 = JsonPath.Select(doc.RootElement, "$..book[?(@.price<10)]");
                foreach (var value in values9)
                {
                    Console.WriteLine(value);
                }
                Console.WriteLine();

                Console.WriteLine("(10) All members of JSON value");
                IReadOnlyList<JsonElement> values10 = JsonPath.Select(doc.RootElement, "$..*");
                foreach (var value in values10)
                {
                    Console.WriteLine(value);
                }
                Console.WriteLine();
            }
            finally
            {
                if (!Object.ReferenceEquals(null, doc)) 
                    doc.Dispose();
            }

        }

        public static void SelectWithAndWithoutDuplicates()
        {
            string jsonString = @"
{
    ""books"":
    [
        {
            ""category"": ""fiction"",
            ""title"" : ""A Wild Sheep Chase"",
            ""author"" : ""Haruki Murakami"",
            ""price"" : 22.72
        },
        {
            ""category"": ""fiction"",
            ""title"" : ""The Night Watch"",
            ""author"" : ""Sergei Lukyanenko"",
            ""price"" : 23.58
        },
        {
            ""category"": ""fiction"",
            ""title"" : ""The Comedians"",
            ""author"" : ""Graham Greene"",
            ""price"" : 21.99
        },
        {
            ""category"": ""memoir"",
            ""title"" : ""The Night Watch"",
            ""author"" : ""David Atlee Phillips""
        }
    ]
}
            ";

            JsonDocument doc = null;
            JsonPathExpression path = null;

            try
            {
                doc = JsonDocument.Parse(jsonString);
                path = JsonPathExpression.Parse("$.books[3,1,1].title");

                Console.WriteLine("Allow duplicate nodes");
                IReadOnlyList<JsonPathNode> nodes = JsonPath.SelectNodes(doc.RootElement, path);
                foreach (var node in nodes)
                {
                    Console.WriteLine($"{node.Path} => {node.Value}");
                }
                Console.WriteLine();

                Console.WriteLine("Allow duplicate nodes and sort by path");
                IReadOnlyList<JsonPathNode> nodesSort = JsonPath.SelectNodes(doc.RootElement, 
                                                                             path,
                                                                             ResultOptions.Sort);
                foreach (var node in nodesSort)
                {
                    Console.WriteLine($"{node.Path} => {node.Value}");
                }
                Console.WriteLine();

                Console.WriteLine("Remove duplicate nodes");
                IReadOnlyList<JsonPathNode> nodesNoDups = JsonPath.SelectNodes(doc.RootElement, 
                                                                               path, 
                                                                               ResultOptions.NoDups);
                foreach (var node in nodesNoDups)
                {
                    Console.WriteLine($"{node.Path} => {node.Value}");
                }
                Console.WriteLine();

                Console.WriteLine("Remove duplicate nodes and sort by paths");
                IReadOnlyList<JsonPathNode> nodesNoDupsSort = JsonPath.SelectNodes(doc.RootElement, 
                                                                                   path, 
                                                                                   ResultOptions.NoDups | ResultOptions.Sort);
                foreach (var node in nodesNoDupsSort)
                {
                    Console.WriteLine($"{node.Path} => {node.Value}");
                }
                Console.WriteLine();
            }
            finally
            {
                if (!Object.ReferenceEquals(null, doc)) 
                    doc.Dispose();
                if (!Object.ReferenceEquals(null, path)) 
                    path.Dispose();
            }

        }

        public static void UsingTheParentOperator()
        {
            string jsonString = @"
[
    {
      ""author"" : ""Haruki Murakami"",
      ""title"": ""A Wild Sheep Chase"",
      ""reviews"": [{""rating"": 4, ""reviewer"": ""Nan""}]
    },
    {
      ""author"" : ""Sergei Lukyanenko"",
      ""title"": ""The Night Watch"",
      ""reviews"": [{""rating"": 5, ""reviewer"": ""Alan""},
                  {""rating"": 3,""reviewer"": ""Anne""}]
    },
    {
      ""author"" : ""Graham Greene"",
      ""title"": ""The Comedians"",
      ""reviews"": [{""rating"": 4, ""reviewer"": ""Lisa""},
                  {""rating"": 5, ""reviewer"": ""Robert""}]
    }
]
            ";

            JsonDocument doc = null;
            JsonPathExpression path = null;

            try
            {
                doc = JsonDocument.Parse(jsonString);
                path = JsonPathExpression.Parse("$[*].reviews[?(@.rating == 5)]^^");

                Console.WriteLine("Select grandparent node");
                IReadOnlyList<JsonElement> values = JsonPath.Select(doc.RootElement, path);
                foreach (var value in values)
                {
                    Console.WriteLine(value);
                }
                Console.WriteLine();
            }
            finally
            {
                if (!Object.ReferenceEquals(null, doc)) 
                    doc.Dispose();
                if (!Object.ReferenceEquals(null, path)) 
                    path.Dispose();
            }

        }

        public static void Main(string[] args)
        {
            StoreExample();
            SelectWithAndWithoutDuplicates();
            UsingTheParentOperator();
        }
    }
}