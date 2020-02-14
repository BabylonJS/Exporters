using System.Drawing;

namespace BabylonExport.Tools
{
    interface ILoggingHelper
    {
        void ReportProgressChanged(int progress);

        void RaiseError(string error, int rank = 0);

        void RaiseWarning(string warning, int rank = 0);

        void RaiseMessage(string message, int rank = 0, bool emphasis = false);

        void RaiseMessage(string message, Color color, int rank = 0, bool emphasis = false);

        // For debug purpose
        void RaiseVerbose(string message, int rank = 0, bool emphasis = false);
    }
}
