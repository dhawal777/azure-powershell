using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Tools.Common.Issues;
using Tools.Common.Loggers;
using StaticAnalysis.BreakingChangeAnalyzer;
using StaticAnalysis.DependencyAnalyzer;

namespace StaticAnalysis.IssueChecker
{
    public class IssueChecker : IStaticAnalyzer
    {
        public AnalysisLogger Logger { get; set; }

        public string Name { get; private set; }

        public IssueChecker()
        {
            Name = "Issue Checker";
        }

        public void Analyze(IEnumerable<string> scopes)
        {
            Analyze(scopes, null);
        }

        public void Analyze(IEnumerable<string> scopes, IEnumerable<string> modulesToAnalyze)
        {
            foreach (string scope in scopes)
            {
                Console.WriteLine(scope);
            }
            if (scopes.ToList().Count != 1)
            {
                throw new InvalidOperationException(string.Format("scopes for IssueChecker should be a array contains only reportsDirectory, but here is [{0}]", string.Join(", ", scopes.ToList())));
            }
            string reportsDirectory = scopes.First();

            bool hasCriticalIssue = false;
            if (IsSingleExceptionFileHasCriticalIssue<BreakingChangeIssue>(reportsDirectory, "BreakingChangeIssues.csv"))
            {
                hasCriticalIssue = true;
            }
            if (IsSingleExceptionFileHasCriticalIssue<AssemblyVersionConflict>(reportsDirectory, "AssemblyVersionConflict.csv"))
            {
                hasCriticalIssue = true;
            }
            if (IsSingleExceptionFileHasCriticalIssue<SharedAssemblyConflict>(reportsDirectory, "SharedAssemblyConflict.csv"))
            {
                hasCriticalIssue = true;
            }
            if (IsSingleExceptionFileHasCriticalIssue<MissingAssembly>(reportsDirectory, "MissingAssemblies.csv"))
            {
                hasCriticalIssue = true;
            }
            if (IsSingleExceptionFileHasCriticalIssue<ExtraAssembly>(reportsDirectory, "ExtraAssemblies.csv"))
            {
                hasCriticalIssue = true;
            }
            if (hasCriticalIssue)
            {
                throw new InvalidOperationException(string.Format("One or more errors occurred in validation. " +
                                                                  "See the analysis reports at {0} for details",
                    reportsDirectory));
            }
        }

        private bool IsSingleExceptionFileHasCriticalIssue<T>(string reportsDirectory, string exceptionFileName) where T : IReportRecord
        {
            string exceptionFilePath = Path.Combine(reportsDirectory, exceptionFileName);
            if (!File.Exists(exceptionFilePath))
            {
                return false;
            }
            bool hasError = false;
            using (var reader = new StreamReader(exceptionFilePath))
            {
                List<IReportRecord> recordList = new List<IReportRecord>();
                string header = reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    IReportRecord newRecord = (T)Activator.CreateInstance(typeof(T));
                    recordList.Add(newRecord.Parse(line));
                }
                var errorText = new StringBuilder();
                errorText.AppendLine(recordList.First().PrintHeaders());
                foreach (IReportRecord record in recordList)
                {
                    if (record.Severity < 2)
                    {
                        hasError = true;
                        errorText.AppendLine(record.FormatRecord());
                    }
                }
                if (hasError)
                {
                    Console.WriteLine("{0} Errors", exceptionFilePath);
                    Console.WriteLine(errorText.ToString());
                }
            }
            return hasError;
        }

        public void Analyze(IEnumerable<string> cmdletProbingDirs, Func<IEnumerable<string>, IEnumerable<string>> directoryFilter, Func<string, bool> cmdletFilter)
        {
            throw new NotImplementedException();
        }

        public void Analyze(IEnumerable<string> cmdletProbingDirs, Func<IEnumerable<string>, IEnumerable<string>> directoryFilter, Func<string, bool> cmdletFilter, IEnumerable<string> modulesToAnalyze)
        {
            throw new NotImplementedException();
        }

        public AnalysisReport GetAnalysisReport()
        {
            throw new NotImplementedException();
        }
    }
}
