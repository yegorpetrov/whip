using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whip.OS;
using Whip.Scripting;
using Whip.Widgets;

namespace Whip.Runtime
{
    partial class System0
    {
#if DEBUG
        public void Run()
        {
            ScriptLoaded?.Invoke();
        }
#endif

        public event Action ScriptLoaded;
        public event Action ScriptUnloading;
        public event Action Quit;
        public event Action<string, string> SetXuiParam;
        public event Action<string> KeyDown;
        public event Action<string, string, string> Accelerator;
        public event Action<Group> CreateLayout;
        public event Action<Group> ShowLayout;
        public event Action<Group> HideLayout;
        public event Action<int, int> ViewPortChanged;
        public event Action Stop, Play, Pause, Resume;
        public event Action<string> TitleChange;
        public event Action<string> Title2Change;
        public event Action<string> UrlChange;
        public event Action<string> InfoChange;
        public event Action<string> StatusMsg;
        public event Action<int, int> EqBandChanged;
        public event Action<int> EqPreampChanged;
        public event Action<int> EqChanged;
        public event Action<int> EqFreqChanged;
        public event Action<int> VolumeChanged;
        public event Action<int> Seek;
        
        [NeedsContext]
        public Group GetScriptGroup(IScriptContext ctx)
        {
            return (ctx as GroupContextProxy)?.Group;
        }

        [NeedsContext]
        public string GetParam(IScriptContext ctx)
        {
            return (ctx as GroupContextProxy)?.Param;
        }

        public int MessageBox(string msg, string title, int flags, string suppress)
        {
            return (int)OS.MessageBox.Show(msg, title, (MessageBox.MsgButtons)flags, suppress);
        }

        public string GetToken(string s, string sep, int index)
        {
            try
            {
                return s.Split(new[] { sep }, StringSplitOptions.None)[index];
            }
            catch (IndexOutOfRangeException)
            {
                return string.Empty;
            }
        }

        public int StringToInteger(string s) => int.Parse(s);

        public string IntegerToString(int i) => i.ToString();
    }
}
