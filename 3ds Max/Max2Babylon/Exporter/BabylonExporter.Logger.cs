using Autodesk.Max;
using System;
using System.Drawing;

namespace Max2Babylon
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
        public LogLevel logLevel = LogLevel.VERBOSE;

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

        public void Print(IIParamBlock2 paramBlock, int logRank)
        {
            RaiseVerbose("paramBlock=" + paramBlock, logRank);
            if (paramBlock != null)
            {
                RaiseVerbose("paramBlock.NumParams=" + paramBlock.NumParams, logRank + 1);
                for (short i = 0; i < paramBlock.NumParams; i++)
                {
                    ParamType2 paramType = paramBlock.GetParameterType(i);

                    RaiseVerbose("paramBlock.GetLocalName(" + i + ")=" + paramBlock.GetLocalName(i, 0) + ", type=" + paramType, logRank + 1);
                    switch (paramType)
                    {
                        case ParamType2.String:
                            RaiseVerbose("paramBlock.GetProperty(" + i + ")=" + paramBlock.GetStr(i, 0, 0), logRank + 2);
                            break;
                        case ParamType2.Int:
                            RaiseVerbose("paramBlock.GetProperty(" + i + ")=" + paramBlock.GetInt(i, 0, 0), logRank + 2);
                            break;
                        case ParamType2.Float:
                            RaiseVerbose("paramBlock.GetProperty(" + i + ")=" + paramBlock.GetFloat(i, 0, 0), logRank + 2);
                            break;
                        default:
                            RaiseVerbose("Unknown property type", logRank + 2);
                            break;
                    }
                }
            }
        }

        public void Print(IIPropertyContainer propertyContainer, int logRank)
        {
            RaiseVerbose("propertyContainer=" + propertyContainer, logRank);
            if (propertyContainer != null)
            {
                RaiseVerbose("propertyContainer.NumberOfProperties=" + propertyContainer.NumberOfProperties, logRank + 1);
                for (int i = 0; i < propertyContainer.NumberOfProperties; i++)
                {
                    var prop = propertyContainer.GetProperty(i);
                    if (prop != null)
                    {
                        RaiseVerbose("propertyContainer.GetProperty(" + i + ")=" + prop.Name, logRank + 1);
                        switch (prop.GetType_)
                        {
                            case PropType.StringProp:
                                string propertyString = "";
                                RaiseVerbose("prop.GetPropertyValue(ref propertyString, 0)=" + prop.GetPropertyValue(ref propertyString, 0), logRank + 2);
                                RaiseVerbose("propertyString=" + propertyString, logRank + 2);
                                break;
                            case PropType.IntProp:
                                int propertyInt = 0;
                                RaiseVerbose("prop.GetPropertyValue(ref propertyInt, 0)=" + prop.GetPropertyValue(ref propertyInt, 0), logRank + 2);
                                RaiseVerbose("propertyInt=" + propertyInt, logRank + 2);
                                break;
                            case PropType.FloatProp:
                                float propertyFloat = 0;
                                RaiseVerbose("prop.GetPropertyValue(ref propertyFloat, 0, true)=" + prop.GetPropertyValue(ref propertyFloat, 0, true), logRank + 2);
                                RaiseVerbose("propertyFloat=" + propertyFloat, logRank + 2);
                                RaiseVerbose("prop.GetPropertyValue(ref propertyFloat, 0, false)=" + prop.GetPropertyValue(ref propertyFloat, 0, false), logRank + 2);
                                RaiseVerbose("propertyFloat=" + propertyFloat, logRank + 2);
                                break;
                            case PropType.Point3Prop:
                                IPoint3 propertyPoint3 = Loader.Global.Point3.Create(0, 0, 0);
                                RaiseVerbose("prop.GetPropertyValue(ref propertyPoint3, 0)=" + prop.GetPropertyValue(propertyPoint3, 0), logRank + 2);
                                RaiseVerbose("propertyPoint3=" + Point3ToString(propertyPoint3), logRank + 2);
                                break;
                            case PropType.Point4Prop:
                                IPoint4 propertyPoint4 = Loader.Global.Point4.Create(0, 0, 0, 0);
                                RaiseVerbose("prop.GetPropertyValue(ref propertyPoint4, 0)=" + prop.GetPropertyValue(propertyPoint4, 0), logRank + 2);
                                RaiseVerbose("propertyPoint4=" + Point4ToString(propertyPoint4), logRank + 2);
                                break;
                            case PropType.UnknownProp:
                            default:
                                RaiseVerbose("Unknown property type", logRank + 2);
                                break;
                        }
                    }
                    else
                    {
                        RaiseVerbose("propertyContainer.GetProperty(" + i + ") IS NULL", logRank + 1);
                    }
                }
            }
        }

        // -------------------------
        // --------- Utils ---------
        // -------------------------

        private string ColorToString(IColor color)
        {
            if (color == null)
            {
                return "";
            }

            return "{ r=" + color.R + ", g=" + color.G + ", b=" + color.B + " }";
        }

        private string Point3ToString(IPoint3 point)
        {
            if (point == null)
            {
                return "";
            }

            return "{ x=" + point.X + ", y=" + point.Y + ", z=" + point.Z + " }";
        }

        private string Point4ToString(IPoint4 point)
        {
            if (point == null)
            {
                return "";
            }

            return "{ x=" + point.X + ", y=" + point.Y + ", z=" + point.Z + ", w=" + point.W + " }";
        }
    }
}
