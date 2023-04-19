using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Cistern.Matrix;

#if truex
    var m = new Matrices();
    m.L = 128;
    m.M = 128; //10;
    m.N = 128;
    m.GlobalSetup();
    return;
#endif

var summary = BenchmarkRunner.Run<Matrices>();

// BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
// 12th Gen Intel Core i5-1240P, 1 CPU, 16 logical and 12 physical cores
// .NET SDK=7.0.201
//   [Host]     : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2
//   DefaultJob : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2


// | Method | Size |             Mean |          Error |         StdDev |           Median | Ratio | RatioSD |
// |------- |----- |-----------------:|---------------:|---------------:|-----------------:|------:|--------:|
// |    Mlk |   16 |         3.371 us |      0.0628 us |      0.0645 us |         3.387 us |  1.00 |    0.00 |
// | Stripe |   16 |         3.862 us |      0.0179 us |      0.0159 us |         3.863 us |  1.15 |    0.03 |
// |        |      |                  |                |                |                  |       |         |
// |    Mlk |   32 |        11.149 us |      0.1334 us |      0.1248 us |        11.153 us |  1.00 |    0.00 |
// | Stripe |   32 |        11.745 us |      0.1128 us |      0.1000 us |        11.742 us |  1.05 |    0.01 |
// |        |      |                  |                |                |                  |       |         |
// |    Mlk |   64 |        75.518 us |      1.4976 us |      2.9209 us |        75.272 us |  1.00 |    0.00 |
// | Stripe |   64 |        39.833 us |      0.2575 us |      0.2282 us |        39.865 us |  0.51 |    0.01 |
// |        |      |                  |                |                |                  |       |         |
// |    Mlk |  128 |       340.640 us |     11.6188 us |     34.2584 us |       332.927 us |  1.00 |    0.00 |
// | Stripe |  128 |       185.521 us |      1.6166 us |      1.5122 us |       185.612 us |  0.52 |    0.05 |
// |        |      |                  |                |                |                  |       |         |
// |    Mlk |  256 |     1,484.028 us |     29.4958 us |     44.1480 us |     1,482.126 us |  1.00 |    0.00 |
// | Stripe |  256 |       874.801 us |     16.6290 us |     17.0767 us |       868.025 us |  0.59 |    0.02 |
// |        |      |                  |                |                |                  |       |         |
// |    Mlk |  512 |     8,955.867 us |    275.7806 us |    813.1454 us |     9,103.781 us |  1.00 |    0.00 |
// | Stripe |  512 |     5,129.479 us |    101.0370 us |    154.2941 us |     5,162.773 us |  0.56 |    0.06 |
// |        |      |                  |                |                |                  |       |         |
// |    Mlk | 1024 |    40,455.683 us |  2,100.9309 us |  6,194.6421 us |    37,321.068 us |  1.00 |    0.00 |
// | Stripe | 1024 |    27,854.621 us |    494.0827 us |    462.1653 us |    27,780.364 us |  0.65 |    0.09 |
// |        |      |                  |                |                |                  |       |         |
// |    Mlk | 2048 |   222,365.581 us |  1,417.2391 us |  1,183.4588 us |   222,019.240 us |  1.00 |    0.00 |
// | Stripe | 2048 |   172,401.300 us |  1,877.9595 us |  1,756.6445 us |   172,343.937 us |  0.78 |    0.01 |
// |        |      |                  |                |                |                  |       |         |
// |    Mlk | 4096 | 1,354,957.998 us | 17,940.0714 us | 16,781.1539 us | 1,345,971.913 us |  1.00 |    0.00 |
// | Stripe | 4096 | 1,355,375.920 us | 26,431.9635 us | 35,285.9120 us | 1,356,102.329 us |  0.99 |    0.03 |



public class Matrices
{
    [Params(16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192)]
    public int Size;

    public int L { get; set; }
    public int M { get; set; }
    public int N { get; set; }


    double[][] A = null!;
    double[][] B = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
#if true
        L = Size;
        M = Size;
        N = Size;

        var r = new Random(42);
        A = Utils<double>.Create(L, M, (_, _) => r.NextDouble());
        B = Utils<double>.Create(M, N, (_, _) => r.NextDouble());
#else
        A = Utils<double>.Create(L, M, (r, c) => r*M+c);
        B = Utils<double>.Create(M, N, (r, c) => -(r*N+c));
#endif

        Validate();
    }

    public static void EqualOrThrow(double[][] expected, double[][] actual)
    {
        if (expected.Length != actual.Length)
            throw new Exception("expected.Length != actual.Length");

        for(var i=0; i < expected.Length; ++i)
        {
            var e = expected[i];
            var a = actual[i];

            if (e.Length != a.Length)
                throw new Exception("e.Length != a.Length");

            for(var j=0; j < e.Length; ++j)
            {
                var diff = Math.Abs((e[j]-a[j]) / a[j]);
                if (diff > 0.0000000000001)
                    throw new Exception($"e[j] != a[j] ({e[j]} != {a[j]})");
            }
        }
    }

    public void Validate()
    {
        var expected = Mlk();

        EqualOrThrow(expected, EightEight());
        EqualOrThrow(expected, Stripe());
    }

    [Benchmark(Baseline=true)]
    public double[][] Mlk() 
    {
        int i,j;

        var m = A.Length;
        var n = A[0].Length;
        var p = B.Length;
        var q = B[0].Length;

        var aa = new MKLNET.matrix(m, n, (x, y) => A[x][y]);
        var bb = new MKLNET.matrix(p, q, (x, y) => B[x][y]);

        var cc = (aa * bb).Evaluate();

        double[][] c = new double[m][];
        for (i=0; i<m; ++i)
            c[i] = new double[q];

        for (i = 0; i < m; i++)
        {
            for (j = 0; j < q; j++)
            {
                c[i][j] = cc[i,j];
            }
        }

        return c;
    }


    //[Benchmark]
    public double[][] EightEight() 
    {
        var result = Multiplication<double>.Multiply(A, B);

        return result;
    }

    [Benchmark]
    public double[][] Stripe() 
    {
        var result = MultiplicationByStripe<double>.Multiply(A, B);

        return result;
    }
}