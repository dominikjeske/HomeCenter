using HomeCenter.Runner.ConsoleExtentions;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public class CCToolsLampRunner : Runner
    {
        private readonly CCToolsAdapter _cCToolsAdapter;

        public CCToolsLampRunner(CCToolsAdapter cCToolsAdapter) : base(cCToolsAdapter.Name)
        {
            _tasks = new string[] { "TurnOn", "TurnOff", "Switch", "GetState", "FetchState" };
            _cCToolsAdapter = cCToolsAdapter;
        }

        public override async Task RunTask(int taskId)
        {
            switch (taskId)
            {
                case 0:
                    {
                        ConsoleEx.WriteOK("Pin number:");
                        var pinNumber = ConsoleEx.ReadNumber();
                        await _cCToolsAdapter.TurnOn(pinNumber, null); break;
                    }
                case 1:
                    {
                        ConsoleEx.WriteOK("Pin number:");
                        var pinNumber = ConsoleEx.ReadNumber();
                        _cCToolsAdapter.TurnOff(pinNumber); break;
                    }
                case 2:
                    {
                        ConsoleEx.WriteOK("Pin number:");
                        var pinNumber = ConsoleEx.ReadNumber();
                        _cCToolsAdapter.Switch(pinNumber); break;
                    }
                case 3:
                    {
                        ConsoleEx.WriteOK("Pin number:");
                        var pinNumber = ConsoleEx.ReadNumber();
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