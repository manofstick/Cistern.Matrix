using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

var summary = BenchmarkRunner.Run<Matrices>();

// BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
// 12th Gen Intel Core i5-1240P, 1 CPU, 16 logical and 12 physical cores
// .NET SDK=7.0.201
//   [Host]     : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2
//   DefaultJob : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2


// |    Method | Size |             Mean |          Error |         StdDev |           Median | Ratio | RatioSD |
// |---------- |----- |-----------------:|---------------:|---------------:|-----------------:|------:|--------:|
// |       Mlk |   16 |         3.392 us |      0.0437 us |      0.0408 us |         3.393 us |  1.00 |    0.00 |
// | FourThree |   16 |         4.026 us |      0.0113 us |      0.0100 us |         4.029 us |  1.19 |    0.02 |
// |           |      |                  |                |                |                  |       |         |
// |       Mlk |   32 |        11.014 us |      0.1216 us |      0.1078 us |        10.996 us |  1.00 |    0.00 |
// | FourThree |   32 |         8.960 us |      0.0949 us |      0.0793 us |         8.968 us |  0.81 |    0.01 |
// |           |      |                  |                |                |                  |       |         |
// |       Mlk |   64 |        69.013 us |      1.3466 us |      1.3829 us |        68.788 us |  1.00 |    0.00 |
// | FourThree |   64 |        23.693 us |      0.2925 us |      0.2736 us |        23.683 us |  0.34 |    0.01 |
// |           |      |                  |                |                |                  |       |         |
// |       Mlk |  128 |       350.462 us |     10.8868 us |     31.9292 us |       347.908 us |  1.00 |    0.00 |
// | FourThree |  128 |        96.134 us |      1.7690 us |      1.6547 us |        95.828 us |  0.28 |    0.02 |
// |           |      |                  |                |                |                  |       |         |
// |       Mlk |  256 |     1,406.915 us |     24.7263 us |     21.9192 us |     1,412.438 us |  1.00 |    0.00 |
// | FourThree |  256 |       675.618 us |     13.4667 us |     14.4092 us |       673.575 us |  0.48 |    0.01 |
// |           |      |                  |                |                |                  |       |         |
// |       Mlk |  512 |     8,179.953 us |    190.8606 us |    562.7567 us |     8,147.427 us |  1.00 |    0.00 |
// | FourThree |  512 |     4,000.303 us |     79.5126 us |    212.2352 us |     3,983.876 us |  0.49 |    0.04 |
// |           |      |                  |                |                |                  |       |         |
// |       Mlk | 1024 |    35,184.448 us |    696.3836 us |  1,307.9773 us |    34,624.214 us |  1.00 |    0.00 |
// | FourThree | 1024 |    26,022.443 us |    510.9543 us |  1,020.4309 us |    26,472.333 us |  0.74 |    0.05 |
// |           |      |                  |                |                |                  |       |         |
// |       Mlk | 2048 |   222,111.563 us |  1,513.2304 us |  1,415.4766 us |   221,981.978 us |  1.00 |    0.00 |
// | FourThree | 2048 |   237,667.036 us |  4,358.6178 us |  4,077.0538 us |   236,679.665 us |  1.07 |    0.02 |
// |           |      |                  |                |                |                  |       |         |
// |       Mlk | 4096 | 1,218,153.423 us | 20,781.6312 us | 16,224.9307 us | 1,224,100.442 us |  1.00 |    0.00 |
// | FourThree | 4096 | 1,931,779.862 us | 38,114.1631 us | 66,753.8822 us | 1,923,300.085 us |  1.59 |    0.06 |


public class Matrices
{
    [Params(16, 32, 64, 128, 256, 512, 1024, 2048, 4096)]
    public int Size;

    public int L { get; set; }
    public int M { get; set; }
    public int N { get; set; }


    double[][] A;
    double[][] B;

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

        EqualOrThrow(expected, FourThree());
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
    public double[][] FourThree() 
    {
        var result = Multiplication<double>.Multiply(A, B);
        return result;
    }
}