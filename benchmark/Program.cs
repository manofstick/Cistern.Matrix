using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Cistern.Matrix;

var summary = BenchmarkRunner.Run<Matrices>();

// BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
// 12th Gen Intel Core i5-1240P, 1 CPU, 16 logical and 12 physical cores
// .NET SDK=7.0.201
//   [Host]     : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2
//   DefaultJob : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2
//
// | Method     | Size |             Mean |          Error |         StdDev |           Median | Ratio | RatioSD |
// |----------- |----- |-----------------:|---------------:|---------------:|-----------------:|------:|--------:|
// |    Mlk     |   16 |         3.417 us |      0.0578 us |      0.0541 us |         3.406 us |  1.00 |    0.00 |
// | SevenSeven |   16 |         4.062 us |      0.0336 us |      0.0298 us |         4.070 us |  1.19 |    0.02 |
// |            |      |                  |                |                |                  |       |         |
// |    Mlk     |   32 |        11.153 us |      0.1075 us |      0.0953 us |        11.141 us |  1.00 |    0.00 |
// | SevenSeven |   32 |        11.257 us |      0.1338 us |      0.1252 us |        11.210 us |  1.01 |    0.01 |
// |            |      |                  |                |                |                  |       |         |
// |    Mlk     |   64 |        68.643 us |      1.3622 us |      2.1997 us |        68.394 us |  1.00 |    0.00 |
// | SevenSeven |   64 |        30.867 us |      0.1498 us |      0.1328 us |        30.825 us |  0.44 |    0.01 |
// |            |      |                  |                |                |                  |       |         |
// |    Mlk     |  128 |       345.609 us |     13.6544 us |     40.2602 us |       344.083 us |  1.00 |    0.00 |
// | SevenSeven |  128 |       142.832 us |      2.4488 us |      2.2906 us |       142.424 us |  0.38 |    0.03 |
// |            |      |                  |                |                |                  |       |         |
// |    Mlk     |  256 |     1,403.830 us |     27.7088 us |     30.7983 us |     1,403.693 us |  1.00 |    0.00 |
// | SevenSeven |  256 |       875.966 us |     16.1944 us |     15.1483 us |       876.670 us |  0.63 |    0.02 |
// |            |      |                  |                |                |                  |       |         |
// |    Mlk     |  512 |     8,108.094 us |    184.3860 us |    540.7723 us |     8,122.888 us |  1.00 |    0.00 |
// | SevenSeven |  512 |     5,171.060 us |    143.9252 us |    424.3666 us |     5,201.311 us |  0.64 |    0.06 |
// |            |      |                  |                |                |                  |       |         |
// |    Mlk     | 1024 |    36,971.329 us |    679.7125 us |    930.3982 us |    37,034.728 us |  1.00 |    0.00 |
// | SevenSeven | 1024 |    28,264.702 us |    556.9019 us |  1,162.4616 us |    28,719.100 us |  0.75 |    0.04 |
// |            |      |                  |                |                |                  |       |         |
// |    Mlk     | 2048 |   223,099.782 us |  1,698.9674 us |  1,589.2152 us |   222,900.958 us |  1.00 |    0.00 |
// | SevenSeven | 2048 |   213,977.719 us |  2,615.6445 us |  2,318.7007 us |   213,820.806 us |  0.96 |    0.01 |
// |            |      |                  |                |                |                  |       |         |
// |    Mlk     | 4096 | 1,204,761.671 us | 13,318.2296 us | 11,121.3249 us | 1,201,503.407 us |  1.00 |    0.00 |
// | SevenSeven | 4096 | 1,688,858.114 us | 33,186.2164 us | 39,505.8300 us | 1,678,335.851 us |  1.41 |    0.04 |


public class Matrices
{
    [Params(16, 32, 64, 128, 256, 512, 1024, 2048, 4096)]
    public int Size;

    public int L { get; set; }
    public int M { get; set; }
    public int N { get; set; }


    double[][] A = null!;
    double[][] B = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        L = Size;
        M = Size;
        N = Size;

        var r = new Random(42);

        A = Utils<double>.Create(L, M, (_, _) => r.NextDouble());
        B = Utils<double>.Create(M, N, (_, _) => r.NextDouble());

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
                var diff = Math.Abs(e[j]-a[j]);
                if (diff > 0.00000000001)
                    throw new Exception($"e[j] != a[j] ({e[j]} != {a[j]})");
            }
        }
    }

    public void Validate()
    {
        var expected = Mlk();

        EqualOrThrow(expected, SevenSeven());
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
            for (j = 0; j < n; j++)
            {
                c[i][j] = cc[i,j];
            }
        }

        return c;
    }

    [Benchmark]
    public double[][] SevenSeven() 
    {
        var result = Multiplication<double>.Multiply(A, B);
        return result;
    }
}