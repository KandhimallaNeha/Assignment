using System;
using System.Collections.Generic;
using System.Numerics; // Required for BigInteger
using Newtonsoft.Json;

class PolynomialSolver
{
    // Function to decode a value from a given base
    static BigInteger DecodeValue(string value, int baseValue)
    {
        return BigInteger.Parse(value, System.Globalization.NumberStyles.AllowHexSpecifier);
    }

    // Lagrange Interpolation to find the constant term (f(0))
    static BigInteger LagrangeInterpolationAt0(List<(BigInteger x, BigInteger y)> points)
    {
        int k = points.Count;
        BigInteger secret = 0;

        for (int i = 0; i < k; i++)
        {
            BigInteger xi = points[i].x;
            BigInteger yi = points[i].y;

            // Compute L_i(0) for Lagrange basis polynomial at x=0
            BigInteger L_i_0 = 1;
            for (int j = 0; j < k; j++)
            {
                if (i != j)
                {
                    BigInteger xj = points[j].x;
                    L_i_0 *= -xj;
                    L_i_0 /= (xi - xj);
                }
            }

            // Add to the secret (constant term)
            secret += yi * L_i_0;
        }

        return secret;
    }

    // Main solver function
    static BigInteger SolvePolynomial(string inputJson)
    {
        // Parse the input JSON
        var data = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(inputJson);

        // Ensure the parsed data is not null
        if (data == null || !data.ContainsKey("keys"))
        {
            throw new ArgumentException("Invalid JSON input or missing 'keys' section.");
        }

        // Extract n and k safely
        int n = data["keys"]["n"] ?? throw new ArgumentException("Missing 'n' in 'keys'.");
        int k = data["keys"]["k"] ?? throw new ArgumentException("Missing 'k' in 'keys'.");

        List<(BigInteger x, BigInteger y)> points = new List<(BigInteger x, BigInteger y)>();

        // Extract points from JSON data
        foreach (var key in data.Keys)
        {
            if (key != "keys")
            {
                if (data[key] == null || !data[key].ContainsKey("base") || !data[key].ContainsKey("value"))
                {
                    throw new ArgumentException($"Invalid data for key '{key}'.");
                }

                int baseValue = int.Parse(data[key]["base"].ToString());
                string encodedValue = data[key]["value"].ToString();
                BigInteger x = BigInteger.Parse(key);
                BigInteger y = DecodeValue(encodedValue, baseValue);

                points.Add((x, y));
            }
        }

        // Ensure we have at least k points
        if (points.Count < k)
        {
            throw new ArgumentException($"Insufficient points. Expected at least {k} points.");
        }

        // Use Lagrange Interpolation to calculate the constant term
        return LagrangeInterpolationAt0(points.GetRange(0, k));
    }

    static void Main()
    {
        // Example input JSON
        string inputJson = @"{
            ""keys"": {
                ""n"": 10,
                ""k"": 7
            },
            ""1"": {
                ""base"": ""6"",
                ""value"": ""13444211440455345511""
            },
            ""2"": {
                ""base"": ""15"",
                ""value"": ""aed7015a346d63""
            },
            ""3"": {
                ""base"": ""15"",
                ""value"": ""6aeeb69631c227c""
            },
            ""4"": {
                ""base"": ""16"",
                ""value"": ""e1b5e05623d881f""
            },
            ""5"": {
                ""base"": ""8"",
                ""value"": ""316034514573652620673""
            },
            ""6"": {
                ""base"": ""3"",
                ""value"": ""2122212201122002221120200210011020220200""
            },
            ""7"": {
                ""base"": ""3"",
                ""value"": ""20120221122211000100210021102001201112121""
            },
            ""8"": {
                ""base"": ""6"",
                ""value"": ""20220554335330240002224253""
            },
            ""9"": {
                ""base"": ""12"",
                ""value"": ""45153788322a1255483""
            },
            ""10"": {
                ""base"": ""7"",
                ""value"": ""1101613130313526312514143""
            }
        }";

        try
        {
            BigInteger secret = SolvePolynomial(inputJson);
            Console.WriteLine($"The secret is: {secret}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
