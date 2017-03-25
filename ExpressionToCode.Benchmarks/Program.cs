﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ExpressionToCodeLib;
using JetBrains.Annotations;

namespace ExpressionToCode.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<BenchmarkCompile>();
            BenchmarkRunner.Run<BenchmarkPAssert>();
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class BenchmarkCompile
    {
        readonly Expression<Func<bool>> testExpr = GetExpression();

        static Expression<Func<bool>> GetExpression()
        {
            var x = 1;
            return () => x == 1;
        }

        [Benchmark]
        public void Compile() { testExpr.Compile(); }

        [Benchmark]
        public void Emit() { ExpressionTreeCompilers.OptimizedExpressionCompiler.Compile(testExpr); }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class BenchmarkPAssert
    {
        static readonly ExpressionToCodeConfiguration
            baseLineConfiguration = ExpressionToCodeConfiguration.DefaultCodeGenConfiguration,
            withOptimizationConfiguration = ExpressionToCodeConfiguration.DefaultCodeGenConfiguration.WithCompiler(ExpressionTreeCompilers.OptimizedExpressionCompiler);

        [Benchmark]
        public void PAssertWithCompile()
        {
            var x = 1;
            string s = "Test";
            baseLineConfiguration.Assert(() => x == 1 && (s.Contains("S") || s.Contains("s")));
        }

        [Benchmark]
        public void PAssertWithEmit()
        {
            var x = 1;
            string s = "Test";
            withOptimizationConfiguration.Assert(() => x == 1 && (s.Contains("S") || s.Contains("s")));
        }

        [Benchmark]
        public void BaseLinePlainLambdaExec()
        {
            var x = 1;
            string s = "Test";
            BaseLineAssert(() => x == 1 && (s.Contains("S") || s.Contains("s")));
        }

        static void BaseLineAssert(Func<bool> assertion)
        {
            if (!assertion()) {
                throw new Exception();
            }
        }
    }
}
