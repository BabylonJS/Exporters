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

        public void ReportProgressChanged(int progress)
        {
            if (OnExportProgressChanged != null)
            {
                OnExportProgressChanged(progress);
            }
        }

        public void ReportProgressChanged(float progress)
        {
            ReportProgressChanged((int)progress);
        }

        public void RaiseError(string error, int rank = 0)
        {
            if (OnError != null && logLevel >= LogLevel.ERROR)
            {
                OnError(error, rank);
            }
        }

        public void RaiseWarning(string warning, int rank = 0)
        {
            if (OnWarning != null && logLevel >= LogLevel.WARNING)
            {
                OnWarning(warning, rank);
            }
        }

        public void RaiseMessage(string message, int rank = 0, bool emphasis = false)
        {
            RaiseMessage(message, Color.Black, rank, emphasis);
        }

        public void RaiseMessage(string message, Color color, int rank = 0, bool emphasis = false)
        {
            if (OnMessage != null && logLevel >= LogLevel.MESSAGE)
            {
                OnMessage(message, color, rank, emphasis);
            }
        }

        public void RaiseVerbose(string message, int rank = 0, bool emphasis = false)
        {
            RaiseVerbose(message, Color.FromArgb(100, 100, 100), rank, emphasis);
        }

        public void RaiseVerbose(string message, Color color, int rank = 0, bool emphasis = false)
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

            PrintAttributes(dependencyNode, logRank + 1);

            RaiseVerbose("Connections", logRank + 1);
            MPlugArray connections = new MPlugArray();
            try {
                dependencyNode.getConnections(connections);
                RaiseVerbose("connections.Count=" + connections.Count, logRank + 2);
                foreach (MPlug connection in connections)
                {
                    MObject source = connection.source.node;

                    MPlugArray destinations = new MPlugArray();
                    connection.destinations(destinations);

                    if (source != null && source.hasFn(MFn.Type.kDependencyNode))
                    {
                        MFnDependencyNode node = new MFnDependencyNode(source);
                        RaiseVerbose("name=" + connection.name + "    partialName=" + connection.partialName(false, false, false, true) + "    source=" + node.name + "    source.apiType=" + source.apiType, logRank + 2);
                    }
                    else
                    {
                        RaiseVerbose("name=" + connection.name, logRank + 2);
                    }

                    RaiseVerbose("destinations.Count=" + destinations.Count, logRank + 3);
                    foreach (MPlug destination in destinations)
                    {
                        MObject destinationObject = destination.node;
                        if (destinationObject != null && destinationObject.hasFn(MFn.Type.kDependencyNode))
                        {
                            MFnDependencyNode node = new MFnDependencyNode(destinationObject);
                            RaiseVerbose("destination=" + node.name + "    destination.apiType=" + destinationObject.apiType, logRank + 3);

                            if (destinationObject.hasFn(MFn.Type.kShadingEngine))
                            {
                                PrintAttributes(node, logRank + 4);
                            }
                        }
                    }
                }
            }
            catch {}
        }

        void PrintAttributes(MFnDependencyNode dependencyNode, int logRank)
        {
            // prints
            RaiseVerbose("Attributes", logRank);
            for (uint i = 0; i < dependencyNode.attributeCount; i++)
            {
                MObject attribute = dependencyNode.attribute(i);

                if (attribute.hasFn(MFn.Type.kAttribute))
                {
                    MFnAttribute mFnAttribute = new MFnAttribute(attribute);
                    RaiseVerbose("name=" + mFnAttribute.name + "    apiType=" + attribute.apiType, logRank + 1);
                }
            }
        }
    }
}
