# Cistern.Matrix

[xoofx](https://github.com/xoofx) [posted](https://mastodon.social/@xoofx/110102859105876783) a little while back that he was trying to get .net to run matrix multiplication at the performance level of Intel's
MKL level. So I thought I'd give it a crack.

Anyway it looks like he [solved it](https://mastodon.social/@xoofx/110169790460782738)! Maybe I need to go for a hike :-)

But this was a reasonable effort... Doesn't use any fancy Intrinsics, in fact could probably make this .netstandard2.0 compliant... Maybe I'll do that sometime...

| Method | Size |             Mean |          Error |         StdDev | Ratio | RatioSD |
|------- |----- |-----------------:|---------------:|---------------:|------:|--------:|
|    Mlk |   16 |         3.415 us |      0.0566 us |      0.0529 us |  1.00 |    0.00 |
| SixSix |   16 |         3.942 us |      0.0286 us |      0.0253 us |  1.15 |    0.02 |
|        |      |                  |                |                |       |         |
|    Mlk |   32 |        11.064 us |      0.1520 us |      0.1422 us |  1.00 |    0.00 |
| SixSix |   32 |         8.806 us |      0.0482 us |      0.0402 us |  0.80 |    0.01 |
|        |      |                  |                |                |       |         |
|    Mlk |   64 |        71.894 us |      1.3841 us |      1.9850 us |  1.00 |    0.00 |
| SixSix |   64 |        24.620 us |      0.2094 us |      0.1856 us |  0.34 |    0.01 |
|        |      |                  |                |                |       |         |
|    Mlk |  128 |       346.256 us |     12.9472 us |     38.1751 us |  1.00 |    0.00 |
| SixSix |  128 |       108.635 us |      0.8975 us |      0.8396 us |  0.31 |    0.03 |
|        |      |                  |                |                |       |         |
|    Mlk |  256 |     1,391.417 us |     26.7926 us |     32.9037 us |  1.00 |    0.00 |
| SixSix |  256 |       744.932 us |      9.9372 us |      9.2953 us |  0.53 |    0.01 |
|        |      |                  |                |                |       |         |
|    Mlk |  512 |     8,297.379 us |    195.4384 us |    576.2547 us |  1.00 |    0.00 |
| SixSix |  512 |     4,416.129 us |     96.3518 us |    279.5341 us |  0.54 |    0.05 |
|        |      |                  |                |                |       |         |
|    Mlk | 1024 |    38,832.652 us |  1,179.8653 us |  3,478.8595 us |  1.00 |    0.00 |
| SixSix | 1024 |    26,086.377 us |    516.9180 us |  1,268.0086 us |  0.69 |    0.08 |
|        |      |                  |                |                |       |         |
|    Mlk | 2048 |   218,414.801 us |  1,438.3900 us |  1,345.4709 us |  1.00 |    0.00 |
| SixSix | 2048 |   206,362.595 us |  2,151.8194 us |  1,907.5319 us |  0.94 |    0.01 |
|        |      |                  |                |                |       |         |
|    Mlk | 4096 | 1,286,040.549 us |  7,154.3866 us |  6,342.1774 us |  1.00 |    0.00 |
| SixSix | 4096 | 1,617,340.187 us | 31,538.9503 us | 50,929.5259 us |  1.24 |    0.04 |
