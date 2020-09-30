``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.19041
AMD Ryzen 9 3950X, 1 CPU, 24 logical and 24 physical cores
.NET Core SDK=3.1.402
  [Host]     : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT
  DefaultJob : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT


```
|           Method |     Mean |    Error |   StdDev | Ratio |  Gen 0 |  Gen 1 |  Gen 2 | Allocated |
|----------------- |---------:|---------:|---------:|------:|-------:|-------:|-------:|----------:|
| &#39;50K Local Msgs&#39; | 42.42 us | 0.847 us | 2.171 us |  1.00 | 0.7200 | 0.0600 | 0.0600 |   5.56 KB |
