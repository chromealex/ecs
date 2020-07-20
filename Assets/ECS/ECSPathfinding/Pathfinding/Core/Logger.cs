using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ME.ECS.Pathfinding {

    public enum LogLevel : int {

        None = 0x0,
        Path = 0x1,
        PathMods = 0x2,
        GraphBuild = 0x4,
        Full = -1,

    }

    public static class Logger {

        public static void Log(string log) {

            var val = Application.GetStackTraceLogType(LogType.Log);
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Debug.Log(log);
            Application.SetStackTraceLogType(LogType.Log, val);
            
        }

    }

}