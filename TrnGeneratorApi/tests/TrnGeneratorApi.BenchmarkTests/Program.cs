using BenchmarkDotNet.Running;
using TrnGeneratorApi.BenchmarkTests;

var summary = BenchmarkRunner.Run<Benchmarks>();
