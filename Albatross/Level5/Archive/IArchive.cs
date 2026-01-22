using System.Windows.Forms;
using Albatross.Tools;

namespace Albatross.Level5.Archive
{
    public interface IArchive
    {
        string Name { get; }

        VirtualDirectory Directory { get; set; }

        void Save(string path, System.Action<int, int, string> progressCallback);

        void Close();
    }
}

