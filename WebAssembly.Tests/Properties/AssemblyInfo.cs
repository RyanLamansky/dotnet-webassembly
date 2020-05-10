using Microsoft.VisualStudio.TestTools.UnitTesting;

// An apparent bug in MSTest causes occasional random test failures when parallelism is enabled.
#if DEBUG // Only enable parallelism for debug builds to prevent it from stopping CI builds.
[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
#endif