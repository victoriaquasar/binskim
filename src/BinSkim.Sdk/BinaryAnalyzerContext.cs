﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis.BinaryParsers;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.CodeAnalysis.IL.Sdk
{
    public class BinaryAnalyzerContext : AnalyzeContextBase
    {
        private IBinary iBinary;

        static BinaryAnalyzerContext()
        {
        }

        public IBinary Binary
        {
            get
            {
                this.iBinary ??=
                    BinaryTargetManager.GetBinaryFromFile(this.CurrentTarget.Uri,
                                                          this.SymbolPath,
                                                          this.LocalSymbolDirectories,
                                                          this.TracePdbLoads,
                                                          this.ComprehensiveBinaryParsing);

                return this.iBinary;
            }
            set => this.iBinary = value;
        }

        public override bool IsValidAnalysisTarget
        {
            get => this.Binary?.Valid == true;
        }

        public string LocalSymbolDirectories { get; set; }

        public bool ComprehensiveBinaryParsing
        {
            get { return this.Policy?.GetProperty(BinaryParsersProperties.ComprehensiveBinaryParsing) == true; }
            set { this.Policy.SetProperty(BinaryParsersProperties.ComprehensiveBinaryParsing, value); }
        }

        public bool TracePdbLoads { get; set; }

        public string SymbolPath { get; set; }

        public override IAnalysisLogger Logger { get; set; }

        public override ReportingDescriptor Rule { get; set; }

        public override HashData Hashes { get; set; }

        public override string MimeType
        {
            get => Sarif.Writers.MimeType.Binary;
            set => throw new InvalidOperationException();
        }

        public override RuntimeConditions RuntimeErrors { get; set; }

        public override bool AnalysisComplete { get; set; }

        public CompilerDataLogger CompilerDataLogger
        {
            get
            {
                return this.Policy != null
                    ? this.Policy.GetProperty(SharedCompilerDataLoggerProperty)
                    : null;
            }
            set { this.Policy.SetProperty(SharedCompilerDataLoggerProperty, value); }
        }

        public bool IgnorePdbLoadError { get; set; }

        internal bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                this.iBinary?.Dispose();
                this.iBinary = null;

                (this.Logger as IDisposable)?.Dispose();
                this.Logger = null;

                if (this.CompilerDataLogger?.OwningContextHashCode == this.GetHashCode())
                {
                    this.CompilerDataLogger.Dispose();
                }

                this.disposed = true;
            }
        }

        public static PerLanguageOption<CompilerDataLogger> SharedCompilerDataLoggerProperty { get; } =
            new PerLanguageOption<CompilerDataLogger>(
                "CompilerTelemetry", nameof(SharedCompilerDataLoggerProperty), defaultValue: () => null,
                "A shared CompilerDataLogger instance that will be passed to all skimmers.");

        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
        }
    }
}
