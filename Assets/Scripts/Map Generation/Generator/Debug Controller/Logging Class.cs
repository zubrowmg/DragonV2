using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoggingClass
{
    string debugLine = "";

    public LoggingClass()
    {
    }

    public void addLine(string className, string functionName, string text)
    {
        debugLine = debugLine + "\n" +
            "-----------------------------" + "\n" +
            className + " " + functionName + "\n" +
                "\t" + text + "\n" +
            "-----------------------------";
    }
}
