using System.Drawing;

namespace Utilities
{
    public interface ILoggingProvider
    {
        void ReportProgressChanged(int progress);

        void RaiseError(string error, int rank = 0);

        void RaiseWarning(string warning, int rank = 0);

        void RaiseMessage(string message, int rank = 0, bool emphasis = false);

        void RaiseMessage(string message, Color color, int rank = 0, bool emphasis = false);

        void RaiseVerbose(string message, int rank = 0, bool emphasis = false);

        void CheckCancelled();
    }
}
