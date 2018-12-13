namespace HomeCenter.Model.Messages
{
    public static class MessageProperties
    {
        //Common
        public const string MessageSource = nameof(MessageSource);

        //Command
        public const string IsFinishComand = nameof(IsFinishComand);
        public const string ExecutionDelay = nameof(ExecutionDelay);
        public const string CancelPrevious = nameof(CancelPrevious);
        public const string StateName = nameof(StateName);
        public const string ChangeFactor = nameof(ChangeFactor);
        public const string Value = nameof(Value);
        public const string InputSource = nameof(InputSource);
        public const string SurroundMode = nameof(SurroundMode);
        public const string Repeat = nameof(Repeat);
        public const string Code = nameof(Code);

        //Events
        public const string OldValue = nameof(OldValue);
        public const string NewValue = nameof(NewValue);
        public const string EventTime = nameof(EventTime);
        public const string EventType = nameof(EventType);
        public const string EventDirection = nameof(EventDirection);
        public const string CommandCode = nameof(CommandCode);

        //Adapter
        public const string I2cAddress = nameof(I2cAddress);
        public const string PinNumber = nameof(PinNumber);
        public const string ReversePinLevel = nameof(ReversePinLevel);
        public const string PoolInterval = nameof(PoolInterval);
        public const string PollDurationWarningThreshold = nameof(PollDurationWarningThreshold);
        public const string Hostname = nameof(Hostname);
        public const string Zone = nameof(Zone);
        public const string UserName = nameof(UserName);
        public const string Password = nameof(Password);
        public const string Port = nameof(Port);
        public const string MAC = nameof(MAC);
        public const string AuthKey = nameof(AuthKey);
        public const string System = nameof(System);
        public const string Bits = nameof(Bits);
        public const string Unit = nameof(Unit);
        public const string AdapterName = nameof(AdapterName);
        public const string AdapterAuthor = nameof(AdapterAuthor);
        public const string AdapterDescription = nameof(AdapterDescription);
    }
}