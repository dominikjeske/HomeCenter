using HomeCenter.Model.Messages.Commands;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace HomeCenter.Tests.Mocks
{
    public interface ITestAdapter
    {
        Subject<Command> CommandRecieved { get; }

        Task PropertyChanged<T>(string state, T oldValue, T newValue);

        string Uid { get; }
    }
}