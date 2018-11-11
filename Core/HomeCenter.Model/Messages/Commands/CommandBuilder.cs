using HomeCenter.CodeGeneration;
using HomeCenter.Messages;

namespace HomeCenter.Model.Messages.Commands
{
    [CommandBuilder]
    public partial class CommandBuilder
    {
        public Command Build(ProtoCommand protoCommand)
        {
            var command = CreateCommand(protoCommand);

            foreach (var property in protoCommand.Properties)
            {
                command[property.Key] = property.Value;
            }

            return command;
        }
    }
}