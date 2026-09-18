using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Tarotro.Editor.Build {
    public static class EditModeTestRunner {
        public static bool Run() {
            Debug.Log("[Build] Running EditMode tests...");

            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            var listener = new BuildTestListener();
            api.RegisterCallbacks(listener);

            var filter = new Filter {
                testMode = TestMode.EditMode
            };

            api.Execute(new ExecutionSettings(filter));

            while (!listener.IsFinished) {
                System.Threading.Thread.Sleep(100);
            }

            api.UnregisterCallbacks(listener);

            if (listener.FailedTests.Count > 0) {
                Debug.LogError($"[Build] {listener.FailedTests.Count} test(s) failed:");
                foreach (var name in listener.FailedTests)
                    Debug.LogError($"[Build]   FAIL: {name}");
                return false;
            }

            Debug.Log($"[Build] All {listener.TotalCount} test(s) passed.");
            return true;
        }

        private class BuildTestListener : ICallbacks {
            public bool IsFinished { get; private set; }
            public int TotalCount { get; private set; }
            public List<string> FailedTests { get; } = new();

            public void RunStarted(ITestAdaptor testsToRun) { }

            public void RunFinished(ITestResultAdaptor result) {
                IsFinished = true;
            }

            public void TestStarted(ITestAdaptor test) { }

            public void TestFinished(ITestResultAdaptor result) {
                if (!result.HasChildren) {
                    TotalCount++;
                    if (result.TestStatus == TestStatus.Failed)
                        FailedTests.Add(result.FullName);
                }
            }
        }
    }
}
