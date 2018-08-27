using Autodesk.Maya.OpenMaya;
using System;
using System.Drawing;

namespace Maya2Babylon
{
    internal partial class BabylonExporter
    {
        public enum LogLevel
        {
            ERROR,
            WARNING,
            MESSAGE,
            VERBOSE
        }

        // TODO - Update log level for release
        public LogLevel logLevel = LogLevel.MESSAGE;

        public event Action<int> OnExportProgressChanged;
        public event Action<string, int> OnError;
        public event Action<string, int> OnWarning;
        public event Action<string, Color, int, bool> OnMessage;
        public event Action<string, Color, int, bool> OnVerbose;

        void ReportProgressChanged(int progress)
        {
            if (OnExportProgressChanged != null)
            {
                OnExportProgressChanged(progress);
            }
        }

        void ReportProgressChanged(float progress)
        {
            ReportProgressChanged((int)progress);
        }

        void RaiseError(string error, int rank = 0)
        {
            if (OnError != null && logLevel >= LogLevel.ERROR)
            {
                OnError(error, rank);
            }
        }

        void RaiseWarning(string warning, int rank = 0)
        {
            if (OnWarning != null && logLevel >= LogLevel.WARNING)
            {
                OnWarning(warning, rank);
            }
        }

        void RaiseMessage(string message, int rank = 0, bool emphasis = false)
        {
            RaiseMessage(message, Color.Black, rank, emphasis);
        }

        void RaiseMessage(string message, Color color, int rank = 0, bool emphasis = false)
        {
            if (OnMessage != null && logLevel >= LogLevel.MESSAGE)
            {
                OnMessage(message, color, rank, emphasis);
            }
        }

        void RaiseVerbose(string message, int rank = 0, bool emphasis = false)
        {
            RaiseVerbose(message, Color.FromArgb(100, 100, 100), rank, emphasis);
        }

        void RaiseVerbose(string message, Color color, int rank = 0, bool emphasis = false)
        {
            if (OnVerbose != null && logLevel >= LogLevel.VERBOSE)
            {
                OnVerbose(message, color, rank, emphasis);
            }
        }

        void Print(MFnDependencyNode dependencyNode, int logRank, string title)
        {
            // prints
            RaiseVerbose(title, logRank);
            RaiseVerbose("Attributes", logRank + 1);
            for (uint i = 0; i < dependencyNode.attributeCount; i++)
            {
                MObject attribute = dependencyNode.attribute(i);

                if (attribute.hasFn(MFn.Type.kAttribute))
                {
                    MFnAttribute mFnAttribute = new MFnAttribute(attribute);
                    RaiseVerbose("name=" + mFnAttribute.name + "    apiType=" + attribute.apiType, logRank + 2);
                }
            }
            RaiseVerbose("Connections", logRank + 1);
            MPlugArray connections = new MPlugArray();
            try {
                dependencyNode.getConnections(connections);
                RaiseVerbose("connections.Count=" + connections.Count, logRank + 2);
                foreach (MPlug connection in connections)
                {
                    MObject source = connection.source.node;
                    if (source != null && source.hasFn(MFn.Type.kDependencyNode))
                    {
                        MFnDependencyNode node = new MFnDependencyNode(source);
                        RaiseVerbose("name=" + connection.name + "    source=" + node.name + "    source.apiType=" + source.apiType, logRank + 2);
                    }
                    else
                    {
                        RaiseVerbose("name=" + connection.name, logRank + 2);
                    }
                }
            }
            catch {}
        }
    }
}
