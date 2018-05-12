using System;
using System.Diagnostics;
using UnityEngine;

public class DebugLoggingTimer {

	private String tag;
	private long previousCheck;
    private Stopwatch stopwatch;

    public void begin(String tag) {
		this.tag = tag;
		stopwatch = Stopwatch.StartNew();
        UnityEngine.Debug.Log("BEGIN " + tag);
		previousCheck = stopwatch.ElapsedMilliseconds;
	}

	public void end() {
		UnityEngine.Debug.Log("END " + tag + " : " + stopwatch.ElapsedMilliseconds);
		stopwatch.Stop();
	}

	public void logEventComplete(String stage) {
		long time = stopwatch.ElapsedMilliseconds;
		UnityEngine.Debug.Log(">> completed " + stage + " : " + (time - previousCheck));
		previousCheck = time;
	}
}
