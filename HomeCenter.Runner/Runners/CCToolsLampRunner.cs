using HomeCenter.Runner.ConsoleExtentions;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public class CCToolsLampRunner : Runner
    {
        private readonly CCToolsAdapter _cCToolsAdapter;

        public CCToolsLampRunner(string uid, CCToolsAdapter cCToolsAdapter) : base(uid)
        {
            _tasks = new string[] { "TurnOn", "TurnOff", "Switch", "GetState", "FetchState" };
            _cCToolsAdapter = cCToolsAdapter;
        }

        public override async Task RunTask(int taskId)
        {
            ConsoleEx.WriteOK("Pin number:");
            var pinNumber = ConsoleEx.ReadNumber();

            switch (taskId)
            {
                case 0:
                    {
                        await _cCToolsAdapter.TurnOn(pinNumber, null); break;
                    }
                case 1:
                    {
                        _cCToolsAdapter.TurnOff(pinNumber); break;
                    }
                case 2:
                    {
                        _cCToolsAdapter.Switch(pinNumber); break;
                    }
                case 3:
                    {
                        _cCToolsAdapter.GetState(pinNumber); break;
                    }
                case 4:
                    {
                        _cCToolsAdapter.FetchState(); break;
                    }

                default:
                    break;
            }
        }
    }
}