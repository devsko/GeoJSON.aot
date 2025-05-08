// Copyright © devsko 2025
// Licensed under the MIT license.

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

BenchmarkSwitcher switcher = new(typeof(Program).Assembly);

#if DEBUG
switcher.Run(args, new DebugInProcessConfig());
#else
switcher.Run(args);
#endif
